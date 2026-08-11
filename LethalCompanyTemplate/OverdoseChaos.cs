using System.Collections;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace OverdoseChaos
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.urbain7877.overdosechaos";
        public const string NAME = "OverdoseChaos";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log;
        private readonly Harmony harmony = new Harmony(GUID);

        public static Plugin Instance;

        private void Awake()
        {
            Instance = this;
            Log = Logger;
            Log.LogInfo($"Le mod {NAME} est bien chargé !");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(RoundManager))]
    public class RoundManagerPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void PostfixStart(RoundManager __instance)
        {
            if (__instance == null) return;

            Plugin.Log.LogInfo("=== OVERDOSE CHAOS : Activation du chaos total (Loot + Monstres) ! ===");

            // 1. Boost massif de la valeur des objets (+150%)
            __instance.scrapValueMultiplier *= 2.5f;

            // 2. Débridage total du nombre d'objets sur la lune pour qu'il y en ait partout
            if (__instance.currentLevel != null)
            {
                __instance.currentLevel.minScrap = 60;
                __instance.currentLevel.maxScrap = 180;
            }

            // 3. Boost initial de la puissance des monstres à l'intérieur
            __instance.currentMaxInsidePower = Mathf.RoundToInt(__instance.currentMaxInsidePower * 1.85f);

            Plugin.Log.LogInfo("Loot et monstres débridés avec succès !");

            // Lance la boucle infernale de chaos toutes les 40 secondes
            if (Plugin.Instance != null)
            {
                Plugin.Instance.StartCoroutine(ChaosPeriodicRoutine(__instance));
            }
        }

        private static IEnumerator ChaosPeriodicRoutine(RoundManager roundManager)
        {
            // Attente initiale de 40 secondes après l'atterrissage
            yield return new WaitForSeconds(40f);

            while (roundManager != null)
            {
                Plugin.Log.LogInfo("=== VAGUE DE CHAOS : Forçage des spawns de monstres ! ===");

                // Augmente la puissance maximale autorisée pour les monstres
                roundManager.currentMaxInsidePower += 10;

                // Force le jeu à recalculer et réinitialiser les cycles d'apparitions de monstres
                roundManager.RefreshEnemiesList();

                Plugin.Log.LogInfo($"Nouvelle puissance intérieure des monstres : {roundManager.currentMaxInsidePower}");

                // Répète toutes les 40 secondes
                yield return new WaitForSeconds(40f);
            }
        }
    }
}

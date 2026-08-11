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
            Log.LogInfo($"Le mod {NAME} est chargé et prêt !");
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

            Plugin.Log.LogInfo("=== OVERDOSE CHAOS : Initialisation des buffs globaux ===");

            // 1. Boost massif de la valeur des objets (+150%)
            __instance.scrapValueMultiplier *= 2.5f;

            // 2. Forçage du nombre d'objets sur la lune (partout sur la map)
            if (__instance.currentLevel != null)
            {
                __instance.currentLevel.minScrap = 80;
                __instance.currentLevel.maxScrap = 220;
            }

            // 3. Puissance initiale des monstres doublée
            __instance.currentMaxInsidePower = Mathf.RoundToInt(__instance.currentMaxInsidePower * 2f);

            Plugin.Log.LogInfo("Loot et puissance des monstres maximisés !");

            if (Plugin.Instance != null)
            {
                Plugin.Instance.StartCoroutine(ChaosPeriodicRoutine(__instance));
            }
        }

        private static IEnumerator ChaosPeriodicRoutine(RoundManager roundManager)
        {
            // Attente initiale de 40 secondes
            yield return new WaitForSeconds(40f);

            while (roundManager != null)
            {
                Plugin.Log.LogInfo("=== VAGUE DE CHAOS : Hausse de puissance et forçage de spawn ! ===");

                // Augmente la puissance max des monstres en continu
                roundManager.currentMaxInsidePower += 25;

                // Réinitialise le compteur de puissance actuelle pour obliger le jeu à faire spawner de nouveaux ennemis
                roundManager.currentEnemyPower = 0;

                Plugin.Log.LogInfo($"Nouvelle puissance max des monstres : {roundManager.currentMaxInsidePower}");

                // Répète toutes les 40 secondes
                yield return new WaitForSeconds(40f);
            }
        }
    }
}

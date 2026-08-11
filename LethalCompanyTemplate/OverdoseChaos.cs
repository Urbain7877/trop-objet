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

            Plugin.Log.LogInfo("=== INITIALISATION : OverdoseChaos booste le loot et les monstres ! ===");

            // 1. +150% pour la valeur des objets
            float augmentationObjetsPourcent = 150f; 
            __instance.scrapValueMultiplier *= (1f + (augmentationObjetsPourcent / 100f));

            // 2. +85% pour la puissance des entités à l'intérieur
            float augmentationEntitesPourcent = 85f;
            __instance.currentMaxInsidePower = Mathf.RoundToInt(__instance.currentMaxInsidePower * (1f + (augmentationEntitesPourcent / 100f)));

            Plugin.Log.LogInfo($"Objets boostés de {augmentationObjetsPourcent}% et Entités boostées de {augmentationEntitesPourcent}% !");

            if (Plugin.Instance != null)
            {
                Plugin.Instance.StartCoroutine(ChaosPeriodicRoutine(__instance));
            }
        }

        // Patch pour modifier la génération du nombre d'objets par pièce selon tes pourcentages précis
        [HarmonyPatch("SpawnScrapInTile")]
        [HarmonyPrefix]
        static bool PrefixSpawnScrapInTile(ref int ___itemsToSpawn)
        {
            float rand = Random.Range(0f, 100f);

            if (rand <= 50f)
            {
                ___itemsToSpawn = 2; // 50% de chance d'avoir 2 items
            }
            else if (rand <= 80f)
            {
                ___itemsToSpawn = 3; // 30% de chance d'avoir 3 items (50 + 30)
            }
            else
            {
                ___itemsToSpawn = 4; // 20% de chance d'avoir 4 items
            }

            // S'assure qu'il y a au moins un minimum absolu d'un objet par pièce
            if (___itemsToSpawn < 1)
            {
                ___itemsToSpawn = 1;
            }

            return true;
        }

        private static IEnumerator ChaosPeriodicRoutine(RoundManager roundManager)
        {
            yield return new WaitForSeconds(40f);

            while (roundManager != null)
            {
                Plugin.Log.LogInfo("=== VAGUE DE CHAOS : Hausse de la puissance des monstres (+5) ! ===");

                // Incrémente la puissance des monstres régulièrement sans bloquer
                roundManager.currentMaxInsidePower += 5;

                // Répète toutes les 40 secondes
                yield return new WaitForSeconds(40f);
            }
        }
    }
}

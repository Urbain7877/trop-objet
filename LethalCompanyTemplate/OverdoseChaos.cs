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

            Plugin.Log.LogInfo("=== INITIALISATION : OverdoseChaos débride le loot et les monstres ! ===");

            // 1. +150% pour la valeur des objets
            float augmentationObjetsPourcent = 150f; 
            __instance.scrapValueMultiplier *= (1f + (augmentationObjetsPourcent / 100f));

            // 2. +85% pour les entités (puissance de départ)
            float augmentationEntitesPourcent = 85f;
            __instance.currentMaxInsidePower = Mathf.RoundToInt(__instance.currentMaxInsidePower * (1f + (augmentationEntitesPourcent / 100f)));

            // 3. Supprime les limites de quantité de loot en augmentant drastiquement le nombre de spawns d'objets prévus
            __instance.minScrap = Mathf.Max(__instance.minScrap, 40);
            __instance.maxScrap = Mathf.Max(__instance.maxScrap, 90);

            Plugin.Log.LogInfo("Limites de loot supprimées et quantité gonflée à bloc !");

            if (Plugin.Instance != null)
            {
                Plugin.Instance.StartCoroutine(ChaosPeriodicRoutine(__instance));
            }
        }

        // Patch pour modifier la génération du nombre d'objets par emplacement / salle selon tes pourcentages
        [HarmonyPatch("SpawnScrapInTile")]
        [HarmonyPrefix]
        static bool PrefixSpawnScrapInTile(ref int ___itemsToSpawn)
        {
            // Tirage aléatoire entre 0 et 100 pour respecter tes pourcentages :
            // - 50% de chance (0 à 50) -> 2 items
            // - 30% de chance (50 à 80) -> 3 items
            // - 20% de chance (80 à 100) -> 4 items
            float rand = Random.Range(0f, 100f);

            if (rand <= 50f)
            {
                ___itemsToSpawn = 2; // 50%
            }
            else if (rand <= 80f)
            {
                ___itemsToSpawn = 3; // 30% (50 + 30)
            }
            else
            {
                ___itemsToSpawn = 4; // 20% restants
            }

            // On s'assure qu'il y a au moins un minimum absolu d'un objet par salle
            if (___itemsToSpawn < 1)
            {
                ___itemsToSpawn = 1;
            }

            return true; // Laisse le jeu exécuter la méthode avec notre valeur modifiée
        }

        private static IEnumerator ChaosPeriodicRoutine(RoundManager roundManager)
        {
            yield return new WaitForSeconds(40f);

            while (roundManager != null)
            {
                Plugin.Log.LogInfo("=== VAGUE DE CHAOS : Hausse de la puissance des monstres (+5) ! ===");

                // Incrémente la puissance des monstres sans s'arrêter à une petite limite
                roundManager.currentMaxInsidePower += 5;
                roundManager.minsToSpawnEnemy = 0f; 

                // Répète toutes les 40 secondes
                yield return new WaitForSeconds(40f);
            }
        }
    }
}

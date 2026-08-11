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
            Log.LogInfo($"Le mod {NAME} est chargé et prêt pour le chaos total !");
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

            Plugin.Log.LogInfo("=== OVERDOSE CHAOS : Initialisation des paramètres globaux ===");

            // 1. Boost massif de la valeur des objets (+150% minimum)
            __instance.scrapValueMultiplier *= 2.5f;

            // 2. Forçage du loot partout sur la map (couloirs, salles, transitions)
            if (__instance.currentLevel != null)
            {
                __instance.currentLevel.minScrap = 80;
                __instance.currentLevel.maxScrap = 220;

                // 3. S'assurer que les monstres de la lune ont une chance de spawner (corrige les listes vides)
                if (__instance.currentLevel.enemies != null)
                {
                    foreach (var enemy in __instance.currentLevel.enemies)
                    {
                        if (enemy != null && enemy.rarity <= 0)
                        {
                            enemy.rarity = 30; // Donne une chance d'apparition à tous les monstres
                        }
                    }
                }

                if (__instance.currentLevel.outsideEnemies != null)
                {
                    foreach (var outsideEnemy in __instance.currentLevel.outsideEnemies)
                    {
                        if (outsideEnemy != null && outsideEnemy.rarity <= 0)
                        {
                            outsideEnemy.rarity = 30;
                        }
                    }
                }
            }

            // 4. Puissance initiale des monstres augmentée
            __instance.currentMaxInsidePower = Mathf.RoundToInt(__instance.currentMaxInsidePower * 2f);

            Plugin.Log.LogInfo("Loot généralisé et entités débridées avec succès !");

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
                Plugin.Log.LogInfo("=== VAGUE DE CHAOS : Hausse de puissance et forçage de spawn ! ===");

                // On augmente la puissance max des monstres
                roundManager.currentMaxInsidePower += 20;

                // On réinitialise la puissance actuelle pour forcer le jeu à faire spawner de nouvelles créatures immédiatement
                roundManager.currentEnemyPower = 0;

                Plugin.Log.LogInfo($"Nouvelle puissance max des monstres : {roundManager.currentMaxInsidePower}");

                // Répète toutes les 40 secondes
                yield return new WaitForSeconds(40f);
            }
        }
    }
}

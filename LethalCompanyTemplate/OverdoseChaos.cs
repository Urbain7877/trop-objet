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
        private static int limiteAbsoluePower = 30;

        // On cible la méthode "Start" ou une méthode standard de génération de début de partie
        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void PostfixStart(RoundManager __instance)
        {
            if (__instance == null) return;

            Plugin.Log.LogInfo("=== INITIALISATION : OverdoseChaos applique les modifications ! ===");

            // +150% pour les objets
            float augmentationObjetsPourcent = 150f; 
            __instance.scrapValueMultiplier *= (1f + (augmentationObjetsPourcent / 100f));

            // +85% pour les entités / monstres (puissance intérieure)
            float augmentationEntitesPourcent = 85f;
            __instance.currentMaxInsidePower = Mathf.RoundToInt(__instance.currentMaxInsidePower * (1f + (augmentationEntitesPourcent / 100f)));

            Plugin.Log.LogInfo($"Objets boostés de {augmentationObjetsPourcent}% et Entités boostées de {augmentationEntitesPourcent}% !");

            // Lance la boucle toutes les 40 secondes
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
                if (roundManager.currentMaxInsidePower >= limiteAbsoluePower)
                {
                    Plugin.Log.LogInfo("=== LIMITE ATTEINTE : Le chaos s'arrête d'augmenter pour préserver les performances ! ===");
                    yield break; 
                }

                Plugin.Log.LogInfo("=== VAGUE DE CHAOS : Augmentation de la puissance des monstres (+2) ! ===");

                roundManager.currentMaxInsidePower += 2;

                // Répète toutes les 40 secondes
                yield return new WaitForSeconds(40f);
            }
        }
    }
}

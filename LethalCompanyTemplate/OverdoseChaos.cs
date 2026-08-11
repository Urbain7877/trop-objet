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

            Plugin.Log.LogInfo("=== INITIALISATION : OverdoseChaos applique les multiplicateurs ! ===");

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

        private static IEnumerator ChaosPeriodicRoutine(RoundManager roundManager)
        {
            // Attente initiale de 40 secondes
            yield return new WaitForSeconds(40f);

            while (roundManager != null)
            {
                Plugin.Log.LogInfo("=== VAGUE DE CHAOS : Hausse de la puissance des monstres (+5) ! ===");

                // Incrémente la puissance des monstres toutes les 40 secondes
                roundManager.currentMaxInsidePower += 5;

                yield return new WaitForSeconds(40f);
            }
        }
    }
}

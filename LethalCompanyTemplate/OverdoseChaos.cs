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

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"Le mod {NAME} est bien chargé et prêt à foutre le bazar !");

            // Applique tous les patchs Harmony écrits dans le mod
            harmony.PatchAll();
        }
    }

    // Patch pour multiplier le chaos des objets et des entités à l'atterrissage
    [HarmonyPatch(typeof(RoundManager))]
    public class RoundManagerPatch
    {
        [HarmonyPatch("BeginRound")]
        [HarmonyPostfix]
        static void PostfixBeginRound()
        {
            Plugin.Log.LogInfo("=== ATTERRISSAGE : Lancement du protocole OverdoseChaos ! ===");

            // 1. Ici, tu peux booster les paramètres globaux de la lune si le RoundManager le permet
            if (RoundManager.Instance != null)
            {
                // Multiplie par exemple le nombre d'objets scrap à l'intérieur
                RoundManager.Instance.scrapValueMultiplier *= 2.5f;
                Plugin.Log.LogInfo("Valeur du loot multipliée par 2.5 !");
            }
        }
    }
}

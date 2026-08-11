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
        // Limite maximale absolue pour la puissance intérieure des monstres
        private static int limiteAbsoluePower = 30;

        [HarmonyPatch("BeginRound")]
        [HarmonyPostfix]
        static void PostfixBeginRound()
        {
            Plugin.Log.LogInfo("=== ATTERRISSAGE : OverdoseChaos active les pourcentages ! ===");

            if (RoundManager.Instance != null)
            {
                // +150% pour les objets
                float augmentationObjetsPourcent = 150f; 
                RoundManager.Instance.scrapValueMultiplier *= (1f + (augmentationObjetsPourcent / 100f));

                // +85% pour les entités / monstres (puissance intérieure)
                float augmentationEntitesPourcent = 85f;
                RoundManager.Instance.currentMaxInsidePower = Mathf.RoundToInt(RoundManager.Instance.currentMaxInsidePower * (1f + (augmentationEntitesPourcent / 100f)));

                Plugin.Log.LogInfo($"Objets boostés de {augmentationObjetsPourcent}% et Entités boostées de {augmentationEntitesPourcent}% !");

                // Lance la boucle toutes les 40 secondes
                if (Plugin.Instance != null)
                {
                    Plugin.Instance.StartCoroutine(ChaosPeriodicRoutine());
                }
            }
        }

        private static IEnumerator ChaosPeriodicRoutine()
        {
            // Attente initiale de 40 secondes après l'atterrissage
            yield return new WaitForSeconds(40f);

            while (RoundManager.Instance != null)
            {
                // Vérifie si on a atteint la limite de spawn autorisée
                if (RoundManager.Instance.currentMaxInsidePower >= limiteAbsoluePower)
                {
                    Plugin.Log.LogInfo("=== LIMITE ATTEINTE : Le chaos s'arrête d'augmenter pour préserver les performances ! ===");
                    yield break; 
                }

                Plugin.Log.LogInfo("=== VAGUE DE CHAOS : Augmentation de la puissance des monstres (+2) ! ===");

                // Incrémente la puissance d'apparition des entités à l'intérieur
                RoundManager.Instance.currentMaxInsidePower += 2;

                // Répète toutes les 40 secondes
                yield return new WaitForSeconds(40f);
            }
        }
    }
}

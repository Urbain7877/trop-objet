using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace OverdoseChaos
{
    [BepInPlugin(GUID, NAME, VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.tonnom.overdosechaos";
        public const string NAME = "OverdoseChaos";
        public const string VERSION = "1.0.0";

        internal static ManualLogSource Log;
        private readonly Harmony harmony = new Harmony(GUID);

        private void Awake()
        {
            Log = Logger;
            Log.LogInfo($"Le mod {NAME} est bien chargé !");

            // Applique les patchs Harmony si tu en as
            harmony.PatchAll();
        }
    }
}

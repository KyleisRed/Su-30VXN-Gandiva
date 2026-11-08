using BepInEx;
using HarmonyLib;

namespace solarsu30
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("com.nikkorap.blueprinter", "2.1.1")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "kyleisred.nuclearoption.solarsu30";
        public const string PluginName = "Solar Su-30";
        public const string PluginVersion = "1.0.0";

        private Harmony harmony;

        private void Awake()
        {
            harmony = new Harmony(PluginGuid);
            harmony.PatchAll();

            Logger.LogInfo($"Su-30 {PluginVersion} loaded.");
        }

        private void OnDestroy()
        {
            harmony?.UnpatchSelf();
        }
    }
}
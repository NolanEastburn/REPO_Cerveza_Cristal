using System.Diagnostics.Tracing;
using BepInEx.Logging;
using HarmonyLib;

namespace Cerveza_Cristal;

public static class Utils
{
    public enum LevelTypes
    {
        ARCTIC = 0,
        MANOR = 1,
        MUSEUM = 2,
        WIZARD = 3,
        NONE = 4
    }

    private static ManualLogSource _logger { get; set; } = null;

    private static LevelTypes _selectedLevel { get; set; } = LevelTypes.NONE;

    public static void SelectLevel(LevelTypes selectedLevel)
    {
        _selectedLevel = selectedLevel;
    }

    public static void SetLogger(ManualLogSource logger)
    {
        _logger = logger;
    }

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetRunLevel))]
    public static class LevelPatches
    {
        static void Postfix(RunManager __instance)
        {
            if (_selectedLevel != LevelTypes.NONE)
            {
                __instance.levelCurrent = __instance.levels[(int)_selectedLevel];
            }
        }
    }
}
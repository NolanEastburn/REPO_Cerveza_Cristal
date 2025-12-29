using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Cerveza_Cristal;

// Runs after Spawn, but only runs once so we don't re-dump each time something spawns in.
[HarmonyPatch(typeof(ValuableDirector), "Spawn")]
class Dumper
{
    private static ManualLogSource _logger { get; set; } = null;
    private static bool _dumperEnabled { get; set; } = false;

    public static void SetLogger(ManualLogSource logger)
    {
        _logger = logger;
    }

    public static void Enable()
    {
        _dumperEnabled = true;

        _logger.LogWarning("It's dumping time!");
    }


    static void Postfix(ValuableDirector __instance)
    {
        if (_dumperEnabled)
        {
            _logger.LogWarning("Dumping...");

            FieldInfo tinyVolumesField = __instance.GetType().GetField("tinyVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> tinyVolumes = (List<ValuableVolume>)tinyVolumesField.GetValue(__instance);

            FieldInfo smallVolumesField = __instance.GetType().GetField("smallVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> smallVolumes = (List<ValuableVolume>)smallVolumesField.GetValue(__instance);

            FieldInfo mediumVolumesField = __instance.GetType().GetField("mediumVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> mediumVolumes = (List<ValuableVolume>)mediumVolumesField.GetValue(__instance);

            FieldInfo bigVolumesField = __instance.GetType().GetField("bigVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> bigVolumes = (List<ValuableVolume>)bigVolumesField.GetValue(__instance);

            FieldInfo wideVolumesField = __instance.GetType().GetField("wideVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> wideVolumes = (List<ValuableVolume>)wideVolumesField.GetValue(__instance);

            FieldInfo tallVolumesField = __instance.GetType().GetField("tallVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> tallVolumes = (List<ValuableVolume>)tallVolumesField.GetValue(__instance);

            FieldInfo veryTallVolumesField = __instance.GetType().GetField("veryTallVolumes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            List<ValuableVolume> veryTallVolumes = (List<ValuableVolume>)veryTallVolumesField.GetValue(__instance);

            StreamWriter writer = new StreamWriter(Path.Combine(Paths.PluginPath, "Dumper.txt"));

            List<List<ValuableVolume>> volumes = new List<List<ValuableVolume>>()
            {
                tinyVolumes, smallVolumes, mediumVolumes, bigVolumes, wideVolumes, tallVolumes, veryTallVolumes
            };

            foreach (List<ValuableVolume> vs in volumes)
            {
                writer.WriteLine($"Child Components of {vs[0].VolumeType} volumes");
                foreach (ValuableVolume v in vs)
                {
                    foreach (Component c in v.GetComponentsInParent<Component>())
                    {
                        writer.WriteLine($"Component Type: {c.GetType()} | Name: {c.name}");
                    }
                    writer.WriteLine();
                }
            }

            writer.Close();

            _dumperEnabled = false;
        }
    }
}
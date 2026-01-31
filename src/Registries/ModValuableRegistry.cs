namespace Cerveza_Cristal;

using BepInEx.Logging;
using UnityEngine;


public class ModValuableRegistry : ModRegistry<ValuableAddition>
{

    class ModValuableDefaultBehaviour : MonoBehaviour
    {
        private ManualLogSource _logger { get; set; } = null;

        private void Awake()
        {
            _logger = ModEntry.Logger;
            _logger.LogInfo("Spawned " + gameObject.name + " into the world!");
        }
    }



    public ModValuableRegistry(AssetBundle assetBundle, ManualLogSource logger) : base(assetBundle, logger) { }

    public override void ApplyAdditionRegistrations(RunManager runManager)
    {
        foreach (Level level in runManager.levels)
        {
            foreach (LevelValuables lv in level.ValuablePresets)
            {
                foreach ((GameObject, ValuableAddition) regEntry in Registry.Values)
                {
                    PrefabRef prefabRef = CreatePrefabRef(regEntry);

                    switch (regEntry.Item2.ValuableData.ValuableVolumeType)
                    {

                        case ValuableVolume.Type.Tiny:
                            lv.tiny.Add(prefabRef);
                            break;

                        case ValuableVolume.Type.Small:
                            lv.small.Add(prefabRef);
                            break;

                        case ValuableVolume.Type.Medium:
                            lv.medium.Add(prefabRef);
                            break;

                        case ValuableVolume.Type.Big:
                            lv.big.Add(prefabRef);
                            break;

                        case ValuableVolume.Type.Wide:
                            lv.wide.Add(prefabRef);
                            break;

                        case ValuableVolume.Type.Tall:
                            lv.tall.Add(prefabRef);
                            break;

                        case ValuableVolume.Type.VeryTall:
                            lv.veryTall.Add(prefabRef);
                            break;
                    }
                }
            }
        }
    }

    public override PrefabRef CreatePrefabRef((GameObject, ValuableAddition) regEntry)
    {
        PrefabRef prefabRef = new PrefabRef();
        prefabRef.SetPrefab(regEntry.Item1, GetRegistryName(regEntry.Item2));
        return prefabRef;
    }

}
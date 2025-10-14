using UnityEngine;
using BepInEx.Logging;
using System.Collections.Generic;
using Photon.Pun;

namespace Cerveza_Cristal;

public class ModValuableRegistry : IModRegistry
{
    public class ValuableAddition
    {
        public string AssetName { get; private set; }

        public Data ValuableData { get; private set; }
        public List<System.Type> AdditionalComponents { get; private set; }

        public ValuableAddition(string assetName, Data valuableData, List<System.Type> additionalComponents = null)
        {
            AssetName = assetName;
            ValuableData = valuableData;
            AdditionalComponents = additionalComponents;
        }
    }

    public struct Data
    {
        private static readonly (float, float) DEFAULT_VALUE = (100f, 1000f);
        private const float DEFAULT_MASS = 1.0f;

        private static readonly ValuableVolume.Type DEFAULT_VALUABLE_VOLUME_TYPE = ValuableVolume.Type.Medium;

        public string Name { get; set; }
        public (float, float) Value { get; set; }
        public float Mass { get; set; }
        public ValuableVolume.Type ValuableVolumeType { get; set; }
        public Gradient ParticleGradient { get; set; }

        public Data(string name, (float, float)? value = null, float mass = DEFAULT_MASS, ValuableVolume.Type? valuableVolumeType = null, Gradient particleGraident = null)
        {
            Name = name;
            Value = value ?? DEFAULT_VALUE;
            Mass = mass;
            ValuableVolumeType = valuableVolumeType ?? DEFAULT_VALUABLE_VOLUME_TYPE;

            if (particleGraident == null)
            {
                // Default
                ParticleGradient = new Gradient();
                ParticleGradient.colorKeys = new GradientColorKey[1];
                ParticleGradient.colorKeys[0] = new GradientColorKey(Color.white, 0.0f);
                ParticleGradient.alphaKeys = new GradientAlphaKey[1];
                ParticleGradient.alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f);
            }
            else
            {
                ParticleGradient = particleGraident;
            }
        }

    }

    public Dictionary<string, (GameObject, Data)> Registry { get; private set; } = null;

    private AssetBundle _assetBundle { get; set; }

    private ManualLogSource _logger { get; set; }

    class ModValuableDefaultBehaviour : MonoBehaviour
    {
        private ManualLogSource _logger { get; set; } = null;

        private void Awake()
        {
            _logger = ModEntry.Logger;
            _logger.LogInfo("Spawned " + gameObject.name + " into the world!");
        }
    }

    public void Register(ValuableAddition addition)
    {
        GameObject go = _assetBundle.LoadAsset<GameObject>(addition.AssetName);

        if (go != null)
        {

            // Add components
            go.AddComponent(typeof(PhotonTransformView));
            go.AddComponent(typeof(PhysGrabObject));
            go.AddComponent(typeof(RoomVolumeCheck));
            go.AddComponent(typeof(Rigidbody));
            go.AddComponent(typeof(PhysGrabObjectImpactDetector));
            go.AddComponent(typeof(PhotonView));
            go.AddComponent(typeof(ModValuableDefaultBehaviour));

            if (!go.GetComponent<Collider>())
            {
                _logger.LogWarning(addition.ValuableData.Name + " does not have a collider! Adding a BoxCollider!");
                BoxCollider bc = go.AddComponent(typeof(BoxCollider)) as BoxCollider;
            }

            go.AddComponent(typeof(PhysGrabObjectCollider));

            ValuableObject v = go.AddComponent(typeof(ValuableObject)) as ValuableObject;
            v.valuePreset = ScriptableObject.CreateInstance(typeof(Value)) as Value;
            v.valuePreset.valueMin = addition.ValuableData.Value.Item1;
            v.valuePreset.valueMax = addition.ValuableData.Value.Item2;

            v.physAttributePreset = ScriptableObject.CreateInstance(typeof(PhysAttribute)) as PhysAttribute;
            v.physAttributePreset.mass = addition.ValuableData.Mass;
            v.volumeType = addition.ValuableData.ValuableVolumeType;
            v.durabilityPreset = ScriptableObject.CreateInstance(typeof(Durability)) as Durability;

            go.tag = "Phys Grab Object";
            go.name = addition.ValuableData.Name;

            // Put the game object on the PhysGrabObject layer.
            // Many raycasts will not happen if the layer is not correct.
            go.layer = LayerMask.NameToLayer("PhysGrabObject");

            if (addition.AdditionalComponents != null)
            {
                foreach (System.Type c in addition.AdditionalComponents)
                {
                    go.AddComponent(c);
                }
            }

            Registry.Add(addition.ValuableData.Name, (go, addition.ValuableData));
        }
        else
        {
            _logger.LogError("Could not register GameObject " + addition.AssetName + " as it does not exist in the asset bundle!");
        }
    }

    public ModValuableRegistry(AssetBundle assetBundle, ManualLogSource logger)
    {
        _assetBundle = assetBundle;
        _logger = logger;

        Registry = new Dictionary<string, (GameObject, Data)>();
    }

    public void ApplyAdditionRegistrations(RunManager runManager)
    {
        foreach (Level level in runManager.levels)
        {
            foreach (LevelValuables lv in level.ValuablePresets)
            {
                foreach ((GameObject, ModValuableRegistry.Data) regEntry in Registry.Values)
                {
                    switch (regEntry.Item2.ValuableVolumeType)
                    {

                        case ValuableVolume.Type.Tiny:
                            lv.tiny.Add(regEntry.Item1);
                            break;

                        case ValuableVolume.Type.Small:
                            lv.small.Add(regEntry.Item1);
                            break;

                        case ValuableVolume.Type.Medium:
                            lv.medium.Add(regEntry.Item1);
                            break;

                        case ValuableVolume.Type.Big:
                            lv.big.Add(regEntry.Item1);
                            break;

                        case ValuableVolume.Type.Wide:
                            lv.wide.Add(regEntry.Item1);
                            break;

                        case ValuableVolume.Type.Tall:
                            lv.tall.Add(regEntry.Item1);
                            break;

                        case ValuableVolume.Type.VeryTall:
                            lv.veryTall.Add(regEntry.Item1);
                            break;
                    }
                }
            }
        }
    }
}
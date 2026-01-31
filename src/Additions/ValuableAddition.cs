namespace Cerveza_Cristal;

using System.Collections.Generic;
using BepInEx.Logging;
using Photon.Pun;
using UnityEngine;

public class ValuableAddition
{

    class DefaultBehaviour : MonoBehaviour
    {
        public ManualLogSource Logger { private get; set; } = null;

        public void Awake()
        {
            if (Logger != null)
            {
                Logger.LogInfo(string.Format("Spawned {0} into the world!", gameObject.name));
            }
        }
    }

    private ManualLogSource _logger { get; set; }
    private string _assetName { get; set; }

    public Data ValuableData { get; private set; }
    public List<System.Type> AdditionalComponents { get; private set; }

    public struct Data
    {
        private static readonly (float, float) DEFAULT_VALUE = (100f, 1000f);

        private const float DEFAULT_DURABILITY = 100.0f;
        private const float DEFAULT_FRAGILITY = 100.0f;

        private const float DEFAULT_MASS = 1.0f;

        private static readonly ValuableVolume.Type DEFAULT_VALUABLE_VOLUME_TYPE = ValuableVolume.Type.Medium;

        public string Name { get; set; }
        public (float, float) Value { get; set; }
        public float Durability { get; set; }
        public float Fragility { get; set; }
        public float Mass { get; set; }
        public ValuableVolume.Type ValuableVolumeType { get; set; }
        public Gradient ParticleGradient { get; set; }

        public Data(string name, (float, float)? value = null, float mass = DEFAULT_MASS,
         ValuableVolume.Type? valuableVolumeType = null, Gradient particleGraident = null, float durability = DEFAULT_DURABILITY, float fragility = DEFAULT_FRAGILITY)
        {
            Name = name;
            Value = value ?? DEFAULT_VALUE;
            Durability = durability;
            Fragility = fragility;
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

    public ValuableAddition(string assetName, Data valuableData, ManualLogSource logger,, List<System.Type> additionalComponents = null)
    {
        _assetName = assetName;
        _logger = logger;
        ValuableData = valuableData;
        AdditionalComponents = additionalComponents;
    }

    public void Register(IModRegistry registry)
    {
        GameObject go = assetBundle.LoadAsset<GameObject>(_assetName);

        if (go != null)
        {

            // Add components
            go.AddComponent(typeof(PhotonTransformView));
            go.AddComponent(typeof(PhysGrabObject));
            go.AddComponent(typeof(RoomVolumeCheck));
            go.AddComponent(typeof(Rigidbody));
            go.AddComponent(typeof(PhysGrabObjectImpactDetector));
            go.AddComponent(typeof(PhotonView));
            go.AddComponent(typeof(DefaultBehaviour));

            if (!go.GetComponent<Collider>())
            {
                _logger.LogWarning(ValuableData.Name + " does not have a collider! Adding a BoxCollider!");
                BoxCollider bc = go.AddComponent(typeof(BoxCollider)) as BoxCollider;
            }

            go.AddComponent(typeof(PhysGrabObjectCollider));

            ValuableObject v = go.AddComponent(typeof(ValuableObject)) as ValuableObject;
            v.valuePreset = ScriptableObject.CreateInstance(typeof(Value)) as Value;
            v.valuePreset.valueMin = ValuableData.Value.Item1;
            v.valuePreset.valueMax = ValuableData.Value.Item2;

            v.durabilityPreset = ScriptableObject.CreateInstance(typeof(Durability)) as Durability;
            v.durabilityPreset.durability = ValuableData.Durability;
            v.durabilityPreset.fragility = ValuableData.Fragility;

            v.physAttributePreset = ScriptableObject.CreateInstance(typeof(PhysAttribute)) as PhysAttribute;
            v.physAttributePreset.mass = ValuableData.Mass;
            v.volumeType = ValuableData.ValuableVolumeType;
            v.durabilityPreset = ScriptableObject.CreateInstance(typeof(Durability)) as Durability;

            go.tag = "Phys Grab Object";
            go.name = ValuableData.Name;

            // Put the game object on the PhysGrabObject layer.
            // Many raycasts will not happen if the layer is not correct.
            go.layer = LayerMask.NameToLayer("PhysGrabObject");

            if (AdditionalComponents != null)
            {
                foreach (System.Type c in AdditionalComponents)
                {
                    go.AddComponent(c);
                }
            }

            _registry.Add(GetRegistryName(ValuableData), (go, ValuableData));
        }
        else
        {
            _logger.LogError("Could not register GameObject " + addition.AssetName + " as it does not exist in the asset bundle!");
        }
    }
}
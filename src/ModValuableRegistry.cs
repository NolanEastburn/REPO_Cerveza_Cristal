using System.Collections.Generic;
using UnityEngine;
using BepInEx.Logging;
using Photon.Pun;
using UnityEngine.UIElements.Collections;

namespace Cerveza_Cristal;

class ModValuableRegistry
{
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

    public void Register(string assetName, Data data, List<System.Type> components=null)
    {
        GameObject go = _assetBundle.LoadAsset<GameObject>(assetName);

        if (go != null)
        {

            // Add components
            go.AddComponent(typeof(PhotonTransformView));
            go.AddComponent(typeof(PhysGrabObject));
            go.AddComponent(typeof(RoomVolumeCheck));
            go.AddComponent(typeof(Rigidbody));
            go.AddComponent(typeof(PhysGrabObjectImpactDetector));
            go.AddComponent(typeof(PhotonView));

            if (!go.GetComponent<Collider>())
            {
                _logger.LogWarning(data.Name + " does not have a collider! Adding a BoxCollider!");
                go.AddComponent(typeof(BoxCollider));
            }

            go.AddComponent(typeof(BoxCollider));
            go.AddComponent(typeof(PhysGrabObjectCollider));

            ValuableObject v = go.AddComponent(typeof(ValuableObject)) as ValuableObject;
            v.valuePreset = ScriptableObject.CreateInstance(typeof(Value)) as Value;
            v.valuePreset.valueMin = data.Value.Item1;
            v.valuePreset.valueMax = data.Value.Item2;

            v.physAttributePreset = ScriptableObject.CreateInstance(typeof(PhysAttribute)) as PhysAttribute;
            v.physAttributePreset.mass = data.Mass;
            v.volumeType = data.ValuableVolumeType;
            v.durabilityPreset = ScriptableObject.CreateInstance(typeof(Durability)) as Durability;

            go.tag = "Phys Grab Object";
            go.name = data.Name;

            if (components != null)
            {
                foreach (System.Type c in components)
                {
                    go.AddComponent(c);
                }
            }

            Registry.Add(data.Name, (go, data));
        }
        else
        {
            _logger.LogError("Could not register GameObject " + assetName + " as it does not exist in the asset bundle!");
        }
    }

    public ModValuableRegistry(AssetBundle assetBundle, ManualLogSource logger)
    {
        _assetBundle = assetBundle;
        _logger = logger;

        Registry = new Dictionary<string, (GameObject, Data)>();
    }
}
namespace Cerveza_Cristal;

using System.Collections.Generic;
using System.Collections.Specialized;
using BepInEx.Logging;
using Photon.Pun;
using UnityEngine;

public class ValuableAddition : ModAddition
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

    public Data ValuableData { get; private set; }


    public struct Data
    {
        private static readonly (float, float) DEFAULT_VALUE = (100f, 1000f);

        private const float DEFAULT_DURABILITY = 100.0f;
        private const float DEFAULT_FRAGILITY = 100.0f;

        private const float DEFAULT_MASS = 1.0f;

        private static readonly ValuableVolume.Type DEFAULT_VALUABLE_VOLUME_TYPE = ValuableVolume.Type.Medium;
        public (float, float) Value { get; set; }
        public float Durability { get; set; }
        public float Fragility { get; set; }
        public float Mass { get; set; }
        public ValuableVolume.Type ValuableVolumeType { get; set; }
        public Gradient ParticleGradient { get; set; }

        public Data((float, float)? value = null, float mass = DEFAULT_MASS,
         ValuableVolume.Type? valuableVolumeType = null, Gradient particleGradient = null, float durability = DEFAULT_DURABILITY, float fragility = DEFAULT_FRAGILITY)
        {
            Value = value ?? DEFAULT_VALUE;
            Durability = durability;
            Fragility = fragility;
            Mass = mass;
            ValuableVolumeType = valuableVolumeType ?? DEFAULT_VALUABLE_VOLUME_TYPE;

            if (particleGradient == null)
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
                ParticleGradient = particleGradient;
            }
        }

    }

    public ValuableAddition(string assetName, string name, Data valuableData, ManualLogSource logger, List<System.Type> additionalComponents = null) : base(assetName, name, logger, additionalComponents)
    {
        ValuableData = valuableData;
    }

    public override GameObject CreateGameObject(AssetBundle assetBundle)
    {
        /* 
            Valuable hierarchy:
            Root (PhotonView, ValuableObject, PhysGrabObjectImpactDetector, RoomVolumeCheck, PhysGrabObject, RigidBody, PhotonTransformView, PhysGrabObject layer)
                Object
                    Mesh (Contains mesh)
                    Valuable Collider (Collider, PhysGrabObject<Collider Type>Collider, PhysGrabObjectCollider)
                    Valuable Collider (1)
                    ...
        */

        // Besides the collider GameObjects, this hierarchy should be present from the PreFab in the AssetBundle.

        GameObject root = assetBundle.LoadAsset<GameObject>(AssetName);

        if (root != null)
        {

            // Add components
            root.AddComponent(typeof(PhotonTransformView));
            root.AddComponent(typeof(PhysGrabObject));
            root.AddComponent(typeof(RoomVolumeCheck));
            root.AddComponent(typeof(Rigidbody));
            root.AddComponent(typeof(PhysGrabObjectImpactDetector));

            PhotonView pv = root.AddComponent(typeof(PhotonView)) as PhotonView;
            pv.observableSearch = PhotonView.ObservableSearch.AutoFindAll;

            root.AddComponent(typeof(DefaultBehaviour));

            // Get the "Object" GameObject (first child of the root)
            GameObject obj = root.transform.GetChild(0).gameObject;

            if (obj.GetComponentsInChildren<Collider>().Length == 0)
            {
                _logger.LogWarning(Name + " does not have a collider! Adding a BoxCollider!");
                GameObject boxColliderGo = new GameObject("Valuable Collider");
                boxColliderGo.AddComponent(typeof(BoxCollider));
                boxColliderGo.AddComponent(typeof(PhysGrabObjectBoxCollider));
                boxColliderGo.AddComponent(typeof(PhysGrabObjectCollider));
                boxColliderGo.layer = Utils.VALUABLE_LAYER_MASK;
            }

            // Add the PhysGrabObjectColliders to each Valuable Collider GameObject.
            for (int i = 0; i < obj.transform.childCount; ++i)
            {
                GameObject colliderGo = obj.transform.GetChild(i).gameObject;

                // Skip if contains no colliders. Only process a single collider as each GameObject should have a single collider per the observed
                // REPO Valuable hierarchy.
                if (colliderGo.GetComponent<Collider>())
                {
                    // 4 supported colliders, Box, Sphere, Mesh, and Capsule
                    if (colliderGo.GetComponent<BoxCollider>())
                    {
                        colliderGo.AddComponent(typeof(PhysGrabObjectBoxCollider));
                    }
                    else if (colliderGo.GetComponent<SphereCollider>())
                    {
                        colliderGo.AddComponent(typeof(PhysGrabObjectSphereCollider));
                    }
                    else if (colliderGo.GetComponent<PhysGrabObjectMeshCollider>())
                    {
                        colliderGo.AddComponent(typeof(PhysGrabObjectMeshCollider));
                    }
                    else if (colliderGo.GetComponent<PhysGrabObjectCapsuleCollider>())
                    {
                        colliderGo.AddComponent(typeof(PhysGrabObjectCapsuleCollider));
                    }

                    // Always add the PhysGrabObjectCollider
                    colliderGo.AddComponent(typeof(PhysGrabObjectCollider));

                    // Assign correct layer number.
                    colliderGo.layer = Utils.VALUABLE_LAYER_MASK;
                    colliderGo.tag = "Phys Grab Object";

                    // Copy REPO material values
                    Collider c = colliderGo.GetComponent<Collider>();
                    c.material.bounciness = 0.3f;
                    c.material.dynamicFriction = 0.25f;
                    c.material.staticFriction = 0.05f;

                    c.sharedMaterial.bounciness = c.material.bounciness;
                    c.sharedMaterial.dynamicFriction = c.material.dynamicFriction;
                    c.sharedMaterial.staticFriction = c.sharedMaterial.staticFriction;
                }
            }

            ValuableObject v = root.AddComponent(typeof(ValuableObject)) as ValuableObject;
            v.valuePreset = ScriptableObject.CreateInstance(typeof(Value)) as Value;
            v.valuePreset.valueMin = ValuableData.Value.Item1;
            v.valuePreset.valueMax = ValuableData.Value.Item2;

            v.durabilityPreset = ScriptableObject.CreateInstance(typeof(Durability)) as Durability;
            v.durabilityPreset.durability = ValuableData.Durability;
            v.durabilityPreset.fragility = ValuableData.Fragility;

            v.physAttributePreset = ScriptableObject.CreateInstance(typeof(PhysAttribute)) as PhysAttribute;
            v.physAttributePreset.mass = ValuableData.Mass;
            v.volumeType = ValuableData.ValuableVolumeType;

            root.tag = "Phys Grab Object";
            root.name = Name;

            // Put the game object on the PhysGrabObject layer.
            // Many raycasts will not happen if the layer is not correct.
            root.layer = Utils.VALUABLE_LAYER_MASK;
            obj.layer = Utils.VALUABLE_LAYER_MASK;

            if (_additionalComponents != null)
            {
                foreach (System.Type c in _additionalComponents)
                {
                    root.AddComponent(c);
                }
            }

            return root;
        }
        else
        {
            _logger.LogError("Could not create a GameObject of " + AssetName + " as it does not exist in the asset bundle!");
            return null;
        }
    }

}
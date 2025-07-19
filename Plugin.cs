using System;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cerveza_Cristal;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private const string MOD_CONTENT_FOLDER = "nooterdooter_cerveza_cristal";
    private const string RESOURCES_FOLDER = "res";
    private const string MESH_FOLDER = "mesh";

    private Boolean spawned = false;
    private const float CRASH_TIME = 1;

    private static void BuildAssetBundle()
    {
        
    }

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }


    private void Update()
    {
        if (Time.time >= CRASH_TIME && !spawned)
        {
            GameObject testValuable = new GameObject("testValuable");
            MeshFilter mf = testValuable.AddComponent<MeshFilter>();
            MeshRenderer mr = testValuable.AddComponent<MeshRenderer>();
            mr.material.color = new Color(1.0f, 0f, 0f);

            mf.sharedMesh = Resources.Load<Mesh>("Meshes/TestCone.fbx");

            if (mf.sharedMesh == null)
            {
                Logger.LogError("Could not load the cone mesh!");
            }
            // Find truck.

            GameObject truck = null;


            foreach (GameObject g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                TruckLandscapeScroller obj = g.GetComponentInChildren<TruckLandscapeScroller>();

                if (obj != null)
                {
                    truck = obj.gameObject;
                }
            }

            spawned = true;
            GameObject thing = UnityEngine.Object.Instantiate(AssetManager.instance.surplusValuableBig, truck.transform.position + new Vector3(-35, 10, 0), Quaternion.identity);
            thing.transform.localScale = new Vector3(5, 10, 5);
            Logger.LogInfo("Spawned a thing!");

            // Add the valuable to the scene.
            testValuable.SetActive(true);
        }

    }

}

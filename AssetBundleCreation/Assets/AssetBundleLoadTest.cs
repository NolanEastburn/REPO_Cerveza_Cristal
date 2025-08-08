using System.IO;
using UnityEngine;

public class LoadFromFileExample : MonoBehaviour
{
    private Vector3 velocity = new Vector3(1f, 0, 0);
    private Vector3 angularVelocity = new Vector3(0f, 30f, 0);

    GameObject go = null;

    void Start()
    {
        var myLoadedAssetBundle
            = AssetBundle.LoadFromFile(Path.Combine(Application.dataPath, "AssetBundles", "primaryassetbundle"));
        if(myLoadedAssetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle!");
            return;
        }
        go = myLoadedAssetBundle.LoadAsset<GameObject>("Cone");
        go = Instantiate(go);

        go.transform.position = Vector3.zero;
    }

    void Update()
    {
        float t = Time.deltaTime;
        go.transform.position += velocity * t;
        go.transform.rotation *= Quaternion.Euler(angularVelocity * t);
    }
}
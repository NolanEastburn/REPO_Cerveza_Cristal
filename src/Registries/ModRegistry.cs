namespace Cerveza_Cristal;

using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

public abstract class ModRegistry<T> where T : ModAddition
{
    public Dictionary<string, (GameObject, T)> Registry { get; private set; } = null;

    public AssetBundle TheAssetBundle { get; private set; }

    private ManualLogSource _logger { get; set; }

    public ModRegistry(AssetBundle assetBundle, ManualLogSource logger)
    {
        TheAssetBundle = assetBundle;
        _logger = logger;

        Registry = new Dictionary<string, (GameObject, T)>();
    }


    public (GameObject, T) GetRegistryEntry(T addition)
    {
        return Registry[GetRegistryName(addition)];
    }

    public (GameObject, T) GetRegistryEntry(string key)
    {
        return Registry[key];
    }

    public abstract void ApplyAdditionRegistrations(RunManager runManager);

    public abstract PrefabRef CreatePrefabRef((GameObject, T) regEntry);


    public string GetRegistryName(T addition)
    {
        return TheAssetBundle.name + "." + addition.Name;
    }

}
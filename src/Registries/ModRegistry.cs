namespace Cerveza_Cristal;

using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

public abstract class ModRegistry<T> : IModRegistry where T : ModAddition
{
    public Dictionary<string, (GameObject, T)> RegistryDictionary { get; private set; } = null;

    public AssetBundle TheAssetBundle { get; private set; }

    private ManualLogSource _logger { get; set; }

    public ModRegistry(AssetBundle assetBundle, ManualLogSource logger)
    {
        TheAssetBundle = assetBundle;
        _logger = logger;

        RegistryDictionary = new Dictionary<string, (GameObject, T)>();
    }

    public void Register(T addition)
    {
        GameObject go = addition.CreateGameObject(TheAssetBundle);

        if (go != null)
        {
            RegistryDictionary.Add(GetRegistryName(addition), (go, addition));
        }
    }

    public (GameObject, T) GetRegistryEntry(T addition)
    {
        return RegistryDictionary[GetRegistryName(addition)];
    }

    public (GameObject, T) GetRegistryEntry(string key)
    {
        return RegistryDictionary[key];
    }

    public abstract void ApplyAdditionRegistrations(RunManager runManager);

    public abstract PrefabRef CreatePrefabRef((GameObject, T) regEntry);


    public string GetRegistryName(T addition)
    {
        return TheAssetBundle.name + "." + addition.Name;
    }

}
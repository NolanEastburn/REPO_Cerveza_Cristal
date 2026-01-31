namespace Cerveza_Cristal;

using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;

public abstract class ModRegistry<T> : IModRegistry where T : IAddition
{
    private Dictionary<string, (GameObject, T)> _registry { get; set; } = null;

    private AssetBundle _assetBundle { get; set; }

    private ManualLogSource _logger { get; set; }

    public ModRegistry(AssetBundle assetBundle, ManualLogSource logger)
    {
        _assetBundle = assetBundle;
        _logger = logger;

        _registry = new Dictionary<string, (GameObject, T)>();
    }

    public Dictionary<string, (GameObject, T)> GetRegistry()
    {
        return _registry;
    }

    public (GameObject, T) GetRegistryEntry(T addition)
    {
        return _registry[GetRegistryName(addition)];
    }

    public (GameObject, T) GetRegistryEntry(string key)
    {
        return _registry[key];
    }

    public abstract void ApplyAdditionRegistrations(RunManager runManager);

    public abstract PrefabRef CreatePrefabRef((GameObject, T) regEntry);


    public string GetRegistryName(T addition)
    {
        return _assetBundle.name + "." + addition.GetName();
    }

}
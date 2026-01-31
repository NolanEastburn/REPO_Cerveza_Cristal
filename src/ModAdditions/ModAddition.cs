namespace Cerveza_Cristal;

using System.Collections.Generic;
using BepInEx.Logging;
using UnityEngine;


public abstract class ModAddition
{
    protected ManualLogSource _logger { get; set; }
    public string AssetName { get; set; }
    public string Name { get; set; }

    protected List<System.Type> _additionalComponents { get; set; }

    public abstract GameObject CreateGameObject(AssetBundle assetBundle);

    public ModAddition(string assetName, string name, ManualLogSource logger, List<System.Type> additionalComponents = null)
    {
        AssetName = assetName;
        Name = name;
        _logger = logger;
        _additionalComponents = additionalComponents;
    }
}
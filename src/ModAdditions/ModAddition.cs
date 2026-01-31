namespace Cerveza_Cristal;

using BepInEx.Logging;
using System.Collections.Generic;


public abstract class ModAddition
{
    protected ManualLogSource _logger { get; set; }
    public string AssetName { get; set; }
    public string Name { get; set; }

    protected List<System.Type> _additionalComponents { get; set; }

    public abstract void Register<T>(ModRegistry<T> registry) where T : ModAddition;

    public ModAddition(string assetName, string name, ManualLogSource logger, List<System.Type> additionalComponents = null)
    {
        AssetName = assetName;
        Name = name;
        _logger = logger;
        _additionalComponents = additionalComponents;
    }
}
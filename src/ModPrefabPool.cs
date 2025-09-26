using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using BepInEx.Logging;

namespace Cerveza_Cristal;

class ModPrefabPool : IPunPrefabPool
{
    private IPunPrefabPool _defaultPool { set; get; } = null;

    private ModValuableRegistry _modValuableRegistry { get; set; }

    private ManualLogSource _logger { get; set; }

    private string ExtractGameObjectName(string prefabId)
    {
        return prefabId.Substring(prefabId.LastIndexOf('/') + 1);
    }

    private bool IsModAddition(string prefabId)
    {
        return _modValuableRegistry.Registry.ContainsKey(ExtractGameObjectName(prefabId));
    }

    public ModPrefabPool(ModValuableRegistry modValuableRegistry, ManualLogSource logger)
    {
        _defaultPool = PhotonNetwork.PrefabPool;
        _modValuableRegistry = modValuableRegistry;
        _logger = logger;
    }

    public void Destroy(GameObject gameObject)
    {
        _defaultPool.Destroy(gameObject);
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        if (IsModAddition(prefabId))
        {
            string key = ExtractGameObjectName(prefabId);

            if (_modValuableRegistry.Registry.ContainsKey(key))
            {
                GameObject go = Object.Instantiate(_modValuableRegistry.Registry[ExtractGameObjectName(prefabId)].Item1, position, rotation);
                go.SetActive(false);
                return go;
            }
            else
            {
                _logger.LogWarning("Prefab with name " + key + " could not be found in the mod valuable registry!");
                return null;
            }
        }
        else
        {
            return _defaultPool.Instantiate(prefabId, position, rotation);
        }
    }
}

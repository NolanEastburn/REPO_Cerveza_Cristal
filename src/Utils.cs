using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using UnityEngine;

namespace Cerveza_Cristal
{
    public enum LevelTypes
    {
        ARCTIC = 0,
        MANOR = 1,
        MUSEUM = 2,
        WIZARD = 3,
        NONE = 4
    }

    public static class Utils
    {

        public static readonly int VALUABLE_LAYER_MASK = LayerMask.NameToLayer("PhysGrabObject");

        public const string UNIQUE_ORG_STRING = "com.nooterdooter.cerveza_cristal";

        private static ManualLogSource _logger { get; set; } = null;


        public static string LevelTypeString(LevelTypes levelType)
        {
            string result = "Level - ";

            switch (levelType)
            {
                case LevelTypes.ARCTIC:
                    result += "Arctic";
                    break;
                case LevelTypes.MANOR:
                    result += "Manor";
                    break;
                case LevelTypes.WIZARD:
                    result += "Wizard";
                    break;
                case LevelTypes.MUSEUM:
                    result += "Museum";
                    break;
                default:
                    result += "NON-EXTRACTION LEVEL";
                    break;
            }

            return result;
        }

        public static void SetLogger(ManualLogSource logger)
        {
            _logger = logger;
        }

        public static RunManager GetRunManager()
        {
            RunManager result = RunManager.instance;

            if (result == null)
            {
                throw new RepoSingletonNullException(typeof(RunManager));
            }

            return result;
        }

        public static ValuableDirector GetValuableDirector()
        {
            ValuableDirector result = ValuableDirector.instance;

            if (result == null)
            {
                throw new RepoSingletonNullException(typeof(ValuableDirector));
            }

            return result;
        }

        public static GameDirector GetGameDirector()
        {
            GameDirector result = GameDirector.instance;

            if (result == null)
            {
                throw new RepoSingletonNullException(typeof(GameDirector));
            }

            return result;
        }

        public static bool IsExtractionLevelRunning()
        {
            try
            {
                RunManager runManager = GetRunManager();

                Level level = runManager.levelCurrent;

                List<Level> nonExtractionLevels = new List<Level>() {runManager.levelArena, runManager.levelLobby, runManager.levelLobbyMenu,
             runManager.levelMainMenu, runManager.levelRecording, runManager.levelShop, runManager.levelSplashScreen, runManager.levelTutorial};

                return level != null && !nonExtractionLevels.Contains(level);
            }
            catch (RepoSingletonNullException e)
            {
                _logger.LogWarning(string.Format("Could not determine if an extraction level is running due to the following exception: {0}", e.Message));
                return false;
            }
        }

        public static List<GameObject> GetLevelModValuableInstances(ValuableAddition addition)
        {
            return GetLevelGameObjectsByName(addition.Name);
        }

        public static List<GameObject> GetLevelGameObjectsByName(string moduleName)
        {
            List<GameObject> result = new List<GameObject>();

            if (!IsExtractionLevelRunning())
            {
                _logger.LogWarning("Could not get the modules for the level because no extraction level is running!");
                return result;
            }


            foreach (GameObject m in UnityEngine.Object.FindObjectsOfType<GameObject>(includeInactive: false))
            {
                if (m.name.ToLower().Contains(moduleName.ToLower()))
                {
                    result.Add(m);
                }
            }

            return result;
        }

        public static List<ValuableObject> ContainedValuables(GameObject volume)
        {
            // Temporary HashSet to make sure we don't count valuables twice.
            HashSet<ValuableObject> valuableHs = new HashSet<ValuableObject>();

            foreach (ValuableObject v in volume.gameObject.GetComponentsInChildren<ValuableObject>())
            {
                valuableHs.Add(v);
            }

            return new List<ValuableObject>(valuableHs.ToList<ValuableObject>());
        }

        // TODO: Get this working for multiplayer as well (not super important however)
        public static void SpawnModValuable(ModValuableRegistry registry, ValuableAddition valuable)
        {
            // Check to make sure that an extraction level has been started.
            if (!IsExtractionLevelRunning())
            {
                throw new RepoInvalidActionException(action: string.Format("spawn the following custom valuable: {0}", registry.GetRegistryName(valuable)), reason: "an extraction level is not running");
            }

            GameDirector director;
            try
            {
                director = GetGameDirector();
                Transform playerTransform = director.PlayerList[0].gameObject.transform;
                UnityEngine.Object.Instantiate(registry.GetRegistryEntry(valuable).Item1, playerTransform.position, playerTransform.rotation);
            }
            catch (RepoSingletonNullException e)
            {
                _logger.LogWarning(string.Format("Could not spawn {0} due to the following exception: {1}", valuable.AssetName, e.Message));
            }

        }
    }

}

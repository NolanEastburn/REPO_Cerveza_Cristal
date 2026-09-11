namespace Cerveza_Cristal;

using System;
using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

class JingleBehaviour : MonoBehaviour
{
    private const float OPEN_ANGLE = 45.0f;

    private float _targetTime { get; set; } = 0.0f;

    private const float TARGET_TIME_DELTA = 0.5f;

    private bool _foundDoor { get; set; } = false;

    private Quaternion _startingRotation { get; set; } = Quaternion.identity;

    private bool _doorOpened { get; set; }

    private GameObject _door { get; set; }

    private AudioSource _jingleAudioSource { get; set; }

    private Sound _jingleSound { get; set; }

    private Vector3 _doorLocUponOpen { get; set; } = new Vector3();


    public void Awake()
    {
        _jingleAudioSource = gameObject.GetComponentInChildren<AudioSource>();
        _jingleSound = new Sound
        {
            Source = _jingleAudioSource,
            Type = AudioManager.AudioType.Default
        };
    }

    public void OnEnable()
    {
        StartCoroutine(FindDoor());
    }

    private IEnumerator FindDoor()
    {
        Regex doorRegex = new Regex(@"^(D|d)oor");

        // Look for the door every 5 seconds
        while (!_foundDoor)
        {
            // Raycast to find the door
            RaycastHit hitInfo;
            bool didHit = Physics.Raycast(
                origin: transform.position + new Vector3(x: 0, y: 0.1f, z: 0),
                direction: transform.forward,
                hitInfo: out hitInfo,
                maxDistance: 10.0f,
                layerMask: LayerMask.GetMask("PhysGrabObjectHinge"));

            if (didHit)
            {
                GameObject hitObject = hitInfo.transform.gameObject;

                if (doorRegex.IsMatch(hitObject.name))
                {
                    _door = hitObject;
                    _startingRotation = _door.transform.rotation;
                    _foundDoor = true;
                }
            }

            yield return new WaitForSeconds(5.0f);
        }

        yield break;
    }

    private IEnumerator PlayJingle()
    {
        // Play the jingle to the player who opened the door.
        // The player who opened the door will be defined to be the player closest
        // to the door when it was opened. I would have done the player actively grabbing the door
        // when it was opened, but this won't always work since the door can be flung open
        // and not actively held when it reaches its open angle.

        GameDirector gameDir = null;
        try
        {
            gameDir = Utils.GetGameDirector();
        }
        catch (RepoSingletonNullException)
        {
            ModEntry.Instance.Logger.LogError("Null GameDirector returned when attempting to play the jingle!");
        }

        PlayerAvatar closestPlayer = null;
        float distance = float.MaxValue;
        foreach (PlayerAvatar player in gameDir.PlayerList)
        {
            // Use reflection to get the internal isDisabled field.
            FieldInfo field = player.GetType().GetField("isDisabled", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
            bool isDisabled = (bool)field.GetValue(player);
            if (!isDisabled)
            {
                if (closestPlayer == null)
                {
                    closestPlayer = player;
                }
                else
                {
                    // Check if this player is closer to the door.
                    float tempDist = Vector3.Distance(player.playerTransform.position, _doorLocUponOpen);
                    if (tempDist < distance)
                    {
                        distance = tempDist;
                        closestPlayer = player;
                    }

                }
            }

        }

        // Closest player has now been determined.
        ModEntry.Instance.Logger.LogInfo($"The closest player is {closestPlayer}");

        _jingleSound.Play(closestPlayer.playerTransform);
        yield return new WaitUntil(() => !_jingleAudioSource.isPlaying);
        Destroy(this);
        yield break;
    }

    public void Update()
    {
        if (Time.time >= _targetTime && !_doorOpened && _foundDoor)
        {
            if (Quaternion.Angle(_startingRotation, _door.transform.rotation) >= OPEN_ANGLE)
            {
                // Play the jingle and then cleanup this object via a co-routine.
                _doorOpened = true;
                _doorLocUponOpen = _door.transform.position;
                StartCoroutine(PlayJingle());
            }
            else
            {
                _targetTime = Time.time + TARGET_TIME_DELTA;
            }
        }
    }
}
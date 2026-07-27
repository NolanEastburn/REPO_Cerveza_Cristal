namespace Cerveza_Cristal;

using System.Collections;
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

    private AudioSource _jingle { get; set; }


    public void Awake()
    {
        _jingle = gameObject.GetComponentInChildren<AudioSource>();
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
        _jingle.Play();
        yield return new WaitUntil(() => !_jingle.isPlaying);
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
                StartCoroutine(PlayJingle());
            }
            else
            {
                _targetTime = Time.time + TARGET_TIME_DELTA;
            }
        }
    }
}
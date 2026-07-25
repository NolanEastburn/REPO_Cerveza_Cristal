namespace Cerveza_Cristal;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

class JingleBehaviour : MonoBehaviour
{
    private bool _foundDoor { get; set; } = false;
    private bool _doorOpened { get; set; }

    private GameObject _door { get; set; }

    public void OnEnable()
    {
        StartCoroutine(FindDoor());
    }

    private IEnumerator FindDoor()
    {
        Regex doorRegex = new Regex(@"^(D|d)oor");

        // Look for the door every 10 seconds
        while (!_foundDoor)
        {
            // Raycast to find the door
            RaycastHit hitInfo;
            bool didHit = Physics.Raycast(
                origin: transform.position,
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
                    _foundDoor = true;
                    ModEntry.Instance.Logger.LogInfo("Found the door!!!!");
                }
                else
                {
                    ModEntry.Instance.Logger.LogInfo($"Did not hit the door. What was hit: {hitObject.name}");
                }
            }
            else
            {
                ModEntry.Instance.Logger.LogInfo("Bottle did hit anything on the door search raycast");
            }

            yield return new WaitForSeconds(10.0f);
        }

        yield break;
    }

    public void Update()
    {
        // Only look for the door opening once the door is found.
        if (_foundDoor)
        {

        }
    }
}
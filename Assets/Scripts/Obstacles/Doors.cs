using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    public bool isOpen = false;
    public IEnumerator OpenDoor()
    {        
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, 90, 0); // Rotates 90 degrees around Y axis
        float duration = 0.1f; // Duration in seconds
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRotation; // Ensure final rotation is set
    }
}

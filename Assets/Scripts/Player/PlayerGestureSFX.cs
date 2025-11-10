using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGestureSFX : MonoBehaviour
{
    public void PlayGestureSound(int index)
    {
        AudioManager.instance.PlaySound(index);
    }
}

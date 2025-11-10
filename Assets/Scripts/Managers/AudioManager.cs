using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    float lastCoinSoundTime;

    public AudioSource[] audioSource;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    public void PlaySound(int index)
    {
        if (index < 0 || index >= audioSource.Length)
        {
            Debug.LogWarning("PlaySound: Index out of range.");
            return;
        }

        audioSource[index].Play();
    }



    public void PlayCoin()
    {
        if (Time.time - lastCoinSoundTime < 0.05f)
            return; // prevents spam faster than 20 sounds per second

        lastCoinSoundTime = Time.time;
        audioSource[16].PlayOneShot(audioSource[16].clip);
    }

    public void StopSound(int index) => audioSource[index].Stop();

    public void PlayFootstep()
    {
        if (audioSource[0].isPlaying == false)
            audioSource[0].Play();
    }

  

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    float lastCoinSoundTime;

    public AudioSource[] audioSource;
    public AudioSource mainMenuBGM;

    public Slider SFXslider;
    public Slider musicSlider;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if(PlayerPrefs.HasKey("SFXVolume"))        
            SFXslider.value = PlayerPrefs.GetFloat("SFXVolume");
        else
            SFXslider.value = 1f;

        if(PlayerPrefs.HasKey("MusicVolume"))        
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        else
            musicSlider.value = 1f;

        SetSFXVolume();
        SetMusicVolume();
        mainMenuBGM.Play();
    }

    public void SetSFXVolume()
    {
        for (int i = 0; i < audioSource.Length; i++)
        {
            audioSource[i].volume = SFXslider.value;
        }
        PlayerPrefs.SetFloat("SFXVolume", SFXslider.value);
    }

    public void SetMusicVolume()
    {
        mainMenuBGM.volume = musicSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
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

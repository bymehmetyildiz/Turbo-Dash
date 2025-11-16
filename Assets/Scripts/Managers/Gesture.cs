using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Gesture : MonoBehaviour
{
    public string gestureName;
    public int gestureID;
    public int price;
    public TMP_Text gestureNameText;
    public TMP_Text gesturePriceText;
    public bool isOwned = false;
    public bool isPlaying = false;
    public GameObject coin;
    public AudioSource audioSource;


    private void OnEnable()
    {
        gestureNameText.text = gestureName;
        gesturePriceText.text = price.ToString();
        audioSource = GetComponentInChildren<AudioSource>();
    }

    public void PlayGesture()
    {
        if (isOwned && audioSource != null && !audioSource.isPlaying)
            audioSource.Play();
    }
}

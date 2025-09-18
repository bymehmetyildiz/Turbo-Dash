using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestureManager : MonoBehaviour
{ 
    public static GestureManager instance;
    public Gesture[] gestures;
    public Gesture currentGesture;
    private Player player;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        gestures = GetComponentsInChildren<Gesture>();
        player = Player.instance;
    }

    public void PurchaseGesture(Gesture gesture)
    {
        for (int i = 0; i < gestures.Length; i++)
        {
            if (gestures[i].isPlaying)
            {
                Debug.Log("Another gesture is currently playing. Please wait.");
                return;
            }
        }

        currentGesture = gesture;

        if (!currentGesture.isOwned)
        {
            if (player.coinAmount >= currentGesture.price)
            {
                player.coinAmount -= currentGesture.price;
                currentGesture.isOwned = true;
                currentGesture.gesturePriceText.text = "Play";    
                currentGesture.coin.SetActive(false);
            }
            else
            {
                Debug.Log("Not enough coins to purchase this gesture.");
            }
               
        }
        else
        {
            if (!player.isStarted && !currentGesture.isPlaying)
            {
                player.danceIndex = currentGesture.gestureID;
                player.stateMachine.ChangeState(player.gestureState);
                currentGesture.isPlaying = true;
                currentGesture.gesturePriceText.text = "Playing";
            }
        }

    }

}

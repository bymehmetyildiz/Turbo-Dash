using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GestureManager : MonoBehaviour
{
    public static GestureManager instance;

    public Gesture[] gestures;
    public Gesture currentGesture;
    private Player player;

    // --- PlayerPrefs keys ---
    private static string K_Owned(int id) => $"gesture_{id}_owned";
    private const string K_LastSelected = "gesture_last_selected";

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        gestures = GetComponentsInChildren<Gesture>(includeInactive: true);
        player = Player.instance;

        // Sanity check: all IDs must be unique
        var seen = new HashSet<int>();
        foreach (var g in gestures)
        {
            if (!seen.Add(g.gestureID))
                Debug.LogWarning($"[GestureManager] Duplicate gestureID detected: {g.gestureID} on {g.name}");
        }

        LoadGesturesFromPrefs();
        RefreshAllGestureUI();
    }


    // -------------------- PERSISTENCE --------------------
    private void LoadGesturesFromPrefs()
    {
        if (gestures == null) return;

        foreach (var g in gestures)
        {
            // Default is locked (0). No auto-owning any ID.
            g.isOwned = PlayerPrefs.GetInt(K_Owned(g.gestureID), 0) == 1;
            g.isPlaying = false;
        }

        // (Optional) restore last selected gesture id
        int lastId = PlayerPrefs.GetInt(K_LastSelected, -1);
        currentGesture = GetGestureById(lastId);
    }


    private void SaveGestureOwned(Gesture g)
    {
        PlayerPrefs.SetInt(K_Owned(g.gestureID), g.isOwned ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SaveLastSelected(Gesture g)
    {
        PlayerPrefs.SetInt(K_LastSelected, g.gestureID);
        PlayerPrefs.Save();
    }

    // -------------------- UI HELPERS --------------------
    private void RefreshAllGestureUI()
    {
        if (gestures == null) return;
        foreach (var g in gestures) RefreshGestureUI(g);
    }

    private void RefreshGestureUI(Gesture g)
    {
        if (g == null) return;

        if (g.isOwned)
        {
            // Owned → show "Play" (unless currently playing)
            if (g.gesturePriceText != null)
                g.gesturePriceText.text = g.isPlaying ? "Playing" : "Play";

            if (g.coin != null)
                g.coin.SetActive(false);
        }
        else
        {
            // Not owned → show price and coin
            if (g.gesturePriceText != null)
                g.gesturePriceText.text = NumberFormatter.FormatNumber(g.price);

            if (g.coin != null)
                g.coin.SetActive(true);
        }
    }

    private Gesture GetGestureById(int id)
    {
        if (gestures == null) return null;
        foreach (var g in gestures) if (g.gestureID == id) return g;
        return null;
    }

    // -------------------- PUBLIC API --------------------
    public void PurchaseGesture(Gesture gesture)
    {
        // Block if another gesture is playing
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
            // Try to buy
            if (player.totalCoinAmount >= currentGesture.price)
            {
                player.totalCoinAmount -= currentGesture.price;
                UIManager.instance.UpdateTotalCoin();

                currentGesture.isOwned = true;
                SaveGestureOwned(currentGesture);

                // Reflect UI
                RefreshGestureUI(currentGesture);

                // Also persist general save (coins, etc.)
                SaveManager.instance?.SaveGame();
            }
            else
            {
                Debug.Log("Not enough coins to purchase this gesture.");
            }
        }
        else
        {
            // Owned → try to play (only when not running)
            if (!player.isStarted && !currentGesture.isPlaying)
            {
                player.danceIndex = currentGesture.gestureID;
                player.stateMachine.ChangeState(player.gestureState);

                currentGesture.isPlaying = true;
                RefreshGestureUI(currentGesture);

                SaveLastSelected(currentGesture);
            }
        }
    }

    /// <summary>
    /// Call this from an Animation Event (end of the gesture clip)
    /// </summary>
    public void OnCurrentGestureFinished()
    {
        if (currentGesture == null) return;   
        currentGesture.isPlaying = false;
        RefreshGestureUI(currentGesture);
    }

    // Handy reset for testing
    [ContextMenu("Reset Gestures (keep coins)")]
    private void ResetGestures()
    {
        if (gestures == null) return;

        foreach (var g in gestures)
        {
            PlayerPrefs.DeleteKey(K_Owned(g.gestureID));
            g.isOwned = (g.gestureID == 0); // back to default free for id 0
            g.isPlaying = false;
        }
        PlayerPrefs.DeleteKey(K_LastSelected);
        PlayerPrefs.Save();

        RefreshAllGestureUI();
    }

    // GestureManager.cs
    public void DeleteAllGestureSavesAndResetRuntime()
    {
        if (gestures == null || gestures.Length == 0)
            gestures = GetComponentsInChildren<Gesture>(includeInactive: true);

        // Delete only keys that correspond to actual gestures
        foreach (var g in gestures)
            PlayerPrefs.DeleteKey(K_Owned(g.gestureID));

        PlayerPrefs.DeleteKey(K_LastSelected);
        PlayerPrefs.Save();

        // Reset runtime: all locked, none playing
        foreach (var g in gestures)
        {
            g.isOwned = false;      // <— lock all
            g.isPlaying = false;
        }
        currentGesture = null;
        RefreshAllGestureUI();
    }


}

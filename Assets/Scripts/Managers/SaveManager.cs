using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else { Destroy(gameObject); return; }
    }

    public void SaveGame()
    {
        var p = Player.instance ?? FindObjectOfType<Player>();
        if (p == null)
        {
            Debug.LogWarning("SaveGame: Player not found.");
            return;
        }
      
        PlayerPrefs.SetFloat("TankReloadDur", p.tankReloadDur);
        PlayerPrefs.SetFloat("PlaneReloadDur", p.planeReloadDur);
        PlayerPrefs.SetInt("VehicleIndex", p.vehicleIndex);
        PlayerPrefs.SetInt("CarIndex", p.carIndex);
        PlayerPrefs.SetInt("ColorIndex", p.colorIndex);
        PlayerPrefs.SetFloat("CarDriveDur", p.carDriveDur);
        PlayerPrefs.SetFloat("JetDriveDur", p.jetDriveDur);
        PlayerPrefs.SetFloat("TankDriveDur", p.tankDriveDur);
        PlayerPrefs.SetFloat("PlaneFlyDur", p.planeFlyDur);
        PlayerPrefs.SetInt("TotalCoinAmount", p.totalCoinAmount);
        PlayerPrefs.SetInt("IsControlShown", UIManager.instance != null && UIManager.instance.isControlsShown ? 1 : 0);

        PlayerPrefs.Save();
    }

    public void LoadGame()
    {
        var p = Player.instance ?? FindObjectOfType<Player>();
        if (p == null)
        {
            Debug.LogWarning("LoadGame: Player not found yet. Try calling again later.");
            return;
        }
    
        if (PlayerPrefs.HasKey("TankReloadDur")) p.tankReloadDur = PlayerPrefs.GetFloat("TankReloadDur");
        if (PlayerPrefs.HasKey("PlaneReloadDur")) p.planeReloadDur = PlayerPrefs.GetFloat("PlaneReloadDur");
        if (PlayerPrefs.HasKey("VehicleIndex")) p.vehicleIndex = PlayerPrefs.GetInt("VehicleIndex");
        if (PlayerPrefs.HasKey("CarIndex")) p.carIndex = PlayerPrefs.GetInt("CarIndex");
        if (PlayerPrefs.HasKey("ColorIndex")) p.colorIndex = PlayerPrefs.GetInt("ColorIndex");
        if (PlayerPrefs.HasKey("CarDriveDur")) p.carDriveDur = PlayerPrefs.GetFloat("CarDriveDur");
        if (PlayerPrefs.HasKey("JetDriveDur")) p.jetDriveDur = PlayerPrefs.GetFloat("JetDriveDur");
        if (PlayerPrefs.HasKey("TankDriveDur")) p.tankDriveDur = PlayerPrefs.GetFloat("TankDriveDur");
        if (PlayerPrefs.HasKey("PlaneFlyDur")) p.planeFlyDur = PlayerPrefs.GetFloat("PlaneFlyDur");
        if (PlayerPrefs.HasKey("TotalCoinAmount")) p.totalCoinAmount = PlayerPrefs.GetInt("TotalCoinAmount");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    [ContextMenu("Delete All Game Saves")]
    public void DeleteAllGameData()
    {
        Debug.Log("🧹 Deleting all saved data...");

        // Runtime reset first so UI updates right away
        if (GestureManager.instance != null)
            GestureManager.instance.DeleteAllGestureSavesAndResetRuntime();
        else
        {
            // Fallback if manager not in scene
            var gestures = GameObject.FindObjectsOfType<Gesture>(includeInactive: true);
            foreach (var g in gestures) { g.isOwned = false; g.isPlaying = false; }
        }

        // --- Player-related keys ---
        string[] playerKeys =
        {
        "MoveSpeed","TankReloadDur","PlaneReloadDur",
        "VehicleIndex","CarIndex","ColorIndex",
        "CarDriveDur","JetDriveDur","TankDriveDur","PlaneFlyDur",
        "TotalCoinAmount","IsControlShown"
    };

        // --- Upgrade-related keys ---
        string[] upgradeKeys =
        {
        "CarDriveDur","JetDriveDur","TankDriveDur","TankReloadDur",
        "PlaneFlyDur","PlaneReloadDur",
        "Price_CarDur","Price_JetDur","Price_TankDur","Price_TankReloadDur",
        "Price_PlaneDur","Price_PlaneReloadDur"
    };

        // --- Garage (vehicles) keys ---
        for (int i = 0; i < 50; i++)
        {
            PlayerPrefs.DeleteKey($"veh_{i}_unlocked");
            PlayerPrefs.DeleteKey($"veh_{i}_equipped");
            PlayerPrefs.DeleteKey($"veh_{i}_carIndex");
            PlayerPrefs.DeleteKey($"veh_{i}_upgradePrice");
            PlayerPrefs.DeleteKey($"veh_{i}_colorIndex");
        }
        PlayerPrefs.DeleteKey("garage_last_index");

        // --- Gesture keys ---
        for (int i = 0; i < 50; i++)
            PlayerPrefs.DeleteKey($"gesture_{i}_owned");
        PlayerPrefs.DeleteKey("gesture_last_selected");

        // --- Delete other known keys explicitly ---
        foreach (var key in playerKeys) PlayerPrefs.DeleteKey(key);
        foreach (var key in upgradeKeys) PlayerPrefs.DeleteKey(key);

        // --- Final full fallback wipe ---
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("✅ All saved data wiped. All gestures are now locked.");
    }



}

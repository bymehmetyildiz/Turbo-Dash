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
      
        GameData.SetFloat("TankReloadDur", p.tankReloadDur);
        GameData.SetFloat("PlaneReloadDur", p.planeReloadDur);
        GameData.SetInt("VehicleIndex", p.vehicleIndex);
        GameData.SetInt("CarIndex", p.carIndex);
        GameData.SetInt("ColorIndex", p.colorIndex);
        GameData.SetFloat("CarDriveDur", p.carDriveDur);
        GameData.SetFloat("JetDriveDur", p.jetDriveDur);
        GameData.SetFloat("TankDriveDur", p.tankDriveDur);
        GameData.SetFloat("PlaneFlyDur", p.planeFlyDur);
        GameData.SetInt("TotalCoinAmount", p.totalCoinAmount);
        GameData.SetInt("IsControlShown", UIManager.instance != null && UIManager.instance.isControlsShown ? 1 : 0);

        GameData.Save();
    }

    public void LoadGame()
    {
        var p = Player.instance ?? FindObjectOfType<Player>();
        if (p == null)
        {
            Debug.LogWarning("LoadGame: Player not found yet. Try calling again later.");
            return;
        }
    
        if (GameData.HasKey("TankReloadDur")) p.tankReloadDur = GameData.GetFloat("TankReloadDur");
        if (GameData.HasKey("PlaneReloadDur")) p.planeReloadDur = GameData.GetFloat("PlaneReloadDur");
        if (GameData.HasKey("VehicleIndex")) p.vehicleIndex = GameData.GetInt("VehicleIndex");
        if (GameData.HasKey("CarIndex")) p.carIndex = GameData.GetInt("CarIndex");
        if (GameData.HasKey("ColorIndex")) p.colorIndex = GameData.GetInt("ColorIndex");
        if (GameData.HasKey("CarDriveDur")) p.carDriveDur = GameData.GetFloat("CarDriveDur");
        if (GameData.HasKey("JetDriveDur")) p.jetDriveDur = GameData.GetFloat("JetDriveDur");
        if (GameData.HasKey("TankDriveDur")) p.tankDriveDur = GameData.GetFloat("TankDriveDur");
        if (GameData.HasKey("PlaneFlyDur")) p.planeFlyDur = GameData.GetFloat("PlaneFlyDur");
        if (GameData.HasKey("TotalCoinAmount")) p.totalCoinAmount = GameData.GetInt("TotalCoinAmount");
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
            GameData.DeleteKey($"veh_{i}_unlocked");
            GameData.DeleteKey($"veh_{i}_equipped");
            GameData.DeleteKey($"veh_{i}_carIndex");
            GameData.DeleteKey($"veh_{i}_upgradePrice");
            GameData.DeleteKey($"veh_{i}_colorIndex");
        }
        GameData.DeleteKey("garage_last_index");

        // --- Gesture keys ---
        for (int i = 0; i < 50; i++)
            GameData.DeleteKey($"gesture_{i}_owned");
        GameData.DeleteKey("gesture_last_selected");

        // --- Delete other known keys explicitly ---
        foreach (var key in playerKeys) GameData.DeleteKey(key);
        foreach (var key in upgradeKeys) GameData.DeleteKey(key);

        // --- Final full fallback wipe ---
        GameData.DeleteAll();
        GameData.Save();

        Debug.Log("✅ All saved data wiped. All gestures are now locked.");
    }



}

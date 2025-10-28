using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageController : MonoBehaviour
{
    [SerializeField] private GameObject[] vehicle;
    public GameObject activeVehicle;

    [Header("UI")]
    [SerializeField] private GameObject vehicleLock;
    [SerializeField] private GameObject vehicleProps;
    [SerializeField] private TMP_Text vehicleName;
    [SerializeField] private TMP_Text vehiclePrice;
    [SerializeField] private GameObject lockedText;
    [SerializeField] private GameObject lockedImage;
    [SerializeField] private GameObject coinImage;

    [SerializeField] private TMP_Text upgradePriceText;
    [SerializeField] private Button upgradeButton;

    [SerializeField] private DropdownController dropdownController;

    // current selection
    private int index;

    // ---------- PlayerPrefs key helpers ----------
    private const string KEY_LAST_INDEX = "garage_last_index";
    private static string K_Unlocked(int i) => $"veh_{i}_unlocked";
    private static string K_Equipped(int i) => $"veh_{i}_equipped";
    private static string K_CarIndex(int i) => $"veh_{i}_carIndex";
    private static string K_UpgradePrice(int i) => $"veh_{i}_upgradePrice";
    private static string K_ColorIndex(int i) => $"veh_{i}_colorIndex";

    void Start()
    {
        // Deactivate all
        for (int i = 0; i < vehicle.Length; i++)
            vehicle[i].SetActive(false);

        // Load all vehicle states from prefs (includes default unlock for 0)
        LoadAllVehiclesFromPrefs();

        // Restore last viewed index (clamped)
        index = Mathf.Clamp(PlayerPrefs.GetInt(KEY_LAST_INDEX, 0), 0, vehicle.Length - 1);

        vehicle[index].SetActive(true);
        activeVehicle = vehicle[index];
        vehicleName.text = activeVehicle.GetComponent<Vehicle>().vehicleName;

        // Apply UI for the chosen vehicle
        ShowVehicleProps();

        // Sync dropdown to saved color
        dropdownController.SetupColorOptions();
        dropdownController.dropdown.value = activeVehicle.GetComponent<Vehicle>().colorIndex;
        dropdownController.dropdown.RefreshShownValue();
    }

    // ---------- LOAD / SAVE ----------
    private void LoadAllVehiclesFromPrefs()
    {
        for (int i = 0; i < vehicle.Length; i++)
        {
            var v = vehicle[i].GetComponent<Vehicle>();

            // Default unlock for vehicle 0
            bool defaultUnlocked = (i == 0);
            v.isUnlocked = PlayerPrefs.GetInt(K_Unlocked(i), defaultUnlocked ? 1 : 0) == 1;

            // If nothing equipped yet, auto-equip vehicle 0 on first boot
            int defaultEquipped = (i == 0 && PlayerPrefs.HasKey(K_Equipped(0)) == false) ? 1 : 0;
            v.isEquipped = PlayerPrefs.GetInt(K_Equipped(i), defaultEquipped) == 1;

            // Upgrade level and price
            v.carIndex = PlayerPrefs.GetInt(K_CarIndex(i), v.carIndex);
            v.upgradePrice = PlayerPrefs.GetInt(K_UpgradePrice(i), v.upgradePrice);

            // Saved color
            v.colorIndex = PlayerPrefs.GetInt(K_ColorIndex(i), v.colorIndex);

            // Apply visuals to match saved upgrade + color
            v.SetupCar(v.carIndex, v.colorIndex);
        }

        // Ensure exactly one vehicle is equipped.
        EnsureSingleEquipped();
    }

    private void SaveVehicleToPrefs(int i)
    {
        var v = vehicle[i].GetComponent<Vehicle>();
        PlayerPrefs.SetInt(K_Unlocked(i), v.isUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(K_Equipped(i), v.isEquipped ? 1 : 0);
        PlayerPrefs.SetInt(K_CarIndex(i), v.carIndex);
        PlayerPrefs.SetInt(K_UpgradePrice(i), v.upgradePrice);
        PlayerPrefs.SetInt(K_ColorIndex(i), v.colorIndex);
        PlayerPrefs.Save();
    }

    private void SaveAllToPrefs()
    {
        for (int i = 0; i < vehicle.Length; i++)
            SaveVehicleToPrefs(i);

        PlayerPrefs.SetInt(KEY_LAST_INDEX, index);
        PlayerPrefs.Save();
    }

    private void EnsureSingleEquipped()
    {
        int firstEquipped = -1;
        for (int i = 0; i < vehicle.Length; i++)
        {
            var v = vehicle[i].GetComponent<Vehicle>();
            if (v.isEquipped)
            {
                if (firstEquipped == -1) firstEquipped = i;
                else v.isEquipped = false; // keep the first and clear others
            }
        }
        // If none equipped, equip 0 by default
        if (firstEquipped == -1 && vehicle.Length > 0)
        {
            vehicle[0].GetComponent<Vehicle>().isEquipped = true;
        }
    }

    // ---------- UI / FLOW ----------
    private void ShowVehicleProps()
    {
        var v = activeVehicle.GetComponent<Vehicle>();
        vehicleName.text = v.vehicleName;

        if (v.type == VehicleType.Modified) vehicleProps.SetActive(true);
        else vehicleProps.SetActive(false);

        if (!v.isUnlocked)
        {
            vehiclePrice.text = NumberFormatter.FormatNumber(v.price);
            lockedText.SetActive(true);
            vehicleProps.SetActive(false);
            lockedImage.SetActive(true);
            coinImage.SetActive(true);
            upgradePriceText.text = v.upgradePrice.ToString();
            upgradeButton.interactable = false;
        }
        else
        {
            coinImage.SetActive(false);
            lockedText.SetActive(false);
            lockedImage.SetActive(false);
            upgradeButton.interactable = true;

            if (v.type == VehicleType.Modified) vehicleProps.SetActive(true);
            else vehicleProps.SetActive(false);

            vehiclePrice.text = v.isEquipped ? "Equiped" : "Equip";
            upgradePriceText.text = v.carIndex >= 3 ? "Max" : v.upgradePrice.ToString();
            if (v.carIndex >= 3) upgradeButton.interactable = false;
        }
    }

    public void SelectVehicle(bool right)
    {
        // Hide all
        for (int i = 0; i < vehicle.Length; i++)
            vehicle[i].SetActive(false);

        // Move index
        if (right)
        {
            index++;
            if (index >= vehicle.Length) index = vehicle.Length - 1;
        }
        else
        {
            index--;
            if (index < 0) index = 0;
        }

        // Activate new
        vehicle[index].SetActive(true);
        activeVehicle = vehicle[index];

        // UI + dropdown
        ShowVehicleProps();
        dropdownController.SetupColorOptions();

        var v = activeVehicle.GetComponent<Vehicle>();
        vehicleName.text = v.vehicleName;
        dropdownController.dropdown.value = v.colorIndex;
        dropdownController.dropdown.RefreshShownValue();

        // Save current selection
        PlayerPrefs.SetInt(KEY_LAST_INDEX, index);
        PlayerPrefs.Save();
    }

    public void SelectColor()
    {
        var v = activeVehicle.GetComponent<Vehicle>();

        // Apply visually
        int newColor = dropdownController.dropdown.value;
        v.SetupCar(v.carIndex, newColor);

        // Persist on vehicle + prefs
        v.colorIndex = newColor;
        SaveVehicleToPrefs(v.vehicleIndex);

        // Also mirror to player's selection for runtime use if you want
        Player.instance.vehicleIndex = v.vehicleIndex;
        Player.instance.carIndex = v.carIndex;
        Player.instance.colorIndex = newColor;
        SaveManager.instance?.SaveGame();
    }

    public void Upgrade()
    {
        var v = activeVehicle.GetComponent<Vehicle>();

        if (v.carIndex >= 3)
        {
            upgradePriceText.text = "Max";
            upgradeButton.interactable = false;
            return;
        }

        if (Player.instance.totalCoinAmount >= v.upgradePrice)
        {
            Player.instance.totalCoinAmount -= v.upgradePrice;
            UIManager.instance.UpdateTotalCoin();

            v.carIndex++;
            v.upgradePrice *= 3; // your original logic

            // Apply visuals
            v.SetupCar(v.carIndex, v.colorIndex);

            // Update UI text & button
            upgradePriceText.text = v.carIndex >= 3 ? "Max" : v.upgradePrice.ToString();
            if (v.carIndex >= 3) upgradeButton.interactable = false;

            // Persist
            SaveVehicleToPrefs(v.vehicleIndex);
            SaveManager.instance?.SaveGame();
        }
    }

    public void PurchaseVehicle()
    {
        var v = activeVehicle.GetComponent<Vehicle>();

        if (!v.isUnlocked)
        {
            if (Player.instance.totalCoinAmount >= v.price)
            {
                // Unlock
                v.isUnlocked = true;

                // Equip it (and unequip others)
                for (int i = 0; i < vehicle.Length; i++)
                    vehicle[i].GetComponent<Vehicle>().isEquipped = false;

                v.isEquipped = true;

                // Coins
                Player.instance.totalCoinAmount -= v.price;
                UIManager.instance.UpdateTotalCoin();

                // UI refresh
                vehiclePrice.text = "Equiped";
                coinImage.SetActive(false);
                lockedText.SetActive(false);
                lockedImage.SetActive(false);
                vehicleProps.SetActive(v.type == VehicleType.Modified);

                // Mirror to player (optional)
                Player.instance.vehicleIndex = v.vehicleIndex;

                // Persist
                for (int i = 0; i < vehicle.Length; i++) SaveVehicleToPrefs(i);
                SaveManager.instance?.SaveGame();
            }
        }
        else
        {
            // Toggle equip state
            if (v.isEquipped)
            {
                v.isEquipped = false;
                vehiclePrice.text = "Equip";
            }
            else
            {
                for (int i = 0; i < vehicle.Length; i++)
                {
                    vehicle[i].GetComponent<Vehicle>().isEquipped = false;
                }
                v.isEquipped = true;
                vehiclePrice.text = "Equiped";

                // Mirror to player (optional)
                Player.instance.vehicleIndex = v.vehicleIndex;
            }

            // Persist
            for (int i = 0; i < vehicle.Length; i++) SaveVehicleToPrefs(i);
            PlayerPrefs.SetInt(KEY_LAST_INDEX, index);
            PlayerPrefs.Save();
            SaveManager.instance?.SaveGame();
        }
    }
}

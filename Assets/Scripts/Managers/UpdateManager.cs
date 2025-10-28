using UnityEngine;
using TMPro;

public class UpdateManager : MonoBehaviour
{
    Player player;

    // === KEYS ===
    const string CarDurKey = "CarDriveDur";
    const string JetDurKey = "JetDriveDur";
    const string TankDurKey = "TankDriveDur";
    const string TankReloadKey = "TankReloadDur";
    const string PlaneDurKey = "PlaneFlyDur";
    const string PlaneReloadKey = "PlaneReloadDur";

    const string PriceCarKey = "Price_CarDur";
    const string PriceJetKey = "Price_JetDur";
    const string PriceTankKey = "Price_TankDur";
    const string PriceTankReloadKey = "Price_TankReloadDur";
    const string PricePlaneKey = "Price_PlaneDur";
    const string PricePlaneReloadKey = "Price_PlaneReloadDur";

    // --- your serialized UI fields & prices (defaults) ---
    [SerializeField] private TMP_Text carExpText;
    [SerializeField] private TMP_Text carPriceText;
    [SerializeField] private int currentCarPrice;
    private int nextCarPrice;

    [SerializeField] private TMP_Text jetExpText;
    [SerializeField] private TMP_Text jetPriceText;
    [SerializeField] private int currentJetPrice;
    private int nextJetPrice;

    [SerializeField] private TMP_Text tankExpText;
    [SerializeField] private TMP_Text tankPriceText;
    [SerializeField] private TMP_Text tankReloadExpText;
    [SerializeField] private TMP_Text tankReloadPriceText;
    [SerializeField] private int currentTankPrice;
    private int nextTankPrice;
    [SerializeField] private int currentTankReloadPrice;
    private int nextTankReloadPrice;

    [SerializeField] private TMP_Text planeExpText;
    [SerializeField] private TMP_Text planePriceText;
    [SerializeField] private TMP_Text planeReloadExpText;
    [SerializeField] private TMP_Text planeReloadPriceText;
    [SerializeField] private int currentPlanePrice;
    private int nextPlanePrice;
    [SerializeField] private int currentPlaneReloadPrice;
    private int nextPlaneReloadPrice;

  
    void Start()
    {
        player = Player.instance;

        // 1) Load saved upgrade values & prices (or fall back to current defaults)
        LoadUpgradeState();

        // 2) Recompute "next" prices every boot
        RecomputeNextPrices();

        // 3) Refresh texts
        RefreshAllTexts();
    }

    // ---------- SAVE / LOAD ----------
    private void LoadUpgradeState()
    {
        // Durations / reloads (fallback = whatever is already on the player)
        player.carDriveDur = PlayerPrefs.GetFloat(CarDurKey, player.carDriveDur);
        player.jetDriveDur = PlayerPrefs.GetFloat(JetDurKey, player.jetDriveDur);
        player.tankDriveDur = PlayerPrefs.GetFloat(TankDurKey, player.tankDriveDur);
        player.tankReloadDur = PlayerPrefs.GetFloat(TankReloadKey, player.tankReloadDur);
        player.planeFlyDur = PlayerPrefs.GetFloat(PlaneDurKey, player.planeFlyDur);
        player.planeReloadDur = PlayerPrefs.GetFloat(PlaneReloadKey, player.planeReloadDur);

        // Prices (fallback = the serialized defaults set in the Inspector)
        currentCarPrice = PlayerPrefs.GetInt(PriceCarKey, currentCarPrice);
        currentJetPrice = PlayerPrefs.GetInt(PriceJetKey, currentJetPrice);
        currentTankPrice = PlayerPrefs.GetInt(PriceTankKey, currentTankPrice);
        currentTankReloadPrice = PlayerPrefs.GetInt(PriceTankReloadKey, currentTankReloadPrice);
        currentPlanePrice = PlayerPrefs.GetInt(PricePlaneKey, currentPlanePrice);
        currentPlaneReloadPrice = PlayerPrefs.GetInt(PricePlaneReloadKey, currentPlaneReloadPrice);
    }

    private void SaveUpgradeState()
    {
        // Durations / reloads
        PlayerPrefs.SetFloat(CarDurKey, player.carDriveDur);
        PlayerPrefs.SetFloat(JetDurKey, player.jetDriveDur);
        PlayerPrefs.SetFloat(TankDurKey, player.tankDriveDur);
        PlayerPrefs.SetFloat(TankReloadKey, player.tankReloadDur);
        PlayerPrefs.SetFloat(PlaneDurKey, player.planeFlyDur);
        PlayerPrefs.SetFloat(PlaneReloadKey, player.planeReloadDur);

        // Current prices
        PlayerPrefs.SetInt(PriceCarKey, currentCarPrice);
        PlayerPrefs.SetInt(PriceJetKey, currentJetPrice);
        PlayerPrefs.SetInt(PriceTankKey, currentTankPrice);
        PlayerPrefs.SetInt(PriceTankReloadKey, currentTankReloadPrice);
        PlayerPrefs.SetInt(PricePlaneKey, currentPlanePrice);
        PlayerPrefs.SetInt(PricePlaneReloadKey, currentPlaneReloadPrice);

        PlayerPrefs.Save();
    }

    private void RecomputeNextPrices()
    {
        nextCarPrice = Mathf.RoundToInt(currentCarPrice * 1.5f);
        nextJetPrice = Mathf.RoundToInt(currentJetPrice * 1.5f);
        nextTankPrice = Mathf.RoundToInt(currentTankPrice * 1.5f);
        nextTankReloadPrice = Mathf.RoundToInt(currentTankReloadPrice * 1.5f);
        nextPlanePrice = Mathf.RoundToInt(currentPlanePrice * 1.5f);
        nextPlaneReloadPrice = Mathf.RoundToInt(currentPlaneReloadPrice * 1.5f);
    }

    private void RefreshAllTexts()
    {
        carExpText.text = $"{player.carDriveDur} sec -> {player.carDriveDur + 2f}";
        carPriceText.text = currentCarPrice.ToString();

        jetExpText.text = $"{player.jetDriveDur} sec -> {player.jetDriveDur + 2f}";
        jetPriceText.text = currentJetPrice.ToString();

        tankExpText.text = $"{player.tankDriveDur} sec -> {player.tankDriveDur + 2f}";
        tankPriceText.text = currentTankPrice.ToString();

        tankReloadExpText.text = $"{player.tankReloadDur} sec -> {player.tankReloadDur - 0.25f}";
        tankReloadPriceText.text = currentTankReloadPrice.ToString();

        planeExpText.text = $"{player.planeFlyDur} sec -> {player.planeFlyDur + 2f}";
        planePriceText.text = currentPlanePrice.ToString();

        planeReloadExpText.text = $"{player.planeReloadDur} sec -> {player.planeReloadDur - 0.25f}";
        planeReloadPriceText.text = currentPlaneReloadPrice.ToString();
    }

    // ---------- UPGRADE BUTTONS ----------
    public void UpdateCarDur()
    {
        if (player.carDriveDur >= 20f) { carExpText.text = "MAX"; carPriceText.text = "MAX"; return; }
        if (player.totalCoinAmount >= currentCarPrice)
        {
            player.totalCoinAmount -= currentCarPrice;
            player.carDriveDur += 2f;
            currentCarPrice = nextCarPrice;
            RecomputeNextPrices();
            RefreshAllTexts();

            // Persist both upgrades & coins
            SaveUpgradeState();
            SaveManager.instance?.SaveGame();
            UIManager.instance.UpdateTotalCoin();
        }
    }

    public void UpdateJetDur()
    {
        if (player.jetDriveDur >= 20f) { jetExpText.text = "MAX"; jetPriceText.text = "MAX"; return; }
        if (player.totalCoinAmount >= currentJetPrice)
        {
            player.totalCoinAmount -= currentJetPrice;
            player.jetDriveDur += 2f;
            currentJetPrice = nextJetPrice;
            RecomputeNextPrices();
            RefreshAllTexts();

            SaveUpgradeState();
            SaveManager.instance?.SaveGame();
            UIManager.instance.UpdateTotalCoin();
        }
    }

    public void UpdateTankDur()
    {
        if (player.tankDriveDur >= 20f) { tankExpText.text = "MAX"; tankPriceText.text = "MAX"; return; }
        if (player.totalCoinAmount >= currentTankPrice)
        {
            player.totalCoinAmount -= currentTankPrice;
            player.tankDriveDur += 2f;
            currentTankPrice = nextTankPrice;
            RecomputeNextPrices();
            RefreshAllTexts();

            SaveUpgradeState();
            SaveManager.instance?.SaveGame();
            UIManager.instance.UpdateTotalCoin();
        }
    }

    public void UpdateTankReloadDur()
    {
        if (player.tankReloadDur <= 0.5f) { tankReloadExpText.text = "MAX"; tankReloadPriceText.text = "MAX"; return; }
        if (player.totalCoinAmount >= currentTankReloadPrice)
        {
            player.totalCoinAmount -= currentTankReloadPrice;
            player.tankReloadDur -= 0.25f;
            currentTankReloadPrice = nextTankReloadPrice;
            RecomputeNextPrices();
            RefreshAllTexts();

            SaveUpgradeState();
            SaveManager.instance?.SaveGame();
            UIManager.instance.UpdateTotalCoin();
        }
    }

    public void UpdatePlaneDur()
    {
        if (player.planeFlyDur >= 20f) { planeExpText.text = "MAX"; planePriceText.text = "MAX"; return; }
        if (player.totalCoinAmount >= currentPlanePrice)
        {
            player.totalCoinAmount -= currentPlanePrice;
            player.planeFlyDur += 2f;
            currentPlanePrice = nextPlanePrice;
            RecomputeNextPrices();
            RefreshAllTexts();

            SaveUpgradeState();
            SaveManager.instance?.SaveGame();
            UIManager.instance.UpdateTotalCoin();
        }
    }

    public void UpdatePlaneReloadDur()
    {
        if (player.planeReloadDur <= 0.5f) { planeReloadExpText.text = "MAX"; planeReloadPriceText.text = "MAX"; return; }
        if (player.totalCoinAmount >= currentPlaneReloadPrice)
        {
            player.totalCoinAmount -= currentPlaneReloadPrice;
            player.planeReloadDur -= 0.25f;
            currentPlaneReloadPrice = nextPlaneReloadPrice;
            RecomputeNextPrices();
            RefreshAllTexts();

            SaveUpgradeState();
            SaveManager.instance?.SaveGame();
            UIManager.instance.UpdateTotalCoin();
        }
    }

    // Handy for testing in Editor
    [ContextMenu("Reset Upgrades (Keep Coins)")]
    private void ResetUpgrades()
    {
        PlayerPrefs.DeleteKey(CarDurKey);
        PlayerPrefs.DeleteKey(JetDurKey);
        PlayerPrefs.DeleteKey(TankDurKey);
        PlayerPrefs.DeleteKey(TankReloadKey);
        PlayerPrefs.DeleteKey(PlaneDurKey);
        PlayerPrefs.DeleteKey(PlaneReloadKey);

        PlayerPrefs.DeleteKey(PriceCarKey);
        PlayerPrefs.DeleteKey(PriceJetKey);
        PlayerPrefs.DeleteKey(PriceTankKey);
        PlayerPrefs.DeleteKey(PriceTankReloadKey);
        PlayerPrefs.DeleteKey(PricePlaneKey);
        PlayerPrefs.DeleteKey(PricePlaneReloadKey);

        PlayerPrefs.Save();
        LoadUpgradeState();
        RecomputeNextPrices();
        RefreshAllTexts();
    }
}

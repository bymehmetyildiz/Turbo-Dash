using UnityEngine;

// Coin-only economy. Prices follow owned levels, never stale saved price values.
public static class EconomyBalance
{
    public static int VehiclePrice(string name, int fallback)
    {
        switch (name)
        {
            case "Sedan": return 0;
            case "Minivan": return 1800;
            case "Sport": return 3500;
            case "Cross": return 5500;
            case "Suv": return 8000;
            case "Pickup": return 11000;
            case "Vapor": return 14500;
            case "Lawnmover": return 18000;
            case "Truck": return 22000;
            default: return fallback;
        }
    }

    public static int GesturePrice(int id, int fallback)
    {
        switch (id)
        {
            case 3: return 600;
            case 8: return 1000;
            case 0: return 1600;
            case 1: return 2400;
            case 2: return 3400;
            case 4: return 4600;
            case 5: return 6000;
            case 6: return 7800;
            case 7: return 10000;
            default: return fallback;
        }
    }

    public static int UpgradePrice(int initial, int level, float growth)
    {
        return Mathf.Max(50, Mathf.RoundToInt(initial * Mathf.Pow(growth, Mathf.Clamp(level, 0, 20)) / 50f) * 50);
    }
    public static int VehicleUpgrade(int level) => UpgradePrice(1800, level, 1.75f);
    public static int DurationPrice(int initial, float duration) => UpgradePrice(initial, Mathf.RoundToInt((duration - 10f) / 2f), 1.35f);
    public static int ReloadPrice(int initial, float duration) => UpgradePrice(initial, Mathf.RoundToInt((3f - duration) / 0.25f), 1.18f);
}

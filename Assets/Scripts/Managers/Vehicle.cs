using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public VehicleType type;
    public CarCollection cars;
    public GameObject activeCar;
    public int vehicleIndex;
    public int carIndex;
    public int colorIndex; 
    public bool isUnlocked;
    public bool isEquipped;
    public string vehicleName;
    public int price;
    public int upgradePrice;

    public string[] colors;
    public Color[] colorValues;



    void Start()
    {
        SetupCar(carIndex, colorIndex);
    }

    
    void Update()
    {
       
    }

    public void SetupCar(int _carIndex, int _colorIndex)
    {
        if (type == VehicleType.Modified)
        {
            for (int i = 0; i < cars.colors.Length; i++)
            {
                for (int j = 0; j < cars.colors[i].upgradeLevels.Length; j++)
                {
                    cars.colors[i].upgradeLevels[j].SetActive(false);
                }
            }

            activeCar = null;

            // Activate chosen car
            activeCar = cars.colors[_colorIndex].upgradeLevels[_carIndex];
            activeCar.SetActive(true);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class GarageController : MonoBehaviour
{  
    [SerializeField] private GameObject[] vehicle;
    [SerializeField] private GameObject activeVehicle;
    [SerializeField] private GameObject vehicleLock;
    [SerializeField] private GameObject vehicleProps;
    [SerializeField] private TMP_Text vehicleName;
    [SerializeField] private TMP_Text vehiclePrice;
    [SerializeField] private GameObject lockedText;
    [SerializeField] private GameObject lockedImage;
    [SerializeField] private GameObject coinImage;
    private int index;
  
    [SerializeField] private TMP_Text upgradePriceText;
    [SerializeField] private Button upgradeButton;

    [SerializeField] private DropdownController dropdownController;


    void Start()
    {
        for (int i = 0; i < vehicle.Length; i++)
        {
            vehicle[i].SetActive(false);
        }
        vehicle[index].SetActive(true);
        activeVehicle = vehicle[index];
        vehicleName.text = activeVehicle.GetComponent<Vehicle>().vehicleName;
        ShowVehicleProps();
    }

    private void ShowVehicleProps()
    {
        if (activeVehicle.GetComponent<Vehicle>().type == VehicleType.Modified)
            vehicleProps.SetActive(true);
        else
            vehicleProps.SetActive(false);

        if (!activeVehicle.GetComponent<Vehicle>().isUnlocked)
        {
            vehiclePrice.text = NumberFormatter.FormatNumber(activeVehicle.GetComponent<Vehicle>().price);
            lockedText.SetActive(true);
            vehicleProps.SetActive(false);
            lockedImage.SetActive(true);
            upgradePriceText.text = activeVehicle.GetComponent<Vehicle>().upgradePrice.ToString();
        }
        else
        {
            vehiclePrice.text = "Equip";
            coinImage.SetActive(false);
            lockedText.SetActive(false);
            vehicleProps.SetActive(true);
            lockedImage.SetActive(false);
        }
    }

    public void SelectVehicle(bool right)
    {
        for (int i = 0; i < vehicle.Length; i++)
        {
            vehicle[i].SetActive(false);
        }

        if (right)
        {
            index++;
            if (index >= vehicle.Length-1)
                index = vehicle.Length-1;
        }
        else
        {
            index--;
            if (index < 0)
                index = 0;
        }

        vehicle[index].SetActive(true);
        activeVehicle = vehicle[index];
        ShowVehicleProps();
        vehicleName.text = activeVehicle.GetComponent<Vehicle>().vehicleName;

        if (!activeVehicle.GetComponent<Vehicle>().isUnlocked)
        {
            vehiclePrice.text = NumberFormatter.FormatNumber(activeVehicle.GetComponent<Vehicle>().price);
            lockedText.SetActive(true);
            vehicleProps.SetActive(false);
            coinImage.SetActive(true);
            lockedImage.SetActive(true);
           
        }
        else
        {            
            coinImage.SetActive(false);
            lockedText.SetActive(false);
            lockedImage.SetActive(false);

            if (activeVehicle.GetComponent<Vehicle>().type == VehicleType.Modified)
                vehicleProps.SetActive(true);
            else
                vehicleProps.SetActive(false);

            if (vehicle[index].GetComponent<Vehicle>().isEquipped)            
                vehiclePrice.text = "Equiped";                
            
            else
                vehiclePrice.text = "Equip";
        }
    }

    public void SelectColor()
    {
        Vehicle vehicle = activeVehicle.GetComponent<Vehicle>();
        vehicle.SetupCar(vehicle.carIndex, dropdownController.dropdown.value);
        Player.instance.vehicleIndex = activeVehicle.GetComponent<Vehicle>().vehicleIndex;
        Player.instance.carIndex = vehicle.carIndex;
        Player.instance.colorIndex = dropdownController.dropdown.value;
    }

    public void Upgrade()
    {
        if (activeVehicle.GetComponent<Vehicle>().carIndex >= 3)
        {
            upgradePriceText.text = "Max";
            upgradeButton.interactable = false;           
            return;
        }
        else if (Player.instance.totalCoinAmount >= activeVehicle.GetComponent<Vehicle>().upgradePrice 
            && activeVehicle.GetComponent<Vehicle>().carIndex <= 3)
        {            
            Player.instance.totalCoinAmount -= activeVehicle.GetComponent<Vehicle>().upgradePrice;
            activeVehicle.GetComponent<Vehicle>().carIndex++;
            activeVehicle.GetComponent<Vehicle>().upgradePrice *= Mathf.RoundToInt(3);
            upgradePriceText.text = activeVehicle.GetComponent<Vehicle>().upgradePrice.ToString();
            activeVehicle.GetComponent<Vehicle>().SetupCar(activeVehicle.GetComponent<Vehicle>().carIndex, dropdownController.dropdown.value);
            Player.instance.carIndex = activeVehicle.GetComponent<Vehicle>().carIndex;
            Player.instance.vehicleIndex = activeVehicle.GetComponent<Vehicle>().vehicleIndex;
        }
        
    }


    public void PurchaseVehicle()
    {
        if (activeVehicle.GetComponent<Vehicle>().isUnlocked == false)
        {
            if (Player.instance.totalCoinAmount >= activeVehicle.GetComponent<Vehicle>().price)
            {
                activeVehicle.GetComponent<Vehicle>().isUnlocked = true;

                for (int i = 0; i < vehicle.Length; i++)
                {
                    vehicle[i].GetComponent<Vehicle>().isEquipped = false;                    
                }

                activeVehicle.GetComponent<Vehicle>().isEquipped = true;
                vehiclePrice.text = "Equiped";
                coinImage.SetActive(false);
                lockedText.SetActive(false);
                lockedImage.SetActive(false);

                if (activeVehicle.GetComponent<Vehicle>().type == VehicleType.Modified)
                    vehicleProps.SetActive(true);
                else
                    vehicleProps.SetActive(false);

                Player.instance.totalCoinAmount -= activeVehicle.GetComponent<Vehicle>().price;
                Player.instance.vehicleIndex = activeVehicle.GetComponent<Vehicle>().vehicleIndex;
            }
        }
        else
        {
            if (vehicle[index].GetComponent<Vehicle>().isEquipped)
            {
                vehiclePrice.text = "Equip";
                vehicle[index].GetComponent<Vehicle>().isEquipped = false;
            }
            else
            {
                for (int i = 0; i < vehicle.Length; i++)
                {
                    vehicle[i].GetComponent<Vehicle>().isEquipped = false;
                    vehiclePrice.text = "Equip";
                }
                vehicle[index].GetComponent<Vehicle>().isEquipped = true;
                vehiclePrice.text = "Equiped";
            }
        }
    }
}
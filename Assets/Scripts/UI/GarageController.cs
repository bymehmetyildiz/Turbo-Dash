using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GarageController : MonoBehaviour
{  
    [SerializeField] private GameObject[] vehicle;
    [SerializeField] private GameObject activeVehicle;
    [SerializeField] private GameObject vehicleLock;
    [SerializeField] private GameObject vehicleProps;
    [SerializeField] private TMP_Text vehicleName;
    [SerializeField] private TMP_Text vehiclePrice;
    [SerializeField] private GameObject coinImage;
    private int index;

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
            vehiclePrice.text = activeVehicle.GetComponent<Vehicle>().price.ToString();
        else
        {
            vehiclePrice.text = "Equip";
            coinImage.SetActive(false);
        }
    }

    void Update()
    {
        
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

            vehiclePrice.text = activeVehicle.GetComponent<Vehicle>().price.ToString();
        else
        {
            vehiclePrice.text = "Equip";
            coinImage.SetActive(false);
        }
    }

    public void SelectColor()
    {
        Vehicle vehicle = activeVehicle.GetComponent<Vehicle>();
        vehicle.SetupCar(vehicle.carIndex, dropdownController.dropdown.value);
    }

}
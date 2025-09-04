using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GarageController : MonoBehaviour
{  
    [SerializeField] private GameObject[] vehicle;
    [SerializeField] private GameObject activeVehicle;
    [SerializeField] private GameObject vehicleLock;
    [SerializeField] private GameObject vehicleProps;
    private int index;



    void Start()
    {
        for (int i = 0; i < vehicle.Length; i++)
        {
            vehicle[i].SetActive(false);
        }
        vehicle[index].SetActive(true);
        activeVehicle = vehicle[index];
        ShowVehicleProps();

    }

    private void ShowVehicleProps()
    {
        if (activeVehicle.GetComponent<Vehicle>().type == VehicleType.Modified)
            vehicleProps.SetActive(true);
        else
            vehicleProps.SetActive(false);
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
    }

}

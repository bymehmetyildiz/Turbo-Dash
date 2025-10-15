using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdateManager : MonoBehaviour
{
    Player player = Player.instance;

    // Car
    [SerializeField] private TMP_Text carExpText;
    [SerializeField] private TMP_Text carPriceText;
    [SerializeField] private int currentCarPrice;
    private int nextCarPrice;

    //JetPack
    [SerializeField] private TMP_Text jetExpText;
    [SerializeField] private TMP_Text jetPriceText;
    [SerializeField] private int currentJetPrice;
    private int nextJetPrice;

    //Tank
    [SerializeField] private TMP_Text tankExpText;
    [SerializeField] private TMP_Text tankPriceText;
    [SerializeField] private TMP_Text tankReloadExpText;
    [SerializeField] private TMP_Text tankReloadPriceText;
    [SerializeField] private int currentTankPrice;
    private int nextTankPrice;
    [SerializeField] private int currentTankReloadPrice;
    private int nextTankReloadPrice;

    //Plane
    [SerializeField] private TMP_Text planeExpText;
    [SerializeField] private TMP_Text planePriceText;
    [SerializeField] private TMP_Text planeReloadExpText;
    [SerializeField] private TMP_Text planeReloadPriceText;
    [SerializeField] private int currentPlanePrice;
    private int nextPlanePrice;
    [SerializeField] private int currentPlaneReloadPrice;
    private int nextPlaneReloadPrice;


    private void Start()
    {
        carExpText.text = player.carDriveDur + " sec -> " + (player.carDriveDur + 2f);
        carPriceText.text = currentCarPrice.ToString();

        jetExpText.text = player.jetDriveDur + " sec -> " + (player.jetDriveDur + 2f);
        jetPriceText.text = currentJetPrice.ToString();

        tankExpText.text = player.tankDriveDur + " sec -> " + (player.tankDriveDur + 2f);
        tankPriceText.text = currentTankPrice.ToString();

        tankReloadExpText .text = player.tankReloadDur + " sec -> " + (player.tankReloadDur - 0.25f);
        tankReloadPriceText.text = currentTankReloadPrice.ToString();

        planeExpText.text = player.planeFlyDur + " sec -> " + (player.planeFlyDur + 2);
        planePriceText.text = currentPlanePrice.ToString();

        planeReloadExpText.text = player.planeReloadDur + " sec -> " + (player.planeReloadDur - 0.25f);
        planeReloadPriceText.text = currentPlaneReloadPrice.ToString();

        nextCarPrice = Mathf.RoundToInt(currentCarPrice * 1.5f);
        nextJetPrice = Mathf.RoundToInt(currentJetPrice * 1.5f);
        nextTankPrice = Mathf.RoundToInt(currentTankPrice * 1.5f);
        nextTankReloadPrice = Mathf.RoundToInt(currentTankReloadPrice * 1.5f);
        nextPlanePrice = Mathf.RoundToInt(currentPlanePrice * 1.5f);
        nextPlaneReloadPrice = Mathf.RoundToInt(currentPlaneReloadPrice * 1.5f);
    }

    public void UpdateCarDur()
    {
        if (player.carDriveDur >= 20f)
        {
            carExpText.text = "MAX";
            carPriceText.text = "MAX";
            return;
        }

        if (player.totalCoinAmount >= currentCarPrice)
        {            
            player.totalCoinAmount -= currentCarPrice;
            player.carDriveDur += 2f;
            currentCarPrice = nextCarPrice;
            nextCarPrice = Mathf.RoundToInt(currentCarPrice * 1.5f);
            carExpText.text = player.carDriveDur + " sec -> " + (player.carDriveDur + 2f);
            carPriceText.text = currentCarPrice.ToString();
        }
        UIManager.instance.UpdateTotalCoin();
    }

    public void UpdateJetDur()
    {
        if (player.jetDriveDur >= 20f)
        {
            jetExpText.text = "MAX";
            jetPriceText.text = "MAX";
            return;
        }

        if (player.totalCoinAmount >= currentJetPrice)
        {           
            player.totalCoinAmount -= currentJetPrice;
            player.jetDriveDur += 2f;
            currentJetPrice = nextJetPrice;
            nextJetPrice = Mathf.RoundToInt(currentJetPrice * 1.5f);
            jetExpText.text = player.jetDriveDur + " sec -> " + (player.jetDriveDur + 2f);
            jetPriceText.text = currentJetPrice.ToString();
        }
        UIManager.instance.UpdateTotalCoin();
    }

    public void UpdateTankDur()
    {
        if (player.tankDriveDur >= 20f)
        {
            tankExpText.text = "MAX";
            tankPriceText.text = "MAX";
            return;
        }

        if (player.totalCoinAmount >= currentTankPrice)
        {            
            player.totalCoinAmount -= currentTankPrice;
            player.tankDriveDur += 2f;
            currentTankPrice = nextTankPrice;
            nextTankPrice = Mathf.RoundToInt(currentTankPrice * 1.5f);
            tankExpText.text = player.tankDriveDur + " sec -> " + (player.tankDriveDur + 2f);
            tankPriceText.text = currentTankPrice.ToString();
        }
        UIManager.instance.UpdateTotalCoin();
    }

    public void UpdateTankReloadDur()
    {
        if (player.tankReloadDur <= 0.5f)
        {
            tankReloadExpText.text = "MAX";
            tankReloadPriceText.text = "MAX";
            return;
        }

        if (player.totalCoinAmount >= currentTankReloadPrice)
        {            
            player.totalCoinAmount -= currentTankReloadPrice;
            player.tankReloadDur -= 0.25f;
            currentTankReloadPrice = nextTankReloadPrice;
            nextTankReloadPrice = Mathf.RoundToInt(currentTankReloadPrice * 1.5f);
            tankReloadExpText.text = player.tankReloadDur + " sec -> " + (player.tankReloadDur - 0.25f);
            tankReloadPriceText.text = currentTankReloadPrice.ToString();
        }
        UIManager.instance.UpdateTotalCoin();
    }

    public void UpdatePlaneDur()
    {
        if (player.planeFlyDur >= 20f)
        {
            planeExpText.text = "MAX";
            planePriceText.text = "MAX";
            return;
        }

        if (player.totalCoinAmount >= currentPlanePrice)
        {            
            player.totalCoinAmount -= currentPlanePrice;
            player.planeFlyDur += 2f;
            currentPlanePrice = nextPlanePrice;
            nextPlanePrice = Mathf.RoundToInt(currentPlanePrice * 1.5f);
            planeExpText.text = player.planeFlyDur + " sec -> " + (player.planeFlyDur + 2f);
            planePriceText.text = currentPlanePrice.ToString();
        }
        UIManager.instance.UpdateTotalCoin();
    }

    public void UpdatePlaneReloadDur()
    {
        if (player.planeReloadDur <= 0.5f)
        {
            planeReloadExpText.text = "MAX";
            planeReloadPriceText.text = "MAX";            
            return;
        }
        
        if (player.totalCoinAmount >= currentPlaneReloadPrice)
        {
           
            player.totalCoinAmount -= currentPlaneReloadPrice;
            player.planeReloadDur -= 0.25f;
            currentPlaneReloadPrice = nextPlaneReloadPrice;
            nextPlaneReloadPrice = Mathf.RoundToInt(currentPlaneReloadPrice * 1.5f);
            planeReloadExpText.text = player.planeReloadDur + " sec -> " + (player.planeReloadDur - 0.25f);
            planeReloadPriceText.text = currentPlaneReloadPrice.ToString();            
        }
        UIManager.instance.UpdateTotalCoin();
    }

}

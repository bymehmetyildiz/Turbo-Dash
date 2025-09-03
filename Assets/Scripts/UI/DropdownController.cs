using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownController : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    public VehicleController vehicle;

    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.options.Clear();

        // Create colored options
        dropdown.options.Add(new TMP_Dropdown.OptionData("Black", CreateColorSprite(Color.black)));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Blue", CreateColorSprite(Color.blue)));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Cyan", CreateColorSprite(Color.cyan)));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Green", CreateColorSprite(Color.green)));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Magenta", CreateColorSprite(Color.magenta)));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Red", CreateColorSprite(Color.red)));
        dropdown.options.Add(new TMP_Dropdown.OptionData("Yellow", CreateColorSprite(Color.yellow)));

        dropdown.RefreshShownValue();

        ChangeColor();
    }

    // Helper: makes a small square sprite in given color
    private Sprite CreateColorSprite(Color color)
    {
        Texture2D tex = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
    }

    public void ChangeColor()
    {
        Player.instance.colorIndex = dropdown.value;
        vehicle.SetupCar(Player.instance.carIndex, Player.instance.colorIndex);

        // If a vehicle is currently active, refresh it
        if (Player.instance.activeVehicle != null)
        {
            // Destroy current car and spawn with new color
            Player.instance.DestroyCar();
            Player.instance.InstantiateCar();
        }
    }
    

}
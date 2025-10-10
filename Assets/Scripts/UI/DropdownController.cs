using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DropdownController : MonoBehaviour
{
    public TMP_Dropdown dropdown;
    [SerializeField] private GarageController garageController;
 
    void Start()
    {
        SetupColorOptions();
    }

    public void SetupColorOptions()
    {
        dropdown.options.Clear();
        for (int i = 0; i < garageController.activeVehicle.GetComponent<Vehicle>().colors.Length; i++)
        {
            dropdown.options.
                Add(new TMP_Dropdown.OptionData(garageController.activeVehicle.
                GetComponent<Vehicle>().colors[i], 
                CreateColorSprite(garageController.activeVehicle.
                GetComponent<Vehicle>().colorValues[i])));
        }
        dropdown.RefreshShownValue();
    }

    // Helper: makes a small square sprite in given color
    private Sprite CreateColorSprite(Color color)
    {
        color.a = 1f;
        Texture2D tex = new Texture2D(16, 16);
        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
    }
}
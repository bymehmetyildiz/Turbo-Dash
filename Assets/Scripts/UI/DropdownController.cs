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

    // Cache to avoid repeated GetComponent calls and texture creation
    private Vehicle activeVehicle;
    private List<Sprite> colorSprites;
 
    void Start()
    {
        if (garageController != null && garageController.activeVehicle != null)
            activeVehicle = garageController.activeVehicle.GetComponent<Vehicle>();

        SetupColorOptions();
    }

    public void SetupColorOptions()
    {
        if (garageController != null && garageController.activeVehicle != null && activeVehicle == null)
            activeVehicle = garageController.activeVehicle.GetComponent<Vehicle>();

        if (activeVehicle == null || dropdown == null) return;

        dropdown.options.Clear();

        if (colorSprites == null)
            CreateColorSprites();

        for (int i =0; i < activeVehicle.colors.Length; i++)
        {
            var label = activeVehicle.colors[i];
            Sprite icon = (i < colorSprites.Count) ? colorSprites[i] : null;
            dropdown.options.Add(new TMP_Dropdown.OptionData(label, icon));
        }

        dropdown.RefreshShownValue();
    }

    private void CreateColorSprites()
    {
        colorSprites = new List<Sprite>();
        if (activeVehicle == null) return;

        var values = activeVehicle.colorValues;
        for (int i =0; i < values.Length; i++)
        {
            colorSprites.Add(CreateColorSprite(values[i]));
        }
    }

    // Helper: makes a small square sprite in given color
    private Sprite CreateColorSprite(Color color)
    {
        color.a =1f;
        Texture2D tex = new Texture2D(16,16, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[16 *16];
        for (int i =0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0,0,16,16), new Vector2(0.5f,0.5f));
    }
}
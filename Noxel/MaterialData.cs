using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


public class MaterialData
{
    List<string> name;
    List<float> sideScale;
    List<float> wallScale;
    List<Color32> color;
    List<bool> uniform;

    public MaterialData()
    {
        name = new List<string>();
        sideScale = new List<float>();
        wallScale = new List<float>();
        color = new List<Color32>();
        uniform = new List<bool>();

        Add("Wood", .25f, .21f, new Color32(86, 46, 11, 255), false);
        Add("Plaster", .20f, .15f, new Color32(200, 200, 200, 255), false);
        Add("Stone", .35f, .30f, new Color32(75,75,75,255), true);
        Add("Thatch", .35f, .30f, new Color32(122, 109, 56, 255), false);
    }

    void Add(string newName, float newSideScale, float newWallScale, Color32 newColor, bool newUniform)
    {
        name.Add(newName);
        sideScale.Add(newSideScale);
        wallScale.Add(newWallScale);
        color.Add(newColor);
        uniform.Add(newUniform);
    }

    public Color32 GetColor(int id)
    {
        return color[id];
    }
    public float GetSideScale(int id)
    {
        return sideScale[id];
    }
    public float GetWallScale(int id)
    {
        return wallScale[id];
    }
    public string GetName(int id)
    {
        return name[id];
    }
    public int GetId(string findName)
    {
        for (int i = 0; i < name.Count; i++)
        {
            if (findName == name[i])
            {
                return i;
            }
        }
        return -1;
    }
    public int GetMax()
    {
        return name.Count;
    }
    public bool IsUniform(int id)
    {
        return uniform[id];
    }
}


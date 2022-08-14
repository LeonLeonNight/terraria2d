using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BiomeClass 
{
    public string biomeName;

    public Color biomeCol;

    public TileAtlas tileAtlas;

    [Header("Noise Settings")]
    public Texture2D caveNoiseTexture;

    [Header("Generation Settings")]
    public bool generatCaves = true;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 4f;
    public int dirtLayerHeight = 5;

    [Header("Tree confs")]
    public int treeChance = 15;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Addons")]
    public int tallGrassChance = 10;

    [Header("Ore Settings")]
    public OreClass[] ores;

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BiomeClass 
{
    public string biomeName;
    //public Color biomeColor;

    public TileAtlas biomeTiles;

    [Header("Noise Settings")]
    public float caveFreq = 0.04f;
    public float terrainFreq = 0.08f;
    public Texture2D caveNoiseTexture;

    [Header("Generation Settings")]
    public bool generatCaves = true;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 25;
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

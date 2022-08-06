using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    #region Params
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;
    public float seed = -5229;

    public BiomeClass[] biomes;

    [Header("Biomes")]
    public float biomeFrequency;
    public Gradient biomeGradient;
    public Texture2D biomeMap;

    [Header("Tree confs")]
    public int treeChance = 15;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Addons")]
    public int tallGrassChance = 10;

    [Header("Generation Settings")]
    public int chunckSize = 20;
    public bool generatCaves = true;
    public float surfaceValue = 0.25f;
    public float heightMultiplier = 25;
    public int dirtLayerHeight = 5;
    public int heightAddition = 25;
    public int worldSize = 100;

    [Header("Noise Settings")]
    public float caveFreq = 0.04f;
    public float terrainFreq = 0.08f;
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();
    private BiomeClass curBiome;
    #endregion
    private void OnValidate()
    {
        DrawTextures();
    }

    public void DrawTextures()
    {
        biomeMap = new Texture2D(worldSize, worldSize);
        DrawBiomeTexture();

        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);

            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
            }

            GenerationNoiseTexture(
                biomes[i].caveFreq,
                biomes[i].surfaceValue,
                biomes[i].caveNoiseTexture);
           
            //ores
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                GenerationNoiseTexture(
                    biomes[i].ores[o].rarity, 
                    biomes[i].ores[o].size, 
                    biomes[i].ores[o].spreadTexture);
            }
        }
    }

    public void DrawBiomeTexture()
    {
        for (int x = 0; x < biomeMap.width; x++)
        {
            for (int y = 0; y < biomeMap.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * biomeFrequency, (y + seed) * biomeFrequency);
                Color col = biomeGradient.Evaluate(v);
                biomeMap.SetPixel(x, y, col);
            }
        }

        biomeMap.Apply();
    }

    void Start()
    {
        seed = UnityEngine.Random.Range(-100, 100);

        DrawTextures();

        CreateChunks();
        GenerationTiles();
    }

    public void CreateChunks()
    {
        int numChunks = worldSize / chunckSize;
        worldChunks = new GameObject[numChunks];

        for (int i = 0; i < numChunks; i++)
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString();
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk;
        }
    }

    public BiomeClass GetCurrentBiome(int x, int y)
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            if (biomes[i].biomeCol == biomeMap.GetPixel(x, y))
            {
                return biomes[i];
            }
        }
        return curBiome;
    }

    public void GenerationTiles()
    {
        Sprite[] tileSprites;

        for (int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for (int y = 0; y < height; y++)
            {
                if (y < height - dirtLayerHeight)
                {
                    tileSprites = GetCurrentBiome(x, y).tileAtlas.stone.TileSprites;
                    
                    if (ores[0].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[0].maxSpawnHeight)
                        tileSprites = tileAtlas.coal.TileSprites;
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)
                        tileSprites = tileAtlas.iron.TileSprites;
                    if (ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)
                        tileSprites = tileAtlas.gold.TileSprites;
                    if (ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight)
                        tileSprites = tileAtlas.diamond.TileSprites;
                }
                else if (y < height - 1)
                {
                    tileSprites = tileAtlas.dirt.TileSprites;
                }
                else
                {
                    //top layer of the terrain
                    tileSprites = tileAtlas.grass.TileSprites;
                }

                if (generatCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileSprites, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprites, x, y);
                }

                if (y >= height - 1)
                {
                    int t = UnityEngine.Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        if (worldTiles.Contains(new Vector2(x, y)))
                        { GenerateTree(x, y + 1); }
                    }
                    else
                    {
                        int i = UnityEngine.Random.Range(0, tallGrassChance);
                        if (i == 1)
                        {
                            if (worldTiles.Contains(new Vector2(x, y)))
                            { PlaceTile(tileAtlas.tallGrass.TileSprites, x, y + 1); }
                        }
                    }
                }
            }
        }
    }

    public void GenerationNoiseTexture(float frequency, float limit, Texture2D noiseTexture)
    {
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                if (v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }

        noiseTexture.Apply();
    }

    public void GenerateTree(int x, int y)
    {
        int treeHeight = UnityEngine.Random.Range(minTreeHeight, maxTreeHeight);
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log.TileSprites, x, y + i);
        }

        PlaceTile(tileAtlas.leaf.TileSprites, x, y + treeHeight);

        PlaceTile(tileAtlas.leaf.TileSprites, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.TileSprites, x, y + treeHeight + 2);

        PlaceTile(tileAtlas.leaf.TileSprites, x - 1, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.TileSprites, x - 1, y + treeHeight);

        PlaceTile(tileAtlas.leaf.TileSprites, x + 1, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.TileSprites, x + 1, y + treeHeight);
    }

    public void PlaceTile(Sprite[] tileSprites, int x, int y)
    {
        if (!worldTiles.Contains(new Vector2Int(x, y)))
        {
            GameObject newTile = new GameObject();

            float chunckCoord = (Mathf.RoundToInt(x / chunckSize) * chunckSize);
            chunckCoord /= chunckSize;
            newTile.transform.parent = worldChunks[(int)chunckCoord].transform;

            newTile.AddComponent<SpriteRenderer>();
            int spriteIndex = UnityEngine.Random.Range(0, tileSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];
            newTile.name = tileSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            worldTiles.Add(newTile.transform.position - Vector3.one * 0.5f);
        }
    }
}

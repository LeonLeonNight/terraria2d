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

    [Header("Generation Settings")]
    public int chunckSize = 16;
    public bool generatCaves = true;
    public int heightAddition = 25;
    public int worldSize = 100;

    [Header("Noise Settings")]
    public Texture2D caveNoiseTexture;
    public float caveFreq = 0.05f;
    public float terrainFreq = 0.05f;

    [Header("Ore Settings")]
    public OreClass[] ores;

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();
    private BiomeClass curBiome;
    public Color[] biomeCols;

    #endregion

    private void Start()
    {
        seed = UnityEngine.Random.Range(-1000, 1000);
        for (int i = 0; i < ores.Length; i++)
        {
            ores[i].spreadTexture = new Texture2D(worldSize, worldSize);
        }

        biomeCols = new Color[biomes.Length];
        for (int i = 0; i < biomes.Length; i++)
        {
            biomeCols[i] = biomes[i].biomeCol;
        }

        //DrawTextures();
        DrawBiomeMap();
        DrawCavesAndOres();

        CreateChunks();
        GenerationTiles();
    }

    public void DrawBiomeMap()
    {

    }

    public void DrawCavesAndOres()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        float v;
        float o;

        for (int x = 0; x < caveNoiseTexture.width; x++)
        {
            for (int y = 0; y < caveNoiseTexture.height; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                if (v > curBiome.surfaceValue)
                    caveNoiseTexture.SetPixel(x, y, Color.white);
                else
                    caveNoiseTexture.SetPixel(x, y, Color.black);

                for (int i = 0; i < ores.Length; i++)
                {
                    ores[i].spreadTexture.SetPixel(x, y, Color.black);
                    if (curBiome.ores.Length >= i + 1)
                    {
                        o = Mathf.PerlinNoise((x + seed) * curBiome.ores[i].frequency, (y + seed) * curBiome.ores[i].frequency);
                        if (o > curBiome.ores[i].size)
                            ores[i].spreadTexture.SetPixel(x, y, Color.white);
                    }

                    ores[i].spreadTexture.Apply();
                }
            }
        }

        caveNoiseTexture.Apply();
    }

    public void DrawTextures()
    {
        for (int i = 0; i < biomes.Length; i++)
        {
            biomes[i].caveNoiseTexture = new Texture2D(worldSize, worldSize);
            for (int o = 0; o < biomes[i].ores.Length; o++)
            {
                biomes[i].ores[o].spreadTexture = new Texture2D(worldSize, worldSize);
                GenerationNoiseTextures(biomes[i].ores[o].frequency, biomes[i].ores[o].size, biomes[i].ores[o].spreadTexture);
            }
        }
    }

    public void GenerationNoiseTextures(float frequency, float limit, Texture2D noiseTexture)
    {
        float v;
        float b;
        Color col;
        for (int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                v = Mathf.PerlinNoise((x + seed) * frequency, (y + seed) * frequency);
                b = Mathf.PerlinNoise((x + seed) * biomeFrequency, (y + seed) * biomeFrequency);
                col = biomeGradient.Evaluate(b);
                biomeMap.SetPixel(x, y, col);

                if (v > limit)
                    noiseTexture.SetPixel(x, y, Color.white);
                else
                    noiseTexture.SetPixel(x, y, Color.black);
            }
        }

        noiseTexture.Apply();
        biomeMap.Apply();
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
        if (Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y)) > 0)
            return biomes[Array.IndexOf(biomeCols, biomeMap.GetPixel(x, y))];


        return curBiome;
    }

    public void GenerationTiles()
    {
        Sprite[] tileSprites;
        for (int x = 0; x < worldSize; x++)
        {
            float height;


            for (int y = 0; y < worldSize; y++)
            {
                curBiome = GetCurrentBiome(x, y);
                height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * curBiome.heightMultiplier + heightAddition;

                if (y >= height)
                    break;

                if (y < height - curBiome.dirtLayerHeight)
                {
                    tileSprites = curBiome.tileAtlas.stone.TileSprites;

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
                    tileSprites = curBiome.tileAtlas.dirt.TileSprites;
                }
                else
                {
                    //top layer of the terrain
                    tileSprites = curBiome.tileAtlas.grass.TileSprites;
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
                    int t = UnityEngine.Random.Range(0, curBiome.treeChance);
                    if (t == 1)
                    {
                        if (worldTiles.Contains(new Vector2(x, y)))
                        {
                            if (curBiome.biomeName == "Desert")
                            {
                                //generate cactus
                                GenerateCactus(curBiome.tileAtlas, UnityEngine.Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);
                            }
                            else
                                GenerateTree(UnityEngine.Random.Range(curBiome.minTreeHeight, curBiome.maxTreeHeight), x, y + 1);
                        }
                    }
                    else
                    {
                        int i = UnityEngine.Random.Range(0, curBiome.tallGrassChance);
                        if (i == 1)
                        {
                            if (worldTiles.Contains(new Vector2(x, y)))
                            {
                                if (curBiome.tileAtlas.tallGrass != null)
                                    PlaceTile(curBiome.tileAtlas.tallGrass.TileSprites, x, y + 1);
                            }
                        }
                    }
                }

            }
        }
    }

    public void GenerateCactus(TileAtlas atlas, int treeHeight, int x, int y)
    {
        //generate log
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(atlas.log.TileSprites, x, y + i);
        }
    }

    public void GenerateTree(int treeHeight, int x, int y)
    {
        //generate log
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(tileAtlas.log.TileSprites, x, y + i);
        }

        //generate leaves
        PlaceTile(tileAtlas.leaf.TileSprites, x, y + treeHeight);
        PlaceTile(tileAtlas.leaf.TileSprites, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.TileSprites, x, y + treeHeight + 2);

        PlaceTile(tileAtlas.leaf.TileSprites, x - 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf.TileSprites, x - 1, y + treeHeight + 1);

        PlaceTile(tileAtlas.leaf.TileSprites, x + 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf.TileSprites, x + 1, y + treeHeight + 1);
    }

    public void PlaceTile(Sprite[] tileSprites, int x, int y)
    {
        if (!worldTiles.Contains(new Vector2Int(x, y)))
        {
            GameObject newTile = new GameObject();

            int chunckCoord = Mathf.RoundToInt(Mathf.Round(x / chunckSize) * chunckSize);
            chunckCoord /= chunckSize;

            newTile.transform.parent = worldChunks[chunckCoord].transform;

            newTile.AddComponent<SpriteRenderer>();

            int spriteIndex = UnityEngine.Random.Range(0, tileSprites.Length);
            newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];

            newTile.name = tileSprites[0].name;
            newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

            worldTiles.Add(newTile.transform.position - Vector3.one * 0.5f);
        }
    }
}

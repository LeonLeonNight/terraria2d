using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;

    [Header("Tree confs")]
    public int treeChance = 15;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 6;

    [Header("Addons")]
    public int tallGrassChance = 10;

    [Header("Generation Settings")]
    public int chunckSize = 20; // ошибка при делении 100 на 16 как пример
    public bool generatCaves = true;
    public float surfaceValue = 0.25f;
    public int worldSize = 100;
    public float heightMultiplier = 25;
    public int heightAddition = 25;
    public int dirtLayerHeight = 5;

    [Header("Noise Settings")]
    public float caveFreq = 0.04f;
    public float terrainFreq = 0.08f;
    public float seed = -5229;
    public Texture2D caveNoiseTexture;

    [Header("Ore Settings")]
    public OreClass[] ores;
    /* public float coalRarity;//0.1
     public float coalSize;//0.8
     public float ironRarity, ironSize; //0.09 0.8
     public float goldRarity, goldSize; //0.2 0.9
     public float diamondRarity, diamondSize; // 0.8 0.93

     public Texture2D coalSpread;
     public Texture2D ironSpread;
     public Texture2D goldSpread;
     public Texture2D diamondSpread;*/

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();

    void OnValidate()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[3].spreadTexture = new Texture2D(worldSize, worldSize);

        GenerationNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        //ores
        GenerationNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture);
        GenerationNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);
        GenerationNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);
        GenerationNoiseTexture(ores[3].rarity, ores[3].size, ores[3].spreadTexture);

    }

    void Start()
    {
        seed = UnityEngine.Random.Range(-100, 100);

        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[3].spreadTexture = new Texture2D(worldSize, worldSize);

        GenerationNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        //ores
        GenerationNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture);
        GenerationNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);
        GenerationNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);
        GenerationNoiseTexture(ores[3].rarity, ores[3].size, ores[3].spreadTexture);

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

    public void GenerationTiles()
    {
        for (int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for (int y = 0; y < height; y++)
            {
                Sprite[] tileSprites;

                if (y < height - dirtLayerHeight)
                {
                    tileSprites = tileAtlas.stone.TileSprites;

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

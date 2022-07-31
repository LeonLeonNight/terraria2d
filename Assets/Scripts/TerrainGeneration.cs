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
    public float coalRarity;//0.1
    public float coalSize;//0.8
    public float ironRarity, ironSize; //0.09 0.8
    public float goldRarity, goldSize; //0.2 0.9
    public float diamondRarity, diamondSize; // 0.8 0.93

    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();

    void OnValidate()
    {
        if (caveNoiseTexture == null)
        {
            caveNoiseTexture = new Texture2D(worldSize, worldSize);
            coalSpread = new Texture2D(worldSize, worldSize);
            ironSpread = new Texture2D(worldSize, worldSize);
            goldSpread = new Texture2D(worldSize, worldSize);
            diamondSpread = new Texture2D(worldSize, worldSize);
        }

        GenerationNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        //ores
        GenerationNoiseTexture(coalRarity, coalSize, coalSpread);
        GenerationNoiseTexture(ironRarity, ironSize, ironSpread);
        GenerationNoiseTexture(goldRarity, goldSize, goldSpread);
        GenerationNoiseTexture(diamondRarity, diamondSize, diamondSpread);

    }

    void Start()
    {
        seed = UnityEngine.Random.Range(-100, 100);
        if (caveNoiseTexture == null)
        {
            caveNoiseTexture = new Texture2D(worldSize, worldSize);
            coalSpread = new Texture2D(worldSize, worldSize);
            ironSpread = new Texture2D(worldSize, worldSize);
            goldSpread = new Texture2D(worldSize, worldSize);
            diamondSpread = new Texture2D(worldSize, worldSize);
        }

        GenerationNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        //ores
        GenerationNoiseTexture(coalRarity, coalSize, coalSpread);
        GenerationNoiseTexture(ironRarity, ironSize, ironSpread);
        GenerationNoiseTexture(goldRarity, goldSize, goldSpread);
        GenerationNoiseTexture(diamondRarity, diamondSize, diamondSpread);

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
                Sprite tileSprite;

                if (y < height - dirtLayerHeight)
                {
                    if (coalSpread.GetPixel(x, y).r > 0.5f)
                        tileSprite = tileAtlas.coal.TileSprite;
                    else if (ironSpread.GetPixel(x, y).r > 0.5f)
                        tileSprite = tileAtlas.iron.TileSprite;
                    else if (goldSpread.GetPixel(x, y).r > 0.5f)
                        tileSprite = tileAtlas.gold.TileSprite;
                    else if (diamondSpread.GetPixel(x, y).r > 0.5f)
                        tileSprite = tileAtlas.diamond.TileSprite;
                    else 
                        tileSprite = tileAtlas.stone.TileSprite;
                }
                else if (y < height - 1)
                {
                    tileSprite = tileAtlas.dirt.TileSprite;
                }
                else
                {
                    //top layer of the terrain
                    tileSprite = tileAtlas.grass.TileSprite;
                }

                if (generatCaves)
                {
                    if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                    {
                        PlaceTile(tileSprite, x, y);
                    }
                }
                else
                {
                    PlaceTile(tileSprite, x, y);
                }

                if (y >= height - 1)
                {
                    int t = UnityEngine.Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        if (worldTiles.Contains(new Vector2(x, y)))
                        { GenerateTree(x, y + 1); }
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
            PlaceTile(tileAtlas.log.TileSprite, x, y + i);
        }

        PlaceTile(tileAtlas.leaf.TileSprite, x, y + treeHeight);

        PlaceTile(tileAtlas.leaf.TileSprite, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.TileSprite, x, y + treeHeight + 2);

        PlaceTile(tileAtlas.leaf.TileSprite, x - 1, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.TileSprite, x - 1, y + treeHeight);

        PlaceTile(tileAtlas.leaf.TileSprite, x + 1, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.TileSprite, x + 1, y + treeHeight);
    }

    public void PlaceTile(Sprite tileSprite, int x, int y)
    {
        GameObject newTile = new GameObject();

        float chunckCoord = (Mathf.RoundToInt(x / chunckSize) * chunckSize);
        chunckCoord /= chunckSize;
        newTile.transform.parent = worldChunks[(int)chunckCoord].transform;

        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldTiles.Add(newTile.transform.position - Vector3.one * 0.5f);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Sprites textures")]
    public Sprite stone;
    public Sprite grass;
    public Sprite dirt;
    public Sprite log;
    public Sprite leaf;

    [Header("Tree confs")]
    public int treeChance = 10;
    public int minTreeHeight = 4;
    public int maxTreeHeight = 7;

    [Header("Generation Settings")]
    public int chunckSize = 20; // ошибка при делении 100 на 16 как пример
    public bool generatCaves = true;
    public float surfaceValue = 0.25f;
    public int worldSize = 100;
    public float heightMultiplier = 4f;
    public int heightAddition = 25;
    public int dirtLayerHeight = 5;

    [Header("Noise Settings")]
    public float caveFreq = 0.05f;
    public float terrainFreq = 0.05f;
    public float seed;
    public Texture2D texture;

    public GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>();

    void Start()
    {
        seed = UnityEngine.Random.Range(-100, 100);
        GenerationMapTexture();
        CreateChunks();
        GenerationTiles();
        
    }

    public void CreateChunks()
    {
        int numChunks = worldSize / chunckSize;
        worldChunks = new GameObject[numChunks];

        for (int i =0; i < numChunks; i++)
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
                    tileSprite = stone;
                }
                else if (y < height - 1)
                {
                    tileSprite = dirt;
                }
                else
                {
                    //top layer of the terrain
                    tileSprite = grass;
                }

                if (generatCaves)
                {
                    if (texture.GetPixel(x, y).r > surfaceValue)
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

    public void GenerationMapTexture()
    {
        texture = new Texture2D(worldSize, worldSize);

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed) * caveFreq, (y + seed) * caveFreq);
                texture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        texture.Apply();
    }

    public void GenerateTree(int x, int y)
    {
        int treeHeight = UnityEngine.Random.Range(minTreeHeight, maxTreeHeight);
        for (int i = 0; i < treeHeight; i++)
        {
            PlaceTile(log, x, y + i);
        }

        PlaceTile(leaf, x, y + treeHeight);

        PlaceTile(leaf, x, y + treeHeight + 1);
        PlaceTile(leaf, x, y + treeHeight + 2);

        PlaceTile(leaf, x - 1, y + treeHeight + 1);
        PlaceTile(leaf, x - 1, y + treeHeight);

        PlaceTile(leaf, x + 1, y + treeHeight + 1);
        PlaceTile(leaf, x + 1, y + treeHeight);
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

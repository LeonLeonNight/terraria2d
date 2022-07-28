using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    public Sprite tile;

    public float surfaceValue = 0.25f;
    public int worldSize = 100;
    public float caveFreq = 0.05f;
    public float terrainFreq = 0.05f;
    public float heightMultiplier = 4f;
    public int heightAddition = 25;

    public float seed;
    public Texture2D texture;

    void Start()
    {
        seed = Random.Range(-100, 100);
        GenerationMapTexture();
        GenerationTiles();
    }

    public void GenerationTiles()
    {
        for (int x = 0; x < worldSize; x++)
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for (int y = 0; y < height; y++)
            {
                if (texture.GetPixel(x, y).r > surfaceValue)
                {
                    GameObject newTile = new GameObject(name = "tile");
                    newTile.transform.parent = transform;
                    newTile.AddComponent<SpriteRenderer>();
                    newTile.GetComponent<SpriteRenderer>().sprite = tile;
                    newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);
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
}

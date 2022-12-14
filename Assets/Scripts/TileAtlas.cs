using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileAtlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    [Header("Environment")]
    [SerializeField] public TileClass stone;
    [SerializeField] public TileClass grass;
    [SerializeField] public TileClass dirt;
    [SerializeField] public TileClass log;
    [SerializeField] public TileClass leaf;
    [SerializeField] public TileClass tallGrass;
    [SerializeField] public TileClass snow;
    [SerializeField] public TileClass sand;

    [Header("Ores")]
    [SerializeField] public TileClass coal;
    [SerializeField] public TileClass iron;
    [SerializeField] public TileClass gold;
    [SerializeField] public TileClass diamond;
}

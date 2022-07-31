using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="newtileClass", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
    [SerializeField] public string TileName;
    [SerializeField] public Sprite TileSprite;
}

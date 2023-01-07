using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainOffset : MonoBehaviour
{
    TerrainLayer[] terrain;

    [Header("Consistent Texture Offset via Terrain Layer in order:")]
    [SerializeField] Vector2[] offsetSpeedLayers;


    void Start() 
    {
        terrain = GetComponent<Terrain>().terrainData.terrainLayers;
    }

    void FixedUpdate()
    {
        for(int index = 0; index < terrain.Length; index++)
        {
            terrain[index].tileOffset += offsetSpeedLayers[index] * Time.deltaTime;
        }
    }
}

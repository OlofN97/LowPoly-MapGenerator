using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;
    public RiverAgent riverAgent;
    public int mapWidth;
    public int mapHeight;
    public float frequency;
    public float amplitude;
    public int octaveCount;
    public float persistance;
    public float lacunarity;

    public bool autoUpdate;
    public float[,] noiseMap;
    private float[,] riverMap;


    public void GenerateMap()
    {
        noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, frequency, amplitude, octaveCount, persistance, lacunarity);

        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.DrawNoiseMap(noiseMap);
        riverMap = riverAgent.generateRiver(mapHeight, mapWidth, noiseMap, amplitude);
        terrainGenerator.GenerateTerrarin(mapWidth, mapHeight, noiseMap, riverMap, amplitude);
    }

    public void Start()
    {
        GenerateMap();
    }
}

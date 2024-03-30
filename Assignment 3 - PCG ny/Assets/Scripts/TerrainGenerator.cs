using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static MapDisplay;

public class TerrainGenerator : MonoBehaviour
{
    int height;
    int width;
    int depth;
    
    public MapGenerator mapGenerator;
    public float heightScale;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Mesh mesh;
    private float amplitude;

    //Texture
    private Renderer rend;
    public Region[] regions;
    private Texture2D texture;
    private Color[] colorMap;

    private void Awake()
    {

    }
    public void GenerateTerrarin(int width, int height, float[,] heightMap, float[,] riverMap, float amplitude)
    {
        this.width = width;
        this.height = height;
        this.amplitude = amplitude;
        texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32; 
        GetComponent<MeshFilter>().mesh = mesh;
        rend = GetComponent<Renderer>();
        vertices = new Vector3[width * height];
        triangles = new int[width * height * 6];

        CreatePlane(heightMap, riverMap);
    }

    private void CreatePlane(float[,] heightMap, float[,] riverMap)
    {
        CreateVertices(heightMap, riverMap);
        CreateTriangles();
        CreateUVCords();
        CreateColorMap(heightMap, riverMap);
        ApplySettingsToMesh();
    }



    private void ApplySettingsToMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        texture.SetPixels(colorMap);
        texture.Apply();
        rend.sharedMaterial.mainTexture = texture;
    }

    private void CreateUVCords()
    {
        uvs = new Vector2[vertices.Length];
        int counter = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                uvs[counter] = new Vector2(y / (float)height, x / (float)width);
                counter++;
            }
        }
    }

    private void CreateTriangles()
    {

        // [0,1] [1,1]
        // [0,0] [1,0] 
        int triangleCount = 0;
        for (int i = 0; i < height - 1; i++)
        {
            for (int j = 0; j < width - 1; j++)
            {

                triangles[triangleCount] = j + i * (width); // [0,0]   
                triangleCount++;
                triangles[triangleCount] = j + (i + 1) * (width); // [0,1]
                triangleCount++;
                triangles[triangleCount] = (j + 1) + i * (width); // [1, 0]
                triangleCount++;


                triangles[triangleCount] = j + (i + 1) * (width); //[1,0]
                triangleCount++;
                triangles[triangleCount] = (j + 1) + (i + 1) * (width); //[1,1]
                triangleCount++;
                triangles[triangleCount] = (j + 1) + i * (width); //[1,0]
                triangleCount++;
            }
        }
    }

    private void CreateVertices(float[,] heightMap, float[,] riverMap)
    {
        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                if (riverMap[i, j] != 0)
                {
                    vertices[j + i * width] = new Vector3(j, (heightMap[i, j] - 0.20f * amplitude + riverMap[i, j] * amplitude) * heightScale , i);
                }
                else
                {
                    vertices[j + i * width] = new Vector3(j, (heightMap[i, j]) * heightScale, i);
                }
            }
        }
    }

    private void CreateColorMap(float[,] noiseMap, float[,] riverMap)
    {
        colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                foreach (Region reg in regions)
                {
                    if (riverMap[x, y] != 0) //if there is a river tile to be painted
                    {
                        if(noiseMap[x,y] > regions[0].maxHeight * amplitude) //if there already exists ocean on that tile
                        {
                            colorMap[width * y + x] = regions[0].color * (riverMap[x, y] / regions[0].maxHeight); //Times noisemap value to add gradient
                        }
                        else
                        {
                            colorMap[width * y + x] = reg.color * (noiseMap[x, y] / (reg.maxHeight * amplitude));
                        }
                        break;
                    }
                    else if (reg.minHeight * amplitude <= noiseMap[x, y]) 
                    {
                        if (reg.maxHeight * amplitude >= noiseMap[x, y])
                        {
                            colorMap[width * y + x] = reg.color * (noiseMap[x, y] / (reg.maxHeight * amplitude)); //Times noisemap value to add gradient
                            break;
                        }
                    }
                }

            }
        }
    }

    [Serializable]
    public struct Region
    {
        public float minHeight;
        public float maxHeight;
        public Color color;
        public string name;
    }
}

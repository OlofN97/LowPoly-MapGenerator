using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int width, int height, float frequency, float amplitude, int octaveCount, float persistence, float lacunarity)
    {
        float[,] noiseMap = new float[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)//G� igenom hela mappen och evaluera varje positions v�rde.
            {                              
                noiseMap[i, j] = EvaluatePosition(i, j, frequency, amplitude, octaveCount, persistence, lacunarity);
            }
        }
        return noiseMap;
    }

    public static float EvaluatePosition(int x, int y, float frequency, float amplitude, int octaveCount, float persistence, float lacunarity)
    {
        float value = 0;
        for (int k = 0; k < octaveCount; k++) //Octave -> Hur m�nga lager av perlin noice.
        {
            value += PerlinNoise.noice(x * frequency, y * frequency) * amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        value = value * 0.5f + 0.5f;
        return value;
    }
}

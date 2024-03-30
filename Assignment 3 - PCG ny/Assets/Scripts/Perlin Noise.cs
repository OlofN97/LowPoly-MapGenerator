using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PerlinNoise
{
    private static readonly int[] permutation = { 151,160,137,91,90,15,					
		131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
		190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,    
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
    };

    private static readonly int[] p; 													
    static PerlinNoise()
    {
        //System.Random rand = new System.Random();
        //for(int i = 0; i < 256; i++)
        //{
        //    permutation[i] = rand.Next(0 , 256);
        //}

        p = new int[512];
        for (int x = 0; x < 512; x++)
        {
            p[x] = permutation[x % 256];
        }
    }


    public static float noice(float x, float y)
    {
        int xPos =(int) (Mathf.Floor(x) % 255);
        int yPos =(int) (Mathf.Floor(y) % 255);

        float xf = x - Mathf.Floor(x);
        float yf = y - Mathf.Floor(y);
       

        Vector2 topRight = new Vector2(xf - 1.0f, yf - 1.0f);
        Vector2 topLeft = new Vector2(xf, yf - 1.0f);
        Vector2 bottomRight = new Vector2(xf - 1.0f, yf);
        Vector2 bottomLeft = new Vector2(xf, yf);

         
        int valueTopRight = p[p[xPos + 1] + yPos + 1];
        int valueTopLeft = p[p[xPos] + yPos + 1];
        int valueBottomRight = p[p[xPos + 1] + yPos];
        int valueBottomLeft = p[p[xPos] + yPos];

        float dotTopRight = Vector2.Dot(GetConstantVector(valueTopRight), topRight);
        float dotTopLeft = Vector2.Dot(GetConstantVector(valueTopLeft), topLeft);
        float dotBottomRight = Vector2.Dot( GetConstantVector(valueBottomRight), bottomRight);
        float dotBottomLeft = Vector2.Dot(GetConstantVector(valueBottomLeft), bottomLeft);

        float u = Fade(xf);
        float v = Fade(yf);
        
        return Lerp(u,
            Lerp(v, dotBottomLeft, dotTopLeft),
            Lerp(v, dotBottomRight, dotTopRight)
        );


    }
    private static float Fade( float t)
    {
        return ((6 * t - 15) * t + 10) * t * t * t;
    }

    private static float Lerp(float t,float a1, float a2)
    {
        return a1 + t * (a2 - a1);
    }

    public static Vector2 GetConstantVector(int v)
    {
        int h = v % 4;
        switch (h)
        {
            case 0:
                return new Vector2(1.0f, 1.0f);
            case 1:
                return new Vector2(-1.0f, 1.0f);
            case 2:
                return new Vector2(-1.0f, -1.0f);
            case 3:
                return new Vector2(1.0f, -1.0f);
            default:
                return new Vector2(0, 0);

        }
    }
}

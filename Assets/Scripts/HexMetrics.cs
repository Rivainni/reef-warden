// This entire class is for creating a hexagon.
// innerRadius is distance from centre to edge
// outerRadius is distance from centre to vertex

using UnityEngine;

public static class HexMetrics
{
    // How many cells form a chunk. Here we assume 25 cells per chunk.
    public const int chunkSizeX = 5, chunkSizeZ = 5;

    // outerRadius is distance from centre to vertex
    public const float outerRadius = 10f;

    // innerRadius is distance from centre to edge
    public const float innerRadius = outerRadius * 0.866025404f;

    // point top
    static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int)direction + 1];
    }
}
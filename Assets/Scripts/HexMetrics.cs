using UnityEngine;

public static class HexMetrics
{

    public const float outerRadius = 10f;

    public const float innerRadius = outerRadius * 0.866025404f;

    // point top
    // public static Vector3[] corners = {
    //     new Vector3(0f, 0f, outerRadius),
    //     new Vector3(innerRadius, 0f, 0.5f * outerRadius),
    //     new Vector3(innerRadius, 0f, -0.5f * outerRadius),
    //     new Vector3(0f, 0f, -outerRadius),
    //     new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
    //     new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
    // };


    // flat top (We did use this in the GDD so we'll stick with this for now)
    public static Vector3[] corners = {
        new Vector3(-0.5f * outerRadius, 0f, innerRadius),
        new Vector3(0.5f * outerRadius, 0f, innerRadius),
        new Vector3(outerRadius, 0f, 0f),
        new Vector3(0.5f * outerRadius, 0f, -innerRadius),
        new Vector3(-0.5f * outerRadius, 0f, -innerRadius),
        new Vector3(-outerRadius, 0f, 0f),
    };
}
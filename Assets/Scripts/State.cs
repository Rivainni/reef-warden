using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "State")]
public class State : ScriptableObject
{
    [SerializeField] List<Vector2> landBlocks;

    public bool IsLand(Vector2 coordinates)
    {
        if (landBlocks.Contains(coordinates))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
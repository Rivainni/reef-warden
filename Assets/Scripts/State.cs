using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "State")]
public class State : ScriptableObject
{
    [SerializeField] List<Vector2> landBlocks;
    [SerializeField] List<Vector2> structureLocations;
    [SerializeField] List<Vector2> initialUnits;

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

    public bool HasStructure(Vector2 coordinates)
    {
        if (structureLocations.Contains(coordinates))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public int HasInitialUnit(Vector2 coordinates)
    {
        if (coordinates == initialUnits[0])
        {
            return 0;
        }
        else if (coordinates == initialUnits[1])
        {
            return 1;
        }
        else
        {
            return 2;
        }
    }

    public Vector2 GetBaseLocation()
    {
        return structureLocations[0];
    }
}
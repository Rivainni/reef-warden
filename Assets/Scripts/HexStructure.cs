using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexStructure : MonoBehaviour
{
    public string StructureType
    {
        get
        {
            return structureType;
        }
        set
        {
            structureType = value;
        }
    }
    string structureType;

    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location)
            {
                location.Structure = null;
            }
            location = value;
            value.Structure = this;
            transform.localPosition = value.Position;
        }
    }
    HexCell location;

    public float Orientation
    {
        get
        {
            return orientation;
        }
        set
        {
            orientation = value;
            transform.localRotation = Quaternion.Euler(0f, value, 0f);
        }
    }

    float orientation;

    // methods beyond this point

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Die()
    {
        location.Unit = null;
        Destroy(gameObject);
    }
}
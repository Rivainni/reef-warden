using UnityEngine;

[System.Serializable]
public struct HexCoordinates
{
    [SerializeField] int x, z;
    public int X
    {
        get
        {
            return x;
        }
    }

    public int Y
    {
        get
        {
            return -X - Z;
        }
    }
    public int Z
    {
        get
        {
            return z;
        }
    }

    public HexCoordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        // return new HexCoordinates(x, z - x / 2);
        return new HexCoordinates(x - z / 2, z);
    }

    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Z.ToString() + ")";
    }

    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Z.ToString();
    }

    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (HexMetrics.outerRadius * 1.5f);
        float y = -x;

        float offset = position.z / (HexMetrics.outerRadius * 4f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        if (iX + iY + iZ != 0)
        {
            Debug.LogWarning("Rounding error!");
        }

        return new HexCoordinates(iX, iZ);
    }
}

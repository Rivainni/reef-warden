using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexUnit : MonoBehaviour
{
    public string UnitType
    {
        get
        {
            return unitType;
        }
        set
        {
            unitType = value;
        }
    }
    string unitType;

    public int ActionPoints
    {
        get
        {
            return actionPoints;
        }
        set
        {
            actionPoints = value;
        }
    }

    int actionPoints;
    int maxActionPoints;

    public int hp;

    public bool takenTurn;
    public bool movement;
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
                location.Unit = null;
            }
            location = value;
            value.Unit = this;
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
    const float travelSpeed = 2f;
    const float rotationSpeed = 180f;
    List<HexCell> pathToTravel;

    // methods beyond this point
    void Start()
    {
        maxActionPoints = actionPoints;
    }

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Die()
    {
        location.Unit = null;
        Destroy(gameObject);
    }

    public bool IsValidDestination(HexCell cell)
    {
        return !cell.IsImpassable && !cell.Unit;
    }

    public void Travel(List<HexCell> path)
    {
        Location = path[path.Count - 1];
        pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(TravelPath());
    }

    IEnumerator TravelPath()
    {
        Vector3 a, b, c = pathToTravel[0].Position;
        transform.localPosition = c;
        yield return LookAt(pathToTravel[1].Position);

        float t = Time.deltaTime * travelSpeed;

        for (int i = 1; i < pathToTravel.Count; i++)
        {
            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + pathToTravel[i].Position) * 0.5f;
            for (; t < 1f; t += Time.deltaTime * travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }
            t -= 1f;
        }

        a = c;
        b = pathToTravel[pathToTravel.Count - 1].Position;
        c = b;
        for (; t < 1f; t += Time.deltaTime * travelSpeed)
        {
            transform.localPosition = Bezier.GetPoint(a, b, c, t);
            Vector3 d = Bezier.GetDerivative(a, b, c, t);
            d.y = 0f;
            transform.localRotation = Quaternion.LookRotation(d);
            yield return null;
        }

        orientation = transform.localRotation.eulerAngles.y;
        transform.localPosition = location.Position;
        pathToTravel = null;
    }

    IEnumerator LookAt(Vector3 point)
    {
        point.y = transform.localPosition.y;
        Quaternion fromRotation = transform.localRotation;
        Quaternion toRotation = Quaternion.LookRotation(point - transform.localPosition);
        float angle = Quaternion.Angle(fromRotation, toRotation);

        if (angle > 0f)
        {
            float speed = rotationSpeed / angle;

            for (float t = Time.deltaTime * speed; t < 1f; t += Time.deltaTime * speed)
            {
                transform.localRotation = Quaternion.Slerp(fromRotation, toRotation, t);
                yield return null;
            }
        }

        transform.LookAt(point);
        orientation = transform.localRotation.eulerAngles.y;
    }

    public void DecreaseHP()
    {
        hp -= (maxActionPoints - actionPoints);
    }

    public void ResetMovement()
    {
        ActionPoints = maxActionPoints;
    }


    // public void Save(BinaryWriter writer)
    // {
    //     location.coordinates.Save(writer);
    //     writer.Write(orientation);
    // }

    // public static void Load(BinaryReader reader, HexGrid grid)
    // {
    //     HexCoordinates coordinates = HexCoordinates.Load(reader);
    //     float orientation = reader.ReadSingle();
    //     grid.AddUnit(Instantiate(unitPrefab), grid.GetCell(coordinates), orientation);
    // }
}
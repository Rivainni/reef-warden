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

    public int VisionRange
    {
        get
        {
            return visionRange;
        }
        set
        {
            visionRange = value;
        }
    }
    int visionRange;

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

    int actionPoints, maxActionPoints, reducedActionPoints;

    public float HP
    {
        get
        {
            return hp;
        }
        set
        {
            hp = value;
            healthBar.SetHealth(value);
        }
    }
    float hp;
    public HealthBar healthBar;

    public bool takenTurn;
    public bool movement = false;
    public HexCell Location
    {
        get
        {
            return location;
        }
        set
        {
            if (location && IsPlayerControlled())
            {
                Grid.DecreaseVisibility(location, visionRange);
                location.Unit = null;
            }
            else if (location)
            {
                location.Unit = null;
            }

            location = value;
            value.Unit = this;

            if (IsPlayerControlled())
            {
                value.IncreaseVisibility();
                Grid.IncreaseVisibility(value, visionRange);
            }

            transform.localPosition = value.Position;
        }
    }
    HexCell location, currentTravelLocation;

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

    public bool IsVisible
    {
        get
        {
            return isVisible;
        }
        set
        {
            isVisible = value;
            // foreach (Renderer item in unitRenderers)
            // {
            //     item.enabled = value;
            // }
        }
    }
    bool isVisible;

    public HexGrid Grid { get; set; }

    float orientation;
    bool busy;
    bool interacted;
    bool moored;
    const float travelSpeed = 3.0f;
    const float rotationSpeed = 180f;
    List<HexCell> pathToTravel;
    Renderer[] unitRenderers;

    void Awake()
    {
        unitRenderers = GetComponentsInChildren<Renderer>();
    }

    void Start()
    {
        maxActionPoints = actionPoints;
        reducedActionPoints = maxActionPoints;
        if (IsPatrolBoat())
        {
            HP = 100;
            healthBar.SetMaxHealth(HP);
        }
        busy = false;
        interacted = false;
        moored = false;
    }

    public void ValidateLocation()
    {
        transform.localPosition = location.Position;
    }

    public void Die()
    {
        if (location && IsPlayerControlled())
        {
            Grid.DecreaseVisibility(location, visionRange);
        }
        location.Unit = null;
        Grid.GetAudioManager().Stop("Boat");
        Destroy(gameObject);
    }

    public bool IsValidDestination(HexCell cell)
    {
        return !GlobalCellCheck.IsImpassable(cell) && !cell.Unit && !GlobalCellCheck.IsNotReachable(cell.Index);
    }

    public void Travel(List<HexCell> path)
    {
        location.Unit = null;
        location = path[path.Count - 1];
        location.Unit = this;
        pathToTravel = path;
        StopAllCoroutines();
        StartCoroutine(TravelPath());
    }

    IEnumerator TravelPath()
    {
        Grid.GetAudioManager().Play("Boat", 0);
        Vector3 a, b, c = pathToTravel[0].Position;
        // transform.localPosition = c;
        yield return LookAt(pathToTravel[1].Position);

        if (IsPlayerControlled())
        {
            Grid.DecreaseVisibility
            (
                currentTravelLocation ? currentTravelLocation : pathToTravel[0],
                visionRange
            );
        }

        float t = Time.deltaTime * travelSpeed;

        for (int i = 1; i < pathToTravel.Count; i++)
        {
            currentTravelLocation = pathToTravel[i];
            a = c;
            b = pathToTravel[i - 1].Position;
            c = (b + currentTravelLocation.Position) * 0.5f;

            if (IsPlayerControlled())
            {
                Grid.IncreaseVisibility(pathToTravel[i], visionRange);
            }

            for (; t < 1f; t += Time.deltaTime * travelSpeed)
            {
                transform.localPosition = Bezier.GetPoint(a, b, c, t);
                Vector3 d = Bezier.GetDerivative(a, b, c, t);
                d.y = 0f;
                transform.localRotation = Quaternion.LookRotation(d);
                yield return null;
            }

            if (IsPlayerControlled())
            {
                Grid.DecreaseVisibility(pathToTravel[i], visionRange);
            }

            t -= 1f;
        }
        currentTravelLocation = null;

        a = c;
        // b = pathToTravel[pathToTravel.Count - 1].Position;
        b = location.Position;
        c = b;

        if (IsPlayerControlled())
        {
            Grid.IncreaseVisibility(location, visionRange);
        }

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
        movement = false;
        Grid.GetAudioManager().Stop("Boat");
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
        HP -= reducedActionPoints - ActionPoints;
        healthBar.SetHealth(HP);
        if (HP <= 10.0f)
        {
            reducedActionPoints = Mathf.RoundToInt((10 / 100) * maxActionPoints);
        }
        else
        {
            reducedActionPoints = Mathf.RoundToInt((HP / 100) * maxActionPoints);
        }
        Debug.Log(HP + " HP. Your max points are " + reducedActionPoints + ".");
    }

    public void RestoreHP()
    {
        HP = 100;
        reducedActionPoints = maxActionPoints;
    }

    public void ResetMovement()
    {
        if (busy)
        {
            ActionPoints = 0;
        }
        else
        {
            ActionPoints = reducedActionPoints;
        }
    }

    public void ToggleBusy()
    {
        if (busy)
        {
            busy = false;
        }
        else
        {
            busy = true;
        }
    }

    public bool IsPlayerControlled()
    {
        return UnitType.Contains("Patrol Boat") || UnitType.Contains("Service Boat");
    }

    public bool IsPatrolBoat()
    {
        return UnitType.Contains("Patrol Boat");
    }

    public bool IsDayBoat()
    {
        return IsPlayerControlled() || UnitType.Contains("Tourist Boat");
    }

    public AIBehaviour GetAIBehaviour()
    {
        return GetComponent<AIBehaviour>();
    }

    public PlayerBehaviour GetPlayerBehaviour()
    {
        return GetComponent<PlayerBehaviour>();
    }

    public void SetInteracted()
    {
        interacted = true;
    }

    public void FailedInteraction()
    {
        interacted = false;
    }

    public bool HasInteracted()
    {
        return interacted;
    }

    public void SetMoored()
    {
        moored = true;
    }

    public bool HasMoored()
    {
        return moored;
    }

    public bool ScanFor(string type)
    {
        for (HexDirection i = HexDirection.NE; i <= HexDirection.NW; i++)
        {
            HexCell currentA = Location.GetNeighbor(i);
            if (currentA != null)
            {
                if (currentA.Unit)
                {
                    if (currentA.Unit.UnitType.Contains(type))
                    {
                        return true;
                    }
                }
                for (HexDirection j = HexDirection.NE; j <= HexDirection.NW; j++)
                {
                    HexCell currentB = currentA.GetNeighbor(j);
                    if (currentB != null)
                    {
                        if (currentB.Unit != null)
                        {
                            if (currentB.Unit.UnitType.Contains(type))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
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
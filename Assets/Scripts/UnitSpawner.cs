using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public HexGrid hexGrid;
    // PLAYER: Tier 1 Patrol Boat, Tier 2 Patrol Boat, Service Boat
    // ALLIED: Tourist Boat, Research Boat
    // ENEMY: Fishing Boat
    // This has been moved to the inspector for ease of editing.
    [SerializeField] string[] unitTypes;
    [SerializeField] int[] movementPoints;
    [SerializeField] HexUnit[] unitPrefabs;

    // PLAYER: Buoy, Ranger Station, Basketball Court, Rec Room, Radio, AIS, Radar, IGSAT, Souvenir Stand, 3rd Party Marketing Agencies
    [SerializeField] string[] structureTypes;
    [SerializeField] HexStructure[] structurePrefabs;

    public void SpawnUnit(HexCell cell, string unitType)
    {
        int unitIndex = System.Array.IndexOf(unitTypes, unitType);
        if (cell && !cell.Unit)
        {
            hexGrid.AddUnit(Instantiate(unitPrefabs[unitIndex]), cell, Random.Range(0f, 360f), unitType, movementPoints[unitIndex]);
        }
    }

    public void SpawnStructure(HexCell cell, string structureType)
    {
        int structureIndex = System.Array.IndexOf(structureTypes, structureType);
        if (cell && !cell.Structure)
        {
            hexGrid.AddStructure(Instantiate(structurePrefabs[structureIndex]), cell, Random.Range(0f, 360f), structureType);
        }
    }

    public void DestroyUnit(HexUnit unit)
    {
        hexGrid.RemoveUnit(unit);
    }

    public string[] GetStructureTypes()
    {
        return structureTypes;
    }

    HexCell GetCellUnderCursor()
    {
        return hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
    }
}
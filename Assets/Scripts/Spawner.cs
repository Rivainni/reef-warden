using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
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
    [SerializeField] string[] upgradeTypes;
    [SerializeField] HexStructure[] structurePrefabs;
    [SerializeField] Upgrade[] upgradePrefabs;

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
            hexGrid.AddStructure(Instantiate(structurePrefabs[structureIndex]), cell, 315f, structureType);
        }
    }

    public void SpawnUpgrade(HexCell cell, string upgradeType, int constructionTime, int researchCost, int buildCost)
    {
        int upgradeIndex = System.Array.IndexOf(upgradeTypes, upgradeType);
        if (cell && !cell.Structure)
        {
            hexGrid.AddUpgrade(Instantiate(upgradePrefabs[upgradeIndex]), cell, Random.Range(0f, 360f), upgradeType, constructionTime, researchCost, buildCost);
        }
    }

    public void DestroyUnit(HexUnit unit)
    {
        AIBehaviour currentBehaviour = unit.gameObject.GetComponent<AIBehaviour>();
        currentBehaviour.Clean();
        hexGrid.RemoveUnit(unit);
    }

    public string[] GetStructureTypes()
    {
        return structureTypes;
    }

    public string[] GetUpgradeTypes()
    {
        return upgradeTypes;
    }

    // actually spawning other types
    public void RandomSpawn(string unitType)
    {
        int random = Random.Range(0, 624);
        HexCell cell = hexGrid.GetCells()[random];

        while (GlobalCellCheck.IsImpassable(cell) || GlobalCellCheck.IsNotReachable(random))
        {
            random = Random.Range(0, 624);
            cell = hexGrid.GetCells()[random];
        }

        int unitIndex = System.Array.IndexOf(unitTypes, unitType);
        if (cell && !cell.Unit)
        {
            hexGrid.AddUnit(Instantiate(unitPrefabs[unitIndex]), cell, Random.Range(0f, 360f), unitType, movementPoints[unitIndex]);
        }
    }
}
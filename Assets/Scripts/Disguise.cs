using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disguise : MonoBehaviour
{
    public HexGrid grid;
    Renderer renderer;
    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        renderer.material.color = grid.GetCells()[0].GetComponent<Renderer>().material.color;
    }
}
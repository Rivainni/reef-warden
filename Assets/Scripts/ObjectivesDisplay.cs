using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ObjectivesDisplay : MonoBehaviour
{
    public PlayerState currentState;
    [SerializeField] Text textPrefab;
    List<Text> objectives;

    void Start()
    {
        objectives = new List<Text>();
    }

    public void DisplayObjectives()
    {
        foreach (Text item in objectives)
        {
            Destroy(item.gameObject);
        }
        objectives.Clear();

        foreach (string item in currentState.GetObjectives())
        {
            Text temp = Instantiate(textPrefab, transform.position, Quaternion.identity, transform);
            temp.text = item;
            Debug.Log(temp.text);
            objectives.Add(temp);
        }
    }
}

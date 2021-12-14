using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ObjectivesDisplay : MonoBehaviour
{
    public PlayerState currentState;
    [SerializeField] Text textPrefab;
    List<Text> objectives;
    // Start is called before the first frame update
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
            objectives.Add(temp);
        }
    }
}

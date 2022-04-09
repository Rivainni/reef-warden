using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Feature : MonoBehaviour
{
    [SerializeField] Sprite[] features;
    Image currentImage;
    // Start is called before the first frame update
    void Awake()
    {
        currentImage = GetComponent<Image>();
    }

    public void SetImage(int index)
    {
        currentImage.sprite = features[index];
    }

    public void SetImageScale(float x, float y)
    {
        currentImage.rectTransform.localScale = new Vector2(x, y);
    }

    public void EnableImage(bool toggle)
    {
        currentImage.enabled = toggle;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveArrow : MonoBehaviour
{
    public Transform targetTransform;
    public RectTransform arrowRectTransform;

    float border;


    void Start()
    {
        arrowRectTransform = GetComponent<RectTransform>();
        border = Screen.width * 0.1f;
        arrowRectTransform.position = targetTransform.position;
    }

    void LateUpdate()
    {
        Vector3 fromPosition = Camera.main.transform.position;
        fromPosition.z = 0f;
        Vector3 toPosition = targetTransform.position;
        Vector3 direction = (toPosition - fromPosition).normalized;
        float angle = GetAngleFromVectorFloat(direction);
        arrowRectTransform.localEulerAngles = new Vector3(0, 0, angle);

        Vector3 targetPositionScreenPoint = Camera.main.WorldToScreenPoint(targetTransform.position);

        if (IsOffScreen(targetPositionScreenPoint))
        {
            Vector3 cappedTargetScreenPosition = targetPositionScreenPoint;
            cappedTargetScreenPosition.x = Mathf.Clamp(targetPositionScreenPoint.x, border, Screen.width - border);
            cappedTargetScreenPosition.y = Mathf.Clamp(targetPositionScreenPoint.y, border, Screen.height - border);

            Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint(cappedTargetScreenPosition);
            // RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent.GetComponent<RectTransform>(), cappedTargetScreenPosition, Camera.main, out pointerWorldPosition);
            arrowRectTransform.position = pointerWorldPosition;
            arrowRectTransform.localPosition = new Vector3(arrowRectTransform.position.x, arrowRectTransform.position.y, 0f);
        }
        else
        {
            Vector3 pointerWorldPosition = Camera.main.ScreenToWorldPoint(targetPositionScreenPoint);
            // RectTransformUtility.ScreenPointToWorldPointInRectangle(transform.parent.GetComponent<RectTransform>(), targetPositionScreenPoint, Camera.main, out pointerWorldPosition);
            arrowRectTransform.position = pointerWorldPosition;
            arrowRectTransform.localPosition = new Vector3(arrowRectTransform.position.x, arrowRectTransform.position.y, 0f);
        }
    }

    float GetAngleFromVectorFloat(Vector3 convert)
    {
        return (Mathf.Atan2(convert.y, convert.x) * Mathf.Rad2Deg) % 360;
    }

    bool IsOffScreen(Vector3 target)
    {
        return target.x <= border || target.x >= (Screen.width - border) || target.x <= border || target.x >= (Screen.height - border);
    }
}

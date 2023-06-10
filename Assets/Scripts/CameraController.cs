using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    [SerializeField] float movementSpeed;
    [SerializeField] float rotationAmount;
    [SerializeField] float edgeSize;
    [SerializeField] Vector3 zoomAmount;

    Vector3 newPosition;
    Quaternion newRotation;
    Vector3 newZoom;
    Vector3 rotateStartPosition;
    Vector3 rotateCurrentPosition;

    float minX;
    float minZoomY;
    float minZoomZ;
    float minZ;
    float maxX;
    float maxZoomY;
    float maxZoomZ;
    float maxZ;
    bool edgeToggle;
    bool freeze;

    void Start()
    {
        freeze = false;
        SetClamps();
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
        edgeToggle = true;
    }

    void Update()
    {
        // allows us to disable the camera movement when needed
        if (!freeze)
        {
            HandleMouseButtons();
            HandleMovementInput();
        }
    }

    void HandleMouseButtons()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
        }

        if (Input.GetMouseButtonDown(2))
        {
            rotateStartPosition = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            rotateCurrentPosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotateCurrentPosition;

            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }

    void HandleMovementInput()
    {
        bool locked = false;

        // keyboard movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += transform.forward * movementSpeed;
            locked = true;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += transform.forward * -movementSpeed;
            locked = true;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += transform.right * -movementSpeed;
            locked = true;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += transform.right * movementSpeed;
            locked = true;
        }

        // edge movement. if you press spacebar it temporarily suspends the thingo.
        if (Input.GetKey(KeyCode.Space))
        {
            if (edgeToggle)
            {
                edgeToggle = false;
            }
            else
            {
                edgeToggle = true;
            }
        }

        if (edgeToggle && !locked)
        {
            if (Input.mousePosition.y > Screen.height - edgeSize)
            {
                newPosition += transform.forward * movementSpeed;
            }
            if (Input.mousePosition.y < edgeSize)
            {
                newPosition += transform.forward * -movementSpeed;
            }
            if (Input.mousePosition.x < edgeSize)
            {
                newPosition += transform.right * -movementSpeed;
            }
            if (Input.mousePosition.x > Screen.width - edgeSize)
            {
                newPosition += transform.right * movementSpeed;
            }
        }

        if (Input.GetKey(KeyCode.R))
        {
            newZoom += zoomAmount;
        }

        if (Input.GetKey(KeyCode.F))
        {
            newZoom -= zoomAmount;
        }

        newPosition = new Vector3(Mathf.Clamp(newPosition.x, minX, maxX), newPosition.y, Mathf.Clamp(newPosition.z, minZ, maxZ));
        newZoom = new Vector3(newZoom.x, Mathf.Clamp(newZoom.y, minZoomY, maxZoomY), Mathf.Clamp(newZoom.z, minZoomZ, maxZoomZ));

        // smooth movement, have to linear interpolate
        transform.position = Vector3.Lerp(transform.position, newPosition, 0.5f);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.5f);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, 0.5f);
    }

    void SetClamps()
    {
        // hardcode for now
        minX = -230.0f;
        minZoomY = -10.0f;
        minZoomZ = 25.0f;
        minZ = -230.0f;
        maxX = 1004f;
        maxZoomY = 50.0f;
        maxZoomZ = 80.0f;
        maxZ = 836f;
    }

    public void FreezeCamera(bool toggle)
    {
        freeze = toggle;
    }
}

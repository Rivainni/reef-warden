using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;

    [SerializeField] float normalSpeed;
    [SerializeField] float fastSpeed;
    [SerializeField] float movementSpeed;
    [SerializeField] float movementTime;
    [SerializeField] float rotationAmount;
    [SerializeField] float edgeSize;
    [SerializeField] Vector3 zoomAmount;

    Vector3 newPosition;
    Quaternion newRotation;
    Vector3 newZoom;

    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;
    Vector3 rotateStartPosition;
    Vector3 rotateCurrentPosition;

    float minX;
    float minY;
    float minZ;
    float maxX;
    float maxY;
    float maxZ;

    void Start()
    {
        SetClamps();
        newPosition = transform.position;
        newRotation = transform.rotation;
        // newZoom = new Vector3(cameraTransform.localPosition.x, cameraTransform.localPosition.y + 20.0f, cameraTransform.localPosition.z);
        newZoom = cameraTransform.localPosition;
    }

    void Update()
    {
        HandleMouseInput();
        HandleMovementInput();
    }

    void HandleMouseInput()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;
        }

        // if (Input.GetMouseButtonDown(1))
        // {
        //     Plane plane = new Plane(Vector3.up, Vector3.zero);

        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //     float entry;

        //     if (plane.Raycast(ray, out entry))
        //     {
        //         dragStartPosition = ray.GetPoint(entry);
        //     }
        // }
        // if (Input.GetMouseButton(1))
        // {
        //     Plane plane = new Plane(Vector3.up, Vector3.zero);

        //     Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //     float entry;

        //     if (plane.Raycast(ray, out entry))
        //     {
        //         dragCurrentPosition = ray.GetPoint(entry);

        //         newPosition = transform.position + dragStartPosition - dragCurrentPosition;
        //     }
        // }

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
        // fast movement
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = fastSpeed;
        }
        else
        {
            movementSpeed = normalSpeed;
        }

        // keyboard movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            newPosition += transform.forward * movementSpeed;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            newPosition += transform.forward * -movementSpeed;
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            newPosition += transform.right * -movementSpeed;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            newPosition += transform.right * movementSpeed;
        }

        // edge movement
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

        if (Input.GetKey(KeyCode.Q))
        {
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
        }
        if (Input.GetKey(KeyCode.E))
        {
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        }

        if (Input.GetKey(KeyCode.R))
        {
            newZoom += zoomAmount;
        }

        if (Input.GetKey(KeyCode.F))
        {
            newZoom -= zoomAmount;
        }

        // newPosition = new Vector3(Mathf.Clamp(newPosition.x, minX, maxX), newPosition.y, Mathf.Clamp(newPosition.z, minZ, maxZ));
        // newZoom = new Vector3(newZoom.x, Mathf.Clamp(newZoom.y, minY, maxY), newZoom.z);

        // smooth movement, have to linear interpolate
        transform.position = Vector3.Lerp(transform.position, newPosition, 0.5f);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, 0.5f);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, 0.5f);
    }

    void SetClamps()
    {
        // hardcode for now
        minX = 0.0f;
        minY = 0.0f;
        minZ = 0.0f;
        maxX = 415f;
        maxY = 70.0f;
        maxZ = 360f;
    }
}

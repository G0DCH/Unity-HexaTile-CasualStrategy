using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private Vector3 startMousePosition;

    private Vector3 currentMousePosition;

    private Vector3 startCameraPosition;

    public float moveSpeed = 0.01f;

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
        {
            startMousePosition = Input.mousePosition;
            startCameraPosition = transform.position;
        }
        else if(Input.GetMouseButton(1))
        {
            currentMousePosition = Input.mousePosition;

            Vector3 diff = startMousePosition - currentMousePosition;

            diff = new Vector3(diff.x, 0, diff.y);

            transform.position = startCameraPosition + diff * moveSpeed;
        }
    }
}
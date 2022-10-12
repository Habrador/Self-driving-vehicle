using UnityEngine;
using System.Collections;
using PathfindingForVehicles;

//Move the car we want to drive towards to the mouse position and rotate it
public class MoveTarget : MonoBehaviour
{
    //Fire a ray against an invisible infinite large plane, which is the ground
    //Is easier to use than a gameobject with a collider because we dont need to care about all other colliders
    //the ray may intersect with
    Plane groundPlane;

    Camera thisCamera;



    private void Start()
    {
        groundPlane = new Plane(Vector3.up, Vector3.zero);

        //Faster to cache the camera than using camera.main each update
        thisCamera = Camera.main;
    }



    void Update()
    {
        //The menu is active or we are above ui element
        if (SimController.current != null && !SimController.current.CanClick())
        {
            return;
        }

        //Move target all the time
        MoveTargeToMousePos();

        //Rotate the target around y
        RotateTarget();
    }



    //Move target all the time to the mouse pos
    void MoveTargeToMousePos()
    {
        Ray ray = thisCamera.ScreenPointToRay(Input.mousePosition);

        float rayDistance;

        //Intersect a ray with the plane
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            //Where did the ray hit the plane?
            Vector3 hitCoordinate = ray.GetPoint(rayDistance);

            //If we are within the grid, move the target to the new position
            int mapWidth = Parameters.mapWidth;

            if (hitCoordinate.x > 0f && hitCoordinate.x < mapWidth && hitCoordinate.z > 0f && hitCoordinate.z < mapWidth)
            {
                Transform carMouse = SimController.current.GetCarMouse();

                if (carMouse == null)
                {
                    return;
                }

                carMouse.position = hitCoordinate;
            }
        }
    }



    //Rotate target
    void RotateTarget()
    {
        float rotationSpeed = 100f;

        Transform carMouse = SimController.current.GetCarMouse();

        if (carMouse == null)
        {
            return;
        }

        //Rotate counter clock-wise
        if (Input.GetKey(KeyCode.Q))
        {
            carMouse.Rotate(-Vector3.up * Time.deltaTime * rotationSpeed);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            carMouse.Rotate(Vector3.up * Time.deltaTime * rotationSpeed);
        }
    }
}


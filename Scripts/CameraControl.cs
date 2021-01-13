using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{

    [SerializeField] private Camera cam;
    [SerializeField] private Transform target;
    private Vector3 previousPosition;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)){
            Vector3 direction = previousPosition - cam.ScreenToViewportPoint(Input.mousePosition);
            cam.transform.position = target.position;
            cam.transform.Rotate(new Vector3(1,0,0), direction.y * 180);
            cam.transform.Rotate(new Vector3(0,1,0), -direction.x * 180, Space.World);
            cam.transform.Translate(0,0,-8);

        }

        previousPosition = cam.ScreenToViewportPoint(Input.mousePosition);
    }
}

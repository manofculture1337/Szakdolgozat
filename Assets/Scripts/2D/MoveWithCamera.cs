using UnityEngine;



public class MoveWithCamera : MonoBehaviour
{
    public float distance = 0.5f;

    public bool isBoundingBoxVertex = false;

    private Camera _mainCamera;



    private void Start()
    {
        _mainCamera = Camera.main;

        transform.LookAt(_mainCamera.transform);
    }

    private void Update()
    {
        //transform.position = _mainCamera.transform.position + _mainCamera.transform.forward * distance;
        if (!isBoundingBoxVertex)
        {
            transform.LookAt(_mainCamera.transform);
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using Mirror;

public class ControlCamera : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private float _moveSpeed = 6f;

    [SerializeField]
    private float _rotateSpeed = 3f;

    [SerializeField]
    private float _zoomSpeed = 5f;

    private const float _minFov = 1f;

    private const float _maxFov = 150f;

    private bool _controlling = false;


    private void Update()
    {
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject())
        {
            _controlling = true;
        }
        else if (Input.GetMouseButtonUp(1))
        {
            _controlling = false;
        }

        Movement();
        Rotate();
        Zoom();
    }

    private void Movement()
    {
        if (_controlling)
        {
            if (Input.GetKey(KeyCode.W))
            {
                _camera.transform.position += _camera.transform.forward * Time.deltaTime * _moveSpeed;
            }

            if (Input.GetKey(KeyCode.A))
            {
                _camera.transform.position += -_camera.transform.right * Time.deltaTime * _moveSpeed;
            }

            if (Input.GetKey(KeyCode.S))
            {
                _camera.transform.position += -_camera.transform.forward * Time.deltaTime * _moveSpeed;
            }

            if (Input.GetKey(KeyCode.D))
            {
                _camera.transform.position += _camera.transform.right * Time.deltaTime * _moveSpeed;
            }

            if (Input.GetKey(KeyCode.Q))
            {
                _camera.transform.position += -_camera.transform.up * Time.deltaTime * _moveSpeed;
            }

            if (Input.GetKey(KeyCode.E))
            {
                _camera.transform.position += _camera.transform.up * Time.deltaTime * _moveSpeed;
            }
        }
    }

    private void Rotate()
    {
        if (_controlling)
        {
            _camera.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * _rotateSpeed, Space.World);
            _camera.transform.Rotate(Vector3.right, -Input.GetAxis("Mouse Y") * _rotateSpeed, Space.Self);
        }
    }

    private void Zoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0f && !EventSystem.current.IsPointerOverGameObject())
        {
            _camera.fieldOfView = Mathf.Clamp(_camera.fieldOfView - Input.GetAxis("Mouse ScrollWheel") * _camera.fieldOfView / _zoomSpeed, _minFov, _maxFov);
        }
    }
}
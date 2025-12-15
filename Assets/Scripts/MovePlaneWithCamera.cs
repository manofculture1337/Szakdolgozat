using UnityEngine;


public class MovePlaneWithCamera : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private float _forwardDistance = 4f;

    [SerializeField]
    private float _rightDistance = -4f;


    private void Update()
    {
        transform.position = _camera.transform.position + _camera.transform.forward * _forwardDistance + _camera.transform.right * _rightDistance;
        transform.rotation = Quaternion.LookRotation(-_camera.transform.right, -_camera.transform.forward);
    }
}

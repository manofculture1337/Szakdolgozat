using UnityEngine;


public class MoveCanvasWithCamera : MonoBehaviour
{
    [SerializeField]
    private Camera _camera;

    private float _forwardDistance = 4f;
    
    private float _rightDistance = -4f;


    private void Update()
    {
        transform.position = _camera.transform.position + _camera.transform.forward * _forwardDistance + _camera.transform.right * _rightDistance;
        transform.rotation = Quaternion.LookRotation(_camera.transform.forward, _camera.transform.up);
    }
}

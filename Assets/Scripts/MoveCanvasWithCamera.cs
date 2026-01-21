using UnityEngine;


public class MoveCanvasWithCamera : MonoBehaviour
{
    /*[SerializeField]*/
    private Camera _camera;

    [SerializeField]
    private float _forwardDistance = 4f;
    
    [SerializeField]
    private float _rightDistance = -3f;


    private void Start()
    {
        _camera = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
    }

    private void Update()
    {
        transform.position = _camera.transform.position + _camera.transform.forward * _forwardDistance + _camera.transform.right * _rightDistance;
        transform.rotation = Quaternion.LookRotation(_camera.transform.forward, _camera.transform.up);
    }
}

using UnityEngine;
using UnityEngine.UI;
using Mirror;


public class ChangeDimensions : MonoBehaviour
{
    [SerializeField]
    private Button _to2DButton;

    private GameObject _dispatcherCanvas;

    public GameObject drawCanvas;

    private Camera _mainCamera;

    private Camera _axesCamera;

    private Vector3 _originalCameraPos;

    private Quaternion _originalCameraRot;

    private float _originalCameraFOV;

    private bool _isIn2D = false;


    private void Start()
    {
        _dispatcherCanvas = GameObject.Find("PCCanvas");
        _mainCamera = Camera.main;
        _axesCamera = GameObject.Find("AxesCamera").GetComponent<Camera>();

        _to2DButton.onClick.AddListener(() => To2D());
    }

    private void To2D()
    {
        _originalCameraPos = _mainCamera.transform.position;
        _originalCameraRot = _mainCamera.transform.rotation;
        _originalCameraFOV = _mainCamera.fieldOfView;

        _mainCamera.transform.position = new Vector3(1000, 0, 0);
        _mainCamera.transform.eulerAngles = new Vector3(0, 90, 0);
        _mainCamera.fieldOfView = 60;
        _axesCamera.transform.position = new Vector3(1000, 0, 0);
        _axesCamera.transform.eulerAngles = new Vector3(0, 90, 0);
        _axesCamera.fieldOfView = 60;

        _mainCamera.GetComponent<ControlCamera>().enabled = false;
        _axesCamera.GetComponent<ControlCamera>().enabled = false;

        _dispatcherCanvas.SetActive(false);
        drawCanvas.SetActive(true);
        _isIn2D = true;
    }

    public void To3D()
    {
        _mainCamera.transform.position = _originalCameraPos;
        _mainCamera.transform.rotation = _originalCameraRot;
        _mainCamera.fieldOfView = _originalCameraFOV;
        _axesCamera.transform.position = _originalCameraPos;
        _axesCamera.transform.rotation = _originalCameraRot;
        _axesCamera.fieldOfView = _originalCameraFOV;

        _mainCamera.GetComponent<ControlCamera>().enabled = true;
        _axesCamera.GetComponent<ControlCamera>().enabled = true;

        drawCanvas.SetActive(false);
        _dispatcherCanvas.SetActive(true);
        _isIn2D = false;

        NetworkClient.connection.identity.GetComponent<DrawingManager>().SwitchedTo3D();
    }

    public bool IsIn2D()
    {
        return _isIn2D;
    }
}

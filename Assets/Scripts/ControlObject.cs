using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Mirror;
using Oculus.Interaction;


public enum TransformMode
{
    None,
    Move,
    Rotate,
    Scale
}

public enum GameObjectType
{
    ExclamationMark,
    QuestionMark,
    Arrow,
    Flag,
    FreeLine
}


public class ControlObject : MonoBehaviour
{
    [SerializeField]
    private GameObject _movePrefab;

    [SerializeField] 
    private GameObject _rotatePrefab;

    [SerializeField] 
    private GameObject _scalePrefab;

    [SerializeField]
    private Material _xAxisMaterial;

    [SerializeField] 
    private Material _yAxisMaterial;

    [SerializeField]
    private Material _zAxisMaterial;

    [SerializeField]
    private Material _selectedAxisMaterial;

    [SerializeField]
    private GameObject _exclamationMarkPrefab;

    [SerializeField]
    private GameObject _questionMarkPrefab;

    [SerializeField]
    private GameObject _arrowPrefab;

    [SerializeField]
    private GameObject _flagPrefab;

    [SerializeField] 
    private GameObject _freeLinePrefab;

    [SerializeField]
    private GameObject _gameObjectButtons;

    [SerializeField]
    private Transform _scrollViewContent;

    [SerializeField]
    private GameObject _scrollViewItem;

    [SerializeField]
    private Button _deleteButton;

    [SerializeField]
    private GameObject _objectData;

    [SerializeField]
    private FlexibleColorPicker _colorPicker;

    public RayInteractor rightInteractor;

    private GameObject _xAxis = null;

    private GameObject _yAxis = null;

    private GameObject _zAxis = null;

    private GameObject _selectedAxis = null;
    
    private GameObject _selectedObject = null;

    private Button _exclamationMarkButton;

    private Button _questionMarkButton;

    private Button _arrowButton;

    private Button _flagButton;

    private Button _freeLineButton;

    private TMP_InputField _nameInput;

    private TMP_InputField _xPosInput;

    private TMP_InputField _yPosInput;

    private TMP_InputField _zPosInput;

    private TMP_InputField _xRotInput;

    private TMP_InputField _yRotInput;

    private TMP_InputField _zRotInput;

    private TMP_InputField _xScaleInput;

    private TMP_InputField _yScaleInput;

    private TMP_InputField _zScaleInput;

    private TransformMode _currentMode = TransformMode.None;
    public TransformMode CurrentMode 
    {
        get => _currentMode;
        set 
        {
            _currentMode = value;

            if (_selectedObject != null)
            {
                InstantiateAxes();
            }
        } 
    }

    private bool _transforming = false;

    private bool _intentionalSelect = false;

    private LineRenderer _line = null;

    private bool _drawing = false;

    private LayerMask _axisLayerMask;

    private LayerMask _spawnableObjectLayerMask;

    private RaycastHit _hit;

    private Camera _mainCam;

    private IInput _input;

    private ChangeDimensions _changeDimensions;


    private void Start()
    {

#if SERVER_BUILD
        SetUpInputFields();
        _input = new PCInput();

        return;
#endif

#if PC_BUILD
        _input = new PCInput();
        _changeDimensions = GameObject.FindGameObjectWithTag("Dispatcher").GetComponent<ChangeDimensions>();
#endif

#if VR_BUILD
        _input = new VRInput(rightInteractor);
#endif

        _axisLayerMask = LayerMask.GetMask("Axis");
        _spawnableObjectLayerMask = LayerMask.GetMask("SpawnableObject");
        _mainCam = Camera.main;

        SetUpButtons();
        SetUpInputFields();
        
        _colorPicker.onColorChange.AddListener((color) => RecolorSelected(color));
    }

    private void Update()
    {

#if SERVER_BUILD
        return;
#endif

#if PC_BUILD
        if (_changeDimensions.IsIn2D())
        {
            return;
        }

        if (Input.GetKey(KeyCode.Delete))
        {
            DeleteGameObject();
        }
#endif

        if (!_input.GetSecondaryClick())
        {
            _input.CheckKeyForModeSwitch(this);
        }

        if (_input.GetClickDown() && !_input.IsOverUI())
        {
            if (Physics.Raycast(_input.GetRay(_mainCam), out _hit, Mathf.Infinity, _axisLayerMask))
            {
                _transforming = true;
                _selectedAxis = _hit.transform.gameObject;
                ColorAxis(_selectedAxis, _selectedAxisMaterial);
            }
            else if (_line != null)
            {
                _drawing = true;
            }

            return;
        }
        else if (_input.GetClick())
        {
            if (_transforming)
            {
                Transform();
            }
            else if (_drawing)
            {
                Vector3 inputPosWorld = _input.GetWorldPosition(_mainCam);
                if (_line.positionCount > 0 && Vector3.Distance(_line.GetPosition(_line.positionCount - 1), inputPosWorld) < 0.01f)
                {
                    return;
                }

                NetworkClient.connection.identity.GetComponent<NetworkedObjectControl>().CmdNewPointForFreeLine(_selectedObject, inputPosWorld);
            }

            return;
        }
        else if (_input.GetClickUp())
        {
            if (_transforming)
            {
                ColorAxis(_selectedAxis,
                          _selectedAxis == _xAxis ? _xAxisMaterial :
                          _selectedAxis == _yAxis ? _yAxisMaterial :
                                                    _zAxisMaterial);
                _selectedAxis = null;
                _transforming = false;

                return;
            }
            else if (_drawing)
            {
                _drawing = false;

                return;
            }

            if (Physics.Raycast(_input.GetRay(_mainCam), out _hit, Mathf.Infinity, _spawnableObjectLayerMask))
            {
                _intentionalSelect = true;
                TrySelectObject(_hit.transform.root.gameObject);

                return;
            }
            else if (!_input.IsOverUI())
            {
                DeselectSelectedObject();

                return;
            }
        }

        _input.SetPrevPosition();
    }

    private void Transform()
    {
        switch (CurrentMode)
        {
            default:
                break;
            case TransformMode.Move:
                Move();
                break;
            case TransformMode.Rotate:
                Rotate();
                break;
            case TransformMode.Scale:
                Scale();
                break;
        }
    }

    private Vector3 GetWorldAxis(GameObject selectedAxis)
    {
        if (_selectedAxis == _xAxis)
        {
            return Vector3.right;
        }
        else if (_selectedAxis == _yAxis)
        {
            return Vector3.up;
        }
        else
        {
            return Vector3.forward;
        }
    }

    private void Move()
    {
        Vector3 axisDirWorld = GetWorldAxis(_selectedAxis);
        Vector3 dir = Vector3.zero;
        float scale = 0f;

#if PC_BUILD
        Vector3 objectPosWorld = _selectedObject.transform.position;
        Vector3 axisEndWorld = objectPosWorld + axisDirWorld;

        Vector2 axisDirScreen = (_mainCam.WorldToScreenPoint(axisEndWorld) - _mainCam.WorldToScreenPoint(objectPosWorld)).normalized;
        dir = axisDirScreen;
        scale = 0.01f;
#endif

#if VR_BUILD
        dir = axisDirWorld;
        scale = 2.5f;
#endif

        float axisDelta = Vector3.Dot(_input.GetDelta(), dir);
        Vector3 movementWorld = axisDirWorld * axisDelta * scale;
        _input.SetPrevPosition();

        _selectedObject.transform.position += movementWorld;
        _xAxis.transform.position += movementWorld;
        _yAxis.transform.position += movementWorld;
        _zAxis.transform.position += movementWorld;

        _xPosInput.text = _selectedObject.transform.position.x.ToString();
        _yPosInput.text = _selectedObject.transform.position.y.ToString();
        _zPosInput.text = _selectedObject.transform.position.z.ToString();
    }

    private void Rotate()
    {
        Vector3 axisDirWorld = GetWorldAxis(_selectedAxis);
        float angle = 0f;

#if PC_BUILD
        Vector3 objectPosScreen = _mainCam.WorldToScreenPoint(_selectedObject.transform.position);

        Vector2 previous = _input.GetPos() - _input.GetDelta() - objectPosScreen;
        Vector2 current = _input.GetPos() - objectPosScreen;

        float angleDelta = Vector2.SignedAngle(previous, current);
        float direction = Mathf.Sign(Vector3.Dot(_mainCam.transform.forward, axisDirWorld));
        angle = direction * angleDelta;
#endif

#if VR_BUILD
        angle = Mathf.Max(Vector3.Dot(_input.GetDelta(), Vector3.right), Vector3.Dot(_input.GetDelta(), Vector3.forward));
#endif

        _selectedObject.transform.rotation = Quaternion.AngleAxis(angle, axisDirWorld) * _selectedObject.transform.rotation;

        _xRotInput.text = _selectedObject.transform.eulerAngles.x.ToString();
        _yRotInput.text = _selectedObject.transform.eulerAngles.y.ToString();
        _zRotInput.text = _selectedObject.transform.eulerAngles.z.ToString();
    }

    private void Scale()
    {
        Vector3 axisDirWorld = GetWorldAxis(_selectedAxis);
        Vector3 dir = Vector3.zero;
        float scale = 0f;

#if PC_BUILD
        Vector3 objectPosWorld = _selectedObject.transform.position;
        Vector3 axisEndWorld = objectPosWorld + axisDirWorld;

        Vector2 axisDirScreen = (_mainCam.WorldToScreenPoint(axisEndWorld) - _mainCam.WorldToScreenPoint(objectPosWorld)).normalized;
        dir = axisDirScreen;
        scale = 0.01f;
#endif

#if VR_BUILD
        dir = axisDirWorld;
        scale = 2.5f;
#endif

        float axisDelta = Vector2.Dot(_input.GetDelta(), dir);
        _selectedObject.transform.localScale += axisDirWorld * axisDelta * scale;
        _input.SetPrevPosition();

        _xScaleInput.text = _selectedObject.transform.localScale.x.ToString();
        _yScaleInput.text = _selectedObject.transform.localScale.y.ToString();
        _zScaleInput.text = _selectedObject.transform.localScale.z.ToString();
    }

    private void InstantiateAxes()
    {
        DeleteAxes();

        switch (CurrentMode)
        {
            default:
                return;
            case TransformMode.Move:
                _xAxis = Instantiate(_movePrefab, _selectedObject.transform.position, Quaternion.Euler(0, 0, -90));
                _yAxis = Instantiate(_movePrefab, _selectedObject.transform.position, Quaternion.Euler(0, 0, 0));
                _zAxis = Instantiate(_movePrefab, _selectedObject.transform.position, Quaternion.Euler(90, 0, 0));
                break;
            case TransformMode.Rotate:
                _xAxis = Instantiate(_rotatePrefab, _selectedObject.transform.position, Quaternion.Euler(0, 0, -90));
                _yAxis = Instantiate(_rotatePrefab, _selectedObject.transform.position, Quaternion.Euler(0, 0, 0));
                _zAxis = Instantiate(_rotatePrefab, _selectedObject.transform.position, Quaternion.Euler(90, 0, 0));
                break;
            case TransformMode.Scale:
                _xAxis = Instantiate(_scalePrefab, _selectedObject.transform.position, _selectedObject.transform.rotation * Quaternion.Euler(0, 0, -90));
                _yAxis = Instantiate(_scalePrefab, _selectedObject.transform.position, _selectedObject.transform.rotation * Quaternion.Euler(0, 0, 0));
                _zAxis = Instantiate(_scalePrefab, _selectedObject.transform.position, _selectedObject.transform.rotation * Quaternion.Euler(90, 0, 0));
                break;
        }

        ColorAxis(_xAxis, _xAxisMaterial);
        ColorAxis(_yAxis, _yAxisMaterial);
        ColorAxis(_zAxis, _zAxisMaterial);
    }

    private void DeleteAxes()
    {
        if (_xAxis != null)
        {
            Destroy(_xAxis);
            Destroy(_yAxis);
            Destroy(_zAxis);

            _xAxis = null;
            _yAxis = null;
            _zAxis = null;
        }
    }

    private void ColorAxis(GameObject axis, Material material)
    {
        switch (CurrentMode)
        {
            default:
                break;
            case TransformMode.Move:
            case TransformMode.Scale:
                axis.transform.GetChild(0).GetComponent<Renderer>().material = material;
                axis.transform.GetChild(1).GetComponent<Renderer>().material = material;
                break;
            case TransformMode.Rotate:
                axis.GetComponent<Renderer>().material = material;
                break;
        }
    }

    public void TrySelectObject(GameObject selectedObject)
    {
        if (_selectedObject == selectedObject || !_intentionalSelect)
        {
            return;
        }

        _intentionalSelect = false;
        selectedObject.GetComponent<ObjectHandle>().RequestAuthority();
    }

    public void SelectObject(GameObject selectedObject)
    {
        if (_selectedObject != null)
        {
            DeselectSelectedObject();
        }

        _selectedObject = selectedObject;
        _selectedObject.TryGetComponent<LineRenderer>(out _line);
        InstantiateAxes();
        MakeInteractable();
    }

    private void DeselectSelectedObject()
    {
        if (_selectedObject == null)
        {
            return;
        }

        _selectedObject.GetComponent<ObjectHandle>().ReleaseAuthority();
        _selectedObject = null;
        _line = null;
        DeleteAxes();
        MakeNonInteractable();
    }

    private void NewGameObject(GameObject prefab, GameObjectType type)
    {
        _intentionalSelect = true;

        NetworkedObjectControl networkedObjectControl = NetworkClient.connection.identity.GetComponent<NetworkedObjectControl>();
        networkedObjectControl.CmdSetNextObjectType(type);
        networkedObjectControl.CmdNewObject();
    }

    private void DeleteGameObject()
    {
        if (_selectedObject == null)
        {
            return;
        }

        NetworkClient.connection.identity.GetComponent<NetworkedObjectControl>().CmdDestroyObject(_selectedObject);
    }

    public void RegisterObject(GameObject obj)
    {
        GameObject listItem = Instantiate(_scrollViewItem, _scrollViewContent);
        TextMeshProUGUI listItemText = listItem.GetComponentInChildren<TextMeshProUGUI>();
        listItem.GetComponentInChildren<GameObjectListItemHandle>().SetGameObject(obj);
        obj.GetComponent<ObjectHandle>().SetListItem(listItemText);
    }

    public void DeregisterObject(GameObject obj)
    {
        Destroy(obj.GetComponent<ObjectHandle>().listItem.transform.gameObject);

        if (_selectedObject == obj)
        {
            DeselectSelectedObject();
        }
    }

    private void MakeInteractable()
    {
        _deleteButton.interactable = true;

        _nameInput.interactable = true;
        _nameInput.text = _selectedObject.name;

        Vector3 pos = _selectedObject.transform.position;
        _xPosInput.interactable = true;
        _xPosInput.text = pos.x.ToString();
        _yPosInput.interactable = true;
        _yPosInput.text = pos.y.ToString();
        _zPosInput.interactable = true;
        _zPosInput.text = pos.z.ToString();

        Vector3 rot = _selectedObject.transform.eulerAngles;
        _xRotInput.interactable = true;
        _xRotInput.text = rot.x.ToString();
        _yRotInput.interactable = true;
        _yRotInput.text = rot.y.ToString();
        _zRotInput.interactable = true;
        _zRotInput.text = rot.z.ToString();

        Vector3 scale = _selectedObject.transform.localScale;
        _xScaleInput.interactable = true;
        _xScaleInput.text = scale.x.ToString();
        _yScaleInput.interactable = true;
        _yScaleInput.text = scale.y.ToString();
        _zScaleInput.interactable = true;
        _zScaleInput.text = scale.z.ToString();
    }

    private void MakeNonInteractable()
    {
        _deleteButton.interactable = false;

        _nameInput.interactable = false;
        _nameInput.text = "";

        _xPosInput.interactable = false;
        _xPosInput.text = "";
        _yPosInput.interactable = false;
        _yPosInput.text = "";
        _zPosInput.interactable = false;
        _zPosInput.text = "";

        _xRotInput.interactable = false;
        _xRotInput.text = "";
        _yRotInput.interactable = false;
        _yRotInput.text = "";
        _zRotInput.interactable = false;
        _zRotInput.text = "";

        _xScaleInput.interactable = false;
        _xScaleInput.text = "";
        _yScaleInput.interactable = false;
        _yScaleInput.text = "";
        _zScaleInput.interactable = false;
        _zScaleInput.text = "";
    }

    private void SetUpButtons()
    {
        _exclamationMarkButton = _gameObjectButtons.transform.GetChild(0).GetComponent<Button>();
        _questionMarkButton = _gameObjectButtons.transform.GetChild(1).GetComponent<Button>();
        _arrowButton = _gameObjectButtons.transform.GetChild(2).GetComponent<Button>();
        _flagButton = _gameObjectButtons.transform.GetChild(3).GetComponent<Button>();
        _freeLineButton = _gameObjectButtons.transform.GetChild(4).GetComponent<Button>();

        _exclamationMarkButton.onClick.AddListener(() => NewGameObject(_exclamationMarkPrefab, GameObjectType.ExclamationMark));
        _questionMarkButton.onClick.AddListener(() => NewGameObject(_questionMarkPrefab, GameObjectType.QuestionMark));
        _arrowButton.onClick.AddListener(() => NewGameObject(_arrowPrefab, GameObjectType.Arrow));
        _flagButton.onClick.AddListener(() => NewGameObject(_flagPrefab, GameObjectType.Flag));
        _freeLineButton.onClick.AddListener(() => NewGameObject(_freeLinePrefab, GameObjectType.FreeLine));

        _deleteButton.onClick.AddListener(() => DeleteGameObject());
    }

    private void SetUpInputFields()
    {
        _nameInput = _objectData.transform.GetChild(0).GetComponentInChildren<TMP_InputField>();

        Transform transform = _objectData.transform.GetChild(1);
        Transform position = transform.GetChild(0);
        Transform rotation = transform.GetChild(1);
        Transform scale = transform.GetChild(2);

        _xPosInput = position.GetChild(1).GetComponentInChildren<TMP_InputField>();
        _yPosInput = position.GetChild(2).GetComponentInChildren<TMP_InputField>();
        _zPosInput = position.GetChild(3).GetComponentInChildren<TMP_InputField>();

        _xRotInput = rotation.GetChild(1).GetComponentInChildren<TMP_InputField>();
        _yRotInput = rotation.GetChild(2).GetComponentInChildren<TMP_InputField>();
        _zRotInput = rotation.GetChild(3).GetComponentInChildren<TMP_InputField>();

        _xScaleInput = scale.GetChild(1).GetComponentInChildren<TMP_InputField>();
        _yScaleInput = scale.GetChild(2).GetComponentInChildren<TMP_InputField>();
        _zScaleInput = scale.GetChild(3).GetComponentInChildren<TMP_InputField>();

        _nameInput.onEndEdit.AddListener((value) => 
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _nameInput.text = _selectedObject.name;

                return;
            }

            _selectedObject.GetComponent<ObjectHandle>().ChangeName(value);
        });

        _xPosInput.onValueChanged.AddListener((value) => 
        {
            XChanged(value, TransformMode.Move);
        });

        _yPosInput.onValueChanged.AddListener((value) => 
        {
            YChanged(value, TransformMode.Move);
        });

        _zPosInput.onValueChanged.AddListener((value) => 
        {
            ZChanged(value, TransformMode.Move);
        });

        _xRotInput.onValueChanged.AddListener((value) => 
        {
            XChanged(value, TransformMode.Rotate);
        });

        _yRotInput.onValueChanged.AddListener((value) => 
        {
            YChanged(value, TransformMode.Rotate);
        });

        _zRotInput.onValueChanged.AddListener((value) => 
        {
            ZChanged(value, TransformMode.Rotate);
        });

        _xScaleInput.onValueChanged.AddListener((value) =>
        {   
            XChanged(value, TransformMode.Scale);
        });

        _yScaleInput.onValueChanged.AddListener((value) =>
        {
            YChanged(value, TransformMode.Scale);
        });

        _zScaleInput.onValueChanged.AddListener((value) =>
        {
            ZChanged(value, TransformMode.Scale);
        });

        MakeNonInteractable();
    }

    private void XChanged(string value, TransformMode mode)
    {
        if (_selectedObject == null || !float.TryParse(value, out float result))
        {
            return;
        }

        switch (mode)
        {
            default:
                break;
            case TransformMode.Move:
                _selectedObject.transform.position = new Vector3(result, _selectedObject.transform.position.y, _selectedObject.transform.position.z);
                if (CurrentMode != TransformMode.None)
                {
                    Vector3 pos = _selectedObject.transform.position;
                    _xAxis.transform.position = new Vector3(result, pos.y, pos.z);
                    _yAxis.transform.position = new Vector3(result, pos.y, pos.z);
                    _zAxis.transform.position = new Vector3(result, pos.y, pos.z);
                }
                break;
            case TransformMode.Rotate:
                _selectedObject.transform.eulerAngles = new Vector3(result, _selectedObject.transform.eulerAngles.y, _selectedObject.transform.eulerAngles.z);
                if (CurrentMode == TransformMode.Scale)
                {
                    Vector3 rot = _selectedObject.transform.eulerAngles;
                    _xAxis.transform.rotation = Quaternion.Euler(result, rot.y, rot.z) * Quaternion.Euler(0, 0, -90);
                    _yAxis.transform.rotation = Quaternion.Euler(result, rot.y, rot.z) * Quaternion.Euler(0, 0, 0);
                    _zAxis.transform.rotation = Quaternion.Euler(result, rot.y, rot.z) * Quaternion.Euler(90, 0, 0);
                }
                break;
            case TransformMode.Scale:
                _selectedObject.transform.localScale = new Vector3(result, _selectedObject.transform.localScale.y, _selectedObject.transform.localScale.z);
                break;
        }
    }

    private void YChanged(string value, TransformMode mode)
    {
        if (_selectedObject == null || !float.TryParse(value, out float result))
        {
            return;
        }

        switch (mode)
        {
            default:
                break;
            case TransformMode.Move:
                _selectedObject.transform.position = new Vector3(_selectedObject.transform.position.x, result, _selectedObject.transform.position.z);
                if (CurrentMode != TransformMode.None)
                {
                    Vector3 pos = _selectedObject.transform.position;
                    _xAxis.transform.position = new Vector3(pos.x, result, pos.z);
                    _yAxis.transform.position = new Vector3(pos.x, result, pos.z);
                    _zAxis.transform.position = new Vector3(pos.x, result, pos.z);
                }
                break;
            case TransformMode.Rotate:
                _selectedObject.transform.eulerAngles = new Vector3(_selectedObject.transform.eulerAngles.x, result, _selectedObject.transform.eulerAngles.z);
                if (CurrentMode == TransformMode.Scale)
                {
                    Vector3 rot = _selectedObject.transform.eulerAngles;
                    _xAxis.transform.rotation = Quaternion.Euler(rot.x, result, rot.z) * Quaternion.Euler(0, 0, -90);
                    _yAxis.transform.rotation = Quaternion.Euler(rot.x, result, rot.z) * Quaternion.Euler(0, 0, 0);
                    _zAxis.transform.rotation = Quaternion.Euler(rot.x, result, rot.z) * Quaternion.Euler(90, 0, 0);
                }
                break;
            case TransformMode.Scale:
                _selectedObject.transform.localScale = new Vector3(_selectedObject.transform.localScale.x, result, _selectedObject.transform.localScale.z);
                break;
        }
    }

    private void ZChanged(string value, TransformMode mode)
    {
        if (_selectedObject == null || !float.TryParse(value, out float result))
        {
            return;
        }

        switch (mode)
        {
            default:
                break;
            case TransformMode.Move:
                _selectedObject.transform.position = new Vector3(_selectedObject.transform.position.x, _selectedObject.transform.position.y, result);
                if (CurrentMode != TransformMode.None)
                {
                    Vector3 pos = _selectedObject.transform.position;
                    _xAxis.transform.position = new Vector3(pos.x, pos.y, result);
                    _yAxis.transform.position = new Vector3(pos.x, pos.y, result);
                    _zAxis.transform.position = new Vector3(pos.x, pos.y, result);
                }
                break;
            case TransformMode.Rotate:
                _selectedObject.transform.eulerAngles = new Vector3(_selectedObject.transform.eulerAngles.x, _selectedObject.transform.eulerAngles.y, result);
                if (CurrentMode == TransformMode.Scale)
                {
                    Vector3 rot = _selectedObject.transform.eulerAngles;
                    _xAxis.transform.rotation = Quaternion.Euler(rot.x, rot.y, result) * Quaternion.Euler(0, 0, -90);
                    _yAxis.transform.rotation = Quaternion.Euler(rot.x, rot.y, result) * Quaternion.Euler(0, 0, 0);
                    _zAxis.transform.rotation = Quaternion.Euler(rot.x, rot.y, result) * Quaternion.Euler(90, 0, 0);
                }
                break;
            case TransformMode.Scale:
                _selectedObject.transform.localScale = new Vector3(_selectedObject.transform.localScale.x, _selectedObject.transform.localScale.y, result);
                break;
        }
    }

    public void SetIntentional()
    {
        _intentionalSelect = true;
    }

    private void RecolorSelected(Color color)
    {
        if (_selectedObject != null)
        {
            _selectedObject.GetComponent<ObjectHandle>().SetObjectColor(color);
        }
    }
}
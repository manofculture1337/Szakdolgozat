using Mirror;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[System.Serializable]
public struct LineData
{
    public List<Vector3> positions;
    public Vector4 color;
    public float size;
    public Vector3 forward;
    public bool isLooped;
}

[System.Serializable]
public struct LineDataList
{
    public List<LineData> lines;
}

public class Drawing
{
    public List<LineRenderer> lines = new List<LineRenderer>();
    public Vector3 forward;

    public LineDataList lineDataList;

    public string ToString(Vector3 markerPosition)
    {
        var lineDatas = new List<LineData>();
        foreach (var line in lines)
        {
            var linePositions = new Vector3[line.positionCount];
            line.GetPositions(linePositions);
            lineDatas.Add(new LineData
            {
                positions = linePositions.Select(pos => pos - markerPosition).ToList(),
                color = line.startColor,
                size = line.startWidth,
                forward = forward,
                isLooped = line.loop
            });
        }

        return JsonUtility.ToJson(new LineDataList { lines = lineDatas });
    }
}

public class DrawingManager : NetworkBehaviour
{
    public float minDistance = 0.01f;

    public GameObject linePrefab;

    public GameObject drawCanvas;

    public GameObject boundingBoxVertexPrefab;

    public GameObject plane;

    public Camera tempCamera;

    public GameObject tempPlane;

    private List<Drawing> _drawings = new List<Drawing>();

    private List<(List<Vector2> positions, (Color color, float size) brush, (float width, float height) screen, (float x, float y) offsets)> _drawingDataWhenFrozen = new List<(List<Vector2>, (Color, float), (float, float), (float, float))>();

    private List<GameObject> _shapes = new List<GameObject>();

    private List<GameObject> _boundingBoxVertexObjects = new List<GameObject>();

    private List<GameObject> _linesAndShapes = new List<GameObject>();

    private Drawing _currentDrawing;

    private LineRenderer _currentLine;

    private GameObject _currentShape;

    private GameObject _previousShape;

    private Vector2 _previousPosition;

    private bool _lastActionWasShape;

    private bool _clickedOnDrawingField;

    private RaycastHit _hit;

    private LayerMask _layerMask;

    private GameObject _plane;

    //private GameObject _lineHolder;

    private const float _drawCanvasDistance = 3f;

    private Freeze _freeze;

    private UndoDelete _undoDelete;

    private BrushSettings _brushSettings;

    private Camera _tempCamera;

    private GameObject _tempPlane;

    private LayerMask _tempLayerMask;

    private RectTransform _drawAreaRectTransform;

    private TextMeshProUGUI _freezeButtonText;

    private Button _doneButton;

    private Toggle _instantSendToggle;

    private GameObject _shapeButtons;

    //private WebRTCManager _webRTCManager;
    
    private Camera _streamingCamera;

    private Button _startStreamButton;

    //private StepManager _stepManager;

    private bool _isDispatcher;

    private ChangeDimensions _changeDimensions;


    private void Start()
    {
        if (isClient)
        {
            _isDispatcher = GameObject.FindGameObjectWithTag("Dispatcher") != null;
        }

        if (isLocalPlayer)
        {
            var canvas = Instantiate(drawCanvas);
            canvas.GetComponent<Canvas>().worldCamera = Camera.main;
            canvas.GetComponent<Canvas>().planeDistance = _drawCanvasDistance;
            _freeze = canvas.GetComponentInChildren<Freeze>();
            _undoDelete = canvas.GetComponentInChildren<UndoDelete>();
            _brushSettings = canvas.GetComponentInChildren<BrushSettings>();
            _brushSettings.OnBrushColorChanged += OnBrushColorChangedHandler;
            _brushSettings.OnBrushSizeChanged += OnBrushSizeChangedHandler;
            _shapeButtons = canvas.GetNamedChild("ShapeButtons");
            _shapeButtons.GetComponent<ShapeFactory>().OnShapeCreated += NewShape;
            _shapeButtons.SetActive(false);
            _drawAreaRectTransform = canvas.GetNamedChild("DrawAreaCanvas").GetComponent<RectTransform>();
            _freezeButtonText = canvas.GetNamedChild("FreezeButton").GetComponentInChildren<TextMeshProUGUI>();
            _doneButton = canvas.GetNamedChild("DoneButton").GetComponent<Button>();
            _doneButton.interactable = false;
            _instantSendToggle = canvas.GetNamedChild("InstantSendToggle").GetComponent<Toggle>();
            _instantSendToggle.onValueChanged.AddListener(value =>
            {
                if (value == true)
                {
                    _brushSettings.BrushColor = _brushSettings.BrushColor;
                    _brushSettings.BrushSize = _brushSettings.BrushSize;
                }
            });
            _instantSendToggle.interactable = false;

            if (_isDispatcher)
            {
                _changeDimensions = GameObject.FindGameObjectWithTag("Dispatcher").GetComponent<ChangeDimensions>();
                _changeDimensions.drawCanvas = canvas;
                canvas.GetNamedChild("3DButton").GetComponent<Button>().onClick.AddListener(() => _changeDimensions.To3D());
            }
            else
            {
                canvas.SetActive(false);
            }
        }

        /*if (!isServer)
        {
            canvas.SetActive(false);
            //_webRTCManager = WebRTCManager.Instance;
            _streamingCamera = GameObject.Find("StreamingCamera").GetComponent<Camera>();
            _startStreamButton = GameObject.Find("StartStreamButton").GetComponent<Button>();
            //_startStreamButton.onClick.AddListener(StartStream);
            //_stepManager = GameObject.Find("StepManager").GetComponent<StepManager>();
            //GameObject.Find("TutorialCanvasManager").GetComponent<TutorialCanvasManager>().OnMarkerDetected += markerPos => Instantiate(plane, markerPos, Quaternion.identity);
        }
        else if (isServer && !isLocalPlayer)
        {
            canvas.SetActive(false);
        }
        else if (isServer)
        {
            GameObject.Find("StreamerUI").SetActive(false);
        }*/

        _layerMask = LayerMask.GetMask("Draw");
        _tempLayerMask = LayerMask.GetMask("TempDraw");

        //_plane = GameObject.Find("PlaneStore");
        _plane = GameObject.Find("Plane")?.transform.parent.gameObject;
        //_lineHolder = GameObject.Find("LineHolder");

        NewDrawing();
        NewLine();
    }

    private void Update()
    {
        if (_plane == null)
        {
            Debug.Log("PlaneStore not found, trying to find it again.");
            //_plane = GameObject.Find("PlaneStore");
            _plane = GameObject.Find("Plane")?.transform.parent.gameObject;
        }

        if (!_isDispatcher && isLocalPlayer)
        {
            CheckAngle();
        }

        if (!_isDispatcher || !isLocalPlayer)
        {
            return;
        }

        if (!_changeDimensions.IsIn2D())
        {
            return;
        }

        /*if (isServer)
        {*/
            if (_freeze.IsReady)
            {
                _freeze.IsReady = false;

                if (!_freeze.IsFrozen)
                {
                    _freeze.IsFrozen = true;
                    _freezeButtonText.text = "Unfreeze";
                    _doneButton.interactable = true;
                    _instantSendToggle.interactable = true;
                    _shapeButtons.SetActive(true);

                    //RpcFreeze();
                    CmdFreeze();

                    return;
                }
                else
                {
                    _freeze.IsFrozen = false;
                    _freezeButtonText.text = "Freeze";
                    _doneButton.interactable = false;
                    _instantSendToggle.interactable = false;
                    _shapeButtons.SetActive(false);

                    foreach (var line in _currentDrawing.lines)
                    {
                        if (line != null)
                        {
                            Destroy(line.gameObject);
                        }
                    }

                    foreach (var shape in _shapes)
                    {
                        Destroy(shape);
                    }
                    _shapes.Clear();
                    _currentShape = null;
                    _previousShape = null;
                    _lastActionWasShape = false;

                    foreach (var gameObject in _boundingBoxVertexObjects)
                    {
                        Destroy(gameObject);
                    }
                    _boundingBoxVertexObjects.Clear();

                    NewDrawing();
                    NewLine();

                    _drawingDataWhenFrozen.Clear();
                    _linesAndShapes.Clear();

                    //RpcEndDraw();
                    CmdEndDraw();
                    //RpcUnfreeze();
                    CmdUnfreeze();

                    return;
                }
            }

            if (_freeze.IsFrozen)
            {
                _instantSendToggle.interactable = _drawingDataWhenFrozen.Count == 0 || (_drawingDataWhenFrozen.Count == 1 && _drawingDataWhenFrozen[0].positions.Count == 0);

                if (_undoDelete.ShouldUndo)
                {
                    _undoDelete.ShouldUndo = false;

                    if (_freeze.InstantSend)
                    {
                        //RpcUndo(true);
                        CmdUndo(true);
                    }

                    if (_linesAndShapes.Count == 0)
                    {
                        return;
                    }

                    for (int i = _linesAndShapes.Count - 1; i >= 0; i--)
                    {
                        if (_linesAndShapes[i].GetComponent<LineRenderer>().positionCount != 0)
                        {
                            if (_lastActionWasShape)
                            {
                                _lastActionWasShape = false;
                                _currentShape.GetComponent<Shape>().Finish();
                            }

                            Destroy(_linesAndShapes[i]);
                            _linesAndShapes.RemoveAt(i);
                            _drawingDataWhenFrozen.RemoveAt(i);

                            break;
                        }
                    }

                    return;
                }

                if (_undoDelete.ShouldDelete)
                {
                    _undoDelete.ShouldDelete = false;

                    if (_freeze.InstantSend)
                    {
                        //RpcDelete(true);
                        CmdDelete(true);
                    }

                    foreach (var drawing in _drawings)
                    {
                        foreach (var line in drawing.lines)
                        {
                            if (line != null)
                            {
                                Destroy(line.gameObject);
                            }
                        }
                    }
                    _drawings.Clear();
                    _currentDrawing = null; 
                    _currentLine = null;

                    foreach (var shape in _shapes)
                    {
                        Destroy(shape);
                    }
                    _shapes.Clear();
                    _currentShape = null;
                    _previousShape = null;
                    _lastActionWasShape = false;

                    foreach (var gameObject in _boundingBoxVertexObjects)
                    {
                        Destroy(gameObject);
                    }
                    _boundingBoxVertexObjects.Clear();

                    _drawingDataWhenFrozen.Clear();
                    _linesAndShapes.Clear();

                    NewDrawing();
                    NewLine();

                    return;
                }

                if (_freeze.IsDone)
                {
                    _freeze.IsDone = false;

                    if (!_freeze.InstantSend)
                    {
                        /*RpcDrawFrozen(_drawingDataWhenFrozen.ConvertAll(x => x.positions),
                                      _drawingDataWhenFrozen.ConvertAll(x => x.brush.color),
                                      _drawingDataWhenFrozen.ConvertAll(x => x.brush.size),
                                      _drawingDataWhenFrozen.ConvertAll(x => x.screen.width),
                                      _drawingDataWhenFrozen.ConvertAll(x => x.screen.height),
                                      _drawingDataWhenFrozen.ConvertAll(x => x.offsets.x),
                                      _drawingDataWhenFrozen.ConvertAll(x => x.offsets.y));*/
                        CmdDrawFrozen(_drawingDataWhenFrozen.ConvertAll(x => x.positions),
                                    _drawingDataWhenFrozen.ConvertAll(x => x.brush.color),
                                    _drawingDataWhenFrozen.ConvertAll(x => x.brush.size),
                                    _drawingDataWhenFrozen.ConvertAll(x => x.screen.width),
                                    _drawingDataWhenFrozen.ConvertAll(x => x.screen.height),
                                    _drawingDataWhenFrozen.ConvertAll(x => x.offsets.x),
                                    _drawingDataWhenFrozen.ConvertAll(x => x.offsets.y));
                    }
                    /*else
                    {
                        RpcAddDrawingStep();
                    }*/

                    foreach (var line in _currentDrawing.lines)
                    {
                        line?.gameObject?.SetActive(false);
                    }

                    foreach (var shape in _shapes)
                    {
                        shape?.SetActive(false);
                    }
                    _currentShape = null;
                    _previousShape = null;
                    _lastActionWasShape = false;

                    foreach (var gameObject in _boundingBoxVertexObjects)
                    {
                        Destroy(gameObject);
                    }
                    _boundingBoxVertexObjects.Clear();

                    NewDrawing();
                    NewLine();

                    _drawingDataWhenFrozen.Clear();
                    _linesAndShapes.Clear();

                    //RpcEndDraw();
                    CmdEndDraw();

                    return;
                }

                if (_currentShape != null)
                {
                    var shape = _currentShape.GetComponent<Shape>();
                    
                    if (_previousShape != null)
                    {
                        if (!shape.Created)
                        {
                            foreach (var gameObject in _boundingBoxVertexObjects)
                            {
                                Destroy(gameObject);
                            }
                            _boundingBoxVertexObjects.Clear();

                            _previousShape.GetComponent<Shape>().Finish();
                            _previousShape = null;
                            _lastActionWasShape = false;

                            if (_freeze.InstantSend)
                            {
                                //RpcNewLine();
                                CmdNewLine();
                            }
                        }

                        return;
                    }
                    else if (shape.Finished)
                    {
                        foreach (var gameObject in _boundingBoxVertexObjects)
                        {
                            Destroy(gameObject);
                        }
                        _boundingBoxVertexObjects.Clear();

                        _currentShape = null;
                        _lastActionWasShape = false;

                        if (_freeze.InstantSend)
                        { 
                            //RpcNewLine();
                            CmdNewLine();
                        }

                        return;
                    }
                    else
                    {
                        if (shape.changed)
                        {
                            shape.changed = false;

                            var boundingBoxVertices = shape.BoundingBoxVertices;
                            if (_boundingBoxVertexObjects.Count == 0)
                            {
                                var parent = GameObject.Find("BoundingBoxVertices");
                                var scale = _drawCanvasDistance / 100f;
                                for (int i = 0; i < boundingBoxVertices.Count; i++)
                                {
                                    _boundingBoxVertexObjects.Add(Instantiate(boundingBoxVertexPrefab, parent.transform));
                                    _boundingBoxVertexObjects[i].transform.localScale = new Vector3(scale, scale, scale);
                                }
                            }

                            var boundingBoxPositions = new List<Vector3>();
                            var boundingBoxLineRenderer = _currentShape.transform.Find("BoundingBox").GetComponent<LineRenderer>();
                            boundingBoxLineRenderer.positionCount = shape.BoundingBoxVertices.Count;
                            for (int i = 0; i < boundingBoxVertices.Count; i++)
                            {
                                var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(boundingBoxVertices[i].x, boundingBoxVertices[i].y, _drawCanvasDistance));
                                boundingBoxPositions.Add(worldPosition);

                                _boundingBoxVertexObjects[i].transform.position = worldPosition;
                                if (boundingBoxVertices[i].y < _drawAreaRectTransform.offsetMin.y || boundingBoxVertices[i].y > Screen.height + _drawAreaRectTransform.offsetMax.y + 32 ||
                                    boundingBoxVertices[i].x < _drawAreaRectTransform.offsetMin.x || boundingBoxVertices[i].x > Screen.width + _drawAreaRectTransform.offsetMax.x)
                                {
                                    _boundingBoxVertexObjects[i].SetActive(false);
                                }
                                else
                                {
                                    _boundingBoxVertexObjects[i].SetActive(true);
                                }
                            }
                            boundingBoxLineRenderer.SetPositions(boundingBoxPositions.ToArray());

                            var shapePositions = new List<Vector3>();
                            var shapeLineRenderer = _currentShape.transform.Find("Shape").GetComponent<LineRenderer>();
                            shapeLineRenderer.positionCount = shape.ShapeVertices.Count;
                            foreach (var vertex in shape.ShapeVertices)
                            {
                                var worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(vertex.x, vertex.y, _drawCanvasDistance));
                                shapePositions.Add(worldPosition);
                            }
                            shapeLineRenderer.SetPositions(shapePositions.ToArray());

                            _drawingDataWhenFrozen[^1] = (shape.ShapeVertices, (_brushSettings.BrushColor, -1 * _brushSettings.BrushSize),
                                                         (_drawAreaRectTransform.rect.width, _drawAreaRectTransform.rect.height),
                                                         (_drawAreaRectTransform.offsetMin.x, _drawAreaRectTransform.offsetMin.y));

                            if (_freeze.InstantSend)
                            {
                                //RpcSetForwardFrozen();
                                CmdSetForwardFrozen();
                                /*RpcDrawShape(shape.ShapeVertices, !_lastActionWasShape, _drawAreaRectTransform.rect.width, _drawAreaRectTransform.rect.height, 
                                             _drawAreaRectTransform.offsetMin.x, _drawAreaRectTransform.offsetMin.y);*/
                                CmdDrawShape(shape.ShapeVertices, !_lastActionWasShape, _drawAreaRectTransform.rect.width, _drawAreaRectTransform.rect.height,
                                             _drawAreaRectTransform.offsetMin.x, _drawAreaRectTransform.offsetMin.y);
                            }

                            _lastActionWasShape = true;
                        }

                        return;
                    }
                }

                if (Pointer.current.press.wasPressedThisFrame)
                {
                    var screenPosition = Pointer.current.position.ReadValue();

                    if (screenPosition.y > _drawAreaRectTransform.offsetMin.y && screenPosition.y < Screen.height + _drawAreaRectTransform.offsetMax.y + 32 &&
                        screenPosition.x > _drawAreaRectTransform.offsetMin.x && screenPosition.x < Screen.width + _drawAreaRectTransform.offsetMax.x)
                    {
                        _clickedOnDrawingField = true;
                        _lastActionWasShape = false; 
                        _drawingDataWhenFrozen.Add((new List<Vector2>(), (_brushSettings.BrushColor, _brushSettings.BrushSize), (0f, 0f), (0f, 0f)));
                        _linesAndShapes.Add(_currentLine.gameObject);

                        if (_freeze.InstantSend)
                        {
                            //RpcSetForwardFrozen();
                            CmdSetForwardFrozen();
                        }
                    }
                    else
                    {
                        _clickedOnDrawingField = false;
                    }
                }

                if (!_clickedOnDrawingField)
                {
                    return;
                }

                if (Pointer.current.press.isPressed)
                {
                    var screenPosition = Pointer.current.position.ReadValue();
                    _drawingDataWhenFrozen[^1].positions.Add(screenPosition);

                    var screenPositionWithZ = new Vector3(screenPosition.x, screenPosition.y, _drawCanvasDistance);
                    var worldPosition = Camera.main.ScreenToWorldPoint(screenPositionWithZ);

                    if (_currentLine.positionCount == 0)
                    {
                        NewPoint(0, worldPosition);
                    }
                    else if (Vector3.Distance(_previousPosition, worldPosition) > minDistance)
                    {
                        NewPoint(_currentLine.positionCount, worldPosition);
                    }

                    if (_freeze.InstantSend)
                    {
                        /*RpcDraw(screenPosition, _drawAreaRectTransform.rect.width, _drawAreaRectTransform.rect.height,
                                _drawAreaRectTransform.offsetMin.x, _drawAreaRectTransform.offsetMin.y);*/
                        CmdDraw(screenPosition, _drawAreaRectTransform.rect.width, _drawAreaRectTransform.rect.height,
                                _drawAreaRectTransform.offsetMin.x, _drawAreaRectTransform.offsetMin.y);
                    }
                }
                else if(Pointer.current.press.wasReleasedThisFrame)
                {
                    _drawingDataWhenFrozen[^1] = (_drawingDataWhenFrozen[^1].positions, (_brushSettings.BrushColor, _brushSettings.BrushSize), 
                                                 (_drawAreaRectTransform.rect.width, _drawAreaRectTransform.rect.height),
                                                 (_drawAreaRectTransform.offsetMin.x, _drawAreaRectTransform.offsetMin.y));
                    NewLine();

                    if (_freeze.InstantSend)
                    {
                        //RpcNewLine();
                        CmdNewLine();
                    }
                }
            }
            else
            {
                if (_undoDelete.ShouldDelete)
                {
                    _undoDelete.ShouldDelete = false;
                    //RpcDelete(false);
                    CmdDelete(false);

                    return;
                }

                if (_undoDelete.ShouldUndo)
                {
                    _undoDelete.ShouldUndo = false;
                    //RpcUndo(false);
                    CmdUndo(false);

                    return;
                }

                var screenPosition = Pointer.current.position.ReadValue();

                if (Pointer.current.press.wasPressedThisFrame)
                {
                    if (screenPosition.y > _drawAreaRectTransform.offsetMin.y && screenPosition.y < Screen.height + _drawAreaRectTransform.offsetMax.y + 32 &&
                        screenPosition.x > _drawAreaRectTransform.offsetMin.x && screenPosition.x < Screen.width + _drawAreaRectTransform.offsetMax.x)
                    {
                        _brushSettings.BrushSize = _brushSettings.BrushSize;
                        _clickedOnDrawingField = true;
                        //RpcStartDraw();
                        CmdStartDraw();
                    }
                    else
                    {
                        _clickedOnDrawingField = false;
                    }
                }

                if (!_clickedOnDrawingField)
                {
                    return;
                }

                if (Pointer.current.press.isPressed)
                {
                    /*RpcDraw(screenPosition, _drawAreaRectTransform.rect.width, _drawAreaRectTransform.rect.height,
                            _drawAreaRectTransform.offsetMin.x, _drawAreaRectTransform.offsetMin.y);*/
                    CmdDraw(screenPosition, _drawAreaRectTransform.rect.width, _drawAreaRectTransform.rect.height,
                            _drawAreaRectTransform.offsetMin.x, _drawAreaRectTransform.offsetMin.y);
                }
                else if (Pointer.current.press.wasReleasedThisFrame)
                {
                    //RpcEndDraw();
                    CmdEndDraw();
                }
            }
        //}

        /*
            HA A SZERELONEK IS KELL TUDNIA RAJZOLNI
        */

        /*if (Pointer.current.press.wasPressedThisFrame && _currentDrawing.forward == Vector3.zero)
        {
            _currentDrawing.forward = _plane.transform.forward;
        }

        if (Pointer.current.press.isPressed)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Pointer.current.position.ReadValue()), out _hit, 100f, _layerMask.value))
            {
                _isDrawing = true;
                Vector3 currentPosition = _hit.point;

                if (_currentLine.positionCount == 0)
                {
                    NewPoint(0, currentPosition);

                }
                else if (Vector3.Distance(_previousPosition, currentPosition) > minDistance)
                {
                    NewPoint(_currentLine.positionCount, currentPosition);
                }
            }
        }
        else if (_isDrawing && Pointer.current.press.wasReleasedThisFrame)
        {
            _isDrawing = false;
            NewDrawing();
            NewLine();
        }*/
    }

    private void NewDrawing()
    {
        Drawing drawing = new Drawing();
        _drawings.Add(drawing);
        _currentDrawing = drawing;
    }

    private void NewLine()
    {
        var line = Instantiate(linePrefab/*, _lineHolder.transform*/).GetComponent<LineRenderer>();
        _currentDrawing.lines.Add(line);
        line.startColor = _brushSettings.BrushColor;
        line.endColor = _brushSettings.BrushColor;
        line.startWidth = _brushSettings.BrushSize;
        _currentLine = line;
    }

    private void NewPoint(int index, Vector3 position)
    {
        _currentLine.positionCount++;
        _currentLine.SetPosition(index, position);
        _previousPosition = position;
    }

    public void NewShape()
    {
        if (_currentShape != null)
        {
            _previousShape = _currentShape;
        }

        var shape = _shapeButtons.GetComponent<ShapeFactory>().Shape;

        var boundingBoxLine = shape.transform.Find("BoundingBox").GetComponent<LineRenderer>();
        boundingBoxLine.startColor = Color.black;
        boundingBoxLine.endColor = Color.black;
        boundingBoxLine.startWidth = 0.01f;
        boundingBoxLine.endWidth = 0.01f;

        var shapeLine = shape.transform.Find("Shape").GetComponent<LineRenderer>();
        shapeLine.startColor = _brushSettings.BrushColor;
        shapeLine.endColor = _brushSettings.BrushColor;
        shapeLine.startWidth = _brushSettings.BrushSize;
        shapeLine.endWidth = _brushSettings.BrushSize;

        _currentShape = shape;
        _shapes.Add(shape);

        if (_drawingDataWhenFrozen.Count > 0 && _drawingDataWhenFrozen[^1].positions.Count == 0)
        {
            _drawingDataWhenFrozen[^1] = (new List<Vector2>(), (_brushSettings.BrushColor, -1 * _brushSettings.BrushSize), (0f, 0f), (0f, 0f));
            _linesAndShapes[^1] = shapeLine.gameObject;
        }
        else
        {
            _drawingDataWhenFrozen.Add((new List<Vector2>(), (_brushSettings.BrushColor, -1 * _brushSettings.BrushSize), (0f, 0f), (0f, 0f)));
            _linesAndShapes.Add(shapeLine.gameObject);
        }
    }

    private void CheckAngle()
    {
        foreach (var drawing in _drawings)
        {
            var angle = Mathf.Clamp(Vector3.Angle(drawing.forward, -Camera.main.transform.forward), 0, 90);
            
            foreach (var line in drawing.lines)
            {
                if (line == null)
                {
                    continue;
                }

                var color = new Color(line.startColor.r, line.startColor.g, line.startColor.b, 1f - angle * 1f / 90f);
                line.startColor = color;
                line.endColor = color;
            }
        }
    }

    private void OnBrushColorChangedHandler()
    {
        if (_currentLine != null)
        {
            _currentLine.startColor = _brushSettings.BrushColor;
            _currentLine.endColor = _brushSettings.BrushColor;

            if (_currentShape != null)
            {
                var shapeLine = _currentShape.transform.Find("Shape").GetComponent<LineRenderer>();
                shapeLine.startColor = _brushSettings.BrushColor;
                shapeLine.endColor = _brushSettings.BrushColor;
                _currentShape.GetComponent<Shape>().changed = true;
            }
        }

        /*if (_freeze.IsFrozen)
        {
            if (_freeze.InstantSend)
            {
                RpcOnBrushColorChangedHandler(_brushSettings.BrushColor);
            }

            return;
        }

        if (isServer)
        {
            RpcOnBrushColorChangedHandler(_brushSettings.BrushColor);
        }*/

        if (_freeze.IsFrozen && !_freeze.InstantSend)
        {
            return;
        }

        CmdOnBrushColorChangedHandler(_brushSettings.BrushColor);
    }

    [Command (requiresAuthority = false)]
    private void CmdOnBrushColorChangedHandler(Color brushColor)
    {
        RpcOnBrushColorChangedHandler(brushColor);
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcOnBrushColorChangedHandler(Color brushColor)
    {
        if (_isDispatcher)
        {
            return;
        }

        _brushSettings.BrushColor = brushColor;
    }

    private void OnBrushSizeChangedHandler()
    {
        if (_currentLine != null)
        {
            _currentLine.startWidth = _brushSettings.BrushSize;
            _currentLine.endWidth = _brushSettings.BrushSize;

            if (_currentShape != null)
            {
                var shapeLine = _currentShape.transform.Find("Shape").GetComponent<LineRenderer>();
                shapeLine.startWidth = _brushSettings.BrushSize;
                shapeLine.endWidth = _brushSettings.BrushSize;
                _currentShape.GetComponent<Shape>().changed = true;
            }
        }

        /*if (_freeze.IsFrozen)
        {
            if (_freeze.InstantSend)
            {
                RpcOnBrushSizeChangedHandler(_brushSettings.BrushSize / 2f);
            }

            return;
        }

        if (isServer)
        {
            RpcOnBrushSizeChangedHandler(_brushSettings.BrushSize / 2f);
        }*/

        if (_freeze.IsFrozen && !_freeze.InstantSend)
        { 
            return; 
        }

        CmdOnBrushSizeChangedHandler(_brushSettings.BrushSize);
    }

    [Command (requiresAuthority = false)]
    private void CmdOnBrushSizeChangedHandler(float brushSize)
    {
        RpcOnBrushSizeChangedHandler(brushSize);
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcOnBrushSizeChangedHandler(float brushSize)
    {
        if (_isDispatcher)
        {
            return; 
        }

        _brushSettings.BrushSize = brushSize;
    }

    [Command (requiresAuthority = false)]
    private void CmdFreeze()
    {
        RpcFreeze();
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcFreeze()
    {
        if (_isDispatcher)
        {
            return;
        }

        _tempCamera = Instantiate(tempCamera, Camera.main.transform.position, Camera.main.transform.rotation);
        _tempCamera.gameObject.SetActive(false);

        _tempPlane = Instantiate(tempPlane, _plane.transform.position, _plane.transform.rotation);

        //_stepManager.AddStep();
    }

    [Command (requiresAuthority = false)]
    private void CmdUnfreeze()
    {
        RpcUnfreeze();
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcUnfreeze()
    {
        if (_isDispatcher)
        {
            return;
        }    

        Destroy(_tempCamera.gameObject);
        _tempCamera = null;

        Destroy(_tempPlane);
        _tempPlane = null;

        if (_currentDrawing.forward != Vector3.zero)
        {
            _currentDrawing.forward = Vector3.zero;
        }
    }

    [Command (requiresAuthority = false)]
    private void CmdStartDraw()
    {
        RpcStartDraw();
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcStartDraw()
    {
        if (_isDispatcher)
        {
            return;
        }

        if (_currentDrawing.forward == Vector3.zero)
        {
            _currentDrawing.forward = _plane.transform.forward;
        }
    }

    [Command (requiresAuthority = false)]
    private void CmdDraw(Vector2 screenPosition, float screenWidth, float screenHeight, float offsetX, float offsetY)
    {
        RpcDraw(screenPosition, screenWidth, screenHeight, offsetX, offsetY);
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcDraw(Vector2 screenPosition, float screenWidth, float screenHeight, float offsetX, float offsetY)
    {
        if (_isDispatcher)
        {
            return;
        }

        var scaledScreenPosition = new Vector2((screenPosition.x - offsetX) * Screen.width / screenWidth,
                                               (screenPosition.y - offsetY) * Screen.height / screenHeight);
        var camera = _tempCamera == null ? Camera.main : _tempCamera;
        var layerMask = _tempCamera == null ? _layerMask : _tempLayerMask;

        if (Physics.Raycast(camera.ScreenPointToRay(scaledScreenPosition), out _hit, 100f, layerMask.value))
        {
            Vector3 currentPosition = _hit.point;

            if (_currentLine.positionCount == 0)
            {
                NewPoint(0, currentPosition);
            }
            else if (Vector3.Distance(_previousPosition, currentPosition) > minDistance)
            {
                NewPoint(_currentLine.positionCount, currentPosition);
            }
        }
    }

    [Command (requiresAuthority = false)]
    private void CmdDrawFrozen(List<List<Vector2>> lines, List<Color> brushColors, List<float> brushSizes, List<float> screenWidths, List<float> screenHeights, List<float> offsetsX, List<float> offsetsY)
    {
        RpcDrawFrozen(lines, brushColors, brushSizes, screenWidths, screenHeights, offsetsX, offsetsY);
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcDrawFrozen(List<List<Vector2>> lines, List<Color> brushColors, List<float> brushSizes, List<float> screenWidths, List<float> screenHeights, List<float> offsetsX, List<float> offsetsY)
    {
        if (_isDispatcher)
        { 
            return;
        }

        if (_currentDrawing.forward == Vector3.zero)
        {
            _currentDrawing.forward = _tempPlane.transform.forward;
        }

        _currentDrawing.lines.Clear();

        for (int i = 0; i < lines.Count; i++)
        {
            var worldPositions = new List<Vector3>();
            var brushColor = brushColors[i];
            var brushSize = brushSizes[i] / 2f;
            var screenWidth = screenWidths[i];
            var screenHeight = screenHeights[i];
            var offsetX = offsetsX[i];
            var offsetY = offsetsY[i];

            foreach (var screenPosition in lines[i])
            { 
                var scaledScreenPosition = new Vector2((screenPosition.x - offsetX) * Screen.width / screenWidth, 
                                                       (screenPosition.y - offsetY) * Screen.height / screenHeight);
                Physics.Raycast(_tempCamera.ScreenPointToRay(scaledScreenPosition), out _hit, 100f, _tempLayerMask.value);
                worldPositions.Add(_hit.point);
            }

            if (worldPositions.Count == 0)
            {
                continue;
            }

            var newLine = Instantiate(linePrefab/*, _lineHolder.transform*/).GetComponent<LineRenderer>();
            newLine.startColor = newLine.endColor = brushColor;
            newLine.startWidth = newLine.endWidth = brushSize < 0f ? -1 * brushSize : brushSize;
            newLine.loop = brushSize < 0f;
            newLine.positionCount = worldPositions.Count;
            newLine.SetPositions(worldPositions.ToArray());

            _currentDrawing.lines.Add(newLine);
        }

        //_stepManager.AddDrawing(_currentDrawing);
    }

    [Command (requiresAuthority = false)]
    private void CmdDrawShape(List<Vector2> screenPositions, bool isNewShape, float screenWidth, float screenHeight, float offsetX, float offsetY)
    {
        RpcDrawShape(screenPositions, isNewShape, screenWidth, screenHeight, offsetX, offsetY);
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcDrawShape(List<Vector2> screenPositions, bool isNewShape, float screenWidth, float screenHeight, float offsetX, float offsetY)
    {
        if (_isDispatcher)
        {
            return;
        }

        var scaledScreenPositions = screenPositions.ConvertAll(pos => new Vector2((pos.x - offsetX) * Screen.width / screenWidth,
                                                                                  (pos.y - offsetY) * Screen.height / screenHeight));

        if (isNewShape)
        {
            foreach (var position in scaledScreenPositions)
            {
                if (Physics.Raycast(_tempCamera.ScreenPointToRay(position), out _hit, 100f, _tempLayerMask.value))
                {
                    NewPoint(_currentLine.positionCount, _hit.point);
                }
            }
            _currentLine.loop = true;
        }
        else
        {
            var newPositions = new List<Vector3>();
            foreach (var position in scaledScreenPositions)
            {
                if (Physics.Raycast(_tempCamera.ScreenPointToRay(position), out _hit, 100f, _tempLayerMask.value))
                {
                    newPositions.Add(_hit.point);
                }
            }
            _currentLine.positionCount = newPositions.Count;
            _currentLine.SetPositions(newPositions.ToArray());
        }
    }

    [Command (requiresAuthority = false)]
    private void CmdSetForwardFrozen()
    {
        RpcSetForwardFrozen();
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcSetForwardFrozen()
    {
        if (_isDispatcher)
        { 
            return;
        }

        if (_currentDrawing.forward == Vector3.zero)
        {
            _currentDrawing.forward = _tempPlane.transform.forward;
        }
    }

    [Command (requiresAuthority = false)]
    private void CmdNewLine()
    {
        RpcNewLine();
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcNewLine()
    {
        if (_isDispatcher)
        {
            return;
        }

        NewLine();
    }

    [Command (requiresAuthority = false)]
    private void CmdEndDraw()
    {
        RpcEndDraw();
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcEndDraw()
    {
        if (_isDispatcher)
        {
            return;
        }

        NewDrawing();
        NewLine();
    }

    [Command(requiresAuthority = false)]
    private void CmdDelete(bool currentOnly)
    {
        RpcDelete(currentOnly);
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcDelete(bool currentOnly)
    {
        if (_isDispatcher)
        {
            return;
        }    

        if (currentOnly)
        {
            foreach (var line in _currentDrawing.lines)
            {
                Destroy(line.gameObject);
            }
            _currentDrawing.lines.Clear();
        }
        else
        {
            foreach (var drawing in _drawings)
            {
                foreach (var line in drawing.lines)
                {
                    Destroy(line.gameObject);
                }
            }
            _drawings.Clear();

            NewDrawing();
        }

        NewLine();
    }

    [Command (requiresAuthority = false)]
    private void CmdUndo(bool currentOnly)
    {
        RpcUndo(currentOnly);
    }

    [ClientRpc/*(includeOwner = false)*/]
    private void RpcUndo(bool currentOnly)
    {
        if (_isDispatcher)
        {
            return;
        }

        if (currentOnly)
        {
            for (int i = _currentDrawing.lines.Count - 1; i >= 0; i--)
            {
                var line = _currentDrawing.lines[i];
                if (line.positionCount > 0)
                {
                    Destroy(line.gameObject);
                    _currentDrawing.lines.RemoveAt(i);

                    break;
                }
            }
        }
        else
        {
            var final = false;
            for (int i = _drawings.Count - 1; i >= 0; i--)
            {
                var drawing = _drawings[i];
                if (drawing.lines.Any(line => line.positionCount > 0))
                {
                    final = true;
                }

                foreach (var line in drawing.lines)
                {
                    Destroy(line.gameObject);
                }
                _drawings.RemoveAt(i);

                if (final)
                {
                    break;
                }
            }

            NewDrawing();
            NewLine();
        }
    }

    public void SwitchedTo3D()
    {
        if (_freeze.InstantSend)
        {
            CmdDelete(true);
        }

        foreach (var drawing in _drawings)
        {
            foreach (var line in drawing.lines)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
        }
        _drawings.Clear();
        _currentDrawing = null;
        _currentLine = null;

        foreach (var shape in _shapes)
        {
            Destroy(shape);
        }
        _shapes.Clear();
        _currentShape = null;
        _previousShape = null;
        _lastActionWasShape = false;

        foreach (var gameObject in _boundingBoxVertexObjects)
        {
            Destroy(gameObject);
        }
        _boundingBoxVertexObjects.Clear();

        _drawingDataWhenFrozen.Clear();
        _linesAndShapes.Clear();

        NewDrawing();
        NewLine();
    }
/*    private void StartStream()
    {
        var videoStreamTrack = _streamingCamera.CaptureStreamTrack(Screen.width, Screen.height);
        _webRTCManager.SendVideoTrack(videoStreamTrack);
        _startStreamButton.interactable = false;
    }

    [ClientRpc(includeOwner = false)]
    private void RpcAddDrawingStep()
    {
        _stepManager.AddDrawing(_currentDrawing);
    }*/
}

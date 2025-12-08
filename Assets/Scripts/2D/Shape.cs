using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;



public enum ShapeType
{
    None = -1,
    Square,
    Ellipse,
    TriangleUp,
    TriangleRight,
    TriangleDown,
    TriangleLeft,
    RightTriangleTopLeft,
    RightTriangleTopRight,
    RightTriangleBottomRight,
    RightTriangleBottomLeft,
    ArrowUp,
    ArrowRight,
    ArrowDown,
    ArrowLeft,
}

public class Shape : MonoBehaviour
{
    private enum BoundingBoxVertexType
    {
        None = -1,
        TopLeft,
        TopCenter,
        TopRight,
        CenterRight,
        BottomRight,
        BottomCenter,
        BottomLeft,
        CenterLeft,
    }

    private class BoundingBoxVertex
    {
        public BoundingBoxVertexType type;

        public Vector2 position;

        public Dictionary<BoundingBoxVertexType, (float xScale, float yScale)> scales;
    }

    private class ShapeVertex
    {
        public Vector2 position;

        public Dictionary<BoundingBoxVertexType, (float xScale, float yScale)> scales;
    }

    public ShapeType type;

    private Dictionary<BoundingBoxVertexType, ShapeVertex> _boundingBoxVertices;
    public List<Vector2> BoundingBoxVertices => _boundingBoxVertices.Select(vertex => vertex.Value.position).ToList();

    private List<ShapeVertex> _shapeVertices;
    public List<Vector2> ShapeVertices => _shapeVertices.Select(vertex => vertex.position).ToList();

    private BoundingBoxVertexType _selectedBoundingBoxVertex = BoundingBoxVertexType.None;

    private ShapeVertex _centerVertex;

    private Vector2 _draggingPosition;

    private bool _created;
    public bool Created => _created;

    public bool changed;

    private bool _finished;
    public bool Finished => _finished;

    private RectTransform _drawAreaRectTransform;

    private int _baseWidth;

    private int _baseHeight;

    private int _minWidth;

    private int _minHeight;

    private float _maxDistanceToBoundingBoxVertex;



    private void Start()
    {
        _boundingBoxVertices = new Dictionary<BoundingBoxVertexType, ShapeVertex>();
        _shapeVertices = new List<ShapeVertex>();

        _drawAreaRectTransform = GameObject.Find("DrawAreaCanvas").GetComponent<RectTransform>();

        _baseWidth = _baseHeight = (int)((_drawAreaRectTransform.rect.width / 19.2f + _drawAreaRectTransform.rect.height / 10.8f) / 2f);
        _minWidth = _minHeight = (int)(_baseWidth / 10f);
        _maxDistanceToBoundingBoxVertex = 7f;

        _created = true;
    }

    private void Update()
    {
        if (_finished)
        {
            return;
        }

        var mousePosition = Pointer.current.position.ReadValue();

        if (_created && Pointer.current.press.wasPressedThisFrame)
        {
            if (mousePosition.y > _drawAreaRectTransform.offsetMin.y && mousePosition.y < Screen.height + _drawAreaRectTransform.offsetMax.y + 32 &&
                mousePosition.x > _drawAreaRectTransform.offsetMin.x && mousePosition.x < Screen.width + _drawAreaRectTransform.offsetMax.x)
            {
                _created = false;

                switch (type)
                {
                    case ShapeType.Square:
                        Square(mousePosition);
                        break;
                    case ShapeType.Ellipse:
                        Ellipse(mousePosition);
                        break;
                    case ShapeType.TriangleUp:
                        TriangleUp(mousePosition);
                        break;
                    case ShapeType.TriangleRight:
                        TriangleRight(mousePosition);
                        break;
                    case ShapeType.TriangleDown:
                        TriangleDown(mousePosition);
                        break;
                    case ShapeType.TriangleLeft:
                        TriangleLeft(mousePosition);
                        break;
                    case ShapeType.RightTriangleTopLeft:
                        RightTriangleTopLeft(mousePosition);
                        break;
                    case ShapeType.RightTriangleTopRight:
                        RightTriangleTopRight(mousePosition);
                        break;
                    case ShapeType.RightTriangleBottomRight:
                        RightTriangleBottomRight(mousePosition);
                        break;
                    case ShapeType.RightTriangleBottomLeft:
                        RightTriangleBottomLeft(mousePosition);
                        break;
                    case ShapeType.ArrowUp:
                        ArrowUp(mousePosition);
                        break;
                    case ShapeType.ArrowRight:
                        ArrowRight(mousePosition);
                        break;
                    case ShapeType.ArrowDown:
                        ArrowDown(mousePosition);
                        break;
                    case ShapeType.ArrowLeft:
                        ArrowLeft(mousePosition);
                        break;
                }
            }

            return;
        }

        if (Pointer.current.press.wasPressedThisFrame &&
            mousePosition.y > _drawAreaRectTransform.offsetMin.y && mousePosition.y < Screen.height + _drawAreaRectTransform.offsetMax.y + 32 && 
            mousePosition.x > _drawAreaRectTransform.offsetMin.x && mousePosition.x < Screen.width + _drawAreaRectTransform.offsetMax.x)
        {
            var distances = new List<float>();
            foreach (var vertex in _boundingBoxVertices)
            {
                distances.Add(Vector2.Distance(mousePosition, vertex.Value.position));
            }

            if (distances.Min() <= _maxDistanceToBoundingBoxVertex)
            {
                _selectedBoundingBoxVertex = _boundingBoxVertices.ElementAt(distances.IndexOf(distances.Min())).Key;
                _draggingPosition = mousePosition;
            }
            else if (mousePosition.x > _boundingBoxVertices[BoundingBoxVertexType.CenterLeft].position.x &&
                     mousePosition.x < _boundingBoxVertices[BoundingBoxVertexType.CenterRight].position.x &&
                     mousePosition.y > _boundingBoxVertices[BoundingBoxVertexType.BottomCenter].position.y &&
                     mousePosition.y < _boundingBoxVertices[BoundingBoxVertexType.TopCenter].position.y)
            {
                _draggingPosition = mousePosition;
            }
            else
            {
                _finished = true;
                transform.Find("BoundingBox").gameObject.SetActive(false);
            }
        }


        if (_selectedBoundingBoxVertex == BoundingBoxVertexType.None && _draggingPosition != Vector2.zero)
        {
            if (Pointer.current.press.isPressed)
            {
                var difference = mousePosition - _draggingPosition;

                foreach (var vertex in _boundingBoxVertices)
                {
                    _boundingBoxVertices[vertex.Key].position += new Vector2(difference.x, difference.y);
                }

                for (int i = 0; i < _shapeVertices.Count; i++)
                {
                    _shapeVertices[i].position += new Vector2(difference.x, difference.y);
                }

                if (_centerVertex != null)
                {
                    _centerVertex.position += new Vector2(difference.x, difference.y);
                }

                _draggingPosition = mousePosition;
                changed = true;
            }
            else if (Pointer.current.press.wasReleasedThisFrame)
            {
                _draggingPosition = Vector2.zero;
            }
        }
        else if (_selectedBoundingBoxVertex != BoundingBoxVertexType.None)
        {
            if (Pointer.current.press.isPressed)
            {
                var difference = mousePosition - _draggingPosition;

                var newBoundingBoxVertices = new Dictionary<BoundingBoxVertexType, ShapeVertex>();
                foreach (var vertex in _boundingBoxVertices)
                {
                    var newPosition = vertex.Value.position + new Vector2((int)(difference.x * vertex.Value.scales[_selectedBoundingBoxVertex].xScale),
                                                                          (int)(difference.y * vertex.Value.scales[_selectedBoundingBoxVertex].yScale));
                    newBoundingBoxVertices[vertex.Key] = new ShapeVertex
                    {
                        position = newPosition,
                        scales = vertex.Value.scales
                    };
                }

                var tooThin = false;
                if (Vector2.Distance(newBoundingBoxVertices[BoundingBoxVertexType.CenterLeft].position, newBoundingBoxVertices[BoundingBoxVertexType.CenterRight].position) < _minWidth ||
                                     newBoundingBoxVertices[BoundingBoxVertexType.CenterLeft].position.x > newBoundingBoxVertices[BoundingBoxVertexType.CenterRight].position.x)
                {
                    tooThin = true;
                }

                var tooShort = false;
                if (Vector2.Distance(newBoundingBoxVertices[BoundingBoxVertexType.TopCenter].position, newBoundingBoxVertices[BoundingBoxVertexType.BottomCenter].position) < _minHeight ||
                                     newBoundingBoxVertices[BoundingBoxVertexType.TopCenter].position.y < newBoundingBoxVertices[BoundingBoxVertexType.BottomCenter].position.y)
                {
                    tooShort = true;
                }

                foreach (var vertex in _boundingBoxVertices)
                {
                    var newPosition = new Vector2(tooThin ? vertex.Value.position.x : newBoundingBoxVertices[vertex.Key].position.x,
                                                  tooShort ? vertex.Value.position.y : newBoundingBoxVertices[vertex.Key].position.y);
                    _boundingBoxVertices[vertex.Key].position = newPosition;
                }

                if (_centerVertex != null)
                {
                    _centerVertex.position = new Vector2((int)((_boundingBoxVertices[BoundingBoxVertexType.CenterLeft].position.x + _boundingBoxVertices[BoundingBoxVertexType.CenterRight].position.x) / 2f),
                                                         (int)((_boundingBoxVertices[BoundingBoxVertexType.BottomCenter].position.y + _boundingBoxVertices[BoundingBoxVertexType.TopCenter].position.y) / 2f));

                    var radiusX = Vector2.Distance(_boundingBoxVertices[BoundingBoxVertexType.CenterRight].position, _centerVertex.position);
                    var radiusY = Vector2.Distance(_boundingBoxVertices[BoundingBoxVertexType.TopCenter].position, _centerVertex.position);
                    var numOfVertices = Mathf.Min(400, Mathf.Max(8, (int)(2f * radiusX * 2f * radiusY / 834f)));
                    var angleStep = 360f / numOfVertices;
                    _shapeVertices.Clear();
                    for (int i = 0; i < numOfVertices; i++)
                    {
                        _shapeVertices.Add(new ShapeVertex
                        {
                            position = new Vector2((int)(_centerVertex.position.x + radiusX * Mathf.Cos(i * angleStep * Mathf.PI / 180f)),
                                                   (int)(_centerVertex.position.y + radiusY * Mathf.Sin(i * angleStep * Mathf.PI / 180f))),
                            scales = null
                        });
                    }
                }
                else
                { 
                    for (int i = 0; i < _shapeVertices.Count; i++)
                    {
                        var newPosition = _shapeVertices[i].position + new Vector2((tooThin ? 0 : 1) * (int)(difference.x * _shapeVertices[i].scales[_selectedBoundingBoxVertex].xScale),
                                                                                   (tooShort ? 0 : 1) * (int)(difference.y * _shapeVertices[i].scales[_selectedBoundingBoxVertex].yScale));
                        _shapeVertices[i].position = newPosition;
                    }
                }

                _draggingPosition = mousePosition;
                changed = true;
            }
            else if (Pointer.current.press.wasReleasedThisFrame)
            {
                _selectedBoundingBoxVertex = BoundingBoxVertexType.None;
                _draggingPosition = Vector2.zero;
            }
        }
    }

    public void Finish()
    {
        _finished = true;
        transform.Find("BoundingBox").gameObject.SetActive(false);
    }

    private void CreateBoundingBox(Vector2 origin)
    {
        var top = (int)(origin.y + _baseHeight / 2f); 
        var right = (int)(origin.x + _baseWidth / 2f);
        var bottom = (int)(origin.y - _baseHeight / 2f);
        var left = (int)(origin.x - _baseWidth / 2f);

        _boundingBoxVertices[BoundingBoxVertexType.TopLeft] = new ShapeVertex
        {
            position = new Vector2(left, top),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        };

        _boundingBoxVertices[BoundingBoxVertexType.TopCenter] = new ShapeVertex
        {
            position = new Vector2(origin.x, top),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        };

        _boundingBoxVertices[BoundingBoxVertexType.TopRight] = new ShapeVertex
        {
            position = new Vector2(right, top),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (1f, 1f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        };

        _boundingBoxVertices[BoundingBoxVertexType.CenterRight] = new ShapeVertex
        {
            position = new Vector2(right, origin.y),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        };

        _boundingBoxVertices[BoundingBoxVertexType.BottomRight] = new ShapeVertex
        {
            position = new Vector2(right, bottom),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (1f, 0f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        };

        _boundingBoxVertices[BoundingBoxVertexType.BottomCenter] = new ShapeVertex
        {
            position = new Vector2(origin.x, bottom),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        };

        _boundingBoxVertices[BoundingBoxVertexType.BottomLeft] = new ShapeVertex
        {
            position = new Vector2(left, bottom),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        };

        _boundingBoxVertices[BoundingBoxVertexType.CenterLeft] = new ShapeVertex
        {
            position = new Vector2(left, origin.y),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        };
    }

    public void Square(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (1f, 1f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (1f, 0f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }

    public void Ellipse(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _centerVertex = new ShapeVertex
        {
            position = new Vector2(origin.x, origin.y),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        };

        var radiusX = _baseWidth / 2f;
        var radiusY = _baseHeight / 2f;
        var numOfVertices = Mathf.Min(400, Mathf.Max(8, (int)(_baseWidth * _baseHeight / 834f)));
        var angleStep = 360f / numOfVertices;
        for (int i = 0; i < numOfVertices; i++)
        {
            _shapeVertices.Add(new ShapeVertex
            {
                position = new Vector2((int)(_centerVertex.position.x + radiusX * Mathf.Cos(i * angleStep * Mathf.PI / 180f)),
                                       (int)(_centerVertex.position.y + radiusY * Mathf.Sin(i * angleStep * Mathf.PI / 180f))),
                scales = null
            });
        }

        changed = true;
    }

    public void TriangleUp(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopCenter].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (1f, 0f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }

    public void TriangleRight(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.CenterRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }

    public void TriangleDown(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (1f, 1f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomCenter].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        changed = true;
    }

    public void TriangleLeft(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (1f, 1f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (1f, 0f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.CenterLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }

    public void RightTriangleTopLeft(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (1f, 1f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }

    public void RightTriangleTopRight(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (1f, 1f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (1f, 0f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        changed = true;
    }

    public void RightTriangleBottomRight(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (1f, 1f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (1f, 0f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }

    public void RightTriangleBottomLeft(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (1f, 0f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }

    public void ArrowUp(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopCenter].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.CenterRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x + _baseWidth / 4f), origin.y),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.25f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0.75f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0.75f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.75f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0.25f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0.25f, 0f) }
            }
        }); _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x + _baseWidth / 4f), (int)(origin.y - _baseHeight / 2f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.25f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0.75f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0.75f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.75f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0.25f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0.25f, 0f) }
            }
        }); _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x - _baseWidth / 4f), (int)(origin.y - _baseHeight / 2f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.75f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0.25f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0.25f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.25f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0.75f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0.75f, 0f) }
            }
        }); _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x - _baseWidth / 4f), origin.y),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.75f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0.25f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0.25f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.25f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0.75f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0.75f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.CenterLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }

    public void ArrowRight(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopCenter].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.CenterRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomCenter].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2(origin.x, (int)(origin.y - _baseHeight / 4f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0.25f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.25f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0.25f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0.75f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.75f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0.75f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x - _baseWidth / 2f), (int)(origin.y - _baseHeight / 4f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0.25f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.25f) },
                { BoundingBoxVertexType.TopRight, (0f, 0.25f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0.75f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.75f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0.75f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x - _baseWidth / 2f), (int)(origin.y + _baseHeight / 4f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0.75f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.75f) },
                { BoundingBoxVertexType.TopRight, (0f, 0.75f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0.25f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.25f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0.25f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2(origin.x, (int)(origin.y + _baseHeight / 4f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0.75f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.75f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0.75f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0.25f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.25f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0.25f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        changed = true;
    }

    public void ArrowDown(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x - _baseWidth / 4f), (int)(origin.y + _baseHeight / 2f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.75f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0.25f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0.25f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.25f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0.75f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0.75f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x + _baseWidth / 4f), (int)(origin.y + _baseHeight / 2f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.25f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0.75f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0.75f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.75f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0.25f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0.25f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x + _baseWidth / 4f), origin.y),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.25f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0.75f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0.75f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.75f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0.25f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0.25f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.CenterRight].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomCenter].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.CenterLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x - _baseWidth / 4f), origin.y),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.75f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0.25f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0.25f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.25f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (0.75f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (0.75f, 0f) }
            }
        });

        changed = true;
    }

    public void ArrowLeft(Vector2 origin)
    {
        CreateBoundingBox(origin);

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.TopCenter].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.TopCenter, (0f, 1f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2(origin.x, (int)(origin.y + _baseHeight / 4f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0.75f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.75f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0.75f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0.25f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.25f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0.25f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x + _baseWidth / 2f), (int)(origin.y + _baseHeight / 4f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0.75f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.75f) },
                { BoundingBoxVertexType.TopRight, (1f, 0.75f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0.25f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.25f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0.25f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2((int)(origin.x + _baseWidth / 2f), (int)(origin.y - _baseHeight / 4f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0f, 0.25f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.25f) },
                { BoundingBoxVertexType.TopRight, (1f, 0.25f) },
                { BoundingBoxVertexType.CenterRight, (1f, 0f) },
                { BoundingBoxVertexType.BottomRight, (1f, 0.75f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.75f) },
                { BoundingBoxVertexType.BottomLeft, (0f, 0.75f) },
                { BoundingBoxVertexType.CenterLeft, (0f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = new Vector2(origin.x, (int)(origin.y - _baseHeight / 4f)),
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0.25f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.25f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0.25f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 0.75f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.75f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 0.75f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.BottomCenter].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (0.5f, 0f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0f) },
                { BoundingBoxVertexType.TopRight, (0.5f, 0f) },
                { BoundingBoxVertexType.CenterRight, (0.5f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0.5f, 1f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 1f) },
                { BoundingBoxVertexType.BottomLeft, (0.5f, 1f) },
                { BoundingBoxVertexType.CenterLeft, (0.5f, 0f) }
            }
        });

        _shapeVertices.Add(new ShapeVertex
        {
            position = _boundingBoxVertices[BoundingBoxVertexType.CenterLeft].position,
            scales = new Dictionary<BoundingBoxVertexType, (float xScale, float yScale)>
            {
                { BoundingBoxVertexType.TopLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.TopCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.TopRight, (0f, 0.5f) },
                { BoundingBoxVertexType.CenterRight, (0f, 0f) },
                { BoundingBoxVertexType.BottomRight, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomCenter, (0f, 0.5f) },
                { BoundingBoxVertexType.BottomLeft, (1f, 0.5f) },
                { BoundingBoxVertexType.CenterLeft, (1f, 0f) }
            }
        });

        changed = true;
    }
}

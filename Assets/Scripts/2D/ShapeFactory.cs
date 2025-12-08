using UnityEngine;


public delegate void ShapeFactoryDelegate();

public class ShapeFactory : MonoBehaviour
{
    public event ShapeFactoryDelegate OnShapeCreated;

    public GameObject shapePrefab;

    //private GameObject _shapeHolder;

    private GameObject _shape;
    public GameObject Shape
    {
        get
        {
            return _shape;
        }

        set
        {
            _shape = value;
            OnShapeCreated?.Invoke();
        }
    }



    private void Start()
    {
        //_shapeHolder = GameObject.Find("ShapeHolder");
    }

    private void Create(ShapeType type)
    {
        GameObject shape = Instantiate(shapePrefab/*, _shapeHolder.transform*/);
        shape.GetComponent<Shape>().type = type;
        Shape = shape;
    }

    public void Square()
    {
        Create(ShapeType.Square);
    }

    public void Ellipse()
    {
        Create(ShapeType.Ellipse);
    }

    public void TriangleUp()
    {
        Create(ShapeType.TriangleUp);
    }

    public void TriangleRight()
    {
        Create(ShapeType.TriangleRight);
    }

    public void TriangleDown()
    {
        Create(ShapeType.TriangleDown);
    }

    public void TriangleLeft()
    {
        Create(ShapeType.TriangleLeft);
    }

    public void RightTriangleTopLeft()
    {
        Create(ShapeType.RightTriangleTopLeft);
    }

    public void RightTriangleTopRight()
    {
        Create(ShapeType.RightTriangleTopRight);
    }

    public void RightTriangleBottomRight()
    {
        Create(ShapeType.RightTriangleBottomRight);
    }

    public void RightTriangleBottomLeft()
    {
        Create(ShapeType.RightTriangleBottomLeft);
    }

    public void ArrowUp()
    {
        Create(ShapeType.ArrowUp);
    }

    public void ArrowRight()
    {
        Create(ShapeType.ArrowRight);
    }

    public void ArrowDown()
    {
        Create(ShapeType.ArrowDown);
    }

    public void ArrowLeft()
    {
        Create(ShapeType.ArrowLeft);
    }
}

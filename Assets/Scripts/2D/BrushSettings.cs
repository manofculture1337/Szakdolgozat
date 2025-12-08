using UnityEngine;



public delegate void BrushSettingsDelegate();

public class BrushSettings : MonoBehaviour
{
    public event BrushSettingsDelegate OnBrushColorChanged;
    public event BrushSettingsDelegate OnBrushSizeChanged;

    private Color _brushColor;
    public Color BrushColor 
    {
        get
        {
            return _brushColor;
        }

        set
        {
            _brushColor = value;
            OnBrushColorChanged?.Invoke();
        }
    }

    private float _brushSize;
    public float BrushSize

    {
        get
        {
            return _brushSize;
        }

        set
        {
            _brushSize = value;
            OnBrushSizeChanged?.Invoke();
        }
    }

    private Color BLACK = new Color(0f, 0f, 0f);
    private Color WHITE = new Color(1f, 1f, 1f);
    private Color GRAY = new Color(0.498f, 0.498f, 0.498f);
    private Color BROWN = new Color(0.725f, 0.478f, 0.341f);
    private Color DARKRED = new Color(0.533f, 0f, 0.082f);
    private Color RED = new Color(0.929f, 0.11f, 0.141f);
    private Color PURPLE = new Color(0.639f, 0.286f, 0.643f);
    private Color PINK = new Color(1f, 0.682f, 0.788f);
    private Color ORANGE = new Color(1f, 0.498f, 0.153f);
    private Color YELLOW = new Color(1f, 0.949f, 0f);
    private Color DARKGREEN = new Color(0.133f, 0.694f, 0.298f);
    private Color GREEN = new Color(0.71f, 0.902f, 0.114f);
    private Color DARKBLUE = new Color(0.247f, 0.282f, 0.8f);
    private Color BLUE = new Color(0f, 0.635f, 0.91f);

    private void Start()
    {
        BrushColor = BLACK;
        BrushSize = 0.02f;
    }

    public void Black()
    {
        BrushColor = BLACK;
    }
    public void White()
    {
        BrushColor = WHITE;
    }
    public void Gray()
    {
        BrushColor = GRAY;
    }
    public void Brown()
    {
        BrushColor = BROWN;
    }
    public void DarkRed()
    {
        BrushColor = DARKRED;
    }
    public void Red()
    {
        BrushColor = RED;
    }
    public void Purple()
    {
        BrushColor = PURPLE;
    }
    public void Pink()
    {
        BrushColor = PINK;
    }
    public void Orange()
    {
        BrushColor = ORANGE;
    }
    public void Yellow()
    {
        BrushColor = YELLOW;
    }
    public void DarkGreen()
    {
        BrushColor = DARKGREEN;
    }
    public void Green()
    {
        BrushColor = GREEN;
    }
    public void DarkBlue()
    {
        BrushColor = DARKBLUE;
    }
    public void Blue()
    {
        BrushColor = BLUE;
    }
}

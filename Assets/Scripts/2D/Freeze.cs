using UnityEngine;

public delegate void ReadyEventHandler();

public class Freeze : MonoBehaviour
{
    public event ReadyEventHandler ReadyChanged;

    private bool _isReady = false;
    public bool IsReady 
    { 
        get => _isReady;
        set
        {
            _isReady = value;
            if (_isReady)
            {
                ReadyChanged?.Invoke();
            }
        }
    }

    public bool IsFrozen { get; set; }

    public bool IsDone { get; set; }

    public bool InstantSend { get; set; }
}

using WebSocketSharp;
using UnityEngine;
using VContainer;
using System.Threading;

public delegate void ConnectionStateChange(WebSocketState state);
public class WebSocketConnectPresenter: MonoBehaviour
{

    private SynchronizationContext _mainThreadContext;

    public event ConnectionStateChange ConnectionStateChanged;
    private WebSocketClientUsecase _usecase;

    [Inject]
    private WebSocketClientService _service;

    [Inject]

    private void Awake()
    {
        _usecase = new WebSocketClientUsecase(_service);
    }


    void Start()
    {
        _mainThreadContext = SynchronizationContext.Current;
        _usecase.OnWebSocketStateChange(OnConnectionStateChanged);
    }
    public int Connect(string serverIP, string serverPort)
    {
        return _usecase.Connect(serverIP, serverPort);
    }
    private void OnConnectionStateChanged(WebSocketState state)
    {
        // Notify subscribers about the connection status change

        if (SynchronizationContext.Current == _mainThreadContext)
        {
            ConnectionStateChanged?.Invoke(state);
        }
        else
        {
            _mainThreadContext.Post(_ =>
            {
                ConnectionStateChanged?.Invoke(state);
            }, null);
        }
    }

}
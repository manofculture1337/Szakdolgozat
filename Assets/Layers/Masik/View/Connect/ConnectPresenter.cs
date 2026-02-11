using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using WebSocketSharp;

public class ConnectPresenter : MonoBehaviour
{
    private SynchronizationContext _mainThreadContext;

    private ConnectUseCase _connectUseCase;
    [SerializeField]
    private MyNetworkManager myNetworkManager;

    public event ConnectionStateChange ConnectionStateChanged;
    public event ConnectionStatusChangedHandler StreamingStateChanged;
    private WebSocketClientUsecase _websocketusecase;
    private WebSocketStreamingClientUsecase _streamingusecase;
    private DocumentationUseCase _documentationUseCase;
    private bool IsStreamer;

    [Inject]
    private readonly IDocumentationLogger logger;

    [Inject]
    private WebSocketClientService _service;

    [Inject]
    private WebSocketStreamingClientService _streamingService;

    /*[Inject]
    private MyNetworkManager myNetworkManager;*/

    [Inject]
    void Awake()
    {
        _documentationUseCase= new DocumentationUseCase(logger);
        _websocketusecase = new WebSocketClientUsecase(_service);
        _streamingusecase = new WebSocketStreamingClientUsecase(_streamingService, _service);
        _connectUseCase = new ConnectUseCase(myNetworkManager);
    }

    void Start()
    {
        _mainThreadContext = SynchronizationContext.Current;
        IsStreamer = false;
        _websocketusecase.OnWebSocketStateChange(OnConnectionStateChanged);
        _streamingusecase.OnConnectionStatusChanged(OnConnectionStatusChanged);
    }
    public void Connect(string ip, string port)
    {
        /*_connectUseCase.Connect(ip, port);
        _websocketusecase.Connect(ip, "8080");
        _documentationUseCase.loggerSetup(Application.persistentDataPath);*/

        IsStreamer = false;
        _websocketusecase.Connect(ip, "8080");
        _connectUseCase.Connect(ip, port, gameObject);
    }

    public void StartHost(string ip, string port)
    {
        IsStreamer = true;
        _connectUseCase.StartHost(ip, port);
        _websocketusecase.Connect(ip, "8080");

        //_documentationUseCase.loggerSetup(Application.persistentDataPath);
    }

    

    public void SetToOnline(/*INetworkManager manager*/)
    {
        //SceneManager.LoadScene("AuthMenu");
        //_connectUseCase.SetToOnline(manager);
    }

    public void ChangeToOfflineScene()
    {
        SceneManager.LoadScene("TutorialScene");
    }
    private void OnConnectChangedPrivate(WebSocketState state)
    {
        Debug.Log("belep");
        ConnectionStateChanged?.Invoke(state);
        if (IsStreamer && state==WebSocketState.Open)
        {
            Debug.Log("1");
            _streamingusecase.ConnectToStreamingAsStreamer();
        }
        else if(!IsStreamer && state == WebSocketState.Open)
        {
            Debug.Log("2");
            _streamingusecase.ConnectToStreamingAsViewer();
        }
    }
    private void OnConnectionStateChanged(WebSocketState state)
    {
        // Notify subscribers about the connection status change

        if (SynchronizationContext.Current == _mainThreadContext)
        {
            OnConnectChangedPrivate(state);
        }
        else
        {
            _mainThreadContext.Post(_ =>
            {
                OnConnectChangedPrivate(state);
            }, null);
        }
    }
    
    private void OnConnectionStatusChanged(WebSocketEnums.ConnectionStatus status)
    {
        if (SynchronizationContext.Current == _mainThreadContext)
        {
            var valami = _streamingusecase.GetCurrentConnectionType();
            StreamingStateChanged?.Invoke(status, valami);
            SwitchSceneBasedOnStreaming(status, valami);
        }
        else
        {
            _mainThreadContext.Post(_ =>
            {
                var valami = _streamingusecase.GetCurrentConnectionType();
                StreamingStateChanged?.Invoke(status, valami);
                SwitchSceneBasedOnStreaming(status, valami);
            }, null);
        }
    }
    private void SwitchSceneBasedOnStreaming(WebSocketEnums.ConnectionStatus status, WebSocketEnums.ConnectionType connectionType)
    {
        if(status==WebSocketEnums.ConnectionStatus.Connected && connectionType == WebSocketEnums.ConnectionType.Streamer)
        {
            SceneManager.LoadScene("XRScene");
        }
        if (status == WebSocketEnums.ConnectionStatus.Connected && connectionType == WebSocketEnums.ConnectionType.Viewer)
        {
            Debug.Log("XD");
            SceneManager.LoadScene("DispatcherScene");
        }
    }
}

using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;
using WebSocketSharp;

public class ConnectPresenter : MonoBehaviour
{
    private SynchronizationContext _mainThreadContext;

    private ConnectUseCase _connectUseCase;


    public event ConnectionStateChange ConnectionStateChanged;
    private WebSocketClientUsecase _websocketusecase;
    private WebSocketStreamingClientUsecase _streamingusecase;
    private DocumentationUseCase _documentationUseCase;

    [Inject]
    private readonly IDocumentationLogger logger;

    [Inject]
    private WebSocketClientService _service;

    [Inject]
    private WebSocketStreamingClientService _streamingService;

    [Inject]
    private MyNetworkManager myNetworkManager;

    [Inject]
    void Awake()
    {
        _documentationUseCase= new DocumentationUseCase(logger);
        _websocketusecase = new WebSocketClientUsecase(_service);
        _streamingusecase = new WebSocketStreamingClientUsecase(_streamingService, _service);
    }

    void Start()
    {
        _mainThreadContext = SynchronizationContext.Current;
        _websocketusecase.OnWebSocketStateChange(OnConnectionStateChanged);
    }

    public void Connect(string ip, string port)
    {
        /*_connectUseCase.Connect(ip, port);
        _websocketusecase.Connect(ip, "8080");
        _documentationUseCase.loggerSetup(Application.persistentDataPath);*/

        _websocketusecase.Connect(ip, "8080");
        _connectUseCase.Connect(ip, port, gameObject);
    }

    public void StartHost(string ip, string port)
    {
        /*_connectUseCase.StartHost(ip, port);
        _websocketusecase.Connect(ip, "8080");
        _documentationUseCase.loggerSetup(Application.persistentDataPath);*/
    }

    public void SetToOnline(/*INetworkManager manager*/)
    {
        SceneManager.LoadScene("AuthMenu");
        //_connectUseCase.SetToOnline(manager);
    }

    public void ChangeToOfflineScene()
    {
        SceneManager.LoadScene("TutorialScene");
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

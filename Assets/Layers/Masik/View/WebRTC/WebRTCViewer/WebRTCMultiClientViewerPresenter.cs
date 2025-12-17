using System.Threading;
using Unity.WebRTC;
using UnityEngine;
using VContainer;

public class WebRTCMultiClientViewerPresenter : MonoBehaviour
{
    private SynchronizationContext _mainThreadContext;


    private WebRTCMultiClientViewingUsecase _usecase;

    [Inject]
    private WebSocketStreamingClientService _webSocketStreamingClientService;
    [Inject]
    private WebSocketClientService _webSocketClientService;
    [Inject]
    private WebRTCViewerMessageHandlerService _webRTCViewerMessageHandlerService;

    [Inject]
    void Awake()
    {
        
    }

    public event VideoStreamReceivedHandler VideoStreamReceived;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _usecase = new WebRTCMultiClientViewingUsecase(_webSocketClientService, _webSocketStreamingClientService, _webRTCViewerMessageHandlerService);
        _mainThreadContext = SynchronizationContext.Current;
        _usecase.OnVideoStreamReceived(OnVideoStreamReceived);
        _usecase.OnDebugMessageReceived(Log);

    }

    private void OnVideoStreamReceived(VideoStreamTrack videoStreamTrack)
    {

        if (videoStreamTrack != null)
        {
            if (SynchronizationContext.Current == _mainThreadContext)
            {
                VideoStreamReceived?.Invoke(videoStreamTrack);
            }
            else
            {
                _mainThreadContext.Post(_ =>
                {
                    VideoStreamReceived?.Invoke(videoStreamTrack);
                }, null);
            }
        }
        else
        {
            Debug.LogError("Video Stream is null.");
        }
    }
    public void Disconnect()
    {
        _usecase.StopCurrentConnetionAndOpenNew();
    }
    private void Log(string message)
    {
        Debug.Log(message);
    }
    // Update is called once per frame
    void Update()
    {

    }
    // OnDestroy is called when the MonoBehaviour will be destroyed it is used to clean up resources.
    // This is just a temporary implementation.
    void OnDestroy()
    {
        _usecase.StopConnection();
    }
}

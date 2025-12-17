using System.Threading;
using Unity.WebRTC;
using UnityEngine;
using VContainer;

public delegate void VideoStreamReceivedHandler(VideoStreamTrack videoStreamTrack);
public class WebRTCViewerPresenter : MonoBehaviour
{

    private SynchronizationContext _mainThreadContext;


    private WebRTCViewingUsecase _usecase;

    [Inject]
    private WebSocketStreamingClientService _webSocketStreamingClientService;
    [Inject]
    private WebSocketClientService _webSocketClientService;
    [Inject]
    private WebRTCService _webRTCService;
    [Inject]
    private WebRTCMessageHandlerService _webRTCMessageHandlerService;

    [Inject]
    void Awake()
    {
        _usecase = new WebRTCViewingUsecase(_webSocketClientService, _webSocketStreamingClientService, _webRTCMessageHandlerService, _webRTCService);
    }

    public event VideoStreamReceivedHandler VideoStreamReceived;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _mainThreadContext = SynchronizationContext.Current;
        _usecase.OnVideoStreamReceived(OnVideoStreamReceived);


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

    // Update is called once per frame
    void Update()
    {
        
    }
}

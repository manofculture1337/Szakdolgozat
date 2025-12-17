
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VContainer;
using Unity.WebRTC;
using System.Threading;


public delegate void JobDone();
public class WebRTCStreamerPresenter : MonoBehaviour
{

    private SynchronizationContext _mainThreadContext;

    public event JobDone ConnectionStabilized;
    public event JobDone ViewersIDsRecived;

    private WebRTCStreamingUsecase _usecase;

    private List<uint> _viewerIDs;

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
        _usecase = new WebRTCStreamingUsecase(_webSocketClientService, _webSocketStreamingClientService, _webRTCMessageHandlerService, _webRTCService);
    }

    void Start()
    {
        _mainThreadContext = SynchronizationContext.Current;
        _usecase.PairUpDone(Connect);
        _viewerIDs = new List<uint>();

    }
    void Update()
    {
        if (_ConnectionDone)
        {
            if (_usecase.GetSignalingState() == RTCSignalingState.Stable)
            {
                _ConnectionDone = false;
                ConnectionStabilized?.Invoke();
            }
        }
    }
    //After pairing up with viewer, connect WebRTC
    private void Connect()
    {
        _usecase.OnConnectionDone(() => _ConnectionDone = true);
        _usecase.Connect();
    }

    private bool _ConnectionDone = false;
    private string _viewerId = "";
    public void Refresh()
    {
        _usecase.GetPossibleViewersTask(OnViewerIDsRecived);
    }
    public List<uint> GetViewerIDs() =>_viewerIDs;
    private void OnViewerIDsRecived(List<uint> viewerIDs)
    {
        if (SynchronizationContext.Current == _mainThreadContext)
        {
            _viewerIDs = viewerIDs;
            ViewersIDsRecived?.Invoke();
        }
        else
        {
            _mainThreadContext.Post(_ =>
            {
                _viewerIDs = viewerIDs;
                ViewersIDsRecived?.Invoke();
            }, null);
        }
    }
    public void Call(string viewerID)
    {


        if (uint.TryParse(viewerID, out uint viewerIdInt))
        {
            _usecase.PairUp(viewerIdInt);
            _viewerId = viewerID;
        }
        else
        {
            Debug.LogError("Failed to parse viewer ID: " + viewerID);
        }

    }
    public void StartStream(Camera camera)
    {

        var videoStreamTrack = camera.CaptureStreamTrack(1280, 720);
        //var videoStreamTrack = _camera.CaptureStreamTrack(640, 360);
        _usecase.SendVideoTrack(videoStreamTrack);
    }
    public string GetViewerId()
    {
        return _viewerId;
    }
}
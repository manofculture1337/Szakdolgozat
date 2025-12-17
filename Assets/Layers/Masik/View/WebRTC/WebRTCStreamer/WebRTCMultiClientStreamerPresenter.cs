using System.Collections.Generic;
using System.Threading;
using System;
using Unity.VisualScripting;
using Unity.WebRTC;
using UnityEngine;
using VContainer;

public class WebRTCMultiClientStreamerPresenter : MonoBehaviour
{

    private SynchronizationContext _mainThreadContext;

    //public event JobDone ConnectionStabilized;
    public event JobDone ViewersIDsRecived;
    public event Action<string> OnViewerConnected;
    public event Action<string> OnViewerDisconnected;
    public event Action<string> OnPairingFailed;

    private WebRTCMultiClientStreamingUsecase _usecase;

    private string lastID = "";
    private List<uint> _viewerIDs;

    [Inject]
    private WebSocketStreamingClientService _webSocketStreamingClientService;
    [Inject]
    private WebSocketClientService _webSocketClientService;
    [Inject]
    private WebRTCStreamerMessageHandlerService _webRTCStreamerMessageHandlerService;

    [Inject]
    void Awake()
    {
        _usecase = new WebRTCMultiClientStreamingUsecase(_webSocketClientService, _webSocketStreamingClientService, _webRTCStreamerMessageHandlerService);
    }
    //VIEW:  set video source, List viewer IDs, select one to call instant call, 
    void Start()
    {
        _mainThreadContext = SynchronizationContext.Current;
        _usecase.PairUpDone(Connect);
        _viewerIDs = new List<uint>();
        _usecase.onDebugMessageReceived(Log);
        _usecase.OnConnectionDone(OnConnected);
        _usecase.OnViewerDisconnected(OnDisconnected);

    }
    void Update()
    {
        /*
        if (_ConnectionDone)
        {
            if (_usecase.GetSignalingState() == RTCSignalingState.Stable)
            {
                _ConnectionDone = false;
                ConnectionStabilized?.Invoke();
            }
        }
        */
    }
    //After pairing up with viewer, connect WebRTC
    private void Connect(string id)
    {
        if(string.IsNullOrEmpty(id))
        {
            if (SynchronizationContext.Current == _mainThreadContext)
            {
                OnPairingFailed?.Invoke(lastID);
            }
            else
            {
                _mainThreadContext.Post(_ =>
                {
                    OnPairingFailed?.Invoke(lastID);
                }, null);
            }
            return;
        }
        //TDOD: make separate ConnecntionDone for all viewers
        //_usecase.OnConnectionDone((string id) => _ConnectionDone = true);
        _usecase.ConnectViewer(id);
    }

    //private bool _ConnectionDone = false;
    //private List<string> _connectedViewerIds = new List<string>();
    public void Refresh()
    {
        _usecase.GetPossibleViewersTask(OnViewerIDsRecived);
    }
    public List<uint> GetViewerIDs() => _viewerIDs;
    private void OnViewerIDsRecived(List<uint> viewerIDs)
    {
        if (SynchronizationContext.Current == _mainThreadContext)
        {
            _viewerIDs = viewerIDs;
            _viewerIDs.AddRange(_usecase.GetConnectedViewersIDs());
            //_viewerIDs.RemoveAll(id => _connectedViewerIds.Contains(id.ToString()));
            ViewersIDsRecived?.Invoke();
        }
        else
        {
            _mainThreadContext.Post(_ =>
            {
                _viewerIDs = viewerIDs;
                _viewerIDs.AddRange(_usecase.GetConnectedViewersIDs());
                //_viewerIDs.RemoveAll(id => _connectedViewerIds.Contains(id.ToString()));
                ViewersIDsRecived?.Invoke();
            }, null);
        }
    }
    public void Call(string viewerID)
    {


        if (uint.TryParse(viewerID, out uint viewerIdInt))
        {
            if(_usecase.IsConnectedTo(viewerIdInt)){
                _usecase.RemoveViewer(viewerIdInt);
            }
            else{
                lastID = viewerID;
                _usecase.PairUp(viewerIdInt);
            }
            //_connectedViewerIds.Add(viewerID);
        }
        else
        {
            Debug.LogError("Failed to parse viewer ID: " + viewerID);
        }

    }
    public void SetStream(Camera camera)
    {

        var videoStreamTrack = camera.CaptureStreamTrack(1280, 720);
        //var videoStreamTrack = _camera.CaptureStreamTrack(640, 360);
        _usecase.SetVideoTrack(videoStreamTrack);
    }
    private void Log(string message)
    {
        Debug.Log("[WebRTCMultiClientStreamerPresenter]: " + message);
    }
    private void OnConnected(string viewerID)
    {
        if (SynchronizationContext.Current == _mainThreadContext)
        {
            //_connectedViewerIds.Add(viewerID);
            OnViewerConnected?.Invoke(viewerID);
        }
        else
        {
            _mainThreadContext.Post(_ =>
            {
                //_connectedViewerIds.Add(viewerID);
                OnViewerConnected?.Invoke(viewerID);
            }, null);
        }
    }
    private void OnDisconnected(string viewerID)
    {
        if (SynchronizationContext.Current == _mainThreadContext)
        {
            //_connectedViewerIds.Remove(viewerID);
            OnViewerDisconnected?.Invoke(viewerID);
        }
        else
        {
            _mainThreadContext.Post(_ =>
            {
                //_connectedViewerIds.Remove(viewerID);
                OnViewerDisconnected?.Invoke(viewerID);
            }, null);
        }
    }
    public void RemoveViewer(uint viewerID)
    {
        _usecase.RemoveViewer(viewerID);
    }
    public bool IsConnectedTo(string viewerID)
    {

        if (uint.TryParse(viewerID, out uint viewerIdInt))
        {
            return _usecase.IsConnectedTo(viewerIdInt);
        }
        else
        {
            Debug.LogError("Failed to parse viewer ID: " + viewerID);
            return false;
        }
    }
    //Temporary fix for stopping all 
    private void OnDestroy()
    {
        _usecase.StopAll();
    }
}

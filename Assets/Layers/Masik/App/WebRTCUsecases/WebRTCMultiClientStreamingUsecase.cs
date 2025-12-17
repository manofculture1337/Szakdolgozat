
using System;
using System.Collections.Generic;
using Unity.WebRTC;
using VContainer;

public class WebRTCMultiClientStreamingUsecase
{
    private readonly WebSocketStreamingClientService _webSocketStreamingClientService;
    private readonly WebSocketClientService _webSocketClientService;
    private readonly WebRTCStreamerMessageHandlerService _webRTCStreamerMessageHandlerService;
    //private readonly WebRTCStreamerService _webRTCStreamerService;

    [Inject]
    public WebRTCMultiClientStreamingUsecase(WebSocketClientService webSocketClientService, WebSocketStreamingClientService webSocketStreamingClientService, WebRTCStreamerMessageHandlerService webRTCStreamerMessageHandlerService)
    {
        this._webSocketClientService = webSocketClientService;
        this._webSocketStreamingClientService = webSocketStreamingClientService;
        this._webRTCStreamerMessageHandlerService = webRTCStreamerMessageHandlerService;
        //this._webRTCStreamerService = WebRTCStreamerService.Instance;
        this._webRTCStreamerMessageHandlerService.SetServices(this._webSocketClientService, this._webSocketStreamingClientService);
        this._webSocketStreamingClientService.SetNewVersion(true);

    }
    //If pair up is done, signaling is ready to start negotiation call ConnectViewer with viewerId
    public void PairUpDone(Action<string> callback)
    {
        _webSocketStreamingClientService.ListenToPairUpDoneWithID(callback);
    }
    //Not so useful method
    public void OnConnectionDone(Action<string> callback)
    {
        WebRTCStreamerService.Instance.OnViewerConnected += delegate (string message)
        {
            callback(message);
        };
    }
    //First connect to signaling server, then start negotiation
    public void PairUp(uint viewerId)
    {
        _webSocketStreamingClientService.PairUp(viewerId);
    }
    public void GetPossibleViewersTask(Action<List<uint>> listener)
    {
        _webSocketStreamingClientService.GetPossibleViewersTask(listener);
    }
    //Set tracks to be streamed to viewers call PairUp after this
    public void SetVideoTrack(VideoStreamTrack videoStreamTrack)
    {
        WebRTCStreamerService.Instance.SetVideoTrack(videoStreamTrack);
    }
    public void SetAudioTrack(AudioStreamTrack audioStreamTrack)
    {
        WebRTCStreamerService.Instance.SetAudioTrack(audioStreamTrack);
    }
    public void RemoveViewer(uint viewerId)
    {
        WebRTCStreamerService.Instance.RemoveViewer(viewerId.ToString());
    }
    public void OnViewerDisconnected(Action<string> callback)
    {
        WebRTCStreamerService.Instance.OnViewerDisconnected += delegate (string message)
        {
            callback(message);
        };
    }
    public void StopAll()
    {
        WebRTCStreamerService.Instance.StopAll();
    }
    //After pairing up with viewer, connect WebRTC
    public void ConnectViewer(string viewerId)
    {
        WebRTCStreamerService.Instance.CreateConnectionForViewer(viewerId.ToString());
    }
    public void onDebugMessageReceived(Action<string> callback)
    {
        WebRTCStreamerService.Instance.OnDebugMessage += delegate (string message)
        {
            callback(message);
        };
        WebRTCStreamerMessageHandlerService.Instance.DisplayDebugMessage += delegate (string message)
        {
            callback(message);
        };
    }

    public bool IsConnectedTo(uint viewerID)
    {
        return WebRTCStreamerService.Instance.IsViewer(viewerID.ToString());
    }
    public List<uint> GetConnectedViewersIDs()
    {
        List<uint> viewerIDs = new List<uint>();
        foreach (var id in WebRTCStreamerService.Instance.GetViewerIDs())
        {
            viewerIDs.Add(Convert.ToUInt32(id));
        }
        return viewerIDs;
    }
}

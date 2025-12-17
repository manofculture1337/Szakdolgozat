
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.WebRTC;
using VContainer;

public class WebRTCStreamingUsecase
{
    private readonly WebSocketStreamingClientService _webSocketStreamingClientService;
    private readonly WebSocketClientService _webSocketClientService;
    private readonly WebRTCMessageHandlerService _webRTCMessageHandlerService;
    private readonly WebRTCService _webRTCService;

    [Inject]
    public WebRTCStreamingUsecase(WebSocketClientService webSocketClientService, WebSocketStreamingClientService webSocketStreamingClientService, WebRTCMessageHandlerService webRTCMessageHandlerService, WebRTCService webRTCService)
    {
        this._webSocketClientService = webSocketClientService;
        this._webSocketStreamingClientService = webSocketStreamingClientService;
        this._webRTCMessageHandlerService = webRTCMessageHandlerService;
        this._webRTCService = webRTCService;
        this._webRTCMessageHandlerService.SetServices(this._webSocketClientService, this._webSocketStreamingClientService, this._webRTCService);
        this._webRTCService.SetServices(this._webRTCMessageHandlerService);

    }
    public void Connect()
    {
        _webRTCService.Negotiate();
    }
    public void PairUpDone(System.Action callback)
    {
        _webSocketStreamingClientService.ListenToPairUpDone(callback);
    }
    public void OnConnectionDone(System.Action callback)
    {
        _webRTCService.ListenConnectionDone(callback);
    }
    public void PairUp(uint viewerId)
    {
        _webSocketStreamingClientService.PairUp(viewerId);
    }
    public void SendVideoTrack(VideoStreamTrack videoStreamTrack)
    {
        _webRTCService.SendVideoTrack(videoStreamTrack);
    }
    public void SendAudioTrack(AudioStreamTrack audioStreamTrack)
    {
        _webRTCService.SendAudioTrack(audioStreamTrack);
    }
    public void GetPossibleViewersTask(Action<List<uint>> listener)
    {
        _webSocketStreamingClientService.GetPossibleViewersTask(listener);
    }
    public RTCSignalingState GetSignalingState()
    {
        return _webRTCService.Instance.SignalingState;
    }
}


using System;
using Unity.WebRTC;
using VContainer;

public class WebRTCViewingUsecase
{
    private readonly WebSocketStreamingClientService _webSocketStreamingClientService;
    private readonly WebSocketClientService _webSocketClientService;
    private readonly WebRTCMessageHandlerService _webRTCMessageHandlerService;
    private readonly WebRTCService _webRTCService;

    [Inject]
    public WebRTCViewingUsecase(WebSocketClientService webSocketClientService, WebSocketStreamingClientService webSocketStreamingClientService, WebRTCMessageHandlerService webRTCMessageHandlerService, WebRTCService webRTCService)
    {
        this._webSocketClientService = webSocketClientService;
        this._webSocketStreamingClientService = webSocketStreamingClientService;
        this._webRTCMessageHandlerService = webRTCMessageHandlerService;
        this._webRTCService = webRTCService;
        this._webRTCMessageHandlerService.SetServices(this._webSocketClientService, this._webSocketStreamingClientService, this._webRTCService);
        this._webRTCService.SetServices(this._webRTCMessageHandlerService);

    }
    public void OnVideoStreamReceived(Action<VideoStreamTrack> callback)
    {
        _webRTCService.Instance.OnVideoStreamReceived += delegate (VideoStreamTrack videoStreamTrack)
        {
            callback(videoStreamTrack);
        };
    }
}

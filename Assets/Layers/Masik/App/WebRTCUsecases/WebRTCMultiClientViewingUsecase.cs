
using System;
using Unity.WebRTC;
using VContainer;

public class WebRTCMultiClientViewingUsecase
{
    private readonly WebSocketStreamingClientService _webSocketStreamingClientService;
    private readonly WebSocketClientService _webSocketClientService;
    private readonly WebRTCViewerMessageHandlerService _webRTCViewerMessageHandlerService;
    //private readonly WebRTCViewerService _webRTCViewerService;

    [Inject]
    public WebRTCMultiClientViewingUsecase(WebSocketClientService webSocketClientService, WebSocketStreamingClientService webSocketStreamingClientService, WebRTCViewerMessageHandlerService webRTCViewerMessageHandlerService)
    {
        this._webSocketClientService = webSocketClientService;
        this._webSocketStreamingClientService = webSocketStreamingClientService;
        this._webRTCViewerMessageHandlerService = webRTCViewerMessageHandlerService;
        //this._webRTCViewerService = WebRTCViewerService.Instance;
        this._webRTCViewerMessageHandlerService.SetServices(this._webSocketClientService, this._webSocketStreamingClientService);
        
        WebRTCViewerService.Instance.CreateConnection();

    }
    public void OnVideoStreamReceived(Action<VideoStreamTrack> callback)
    {
        WebRTCViewerService.Instance.OnVideoReceived += delegate (VideoStreamTrack videoStreamTrack)
        {
            callback(videoStreamTrack);
        };
    }
    public void OnAudioStreamReceived(Action<AudioStreamTrack> callback)
    {
        WebRTCViewerService.Instance.OnAudioReceived += delegate (AudioStreamTrack audioStreamTrack)
        {
            callback(audioStreamTrack);
        };
    }
    public void OnDebugMessageReceived(Action<string> callback)
    {
        WebRTCViewerService.Instance.OnDebugMessage += delegate (string message)
        {
            callback(message);
        };
        WebRTCViewerMessageHandlerService.Instance.DisplayDebugMessage += delegate (string message)
        {
            callback(message);
        };
    }
    public void StopConnection()
    {
        WebRTCViewerService.Instance.StopAll();
    }
    public void StopCurrentConnetionAndOpenNew()
    {
        WebRTCViewerService.Instance.Disconnect();
        WebRTCViewerService.Instance.CreateConnection();
    }
}


using System;
using VContainer;

public class WebSocketStreamingClientUsecase
{
    private readonly WebSocketStreamingClientService _service;
    private readonly WebSocketClientService _webSocketClientService;
    [Inject]
    public WebSocketStreamingClientUsecase(WebSocketStreamingClientService webSocketStreamingClientService, WebSocketClientService webSocketClientService)
    {
        this._service = webSocketStreamingClientService;
        this._webSocketClientService = webSocketClientService;
        webSocketStreamingClientService.SetServices( this._webSocketClientService);
    }
    public void ConnectToStreamingAsStreamer()
    {
        _service.ConnectToStreaming(WebSocketEnums.ConnectionType.Streamer);
    }
    public void ConnectToStreamingAsViewer()
    {
        _service.ConnectToStreaming(WebSocketEnums.ConnectionType.Viewer);
    }
    public void OnConnectionStatusChanged(Action<WebSocketEnums.ConnectionStatus> listener)
    {
        _service.Instance.ConnectionStatusChanged += delegate (WebSocketEnums.ConnectionStatus status)
        {
            listener(status);
        };
    }
    public WebSocketEnums.ConnectionStatus GetCurrentConnectionStatus()
    {
        return _service.Status;
    }
    public WebSocketEnums.ConnectionType GetCurrentConnectionType()
    {
        return _service.Type;
    }
}

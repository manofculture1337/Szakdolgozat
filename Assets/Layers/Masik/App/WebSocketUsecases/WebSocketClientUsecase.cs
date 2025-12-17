using System;
using VContainer;
using WebSocketSharp;
public class WebSocketClientUsecase
{
    private readonly WebSocketClientService _webSocketClientService;
    [Inject]
    public WebSocketClientUsecase(WebSocketClientService webSocketClientService)
    {
        this._webSocketClientService = webSocketClientService;
    }
    public int Connect(string serverIp, string serverPort)
    {
        int serverPortInt;
        if(!int.TryParse(serverPort, out serverPortInt))
            serverPortInt = 8080;
        if (serverPortInt < 1 || serverPortInt > 65535)
            serverPortInt = 8080;
        _webSocketClientService.Connect(serverIp, serverPortInt);
        return serverPortInt;
    }
    public void Disconnect()
    {
        _webSocketClientService.Disconnect();
    }
    public void SendMessage(string message)
    {
        _webSocketClientService.SendMessage(message);
    }
    public void SendMessage(DTOMessageWrapper dTOMessage)
    {
        _webSocketClientService.SendMessage(dTOMessage);
    }
    public void SendMessageToServer(DTOMessageWrapper dTOMessage)
    {
        _webSocketClientService.SendMessageToServer(dTOMessage);
    }

    public void SendMessageToClient(DTOMessageWrapper dTOMessage)
    {
        _webSocketClientService.SendMessageToClient(dTOMessage);
    }

    public void OnWebSocketStateChange(Action<WebSocketState> listener)
    {
        _webSocketClientService.Instance.ConnectionStatusChanged += delegate (WebSocketState state)
        {
            listener(state);
        };
    }

}

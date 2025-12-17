
using System;
using System.Collections.Concurrent;
using WebSocketSharp;

public delegate void MessageHandler(string message);
public delegate void DisplayMessageHandler(string message);
public delegate void ConnectionStatusHandler(WebSocketState ConnectionState);

public class WebSocketClientService
{
    private static readonly int sleepDurationMs = 100;
    private static WebSocketClientService _instance;
    public WebSocketClientService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new WebSocketClientService();
            }
            return _instance;
        }
    }
    private WebSocketClient _webSocketClient;
    public WebSocketState State => Instance._webSocketClient != null && Instance._webSocketClient.WebSocket != null ? Instance._webSocketClient.State : WebSocketState.Closed;
    public event DisplayMessageHandler DisplayDebugMessage;
    public event ConnectionStatusHandler ConnectionStatusChanged;
    public event MessageHandler MessageReceived;
    public void Connect(string serverIp, int serverPort) => Instance.InstanceConnect(serverIp, serverPort);

    public void Disconnect() => Instance.InstanceDisconnect();

    public void SendMessage(string message) => Instance.InstanceSendMessage(message);
    
    public void SendMessage(DTOMessageWrapper dTOMessage)
    {
        string message = DTOMessageWrapper.ConvertToMessage(dTOMessage);
        SendMessage(message);
    }
    public void SendMessageToServer(DTOMessageWrapper dTOMessage)
    {
        DTOMessageWrapper dTOMessageToSend = new DTOMessageWrapper
        {
            Type = (int)WebSocketEnums.MessageType.ToServer,
            Payload = dTOMessage
        };
        SendMessage(dTOMessageToSend);
    }
    public void SendMessageToClient(DTOMessageWrapper dTOMessage)
    {
        DTOMessageWrapper dTOMessageToSend = new DTOMessageWrapper
        {
            Type = (int)WebSocketEnums.MessageType.ToOtherClient,
            Payload = dTOMessage
        };
        SendMessage(dTOMessageToSend);
    }
    private void InstanceConnect(string serverIp, int serverPort)
    {
        if (_webSocketClient == null)
        {
            _webSocketClient = new WebSocketClient();
        }
        if (_webSocketClient.WebSocket != null &&
            (_webSocketClient.WebSocket.ReadyState == WebSocketState.Open || _webSocketClient.WebSocket.ReadyState == WebSocketState.Connecting))
        {
            DisplayDebugMessage?.Invoke("WebSocket is already connected or connecting.");
            return;
        }

        _webSocketClient.ServerIp = serverIp;
        _webSocketClient.ServerPort = serverPort;
        _webSocketClient.WebSocket = new WebSocket($"ws://{_webSocketClient.ServerIp}:{_webSocketClient.ServerPort}/{WebSocketClient.WebSocketServicePath}");
        _webSocketClient.ReceivedMessages = new ConcurrentQueue<string>();
        _webSocketClient.ReceivedErrors = new ConcurrentQueue<string>();
        StartListening();
        _webSocketClient.WebSocket.ConnectAsync();
        StartMessageProcessors();
    }

    private void StartListening()
    {
        _webSocketClient.WebSocket.OnOpen += OnOpen;
        _webSocketClient.WebSocket.OnMessage += OnMessage;
        _webSocketClient.WebSocket.OnError += OnError;
        _webSocketClient.WebSocket.OnClose += OnClose;
    }
    private void OnOpen(object sender, EventArgs e)
    {
        DisplayDebugMessage?.Invoke("WebSocket connection opened.");
        ConnectionStatusChanged?.Invoke(_webSocketClient.WebSocket.ReadyState);
    }
    private void OnMessage(object sender, MessageEventArgs e)
    {
        DisplayDebugMessage?.Invoke("Message received: " +  e.Data);
        _webSocketClient.ReceivedMessages.Enqueue(e.Data);
    }
    private void OnError(object sender, ErrorEventArgs e)
    {
        DisplayDebugMessage?.Invoke("WebSocket error recived " + e.Message);
        _webSocketClient.ReceivedErrors.Enqueue(e.Message);
    }
    private void OnClose(object sender, CloseEventArgs e)
    {
        DisplayDebugMessage?.Invoke("WebSocket connection closed.");
        ConnectionStatusChanged?.Invoke(_webSocketClient.WebSocket.ReadyState);
        _webSocketClient.WebSocket.OnClose -= OnClose;
        _webSocketClient.WebSocket.OnError -= OnError;
        _webSocketClient.WebSocket.OnMessage -= OnMessage;
        _webSocketClient.WebSocket.OnOpen -= OnOpen;
        _webSocketClient.WebSocket = null;
    }
    private void StartMessageProcessors()
    {
        // Start a background task to process received messages
        System.Threading.Tasks.Task.Run(() =>
        {
            while (true)
            {
                if (_webSocketClient.ReceivedMessages.TryDequeue(out string message))
                {
                    MessageReceived?.Invoke(message);
                }
                System.Threading.Thread.Sleep(sleepDurationMs);
            }
        });
        // Start a background task to process received errors
        System.Threading.Tasks.Task.Run(() =>
        {
            while (true)
            {
                if (_webSocketClient.ReceivedErrors.TryDequeue(out string error))
                {
                    DisplayDebugMessage?.Invoke("WebSocket error: " + error);
                }
                System.Threading.Thread.Sleep(sleepDurationMs);
            }
        });
    }


    private void InstanceSendMessage(string message)
    {
        if(_webSocketClient.WebSocket != null && _webSocketClient.WebSocket.ReadyState == WebSocketState.Open)
        {
            _webSocketClient.WebSocket.Send(message);
            DisplayDebugMessage?.Invoke("Message sent: " + message);
        }
        else
        {
            DisplayDebugMessage?.Invoke("WebSocket is not connected. Cannot send message.");
            throw new InvalidOperationException("WebSocket is not connected. Cannot send message.");
        }
    }
    private void InstanceDisconnect()
    {
        if (_webSocketClient.WebSocket != null && _webSocketClient.WebSocket.ReadyState == WebSocketState.Open)
        {
            _webSocketClient.WebSocket.CloseAsync();
        }
        else if (_webSocketClient.WebSocket != null && _webSocketClient.WebSocket.ReadyState == WebSocketState.Connecting)
        {
            DisplayDebugMessage?.Invoke("WebSocket is connecting. Please wait for the connection to open before disconnecting.");
        }
        else
        {
            DisplayDebugMessage?.Invoke("WebSocket is not connected.");
        }
    }
}

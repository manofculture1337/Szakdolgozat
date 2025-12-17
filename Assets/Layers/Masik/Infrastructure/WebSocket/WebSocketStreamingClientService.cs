
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebSocketSharp;

public delegate void DTOMessageHandler(DTOMessageWrapper message);
public delegate void DoneEventHandler();
public delegate void WebSocketStreamingStatusHandler(WebSocketEnums.ConnectionStatus connectionStatus);
public class WebSocketStreamingClientService
{
    private bool newVersion = false;
    public void SetNewVersion(bool isNewVersion)
    {
        Instance.newVersion = isNewVersion;
    }
    public event DisplayMessageHandler DisplayDebugMessage;

    public event DTOMessageHandler StreamingMessageReceived;
    public event DTOMessageHandler OkAnswerRecived;
    public event DoneEventHandler PaierUpDone;
    public event WebSocketStreamingStatusHandler ConnectionStatusChanged;

    public event Action<string> PaierUpDoneWithID;

    private readonly WebSocketStreamingClient _webSocketStreamingClient = new WebSocketStreamingClient();
    public WebSocketEnums.ConnectionStatus Status => _instance._webSocketStreamingClient.ConnectionToStreamingStatus;
    public WebSocketEnums.ConnectionType Type => _instance._webSocketStreamingClient.ConnectionType;
    private static WebSocketStreamingClientService _instance;
    public WebSocketStreamingClientService Instance
    {
        get
        {
            return _instance;
        }
    }

    private WebSocketClientService _webSocketClientService;
    public void SetServices(WebSocketClientService webSocketClientService)
    {
        if (_instance == null)
        {
            _instance = new WebSocketStreamingClientService();
            _instance._webSocketClientService = webSocketClientService;
        }
    }

    //_webSocketClientService.MessageReceived += OnWebSocketMessageRecived;
    public void ConnectToStreaming(WebSocketEnums.ConnectionType connectionType) => _instance.InstanceConnectToStreaming(connectionType);
    public void SendWebSocketMessageToPairClient(string message, int messageID) => _instance.InstanceSendWebSocketMessageToPairClient(message, messageID);
    public void GetPossibleViewersTask(Action<List<uint>> listener) => _instance.InstanceGetPossibleViewersTask(listener);
    public void PairUp(uint viewerId) => _instance.InstancePairUp(viewerId);
    public void ListenToPairUpDone(System.Action listener)
    {
        _instance.PaierUpDone += delegate
        {
            listener();
        };
    }
    public void ListenToPairUpDoneWithID(System.Action<string> listener)
    {
        _instance.PaierUpDoneWithID += delegate (string id)
        {
            listener(id);
        };
    }
    public void SendWebSocketMessageToPairClientWithID(string message, int messageID, uint pairID)
    {
        _instance.InstanceSendWebSocketMessageToPairClientWithIDOrWithoutID(message, messageID, pairID);
    }

    public void Unpair() => _instance.InstanceUnpair();
    public void DisconnectedFrom(uint pairID) => _instance.InstanceDisconnectedFrom(pairID);
    private void InstanceSendWebSocketMessageToPairClientWithIDOrWithoutID(string message, int messageID, uint pairID)
    {
        if (_webSocketStreamingClient.ConnectionType == WebSocketEnums.ConnectionType.Viewer
            && _webSocketStreamingClient.PairID == pairID)
        {
            _instance.InstanceSendWebSocketMessageToPairClient(message, messageID);
        }
        else if (_webSocketStreamingClient.ConnectionType == WebSocketEnums.ConnectionType.Streamer)
        {
            _instance.InstanceSendWebSocketMessageToPairClientWithID(message, messageID, pairID);
        }
    }
    private void InstanceConnectToStreaming(WebSocketEnums.ConnectionType connectionType)
    {
        if (connectionType == WebSocketEnums.ConnectionType.None)
        {
            DisplayDebugMessage?.Invoke($"ConnectionType is None. Cannot connect.");
            return;
        }
        if ((_webSocketClientService.State == WebSocketState.Open) &&
                (
                Status == WebSocketEnums.ConnectionStatus.NotConnected ||
                Status == WebSocketEnums.ConnectionStatus.ConnectionFailed
                )
            )
        {
            _webSocketStreamingClient.ConnectionType = connectionType;
            _webSocketStreamingClient.ConnectionToStreamingStatus = WebSocketEnums.ConnectionStatus.ConnectionRequested;
            ConnectionStatusChanged?.Invoke(Status);
            DisplayDebugMessage?.Invoke($"Requesting connection as {Type}.");

            DTOMessageWrapper dTOMessage = new DTOMessageWrapper
            {
                Type = (int)(
                        connectionType == WebSocketEnums.ConnectionType.Viewer ?
                            WebSocketEnums.ToServerMessageType.ToJoinAsViewer :
                        connectionType == WebSocketEnums.ConnectionType.Streamer ?
                            WebSocketEnums.ToServerMessageType.ToJoinAsStreamer :
                        WebSocketEnums.ToServerMessageType.ToJoinAsViewer
                    ),
                Message = $"Requesting connection as {connectionType}",
                Payload = null
            };
            _webSocketClientService.SendMessageToServer(dTOMessage);
            _webSocketClientService.Instance.MessageReceived += OnMessageReceived;
        }

        else if ((_webSocketClientService.State != WebSocketState.Open))
        {
            DisplayDebugMessage?.Invoke($"Not Connected to WebSocketServer");
            return;
        }

        else if (Status == WebSocketEnums.ConnectionStatus.ConnectionRequested)
        {
            DisplayDebugMessage?.Invoke($"Already requested connection as {Type}.");
            return;
        }

        else if (Status == WebSocketEnums.ConnectionStatus.Connected)
        {
            DisplayDebugMessage?.Invoke($"Already connected as {Type}.");
            return;
        }
    }

    private void OnMessageReceived(string message)
    {
        if (_webSocketClientService.State != WebSocketState.Open)
        {
            DisplayDebugMessage?.Invoke($"WebSocket is not open. Current state: {_webSocketClientService.State}");
            return;
        }
        if (Status == WebSocketEnums.ConnectionStatus.ConnectionRequested)
        {
            HandleConnectionRequestReply(message);
        }
        else if (Status == WebSocketEnums.ConnectionStatus.Connected)
        {
            HandleMessage(message);
        }
    }

    private void HandleConnectionRequestReply(string message)
    {
        DTOMessageWrapper dTOMessage = DTOMessageWrapper.ConvertFromMessage(message);
        switch ((WebSocketEnums.AnswerType)dTOMessage.Type)
        {
            case WebSocketEnums.AnswerType.Ok:
                _webSocketStreamingClient.ConnectionToStreamingStatus = WebSocketEnums.ConnectionStatus.Connected;
                ConnectionStatusChanged?.Invoke(Status);

                if (int.TryParse(dTOMessage.Message, out int parsedId))
                {
                    _webSocketStreamingClient.IDOnServer = parsedId;
                }
                else
                {
                    DisplayDebugMessage?.Invoke($"Failed to parse ID from payload: {dTOMessage.Message}");
                    break;
                }
                DisplayDebugMessage?.Invoke($"Connected as {Type} with ID: {_webSocketStreamingClient.IDOnServer}.");
                break;
            case WebSocketEnums.AnswerType.Err:
                _webSocketStreamingClient.ConnectionToStreamingStatus = WebSocketEnums.ConnectionStatus.ConnectionFailed;
                ConnectionStatusChanged?.Invoke(Status);
                DisplayDebugMessage?.Invoke($"Failed to connect as {Type}.");
                break;
            default:
                DisplayDebugMessage?.Invoke($"Unknown Connection request type: {dTOMessage.Type}");
                break;
        }
    }
    private void HandleMessage(string message)
    {
        DTOMessageWrapper dTOMessage = DTOMessageWrapper.ConvertFromMessage(message); //JsonUtility.FromJson<DTOMessageWrapper>(message);
        switch ((WebSocketEnums.AnswerType)dTOMessage.Type)
        {
            case WebSocketEnums.AnswerType.Ok:
                DisplayDebugMessage?.Invoke($"Conformation message received: {dTOMessage.Payload}");
                OkAnswerRecived?.Invoke(dTOMessage);
                break;
            case WebSocketEnums.AnswerType.Relayed:
                HandleRelayedMessage(dTOMessage.Payload);
                break;
            case WebSocketEnums.AnswerType.Paired:
                if (newVersion)
                    HandlePairMessageNew(dTOMessage.Message);
                else
                    HandlePairMessage(dTOMessage.Message);
                break;
            case WebSocketEnums.AnswerType.Logged:
                DisplayDebugMessage?.Invoke($"Message logged on Server.");
                break;
            case WebSocketEnums.AnswerType.ConnectionRequest:
                //TODO: This is not yet used, but we can use it to ask the viewer to pair with a streamer
                DisplayDebugMessage?.Invoke($"Connection request received.");
                break;
            case WebSocketEnums.AnswerType.Err:
                //Temp fix for pair up failure
                DisplayDebugMessage?.Invoke($"Error message recived: {dTOMessage.Message}");
                if(dTOMessage.Message.Contains("pair", StringComparison.OrdinalIgnoreCase))
                {
                    DisplayDebugMessage?.Invoke($"Pair up failed.");
                    PaierUpDone?.Invoke();
                    if (newVersion)
                        PaierUpDoneWithID?.Invoke("");
                }
                break;
            default:
                DisplayDebugMessage?.Invoke($"Unknown message type: {dTOMessage.Type}");
                break;

        }
    }

    private void HandleRelayedMessage(DTOMessageWrapper message)
    {
        DisplayDebugMessage?.Invoke($"Relayed message received");
        StreamingMessageReceived?.Invoke(message);
    }

    private void HandlePairMessage(string message)
    {

        if (uint.TryParse(message, out uint parsedId))
        {
            _webSocketStreamingClient.PairID = parsedId;
            PaierUpDone?.Invoke();
        }
        else
        {
            DisplayDebugMessage?.Invoke($"Failed to parse pair ID from payload: {message}");
        }
    }

    private void HandlePairMessageNew(string message)
    {

        if (uint.TryParse(message, out uint parsedId))
        {
            if (_webSocketStreamingClient.ConnectionType == WebSocketEnums.ConnectionType.Viewer)
            {
                _webSocketStreamingClient.PairID = parsedId;
                PaierUpDone?.Invoke();
                PaierUpDoneWithID?.Invoke(parsedId.ToString());
            }
            else if (_webSocketStreamingClient.ConnectionType == WebSocketEnums.ConnectionType.Streamer)
            {
                _webSocketStreamingClient.Pairs.Add(parsedId);
                PaierUpDone?.Invoke();
                PaierUpDoneWithID?.Invoke(parsedId.ToString());
            }
            else
            {
                DisplayDebugMessage?.Invoke($"Failed to pair Not viewer not streamer ID: {parsedId}");
            }
        }
        else
        {
            DisplayDebugMessage?.Invoke($"Failed to parse pair ID from payload: {message}");
        }
    }

    //The messageID should be bigger than 100, as the basic message types are below that
    private void InstanceSendWebSocketMessageToPairClient(string message, int messageID)
    {
        if (_webSocketClientService.State != WebSocketState.Open)
        {
            DisplayDebugMessage?.Invoke($"WebSocket is not open. Current state: {_webSocketClientService.State}");
            return;
        }
        if (Status != WebSocketEnums.ConnectionStatus.Connected)
        {
            DisplayDebugMessage?.Invoke($"Not connected to WebSocketServer. Current status: {Status}");
            return;
        }
        if (_webSocketStreamingClient.PairID == 0)
        {
            DisplayDebugMessage?.Invoke($"Pair ID is not set. Cannot send message to pair client.");
            return;
        }
        DTOMessageWrapper dTOMessage;
        switch (Type)
        {
            case WebSocketEnums.ConnectionType.Streamer:
                var wrapper = new DTOMessageWrapper
                {
                    Type = messageID,
                    Message = message,
                    Payload = null
                };
                var wrapper1 = new DTOMessageWrapper
                {
                    Type = (int)_webSocketStreamingClient.PairID,
                    Message = "Relayed message",
                    Payload = wrapper
                };
                var wrapper2 = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToViewrMessageType.ToTraget,
                    Payload = wrapper1,
                    Message = "Message to pair client"
                };
                dTOMessage = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToOtherClientMessageType.ToViewer,
                    Payload = wrapper2,
                    Message = "Message to viewer"
                };
                break;
            case WebSocketEnums.ConnectionType.Viewer:
                var wrapper3 = new DTOMessageWrapper
                {
                    Type = messageID,
                    Message = message,
                    Payload = null
                };
                var wrapper4 = new DTOMessageWrapper
                {
                    Type = (int)_webSocketStreamingClient.PairID,
                    Payload = wrapper3,
                    Message = "Relayed message"
                };
                dTOMessage = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToOtherClientMessageType.ToStreamer,
                    Payload = wrapper4,
                    Message = "Message to streamer"
                };
                break;
            default:
                DisplayDebugMessage?.Invoke($"Unknown connection type: {Type}");
                return;
        }

        _webSocketClientService.SendMessageToClient(dTOMessage);
    }
    private void InstanceGetPossibleViewersTask(Action<List<uint>> listener)
    {
        if (Type != WebSocketEnums.ConnectionType.Streamer)
        {
            DisplayDebugMessage?.Invoke($"Not a streamer. Cannot get viewers.");
            return;
        }

        var tcs = new TaskCompletionSource<List<uint>>();
        List<uint> viewers = new List<uint>();

        void OnOkAnswerReceived(DTOMessageWrapper payload)
        {
            DisplayDebugMessage?.Invoke($"Received viewers list: {payload.Message}");
            try
            {

                viewers = payload == null || payload.Message == ",0"
                   ? new List<uint>()
                   : payload.Message//.TrimEnd('0', ',')
                            .Split(',')
                            .Select(uint.Parse)
                            .ToList();
                viewers.Remove(0);
                tcs.TrySetResult(viewers);
                listener?.Invoke(viewers);
            }
            catch (Exception ex)
            {
                DisplayDebugMessage?.Invoke($"Failed to parse viewers list: {ex.Message}");
                tcs.TrySetException(ex);
                listener?.Invoke(new List<uint>());
            }
            finally
            {
                OkAnswerRecived -= OnOkAnswerReceived;
            }
        }

        OkAnswerRecived += OnOkAnswerReceived;

        _webSocketClientService.SendMessageToServer(new DTOMessageWrapper
        {
            Type = (int)WebSocketEnums.ToServerMessageType.ToGetViewrs,
            Message = _webSocketStreamingClient.IDOnServer.ToString(),
            Payload = null
        });

        DisplayDebugMessage?.Invoke($"Requesting viewers for streamer with ID: {_webSocketStreamingClient.IDOnServer}");

    }

    private void InstancePairUp(uint viewerId)
    {
        if (Type != WebSocketEnums.ConnectionType.Streamer)
        {
            DisplayDebugMessage?.Invoke($"Not a streamer. Cannot pair up.");
            return;
        }
        if (Status != WebSocketEnums.ConnectionStatus.Connected)
        {
            DisplayDebugMessage?.Invoke($"Not connected to WebSocketServer. Current status: {Status}");
            return;
        }
        var wrapper1 = new DTOMessageWrapper
        {
            Type = (int)viewerId,
            Message = "Pair request",
            Payload = null
        };
        var wrapper2 = new DTOMessageWrapper
        {
            Type = (int)WebSocketEnums.ToViewrMessageType.ToConnect,
            Payload = wrapper1
        };
        var dTOMessage = new DTOMessageWrapper
        {
            Type = (int)WebSocketEnums.ToOtherClientMessageType.ToViewer,
            Payload = wrapper2
        };
        _webSocketClientService.SendMessageToClient(dTOMessage);
    }


    private void InstanceSendWebSocketMessageToPairClientWithID(string message, int messageID, uint pairID)
    {
        if (_webSocketClientService.State != WebSocketState.Open)
        {
            DisplayDebugMessage?.Invoke($"WebSocket is not open. Current state: {_webSocketClientService.State}");
            return;
        }
        if (Status != WebSocketEnums.ConnectionStatus.Connected)
        {
            DisplayDebugMessage?.Invoke($"Not connected to WebSocketServer. Current status: {Status}");
            return;
        }
        if (!_webSocketStreamingClient.Pairs.Contains(pairID))
        {
            DisplayDebugMessage?.Invoke($"Pair ID:{pairID} is not set. Cannot send message to pair client.");
            return;
        }
        DTOMessageWrapper dTOMessage;
        switch (Type)
        {
            case WebSocketEnums.ConnectionType.Streamer:
                var wrapper = new DTOMessageWrapper
                {
                    Type = messageID,
                    Message = message,
                    Payload = null
                };
                var wrapper1 = new DTOMessageWrapper
                {
                    Type = (int)pairID,
                    Message = "Relayed message",
                    Payload = wrapper
                };
                var wrapper2 = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToViewrMessageType.ToTraget,
                    Payload = wrapper1,
                    Message = "Message to pair client"
                };
                dTOMessage = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToOtherClientMessageType.ToViewer,
                    Payload = wrapper2,
                    Message = "Message to viewer"
                };
                break;
            case WebSocketEnums.ConnectionType.Viewer:
                var wrapper3 = new DTOMessageWrapper
                {
                    Type = messageID,
                    Message = message,
                    Payload = null
                };
                var wrapper4 = new DTOMessageWrapper
                {
                    Type = (int)_webSocketStreamingClient.PairID,
                    Payload = wrapper3,
                    Message = "Relayed message"
                };
                dTOMessage = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToOtherClientMessageType.ToStreamer,
                    Payload = wrapper4,
                    Message = "Message to streamer"
                };
                break;
            default:
                DisplayDebugMessage?.Invoke($"Unknown connection type: {Type}");
                return;
        }

        _webSocketClientService.SendMessageToClient(dTOMessage);
    }
    public int GetIDOnServer()
    {
        return _instance._webSocketStreamingClient.IDOnServer;
    }
    private void InstanceUnpair()
    {
        if ((_webSocketClientService.State == WebSocketState.Open) &&
                (Status == WebSocketEnums.ConnectionStatus.Connected)
            )
        {
            if (Type == WebSocketEnums.ConnectionType.Viewer)
            {
                DTOMessageWrapper dTOMessage = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToServerMessageType.ToUnpair,
                    Message = $"Request unpair from {_webSocketStreamingClient.PairID}",
                    Payload = null
                };
                _webSocketClientService.SendMessageToServer(dTOMessage);
                _webSocketStreamingClient.PairID = 0;

            }
            else if (Type == WebSocketEnums.ConnectionType.Streamer)
            {
                //Todo: server side unpair for multiple viewers
                //_webSocketStreamingClient.Pairs.Clear();
            }
            else
            {
                DisplayDebugMessage?.Invoke($"Not a Streamer or Viewer, connection type: {Type}");
                return;
            }

        }

        else if ((_webSocketClientService.State != WebSocketState.Open))
        {
            DisplayDebugMessage?.Invoke($"Not Connected to WebSocketServer");
            return;
        }
        else if (Status != WebSocketEnums.ConnectionStatus.Connected)
        {
            DisplayDebugMessage?.Invoke($"Not connected to WebSocketServer. Current status: {Status}");
            return;
        }

    }
    private void InstanceDisconnectedFrom(uint pairID)
    {
        if (Type == WebSocketEnums.ConnectionType.Streamer)
        {
            if (_webSocketStreamingClient.Pairs.Contains(pairID))
            {
                DTOMessageWrapper dTOMessage = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToServerMessageType.ToUnpair,
                    Message = $"{pairID}",
                    Payload = null
                };
                _webSocketClientService.SendMessageToServer(dTOMessage);
                _webSocketStreamingClient.Pairs.Remove(pairID);
            }
        }
        else if (Type == WebSocketEnums.ConnectionType.Viewer)
        {
            if (_webSocketStreamingClient.PairID == pairID)
            {
                DTOMessageWrapper dTOMessage = new DTOMessageWrapper
                {
                    Type = (int)WebSocketEnums.ToServerMessageType.ToUnpair,
                    Message = $"Request unpair from {pairID}",
                    Payload = null
                };
                _webSocketClientService.SendMessageToServer(dTOMessage);
                _webSocketStreamingClient.PairID = 0;
            }
        }

    }
}

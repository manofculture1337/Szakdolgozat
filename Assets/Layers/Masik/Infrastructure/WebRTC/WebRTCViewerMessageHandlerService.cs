using System;
using Unity.WebRTC;
using UnityEngine;


public class WebRTCViewerMessageHandlerService
{
    private WebSocketClientService _webSocketClientService;
    private WebSocketStreamingClientService _webSocketStreamingClientService;
    public event DisplayMessageHandler DisplayDebugMessage;
    private static int sdpType = 420;
    private static int iceType = 210;

    private static WebRTCViewerMessageHandlerService _instance;
    public static WebRTCViewerMessageHandlerService Instance { get { return _instance; } }
    public void SetServices(WebSocketClientService webSocketClientService, WebSocketStreamingClientService webSocketStreamingClientService)
    {
        if (_instance == null)
        {
            _instance = new WebRTCViewerMessageHandlerService();
            _instance._webSocketClientService = webSocketClientService;
            _instance._webSocketStreamingClientService = webSocketStreamingClientService;
            _instance.SubscribeToWebScoketMessages();
            _instance.SubscribeToWebRTCViewerMessages();
        }
    }

    private void SubscribeToWebScoketMessages()
    {
        DisplayDebugMessage?.Invoke("Subscribing to WebSocket messages in WebRTCViewerMessageHandlerService");
        _webSocketClientService.Instance.MessageReceived += WebSocketMessageRecived;
    }
    private void SubscribeToWebRTCViewerMessages()
    {
        DisplayDebugMessage?.Invoke("Subscribing to WebRTCViewer messages in WebRTCViewerMessageHandlerService");
        WebRTCViewerService.Instance.OnLocalIceCandidate += (iceCandidate) =>
        {
            SendICEMessage(iceCandidate);
        };
        WebRTCViewerService.Instance.OnLocalAnswerCreated += (answer) =>
        {
            SendSDPMessage(answer);
        };
        WebRTCViewerService.Instance.OnDisconnected += () =>
        {
            SendUnpairMessage();
        };
    }

    private void SendUnpairMessage()
    {
        DisplayDebugMessage?.Invoke("Sending unpair message to paired client");
        _webSocketStreamingClientService.Unpair();
    }

    private void SendSDPMessage(RTCSessionDescription offer)
    {
        DisplayDebugMessage?.Invoke("Sending SDP message to paired client");
        var message = JsonUtility.ToJson(offer);
        var type = _webSocketStreamingClientService.GetIDOnServer() * 1000 + sdpType;
        DisplayDebugMessage?.Invoke("SDP Message Type: " + type);
        _webSocketStreamingClientService.SendWebSocketMessageToPairClient(message, type);
    }

    private void SendICEMessage(RTCIceCandidate iceCandidate)
    {
        DisplayDebugMessage?.Invoke("Sending ICE message to paired client");
        RTCIceCandidateInit rTCIceCandidateInit = new RTCIceCandidateInit
        {
            candidate = iceCandidate.Candidate,
            sdpMid = iceCandidate.SdpMid,
            sdpMLineIndex = iceCandidate.SdpMLineIndex,
        };
        var message = JsonUtility.ToJson(rTCIceCandidateInit);
        var type = _webSocketStreamingClientService.GetIDOnServer() * 1000 + iceType;
        DisplayDebugMessage?.Invoke("ICE Message Type: " + type);
        _webSocketStreamingClientService.SendWebSocketMessageToPairClient(message, type);
    }


    private void WebSocketMessageRecived(string message)
    {
        var wrappedMessage = DTOMessageWrapper.ConvertFromMessage(message);
        if (wrappedMessage == null)
        {
            DisplayDebugMessage?.Invoke("Failed to parse message: " + message);
            return;
        }
        if ((WebSocketEnums.AnswerType)wrappedMessage.Type == WebSocketEnums.AnswerType.Relayed)
        {
            var relayedMessage = wrappedMessage.Payload;
            switch (relayedMessage.Type)
            {
                case var type when type == sdpType:
                    DisplayDebugMessage?.Invoke("Received SDP message from paired client");
                    var sdp = JsonUtility.FromJson<RTCSessionDescription>(relayedMessage.Message);
                    WebRTCViewerService.Instance.ReceiveOffer(sdp);
                    break;
                case var type when type == iceType:
                    DisplayDebugMessage?.Invoke("Received ICE message from paired client");
                    var iceCandidateInit = JsonUtility.FromJson<RTCIceCandidateInit>(relayedMessage.Message);
                    RTCIceCandidate iceCandidate = new RTCIceCandidate(iceCandidateInit);
                    WebRTCViewerService.Instance.AddRemoteIceCandidate(iceCandidate);
                    break;
                default:
                    DisplayDebugMessage?.Invoke("Message type not for rtc: " + relayedMessage.Type);
                    break;
            }
        }
    }


}

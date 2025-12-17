using Unity.WebRTC;
using UnityEngine;


public class WebRTCMessageHandlerService
{
    private WebSocketClientService _webSocketClientService;
    private WebSocketStreamingClientService _webSocketStreamingClientService;
    private WebRTCService _webRTCService;
    public event DisplayMessageHandler DisplayDebugMessage;
    private static int sdpType = 420;
    private static int iceType = 210;

    private static WebRTCMessageHandlerService _instance;
    public void SetServices(WebSocketClientService webSocketClientService, WebSocketStreamingClientService webSocketStreamingClientService, WebRTCService webRTCService)
    {
        if (_instance == null)
        {
            _instance = new WebRTCMessageHandlerService();
            _instance._webSocketClientService = webSocketClientService;
            _instance._webSocketStreamingClientService = webSocketStreamingClientService;
            _instance._webRTCService = webRTCService;
            _instance.SubscribeToWebScoketMessages();
        }
    }
    public void SendSDPMessage(RTCSessionDescription sdp) => _instance.InstanceSendSDPMessage(sdp);
    public void SendICEMessage(RTCIceCandidate iceCandidate) => _instance.InstanceSendICEMessage(iceCandidate);
    private void SubscribeToWebScoketMessages()
    {
        _webSocketClientService.Instance.MessageReceived += WebSocketMessageRecived;
    }
    private void InstanceSendSDPMessage(RTCSessionDescription sdp)
    {
        var message = JsonUtility.ToJson(sdp);
        _webSocketStreamingClientService.SendWebSocketMessageToPairClient(message, sdpType);
    }
    private void InstanceSendICEMessage(RTCIceCandidate iceCandidate)
    {
        RTCIceCandidateInit rTCIceCandidateInit = new RTCIceCandidateInit
        {
            candidate = iceCandidate.Candidate,
            sdpMid = iceCandidate.SdpMid,
            sdpMLineIndex = iceCandidate.SdpMLineIndex,
        };
        var message = JsonUtility.ToJson(rTCIceCandidateInit);
        _webSocketStreamingClientService.SendWebSocketMessageToPairClient(message, iceType);
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
                    var sdp = JsonUtility.FromJson<RTCSessionDescription>(relayedMessage.Message);
                    _webRTCService.RTCSDpRecived(sdp);
                    break;
                case var type when type == iceType:
                    var iceCandidateInit = JsonUtility.FromJson<RTCIceCandidateInit>(relayedMessage.Message);
                    RTCIceCandidate iceCandidate = new RTCIceCandidate(iceCandidateInit);
                    _webRTCService.IceCanditetRecived(iceCandidate);
                    break;
                default:
                    DisplayDebugMessage?.Invoke("Message type not for rtc: " + relayedMessage.Type);
                    break;
            }
        }
    }
}

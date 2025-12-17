using UnityEngine;

public class WebSocketEnums
{
    [System.Serializable]
    public enum MessageType
    {
        ToServer,
        ToOtherClient
    }
    [System.Serializable]
    public enum ToServerMessageType
    {
        ToLog,
        ToJoinAsStreamer,
        ToJoinAsViewer,
        ToGetViewrs,
        ToGetConnectedViewrs,
        ToUnpair
    }
    [System.Serializable]
    public enum ToOtherClientMessageType
    {
        ToStreamer,
        ToViewer
    }
    [System.Serializable]
    public enum ToViewrMessageType
    {
        ToBroadcast,
        ToTraget,
        ToConnect,
    }
    [System.Serializable]
    public enum AnswerType
    {
        Ok,
        Err,
        Logged,
        Relayed,
        ConnectionRequest,
        Paired
    }

    [System.Serializable]
    public enum RelayedMessageType
    {
        RTC,
        Other
    }


    [System.Serializable]
    public enum ConnectionStatus
    {
        NotConnected,
        ConnectionRequested,
        Connected,
        ConnectionFailed,
    }
    [System.Serializable]
    public enum ConnectionType
    {
        None=-1,
        Streamer=0,
        Viewer=1,
    }

}

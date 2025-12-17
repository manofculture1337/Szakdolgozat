
using System.Collections.Concurrent;
using WebSocketSharp;

public class WebSocketClient
{
    public static string WebSocketServicePath { get; set; } = "streaming";

    public string ServerIp { get; set; }
    public int ServerPort { get; set; }

    public WebSocket WebSocket { get; set; }
    public WebSocketState State => WebSocket != null ? WebSocket.ReadyState : WebSocketState.Closed;
    public ConcurrentQueue<string> ReceivedMessages { get; set; }
    public ConcurrentQueue<string> ReceivedErrors { get; set; }

}

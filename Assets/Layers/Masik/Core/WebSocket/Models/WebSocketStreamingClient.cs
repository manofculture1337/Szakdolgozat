using System.Collections.Generic;
using NUnit.Framework;

public class WebSocketStreamingClient
{

    public int IDOnServer { get; set; } = -1;
    public uint PairID { get; set; } = 0;
    public bool IsPaired => PairID != 0;
    public WebSocketEnums.ConnectionType ConnectionType { get; set; } = WebSocketEnums.ConnectionType.None;
    public WebSocketEnums.ConnectionStatus ConnectionToStreamingStatus { get; set; } = WebSocketEnums.ConnectionStatus.NotConnected;
    public List<uint> Pairs { get; set; } = new List<uint>();
}


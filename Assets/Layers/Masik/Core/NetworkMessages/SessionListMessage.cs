using Mirror;
using System.Collections.Generic;

public struct SessionListMessage : NetworkMessage
{
    public List<SessionInfo> sessions;
}

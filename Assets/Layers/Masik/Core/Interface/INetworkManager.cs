using System.Collections.Generic;

public interface INetworkManager
{
    public void ChangeSceneToStream();
    public void ChangeSceneToView();
    public void Connect(string ipInput, string portInput);
    public void Disconnect();
    public void StartServerFromInput(string ipInput, string portInput);

    public void HostSession(string sessionName);
    public void JoinSession(int id);
    public List<SessionInfo> GetSessions();

    public void SendTargetedMessage(string message);
}
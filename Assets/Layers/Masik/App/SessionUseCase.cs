using System.Collections.Generic;

public class SessionUseCase
{
    private IDocumentationLogger logger;
    private INetworkManager networkManager;

    public SessionUseCase(IDocumentationLogger logger,INetworkManager networkManager)
    {
        this.logger = logger;
        this.networkManager = networkManager;
    }

    public void ChangeToStream()
    {
        //logger.LogToJSON("User choose streaming", LogLevel.User);
        networkManager.ChangeSceneToStream();
        networkManager.HostSession("random");
    }

    public void ChangeToView(int id)
    {
        //logger.LogToJSON("User choose viewing", LogLevel.User);
        networkManager.ChangeSceneToView();
        networkManager.JoinSession(id);
    }

    public List<SessionInfo> GetSessionList()
    {
        return  networkManager.GetSessions();
    }
}
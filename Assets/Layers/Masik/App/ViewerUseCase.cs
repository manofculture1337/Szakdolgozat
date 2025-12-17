public class ViewerUseCase
{
    private IDocumentationLogger logger;
    private INetworkManager networkManager;

    public ViewerUseCase(IDocumentationLogger logger, INetworkManager networkManager)
    {
        this.logger = logger;
        this.networkManager = networkManager;
    }

    public void SendMessage()
    {
        //logger.LogToJSON("Viewer is sending a message", LogLevel.User);
        networkManager.SendTargetedMessage("Hello from Viewer!");
    }
}
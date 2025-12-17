using System.Collections.Generic;
using VContainer;

public class SessionPresenter
{
    private SessionUseCase useCase;

    private WebSocketStreamingClientUsecase _streamingusecase;

    [Inject]
    public SessionPresenter(IDocumentationLogger logger, /*INetworkManager networkManager,*/ WebSocketStreamingClientService streamingService, WebSocketClientService service)
    {
        //useCase = new SessionUseCase(logger, networkManager);
        _streamingusecase = new WebSocketStreamingClientUsecase(streamingService, service);
    }

    public void ChangeToStream()
    {
        _streamingusecase.ConnectToStreamingAsStreamer();
        useCase.ChangeToStream();
    }

    public void ChangeToView(int id)
    {
        _streamingusecase.ConnectToStreamingAsViewer();
        useCase.ChangeToView(id);
    }

    public List<SessionInfo> UpdateSessionList()
    {
        return useCase.GetSessionList();
    }
}
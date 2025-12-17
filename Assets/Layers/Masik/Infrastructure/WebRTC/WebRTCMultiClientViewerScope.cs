using VContainer;
using VContainer.Unity;

public class WebRTCMultiClientViewerScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<WebRTCMultiClientViewerView>();

        builder.Register<WebSocketClientService>(Lifetime.Singleton);
        builder.Register<WebSocketStreamingClientService>(Lifetime.Singleton);
        builder.Register<WebRTCViewerMessageHandlerService>(Lifetime.Singleton);

        builder.RegisterComponentOnNewGameObject<WebRTCMultiClientViewerPresenter>(Lifetime.Singleton, "WebRTCMultiClientViewerPresenter");
    }
}
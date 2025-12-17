using VContainer;
using VContainer.Unity;

public class WebRTCViewerScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<WebRTCViewerView>();

        builder.Register<WebSocketClientService>(Lifetime.Singleton);
        builder.Register<WebSocketStreamingClientService>(Lifetime.Singleton);
        builder.Register<WebRTCService>(Lifetime.Singleton);
        builder.Register<WebRTCMessageHandlerService>(Lifetime.Singleton);

        builder.RegisterComponentOnNewGameObject<WebRTCViewerPresenter>(Lifetime.Singleton, "WebRTCViewerPresenter");
    }
}
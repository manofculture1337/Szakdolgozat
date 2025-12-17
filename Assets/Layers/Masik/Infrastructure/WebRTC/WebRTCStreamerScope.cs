using VContainer.Unity;
using VContainer;

public class WebRTCStreamerScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<WebRTCStreamerView>();

        builder.Register<WebSocketClientService>(Lifetime.Singleton);
        builder.Register<WebSocketStreamingClientService>(Lifetime.Singleton);
        builder.Register<WebRTCService>(Lifetime.Singleton);
        builder.Register<WebRTCMessageHandlerService>(Lifetime.Singleton);

        builder.RegisterComponentOnNewGameObject<WebRTCStreamerPresenter>(Lifetime.Singleton, "WebRTCStreamerPresenter");
    }
}
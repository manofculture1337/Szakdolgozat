using VContainer.Unity;
using VContainer;

public class WebRTCMultiClientStreamerScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<WebRTCMultiClientStreamerView>();

        /*builder.Register<WebSocketClientService>(Lifetime.Singleton);
        builder.Register<WebSocketStreamingClientService>(Lifetime.Singleton);*/
        builder.Register<WebRTCStreamerMessageHandlerService>(Lifetime.Singleton);

        builder.RegisterComponentOnNewGameObject<WebRTCMultiClientStreamerPresenter>(Lifetime.Singleton, "WebRTCMultiClientStreamerPresenter");
    }
}
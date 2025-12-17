using VContainer;
using VContainer.Unity;

public class RootScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<DocumentationLogger>(Lifetime.Singleton).AsImplementedInterfaces();
        builder.Register<WebSocketClientService>(Lifetime.Singleton);
        builder.Register<WebSocketStreamingClientService>(Lifetime.Singleton);
    }
}

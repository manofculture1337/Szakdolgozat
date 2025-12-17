
using VContainer.Unity;
using VContainer;

public class WebSocketScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<WebSocketConnectView>();
        builder.RegisterComponentInHierarchy<ChooseRoleView>();

        builder.Register<WebSocketClientService>(Lifetime.Scoped);
        builder.Register<WebSocketStreamingClientService>(Lifetime.Scoped);


        builder.RegisterComponentOnNewGameObject<WebSocketConnectPresenter>(Lifetime.Scoped, "WebSocketConnectionPresenter");
        builder.RegisterComponentOnNewGameObject<ChooseRolePresenter>(Lifetime.Scoped, "ChooseRolePresenter");
    }
}

using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ConnectScope: LifetimeScope
{
    [SerializeField]
    private CustomNetworkManager networkManagerPrefab;
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<ConnectView>();
        builder.RegisterComponentInHierarchy<MyNetworkManager>();

        builder.Register<WebSocketClientService>(Lifetime.Scoped);
        builder.Register<WebSocketStreamingClientService>(Lifetime.Scoped);
        

        builder.RegisterComponentOnNewGameObject<ConnectPresenter>(Lifetime.Scoped, "ConnectPresenter");

        builder.RegisterComponentInNewPrefab(networkManagerPrefab, Lifetime.Singleton).AsImplementedInterfaces();

        
    }
}
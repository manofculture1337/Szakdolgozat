using UnityEngine;
using VContainer;
using VContainer.Unity;

public class ViewerScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<ViewerView>();
        var networkManager = FindFirstObjectByType<CustomNetworkManager>();
        if (networkManager != null)
        {
            builder.RegisterComponent(networkManager);
        }

        builder.Register<ViewerPresenter>(Lifetime.Scoped).AsSelf();
    }
}

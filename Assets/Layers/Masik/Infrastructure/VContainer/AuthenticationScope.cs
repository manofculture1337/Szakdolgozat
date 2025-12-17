using VContainer;
using VContainer.Unity;

public class AuthenticationScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<LoginView>();

        builder.Register<AuthenticationService>(Lifetime.Scoped).AsImplementedInterfaces();

        builder.RegisterComponentOnNewGameObject<LoginPresenter>(Lifetime.Scoped, "LoginPresenter");
    }
}

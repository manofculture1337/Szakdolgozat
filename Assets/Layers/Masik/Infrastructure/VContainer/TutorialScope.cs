using VContainer;
using VContainer.Unity;

public class TutorialScope: LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<TutorialView>();

        builder.Register<StepHandler>(Lifetime.Scoped).AsImplementedInterfaces();

        builder.RegisterComponentOnNewGameObject<TutorialPresenter>(Lifetime.Scoped, "TutorialPresenter");
    }
}

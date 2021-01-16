using RollingBall.Memento;
using RollingBall.StageObject;
using Zenject;

namespace RollingBall.StageData
{
    public sealed class StageInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<Caretaker>()
                .AsCached();

            Container
                .Bind<ClearAction>()
                .AsCached();

            Container
                .Bind<StageLoader>()
                .AsCached()
                .NonLazy();
        }
    }
}
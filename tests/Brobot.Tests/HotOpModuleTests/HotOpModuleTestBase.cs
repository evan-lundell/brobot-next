using Brobot.Modules;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests.HotOpModuleTests;

[TestFixture]
public abstract class HotOpModuleTestBase : InteractionModuleTestBase<HotOpModule>
{
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }
    protected Mock<IHotOpService> HotOpServiceMock { get; private set; }

    protected HotOpModule HotOpModule => Module;

    [SetUp]
    public void Setup()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        HotOpServiceMock = new Mock<IHotOpService>();

        InitializeModule(new HotOpModule(
            UnitOfWorkMock.Object,
            HotOpServiceMock.Object,
            Mock.Of<ILogger<HotOpModule>>()
        ));
    }
}

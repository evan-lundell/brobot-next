using Brobot.Modules;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Brobot.Tests.BrobotModuleTests;

[TestFixture]
public abstract class BrobotModuleTestBase : InteractionModuleTestBase<BrobotModule>
{
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; private set; }
    protected Mock<IGiphyService> GiphyServiceMock { get; private set; }
    protected Mock<IRandomFactService> RandomFactServiceMock { get; private set; }
    protected Mock<IDictionaryService> DictionaryServiceMock { get; private set; }
    protected Mock<IScheduledMessageService> ScheduledMessageServiceMock { get; private set; }
    protected Mock<IAssemblyService> AssemblyServiceMock { get; private set; }

    protected BrobotModule BrobotModule => Module;

    [SetUp]
    public void Setup()
    {
        UnitOfWorkMock = new Mock<IUnitOfWork>();
        GiphyServiceMock = new Mock<IGiphyService>();
        RandomFactServiceMock = new Mock<IRandomFactService>();
        DictionaryServiceMock = new Mock<IDictionaryService>();
        ScheduledMessageServiceMock = new Mock<IScheduledMessageService>();
        AssemblyServiceMock = new Mock<IAssemblyService>();

        InitializeModule(new BrobotModule(
            UnitOfWorkMock.Object,
            new Random(100),
            GiphyServiceMock.Object,
            RandomFactServiceMock.Object,
            DictionaryServiceMock.Object,
            ScheduledMessageServiceMock.Object,
            AssemblyServiceMock.Object,
            Mock.Of<ILogger<BrobotModule>>()
        ));
    }
}

using Brobot.Contexts;
using Brobot.Models;
using Brobot.Repositories;
using Brobot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Brobot.Tests;

[TestFixture]
public class StopWordServiceTests
{
    private StopWordService _stopWordService;
    private BrobotDbContext _context;
    private ServiceProvider _serviceProvider;
    
    [SetUp]
    public void Setup()
    {
        ServiceCollection serviceCollection = new();
        serviceCollection.AddDbContext<BrobotDbContext>(options =>
            options.UseInMemoryDatabase("Brobot"));
        serviceCollection.AddTransient<IUnitOfWork, UnitOfWork>();
        _serviceProvider = serviceCollection.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<BrobotDbContext>();
        Mock<IServiceScope> serviceScopeMock = new();
        serviceScopeMock.Setup(x => x.ServiceProvider).Returns(_serviceProvider);
        Mock<IServiceScopeFactory> serviceScopeFactoryMock = new();
        serviceScopeFactoryMock.Setup(x => x.CreateScope())
            .Returns(serviceScopeMock.Object);

        _context.StopWords.AddRange(new StopWordModel { Word = "test" }, new StopWordModel { Word = "stop" }, new StopWordModel { Word = "word" });
        _context.SaveChanges();
        
        _stopWordService = new StopWordService(serviceScopeFactoryMock.Object);
    }
    
    [Test]
    public async Task IsStopWord_ReturnsTrueForStopWord()
    {
        var result = await _stopWordService.IsStopWord("Stop");
        
        Assert.That(result, Is.True);
    }
    
    [Test]
    public async Task IsStopWord_ReturnsFalseForNonStopWord()
    {
        var result = await _stopWordService.IsStopWord("NonStopWord");
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public async Task StopWordAdded_WithoutUpdate_ReturnsFalse()
    {
        var result = await _stopWordService.IsStopWord("NewStopWord");
        
        Assert.That(result, Is.False);
    }
    
    [Test]
    public async Task StopWordAdded_WithUpdate_ReturnsTrue()
    {
        var result = await _stopWordService.IsStopWord("NewStopWord");
        Assert.That(result, Is.False);
        
        _context.StopWords.Add(new StopWordModel { Word = "newstopword" });
        await _context.SaveChangesAsync();
        _stopWordService.StopWordsUpdated();
        result = await _stopWordService.IsStopWord("NewStopWord");
        
        Assert.That(result, Is.True);
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        _serviceProvider.Dispose();
    }
}
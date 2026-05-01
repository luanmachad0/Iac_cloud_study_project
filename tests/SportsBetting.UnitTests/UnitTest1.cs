using Moq;
using SportsBetting.Application.Bets;
using SportsBetting.Application.Bets.Contracts;
using SportsBetting.Domain.Bets;

namespace SportsBetting.UnitTests;

public class CreateBetUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldCreateBet_WhenRequestIsValid()
    {
        var repository = new Mock<IBetRepository>();
        repository.Setup(r => r.AddAsync(It.IsAny<Bet>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new CreateBetUseCase(repository.Object);

        var response = await useCase.ExecuteAsync(
            new CreateBetRequest("user-1", "match-1", 50m, "TeamA"),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, response.BetId);
        Assert.Equal("Pending", response.Status);
        repository.Verify(r => r.AddAsync(It.IsAny<Bet>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrow_WhenAmountIsInvalid()
    {
        var repository = new Mock<IBetRepository>();
        var useCase = new CreateBetUseCase(repository.Object);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
                new CreateBetRequest("user-1", "match-1", 0m, "TeamA"),
                CancellationToken.None));
    }

    [Fact]
    public async Task Settle_ShouldUpdateCacheAndPublishEvent_WhenBetExists()
    {
        var bet = new Bet("user-1", "match-1", 10m, "TeamA");
        var repository = new Mock<IBetRepository>();
        repository.Setup(r => r.GetByIdAsync(bet.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(bet);

        var cache = new Mock<IBetResultCache>();
        var publisher = new Mock<IBetEventPublisher>();
        var useCase = new SettleBetUseCase(repository.Object, cache.Object, publisher.Object);

        var response = await useCase.ExecuteAsync(
            new SettleBetRequest(bet.Id, true),
            CancellationToken.None);

        Assert.Equal("Won", response.Status);
        repository.Verify(r => r.UpdateAsync(bet, It.IsAny<CancellationToken>()), Times.Once);
        cache.Verify(c => c.SetAsync(bet.Id, "Won", It.IsAny<CancellationToken>()), Times.Once);
        publisher.Verify(p => p.PublishBetSettledAsync(It.IsAny<BetSettledEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Settle_ShouldThrow_WhenBetDoesNotExist()
    {
        var repository = new Mock<IBetRepository>();
        repository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Bet?)null);

        var cache = new Mock<IBetResultCache>();
        var publisher = new Mock<IBetEventPublisher>();
        var useCase = new SettleBetUseCase(repository.Object, cache.Object, publisher.Object);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            useCase.ExecuteAsync(new SettleBetRequest(Guid.NewGuid(), false), CancellationToken.None));
    }
}

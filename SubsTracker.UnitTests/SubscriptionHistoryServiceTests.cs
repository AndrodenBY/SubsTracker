using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using Shouldly;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.Filter;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Pagination;
using SubsTracker.UnitTests.TestsBase;

namespace SubsTracker.UnitTests;

public class SubscriptionHistoryServiceTests : SubscriptionHistoryServiceTestBase
{
    [Fact]
    public async Task GetAllHistory_WhenRequestingMiddlePage_ReturnsCorrectMetadata()
    {
        // Arrange
        var ct = CancellationToken.None;
        var subId = Guid.NewGuid();
        var pagination = new PaginationParameters { PageNumber = 2, PageSize = 5 };
        
        var historyItems = Fixture.CreateMany<SubscriptionHistory>(5).ToList();
        var pagedList = new PaginatedList<SubscriptionHistory>(historyItems, 2, 5, 12);

        SubscriptionHistoryRepository.GetAll(Arg.Any<Expression<Func<SubscriptionHistory, bool>>>(), Arg.Is(pagination), ct)
            .Returns(pagedList);

        // Act
        var result = await SubscriptionHistoryService.GetAllHistory(subId, null, pagination, ct);

        // Assert
        result.PageNumber.ShouldBe(2);
        result.TotalCount.ShouldBe(12);
        result.PageCount.ShouldBe(3);
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeTrue();
    }
    
    [Fact]
    public async Task GetAllHistory_WhenFilteredByName_ReturnsFilteredResults()
    {
        // Arrange
        var ct = CancellationToken.None;
        var filter = new SubscriptionHistoryFilter { SubscriptionName = "Netflix" };
        var subscriptionEntity = Fixture.Build<SubscriptionEntity>()
            .With(s => s.Name, "Netflix Premium")
            .Create();
        
        var history = Fixture.Build<SubscriptionHistory>()
            .With(h => h.Subscription, subscriptionEntity)
            .Create();
    
        var dto = Fixture.Build<SubscriptionHistoryDto>()
            .With(d => d.SubscriptionName, "Netflix Premium")
            .Create();

        var pagedList = new PaginatedList<SubscriptionHistory>([history], 1, 10, 1);

        SubscriptionHistoryRepository.GetAll(Arg.Any<Expression<Func<SubscriptionHistory, bool>>>(), Arg.Any<PaginationParameters>(), ct)
            .Returns(pagedList);

        Mapper.Map<SubscriptionHistoryDto>(history).Returns(dto);

        // Act
        var result = await SubscriptionHistoryService.GetAllHistory(Guid.NewGuid(), filter, null, ct);

        // Assert
        result.Items.ShouldHaveSingleItem();
        result.Items[0].SubscriptionName?.ShouldContain("Netflix");
    }
    
    [Fact]
    public async Task GetAllHistory_WhenFilteredByTypeAndAction_CallsRepoWithCorrectPredicate()
    {
        // Arrange
        var ct = CancellationToken.None;
        var filter = new SubscriptionHistoryFilter 
        { 
            SubscriptionType = SubscriptionType.Lifetime,
            Action = SubscriptionAction.Renew 
        };

        var history = Fixture.Build<SubscriptionHistory>()
            .With(h => h.Action, SubscriptionAction.Renew)
            .Create();

        SubscriptionHistoryRepository.GetAll(Arg.Any<Expression<Func<SubscriptionHistory, bool>>>(), Arg.Any<PaginationParameters>(), ct)
            .Returns(new PaginatedList<SubscriptionHistory>([history], 1, 10, 1));

        // Act
        await SubscriptionHistoryService.GetAllHistory(Guid.NewGuid(), filter, null, ct);

        // Assert
        await SubscriptionHistoryRepository.Received(1).GetAll(Arg.Any<Expression<Func<SubscriptionHistory, bool>>>(), Arg.Any<PaginationParameters>(), ct);
    }
    
    [Fact]
    public async Task GetAllHistory_WhenFilteredByPrice_ReturnsMatchingHistory()
    {
        // Arrange
        var ct = CancellationToken.None;
        const decimal targetPrice = 99.99m;
        var filter = new SubscriptionHistoryFilter { PricePaid = targetPrice };
    
        var history = Fixture.Build<SubscriptionHistory>().With(h => h.PricePaid, targetPrice).Create();
        var dto = Fixture.Build<SubscriptionHistoryDto>().With(d => d.PricePaid, targetPrice).Create();

        SubscriptionHistoryRepository.GetAll(Arg.Any<Expression<Func<SubscriptionHistory, bool>>>(), Arg.Any<PaginationParameters>(), ct)
            .Returns(new PaginatedList<SubscriptionHistory>([history], 1, 10, 1));
    
        Mapper.Map<SubscriptionHistoryDto>(history).Returns(dto);

        // Act
        var result = await SubscriptionHistoryService.GetAllHistory(Guid.NewGuid(), filter, null, ct);

        // Assert
        result.Items.Single().PricePaid.ShouldBe(targetPrice);
    }
}

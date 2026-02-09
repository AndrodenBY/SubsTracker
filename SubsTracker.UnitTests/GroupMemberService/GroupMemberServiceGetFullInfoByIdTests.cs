namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceGetFullInfoByIdTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task GetFullInfoById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var memberId = Fixture.Create<Guid>();
        var cacheKey = $"{memberId}:{nameof(GroupMemberDto)}";

        var cachedDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .With(dto => dto.Role, MemberRole.Admin)
            .Create();

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupMemberDto?>>>(),
            default
        )!.Returns(cachedDto);

        //Act
        var result = await Service.GetFullInfoById(memberId, default);

        //Assert
        result.ShouldBe(cachedDto);
        result?.Role.ShouldBe(MemberRole.Admin);

        await MemberRepository.DidNotReceive().GetFullInfoById(Arg.Any<Guid>(), default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupMemberDto?>>>(),
            default
        );
    }

    [Fact]
    public async Task GetFullInfoById_WhenCacheMiss_FetchesFromRepoAndCaches()
    {
        //Arrange
        var memberId = Fixture.Create<Guid>();
        var cacheKey = $"{memberId}:{nameof(GroupMemberDto)}";

        var memberEntity = Fixture.Create<GroupMember>();
        memberEntity.Id = memberId;

        var memberDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .Create();

        MemberRepository.GetFullInfoById(memberId, default).Returns(memberEntity);
        Mapper.Map<GroupMemberDto>(memberEntity).Returns(memberDto);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupMemberDto?>>>(),
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<GroupMemberDto>>>();
            return factory();
        });

        //Act
        var result = await Service.GetFullInfoById(memberId, default);

        //Assert
        result.ShouldBe(memberDto);
        result?.Id.ShouldBe(memberId);

        await MemberRepository.Received(1).GetFullInfoById(memberId, default);
        Mapper.Received(1).Map<GroupMemberDto>(memberEntity);

        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<GroupMemberDto?>>>(),
            default
        );
    }
}

namespace SubsTracker.UnitTests.GroupMemberService;

public class GroupMemberServiceGetFullInfoByIdTests : GroupMemberServiceTestBase
{
    [Fact]
    public async Task GetFullInfoById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var memberId = Fixture.Create<Guid>();
        var cacheKey = $"{memberId}_{nameof(GroupMemberDto)}";
        
        var cachedDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .With(dto => dto.Role, MemberRole.Admin)
            .Create();
        
        CacheService.GetData<GroupMemberDto>(cacheKey, default).Returns(cachedDto); 
    
        //Act
        var result = await Service.GetFullInfoById(memberId, default);

        //Assert
        result.ShouldBe(cachedDto);
        result.Role.ShouldBe(MemberRole.Admin);
        
        await MemberRepository.DidNotReceive().GetFullInfoById(Arg.Any<Guid>(), default);
        await CacheService.Received(1).GetData<GroupMemberDto>(cacheKey, default);
        await CacheService.DidNotReceive().SetData(
            Arg.Any<string>(), 
            Arg.Any<GroupMemberDto>(), 
            Arg.Any<TimeSpan>(),
            default
        );
    }
    
    [Fact]
    public async Task GetFullInfoById_WhenCacheMiss_FetchesFromRepoAndCaches()
    {
        //Arrange
        var memberId = Fixture.Create<Guid>();
        var cacheKey = $"{memberId}_{nameof(GroupMemberDto)}";
        
        var memberEntity = Fixture.Create<GroupMember>();
        memberEntity.Id = memberId;
        
        var memberDto = Fixture.Build<GroupMemberDto>()
            .With(dto => dto.Id, memberId)
            .Create();
        
        CacheService.GetData<GroupMemberDto>(cacheKey, default).Returns((GroupMemberDto)null!);
        MemberRepository.GetFullInfoById(memberId, default).Returns(memberEntity);
        Mapper.Map<GroupMemberDto>(memberEntity).Returns(memberDto);

        //Act
        var result = await Service.GetFullInfoById(memberId, default);

        //Assert
        result.ShouldBe(memberDto);
        result.Id.ShouldBe(memberId);
        
        await MemberRepository.Received(1).GetFullInfoById(memberId, default);
        await CacheService.Received(1).GetData<GroupMemberDto>(cacheKey, default);
        await CacheService.Received(1).SetData(
            Arg.Is<string>(key => key == cacheKey), 
            Arg.Is<GroupMemberDto>(dto => dto.Id == memberId), 
            Arg.Is<TimeSpan>(ts => ts == TimeSpan.FromMinutes(3)),
            default
        );
    }
}

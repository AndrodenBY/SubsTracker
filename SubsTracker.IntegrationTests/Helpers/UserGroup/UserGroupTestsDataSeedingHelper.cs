using SubsTracker.DAL.Entities;
using SubsTracker.IntegrationTests.Configuration.WebApplicationFactory;

namespace SubsTracker.IntegrationTests.Helpers.UserGroup;

public class UserGroupTestsDataSeedingHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    public async Task<UserGroupSeedEntity> AddOnlyUserGroup()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var group = Fixture.Build<Group>()
            .With(g => g.Name, "Test Group")
            .Without(g => g.Members)
            .Without(g => g.SharedSubscriptions)
            .Without(g => g.User)
            .Without(g => g.UserId)
            .Create();

        await dbContext.UserGroups.AddAsync(group);
        await dbContext.SaveChangesAsync();

        return new UserGroupSeedEntity
        {
            UserEntity = null!,
            GroupEntity = group,
            Subscriptions = new List<SubscriptionModel>(),
            Members = new List<MemberEntity>()
        };
    }

    public async Task<UserGroupSeedEntity> AddUserGroupWithMembers()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var owner = Fixture.Build<UserEntity>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        var memberUsers = Fixture.CreateMany<UserEntity>(3)
            .Select(_ => Fixture.Build<UserEntity>()
                .Without(x => x.Groups)
                .Without(x => x.Subscriptions)
                .Create())
            .ToList();


        var ownerMember = Fixture.Build<MemberEntity>()
            .With(m => m.UserId, owner.Id)
            .With(m => m.Role, MemberRole.Admin)
            .Without(m => m.GroupEntity)
            .Without(m => m.UserEntity)
            .Create();

        var participantMembers = Fixture.Build<MemberEntity>()
            .With(m => m.Role, MemberRole.Participant)
            .Without(m => m.GroupEntity)
            .Without(m => m.UserEntity)
            .CreateMany(3).ToList();

        for (var i = 0; i < participantMembers.Count; i++) participantMembers[i].UserId = memberUsers[i].Id;

        var allMembers = new List<MemberEntity>();
        allMembers.Add(ownerMember);
        allMembers.AddRange(participantMembers);

        var group = Fixture.Build<Group>()
            .With(g => g.UserId, owner.Id)
            .With(g => g.Name, "Group With Members")
            .Without(g => g.SharedSubscriptions)
            .With(g => g.Members, allMembers)
            .Create();

        var usersToSave = new List<UserEntity> { owner };
        usersToSave.AddRange(memberUsers);
        await db.Users.AddRangeAsync(usersToSave);


        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync();

        return new UserGroupSeedEntity
        {
            UserEntity = owner,
            GroupEntity = group,
            Members = allMembers,
            Subscriptions = new List<SubscriptionModel>()
        };
    }

    public async Task<SubscriptionModel> AddSubscription()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);

        var subscription = Fixture.Build<SubscriptionModel>()
            .With(s => s.UserId, user.Id)
            .Without(s => s.User)
            .Create();

        await db.Subscriptions.AddAsync(subscription);
        await db.SaveChangesAsync();

        return subscription;
    }

    public async Task<CreateMemberDto> AddCreateGroupMemberDto(Guid groupId, Guid userId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .With(u => u.Id, userId)
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        return new CreateMemberDto
        {
            UserId = user.Id,
            GroupId = groupId,
            Role = MemberRole.Participant
        };
    }

    public async Task<UserGroupSeedEntity> AddUserGroupAndUser()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);

        var group = Fixture.Build<Group>()
            .With(g => g.UserId, user.Id)
            .With(g => g.Name, "Group Without Members")
            .Create();

        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync();

        return new UserGroupSeedEntity
        {
            UserEntity = user,
            GroupEntity = group,
            Members = new List<MemberEntity>(),
            Subscriptions = new List<SubscriptionModel>()
        };
    }

    public async Task<MemberEntity> AddMemberOnly()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);

        var group = Fixture.Build<Group>()
            .With(g => g.UserId, user.Id)
            .Without(g => g.Members)
            .Without(g => g.SharedSubscriptions)
            .Create();

        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync();

        var member = Fixture.Build<MemberEntity>()
            .With(m => m.UserId, user.Id)
            .With(m => m.GroupId, group.Id)
            .With(m => m.Role, MemberRole.Participant)
            .Without(m => m.UserEntity)
            .Without(m => m.GroupEntity)
            .Create();

        await db.Members.AddAsync(member);
        await db.SaveChangesAsync();

        return member;
    }

    public async Task<UserGroupSeedEntity> AddGroupWithSharedSubscription()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);

        var subscription = Fixture.Build<SubscriptionModel>()
            .With(s => s.UserId, user.Id)
            .With(s => s.Name, "Shared Sub")
            .With(s => s.Active, true)
            .With(s => s.DueDate, DateOnly.FromDateTime(DateTime.Today.AddDays(10)))
            .Without(s => s.User)
            .Create();

        await db.Subscriptions.AddAsync(subscription);

        var group = Fixture.Build<Group>()
            .With(g => g.UserId, user.Id)
            .With(g => g.Name, "Group With Shared Sub")
            .With(g => g.SharedSubscriptions, [subscription])
            .Create();

        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync();

        return new UserGroupSeedEntity
        {
            UserEntity = user,
            GroupEntity = group,
            Subscriptions = new List<SubscriptionModel> { subscription },
            Members = new List<MemberEntity>()
        };
    }

    public async Task<UserGroupSeedEntity> AddSeedUserOnly()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .With(u => u.Auth0Id, TestsAuthHandler.DefaultAuth0Id)
            .Without(u => u.Groups)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new UserGroupSeedEntity
        {
            UserEntity = user,
            Subscriptions = null!,
            GroupEntity = null!,
            Members = null!
        };
    }

    public CreateGroupDto AddCreateUserGroupDto()
    {
        var createDto = Fixture.Build<CreateGroupDto>()
            .With(d => d.Name, "Created Group Name")
            .Create();

        return createDto;
    }

    public UpdateGroupDto AddUpdateUserGroupDto(Guid groupId)
    {
        var updateDto = Fixture.Build<UpdateGroupDto>()
            .With(d => d.Id, groupId)
            .With(d => d.Name, "Updated Group Name")
            .Create();

        return updateDto;
    }
}

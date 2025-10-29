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
            User = null!,
            Group = group,
            Subscriptions = new List<SubscriptionModel>(),
            Members = new List<GroupMember>()
        };
    }

    public async Task<UserGroupSeedEntity> AddUserGroupWithMembers()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var owner = Fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        var memberUsers = Fixture.CreateMany<UserModel>(3)
            .Select(_ => Fixture.Build<UserModel>()
                .Without(x => x.Groups)
                .Without(x => x.Subscriptions)
                .Create())
            .ToList();


        var ownerMember = Fixture.Build<GroupMember>()
            .With(m => m.UserId, owner.Id)
            .With(m => m.Role, MemberRole.Admin)
            .Without(m => m.Group)
            .Without(m => m.User)
            .Create();

        var participantMembers = Fixture.Build<GroupMember>()
            .With(m => m.Role, MemberRole.Participant)
            .Without(m => m.Group)
            .Without(m => m.User)
            .CreateMany(3).ToList();

        for (var i = 0; i < participantMembers.Count; i++) participantMembers[i].UserId = memberUsers[i].Id;

        var allMembers = new List<GroupMember>();
        allMembers.Add(ownerMember);
        allMembers.AddRange(participantMembers);

        var group = Fixture.Build<Group>()
            .With(g => g.UserId, owner.Id)
            .With(g => g.Name, "Group With Members")
            .Without(g => g.SharedSubscriptions)
            .With(g => g.Members, allMembers)
            .Create();

        var usersToSave = new List<UserModel> { owner };
        usersToSave.AddRange(memberUsers);
        await db.Users.AddRangeAsync(usersToSave);


        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync();

        return new UserGroupSeedEntity
        {
            User = owner,
            Group = group,
            Members = allMembers,
            Subscriptions = new List<SubscriptionModel>()
        };
    }

    public async Task<SubscriptionModel> AddSubscription()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserModel>()
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

    public async Task<CreateGroupMemberDto> AddCreateGroupMemberDto(Guid groupId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

        return new CreateGroupMemberDto
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

        var user = Fixture.Build<UserModel>()
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
            User = user,
            Group = group,
            Members = new List<GroupMember>(),
            Subscriptions = new List<SubscriptionModel>()
        };
    }

    public async Task<GroupMember> AddMemberOnly()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserModel>()
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

        var member = Fixture.Build<GroupMember>()
            .With(m => m.UserId, user.Id)
            .With(m => m.GroupId, group.Id)
            .With(m => m.Role, MemberRole.Participant)
            .Without(m => m.User)
            .Without(m => m.Group)
            .Create();

        await db.Members.AddAsync(member);
        await db.SaveChangesAsync();

        return member;
    }

    public async Task<UserGroupSeedEntity> AddGroupWithSharedSubscription()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserModel>()
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
            User = user,
            Group = group,
            Subscriptions = new List<SubscriptionModel> { subscription },
            Members = new List<GroupMember>()
        };
    }

    public async Task<UserGroupSeedEntity> AddSeedUserOnly()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new UserGroupSeedEntity
        {
            User = user,
            Subscriptions = null!,
            Group = null!,
            Members = null!
        };
    }

    public CreateUserGroupDto AddCreateUserGroupDto()
    {
        var createDto = Fixture.Build<CreateUserGroupDto>()
            .With(d => d.Name, "Created Group Name")
            .Create();

        return createDto;
    }

    public UpdateUserGroupDto AddUpdateUserGroupDto(Guid groupId)
    {
        var updateDto = Fixture.Build<UpdateUserGroupDto>()
            .With(d => d.Id, groupId)
            .With(d => d.Name, "Updated Group Name")
            .Create();

        return updateDto;
    }
}

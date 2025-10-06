namespace SubsTracker.IntegrationTests.Helpers.UserGroup;

public class UserGroupTestsDataSeedingHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    public async Task<UserGroupSeedEntity> AddOnlyUserGroup()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        var group = _fixture.Build<Group>()
            .With(g => g.Name, "Test Group")
            .Without(g => g.Members)
            .Without(g => g.SharedSubscriptions)
            .Without(g => g.User)
            .Without(g => g.UserId)
            .Create();
        
        await dbContext.UserGroups.AddAsync(group);
        await dbContext.SaveChangesAsync(default); 
        
        return new UserGroupSeedEntity
        {
            User = null,
            Group = group,
            Subscriptions = new List<SubscriptionModel>(),
            Members = new List<GroupMember>( )
        };
    }
    
    public async Task<UserGroupSeedEntity> AddUserGroupWithMembers()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        var owner = _fixture.Build<User>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();
        
        var memberUsers = _fixture.CreateMany<User>(3)
            .Select(u => _fixture.Build<User>().Without(u => u.Groups).Without(u => u.Subscriptions).Create())
            .ToList();
        
        var ownerMember = _fixture.Build<GroupMember>()
            .With(m => m.UserId, owner.Id)
            .With(m => m.Role, MemberRole.Admin)
            .Without(m => m.Group)
            .Without(m => m.User)
            .Create();
        
        var participantMembers = _fixture.Build<GroupMember>()
            .With(m => m.Role, MemberRole.Participant)
            .Without(m => m.Group)
            .Without(m => m.User)
            .CreateMany(3).ToList();
        
        for (int i = 0; i < participantMembers.Count; i++)
        {
            participantMembers[i].UserId = memberUsers[i].Id;
        }
        
        var allMembers = new List<GroupMember>();
        allMembers.Add(ownerMember);
        allMembers.AddRange(participantMembers);
        
        var group = _fixture.Build<Group>()
            .With(g => g.UserId, owner.Id)
            .With(g => g.Name, "Group With Members")
            .Without(g => g.SharedSubscriptions)
            .With(g => g.Members, allMembers) 
            .Create();
        
        var usersToSave = new List<User> { owner };
        usersToSave.AddRange(memberUsers);
        await db.Users.AddRangeAsync(usersToSave);

        
        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync(default);
        
        return new UserGroupSeedEntity
        {
            User = owner,
            Group = group,
            Members = allMembers,
            Subscriptions = new()
        };
    }
    
    public async Task<SubscriptionModel> AddSubscription()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);
        
        var subscription = _fixture.Build<SubscriptionModel>()
            .With(s => s.UserId, user.Id)
            .Without(s => s.User)
            .Create();

        await db.Subscriptions.AddAsync(subscription);
        await db.SaveChangesAsync(default);

        return subscription;
    }
    
    public async Task<CreateGroupMemberDto> AddCreateGroupMemberDto(Guid groupId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync(default);

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

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync(default);

        var group = _fixture.Build<Group>()
            .With(g => g.UserId, user.Id)
            .With(g => g.Name, "Group Without Members")
            .Create();

        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync(default);

        return new UserGroupSeedEntity
        {
            User = user,
            Group = group,
            Members = new(),
            Subscriptions = new()
        };
    }
    
    public async Task<GroupMember> AddMemberOnly()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();
        
        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);
        await db.SaveChangesAsync(default);
        
        var group = _fixture.Build<Group>()
            .With(g => g.UserId, user.Id)
            .Without(g => g.Members)
            .Without(g => g.SharedSubscriptions)
            .Create();

        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync(default);
        
        var member = _fixture.Build<GroupMember>()
            .With(m => m.UserId, user.Id)
            .With(m => m.GroupId, group.Id)
            .With(m => m.Role, MemberRole.Participant)
            .Without(m => m.User)
            .Without(m => m.Group)
            .Create();

        await db.Members.AddAsync(member);
        await db.SaveChangesAsync(default);

        return member;
    }
    
    public async Task<UserGroupSeedEntity> AddGroupWithSharedSubscription()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);
        
        var subscription = _fixture.Build<SubscriptionModel>()
            .With(s => s.UserId, user.Id)
            .With(s => s.Name, "Shared Sub")
            .With(s => s.Active, true)
            .With(s => s.DueDate, DateOnly.FromDateTime(DateTime.Today.AddDays(10)))
            .Without(s => s.User)
            .Create();

        await db.Subscriptions.AddAsync(subscription);
        

        var group = _fixture.Build<Group>()
            .With(g => g.UserId, user.Id)
            .With(g => g.Name, "Group With Shared Sub")
            .With(g => g.SharedSubscriptions, [ subscription ])
            .Create();

        await db.UserGroups.AddAsync(group);
        await db.SaveChangesAsync(default);

        return new UserGroupSeedEntity
        {
            User = user,
            Group = group,
            Subscriptions = new List<SubscriptionModel> { subscription },
            Members = new()
        };
    }
    
    public async Task<UserGroupSeedEntity> AddSeedUserOnly()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = _fixture.Build<UserModel>()
            .Without(u => u.Groups)
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync(default);

        return new UserGroupSeedEntity()
        {
            User = user,
            Subscriptions = null!,
            Group = null!,
            Members = null!
        };
    }

    public async Task<CreateUserGroupDto> AddCreateUserGroupDto()
    {
        var createDto = _fixture.Build<CreateUserGroupDto>()
            .With(d => d.Name, "Created Group Name")
            .Create();

        return createDto;
    }

    public async Task<UpdateUserGroupDto> AddUpdateUserGroupDto(Guid groupId)
    {
        var updateDto = _fixture.Build<UpdateUserGroupDto>()
            .With(d => d.Id, groupId)
            .With(d => d.Name, "Updated Group Name")
            .Create();

        return updateDto;
    }
    
    public async Task ClearTestDataWithDependencies()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        dbContext.UserGroups.RemoveRange(dbContext.UserGroups.ToList());
        dbContext.Subscriptions.RemoveRange(dbContext.Subscriptions.ToList());
        dbContext.Members.RemoveRange(dbContext.Members.ToList());
        dbContext.Users.RemoveRange(dbContext.Users.ToList());

        await dbContext.SaveChangesAsync(default);
    }
}

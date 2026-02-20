using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL;
using SubsTracker.DAL.Entities;
using SubsTracker.IntegrationTests.Configuration;
using SubsTracker.IntegrationTests.DataSeedEntities;
using SubsTracker.Domain.Enums;

namespace SubsTracker.IntegrationTests.Helpers.Group;

public class GroupTestsDataSeedingHelper(TestsWebApplicationFactory factory) : TestHelperBase(factory)
{
    public async Task<GroupSeedEntity> AddOnlyUserGroup()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var group = Fixture.Build<GroupEntity>()
            .With(g => g.Name, "Test Group")
            .Without(g => g.Members)
            .Without(g => g.SharedSubscriptions)
            .Without(g => g.User)
            .Without(g => g.UserId)
            .Create();

        await dbContext.UserGroups.AddAsync(group);
        await dbContext.SaveChangesAsync();

        return new GroupSeedEntity
        {
            UserEntity = null!,
            GroupEntity = group,
            Subscriptions = new List<SubscriptionEntity>(),
            Members = new List<MemberEntity>()
        };
    }

    public async Task<GroupSeedEntity> AddUserGroupWithMembers()
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

        var group = Fixture.Build<GroupEntity>()
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

        return new GroupSeedEntity
        {
            UserEntity = owner,
            GroupEntity = group,
            Members = allMembers,
            Subscriptions = new List<SubscriptionEntity>()
        };
    }

    public async Task<CreateMemberDto> AddCreateGroupMemberDto(Guid groupId, Guid userId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        // Check if the user already exists to avoid PK violation
        var existingUser = await db.Users.AnyAsync(u => u.Id == userId);

        if (!existingUser)
        {
            var user = Fixture.Build<UserEntity>()
                .With(u => u.Id, userId)
                .Without(u => u.Groups)
                .Without(u => u.Subscriptions)
                .Create();

            db.Users.Add(user);
            await db.SaveChangesAsync();
        }

        return new CreateMemberDto
        {
            UserId = userId,
            GroupId = groupId,
            Role = MemberRole.Participant
        };
    }

    public async Task<GroupSeedEntity> AddUserGroupAndUser()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        await db.Users.AddAsync(user);

        var group = Fixture.Build<GroupEntity>()
            .With(g => g.UserId, user.Id)
            .With(g => g.Name, "Group Without Members")
            .Create();

        db.UserGroups.Add(group);
        await db.SaveChangesAsync();

        return new GroupSeedEntity
        {
            UserEntity = user,
            GroupEntity = group,
            Members = new List<MemberEntity>(),
            Subscriptions = new List<SubscriptionEntity>()
        };
    }

    public async Task<MemberEntity> AddMemberOnly()
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        var user = Fixture.Build<UserEntity>()
            .With(u => u.Auth0Id, TestsAuthHandler.DefaultAuth0Id)
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions)
            .Create();

        var group = Fixture.Build<GroupEntity>()
            .With(g => g.UserId, user.Id)
            .Without(g => g.Members)
            .Without(g => g.SharedSubscriptions)
            .Without(g => g.User)
            .Create();

        var member = Fixture.Build<MemberEntity>()
            // 1. Assign the actual objects, not just IDs
            // This forces EF Core to use the established relationship
            .With(m => m.UserEntity, user)
            .With(m => m.GroupEntity, group)
            .With(m => m.Role, MemberRole.Participant)
            .Create();

        // 2. Add only the 'root' or the entities individually 
        // Since member has references to user and group, AddAsync(member) 
        // actually tracks all three, but being explicit is safer:
        await db.Users.AddAsync(user);
        await db.UserGroups.AddAsync(group);
        await db.Members.AddAsync(member);
    
        await db.SaveChangesAsync();

        return member;
    }

    public async Task<GroupSeedEntity> AddSeedUserOnly()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SubsDbContext>();

        // 1. ОЧИСТКА: Удаляем пользователя, если он остался от прошлых тестов
        var existingUser = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Auth0Id == TestsAuthHandler.DefaultAuth0Id);
        
        if (existingUser != null)
        {
            dbContext.Users.Remove(existingUser);
            await dbContext.SaveChangesAsync();
        }

        // 2. СОЗДАНИЕ: Теперь можно спокойно добавлять
        var user = Fixture.Build<UserEntity>()
            .With(u => u.Auth0Id, TestsAuthHandler.DefaultAuth0Id)
            .Without(u => u.Groups)
            .Without(u => u.Subscriptions) // На всякий случай отключаем навигационные свойства
            .Create();

        await dbContext.Users.AddAsync(user);
        await dbContext.SaveChangesAsync();

        return new GroupSeedEntity
        {
            UserEntity = user,
            Subscriptions = new List<SubscriptionEntity>(),
            GroupEntity = null!,
            Members = new List<MemberEntity>()
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

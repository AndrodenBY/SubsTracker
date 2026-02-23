using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using Shouldly;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.RedisSettings;
using SubsTracker.DAL.Entities;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Pagination;
using SubsTracker.UnitTests.TestsBase;

namespace SubsTracker.UnitTests;

public class UserServiceTests : UserServiceTestsBase
{
    [Fact]
    public async Task Create_WhenUserDoesNotExist_ShouldCreateAndReturnNewUser()
    {
        //Arrange
        var auth0Id = "auth0|123";
        var createDto = Fixture.Create<CreateUserDto>();
        var userEntity = Fixture.Build<UserEntity>().With(x => x.Email, createDto.Email).Create();
        var userDto = Fixture.Create<UserDto>();

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(null));

        Mapper.Map<UserEntity>(createDto).Returns(userEntity);
        UserRepository.Create(userEntity, Arg.Any<CancellationToken>()).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldNotBeNull();
        userEntity.Auth0Id.ShouldBe(auth0Id);
        await UserRepository.Received(1).Create(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await UserRepository.DidNotReceive().Update(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WhenCreateDtoIsNull_ReturnsNull()
    {
        //Act
        var result = await Service.Create(null!, CancellationToken.None);

        //Assert
        result.ShouldBeNull();
    }
    
    [Fact]
    public async Task Create_WhenUserExistsWithAuth0Id_ShouldJustReturnExisting()
    {
        //Arrange
        var auth0Id = "auth0|new";
        var ct = CancellationToken.None;
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<UserEntity>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.Auth0Id, "already-has-id")
            .Create();
        var userDto = Fixture.Create<UserDto>();

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<UserEntity, bool>>>(), ct)
            .Returns(existingUser);

        Mapper.Map<UserDto>(existingUser).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        await UserRepository.DidNotReceive().Update(Arg.Any<UserEntity>(), ct);
        await UserRepository.DidNotReceive().Create(Arg.Any<UserEntity>(), ct);
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Delete_WhenUserExists_DeletesUser()
    {
        //Arrange
        var auth0Id = "auth0|test-id";
        var ct = CancellationToken.None;
        var existingUser = Fixture.Build<UserEntity>()
            .With(x => x.Auth0Id, auth0Id)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(existingUser);

        UserRepository.Delete(existingUser, ct)
            .Returns(true);

        //Act
        var result = await Service.Delete(auth0Id, ct);

        //Assert
        result.ShouldBeTrue();
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, ct);
        await UserRepository.Received(1).Delete(existingUser, ct);
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var auth0Id = "auth0|non-existent";

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<UserEntity?>(null));

        //Act
        var act = async () => await Service.Delete(auth0Id, Arg.Any<CancellationToken>());

        //Assert
        var exception = await act.ShouldThrowAsync<UnknownIdentifierException>();
        exception.Message.ShouldContain(auth0Id);
        await UserRepository.DidNotReceive().Delete(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByFirstName_ReturnsMatchingUsers()
    {
        //Arrange
        var ct = CancellationToken.None;
        const string firstName = "Alexander";
        var filter = new UserFilterDto { FirstName = "aLeX" };

        var user = Fixture.Build<UserEntity>().With(u => u.FirstName, firstName).Create();
        var dto = Fixture.Build<UserDto>().With(u => u.FirstName, firstName).Create();
        
        var pagedList = new PaginatedList<UserEntity>([user], 1, 10, 1);

        UserRepository.GetAll(
                Arg.Any<Expression<Func<UserEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);

        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>()).Returns([dto]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldHaveSingleItem();
        result.Items[0].FirstName.ShouldBe(firstName);

        await UserRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Fact]
    public async Task GetAll_WhenRequestingSecondPage_ReturnsCorrectMetadata()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new UserFilterDto();
        var pagination = new PaginationParameters { PageNumber = 2, PageSize = 5 };
        
        var users = Fixture.CreateMany<UserEntity>(5).ToList();
        var pagedList = new PaginatedList<UserEntity>(users, 2, 5, 12);

        UserRepository.GetAll(
                Arg.Any<Expression<Func<UserEntity, bool>>>(), 
                Arg.Is(pagination), 
                Arg.Is(ct))
            .Returns(pagedList);

        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>())
            .Returns([.. Fixture.CreateMany<UserDto>(5)]);

        //Act
        var result = await Service.GetAll(filter, pagination, ct);

        //Assert
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(5);
        result.TotalCount.ShouldBe(12);
        result.PageCount.ShouldBe(3); 
        result.HasPreviousPage.ShouldBeTrue();
        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public async Task GetAll_WhenOnLastPage_HasNextPageIsFalse()
    {
        //Arrange
        var ct = CancellationToken.None;
        var pagination = new PaginationParameters { PageNumber = 2, PageSize = 10 };
        
        var pagedList = new PaginatedList<UserEntity>([.. Fixture.CreateMany<UserEntity>(5)], 2, 10, 15);

        UserRepository.GetAll(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Is(pagination), Arg.Is(ct))
            .Returns(pagedList);

        //Act
        var result = await Service.GetAll(new UserFilterDto(), pagination, ct);

        //Assert
        result.HasNextPage.ShouldBeFalse();
        result.HasPreviousPage.ShouldBeTrue();
        result.PageCount.ShouldBe(2);
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByLastName_ReturnsCorrectUser()
    {
        //Arrange
        var ct = CancellationToken.None;
        const string lastName = "Ivanov";
        var filter = new UserFilterDto { LastName = "IVAN" };

        var user = Fixture.Build<UserEntity>().With(u => u.LastName, lastName).Create();
        var dto = Fixture.Build<UserDto>().With(u => u.LastName, lastName).Create();
        
        var pagedList = new PaginatedList<UserEntity>([user], 1, 10, 1);

        UserRepository.GetAll(
                Arg.Any<Expression<Func<UserEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);

        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>()).Returns([dto]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldHaveSingleItem();
        result.Items[0].LastName.ShouldBe(lastName);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByEmail_ReturnsCorrectUser()
    {
        //Arrange
        var ct = CancellationToken.None;
        var userToFind = Fixture.Create<UserEntity>();
        var userDto = Fixture.Build<UserDto>()
            .With(u => u.Email, userToFind.Email)
            .With(u => u.Id, userToFind.Id)
            .Create();

        var filter = new UserFilterDto { Email = userToFind.Email };
        var pagedList = new PaginatedList<UserEntity>([userToFind], 1, 10, 1);

        UserRepository.GetAll(
                Arg.Any<Expression<Func<UserEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);

        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>()).Returns([userDto]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldNotBeNull();
        result.Items.Single().Email.ShouldBe(userToFind.Email);
        
        await UserRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserEntity, bool>>>(), 
            Arg.Any<PaginationParameters?>(), 
            ct);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentEmail_ReturnsEmptyList()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new UserFilterDto { Email = "nonexistent@example.com" };
        var emptyPagedList = new PaginatedList<UserEntity>([], 1, 10, 0);

        UserRepository.GetAll(
                Arg.Any<Expression<Func<UserEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(emptyPagedList);

        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>()).Returns([]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoUsers_ReturnsEmptyList()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new UserFilterDto();
        
        var emptyPagedList = new PaginatedList<UserEntity>([], 1, 10, 0);
        
        UserRepository.GetAll(
                Arg.Any<Expression<Func<UserEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(emptyPagedList);

        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>()).Returns([]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUsers()
    {
        //Arrange
        var ct = CancellationToken.None;
        var filter = new UserFilterDto();
    
        List<UserEntity> users = [.. Fixture.CreateMany<UserEntity>(3)];
        List<UserDto> userDtos = [.. Fixture.CreateMany<UserDto>(3)];
        
        var pagedList = new PaginatedList<UserEntity>(users, 1, 10, 3);

        UserRepository.GetAll(
                Arg.Any<Expression<Func<UserEntity, bool>>>(), 
                Arg.Any<PaginationParameters?>(), 
                Arg.Is(ct))
            .Returns(pagedList);

        Mapper.Map<List<UserDto>>(users).Returns(userDtos);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
        result.Items.ShouldBe(userDtos);
    }
    
    [Fact]
    public async Task GetByAuth0Id_WhenUserExists_ReturnsMappedUserDto()
    {
        //Arrange
        var auth0Id = "auth0|661f123456789";
        var ct = CancellationToken.None;
        var existingUser = Fixture.Create<UserEntity>();
        var expectedDto = Fixture.Create<UserDto>();

        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(existingUser);
            
        Mapper.Map<UserDto>(existingUser)
            .Returns(expectedDto);

        //Act
        var result = await Service.GetByAuth0Id(auth0Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEquivalentTo(expectedDto);
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, ct);
        Mapper.Received(1).Map<UserDto>(existingUser);
    }

    [Fact]
    public async Task GetByAuth0Id_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var nonExistentAuth0Id = "non-existent-id";
        var ct = CancellationToken.None;

        UserRepository.GetByAuth0Id(nonExistentAuth0Id, ct)
            .Returns((UserEntity?)null);

        //Act
        var act = () => Service.GetByAuth0Id(nonExistentAuth0Id, ct);

        //Assert
        var exception = await Should.ThrowAsync<UnknownIdentifierException>(act);
        exception.Message.ShouldContain(nonExistentAuth0Id);
        Mapper.DidNotReceive().Map<UserDto>(Arg.Any<UserEntity>());
    }

    [Fact]
    public async Task GetByAuth0Id_WhenAuth0IdIsEmpty_ThrowsNotFoundException()
    {
        //Arrange
        var emptyAuth0Id = string.Empty;

        UserRepository.GetByAuth0Id(emptyAuth0Id, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        //Act
        var act = () => Service.GetByAuth0Id(emptyAuth0Id, Arg.Any<CancellationToken>());

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(act);
    }
    
    [Fact]
    public async Task GetByAuth0Id_WhenCancellationTokenIsCancelled_ThrowsTaskCanceledException()
    {
        //Arrange
        var auth0Id = "auth0|cancel-test";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); 
        
        UserRepository.GetByAuth0Id(auth0Id, cancellationTokenSource.Token)
            .Returns(Task.FromCanceled<UserEntity?>(cancellationTokenSource.Token));

        //Act
        var act = () => Service.GetByAuth0Id(auth0Id, cancellationTokenSource.Token);

        //Assert
        await Should.ThrowAsync<OperationCanceledException>(act);
    }
    
    [Fact]
    public async Task GetById_WhenUserExists_ReturnsUser()
    {
        //Arrange
        var existingUser = Fixture.Create<UserEntity>();
        var ct = CancellationToken.None;
        var expectedDto = Fixture.Build<UserDto>()
            .With(user => user.Id, existingUser.Id)
            .With(user => user.FirstName, existingUser.FirstName)
            .With(user => user.Email, existingUser.Email)
            .Create();

        var cacheKey = $"{existingUser.Id}:{nameof(UserEntity)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            ct
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        UserRepository.GetById(existingUser.Id, ct)
            .Returns(existingUser);
        Mapper.Map<UserDto>(existingUser).Returns(expectedDto);

        //Act
        var result = await Service.GetById(existingUser.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingUser.Id);
        result.FirstName.ShouldBe(existingUser.FirstName);

        await UserRepository.Received(1).GetById(existingUser.Id, ct);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            ct
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async () => await Service.GetById(emptyId, Arg.Any<CancellationToken>());

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult());
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, Arg.Any<CancellationToken>());

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(fakeIdResult());
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var cachedDto = Fixture.Create<UserDto>();
        var cacheKey = RedisKeySetter.SetCacheKey<UserEntity>(cachedDto.Id);
        var ct = CancellationToken.None;
        var expirationTime = TimeSpan.FromMinutes(1);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            ct,
            expirationTime
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetById(cachedDto.Id, ct);

        //Assert
        result.ShouldBe(cachedDto);

        await UserRepository.DidNotReceive().GetById(Arg.Any<Guid>(), ct);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            ct,
            expirationTime
        );
    }
    
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUser()
    {
        //Arrange
        var userEntity = Fixture.Create<UserEntity>();
        var updateDto = Fixture.Create<UpdateUserDto>();
        var userDto = Fixture.Create<UserDto>();
        var ct = CancellationToken.None;
        
        UserRepository.GetByAuth0Id(userDto.Auth0Id, ct)
            .Returns(userEntity);

        UserRepository.Update(Arg.Any<UserEntity>(), ct)
            .Returns(userEntity);
        Mapper.Map(updateDto, userEntity).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Update(userDto.Auth0Id, updateDto, ct);

        //Assert
        result.ShouldNotBeNull();
        await UserRepository.Received(1).Update(Arg.Any<UserEntity>(), ct);
    }
    
    [Fact]
    public async Task Update_WhenNull_NotFoundException()
    {
        //Act
        var result = async () => await Service.Update(Guid.Empty, null!, Arg.Any<CancellationToken>());

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
}

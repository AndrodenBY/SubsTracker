using SubsTracker.BLL.Handlers.Signals.User;
using SubsTracker.Domain.Pagination;

namespace SubsTracker.UnitTests.UserService;

public class UserServiceTests : UserServiceTestsBase
{
    [Fact]
    public async Task Create_WhenUserDoesNotExist_ShouldCreateAndReturnNewUser()
    {
        //Arrange
        var auth0Id = "auth0|123";
        var createDto = Fixture.Create<CreateUserDto>();
        var userEntity = Fixture.Build<User>().With(x => x.Email, createDto.Email).Create();
        var userDto = Fixture.Create<UserDto>();
        var ct = CancellationToken.None;

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<User, bool>>>(), ct)
            .Returns((User?)null);

        Mapper.Map<User>(createDto).Returns(userEntity);
        UserRepository.Create(userEntity, ct).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        result.ShouldNotBeNull();
        userEntity.Auth0Id.ShouldBe(auth0Id);
        
        await UserRepository.Received(1).Create(userEntity, ct);
        await UserRepository.DidNotReceive().Update(Arg.Any<User>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<UserCreatedSignal>(), ct);
    }

    [Fact]
    public async Task Create_WhenUserExistsWithoutAuth0Id_ShouldUpdateIdAndPublishSignal()
    {
        //Arrange
        var auth0Id = "auth0|new-link";
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<User>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.Auth0Id, string.Empty)
            .Create();
        var ct = CancellationToken.None;

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<User, bool>>>(), ct)
            .Returns(existingUser);
        
        Mapper.Map<UserDto>(existingUser).Returns(Fixture.Create<UserDto>());

        //Act
        await Service.Create(auth0Id, createDto, ct);

        //Assert
        existingUser.Auth0Id.ShouldBe(auth0Id);
        await UserRepository.Received(1).Update(existingUser, ct);
        await Mediator.Received(1).Publish(Arg.Is<UserCreatedSignal>(s => s.ExternalId == auth0Id), ct);
    }

    [Fact]
    public async Task Create_WhenUserExistsWithAuth0Id_ShouldReturnExistingAndPublishSignal()
    {
        //Arrange
        var auth0Id = "auth0|existing";
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<User>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.Auth0Id, "already-has-id")
            .Create();
        var ct = CancellationToken.None;

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<User, bool>>>(), ct)
            .Returns(existingUser);

        Mapper.Map<UserDto>(existingUser).Returns(Fixture.Create<UserDto>());

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        result.ShouldNotBeNull();
        await UserRepository.DidNotReceive().Update(Arg.Any<User>(), ct);
        await UserRepository.DidNotReceive().Create(Arg.Any<User>(), ct);
        
        await Mediator.Received(1).Publish(Arg.Is<UserCreatedSignal>(s => s.ExternalId == "already-has-id"), ct);
    }
    
    [Fact]
    public async Task Delete_WhenUserExists_DeletesUser()
    {
        //Arrange
        var auth0Id = "auth0|test-id";
        var ct = CancellationToken.None;
        var existingUser = Fixture.Build<User>()
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
    
        await Mediator.Received(1).Publish(
            Arg.Is<UserDeletedSignal>(s => s.ExternalId == auth0Id), 
            ct);

        await UserRepository.Received(1).Delete(existingUser, ct);
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var auth0Id = "auth0|non-existent";
        var ct = CancellationToken.None;

        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns((User?)null);

        //Act & Assert
        var exception = await Should.ThrowAsync<UnknownIdentifierException>(async () => 
            await Service.Delete(auth0Id, ct));

        exception.Message.ShouldContain(auth0Id);
    
        await Mediator.DidNotReceive().Publish(Arg.Any<UserDeletedSignal>(), ct);
        await UserRepository.DidNotReceive().Delete(Arg.Any<User>(), ct);
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByEmail_ReturnsCorrectUser()
    {
        //Arrange
        var userToFind = Fixture.Create<User>();
        var userDto = Fixture.Build<UserDto>()
            .With(u => u.Email, userToFind.Email)
            .With(u => u.Id, userToFind.Id)
            .With(u => u.FirstName, userToFind.FirstName)
            .Create();

        var filter = new UserFilterDto { Email = userToFind.Email };
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var paginatedResult = new PaginatedList<User>(
            [userToFind], 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 1, 
            TotalCount: 1
        );
        
        UserRepository.GetAll(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Is<PaginationParameters>(p => p.PageNumber == 1),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedResult);
        
        Mapper.Map<UserDto>(userToFind).Returns(userDto);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        await UserRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<CancellationToken>()
        );

        result.ShouldNotBeNull();
        result.Items.ShouldHaveSingleItem();
        result.Items.First().Email.ShouldBe(userToFind.Email);
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentEmail_ReturnsEmptyPaginatedList()
    {
        //Arrange
        var filter = new UserFilterDto { Email = "nonexistent@example.com" };
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var emptyPaginatedResult = new PaginatedList<User>(
            new List<User>(), 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 0, 
            TotalCount: 0
        );
        
        UserRepository.GetAll(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(emptyPaginatedResult);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.PageCount.ShouldBe(0);
        
        await UserRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Is<PaginationParameters>(p => p.PageNumber == 1),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetAll_WhenNoUsers_ReturnsEmptyPaginatedList()
    {
        //Arrange
        var filter = new UserFilterDto();
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        var emptyPaginatedResult = new PaginatedList<User>(
            new List<User>(), 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 0, 
            TotalCount: 0
        );
        
        UserRepository.GetAll(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(emptyPaginatedResult);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
        result.PageCount.ShouldBe(0);
        
        await UserRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<User, bool>>>(),
            Arg.Any<PaginationParameters>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllPaginatedUsers()
    {
        //Arrange
        var users = Fixture.CreateMany<User>(3).ToList();
        var userDtos = Fixture.CreateMany<UserDto>(3).ToList();
        
        var paginatedEntities = new PaginatedList<User>(
            users, 
            PageNumber: 1, 
            PageSize: 10, 
            PageCount: 1, 
            TotalCount: 3
        );

        var filter = new UserFilterDto();
        var paginationParams = new PaginationParameters { PageNumber = 1, PageSize = 10 };
        
        UserRepository.GetAll(
                Arg.Any<Expression<Func<User, bool>>>(),
                Arg.Any<PaginationParameters>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(paginatedEntities);
        
        Mapper.Map<UserDto>(users[0]).Returns(userDtos[0]);
        Mapper.Map<UserDto>(users[1]).Returns(userDtos[1]);
        Mapper.Map<UserDto>(users[2]).Returns(userDtos[2]);

        //Act
        var result = await Service.GetAll(filter, paginationParams, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeEmpty();
        result.Items.Count.ShouldBe(3);
        result.TotalCount.ShouldBe(3);
        result.Items.ShouldBe(userDtos);
    }
    
    [Fact]
    public async Task GetByAuth0Id_WhenUserExists_ReturnsMappedUserDto()
    {
        //Arrange
        var auth0Id = "auth0|661f123456789";
        var existingUser = Fixture.Create<User>();
        var expectedDto = Fixture.Create<UserDto>();

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            CancellationToken.None
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        
        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
            
        Mapper.Map<UserDto>(existingUser)
            .Returns(expectedDto);

        //Act
        var result = await Service.GetByAuth0Id(auth0Id, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.ShouldBeEquivalentTo(expectedDto);
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        Mapper.Received(1).Map<UserDto>(existingUser);
    }

    [Fact]
    public async Task GetByAuth0Id_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var nonExistentAuth0Id = "non-existent-id";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            CancellationToken.None
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        
        UserRepository.GetByAuth0Id(nonExistentAuth0Id, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        //Act
        var act = () => Service.GetByAuth0Id(nonExistentAuth0Id, CancellationToken.None);

        //Assert
        var exception = await Should.ThrowAsync<UnknownIdentifierException>(act);
        exception.Message.ShouldContain(nonExistentAuth0Id);
        Mapper.DidNotReceive().Map<UserDto>(Arg.Any<User>());
    }

    [Fact]
    public async Task GetByAuth0Id_WhenAuth0IdIsEmpty_ThrowsNotFoundException()
    {
        //Arrange
        var emptyAuth0Id = string.Empty;

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            CancellationToken.None
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        UserRepository.GetByAuth0Id(emptyAuth0Id, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        //Act
        var act = () => Service.GetByAuth0Id(emptyAuth0Id, CancellationToken.None);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(act);
    }
    
    [Fact]
    public async Task GetById_WhenUserExists_ReturnsUser()
    {
        //Arrange
        var existingUser = Fixture.Create<User>();
        var expectedDto = Fixture.Build<UserDto>()
            .With(user => user.Id, existingUser.Id)
            .With(user => user.FirstName, existingUser.FirstName)
            .With(user => user.Email, existingUser.Email)
            .Create();

        var cacheKey = $"{existingUser.Id}:{nameof(User)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            CancellationToken.None
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        UserRepository.GetById(existingUser.Id, CancellationToken.None).Returns(existingUser);
        Mapper.Map<UserDto>(existingUser).Returns(expectedDto);

        //Act
        var result = await Service.GetById(existingUser.Id, CancellationToken.None);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingUser.Id);
        result.FirstName.ShouldBe(existingUser.FirstName);

        await UserRepository.Received(1).GetById(existingUser.Id, CancellationToken.None);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async () => await Service.GetById(emptyId, CancellationToken.None);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult());
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, CancellationToken.None);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(fakeIdResult());
    }

    [Fact]
    public async Task GetById_WhenCacheHit_ReturnsCachedDataAndSkipsRepo()
    {
        //Arrange
        var cachedDto = Fixture.Create<UserDto>();
        var cacheKey = RedisKeySetter.SetCacheKey<User>(cachedDto.Id);

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            CancellationToken.None
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetById(cachedDto.Id, CancellationToken.None);

        //Assert
        result.ShouldBe(cachedDto);

        await UserRepository.DidNotReceive().GetById(Arg.Any<Guid>(), CancellationToken.None);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            CancellationToken.None
        );
    }
    
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUser()
    {
        //Arrange
        var auth0Id = Fixture.Create<string>();
        var userEntity = Fixture.Build<User>().With(u => u.Auth0Id, auth0Id).Create();
        var updateDto = Fixture.Create<UpdateUserDto>();
        var userDto = Fixture.Build<UserDto>().With(d => d.Auth0Id, auth0Id).Create();
        var ct = CancellationToken.None;
    
        UserRepository.GetByAuth0Id(auth0Id, ct).Returns(userEntity);
        UserRepository.Update(userEntity, ct).Returns(userEntity);
    
        Mapper.Map(updateDto, userEntity).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Update(auth0Id, updateDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Auth0Id.ShouldBe(auth0Id);
    
        await UserRepository.Received(1).Update(userEntity, ct);
        await Mediator.Received(1).Publish(Arg.Is<UserUpdatedSignal>(s => s.ExternalId == auth0Id), ct);
    }

    [Fact]
    public async Task Update_WhenUserDoesNotExist_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var auth0Id = "non-existent-id";
        var updateDto = Fixture.Create<UpdateUserDto>();
        var ct = CancellationToken.None;

        UserRepository.GetByAuth0Id(auth0Id, ct).Returns((User?)null);

        //Act & Assert
        await Should.ThrowAsync<UnknownIdentifierException>(async () =>
        {
            await Service.Update(auth0Id, updateDto, ct);
        });

        await UserRepository.DidNotReceive().Update(Arg.Any<User>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<UserUpdatedSignal>(), ct);
    }
}

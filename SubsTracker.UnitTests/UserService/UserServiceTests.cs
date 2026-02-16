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

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        Mapper.Map<User>(createDto).Returns(userEntity);
        UserRepository.Create(userEntity, Arg.Any<CancellationToken>()).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, default);

        //Assert
        result.ShouldNotBeNull();
        userEntity.Auth0Id.ShouldBe(auth0Id);
        await UserRepository.Received(1).Create(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await UserRepository.DidNotReceive().Update(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Create_WhenCreateDtoIsNull_ReturnsNull()
    {
        //Act
        var result = await Service.Create(null!, default);

        //Assert
        result.ShouldBeNull();
    }
    
    [Fact]
    public async Task Create_WhenUserExistsWithAuth0Id_ShouldJustReturnExisting()
    {
        //Arrange
        var auth0Id = "auth0|new";
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<User>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.Auth0Id, "already-has-id")
            .Create();
        var userDto = Fixture.Create<UserDto>();

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<User, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        Mapper.Map<UserDto>(existingUser).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, default);

        //Assert
        await UserRepository.DidNotReceive().Update(Arg.Any<User>(), Arg.Any<CancellationToken>());
        await UserRepository.DidNotReceive().Create(Arg.Any<User>(), Arg.Any<CancellationToken>());
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Delete_WhenUserExists_DeletesUser()
    {
        //Arrange
        var auth0Id = "auth0|test-id";
        var existingUser = Fixture.Build<User>()
            .With(x => x.Auth0Id, auth0Id)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        UserRepository.Delete(existingUser, Arg.Any<CancellationToken>())
            .Returns(true);

        //Act
        var result = await Service.Delete(auth0Id, default);

        //Assert
        result.ShouldBeTrue();
        await UserRepository.Received(1).GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());
        await UserRepository.Received(1).Delete(existingUser, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var auth0Id = "auth0|non-existent";

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<User?>(null));

        //Act
        var act = async () => await Service.Delete(auth0Id, default);

        //Assert
        var exception = await act.ShouldThrowAsync<UnknownIdentifierException>();
        exception.Message.ShouldContain(auth0Id);
        await UserRepository.DidNotReceive().Delete(Arg.Any<User>(), Arg.Any<CancellationToken>());
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
            new List<User> { userToFind }, 
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
        var result = await Service.GetAll(filter, paginationParams, default);

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
        var result = await Service.GetAll(filter, paginationParams, default);

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
        var result = await Service.GetAll(filter, paginationParams, default);

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
        var result = await Service.GetAll(filter, paginationParams, default);

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

        var cacheKey = $"{auth0Id}:{nameof(User)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            default
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
        var result = await Service.GetByAuth0Id(auth0Id, default);

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

        var cacheKey = $"{nonExistentAuth0Id}:{nameof(User)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        
        UserRepository.GetByAuth0Id(nonExistentAuth0Id, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        //Act
        var act = () => Service.GetByAuth0Id(nonExistentAuth0Id, default);

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

        var cacheKey = $"{emptyAuth0Id}:{nameof(User)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        UserRepository.GetByAuth0Id(emptyAuth0Id, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        //Act
        var act = () => Service.GetByAuth0Id(emptyAuth0Id, default);

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
            default
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        UserRepository.GetById(existingUser.Id, default).Returns(existingUser);
        Mapper.Map<UserDto>(existingUser).Returns(expectedDto);

        //Act
        var result = await Service.GetById(existingUser.Id, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingUser.Id);
        result.FirstName.ShouldBe(existingUser.FirstName);

        await UserRepository.Received(1).GetById(existingUser.Id, default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            default
        );
    }

    [Fact]
    public async Task GetById_WhenEmptyGuid_ThrowsNotFoundException()
    {
        //Arrange
        var emptyId = Guid.Empty;

        //Act
        var emptyIdResult = async () => await Service.GetById(emptyId, default);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(emptyIdResult());
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ThrowsNotFoundException()
    {
        //Arrange
        var fakeId = Guid.NewGuid();

        //Act
        var fakeIdResult = async () => await Service.GetById(fakeId, default);

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
            default
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetById(cachedDto.Id, default);

        //Assert
        result.ShouldBe(cachedDto);

        await UserRepository.DidNotReceive().GetById(Arg.Any<Guid>(), default);
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            default
        );
    }
    
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUser()
    {
        //Arrange
        var userEntity = Fixture.Create<User>();
        var updateDto = Fixture.Create<UpdateUserDto>();
        var userDto = Fixture.Create<UserDto>();
        
        UserRepository.GetByAuth0Id(userDto.Auth0Id, default)
            .Returns(userEntity);

        UserRepository.Update(Arg.Any<User>(), default).Returns(userEntity);
        Mapper.Map(updateDto, userEntity).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Update(userDto.Auth0Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        await UserRepository.Received(1).Update(Arg.Any<User>(), default);
    }
    
    [Fact]
    public async Task Update_WhenNull_NotFoundException()
    {
        //Act
        var result = async () => await Service.Update(Guid.Empty, null!, default);

        //Assert
        await result.ShouldThrowAsync<UnknownIdentifierException>();
    }
}

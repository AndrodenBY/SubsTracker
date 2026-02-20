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
        var result = await Service.Create(null!, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBeNull();
    }
    
    [Fact]
    public async Task Create_WhenUserExistsWithAuth0Id_ShouldJustReturnExisting()
    {
        //Arrange
        var auth0Id = "auth0|new";
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<UserEntity>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.Auth0Id, "already-has-id")
            .Create();
        var userDto = Fixture.Create<UserDto>();

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(existingUser);

        Mapper.Map<UserDto>(existingUser).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, Arg.Any<CancellationToken>());

        //Assert
        await UserRepository.DidNotReceive().Update(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        await UserRepository.DidNotReceive().Create(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Delete_WhenUserExists_DeletesUser()
    {
        //Arrange
        var auth0Id = "auth0|test-id";
        var existingUser = Fixture.Build<UserEntity>()
            .With(x => x.Auth0Id, auth0Id)
            .Create();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);

        UserRepository.Delete(existingUser, Arg.Any<CancellationToken>())
            .Returns(true);

        //Act
        var result = await Service.Delete(auth0Id, Arg.Any<CancellationToken>());

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
            .Returns(Task.FromResult<UserEntity?>(null));

        //Act
        var act = async () => await Service.Delete(auth0Id, Arg.Any<CancellationToken>());

        //Assert
        var exception = await act.ShouldThrowAsync<UnknownIdentifierException>();
        exception.Message.ShouldContain(auth0Id);
        await UserRepository.DidNotReceive().Delete(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task GetAll_WhenFilteredByEmail_ReturnsCorrectUser()
    {
        //Arrange
        var userToFind = Fixture.Create<UserEntity>();
        var userDto = Fixture.Build<UserDto>()
            .With(user => user.Email, userToFind.Email)
            .With(user => user.Id, userToFind.Id)
            .With(user => user.FirstName, userToFind.FirstName)
            .Create();

        var filter = new UserFilterDto { Email = userToFind.Email };

        UserRepository.GetAll(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<UserEntity> { userToFind });

        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>())
            .Returns(new List<UserDto> { userDto });

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        await UserRepository.Received(1).GetAll(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>());
        result.ShouldNotBeNull();
        result.Single().Email.ShouldBe(userToFind.Email);
    }

    [Fact]
    public async Task GetAll_WhenFilteredByNonExistentEmail_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserFilterDto { Email = "nonexistent@example.com" };

        UserRepository.GetAll(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<UserEntity>());
        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>()).Returns(new List<UserDto>());

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenNoUsers_ReturnsEmptyList()
    {
        //Arrange
        var filter = new UserFilterDto();

        UserRepository.GetAll(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(new List<UserEntity>());
        Mapper.Map<List<UserDto>>(Arg.Any<List<UserEntity>>()).Returns(new List<UserDto>());

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAll_WhenFilterIsEmpty_ReturnsAllUsers()
    {
        //Arrange
        var users = Fixture.CreateMany<UserEntity>(3).ToList();
        var userDtos = Fixture.CreateMany<UserDto>(3).ToList();

        var filter = new UserFilterDto();

        UserRepository.GetAll(Arg.Any<Expression<Func<UserEntity, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(users);
        Mapper.Map<List<UserDto>>(users).Returns(userDtos);

        //Act
        var result = await Service.GetAll(filter, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result.Count.ShouldBe(3);
        result.ShouldBe(userDtos);
    }
    
    [Fact]
    public async Task GetByAuth0Id_WhenUserExists_ReturnsMappedUserDto()
    {
        //Arrange
        var auth0Id = "auth0|661f123456789";
        var existingUser = Fixture.Create<UserEntity>();
        var expectedDto = Fixture.Create<UserDto>();

        UserRepository.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>())
            .Returns(existingUser);
            
        Mapper.Map<UserDto>(existingUser)
            .Returns(expectedDto);

        //Act
        var result = await Service.GetByAuth0Id(auth0Id, Arg.Any<CancellationToken>());

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

        UserRepository.GetByAuth0Id(nonExistentAuth0Id, Arg.Any<CancellationToken>())
            .Returns((UserEntity?)null);

        //Act
        var act = () => Service.GetByAuth0Id(nonExistentAuth0Id, Arg.Any<CancellationToken>());

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
        var expectedDto = Fixture.Build<UserDto>()
            .With(user => user.Id, existingUser.Id)
            .With(user => user.FirstName, existingUser.FirstName)
            .With(user => user.Email, existingUser.Email)
            .Create();

        var cacheKey = $"{existingUser.Id}:{nameof(UserEntity)}";

        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            Arg.Any<CancellationToken>()
        )!.Returns(callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto>>>();
            return factory();
        });
        UserRepository.GetById(existingUser.Id, Arg.Any<CancellationToken>()).Returns(existingUser);
        Mapper.Map<UserDto>(existingUser).Returns(expectedDto);

        //Act
        var result = await Service.GetById(existingUser.Id, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingUser.Id);
        result.FirstName.ShouldBe(existingUser.FirstName);

        await UserRepository.Received(1).GetById(existingUser.Id, Arg.Any<CancellationToken>());
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            Arg.Any<CancellationToken>()
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

        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            Arg.Any<CancellationToken>()
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetById(cachedDto.Id, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldBe(cachedDto);

        await UserRepository.DidNotReceive().GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<TimeSpan>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            Arg.Any<CancellationToken>()
        );
    }
    
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUser()
    {
        //Arrange
        var userEntity = Fixture.Create<UserEntity>();
        var updateDto = Fixture.Create<UpdateUserDto>();
        var userDto = Fixture.Create<UserDto>();
        
        UserRepository.GetByAuth0Id(userDto.Auth0Id, Arg.Any<CancellationToken>())
            .Returns(userEntity);

        UserRepository.Update(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>()).Returns(userEntity);
        Mapper.Map(updateDto, userEntity).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Update(userDto.Auth0Id, updateDto, Arg.Any<CancellationToken>());

        //Assert
        result.ShouldNotBeNull();
        await UserRepository.Received(1).Update(Arg.Any<UserEntity>(), Arg.Any<CancellationToken>());
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

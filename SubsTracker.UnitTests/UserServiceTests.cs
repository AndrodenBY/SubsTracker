using System.Linq.Expressions;
using AutoFixture;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Mediator.Signals;
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
        var ct = CancellationToken.None;
        var createDto = Fixture.Create<CreateUserDto>();
        var userEntity = Fixture.Build<UserEntity>().With(x => x.Email, createDto.Email).Create();
        var userDto = Fixture.Create<UserDto>();

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<UserEntity, bool>>>(), ct)
            .Returns(Task.FromResult<UserEntity?>(null));

        Mapper.Map<UserEntity>(createDto).Returns(userEntity);
        UserRepository.Create(userEntity, ct).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        result.ShouldNotBeNull();
        userEntity.Auth0Id.ShouldBe(auth0Id);
        
        await UserRepository.Received(1).Create(userEntity, ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<UserSignals.Created>(), ct);
    }

    [Fact]
    public async Task Create_WhenUserExistsButHasNoAuth0Id_ShouldUpdateAndPublishSignal()
    {
        //Arrange
        var auth0Id = "auth0|link-me";
        var ct = CancellationToken.None;
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<UserEntity>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.Auth0Id, string.Empty)
            .Create();
        var userDto = Fixture.Create<UserDto>();

        UserRepository.GetByPredicate(Arg.Any<Expression<Func<UserEntity, bool>>>(), ct)
            .Returns(existingUser);
        Mapper.Map<UserDto>(existingUser).Returns(userDto);

        //Act
        var result = await Service.Create(auth0Id, createDto, ct);

        //Assert
        await UserRepository.Received(1).Update(Arg.Is<UserEntity>(u => u.Auth0Id == auth0Id), ct);
        await Mediator.Received(1).Publish(
            Arg.Is<UserSignals.Created>(s => s.Auth0Id == auth0Id), 
            ct);
            
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task Create_WhenUserExistsWithAuth0Id_ShouldJustReturnAndPublishSignal()
    {
        //Arrange
        var auth0Id = "auth0|existing";
        var ct = CancellationToken.None;
        var createDto = Fixture.Create<CreateUserDto>();
        var existingUser = Fixture.Build<UserEntity>()
            .With(x => x.Email, createDto.Email)
            .With(x => x.Auth0Id, auth0Id)
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
        await Mediator.Received(1).Publish(
            Arg.Is<UserSignals.Created>(s => s.Auth0Id == auth0Id), 
            ct);
            
        result.ShouldNotBeNull();
    }
    
    [Fact]
    public async Task Delete_WhenUserExists_DeletesUserAndPublishesSignal()
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
    
        await Mediator.Received(1).Publish(
            Arg.Is<UserSignals.Deleted>(s => s.Auth0Id == auth0Id), 
            ct);

        await UserRepository.Received(1).Delete(existingUser, ct);
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ThrowsUnknownIdentifierExceptionAndDoesNotPublish()
    {
        //Arrange
        var auth0Id = "auth0|non-existent";
        var ct = CancellationToken.None;

        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(Task.FromResult<UserEntity?>(null));

        //Act
        var act = async () => await Service.Delete(auth0Id, ct);

        //Assert
        var exception = await act.ShouldThrowAsync<UnknownIdentifierException>();
        exception.Message.ShouldContain(auth0Id);
        
        await UserRepository.DidNotReceive().Delete(Arg.Any<UserEntity>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<UserSignals.Deleted>(), ct);
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
        
        Mapper.Map<UserDto>(Arg.Is<UserEntity>(u => u.Id == user.Id))
            .Returns(dto);

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
        
        Mapper.Map<UserDto>(Arg.Is<UserEntity>(u => u.Id == user.Id)).Returns(dto);

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
        
        Mapper.Map<UserDto>(Arg.Is<UserEntity>(u => u.Id == userToFind.Id)).Returns(userDto);

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
        
        Mapper.Map<UserDto>(Arg.Any<UserEntity>())
            .Returns(userDtos[0], userDtos[1], userDtos[2]);

        //Act
        var result = await Service.GetAll(filter, null, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Items.Count.ShouldBe(3);
        result.Items.ShouldBe(userDtos);
    
        await UserRepository.Received(1).GetAll(
            Arg.Any<Expression<Func<UserEntity, bool>>>(),
            Arg.Any<PaginationParameters?>(),
            ct
        );
    }
    
    [Fact]
    public async Task GetByAuth0Id_WhenUserExists_ReturnsMappedUserDto()
    {
        //Arrange
        var auth0Id = "auth0|661f123456789";
        var ct = CancellationToken.None;
        var existingUser = Fixture.Create<UserEntity>();
        var expectedDto = Fixture.Create<UserDto>();
        
        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            ct
        ).Returns(async callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto?>>>();
            return await factory();
        });

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
    }

    [Fact]
    public async Task GetByAuth0Id_WhenUserDoesNotExist_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var nonExistentAuth0Id = "non-existent-id";
        var ct = CancellationToken.None;
        
        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            ct
        ).Returns(async callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto?>>>();
            return await factory();
        });
        
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
    public async Task GetByAuth0Id_WhenAuth0IdIsEmpty_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var ct = CancellationToken.None;
        var emptyAuth0Id = string.Empty;
        
        UserRepository.GetByAuth0Id(emptyAuth0Id, ct)
            .Returns((UserEntity?)null);
        
        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            ct
        ).Returns(async callInfo =>
        {
            var factory = callInfo.Arg<Func<Task<UserDto?>>>();
            return await factory(); 
        });

        //Act
        var act = () => Service.GetByAuth0Id(emptyAuth0Id, ct);

        //Assert
        await Should.ThrowAsync<UnknownIdentifierException>(act);
    }
    
    [Fact]
    public async Task GetByAuth0Id_WhenCancellationTokenIsCancelled_ThrowsOperationCanceledException()
    {
        //Arrange
        var auth0Id = "auth0|cancel-test";
        var cts = new CancellationTokenSource();
        await cts.CancelAsync(); 
        var token = cts.Token;
        
        CacheService.CacheDataWithLock(
            Arg.Any<string>(),
            Arg.Any<Func<Task<UserDto?>>>(),
            token
        ).ThrowsAsync(new OperationCanceledException(token));

        //Act
        var act = () => Service.GetByAuth0Id(auth0Id, token);

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
        var ct = CancellationToken.None;
        var cacheKey = RedisKeySetter.SetCacheKey<UserEntity>(cachedDto.Id);
        
        CacheService.CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            ct
        ).Returns(cachedDto);

        //Act
        var result = await Service.GetById(cachedDto.Id, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(cachedDto.Id);
        
        await UserRepository.DidNotReceive().GetById(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await CacheService.Received(1).CacheDataWithLock(
            cacheKey,
            Arg.Any<Func<Task<UserDto?>>>(),
            ct
        );
    }
    
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUserAndPublishesSignal()
    {
        //Arrange
        var ct = CancellationToken.None;
        var auth0Id = "auth0|update-test";
        var userEntity = Fixture.Build<UserEntity>()
            .With(x => x.Auth0Id, auth0Id)
            .Create();
        var updateDto = Fixture.Create<UpdateUserDto>();
        var userDto = Fixture.Build<UserDto>()
            .With(x => x.Auth0Id, auth0Id)
            .Create();
        
        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(userEntity);
        
        UserRepository.Update(Arg.Any<UserEntity>(), ct)
            .Returns(x => (UserEntity)x[0]);

        Mapper.Map(updateDto, userEntity).Returns(userEntity);
        Mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await Service.Update(auth0Id, updateDto, ct);

        //Assert
        result.ShouldNotBeNull();
        result.Auth0Id.ShouldBe(auth0Id);
        
        await UserRepository.Received(1).Update(userEntity, ct);
        await Mediator.Received(1).Publish(
            Arg.Is<UserSignals.Updated>(s => s.Auth0Id == auth0Id), 
            ct);
    }

    [Fact]
    public async Task Update_WhenUserDoesNotExist_ThrowsUnknownIdentifierException()
    {
        //Arrange
        var auth0Id = "non-existent-user";
        var updateDto = Fixture.Create<UpdateUserDto>();
        var ct = CancellationToken.None;
        
        UserRepository.GetByAuth0Id(auth0Id, ct)
            .Returns(Task.FromResult<UserEntity?>(null));

        //Act
        var act = async () => await Service.Update(auth0Id, updateDto, ct);

        //Assert
        await act.ShouldThrowAsync<UnknownIdentifierException>();
        await UserRepository.DidNotReceive().Update(Arg.Any<UserEntity>(), ct);
        await Mediator.DidNotReceive().Publish(Arg.Any<UserSignals.Updated>(), ct);
    }
}

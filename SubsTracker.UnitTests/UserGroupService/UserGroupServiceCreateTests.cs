namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceCreateTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task Create_WhenCalled_ReturnsCreatedUserGroupDto()
    {
        //Arrange
        var createDto = Fixture.Create<CreateUserGroupDto>();
        var userGroupEntity = Fixture.Build<UserGroup>()
            .With(userGroup => userGroup.Name, createDto.Name)
            .With(userGroup => userGroup.UserId, createDto.UserId)
            .Create();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupEntity.Name)
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();

        UserRepository.GetById(createDto.UserId, default).Returns(new User { Id = createDto.UserId });
        GroupRepository.Create(Arg.Any<UserGroup>(), default).Returns(userGroupEntity);
        Mapper.Map<UserGroup>(Arg.Any<CreateUserGroupDto>()).Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(userGroupDto);

        //Act
        var result = await Service.Create(createDto, default);

        //Assert
        result.ShouldNotBeNull();
        await GroupRepository.Received(1).Create(Arg.Any<UserGroup>(), default);
        result.ShouldBeEquivalentTo(userGroupDto);
    }

    [Fact]
    public async Task Create_WhenEmptyDto_ThrowsValidationException()
    {
        //Arrange
        var createDto = new CreateUserGroupDto { Name = string.Empty, UserId = Guid.Empty };

        //Act & Assert
        await Should.ThrowAsync<ValidationException>(async () =>
        {
            await Service.Create(Guid.NewGuid(), createDto, default);
        });
    }

    [Fact]
    public async Task Create_WhenCalled_CallsRepositoryExactlyOnce()
    {
        //Arrange
        var createDto = Fixture.Create<CreateUserGroupDto>();
        var userGroupEntity = Fixture.Build<UserGroup>()
            .With(userGroup => userGroup.Name, createDto.Name)
            .With(userGroup => userGroup.UserId, createDto.UserId)
            .Create();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, userGroupEntity.Name)
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();

        UserRepository.GetById(createDto.UserId, default)
            .Returns(new User { Id = createDto.UserId });
        GroupRepository.Create(Arg.Any<UserGroup>(), default)
            .Returns(userGroupEntity);
        Mapper.Map<UserGroup>(Arg.Any<CreateUserGroupDto>())
            .Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(Arg.Any<UserGroup>())
            .Returns(userGroupDto);

        //Act
        await Service.Create(createDto, default);

        //Assert
        await GroupRepository.Received(1).Create(Arg.Any<UserGroup>(), default);
    }
}

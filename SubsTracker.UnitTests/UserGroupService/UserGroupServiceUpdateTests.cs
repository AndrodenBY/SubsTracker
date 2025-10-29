namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceUpdateTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedUserGroupDto()
    {
        //Arrange
        var userGroupEntity = Fixture.Create<UserGroup>();
        var updateDto = Fixture.Build<UpdateUserGroupDto>()
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();
        var userGroupDto = Fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, updateDto.Name)
            .With(userGroup => userGroup.Id, updateDto.Id)
            .Create();

        GroupRepository.GetById(updateDto.Id, default).Returns(userGroupEntity);
        GroupRepository.Update(Arg.Any<UserGroup>(), default).Returns(userGroupEntity);
        Mapper.Map(updateDto, userGroupEntity).Returns(userGroupEntity);
        Mapper.Map<UserGroupDto>(userGroupEntity).Returns(userGroupDto);

        //Act
        var result = await Service.Update(updateDto.Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeEquivalentTo(userGroupEntity.Id);
        result.Name.ShouldBe(updateDto.Name);
        result.Name.ShouldNotBe(userGroupEntity.Name);
        await GroupRepository.Received(1).Update(Arg.Any<UserGroup>(), default);
    }

    [Fact]
    public async Task Update_WhenGivenEmptyModel_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateUserGroupDto();

        //Act
        var result = async () => await Service.Update(Guid.Empty, emptyDto, default);

        //Assert
        await result.ShouldThrowAsync<NotFoundException>();
    }
}
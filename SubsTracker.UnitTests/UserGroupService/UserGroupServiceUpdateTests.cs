namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceUpdateTests : UserGroupServiceTestsBase
{
    [Fact]
    public async Task Update_WhenValidModel_ReturnsUpdatedUserGroupDto()
    {
        //Arrange
        var userGroupEntity = _fixture.Create<UserGroup>();
        var updateDto = _fixture.Build<UpdateUserGroupDto>()
            .With(userGroup => userGroup.Id, userGroupEntity.Id)
            .Create();
        var userGroupDto = _fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, updateDto.Name)
            .With(userGroup => userGroup.Id, updateDto.Id )
            .Create();
        
        _repository.GetById(updateDto.Id, default).Returns(userGroupEntity);
        _repository.Update(Arg.Any<UserGroup>(), default).Returns(userGroupEntity);
        _mapper.Map(updateDto, userGroupEntity).Returns(userGroupEntity);
        _mapper.Map<UserGroupDto>(userGroupEntity).Returns(userGroupDto);
        
        //Act
        var result = await _service.Update(updateDto.Id, updateDto, default);
        
        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeEquivalentTo(userGroupEntity.Id);
        result.Name.ShouldBe(updateDto.Name);
        result.Name.ShouldNotBe(userGroupEntity.Name);
        await _repository.Received(1).Update(Arg.Any<UserGroup>(), default);
    }

    [Fact]
    public async Task Update_WhenGivenEmptyModel_ReturnsNotFoundException()
    {
        //Arrange
        var emptyDto = new UpdateUserGroupDto();

        //Act & Assert
        await Should.ThrowAsync<NotFoundException>(() => _service.Update(Guid.Empty, emptyDto, default));
    }
}

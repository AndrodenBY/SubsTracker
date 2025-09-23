namespace SubsTracker.UnitTests.UserService;

public class UserServiceUpdateTests : UserServiceTestsBase
{
    [Fact]
    public async Task Update_WhenCalled_ReturnsUpdatedUser()
    {
        //Arrange
        var userEntity = _fixture.Create<User>();
        var updateDto = _fixture.Build<UpdateUserDto>()
            .With(userGroup => userGroup.Id, userEntity.Id)
            .Create();
        var userDto = _fixture.Build<UserDto>()
            .With(userGroup => userGroup.Id, updateDto.Id )
            .Create();

        _repository.GetById(updateDto.Id, default).Returns(userEntity);
        _repository.Update(Arg.Any<User>(), default).Returns(userEntity);
        _mapper.Map(updateDto, userEntity).Returns(userEntity);
        _mapper.Map<UserDto>(userEntity).Returns(userDto);

        //Act
        var result = await _service.Update(updateDto.Id, updateDto, default);

        //Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBeEquivalentTo(userEntity.Id);
        await _repository.Received(1).Update(Arg.Any<User>(), default);
    }

    [Fact]
    public async Task Update_WhenNull_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await _service.Update(Guid.Empty, null, default));
    }

}

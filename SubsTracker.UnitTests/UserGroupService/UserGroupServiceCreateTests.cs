using SubsTracker.Domain.Enums;
using SubsTracker.Domain.Exceptions;

namespace SubsTracker.UnitTests.UserGroupService;

public class UserGroupServiceCreateTests : UserGroupServiceTestsBase
{
    private readonly UserGroup _userGroupEntity;
    private readonly UserGroupDto _userGroupDto;
    private readonly CreateUserGroupDto _createDto;
    private readonly CreateGroupMemberDto _createMemberDto;

    public UserGroupServiceCreateTests()
    {
        _createDto = _fixture.Create<CreateUserGroupDto>();
        _userGroupEntity = _fixture.Build<UserGroup>()
            .With(userGroup => userGroup.Name, _createDto.Name)
            .With(userGroup => userGroup.UserId, _createDto.UserId)
            .Create();
        _userGroupDto = _fixture.Build<UserGroupDto>()
            .With(userGroup => userGroup.Name, _userGroupEntity.Name)
            .With(userGroup => userGroup.Id, _userGroupEntity.Id )
            .Create();
        
        _createMemberDto = new CreateGroupMemberDto { UserId = _createDto.UserId, GroupId = _userGroupDto.Id, Role = MemberRole.Admin };
    }
    
    [Fact]
    public async Task Create_WhenCalled_ReturnsCreatedUserGroupDto()
    {
        // Arrange
        _userRepository.GetById(_createDto.UserId, Arg.Any<CancellationToken>()).Returns(new User { Id = _createDto.UserId });
        _repository.Create(Arg.Any<UserGroup>(), Arg.Any<CancellationToken>()).Returns(_userGroupEntity);
        _mapper.Map<UserGroup>(Arg.Any<CreateUserGroupDto>()).Returns(_userGroupEntity);
        _mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(_userGroupDto);

        // Act
        var result = await _service.Create(_createDto, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        await _repository.Received(1).Create(Arg.Any<UserGroup>(), Arg.Any<CancellationToken>());
        result.ShouldBeEquivalentTo(_userGroupDto);
    }
    
    [Fact]
    public async Task Create_WhenEmptyDto_ThrowsValidationException()
    {
        // Arrange
        var createDto = new CreateUserGroupDto{ Name = string.Empty, UserId = Guid.Empty};
    
        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await _service.Create(Guid.NewGuid(), createDto, CancellationToken.None);
        });
    }
    
    [Fact]
    public async Task Create_WhenGroupIsCreated_AddsAdminMember()
    {
        // Arrange
        //todo: invalid operation exception
        
        var createdMemberDto = new GroupMemberDto
        {
            UserId = _createMemberDto.UserId,
            GroupId = _createMemberDto.GroupId,
            Role = _createMemberDto.Role
        };
    
        _userRepository.GetById(_createDto.UserId, Arg.Any<CancellationToken>()).Returns(new User { Id = _createDto.UserId });
    
        _repository.Create(Arg.Any<UserGroup>(), Arg.Any<CancellationToken>())
            .Returns(_userGroupEntity);

        // Настраиваем заглушку для memberService, чтобы она возвращала DTO правильного типа
        //_memberService.Create(Arg.Any<CreateGroupMemberDto>(), Arg.Any<CancellationToken>())
          //  .Returns(createdMemberDto);
        
        _memberService.Create(Arg.Any<CreateGroupMemberDto>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(createdMemberDto));

        _mapper.Map<UserGroupDto>(_userGroupEntity).Returns(_userGroupDto);

        // Act
        var result = await _service.Create(_createDto.UserId, _createDto, CancellationToken.None);

        // Assert
        // Проверяем, что метод Create в MemberService был вызван с нужными параметрами
        await _memberService.Received(1).Create(
            Arg.Is<CreateGroupMemberDto>(dto => 
                dto.UserId == _createDto.UserId && 
                dto.GroupId == _userGroupDto.Id && 
                dto.Role == MemberRole.Admin),
            Arg.Any<CancellationToken>()
        );

        result.ShouldNotBeNull();
        result.Id.ShouldBe(_userGroupDto.Id);
        result.Name.ShouldBe(_userGroupDto.Name);
    }
    
    [Fact]
    public async Task Create_WhenCalled_CallsRepositoryExactlyOnce()
    {
        // Arrange
        _userRepository.GetById(_createDto.UserId, Arg.Any<CancellationToken>()).Returns(new User { Id = _createDto.UserId });
        _repository.Create(Arg.Any<UserGroup>(), Arg.Any<CancellationToken>()).Returns(_userGroupEntity);
        _mapper.Map<UserGroup>(Arg.Any<CreateUserGroupDto>()).Returns(_userGroupEntity);
        _mapper.Map<UserGroupDto>(Arg.Any<UserGroup>()).Returns(_userGroupDto);

        // Act
        await _service.Create(_createDto, CancellationToken.None);

        // Assert
        await _repository.Received(1).Create(Arg.Any<UserGroup>(), Arg.Any<CancellationToken>());
    }
}

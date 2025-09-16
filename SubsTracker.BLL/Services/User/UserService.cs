using System.Linq.Expressions;
using AutoMapper;
using LinqKit;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.BLL.Helpers.Filters;
using SubsTracker.BLL.Interfaces;
using SubsTracker.BLL.Interfaces.User;
using SubsTracker.DAL.Interfaces.Repositories;
using SubsTracker.DAL.Models.User;
using SubsTracker.Domain.Filter;
using UserDto = SubsTracker.BLL.DTOs.User.UserDto;
using CreateUserDto = SubsTracker.BLL.DTOs.User.Create.CreateUserDto;
using UpdateUserDto = SubsTracker.BLL.DTOs.User.Update.UpdateUserDto;
using UserModel = SubsTracker.DAL.Models.User.User;

namespace SubsTracker.BLL.Services.User;

public class UserService(
    IRepository<UserModel> repository, 
    IMapper mapper,
    IService<GroupMember, GroupMemberDto, CreateGroupMemberDto, UpdateGroupMemberDto, GroupMemberFilterDto> memberService
        ) : Service<UserModel, UserDto, CreateUserDto, UpdateUserDto, UserFilterDto>(repository, mapper), IUserService
{
    public async Task<List<UserDto>> GetAll(UserFilterDto? filter, CancellationToken cancellationToken)
    {
        var predicate = UserFilterHelper.CreatePredicate(filter);
        
        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }
}

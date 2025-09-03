using System.Linq.Expressions;
using AutoMapper;
using LinqKit;
using SubsTracker.BLL.Interfaces;
using SubsTracker.Domain.Exceptions;
using SubsTracker.Domain.Filter;
using SubsTracker.Domain.Interfaces;
using UserDto = SubsTracker.BLL.DTOs.User.UserDto;
using CreateUserDto = SubsTracker.BLL.DTOs.User.Create.CreateUserDto;
using UpdateUserDto = SubsTracker.BLL.DTOs.User.Update.UpdateUserDto;
using UserModel = SubsTracker.DAL.Models.User.User;

namespace SubsTracker.BLL.Services.User;

public class UserService(IRepository<UserModel> repository, IMapper mapper) 
    : Service<UserModel, UserDto, CreateUserDto, UpdateUserDto, UserFilter>(repository, mapper), IUserService
{
    public async Task<IEnumerable<UserDto>> GetAll(UserFilter? filter, CancellationToken cancellationToken)
    {
        var predicate = CreatePredicate(filter);
        
        var entities = await base.GetAll(predicate, cancellationToken);
        return entities;
    }
    
    private static Expression<Func<UserModel, bool>> CreatePredicate(UserFilter filter)
    {
        var predicate = PredicateBuilder.New<UserModel>(true);
        
        predicate = AddFilterCondition<UserModel>(
            predicate, 
            filter.FirstName, 
            user => user.FirstName.Contains(filter.FirstName!, StringComparison.OrdinalIgnoreCase)
        );
        
        predicate = AddFilterCondition<UserModel>(
            predicate, 
            filter.LastName, 
            user => user.LastName != null && user.LastName.Contains(filter.LastName!, StringComparison.OrdinalIgnoreCase)
        );
        
        predicate = AddFilterCondition<UserModel>(
            predicate, 
            filter.Email, 
            user => user.Email.Equals(filter.Email, StringComparison.OrdinalIgnoreCase)
        );

        return predicate;
    }
    
    public async Task<UserDto?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = await repository.GetByPredicate(u => u.Email == email, cancellationToken)
            ?? throw new NotFoundException($"User with email {email} not found");
        return mapper.Map<UserDto>(user);
    }
}

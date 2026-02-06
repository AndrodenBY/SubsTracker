using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.Mapper;

public class DtoMappingProfile : Profile
{
    public DtoMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserGroup, UserGroupDto>();
        CreateMap<GroupMember, GroupMemberDto>();
        CreateMap<Subscription, SubscriptionDto>();

        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Auth0Id, opt => opt.Ignore());
        CreateMap<CreateUserGroupDto, UserGroup>();
        CreateMap<CreateGroupMemberDto, GroupMember>();
        CreateMap<CreateSubscriptionDto, Subscription>();

        CreateMap<UpdateUserDto, User>();
        CreateMap<UpdateUserGroupDto, UserGroup>();
        CreateMap<UpdateGroupMemberDto, GroupMember>();
        CreateMap<UpdateSubscriptionDto, Subscription>();
    }
}

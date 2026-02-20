using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Entities.Subscription;
using SubsTracker.DAL.Entities.User;

namespace SubsTracker.BLL.Mapper;

public class DtoMappingProfile : Profile
{
    public DtoMappingProfile()
    {
        CreateMap<UserEntity, UserDto>();
        CreateMap<UserGroup, UserGroupDto>();
        CreateMap<GroupMember, GroupMemberDto>();
        CreateMap<SubscriptionEntity, SubscriptionDto>();

        CreateMap<CreateUserDto, UserEntity>();
        CreateMap<CreateUserGroupDto, UserGroup>();
        CreateMap<CreateGroupMemberDto, GroupMember>();
        CreateMap<CreateSubscriptionDto, SubscriptionEntity>();

        CreateMap<UpdateUserDto, UserEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Auth0Id, opt => opt.Ignore());
        CreateMap<UpdateUserGroupDto, UserGroup>();
        CreateMap<UpdateGroupMemberDto, GroupMember>();
        CreateMap<UpdateSubscriptionDto, SubscriptionEntity>();
    }
}

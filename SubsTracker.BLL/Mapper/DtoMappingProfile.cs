using AutoMapper;
using SubsTracker.BLL.DTOs;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.DAL.Models.Subscription;
using SubsTracker.DAL.Models.User;

namespace SubsTracker.BLL.Mapper;

public class DtoMappingProfile: Profile
{
    public DtoMappingProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserGroup, UserGroupDto>();
        CreateMap<GroupMember, GroupMemberDto>();
        CreateMap<Subscription, SubscriptionDto>();
    }
}
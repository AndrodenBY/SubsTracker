using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;

namespace SubsTracker.API.Mapper;

public class ViewModelMappingProfile : Profile
{
    public ViewModelMappingProfile()
    {
        CreateMap<UserDto, UserViewModel>();
        CreateMap<UserGroupDto, UserGroupViewModel>();
        CreateMap<GroupMemberDto, GroupMemberViewModel>();
        CreateMap<SubscriptionDto, SubscriptionViewModel>();
    }
}
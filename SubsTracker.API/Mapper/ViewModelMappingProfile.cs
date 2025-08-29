using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.API.ViewModels.Subscription;
using SubsTracker.API.ViewModels.User;

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

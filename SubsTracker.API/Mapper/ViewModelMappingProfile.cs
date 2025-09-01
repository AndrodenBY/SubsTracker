using AutoMapper;
using SubsTracker.API.ViewModel.Subscription;
using SubsTracker.API.ViewModel.User;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.DTOs.User;

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
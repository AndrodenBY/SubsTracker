using AutoMapper;
using SubsTracker.API.ViewModel;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.DTOs.User;

namespace SubsTracker.API.Mapper;

public class ViewModelMappingProfile : Profile
{
    public ViewModelMappingProfile()
    {
        CreateMap<UserDto, UserViewModel>();
        CreateMap<GroupDto, GroupViewModel>();
        CreateMap<MemberDto, MemberViewModel>();
        CreateMap<SubscriptionDto, SubscriptionViewModel>();
        CreateMap<UpdateSubscriptionDto, SubscriptionDto>();
    }
}

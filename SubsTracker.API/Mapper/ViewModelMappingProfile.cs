using AutoMapper;
using SubsTracker.API.ViewModel.Subscription;
using SubsTracker.API.ViewModel.User;
using SubsTracker.API.ViewModel.User.Create;
using SubsTracker.API.ViewModel.User.Update;
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
                
        CreateMap<CreateUserViewModel, UserDto>();
        CreateMap<CreateUserGroupViewModel, UserGroupDto>();
        CreateMap<CreateGroupMemberViewModel, GroupMemberDto>();
        CreateMap<CreateSubscriptionViewModel, SubscriptionDto>();

        CreateMap<UpdateUserViewModel, UserDto>();
        CreateMap<UpdateUserGroupViewModel, GroupMemberDto>();
        CreateMap<UpdateGroupMemberViewModel, GroupMemberDto>();
        CreateMap<UpdateSubscriptionDto, SubscriptionDto>();
    }
}
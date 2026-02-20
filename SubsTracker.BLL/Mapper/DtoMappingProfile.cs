using AutoMapper;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Entities;

namespace SubsTracker.BLL.Mapper;

public class DtoMappingProfile : Profile
{
    public DtoMappingProfile()
    {
        CreateMap<UserEntity, UserDto>();
        CreateMap<GroupEntity, GroupDto>();
        CreateMap<MemberEntity, MemberDto>();
        CreateMap<SubscriptionEntity, SubscriptionDto>();

        CreateMap<CreateUserDto, UserEntity>();
        CreateMap<CreateGroupDto, GroupEntity>();
        CreateMap<CreateMemberDto, MemberEntity>();
        CreateMap<CreateSubscriptionDto, SubscriptionEntity>();

        CreateMap<UpdateUserDto, UserEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Auth0Id, opt => opt.Ignore());
        CreateMap<UpdateGroupDto, GroupEntity>();
        CreateMap<UpdateMemberDto, MemberEntity>();
        CreateMap<UpdateSubscriptionDto, SubscriptionEntity>();
    }
}

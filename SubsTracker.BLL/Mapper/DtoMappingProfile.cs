using Mapster;
using SubsTracker.BLL.DTOs.Subscription;
using SubsTracker.BLL.DTOs.User;
using SubsTracker.BLL.DTOs.User.Create;
using SubsTracker.BLL.DTOs.User.Update;
using SubsTracker.DAL.Entities;

namespace SubsTracker.BLL.Mapper;

public static class DtoMappingProfile
{
    public static void Configure()
    { 
        TypeAdapterConfig<SubscriptionHistory, SubscriptionHistoryDto>.NewConfig()
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.SubscriptionName, src => src.Subscription!.Name)
            .Map(dest => dest.SubscriptionActive, src => src.Subscription!.Active)
            .Map(dest => dest.SubscriptionType, src => src.Subscription!.Type)
            .Map(dest => dest.SubscriptionContent, src => src.Subscription!.Content);
        
        TypeAdapterConfig<UpdateUserDto, UserEntity>.NewConfig()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.IdentityId);
    }
}

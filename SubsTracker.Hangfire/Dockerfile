FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 5158

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SubsTracker.Hangfire/SubsTracker.Hangfire.csproj", "SubsTracker.Hangfire/"]
COPY ["SubsTracker.BLL/SubsTracker.BLL.csproj", "SubsTracker.BLL/"]
COPY ["SubsTracker.DAL/SubsTracker.DAL.csproj", "SubsTracker.DAL/"]
COPY ["SubsTracker.Domain/SubsTracker.Domain.csproj", "SubsTracker.Domain/"]
COPY ["SubsTracker.Messaging/SubsTracker.Messaging.csproj", "SubsTracker.Messaging/"]
RUN dotnet restore "SubsTracker.Hangfire/SubsTracker.Hangfire.csproj"
COPY . .
WORKDIR "/src/SubsTracker.Hangfire"
RUN dotnet build "./SubsTracker.Hangfire.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SubsTracker.Hangfire.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubsTracker.Hangfire.dll"]

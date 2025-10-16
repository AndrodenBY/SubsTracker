FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SubsTracker.API/SubsTracker.API.csproj", "SubsTracker.API/"]
COPY ["SubsTracker.BLL/SubsTracker.BLL.csproj", "SubsTracker.BLL/"]
COPY ["SubsTracker.DAL/SubsTracker.DAL.csproj", "SubsTracker.DAL/"]
COPY ["SubsTracker.Domain/SubsTracker.Domain.csproj", "SubsTracker.Domain/"]
RUN dotnet restore "SubsTracker.API/SubsTracker.API.csproj"
COPY . .
WORKDIR "/src/SubsTracker.API"
RUN dotnet build "SubsTracker.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SubsTracker.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SubsTracker.API.dll"]

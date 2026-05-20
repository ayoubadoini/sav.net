# Build stage for .NET API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src

COPY ["SAV.API/SAV.API.csproj", "SAV.API/"]
COPY ["SAV.Application/SAV.Application.csproj", "SAV.Application/"]
COPY ["SAV.Infrastructure/SAV.Infrastructure.csproj", "SAV.Infrastructure/"]
COPY ["SAV.Domain/SAV.Domain.csproj", "SAV.Domain/"]
COPY ["SAV.Shared/SAV.Shared.csproj", "SAV.Shared/"]

RUN dotnet restore "SAV.API/SAV.API.csproj"

COPY . .
RUN dotnet build "SAV.API/SAV.API.csproj" -c Release -o /app/build

FROM api-build AS api-publish
RUN dotnet publish "SAV.API/SAV.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage for runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=api-publish /app/publish .

EXPOSE 80 443
ENTRYPOINT ["dotnet", "SAV.API.dll"]

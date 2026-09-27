FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["GymApi.csproj", "./"]
RUN dotnet restore "GymApi.csproj"

COPY . .
RUN dotnet publish "GymApi.csproj" --configuration Release --output /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GymApi.dll"]

# Build stage

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore the API project

COPY ["RaceDay.API.csproj", "./"]
RUN dotnet restore "RaceDay.API.csproj"

# Copy the remaining API source files

COPY . .

# Publish the API

RUN dotnet publish "RaceDay.API.csproj" -c Release -o /app/publish --no-restore

# Runtime stage

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.API.dll"]

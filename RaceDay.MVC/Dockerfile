# syntax=docker/dockerfile:1

# ---------- Build stage ----------
# Uses the full SDK image because we need the compiler/build tools - this stage
# is thrown away at the end, so its size doesn't matter.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the project file first and restore before copying the rest of the
# source. Docker caches each layer, so as long as RaceDay.MVC.csproj hasn't
# changed, a rebuild after editing a .cshtml or .cs file skips the NuGet
# restore entirely instead of redownloading every package.
COPY ["RaceDay.MVC/RaceDay.MVC.csproj", "RaceDay.MVC/"]
RUN dotnet restore "RaceDay.MVC/RaceDay.MVC.csproj"

# Now bring in the rest of the MVC project (Controllers, Views, wwwroot, etc.)
# and publish a Release build straight to /app/publish.
COPY ["RaceDay.MVC/", "RaceDay.MVC/"]
WORKDIR /src/RaceDay.MVC
RUN dotnet publish "RaceDay.MVC.csproj" -c Release -o /app/publish --no-restore

# ---------- Runtime stage ----------
# Much smaller image that only has what's needed to *run* an ASP.NET Core app,
# not build one - this is the image that actually ships.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Kestrel listens on 8080 inside the container. Map it to whatever host port
# you like with `-p <host-port>:8080` when running the container.
ENV ASPNETCORE_URLS=http://+:8080
ENV ApiSettings__BaseUrl=http://host.docker.internal:5170/
EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]

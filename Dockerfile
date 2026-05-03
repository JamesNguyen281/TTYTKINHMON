# =========================================================================
# TTYT Kinh Mon - ASP.NET Core 8 multi-stage Dockerfile
# Build:  docker build -t ttytkm:latest .
# Run:    docker run -p 5050:8080 ttytkm:latest
# =========================================================================

# ---- Stage 1: build & publish ------------------------------------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy solution + project files first for restore cache
COPY ["WebsiteCore/src/WebsiteCore.Data/WebsiteCore.Data.csproj", "WebsiteCore/src/WebsiteCore.Data/"]
COPY ["WebsiteCore/src/WebsiteCore.Business/WebsiteCore.Business.csproj", "WebsiteCore/src/WebsiteCore.Business/"]
COPY ["WebsiteCore/src/WebsiteCore.Web/WebsiteCore.Web.csproj", "WebsiteCore/src/WebsiteCore.Web/"]
COPY ["Directory.Packages.props", "./"]
COPY ["Directory.Build.props", "./"]
RUN dotnet restore "WebsiteCore/src/WebsiteCore.Web/WebsiteCore.Web.csproj"

# copy full sources and publish
COPY . .
WORKDIR "/src/WebsiteCore/src/WebsiteCore.Web"
RUN dotnet publish "WebsiteCore.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ---- Stage 2: runtime --------------------------------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebsiteCore.Web.dll"]

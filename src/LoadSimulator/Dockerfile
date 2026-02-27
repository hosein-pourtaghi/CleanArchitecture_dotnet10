FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
WORKDIR /app
EXPOSE 5001
ENV ASPNETCORE_URLS=http://+:5001

FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/LoadSimulator/LoadSimulator.csproj", "src/LoadSimulator/"]
RUN dotnet restore "src/LoadSimulator/LoadSimulator.csproj"

COPY . .
WORKDIR "/src/src/LoadSimulator"
RUN dotnet build "LoadSimulator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LoadSimulator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:5001/health || exit 1

ENTRYPOINT ["dotnet", "LoadSimulator.dll"]

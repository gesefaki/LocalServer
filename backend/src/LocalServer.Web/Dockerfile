FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5144
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY . .

RUN dotnet restore "LocalServer.Web/LocalServer.Web.csproj"

WORKDIR "/src/LocalServer.Web"

RUN dotnet build "LocalServer.Web.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

RUN dotnet publish "LocalServer.Web.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LocalServer.Web.dll"]
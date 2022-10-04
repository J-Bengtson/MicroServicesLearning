FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY ["GatewayService/GatewayService.csproj", "GatewayService/"]
COPY ["Core/Core.csproj", "Core/"]
RUN dotnet restore "GatewayService/GatewayService.csproj"
COPY . .
WORKDIR "/src/GatewayService"
RUN dotnet build "GatewayService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GatewayService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GatewayService.dll"]

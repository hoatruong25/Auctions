﻿FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AuctionService/AuctionService.csproj", "src/AuctionService/"]
COPY ["src/Contracts/Contracts.csproj", "src/Contracts/"]
RUN dotnet restore "src/AuctionService/AuctionService.csproj"
COPY . .
WORKDIR "/src/src/AuctionService"
RUN dotnet build "AuctionService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AuctionService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AuctionService.dll"]

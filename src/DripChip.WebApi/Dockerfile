﻿FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["DripChip.WebApi/DripChip.WebApi.csproj", "DripChip.WebApi/"]
RUN dotnet restore "DripChip.WebApi/DripChip.WebApi.csproj"
COPY . .
WORKDIR "/src/DripChip.WebApi"
RUN dotnet build "DripChip.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DripChip.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DripChip.WebApi.dll"]
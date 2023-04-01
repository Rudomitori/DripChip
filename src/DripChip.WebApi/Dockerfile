FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.17 AS base
RUN apk update && apk add curl # For health check
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ../ .
WORKDIR "/src/DripChip.WebApi"
RUN dotnet build "DripChip.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DripChip.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DripChip.WebApi.dll"]

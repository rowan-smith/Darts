FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src
COPY DartsApi/DartsApi.csproj DartsApi/
RUN dotnet restore DartsApi/DartsApi.csproj
COPY DartsApi/ DartsApi/
WORKDIR /src/DartsApi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DartsApi.dll"]

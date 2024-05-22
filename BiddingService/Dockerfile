# Brug den officielle dotnet SDK 8.0 alpine image til build-fasen
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Kopier alle filer fra BiddingService mappen til arbejdsdirektivet
COPY BiddingService/ .

# Specificer den eksakte løsning- eller projektfil til dotnet restore
RUN dotnet restore "BiddingServiceSolution.sln"

# Build og publish applikationen
RUN dotnet publish -c Release -o /app/published-app

# Brug det officielle dotnet ASP.NET 8.0 alpine image til runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Kopier den publish'ede applikation fra build image
COPY --from=build /app/published-app /app

# Kør applikationen
ENTRYPOINT ["dotnet", "BiddingService.dll"]

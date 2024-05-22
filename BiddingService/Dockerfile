# Use the official dotnet SDK 8.0 alpine image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /app

# Copy all files from the BiddingService folder to the working directory
COPY BiddingService/ .

# Specify the exact solution or project file for dotnet restore
RUN dotnet restore "BiddingService/BiddingService.csproj"

# Build and publish the application
RUN dotnet publish "BiddingService/BiddingService.csproj" -c Release -o /app/published-app

# Use the official dotnet ASP.NET 8.0 alpine image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Copy the published application from the build image
COPY --from=build /app/published-app /app

# Run the application
ENTRYPOINT ["dotnet", "BiddingService.dll"]

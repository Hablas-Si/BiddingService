# Use the .NET SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Set the working directory in the container
WORKDIR /app

# Copy the solution file
COPY biddingservice/BiddingServiceSolution.sln ./

# Copy all the project files
COPY biddingservice/BiddingService/ ./BiddingService/

# Restore dependencies
RUN dotnet restore ./BiddingService/BiddingService.csproj

# Publish the application
RUN dotnet publish ./BiddingService/BiddingService.csproj -c Release -o /app/published-app

# Use the .NET runtime image for the runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/published-app ./

# Define the entry point for the application
ENTRYPOINT ["dotnet", "BiddingService.dll"]

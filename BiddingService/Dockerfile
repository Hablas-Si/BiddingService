# Use the .NET SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Set the working directory in the container
WORKDIR /src

# Copy the .sln file and restore the project dependencies
COPY BiddingServiceSolution.sln ./
COPY BiddingService/*.csproj ./BiddingService/
RUN dotnet restore

# Copy the rest of the application code
COPY . .

# Build the application and publish the output to the /app/published-app directory
WORKDIR /src/BiddingService
RUN dotnet publish -c Release -o /app/published-app

# Use the .NET runtime image for the runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/published-app .

# Define the entry point for the application
ENTRYPOINT ["dotnet", "BiddingService.dll"]

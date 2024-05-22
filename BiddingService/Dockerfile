# Use the .NET SDK image for the build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build

# Set the working directory in the container
WORKDIR /app

# Copy the solution file and project file to the container
COPY BiddingService/BiddingServiceSolution.sln ./
COPY BiddingService/BiddingService/*.csproj ./BiddingService/

# Restore the project dependencies
RUN dotnet restore BiddingService/BiddingServiceSolution.sln

# Copy the rest of the application code
COPY BiddingService/ ./BiddingService/

# Build the application and publish the output to the /app/published-app directory
RUN dotnet publish BiddingService/BiddingServiceSolution.sln -c Release -o /app/published-app

# Use the .NET runtime image for the runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime

# Set the working directory in the container
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/published-app .

# Define the entry point for the application
ENTRYPOINT ["dotnet", "BiddingServ

# Use .NET SDK 8.0 image to build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the app and build it
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image using .NET 8.0
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose WebSocket and HTTP port
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "ServiceA.dll"]

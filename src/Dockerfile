# src/Dockerfile
# Use the official SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY src/redis-aspnet-core-sample.csproj .
RUN dotnet restore "./redis-aspnet-core-sample.csproj"

# Copy the rest of the source code and build the application
COPY src/* .
RUN dotnet build "redis-aspnet-core-sample.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "redis-aspnet-core-sample.csproj" -c Release -o /app/publish

# Use the official ASP.NET runtime image for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
EXPOSE 8081

# The entry point for the container, running the API
ENTRYPOINT ["dotnet", "redis-aspnet-core-sample.dll"]

# =========================
# Build stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies (better caching)
COPY VehicleTrackerApi/VehicleTrackerApi.csproj VehicleTrackerApi/
RUN dotnet restore VehicleTrackerApi/VehicleTrackerApi.csproj

# Copy everything else
COPY VehicleTrackerApi/ VehicleTrackerApi/

# Publish the app
RUN dotnet publish VehicleTrackerApi/VehicleTrackerApi.csproj \
    -c Release \
    -o /app

# =========================
# Runtime stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Tell ASP.NET Core to listen on port 8080 inside container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy published output from build stage
COPY --from=build /app .

# Start app
ENTRYPOINT ["dotnet", "VehicleTrackerApi.dll"]
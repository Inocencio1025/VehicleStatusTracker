FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj first (better caching)
COPY src/VehicleTrackerApi/VehicleTrackerApi.csproj VehicleTrackerApi/
RUN dotnet restore VehicleTrackerApi/VehicleTrackerApi.csproj

# copy everything
COPY src/VehicleTrackerApi/ VehicleTrackerApi/

WORKDIR /src/VehicleTrackerApi
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app .

ENTRYPOINT ["dotnet", "VehicleTrackerApi.dll"]
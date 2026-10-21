FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY backend-library/FoodDatabase.Library.csproj backend-library/
COPY backend-maintenance/FoodDatabase.Maintenance.csproj backend-maintenance/
RUN dotnet restore backend-maintenance/FoodDatabase.Maintenance.csproj

COPY . ./
RUN dotnet publish backend-maintenance/FoodDatabase.Maintenance.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
COPY --from=build /source/data /data
ENTRYPOINT ["dotnet", "FoodDatabase.Maintenance.dll"]
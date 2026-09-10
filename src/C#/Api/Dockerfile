FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY Food-Database.csproj ./
COPY Api/Food-Database.Api.csproj Api/
RUN dotnet restore Api/Food-Database.Api.csproj

COPY . ./
RUN dotnet publish Api/Food-Database.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Food-Database.Api.dll"]

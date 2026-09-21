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
ARG API_PORT=8080
ENV API_PORT=${API_PORT}
ENV ASPNETCORE_URLS=http://+:${API_PORT}
EXPOSE ${API_PORT}
ENTRYPOINT ["dotnet", "Food-Database.Api.dll"]

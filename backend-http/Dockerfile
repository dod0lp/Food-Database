FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

COPY backend-library/FoodDatabase.Library.csproj backend-library/
COPY backend-http/FoodDatabase.Http.csproj backend-http/
RUN dotnet restore backend-http/FoodDatabase.Http.csproj

COPY . ./
RUN dotnet publish backend-http/FoodDatabase.Http.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
ARG API_PORT=8080
ENV API_PORT=${API_PORT}
ENV ASPNETCORE_URLS=http://+:${API_PORT}
EXPOSE ${API_PORT}
ENTRYPOINT ["dotnet", "FoodDatabase.Http.dll"]
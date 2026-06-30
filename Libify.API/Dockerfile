# Build a partir da raiz da solution:  docker build -f Libify.API/Dockerfile -t libify-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore Libify.API/Libify.API.csproj
RUN dotnet publish Libify.API/Libify.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Libify.API.dll"]

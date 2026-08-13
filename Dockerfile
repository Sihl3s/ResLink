FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY backend/ResLink.Api/ResLink.Api.csproj backend/ResLink.Api/
RUN dotnet restore backend/ResLink.Api/ResLink.Api.csproj
COPY backend/ResLink.Api/ backend/ResLink.Api/
RUN dotnet publish backend/ResLink.Api/ResLink.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV RESLINK_DATA_DIR=/tmp/reslink
EXPOSE 8080
ENTRYPOINT ["dotnet", "ResLink.Api.dll"]

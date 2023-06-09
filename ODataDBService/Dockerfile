#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY ["ODataDBService.csproj", "ODataDBService/"]
RUN dotnet restore "ODataDBService/ODataDBService.csproj"
COPY . /src/ODataDBService/
WORKDIR "/src/ODataDBService"
RUN dotnet build "ODataDBService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ODataDBService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ODataDBService.dll"]
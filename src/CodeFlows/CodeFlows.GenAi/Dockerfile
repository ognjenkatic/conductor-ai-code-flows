#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

RUN mkdir /app/my-repos
RUN mkdir /app/global-repos

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CodeFlows.GenAi/CodeFlows.GenAi.csproj", "CodeFlows.GenAi/"]
RUN dotnet restore "./CodeFlows.GenAi/CodeFlows.GenAi.csproj"
COPY . .
WORKDIR "/src/CodeFlows.GenAi"
RUN dotnet build "./CodeFlows.GenAi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CodeFlows.GenAi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeFlows.GenAi.dll"]
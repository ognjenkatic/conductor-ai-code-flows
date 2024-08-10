#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base

RUN apt update
RUN apt install gh

USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CodeFlows.Workspace/CodeFlows.Workspace.csproj", "CodeFlows.Workspace/"]
RUN dotnet restore "./CodeFlows.Workspace/CodeFlows.Workspace.csproj"
COPY . .
WORKDIR "/src/CodeFlows.Workspace"
RUN dotnet build "./CodeFlows.Workspace.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CodeFlows.Workspace.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CodeFlows.Workspace.dll"]
#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER app
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Codeflows.WorkflowDeployer/Codeflows.WorkflowDeployer.csproj", "Codeflows.WorkflowDeployer/"]
COPY ["Codeflows.Csharp/Codeflows.Csharp.csproj", "Codeflows.Csharp/"]
COPY ["CodeFlows.GenAi/CodeFlows.GenAi.csproj", "CodeFlows.GenAi/"]
COPY ["CodeFlows.Workspace/CodeFlows.Workspace.csproj", "CodeFlows.Workspace/"]
RUN dotnet restore "./Codeflows.WorkflowDeployer/Codeflows.WorkflowDeployer.csproj"
COPY . .
WORKDIR "/src/Codeflows.WorkflowDeployer"
RUN dotnet build "./Codeflows.WorkflowDeployer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Codeflows.WorkflowDeployer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Codeflows.WorkflowDeployer.dll"]
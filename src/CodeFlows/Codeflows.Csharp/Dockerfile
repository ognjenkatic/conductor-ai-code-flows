#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS base
USER app
WORKDIR /app

RUN mkdir /app/my-repos
RUN mkdir /app/global-repos

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Codeflows.Csharp/Codeflows.Csharp.csproj", "Codeflows.Csharp/"]
RUN dotnet restore "./Codeflows.Csharp/Codeflows.Csharp.csproj"
COPY . .
WORKDIR "/src/Codeflows.Csharp"
RUN dotnet build "./Codeflows.Csharp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Codeflows.Csharp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Codeflows.Csharp.dll"]
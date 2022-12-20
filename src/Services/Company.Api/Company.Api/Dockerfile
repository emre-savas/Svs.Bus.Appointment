#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Services/Company.Api/Company.Api/Company.Api.csproj", "src/Services/Company.Api/Company.Api/"]
RUN dotnet restore "src/Services/Company.Api/Company.Api/Company.Api.csproj"
COPY . .
WORKDIR "/src/src/Services/Company.Api/Company.Api"
RUN dotnet build "Company.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Company.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Company.Api.dll"]
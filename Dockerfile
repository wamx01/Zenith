FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["MundoVs/MundoVs.csproj", "MundoVs/"]
COPY ["Zenith.Contracts/Zenith.Contracts.csproj", "Zenith.Contracts/"]
RUN dotnet restore "MundoVs/MundoVs.csproj"

COPY MundoVs/. MundoVs/
COPY Zenith.Contracts/. Zenith.Contracts/

WORKDIR /src/MundoVs
RUN dotnet publish "MundoVs.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads

ENV ASPNETCORE_URLS=http://+:5130
EXPOSE 5130

ENTRYPOINT ["dotnet", "MundoVs.dll"]

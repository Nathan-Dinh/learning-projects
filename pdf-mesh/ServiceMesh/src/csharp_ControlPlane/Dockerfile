# Stage 1 - build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ControlPlane.csproj", "./"]
RUN dotnet restore "ControlPlane.csproj"

COPY . .
RUN dotnet publish "ControlPlane.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ControlPlane.dll"]

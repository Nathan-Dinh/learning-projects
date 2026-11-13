# Stage 1 - build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ServiceC.csproj", "./"]
RUN dotnet restore "ServiceC.csproj"

COPY . .
RUN dotnet publish "ServiceC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2 - runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ServiceC.dll"]

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy project file and restore dependencies
COPY *.csproj .
RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o out

# Runtime stage (smaller image, no SDK)
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "TelegramMenuBot.dll"]
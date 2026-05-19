# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for dependency restore (allows caching)
COPY ["CADSFINANCE.sln", "./"]
COPY ["CADSFINANCE/CADSFINANCE.csproj", "CADSFINANCE/"]
RUN dotnet restore

# Copy the entire source code and build/publish the application
COPY . .
WORKDIR "/src/CADSFINANCE"
RUN dotnet publish "CADSFINANCE.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

# Configure ASP.NET Core to listen on port 8080 (standard non-root port in .NET 8)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Create a non-root user and switch to it for better security in Kubernetes
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CADSFINANCE.dll"]

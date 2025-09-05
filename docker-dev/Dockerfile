# Development Dockerfile for HRM backend
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

# Install PostgreSQL client for database connectivity
RUN apt-get update && apt-get install -y postgresql-client && rm -rf /var/lib/apt/lists/*

# Copy published backend files
COPY ../backend/bin/Debug/net9.0/ ./

# Expose default ASP.NET port
EXPOSE 5000

# Set environment variables for development
ENV ASPNETCORE_ENVIRONMENT=Development

# Start the backend
ENTRYPOINT ["dotnet", "backend.dll"]

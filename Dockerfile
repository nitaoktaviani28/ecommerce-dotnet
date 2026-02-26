# =========================
# BUILD STAGE
# =========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj & restore
COPY Ecommerce.MonitoringApp.csproj ./
RUN dotnet restore

# copy source
COPY . ./

# publish (self-contained = false, cocok AKS)
RUN dotnet publish -c Release -o /app/publish

# =========================
# RUNTIME STAGE
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# cert & tz (opsional tapi aman)
RUN apt-get update && apt-get install -y ca-certificates tzdata && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# =========================
# ENV (sama seperti Go)
# =========================
ENV ASPNETCORE_URLS=http://+:8080
ENV OTEL_SERVICE_NAME=ecommerce-backend
ENV OTEL_EXPORTER_OTLP_ENDPOINT=http://alloy.monitoring.svc.cluster.local:4318
ENV DATABASE_DSN=postgres://user:pass@postgres:5432/shop?sslmode=disable

EXPOSE 8080
ENTRYPOINT ["dotnet", "Ecommerce.MonitoringApp.dll"]

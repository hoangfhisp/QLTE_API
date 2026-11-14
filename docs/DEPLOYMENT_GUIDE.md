# Production Deployment and Infrastructure Guide

## 1. Prerequisites and Infrastructure Stack
To deploy the QLTE backend in a production environment, ensure the following infrastructure dependencies are provisioned:
- **Runtime**: .NET 8.0 Hosting Bundle or .NET 8 SDK
- **Database Engine**: Microsoft SQL Server 2019+ (Express, Standard, or Enterprise)
- **Reverse Proxy**: Nginx, Cloudflare, or Microsoft IIS
- **Security Certificates**: TLS 1.3 / SSL Certificate (Let's Encrypt or Enterprise CA)
- **Email Service**: SMTP server with TLS support for transactional mail dispatch

---

## 2. Environment Configuration

Copy the example environment configuration and adapt values for the production cluster:

```bash
cp .env.example .env
```

Ensure production values are set for:
1. `ConnectionStrings__AuthDb`, `ConnectionStrings__UserDb`, `ConnectionStrings__ChildDb`: Pointing to production SQL instances with encrypted connection parameters.
2. `AuthOptions__Jwt__SecretKey`: Generate a 64+ character cryptographically secure random string.
3. `EmailSettings__*`: Configure operational SMTP credentials.

---

## 3. Database Migration Execution

Before running the application binary, apply Entity Framework Core migrations for all bounded contexts:

```bash
# Navigate to the backend project root
cd Backend_QLTE

# Update UserDbContext
dotnet ef database update --context UserDbContext

# Update ChildDbContext
dotnet ef database update --context ChildDbContext
```

---

## 4. Production Build and Compilation

### 4.1 Release Publication via .NET CLI
Compile a self-contained or framework-dependent release binary:

```bash
dotnet publish Backend_QLTE/Backend_QLTE.csproj \
  -c Release \
  -o ./publish \
  --runtime linux-x64 \
  --no-self-contained
```

---

## 5. Containerized Deployment (Docker & Compose)

### 5.1 Dockerfile Specification
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Backend_QLTE/Backend_QLTE.csproj", "Backend_QLTE/"]
COPY ["Shared.FileStorage/Shared.FileStorage.csproj", "Shared.FileStorage/"]
RUN dotnet restore "Backend_QLTE/Backend_QLTE.csproj"
COPY . .
WORKDIR "/src/Backend_QLTE"
RUN dotnet build "Backend_QLTE.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Backend_QLTE.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Backend_QLTE.dll"]
```

### 5.2 Docker Compose Configuration
```yaml
version: '3.8'

services:
  qlte-api:
    image: qlte-api:latest
    build:
      context: .
      dockerfile: Dockerfile
    restart: always
    ports:
      - "5000:5000"
      - "5001:5001"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
    env_file:
      - .env
    volumes:
      - qlte_uploads:/app/wwwroot/uploads
    depends_on:
      - sqlserver

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    restart: always
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=YourStrongPassword123!
    ports:
      - "1433:1433"
    volumes:
      - sql_data:/var/opt/mssql

volumes:
  qlte_uploads:
  sql_data:
```

---

## 6. Reverse Proxy Configuration (Nginx)

Nginx must be configured to support WebSocket upgrades for SignalR traffic:

```nginx
server {
    listen 80;
    server_name api.qlte-platform.local;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.qlte-platform.local;

    ssl_certificate /etc/letsencrypt/live/api.qlte-platform.local/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.qlte-platform.local/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $http_connection;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }

    # Dedicated WebSocket timeout extension for SignalR Hubs
    location ~* /(chatHub|locationHub|deviceHub|sosHub|studyHub|restrictionHub) {
        proxy_pass http://127.0.0.1:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection "upgrade";
        proxy_set_header Host $host;
        proxy_read_timeout 86400s;
        proxy_send_timeout 86400s;
    }
}
```

---

## 7. Health Verification and Monitoring
- **Health Check Endpoint**: Test endpoint availability using HTTP GET against `/api/Auth/GetAll` or Swagger root `/swagger/index.html`.
- **Log Inspection**: In production, review ASP.NET Core system logs directed to console standard output or structured log collectors.

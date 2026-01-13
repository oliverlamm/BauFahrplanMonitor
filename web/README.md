# ToDo
- Zähler für erfolgreichen Importiert / Fehler
- ZvFExport Importer
  - ÜB Import
  - Fplo Importl
- NetzfahrplanImporter
- BBPNeo Importer
- Betriebsstellenverwaltung

## Node.js installieren

Node.js & npm installieren (Linux / CachyOS / Arch)
Auf Arch-basierten Systemen (CachyOS):
```bash
sudo pacman -S nodejs npm
```

In dein Webfrontend-Projekt wechseln
Beispiel (bei dir evtl. BauFahrplanMonitor.Web oder frontend):
```bash
cd ~/RiderProjects/BauFahrplanMonitor/web
```

Falls noch kein package.json existiert:
```bash
npm init -y
```
Falls das Projekt neu ist:
```bash
npm create vite@latest bau-fahrplan-monitor-web
```
Dann auswählen:
Framework: React
Variant: TypeScript

Danach:
```bash
cd bau-fahrplan-monitor-web
npm install
```

Typische Zusatzpakete (wie wir sie benutzt haben)
```bash
npm install react-router-dom
npm install clsx
npm install axios
npm install leaflet react-leaflet
```

Entwicklungsserver starten
```bash
npm run dev
```

Build für Produktion
```bash
npm run build
```

Ergebnis:
```env
dist/
```

Das kannst du:
- direkt von ASP.NET aus serven
- oder per nginx / static files ausliefern

## 🐳 Docker Setup

Diese Anleitung beschreibt, wie der **BauFahrplanMonitor** lokal mit Docker gestartet wird.  
Es werden folgende Komponenten verwendet:

- **Backend:** ASP.NET Core API
- **Frontend:** React (Vite)
- **Datenbank:** PostgreSQL mit PostGIS

Alle Dienste werden gemeinsam über `docker compose` gestartet.

---

### Voraussetzungen

Installierte Software:

- Docker
- Docker Compose (Plugin)

Versionen prüfen:

```bash
docker --version
docker compose version
```

### Struktur
BauFahrplanMonitor/
├── docker-compose.yml
├── .env
├── BauFahrplanMonitor.Api/
│   └── Dockerfile
├── web/
│   └── Dockerfile
└── README.md

### Backend – Dockerfile

Datei: BauFahrplanMonitor.Api/Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080

ENTRYPOINT ["dotnet", "BauFahrplanMonitor.Api.dll"]
```

### Frontend – Dockerfile

Datei: web/Dockerfile
```dockerfile
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

### Umgebungsvariablen

Datei: .env
```env
POSTGRES_DB=ujbaudb
POSTGRES_USER=infrago
POSTGRES_PASSWORD=infrago
```

### Docker Compose

Datei: docker-compose.yml
```yaml
version: "3.9"

services:
  db:
    image: postgis/postgis:16-3.4
    container_name: bfm-postgis
    restart: unless-stopped
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    ports:
      - "5432:5432"
    volumes:
      - postgis-data:/var/lib/postgresql/data

  api:
    build:
      context: ./BauFahrplanMonitor.Api
    container_name: bfm-api
    depends_on:
      - db
    environment:
      ASPNETCORE_URLS: http://+:8080
      ConnectionStrings__Default: >
        Host=db;
        Database=${POSTGRES_DB};
        Username=${POSTGRES_USER};
        Password=${POSTGRES_PASSWORD}
    ports:
      - "8080:8080"

  web:
    build:
      context: ./web
    container_name: bfm-web
    depends_on:
      - api
    ports:
      - "5173:80"

volumes:
  postgis-data:
```

Alle Dienste bauen und starten:
```bash
docker compose up --build
```

Im Hintergrund
```bash
docker compose up -d --build
```
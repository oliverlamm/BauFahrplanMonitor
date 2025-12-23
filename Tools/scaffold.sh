#!/usr/bin/env bash

# ===========================================
#   EF Core Scaffold für BauFahrplanMonitor (Linux)
# ===========================================

# Fehler sofort anzeigen
set -e

echo "==========================================="
echo "   EF Core Scaffold für BauFahrplanMonitor"
echo "==========================================="
echo

# ================================
#   PARAMETER
# ================================
DB_SERVER="192.168.1.24"
DB_PORT="5433"
DB_NAME="ujbaudb"
DB_USER="infrago"
DB_PASS="infrago"

echo "Verwende:"
echo "  Server:   $DB_SERVER"
echo "  Port:     $DB_PORT"
echo "  DB Name:  $DB_NAME"
echo

# ================================
#   Verzeichnis wechseln
# ================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR/.." || exit 1

echo "Projekt: $(pwd)"
echo

# ================================
#   Scaffolding starten
# ================================

CONNECTION="Host=$DB_SERVER;Port=$DB_PORT;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASS"

echo "Starte Scaffold..."
echo

if dotnet ef dbcontext scaffold \
    "$CONNECTION" \
    Npgsql.EntityFrameworkCore.PostgreSQL \
    --schema ujbaudb \
    --context UjBauDbContext \
    --context-dir Data \
    --output-dir Models \
    --no-onconfiguring \
    --no-pluralize \
    --force \
    --data-annotations \
    --nullable \
    --verbose; then

    echo
    echo "==========================================="
    echo "  ✔ Scaffolding erfolgreich abgeschlossen"
    echo "==========================================="
else
    CODE=$?
    echo
    echo "==========================================="
    echo "  ❌ Fehler beim Scaffolding (Code $CODE)"
    echo "==========================================="
    exit $CODE
fi

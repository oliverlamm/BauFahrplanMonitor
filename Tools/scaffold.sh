#!/usr/bin/env bash

# ===========================================
#   EF Core Scaffold für BauFahrplanMonitor
# ===========================================

set -euo pipefail

# -------------------------------
# PATH absichern (fish / CI)
# -------------------------------
export PATH="$HOME/.dotnet/tools:$PATH"

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

# ================================
#   Projektpfade
# ================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
PROJECT_FILE="$PROJECT_ROOT/BauFahrplanMonitor.Core/BauFahrplanMonitor.Core.csproj"

echo "Verwende:"
echo "  Server:   $DB_SERVER"
echo "  Port:     $DB_PORT"
echo "  DB Name:  $DB_NAME"
echo
echo "Projekt:"
echo "  Root:     $PROJECT_ROOT"
echo "  CSProj:   $PROJECT_FILE"
echo

# ================================
#   Validierungen
# ================================
if [[ ! -f "$PROJECT_FILE" ]]; then
    echo "❌ Projektdatei nicht gefunden:"
    echo "   $PROJECT_FILE"
    exit 1
fi

# ================================
#   Restore
# ================================
echo "🔄 Restore Projekt..."
dotnet restore "$PROJECT_FILE"
echo

# ================================
#   Connection
# ================================
CONNECTION="Host=$DB_SERVER;Port=$DB_PORT;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASS"

# ================================
#   Scaffolding
# ================================
echo "🚀 Starte Scaffold..."
echo

dotnet ef dbcontext scaffold \
    "$CONNECTION" \
    Npgsql.EntityFrameworkCore.PostgreSQL \
    --project "$PROJECT_FILE" \
    --startup-project "$PROJECT_FILE" \
    --framework net8.0 \
    --schema ujbaudb \
    --context UjBauDbContext \
    --context-dir Data \
    --output-dir Models \
    --no-onconfiguring \
    --no-pluralize \
    --force \
    --data-annotations \
    --nullable \
    --verbose

echo
echo "==========================================="
echo "  ✔ Scaffolding erfolgreich abgeschlossen"
echo "==========================================="

#!/bin/bash
set -e

# ============================================================
#  Zwei-Wege PostgreSQL/PostGIS Sync
#  Stromeferry  <-->  Banavie  <-->  Morar
# ============================================================

DB_NAME="ujbaudb"
SCHEMA_NAME="ujbaudb"
DB_USER="infrago"
DB_PASS="infrago"

# --- Hosts / Ports ------------------------------------------------------------
STROM_HOST="127.0.0.1"
STROM_PORT="5432"

BAN_HOST="192.168.1.24"
BAN_PORT="5433"

MORAR_HOST="192.168.1.6"
MORAR_PORT="5433"

# --- Timestamp für Logs / Dumps ------------------------------------------------
TS=$(date +"%Y%m%d_%H%M%S")

# --- Helper -------------------------------------------------------------------
function do_dump() {
  local HOST=$1
  local PORT=$2
  local TARGET=$3

  echo "  → Dump von $TARGET"
  PGPASSWORD=$DB_PASS pg_dump \
    -h $HOST -p $PORT -U $DB_USER \
    -d $DB_NAME \
    -Fc -n '"'"$SCHEMA_NAME"'"' \
    -f ${TARGET}_$TS.dump
}

function do_restore() {
  local HOST=$1
  local PORT=$2
  local SOURCE=$3

  echo "  → Restore nach $SOURCE"

  # ALWAYS connect to postgres, not infrago!
  PGPASSWORD=$DB_PASS psql -h $HOST -p $PORT -U $DB_USER -d postgres \
      -c "DROP DATABASE IF EXISTS \"$DB_NAME\";"

  PGPASSWORD=$DB_PASS psql -h $HOST -p $PORT -U $DB_USER -d postgres \
      -c "CREATE DATABASE \"$DB_NAME\";"

  # PostGIS
  PGPASSWORD=$DB_PASS psql -h $HOST -p $PORT -U $DB_USER -d $DB_NAME \
      -c "CREATE EXTENSION IF NOT EXISTS postgis;"

  # Restore ohne Ownership/Privilege Statements
  PGPASSWORD=$DB_PASS pg_restore \
     -h $HOST -p $PORT -U $DB_USER \
     -d $DB_NAME \
     --clean --if-exists \
     --no-owner --no-privileges \
     ${SOURCE}_$TS.dump
}

# --- Main Dispatcher -----------------------------------------------------------
echo "===================================================="
echo "               PostgreSQL Sync Tool"
echo "===================================================="
echo "Zeit: $TS"
echo

MODE=$1

case "$MODE" in

  stromeferry-to-banavie)
    echo "### Stromeferry  → Banavie"
    do_dump    $STROM_HOST $STROM_PORT "stromeferry"
    do_restore $BAN_HOST   $BAN_PORT   "stromeferry"
    ;;

  banavie-to-stromeferry)
    echo "### Banavie  → Stromeferry"
    do_dump    $BAN_HOST   $BAN_PORT   "banavie"
    do_restore $STROM_HOST $STROM_PORT "banavie"
    ;;

  morar-to-stromeferry)
    echo "### Morar  → Stromeferry"
    do_dump    $MORAR_HOST $MORAR_PORT "morar"
    do_restore $STROM_HOST $STROM_PORT "morar"
    ;;

  stromeferry-to-morar)
    echo "### Stromeferry  → Morar"
    do_dump    $STROM_HOST $STROM_PORT "stromeferry"
    do_restore $MORAR_HOST $MORAR_PORT "stromeferry"
    ;;

  *)
    echo "⚠️ Benutzung:"
    echo "  ./dbsync.sh stromeferry-to-banavie"
    echo "  ./dbsync.sh banavie-to-stromeferry"
    echo "  ./dbsync.sh morar-to-stromeferry"
    echo "  ./dbsync.sh stromeferry-to-morar"
    exit 1
    ;;
esac

echo
echo "===================================================="
echo "                Vorgang abgeschlossen"
echo "===================================================="

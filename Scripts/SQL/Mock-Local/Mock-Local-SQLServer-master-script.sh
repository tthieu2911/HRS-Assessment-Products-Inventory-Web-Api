#!/bin/bash
# ========================================================
# SQL Server Local Master Setup Script
# ========================================================

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

DB_CREATION_SCRIPT="$SCRIPT_DIR/Docker/SQLServer-local-setup.sh"
DB_TABLE_CREATION_SCRIPT="$SCRIPT_DIR/SQLServer-Products-Table-Create.sh"
DB_MONITOR_SCRIPT="$SCRIPT_DIR/SQLServer-Local-Monitoring.sh"

run_step() {
  local step_name="$1"
  local script_path="$2"

  echo "------------------------------------------------------------"
  echo "▶️  Starting step: $step_name"
  echo "------------------------------------------------------------"

  if [ ! -f "$script_path" ]; then
    echo "❌ Error: Script not found: $script_path"
    exit 1
  fi

  bash "$script_path"
  echo "✅ Completed: $step_name"
  echo ""
}

run_step "1️⃣  SQL Server Container Creation" "$DB_CREATION_SCRIPT"
run_step "2️⃣  Connection Check & Table Creation" "$DB_TABLE_CREATION_SCRIPT"
run_step "3️⃣  Database Monitoring (Adminer)" "$DB_MONITOR_SCRIPT"

echo "============================================================"
echo "🎉 All SQL Server setup steps completed successfully!"
echo "============================================================"

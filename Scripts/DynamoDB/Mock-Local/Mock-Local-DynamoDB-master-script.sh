#!/bin/bash
# ========================================================
# DynamoDB Local Master Setup Script
# ========================================================

set -e
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

DB_CREATION_SCRIPT="$SCRIPT_DIR/Docker/DynamoDB-local-setup.sh"
DB_TABLE_CREATION_SCRIPT="$SCRIPT_DIR/DynamoDB-Products-Table-Create.sh"
DB_MONITOR_SCRIPT="$SCRIPT_DIR/DynamoDB-Local-Monitoring.sh"

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

run_step "1️⃣  Database Creation" "$DB_CREATION_SCRIPT"
run_step "2️⃣  Table Creation" "$DB_TABLE_CREATION_SCRIPT"
run_step "3️⃣  Database Monitoring (DynamoDB Admin UI)" "$DB_MONITOR_SCRIPT"

echo "============================================================"
echo "🎉 All DynamoDB setup steps completed successfully!"
echo "============================================================"

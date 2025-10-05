#!/bin/bash
# ========================================================
# SQL Server Local Setup Script
# ========================================================

CONTAINER_NAME="sqlserver"
SA_PASSWORD="YourStrong!Passw0rd"
IMAGE="mcr.microsoft.com/mssql/server:2022-latest"

echo "🟢 Starting SQL Server container..."

# Remove old container if exists
if [ "$(docker ps -aq -f name=$CONTAINER_NAME)" ]; then
  echo "   → Removing existing container: $CONTAINER_NAME"
  docker rm -f $CONTAINER_NAME >/dev/null 2>&1
fi

# Start new SQL Server container
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=$SA_PASSWORD" \
   -p 1433:1433 --name $CONTAINER_NAME \
   -d $IMAGE >/dev/null

# Wait until SQL Server is reachable
echo "⏳ Waiting for SQL Server to become available on port 1433..."
RETRIES=20
for i in $(seq 1 $RETRIES); do
  if nc -z localhost 1433; then
    echo "✅ SQL Server is ready at host.docker.internal:1433"
    exit 0
  fi
  echo "   ...attempt $i/$RETRIES: not ready yet"
  sleep 3
done

echo "❌ SQL Server did not become available after $RETRIES attempts."
exit 1

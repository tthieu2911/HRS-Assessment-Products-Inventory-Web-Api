#!/bin/bash
# ========================================================
# DynamoDB Local Setup Script
# ========================================================

CONTAINER_NAME="dynamodb-local"

echo "🟢 Starting DynamoDB Local container..."

# Remove any old container
if [ "$(docker ps -aq -f name=$CONTAINER_NAME)" ]; then
  echo "   → Removing existing container: $CONTAINER_NAME"
  docker rm -f $CONTAINER_NAME >/dev/null 2>&1
fi

# Start new DynamoDB Local container in background
docker run -d -p 8000:8000 --name $CONTAINER_NAME amazon/dynamodb-local -jar DynamoDBLocal.jar -sharedDb >/dev/null

# Wait until DynamoDB is reachable
echo "⏳ Waiting for DynamoDB Local to become available on port 8000..."
RETRIES=15
for i in $(seq 1 $RETRIES); do
  if nc -z localhost 8000; then
    echo "✅ DynamoDB Local is ready at http://localhost:8000"
    exit 0
  fi
  echo "   ...attempt $i/$RETRIES: not ready yet"
  sleep 2
done

echo "❌ DynamoDB Local did not become available after $RETRIES attempts."
exit 1

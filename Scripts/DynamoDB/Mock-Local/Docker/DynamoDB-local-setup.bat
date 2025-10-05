#!/bin/bash

# Run DynamoDB Local in detached mode (background)
echo "🟢 Starting DynamoDB Local..."
if [ "$(docker ps -aq -f name=dynamodb-local)" ]; then
  echo "   → Container 'dynamodb-local' already exists. Restarting..."
  docker rm -f dynamodb-local >/dev/null 2>&1
fi

docker run -d -p 8000:8000 --name dynamodb-local amazon/dynamodb-local -jar DynamoDBLocal.jar -sharedDb

# Wait a few seconds to ensure DynamoDB Local is ready
echo "⏳ Waiting for DynamoDB Local to initialize..."
sleep 5
echo "✅ DynamoDB Local is running on http://localhost:8000"

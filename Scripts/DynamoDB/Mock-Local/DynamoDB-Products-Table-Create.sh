#!/bin/bash
set -e

# === CONFIGURATION ===
export AWS_REGION=ap-southeast-1
export AWS_ACCESS_KEY_ID=dummy
export AWS_SECRET_ACCESS_KEY=dummy
DYNAMO_ENDPOINT="http://localhost:8000"
TABLE_NAME="DevSchema_Products"

echo "------------------------------------------------------------"
echo "📦 Creating DynamoDB table: $TABLE_NAME"
echo "------------------------------------------------------------"

# Check if the table already exists
if aws dynamodb describe-table --table-name "$TABLE_NAME" --endpoint-url "$DYNAMO_ENDPOINT" >/dev/null 2>&1; then
  echo "ℹ️  Table '$TABLE_NAME' already exists — skipping creation."
else
  aws dynamodb create-table \
      --table-name "$TABLE_NAME" \
      --attribute-definitions \
          AttributeName=InstitutionCode,AttributeType=S \
          AttributeName=ProductId,AttributeType=S \
      --key-schema \
          AttributeName=InstitutionCode,KeyType=HASH \
          AttributeName=ProductId,KeyType=RANGE \
      --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
      --endpoint-url "$DYNAMO_ENDPOINT"

  echo "✅ Table '$TABLE_NAME' created successfully."
fi

echo "------------------------------------------------------------"
echo "📋 Listing all tables for verification..."
aws dynamodb list-tables --endpoint-url "$DYNAMO_ENDPOINT"
echo "------------------------------------------------------------"

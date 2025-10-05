set AWS_REGION=ap-southeast-1
set AWS_ACCESS_KEY_ID=dummy
set AWS_SECRET_ACCESS_KEY=dummy

aws dynamodb create-table \
    --table-name Products \
    --attribute-definitions \
        AttributeName=InstitutionCode,AttributeType=S \
        AttributeName=ProductId,AttributeType=S \
    --key-schema \
        AttributeName=InstitutionCode,KeyType=HASH \
        AttributeName=ProductId,KeyType=RANGE \
    --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5 \
    --endpoint-url http://localhost:8000

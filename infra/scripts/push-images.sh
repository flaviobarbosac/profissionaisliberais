#!/usr/bin/env bash
# Deploy Libify no EC2 — executar na maquina local apos terraform apply
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
AWS_REGION="${AWS_REGION:-sa-east-1}"
AWS_PROFILE="${AWS_PROFILE:-libify-dev}"
PROJECT="${PROJECT:-libify}"
ENV="${ENVIRONMENT:-dev}"

echo "=== Build e push imagens ECR ==="
ACCOUNT=$(aws sts get-caller-identity --query Account --output text --profile "$AWS_PROFILE")
ECR_API="${ACCOUNT}.dkr.ecr.${AWS_REGION}.amazonaws.com/${PROJECT}-${ENV}-api"
ECR_WORKER="${ACCOUNT}.dkr.ecr.${AWS_REGION}.amazonaws.com/${PROJECT}-${ENV}-worker"

aws ecr get-login-password --region "$AWS_REGION" --profile "$AWS_PROFILE" | \
  docker login --username AWS --password-stdin "${ACCOUNT}.dkr.ecr.${AWS_REGION}.amazonaws.com"

cd "$REPO_ROOT"
docker build -f Libify.API/Dockerfile -t libify-api:latest .
docker build -f Libify.Worker/Dockerfile -t libify-worker:latest .
docker tag libify-api:latest "${ECR_API}:latest"
docker tag libify-worker:latest "${ECR_WORKER}:latest"
docker push "${ECR_API}:latest"
docker push "${ECR_WORKER}:latest"

echo "=== Imagens publicadas ==="
echo "API:    ${ECR_API}:latest"
echo "Worker: ${ECR_WORKER}:latest"

#!/usr/bin/env bash
# Build e push de todas as imagens .NET para o ECR.
# Executar a partir da raiz de devopsproject-ecommerce/.
set -euo pipefail

ACCOUNT_ID="692430448478"
REGION="us-east-1"
TAG="${1:-latest}"
ECR_BASE="${ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com/devopsproject/prod"
CONTEXT="$(cd "$(dirname "$0")" && pwd)"

echo "==> Login no ECR"
aws ecr get-login-password --region "$REGION" | \
  docker login --username AWS --password-stdin "${ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com"

declare -A SERVICES=(
  [main]="src/services/DevOpsProjectEcommerce.Main/Dockerfile"
  [order]="src/services/DevOpsProjectEcommerce.Order/Dockerfile"
  [identity-server]="src/services/DevOpsProjectEcommerce.IdentityServer/Dockerfile"
  [health-checker]="src/services/DevOpsProjectEcommerce.HealthChecker/Dockerfile"
  [notificator]="src/workers/DevOpsProjectEcommerce.Notificator/Dockerfile"
  [invoice-generator]="src/workers/DevOpsProjectEcommerce.InvoiceGenerator/Dockerfile"
)

for SVC in "${!SERVICES[@]}"; do
  DOCKERFILE="${CONTEXT}/${SERVICES[$SVC]}"
  URI="${ECR_BASE}/${SVC}:${TAG}"

  echo ""
  echo "==> [$SVC] Build: $URI"
  docker build \
    --file "$DOCKERFILE" \
    --tag  "$URI" \
    "$CONTEXT"

  echo "==> [$SVC] Push: $URI"
  docker push "$URI"

  if [ "$TAG" != "latest" ]; then
    LATEST_URI="${ECR_BASE}/${SVC}:latest"
    docker tag  "$URI" "$LATEST_URI"
    docker push "$LATEST_URI"
    echo "==> [$SVC] Push (latest): $LATEST_URI"
  fi
done

echo ""
echo "==> Concluído. Imagens disponíveis no ECR com tag: ${TAG}"

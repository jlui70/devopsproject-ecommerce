#!/usr/bin/env bash
# Build e push de todas as imagens .NET para o ECR.
# Executar a partir da raiz de devopsproject-ecommerce/.
#
# ADR-0020: os repositorios ECR sao IMMUTABLE (image_tag_mutability = "IMMUTABLE") -
# nenhuma tag pode ser sobrescrita depois de publicada, nem mesmo ":latest". Por isso
# este script NUNCA usa ":latest" como tag - o default e uma tag unica derivada de
# data/hora, e o script nao publica mais nenhuma tag secundaria fixa. Rodar o script
# de novo (ex.: apos corrigir um bug durante o bootstrap) simplesmente gera uma nova
# tag unica, sem colidir com nenhuma tag ja existente no ECR.
#
# Uso:
#   ./build-push-ecr.sh                 # tag automatica: bootstrap-YYYYMMDDHHMMSS
#   ./build-push-ecr.sh minha-tag-unica # tag explicita (deve ser unica - nunca reusar)
set -euo pipefail

ACCOUNT_ID="692430448478"
REGION="us-east-1"
TAG="${1:-bootstrap-$(date +%Y%m%d%H%M%S)}"
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
done

echo ""
echo "==> Concluído. Imagens disponíveis no ECR com tag: ${TAG}"
echo "==> Próximo passo: atualizar production/kustomization.yml (bloco images:) para"
echo "    newTag: ${TAG} nos 6 serviços, commitar e dar push no repo GitOps."

#!/bin/bash
set -euo pipefail
exec > /var/log/libify-deploy.log 2>&1

echo "=== Libify deploy ==="
REGION="${aws_region}"
BUCKET="${config_bucket}"

dnf install -y docker amazon-ssm-agent jq
systemctl enable --now docker amazon-ssm-agent

mkdir -p /usr/local/lib/docker/cli-plugins
curl -SL "https://github.com/docker/compose/releases/download/v2.32.4/docker-compose-linux-x86_64" \
  -o /usr/local/lib/docker/cli-plugins/docker-compose
chmod +x /usr/local/lib/docker/cli-plugins/docker-compose

mkdir -p /opt/libify
aws s3 cp "s3://$${BUCKET}/docker-compose.yml" /opt/libify/docker-compose.yml --region "$${REGION}"
aws s3 cp "s3://$${BUCKET}/Caddyfile" /opt/libify/Caddyfile --region "$${REGION}"

cd /opt/libify
ACCOUNT="${account_id}"
cat > .env << EOF
ECR_API=$${ACCOUNT}.dkr.ecr.${aws_region}.amazonaws.com/${project}-${environment}-api
ECR_WORKER=$${ACCOUNT}.dkr.ecr.${aws_region}.amazonaws.com/${project}-${environment}-worker
DB_CONNECTION=$(aws ssm get-parameter --name /${project}/${environment}/db/connection_string --with-decryption --query Parameter.Value --output text --region ${aws_region})
JWT_KEY=$(aws ssm get-parameter --name /${project}/${environment}/jwt/key --with-decryption --query Parameter.Value --output text --region ${aws_region})
ASAAS_WEBHOOK_BASE_URL=https://${api_fqdn}
CORS_ORIGIN=https://www.dev.libify.com.br
API_FQDN=${api_fqdn}
ASPIRE_FQDN=${aspire_fqdn}
EOF

aws ecr get-login-password --region ${aws_region} | docker login --username AWS --password-stdin $${ACCOUNT}.dkr.ecr.${aws_region}.amazonaws.com
docker compose pull || true
docker compose up -d
docker compose ps

echo "=== Deploy finalizado ==="

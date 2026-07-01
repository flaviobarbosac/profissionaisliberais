#!/bin/bash
set -euo pipefail
exec > /var/log/libify-user-data.log 2>&1

echo "=== Libify EC2 bootstrap ==="
dnf update -y
dnf install -y docker amazon-ssm-agent jq aws-cli
systemctl enable --now docker amazon-ssm-agent

mkdir -p /usr/local/lib/docker/cli-plugins
curl -SL "https://github.com/docker/compose/releases/download/v2.32.4/docker-compose-linux-x86_64" \
  -o /usr/local/lib/docker/cli-plugins/docker-compose
chmod +x /usr/local/lib/docker/cli-plugins/docker-compose

aws s3 cp "s3://${config_bucket}/deploy.sh" /tmp/libify-deploy.sh --region ${aws_region}
chmod +x /tmp/libify-deploy.sh
/tmp/libify-deploy.sh

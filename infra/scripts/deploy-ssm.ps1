#Requires -Version 5.1
param(
    [string]$Profile = "libify-dev",
    [string]$Region = "sa-east-1",
    [string]$InstanceId = ""
)

$ErrorActionPreference = "Stop"
$env:AWS_PROFILE = $Profile
$env:AWS_REGION = $Region

if (-not $InstanceId) {
    Push-Location "$PSScriptRoot\..\terraform"
    $InstanceId = terraform output -raw ec2_instance_id
    Pop-Location
}

function Invoke-SsmShell {
    param([string]$Script)
    $Script = $Script -replace "`r", ""
    $b64 = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($Script))
    $cmd = "echo $b64 | base64 -d | bash"
    $paramsFile = [System.IO.Path]::GetTempFileName()
    $utf8NoBom = New-Object System.Text.UTF8Encoding $false
    [System.IO.File]::WriteAllText($paramsFile, (@{ commands = @($cmd) } | ConvertTo-Json -Compress), $utf8NoBom)
    $cid = (aws ssm send-command --instance-ids $InstanceId --document-name AWS-RunShellScript --parameters "file://$paramsFile" --output text --query Command.CommandId)
    Remove-Item $paramsFile -Force
    for ($i = 0; $i -lt 90; $i++) {
        Start-Sleep -Seconds 5
        $status = aws ssm get-command-invocation --command-id $cid --instance-id $InstanceId --output text --query Status 2>$null
        if ($status -in @("Success", "Failed", "Cancelled", "TimedOut")) {
            $out = aws ssm get-command-invocation --command-id $cid --instance-id $InstanceId --output text --query StandardOutputContent
            $err = aws ssm get-command-invocation --command-id $cid --instance-id $InstanceId --output text --query StandardErrorContent
            return @{ Status = $status; Out = $out; Err = $err }
        }
    }
    throw "SSM timeout"
}

Write-Host "Deploy EC2 $InstanceId ..." -ForegroundColor Cyan
$r = Invoke-SsmShell -Script @'
set -euo pipefail
BUCKET=libify-dev-config-635198174869
mkdir -p /opt/libify
aws s3 cp s3://$BUCKET/docker-compose.yml /opt/libify/docker-compose.yml --region sa-east-1
aws s3 cp s3://$BUCKET/Caddyfile /opt/libify/Caddyfile --region sa-east-1
cd /opt/libify
ACCOUNT=$(aws sts get-caller-identity --query Account --output text)
DB=$(aws ssm get-parameter --name /libify/dev/db/connection_string --with-decryption --query Parameter.Value --output text --region sa-east-1)
JWT=$(aws ssm get-parameter --name /libify/dev/jwt/key --with-decryption --query Parameter.Value --output text --region sa-east-1)
cat > .env <<EOF
ECR_API=${ACCOUNT}.dkr.ecr.sa-east-1.amazonaws.com/libify-dev-api
ECR_WORKER=${ACCOUNT}.dkr.ecr.sa-east-1.amazonaws.com/libify-dev-worker
DB_CONNECTION=${DB}
JWT_KEY=${JWT}
ASAAS_WEBHOOK_BASE_URL=https://api.dev.libify.com.br
CORS_ORIGIN=http://54.232.84.74
CORS_ORIGIN_AMPLIFY=https://develop.d10vcq31qfr2rm.amplifyapp.com
GOOGLE_CLIENT_ID=560156263793-94lhftrcshishb58en2tqtjgatmv31b9.apps.googleusercontent.com
API_FQDN=api.dev.libify.com.br
ASPIRE_FQDN=aspire.dev.libify.com.br
EOF
aws ecr get-login-password --region sa-east-1 | docker login --username AWS --password-stdin ${ACCOUNT}.dkr.ecr.sa-east-1.amazonaws.com
docker compose pull
docker compose up -d
sleep 15
docker compose ps
'@

Write-Host "Status: $($r.Status)" -ForegroundColor $(if ($r.Status -eq 'Success') { 'Green' } else { 'Red' })
if ($r.Out) { Write-Host $r.Out }
if ($r.Err) { Write-Host $r.Err -ForegroundColor Yellow }
if ($r.Status -ne 'Success') { exit 1 }

Write-Host "`nTeste: http://54.232.84.74/health" -ForegroundColor Cyan

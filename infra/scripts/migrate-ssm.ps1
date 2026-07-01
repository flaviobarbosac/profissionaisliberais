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
    $paramsPath = ($paramsFile -replace '\\', '/')
    $cid = (aws ssm send-command --instance-ids $InstanceId --document-name AWS-RunShellScript --parameters "file://$paramsPath" --output text --query Command.CommandId)
    Remove-Item $paramsFile -Force
    for ($i = 0; $i -lt 90; $i++) {
        Start-Sleep -Seconds 5
        $status = aws ssm get-command-invocation --command-id $cid --instance-id $InstanceId --output text --query Status 2>$null
        if ($status -in @("Success", "Failed", "Cancelled", "TimedOut")) {
            return @{
                Status = $status
                Out    = aws ssm get-command-invocation --command-id $cid --instance-id $InstanceId --output text --query StandardOutputContent
                Err    = aws ssm get-command-invocation --command-id $cid --instance-id $InstanceId --output text --query StandardErrorContent
            }
        }
    }
    throw "SSM timeout"
}

Write-Host "Migrations RDS via EC2 $InstanceId ..." -ForegroundColor Cyan
$r = Invoke-SsmShell -Script @'
set -euo pipefail
BUCKET=libify-dev-config-635198174869
aws s3 cp s3://$BUCKET/migration.sql /tmp/migration.sql --region sa-east-1
CONN=$(aws ssm get-parameter --name /libify/dev/db/connection_string --with-decryption --query Parameter.Value --output text --region sa-east-1)
PGHOST=$(echo "$CONN" | sed -n 's/.*Host=\([^;]*\).*/\1/p')
PGPORT=$(echo "$CONN" | sed -n 's/.*Port=\([^;]*\).*/\1/p')
PGDATABASE=$(echo "$CONN" | sed -n 's/.*Database=\([^;]*\).*/\1/p')
PGUSER=$(echo "$CONN" | sed -n 's/.*Username=\([^;]*\).*/\1/p')
PGPASSWORD=$(echo "$CONN" | sed -n 's/.*Password=\([^;]*\).*/\1/p')
export PGHOST PGPORT PGDATABASE PGUSER PGPASSWORD
docker run --rm -i -e PGHOST -e PGPORT -e PGDATABASE -e PGUSER -e PGPASSWORD postgres:16 \
  psql "sslmode=require" -f - < /tmp/migration.sql
echo "Migrations OK"
'@

Write-Host "Status: $($r.Status)" -ForegroundColor $(if ($r.Status -eq 'Success') { 'Green' } else { 'Red' })
if ($r.Out) { Write-Host $r.Out }
if ($r.Err) { Write-Host $r.Err -ForegroundColor Yellow }
if ($r.Status -ne 'Success') { exit 1 }

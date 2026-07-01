#Requires -Version 5.1
param(
    [string]$Profile = "libify-dev",
    [string]$Region = "sa-east-1"
)

$ErrorActionPreference = "Stop"
$env:AWS_PROFILE = $Profile
$env:AWS_REGION = $Region

$RepoRoot = Resolve-Path "$PSScriptRoot\..\.."
$Account = aws sts get-caller-identity --query Account --output text
$EcrHost = "${Account}.dkr.ecr.${Region}.amazonaws.com"
$EcrApi = "${EcrHost}/libify-dev-api"
$EcrWorker = "${EcrHost}/libify-dev-worker"

Write-Host "Login ECR..." -ForegroundColor Cyan
aws ecr get-login-password --region $Region | docker login --username AWS --password-stdin $EcrHost

Push-Location $RepoRoot
Write-Host "Build API..." -ForegroundColor Cyan
docker build -f Libify.API/Dockerfile -t libify-api:latest .
if ($LASTEXITCODE -ne 0) { throw "Falha no build da API" }
Write-Host "Build Worker..." -ForegroundColor Cyan
docker build -f Libify.Worker/Dockerfile -t libify-worker:latest .
if ($LASTEXITCODE -ne 0) { throw "Falha no build do Worker" }
Pop-Location

docker tag libify-api:latest "${EcrApi}:latest"
docker tag libify-worker:latest "${EcrWorker}:latest"
docker push "${EcrApi}:latest"
docker push "${EcrWorker}:latest"

Write-Host "OK: ${EcrApi}:latest" -ForegroundColor Green
Write-Host "OK: ${EcrWorker}:latest" -ForegroundColor Green

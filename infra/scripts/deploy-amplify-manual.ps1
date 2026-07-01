#Requires -Version 5.1
<#
.SYNOPSIS
  Deploy manual do front no Amplify (zip) — sem conexao GitHub.
#>
param(
    [string]$Profile = "libify-dev",
    [string]$Region = "sa-east-1",
    [string]$AppId = "",
    [string]$Branch = "develop",
    [string]$FrontPath = "D:\Projetos\LibiFy\profissionaisliberais_front\prosiffionaisliberais-front",
    [string]$ApiBaseUrl = "https://api.dev.libify.com.br/api/v1"
)

$ErrorActionPreference = "Stop"
$env:AWS_PROFILE = $Profile
$env:AWS_REGION = $Region

if (-not $AppId) {
    Push-Location "$PSScriptRoot\..\terraform"
    $AppId = terraform output -raw amplify_app_id
    Pop-Location
}

Write-Host "Build front em $FrontPath ..." -ForegroundColor Cyan
Push-Location $FrontPath
$env:VITE_API_BASE_URL = $ApiBaseUrl
$env:VITE_APP_VERSION = "0.1.0-dev"
if (-not (Test-Path "node_modules\.bin\tsc.cmd")) {
    npm ci
    if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }
}
npm run build
if ($LASTEXITCODE -ne 0) { Pop-Location; exit 1 }

$zipPath = Join-Path $env:TEMP "libify-front-$(Get-Date -Format 'yyyyMMddHHmmss').zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path "dist\*" -DestinationPath $zipPath -Force
Pop-Location

Write-Host "Criando deployment Amplify..." -ForegroundColor Cyan
$dep = aws amplify create-deployment `
    --app-id $AppId `
    --branch-name $Branch `
    --output json | ConvertFrom-Json

$uploadUrl = $dep.zipUploadUrl
$jobId = $dep.jobId

Write-Host "Enviando zip ($([math]::Round((Get-Item $zipPath).Length / 1MB, 2)) MB)..." -ForegroundColor Cyan
curl.exe -sS -X PUT -T $zipPath -H "Content-Type: application/zip" $uploadUrl | Out-Null

aws amplify start-deployment `
    --app-id $AppId `
    --branch-name $Branch `
    --job-id $jobId `
    --output json | Out-Null

Remove-Item $zipPath -Force -ErrorAction SilentlyContinue
Write-Host "Deploy iniciado. Acompanhe: https://${Region}.console.aws.amazon.com/amplify/apps/${AppId}/branches/${Branch}/deployments" -ForegroundColor Green
Write-Host "URL: https://${Branch}.${AppId}.amplifyapp.com" -ForegroundColor Green

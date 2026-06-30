<#
  Deploy da Libify.API via FTP para libify.elroy.com.br.
  Faz o publish (Release, framework-dependent) e envia todos os arquivos por FTP.
  Nao depende do Visual Studio.

  Uso:
    .\deploy-ftp.ps1 -FtpPassword 'SUA_SENHA_FTP'

  A senha NAO fica salva em lugar nenhum (passada como parametro em tempo de execucao).
#>
param(
    [Parameter(Mandatory = $true)][string]$FtpPassword,
    [string]$Configuration = "Release",
    [string]$FtpHost = "ftp.elroy.com.br",
    [string]$FtpUser = "flaviobarbosa",
    [string]$FtpRoot = "/wwwroot/libify",
    [switch]$SelfContained,
    [string]$Runtime = "win-x64"
)

$ErrorActionPreference = "Stop"

$proj = Join-Path $PSScriptRoot "Libify.API\Libify.API.csproj"
$out  = Join-Path $env:TEMP "libify-publish"

if (Test-Path $out) { Remove-Item -Recurse -Force $out }

Write-Host "==> Publicando ($Configuration)..." -ForegroundColor Cyan
# IsTransformWebConfigDisabled: mantem nosso web.config (HttpPlatformHandler) intacto
$pubArgs = @($proj, "-c", $Configuration, "-o", $out, "--nologo", "-p:IsTransformWebConfigDisabled=true")
if ($SelfContained) {
    Write-Host "    (self-contained, $Runtime)" -ForegroundColor DarkCyan
    $pubArgs += @("-r", $Runtime, "--self-contained", "true")
}
dotnet publish @pubArgs
if ($LASTEXITCODE -ne 0) { throw "Falha no dotnet publish." }

# Normaliza o caminho (evita problemas com short path do %TEMP%, ex.: WINDOW~1)
$out = (Resolve-Path $out).Path

$cred    = New-Object System.Net.NetworkCredential($FtpUser, $FtpPassword)
$baseUri = "ftp://$FtpHost$FtpRoot"

function Ensure-FtpDir([string]$uri) {
    try {
        $req = [System.Net.FtpWebRequest]::Create($uri)
        $req.Credentials = $cred
        $req.UsePassive = $true
        $req.KeepAlive = $false
        $req.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory
        $req.GetResponse().Close()
    }
    catch [System.Net.WebException] {
        # 550 = diretorio ja existe -> ignora
    }
}

function Upload-File([string]$localPath, [string]$remoteUri) {
    $bytes = [System.IO.File]::ReadAllBytes($localPath)
    $attempt = 0
    while ($true) {
        $attempt++
        try {
            $req = [System.Net.FtpWebRequest]::Create($remoteUri)
            $req.Credentials = $cred
            $req.UsePassive = $true
            $req.UseBinary = $true
            $req.KeepAlive = $false
            $req.Method = [System.Net.WebRequestMethods+Ftp]::UploadFile
            $req.ContentLength = $bytes.Length
            $stream = $req.GetRequestStream()
            $stream.Write($bytes, 0, $bytes.Length)
            $stream.Close()
            $req.GetResponse().Close()
            return
        }
        catch [System.Net.WebException] {
            if ($attempt -ge 4) { throw }
            Start-Sleep -Milliseconds (500 * $attempt)
        }
    }
}

Write-Host "==> Enviando para $baseUri ..." -ForegroundColor Cyan
Ensure-FtpDir $baseUri

function Get-RelPath([string]$fullName) {
    # Caminho relativo a $out, imune a short/long path (WINDOW~1 vs "Windows 11")
    (Resolve-Path -LiteralPath $fullName -Relative) -replace '^\.\\', '' -replace '\\', '/'
}

Push-Location $out
try {
    # Cria a estrutura de pastas no FTP
    Get-ChildItem -Recurse -Directory | ForEach-Object {
        $rel = Get-RelPath $_.FullName
        Ensure-FtpDir "$baseUri/$rel"
    }

    # Envia os arquivos
    $files = Get-ChildItem -Recurse -File
    $total = $files.Count
    $i = 0
    foreach ($f in $files) {
        $i++
        $rel = Get-RelPath $f.FullName
        Write-Host ("[{0}/{1}] {2}" -f $i, $total, $rel)
        Upload-File $f.FullName "$baseUri/$rel"
    }
}
finally {
    Pop-Location
}

Write-Host "==> Deploy concluido com sucesso." -ForegroundColor Green

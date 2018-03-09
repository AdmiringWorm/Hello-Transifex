[CmdletBinding()]
param(
    [string]$Script = "build.cake",
    [string]$Target,
    [string]$Configuration,
    [ValidateSet("Quiet", "Minimal", "Normal", "Verbose", "Diagnostic")]
    [string]$Verbosity,
    [switch]$ShowDescription,
    [Alias("WhatIf", "Noop")]
    [switch]$DryRun,
    [switch]$Experimental,
    [switch]$Mono,
    [version]$CakeVersion = $null,
    [Parameter(Position = 0, Mandatory = $false, ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)


if (!(Test-Path Function:\Expand-Archive)) {
    function Expand-Archive() {
        param([string]$Path, [string]$DestinationPath)

        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($Path, $DestinationPath)
    }
}

function GetProxyEnabledWebClient {
    $wc = New-Object System.Net.WebClient
    $proxy = [System.Net.WebRequest]::GetSystemWebProxy()
    $proxy.Credentials = [System.Net.CredentialCache]::DefaultCredentials
    $wc.Proxy = $proxy
    return $wc
}

$TOOLS_DIR = Join-Path $PSScriptRoot "tools"
$CAKE_EXE_DIR = ""
$CAKE_URL = "https://www.nuget.org/api/v2/package/Cake/$($CakeVersion)"

if ($CakeVersion) {
    $CAKE_EXE_DIR = Join-Path "$TOOLS_DIR" "Cake.$($CakeVersion.ToString())"
} else {
    $CAKE_EXE_DIR = Join-Path "$TOOLS_DIR" "Cake"
}
$CAKE_EXE = Join-Path $CAKE_EXE_DIR "Cake.exe"

if ((Test-Path $PSScriptRoot) -and !(Test-Path $TOOLS_DIR)) {
    Write-Verbose -Message "Creating tools directory..."
    New-Item -Path $TOOLS_DIR -Type Directory | Out-Null
}

if (!(Test-Path $CAKE_EXE)) {
    $tmpDownloadDir = "$env:TEMP/Cake.nupkg"
    Write-Verbose -Message "Downloading Cake package..."
    try {
        $wc = GetProxyEnabledWebClient
        $wc.DownloadFile($CAKE_URL, $tmpDownloadDir)
    } catch {
        throw "Could not download Cake package...`n`nException:$_"
    }

    Write-Verbose "Extracting Cake package..."
    Expand-Archive -Path $tmpDownloadDir -DestinationPath $CAKE_EXE_DIR   
}

$cakeArguments = @("$Script")
if ($Target) { $cakeArguments += "-target=$Target" }
if ($Configuration) { $cakeArguments += "-configuration=$Configuration" }
if ($Verbosity) { $cakeArguments += "-verbosity=$Verbosity" }
if ($ShowDescription) { $cakeArguments += "-showdescription" }
if ($DryRun) { $cakeArguments += "-dryrun" }
if ($Experimental) { $cakeArguments += "-experimental" }
if ($Mono) { $cakeArguments += "-mono" }
$cakeArguments += $ScriptArgs

# Start Cake
Write-Host "Running build script..."
& $CAKE_EXE $cakeArguments
exit $LASTEXITCODE
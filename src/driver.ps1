#!/usr/bin/env pwsh

$port = 8006
$targetUrl = "http://localhost:$($port)/api/values"
$memory = 10
$memoryIncrease = 1

function Log {
    Param ([string] $s)
    Write-Output "###### $s"
}

Log ".NET Docker resource limits test script"
Log
Log "Building aspnetapi Docker image"

docker build --pull -t aspnetapi ./aspnetapi

Log "Build hiver tool"

dotnet publish hiver/hiver.csproj -c release -o app

docker kill aspnetapi

while ($true)
{
    Log "Running Docker container with $memory mb"

    docker run --rm -d -p "$($port):80" -m="$($memory)mb" --name aspnetapi aspnetapi
    Start-Sleep -Seconds 1
    dotnet ./hiver/app/hiver.dll $targetUrl --progressivelyIncreaseRate true

    docker kill aspnetapi

    $memory = $memory + $memoryIncrease
    $memoryIncrease++
}

$tagVersion = "7.0.203-alpine3.17-amd64"
$image = "mcr.microsoft.com/dotnet/sdk:$tagVersion"

$configuration = "Release"
$framework = "net7.0-windows"
$arch = "x64"
$os = "win"

$projectName = (Get-Item -Path ./).Name

Write-Host "Docker image: $image"

docker run --rm -v ${pwd}:/app -w /app $image dotnet publish $projectName.sln -a $arch -c $configuration -f $framework --os $os

if ($?)
{
    Compress-Archive -Force -Path ".\$projectName\bin\$arch\$configuration\$framework\$os-$arch\publish\*" -DestinationPath ".\$projectName.zip"
}

if ($?)
{
    Write-Host "ZIP file: $projectName.zip created."
}

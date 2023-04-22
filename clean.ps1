function Remove {
    param (
        $Path
    )

    if (Test-Path -Path $Path)
    {
        Write-Host "Remove: $Path"
        Remove-Item -Path $Path -Recurse -Force
    }
}

$projectName = (Get-Item -Path ./).Name

Remove -Path "$projectName.zip"
Remove -Path "./$projectName/bin/"
Remove -Path "./$projectName/obj/"

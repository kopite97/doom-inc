$ErrorActionPreference = 'Stop'
$workspaceRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '../../../..')).Path
$assetsRoot = [IO.Path]::GetFullPath((Join-Path $workspaceRoot 'UnityProject/Assets'))
$utf8 = New-Object System.Text.UTF8Encoding($false)

# One-time migration record. Refuse to overwrite the before/after evidence or re-run.
$beforeFile = Join-Path $PSScriptRoot 'asset-migration-before.json'
if (Test-Path -LiteralPath $beforeFile) { throw 'Migration evidence already exists.' }

$folderMap = @{
    'Game' = 'Scripts'
    'Game/Definitions' = 'SO'
    'Game/Foundation' = 'Scripts/Core'
    'Game/Foundation/Definitions' = 'Scripts/Data/Definitions'
    'Game/Foundation/Things' = 'Scripts/Core/Things'
    'Game/Foundation/Spatial' = 'Scripts/Core/Spatial'
    'Game/Editor' = 'Scripts/Editor'
    'Game/Presentation' = 'Scripts/Presentation'
    'Tests' = 'Scripts/Tests'
    'Tests/EditMode' = 'Scripts/Tests/EditMode/Environment'
    'Tests/PlayMode' = 'Scripts/Tests/PlayMode/Environment'
    'Tests/FoundationEditMode' = 'Scripts/Tests/EditMode/Foundation'
    'Tests/SpatialPlayMode' = 'Scripts/Tests/PlayMode/Spatial'
    'Settings/Scenes' = 'Settings/SceneTemplates/Scenes'
    'Settings' = 'Settings'
    'Scenes' = 'Scenes'
}

function CheckedAssetPath([string] $relative) {
    $full = [IO.Path]::GetFullPath((Join-Path $assetsRoot $relative))
    if (-not $full.StartsWith($assetsRoot + [IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Path escapes Assets: $relative"
    }
    return $full
}

function Destination([string] $relative, [bool] $isFolder) {
    if ($isFolder) {
        if (-not $folderMap.ContainsKey($relative)) { throw "Unmapped folder: $relative" }
        return $folderMap[$relative]
    }
    $name = [IO.Path]::GetFileName($relative)
    switch -Regex ($relative) {
        '^Game/Foundation/Definitions/(Resource|Facility|Creature)Definition.cs$' {
            return "Scripts/Data/Definitions/$($Matches[1])/$name"
        }
        '^Game/Foundation/Definitions/' { return "Scripts/Data/Definitions/$name" }
        '^Game/Foundation/Things/Resource' { return "Scripts/Core/Resource/$name" }
        '^Game/Foundation/Things/' { return "Scripts/Core/Things/$name" }
        '^Game/Foundation/Spatial/' { return "Scripts/Core/Spatial/$name" }
        '^Game/Foundation/' { return "Scripts/Core/$name" }
        '^Game/Definitions/OrganicMatter.asset$' { return "SO/Resource/$name" }
        '^Game/Definitions/Storage.asset$' { return "SO/Facility/$name" }
        '^Game/Definitions/PrototypeSupport.asset$' { return "SO/Creature/$name" }
        '^Game/Presentation/.*\.asmdef$' { return "Scripts/Presentation/$name" }
        '^Game/Presentation/' { return "Scripts/Presentation/Diagnostics/Map/$name" }
        '^Game/Editor/FoundationDefinitionAssets.cs$' { return "Scripts/Editor/Definitions/$name" }
        '^Game/Editor/MapFoundationSceneAssets.cs$' { return "Scripts/Editor/Scenes/$name" }
        '^Game/Editor/' { return "Scripts/Editor/$name" }
        '^Tests/FoundationEditMode/DefinitionFoundationTests.cs$' { return "Scripts/Tests/EditMode/Foundation/Definitions/$name" }
        '^Tests/FoundationEditMode/ThingFoundationTests.cs$' { return "Scripts/Tests/EditMode/Foundation/Things/$name" }
        '^Tests/FoundationEditMode/ResourceOwnershipTests.cs$' { return "Scripts/Tests/EditMode/Foundation/Resource/$name" }
        '^Tests/FoundationEditMode/GridMapTests.cs$' { return "Scripts/Tests/EditMode/Foundation/Spatial/$name" }
        '^Tests/FoundationEditMode/FoundationTestFixture.cs$' { return "Scripts/Tests/EditMode/Foundation/Fixtures/$name" }
        '^Tests/FoundationEditMode/' { return "Scripts/Tests/EditMode/Foundation/$name" }
        '^Tests/EditMode/' { return "Scripts/Tests/EditMode/Environment/$name" }
        '^Tests/PlayMode/' { return "Scripts/Tests/PlayMode/Environment/$name" }
        '^Tests/SpatialPlayMode/' { return "Scripts/Tests/PlayMode/Spatial/$name" }
        '^Scenes/SampleScene.unity$' { return "Scenes/Diagnostics/Environment/$name" }
        '^Scenes/MapFoundation.unity$' { return "Scenes/Diagnostics/Map/$name" }
        '^Settings/Scenes/' { return "Settings/SceneTemplates/Scenes/$name" }
        '^Settings/Lit2DSceneTemplate.scenetemplate$' { return "Settings/SceneTemplates/$name" }
        '^Settings/InputSystem_Actions.inputactions$' { return "Settings/Input/$name" }
        '^Settings/.*\.asset$' { return "Settings/Rendering/$name" }
        default { throw "Unmapped asset: $relative" }
    }
}

$records = @(Get-ChildItem -LiteralPath $assetsRoot -Recurse -File -Filter '*.meta' | Sort-Object FullName | ForEach-Object {
    $source = $_.FullName.Substring($assetsRoot.Length + 1).Replace('\', '/')
    $source = $source.Substring(0, $source.Length - 5)
    $sourcePath = CheckedAssetPath $source
    $isFolder = Test-Path -LiteralPath $sourcePath -PathType Container
    $destination = Destination $source $isFolder
    $destinationPath = CheckedAssetPath $destination
    if ($source -ne $destination -and (Test-Path -LiteralPath ($destinationPath + '.meta'))) {
        throw "Destination already has a meta file: $destination"
    }
    $guidLine = Select-String -LiteralPath $_.FullName -Pattern '^guid: (.+)$'
    [pscustomobject]@{
        before = "Assets/$source"
        after = "Assets/$destination"
        folder = $isFolder
        guid = $guidLine.Matches[0].Groups[1].Value
        metaSha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant()
        contentSha256 = $(if ($isFolder) { $null } else { (Get-FileHash -LiteralPath $sourcePath -Algorithm SHA256).Hash.ToLowerInvariant() })
    }
})
[IO.File]::WriteAllText($beforeFile, ($records | ConvertTo-Json -Depth 5) + "`n", $utf8)

foreach ($record in $records) {
    if ($record.before -eq $record.after) { continue }
    $source = CheckedAssetPath $record.before.Substring(7)
    $destination = CheckedAssetPath $record.after.Substring(7)
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($destination)) | Out-Null
    if ($record.folder) {
        [IO.Directory]::CreateDirectory($destination) | Out-Null
    } else {
        if (Test-Path -LiteralPath $destination) { throw "Destination exists: $destination" }
        Move-Item -LiteralPath $source -Destination $destination
    }
    Move-Item -LiteralPath ($source + '.meta') -Destination ($destination + '.meta')
}

# Remove only the old, now empty directories after verifying the resolved path.
foreach ($record in ($records | Where-Object { $_.folder -and $_.before -ne $_.after } | Sort-Object { $_.before.Length } -Descending)) {
    $source = CheckedAssetPath $record.before.Substring(7)
    if (Test-Path -LiteralPath $source -PathType Container) {
        if (@(Get-ChildItem -LiteralPath $source -Force).Count -ne 0) { throw "Old directory is not empty: $source" }
        Remove-Item -LiteralPath $source
    }
}

$after = @($records | ForEach-Object {
    $destination = CheckedAssetPath $_.after.Substring(7)
    $metaSha = (Get-FileHash -LiteralPath ($destination + '.meta') -Algorithm SHA256).Hash.ToLowerInvariant()
    $contentSha = $(if ($_.folder) { $null } else { (Get-FileHash -LiteralPath $destination -Algorithm SHA256).Hash.ToLowerInvariant() })
    if ($metaSha -ne $_.metaSha256 -or $contentSha -ne $_.contentSha256) { throw "Content changed during move: $destination" }
    [pscustomobject]@{ path = $_.after; guid = $_.guid; metaSha256 = $metaSha; contentSha256 = $contentSha }
})
[IO.File]::WriteAllText((Join-Path $PSScriptRoot 'asset-migration-after-move.json'), ($after | ConvertTo-Json -Depth 5) + "`n", $utf8)
Write-Output "Moved assets and folder metadata: $(@($records | Where-Object { $_.before -ne $_.after }).Count); recorded: $($records.Count). All existing metadata and contents preserved byte-for-byte."

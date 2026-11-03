$ErrorActionPreference = 'Stop'
function Invoke-Code([string]$code) {
    $raw = unity command script eval --code $code --project-path UnityProject --caller plugin --skill unity-cli --json
    $value = $raw | ConvertFrom-Json
    if (!$value.success -or !$value.data.result.success) { throw ($raw -join "`n") }
    return $value.data.result.result
}
$cases = @(
    @{name='reload-on-bootstrap'; enabled='false'; flags=0; scene='Bootstrap'},
    @{name='reload-on-laboratory'; enabled='false'; flags=0; scene='Laboratory'},
    @{name='domain-off-title'; enabled='true'; flags=1; scene='Title'},
    @{name='domain-off-laboratory'; enabled='true'; flags=1; scene='Laboratory'},
    @{name='both-off-title'; enabled='true'; flags=3; scene='Title'},
    @{name='both-off-laboratory'; enabled='true'; flags=3; scene='Laboratory'}
)
foreach ($case in $cases) {
    Invoke-Code 'MobileGame.Foundation.Editor.Plan020Probe.RestoreSize(); UnityEditor.EditorApplication.isPlaying=false; return true;' | Out-Null
    Start-Sleep -Seconds 2
    $path = 'Assets/Scenes/Gameplay/' + $case.scene + '/Temp_' + $case.scene + '.unity'
    $code = 'UnityEditor.EditorSettings.enterPlayModeOptionsEnabled=' + $case.enabled + '; UnityEditor.EditorSettings.enterPlayModeOptions=(UnityEditor.EnterPlayModeOptions)' + $case.flags + '; UnityEditor.SceneManagement.EditorSceneManager.OpenScene("' + $path + '"); MobileGame.Foundation.Editor.AppPlayEntry.CurrentNew(); return true;'
    Invoke-Code $code | Out-Null
    Start-Sleep -Seconds 4
    $ready = $false
    for ($i=0; $i -lt 20; $i++) {
        $ready = Invoke-Code 'var f=MobileGame.Presentation.SceneFlowManager.Instance; if(f!=null){f.SendMessage("OnApplicationFocus",true); if(f.Laboratory!=null)f.Laboratory.SendMessage("OnApplicationFocus",true);} return f!=null && !f.IsTransitioning && MobileGame.Presentation.GameDataStore.Instance?.State==MobileGame.Presentation.GameDataLoadState.Ready;'
        if ($ready) { break }
        Start-Sleep -Seconds 1
    }
    if (!$ready) { throw ('entry timeout ' + $case.name) }
    $result = Invoke-Code ('MobileGame.Foundation.Editor.Plan020Probe.Write("entry-' + $case.name + '", MobileGame.Foundation.Editor.Plan020Probe.Snapshot()); return MobileGame.Foundation.Editor.Plan020Probe.Snapshot();')
    $detail = Invoke-Code ('var all=UnityEngine.FindObjectsInactive.Include; var sort=UnityEngine.FindObjectsSortMode.None; var detail=new {labViews=UnityEngine.Object.FindObjectsByType<MobileGame.Presentation.LaboratoryView>(all,sort).Length,popupViews=UnityEngine.Object.FindObjectsByType<MobileGame.Presentation.PopupBase>(all,sort).Length,optionsEnabled=UnityEditor.EditorSettings.enterPlayModeOptionsEnabled,options=(int)UnityEditor.EditorSettings.enterPlayModeOptions}; MobileGame.Foundation.Editor.Plan020Probe.Write("entry-detail-' + $case.name + '",detail); return detail;')
    $expectedViews=if($case.scene -eq 'Laboratory'){1}else{0}
    $expectedPath=if($case.scene -eq 'Laboratory'){'Assets/Scenes/Gameplay/Laboratory/Temp_Laboratory.unity'}else{'Assets/Scenes/Gameplay/Title/Temp_Title.unity'}
    if($detail.labViews -ne $expectedViews -or $detail.popupViews -ne 0 -or $result.scene -ne $expectedPath){throw ('unexpected scene/view lifetime ' + $case.name)}
    if ($result.flow -ne 1 -or $result.data -ne 1 -or $result.hosts -ne 1 -or $result.eventSystems -ne 1 -or $result.popupCount -ne 0 -or $result.error) { throw ('unexpected entry state ' + $case.name) }
    if ($case.scene -eq 'Laboratory' -and ($result.phase -ne 'Playing' -or $result.things -ne 2)) { throw ('unexpected lab state ' + $case.name) }
    Write-Output ($case.name + ' PASS')
}
Invoke-Code 'UnityEditor.EditorApplication.isPlaying=false; return true;' | Out-Null

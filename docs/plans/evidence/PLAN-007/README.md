# PLAN-007 이관·GameFlow·저장 검증 기록

- 날짜: 2026-10-26
- 기준 커밋: `aeec4ccb49f1897692a278d65a51bcbdef9ebb33`
- 범위: 기존 Assets 이관을 먼저 검증하고 GameFlow·단일 슬롯 저장·복원 기반을 구현했다.
- 완료 기준과 결과 요약은 [PLAN-007](../../PLAN-007-gameflow-and-persistence.md), 책임·저장 형식과 지원 범위는 [TECH-004](../../../decisions/TECH-004-session-and-persistence.md)에 있다.

## 이동 기준

| 이전 분류 | 새 분류 |
| --- | --- |
| Game/Foundation/Definitions | Scripts/Data/Definitions, 종류별 Resource·Facility·Creature |
| Game/Foundation/Things | Scripts/Core/Things, 자원 컴포넌트는 Core/Resource |
| Game/Foundation/Spatial | Scripts/Core/Spatial |
| Game/Presentation | Scripts/Presentation/Diagnostics/Map |
| Game/Editor | Scripts/Editor/Definitions·Scenes |
| Tests | Scripts/Tests/EditMode·PlayMode 아래 Environment·Foundation·Spatial. Foundation 검사는 Definitions·Things·Resource·Spatial·Fixtures로 세분화 |
| Game/Definitions | SO/Resource·Facility·Creature |
| Scenes | Scenes/Diagnostics/Environment·Map |
| Settings | Rendering·Input·SceneTemplates, 템플릿용 씬은 SceneTemplates/Scenes |

모든 경로는 `UnityProject/Assets/` 기준이다. 기존 폴더 메타도 대응 경로로 이동했다. 실제 파일이 있는 분류만 만들었다.
Foundation 어셈블리는 Core의 기존 asmdef를 유지하고 Data의 새 asmref가 그 GUID를 참조한다. 폴더 분류만으로 어셈블리·namespace를 분할하지 않았다.
Editor·Presentation·테스트 어셈블리의 이름·플랫폼·참조 설정과 기존 asmdef GUID는 변경하지 않았다.

## 기록과 결과

- [이관 실행 스크립트](migrate-assets.ps1): 절대 경로가 Assets 내부인지 검사하고 PowerShell `Move-Item -LiteralPath`로 에셋·메타를 이동했다. 삭제는 확인된 빈 이전 디렉터리에 한정했다. 재실행 시 기존 증거를 덮어쓰지 않고 중단한다.
- [이동 전 목록과 목적지](asset-migration-before.json): 62개 에셋·폴더의 이전/새 경로, GUID, 메타·내용 SHA-256.
- [이동 직후 목록](asset-migration-after-move.json): 60개 경로 이동, Scenes·Settings 루트 2개 유지. 62개 메타와 기존 파일 내용이 이동 전과 바이트 단위로 일치했다.
- [경로 수정 후 검사](asset-migration-path-update-audit.json): 기존 GUID·메타 62/62 보존, 프로젝트 C# 27개 모두 Scripts 아래, 기존 asmdef 7개 유지.
- 코드 내용 변경은 Editor 생성기 2개와 기존 테스트 4개에 한정했다. 새 경로 문자열·종류별 SO 경로·누락된 대상 하위 폴더 생성 처리를 갱신했다. 기존 게임 로직·SO 값·씬 내용은 바꾸지 않았다.
- EditorBuildSettings와 ProjectSettings의 기본 씬 경로를 갱신했다. 기존 씬·SO·렌더링·입력 GUID 참조는 그대로다.
- 현행 문서의 실제 소스 링크를 갱신했다. 과거 evidence 내용·해시와 PLAN-002의 당시 실행 경로 설명은 보존했다.
- `node scripts/validate-agent-docs.mjs`: active PLAN-007 하나, PLAN 7개 목록 일치, 로컬 파일 링크 941개와 문서 줄 수 검사 통과.
- `git diff --check`: 통과.

새 폴더·파일의 `.meta`는 Unity 재임포트로 생성했다. GUID를 수동 생성하지 않았다.
신규·교체 Sprite가 필요한 작업이 아니므로 Require 요청 폴더를 만들지 않았다.

## 재현 환경과 명령

Windows PC, Unity 6000.3.27f1, Universal 2D(URP), Android 대상 Editor에서 공식 CLI 1.0.0-beta.12와 Pipeline 0.9.1-exp.1을 사용했다.
이번 채팅의 Unity MCP 도구는 제공되지 않아 CLI로 연결했다. 같은 프로젝트의 기존 Editor만 사용했다.
아래는 저장소 루트에서 실행한다. JSON을 실행별 파일에 보관하고 비동기 검사는 완료 응답까지 확인한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command script recompile --project-path $projectPath --json
unity command script recompile_status --project-path $projectPath --json
unity command test run --mode editor --project-path $projectPath --timeout 60 --json
unity command test run --mode playmode --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
unity command editor status --project-path $projectPath --json
unity command console status --project-path $projectPath --json
```

검증 씬이 없을 때만 `unity command editor invoke_menu --path 'Tools/Mobile Game/Create Session Foundation Scene' --project-path $projectPath --json`을 실행한다. 기존 씬은 덮어쓰지 않는다.
직접 확인하려면 `Tools/Mobile Game/Open Session Foundation Scene` 메뉴로 열고 Play를 누른다. 시설 선택·이동/회전·확정/취소, 저장·복원·재시작, 목표/연출 완료 검증 신호를 제공한다.
진단 슬롯은 `Application.persistentDataPath/Diagnostics/PLAN-007/session.json`이다. 테스트는 별도 임시 슬롯으로 격리하며 진단 슬롯을 덮어쓰지 않는다.

## 실행 결과와 증거

| 순서 | 결과 | 증거 |
| --- | --- | --- |
| 이관 직후 | 컴파일 성공, 기존 Edit 29/29·Play 2/2 | [컴파일](migration-recompile-status.json), [Edit](migration-editmode.json), [Play](migration-playmode-status.json) |
| 첫 통합 | 테스트의 SessionState 이름 충돌 수정, 첫 Edit 71/72에서 손상 저장 예외 처리 누락 수정 | [컴파일 실패](session-recompile-status.json), [첫 Edit](session-editmode-first.json) |
| 최종 컴파일·Edit | 컴파일 오류 0, 72/72 통과·실패/건너뜀 0 | [컴파일](session-final-recompile-status.json), [Edit JSON](session-editmode-final.json) |
| 최종 Play | 실제 씬 포함 4/4 통과·실패/건너뜀 0 | [시작](session-playmode-start.json), [완료 JSON](session-playmode-first-status.json), [XML](session-playmode-final.xml) |
| 종료 상태 | Play stopped, 현재 콘솔 오류/경고 0 | [Editor](session-final-editor-status.json), [콘솔](session-final-console-status.json) |
| 최종 에셋 감사 | 기존 메타 62·asmdef 7 보존, C# 39개 전부 Scripts, 메타 116개 누락/중복 0, asmref 4·씬/SO 참조 정상 | [정적 감사](session-asset-final-audit.json) |

Play XML은 콘솔에 출력된 실제 TestResults.xml에서 복사했으며 2026-10-26 06:55:29~06:55:31 UTC 실행과 4건 결과가 일치한다.
Edit XML은 후속 Play 실행이 덮어썼으므로 Edit의 원본 전체 JSON을 증거로 사용한다. XML만을 위해 테스트를 반복하지 않았다.
콘솔 누적 버퍼에는 이 작업 전 QuickInstaller 예외와 수정한 컴파일 오류가 남아 있다. [원본 이력](session-final-console-history.json)과 현재 콘솔 상태를 구분한다.
최종 코드·어셈블리·정의·씬·설정·패키지의 SHA-256은 [소스 목록](session-source-manifest.json)에 기록했다. 미커밋 변경이므로 기준 커밋만으로 이번 소스를 식별하지 않는다.

## 화면·임의 에셋·제한

[화면 PNG](session-foundation-playmode.png)는 실제 Play Mode에서 OnGUI 렌더링 후 캡처했다. 격자·시설·생명체·상호작용 위치·단계·저장 상태·버튼이 표시되고 주요 내용의 잘림이 없음을 확인했다.
임의(Temp) 씬 `Assets/Scenes/Diagnostics/Session/Temp_SessionFoundation.unity`의 `Temp_SessionCamera`·`Temp_SessionFoundationView`는 PC 기반 검증용이다.
기존 진단 구성인 저장소 2개·보조형 2개·유기물 5개를 재사용하며 실제 게임 초기 상태가 아니다. 화면은 내장 whiteTexture를 그려 Sprite 교체 요청이 없다.
정식 화면에서는 Presentation 연결을 교체하고 같은 SessionController·저장 경로 주입을 사용한다. 실제 시작 데이터는 후속 초기 건설 단계에서 연결한다.
마우스 이벤트 주입·최종 게임 UI·Android 빌드/실기기·파일 교체/강제 종료·성능·정식 아트 시각 검수는 이번 검증에 포함하지 않았다.

# PLAN-013 실행 증거

대상: [PLAN-013](../../PLAN-013-responsive-ui-and-mobile-input.md). 기술 선택은 [TECH-010](../../../decisions/TECH-010-responsive-ui-and-mobile-input.md)을 따른다.

## 환경과 재현

- Unity 6000.3.27f1, Windows Editor, Android 대상, Input System 1.20.1, uGUI 2.0.0. 패키지·엔진 업그레이드는 하지 않았다.
- 시작 소스 식별은 [baseline.json](baseline.json)에 있다. 이 파일의 PLAN-012 테스트 수는 과거 기준이며 이번 실행 결과가 아니다.
- 모든 플레이 검사는 임시 저장 슬롯을 사용한다. 합성 Mouse/Touchscreen 기기는 검증 후 제거하고 기존 기기 활성화 상태를 복원한다.
- 실제 게임 씬은 `Assets/Scenes/Gameplay/Laboratory/Temp_Laboratory.unity`다. Canvas/EventSystem과 화면 행은 실행 시 생성하고 기존 Bootstrap/Data 참조와 씬 GUID를 유지한다.

저장소 루트 PowerShell에서 실행한다. 컴파일 완료를 확인한 다음 테스트를 실행하고, 비동기 테스트가 완료된 다음 다른 실행을 시작한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command script recompile --project-path $projectPath --json
unity command script recompile_status --project-path $projectPath --json
unity command test run --mode editor --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
unity command test run --mode playmode --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
```

Android 도구는 프로젝트 경로에 한글을 포함한 비 ASCII 문자가 있으면 빌드하지 못한다. Android 빌드는 Assets·Packages·ProjectSettings를 영문 경로의 독립 복사본에 준비하고 원본과 파일 SHA-256을 대조한 뒤 수행한다. 원본의 저장 파일·Library·빌드 캐시를 공유하지 않는다. 아래 메뉴는 해당 복사본의 Editor 연결이 준비된 상태에서 사용한다. 이번 재시도는 같은 `LaboratoryPlayerBuild.BuildAndroidApk` 메서드를 batch 실행했으며 경로와 실행 식별은 [android-ascii-run.json](android-ascii-run.json)에 있다.

```powershell
# 이번 실행에서 기록한 영문 경로 복사본. 다른 환경에서는 검증한 복사본 경로로 지정한다.
$projectPath = (Get-Content -Raw '.\docs\plans\evidence\PLAN-013\android-ascii-run.json' | ConvertFrom-Json).projectPath
unity command editor invoke_menu --path 'Tools/Mobile Game/Configure Laboratory Player Entry' --project-path $projectPath --json
unity command editor invoke_menu --path 'Tools/Mobile Game/Build PLAN-013 Android APK' --project-path $projectPath --json
```

## 자동 검증 결과

| 실행 | 최종 결과 | 실행 시각 KST (UTC+9) | 증거 |
| --- | --- | --- | --- |
| 컴파일 | completed, compilationFailed=false, 오류 0 | 결과 파일 참조 | [compile-final.json](compile-final.json) |
| 전체 Edit Mode | 259/259 통과, 실패·건너뜀·미확정 0 | 2026-10-28 13:47:19~13:48:11 | [JSON](edit-all.json), [XML](edit-all.xml) |
| 전체 Play Mode | 23/23 통과, 실패·건너뜀·미확정 0 | 2026-10-28 13:49:43~13:50:43 | [JSON](play-all.json), [XML](play-all.xml) |
| 렌더 수정 후 종료 화면 대상 | 1/1 통과, 실패·건너뜀·미확정 0 | 2026-10-28 13:54:40~13:54:41 | [JSON](play-ending-render.json), [XML](play-ending-render.xml) |

JSON 요약과 XML의 실제 실행 수·상태를 대조했다. 마지막 실행은 렌더 수정의 영향을 받는 종료 화면 검사 1건이며, 전체 Play 23건을 다시 실행한 결과로 합산하지 않는다. [play-all-first.json](play-all-first.json)도 23/23 통과한 실행이며 실패 기록이 아니다.

### 확인한 범위

- 제스처 단위 검사: 탭/드래그/핀치, UI 소유권, 취소, 잔여 손가락 차단.
- 실제 입력 어댑터: InputSystem.QueueStateEvent → EnhancedTouch/Mouse → Map 또는 EventSystem → uGUI → 공통 명령. 버튼 onClick 직접 호출로 대체하지 않는다.
- 입력 경유 건설·확정/취소·시설 운용·겹친 생명체 선택·개별 배정/우선순위·방출/보고·결과 재시작과 앱 pause/resume 중 이전 터치 취소를 확인했다.
- 개체/완료 시설을 생성하는 입력용 fixture는 UI 연결 검사를 위한 준비다. 비용·생산·운반·저장 규칙은 기존 회귀 검사에서 별도로 확인한다.
- 같은 세션·카탈로그·배치 초안을 유지하는 6개 화면 크기, 양방향 로딩/복원 실패, 긴 오류 스크롤, 연출 시간·요청·세션 보존과 결과 저장 실패의 복구 버튼 접근을 확인했다.
- 노치/안전 영역 좌표는 별도 순수 계산 검사다. Editor 캡처의 safeArea는 전체 화면이며 실제 기기의 노치 표시 확인을 대신하지 않는다.

### 실패와 수정 후 확인

| 발견 | 수정·확인 | 원본과 재검증 |
| --- | --- | --- |
| 레이아웃 검사 종료 시 `Invalid index 0 18 1` | Game View custom size 제거에 전체 인덱스를 사용하도록 도구 수정. 0/1에서 1/1 통과 | [첫 실행](play-layout-first.json), [재실행](play-layout-second.json) |
| 합성 터치 후 같은 좌표의 마우스 버튼 조작 실패 | 기기 전환 fixture가 실제 위치 이동 이벤트도 보내도록 수정. 첫 2/3 이후 확장 입력 검사 6/6 통과 | [첫 실행](play-input-first.json), [재실행](play-input-second.json) |
| 전체 자동 검사 통과 후 캡처에서 행성 그림 누락 | custom Graphic의 `useLegacyMeshGeneration=false`와 CanvasRenderer 필수화를 적용. renderer mesh·material·cull 검사를 추가하고 종료 화면 1/1 및 실제 캡처 표시 확인 | [최종 대상 검사](play-ending-render.json), 아래 종료 화면 캡처 |
| 종료 렌더 대상 단독 실행에서 카탈로그 정의 참조 불일치 | 검증 codec을 실제 GameDataStore 카탈로그의 정의 참조로 구성. 단독 fixture의 0/1 이후 1/1 통과 | [첫 실행](play-ending-render-first.json), [재실행](play-ending-render.json) |

1024×768 시각 확인에서 배치 도구 문구가 잘리는 문제도 수정했다. 도구 폭을 최소 90 논리 단위로 잡고 좁은 경우 3+3행으로 배치하여 Map 높이와 함께 재계산했다. 상태 검사 통과와 실제 그림·문구 표시는 구분하여 확인했다.

## 화면 증거

PNG 14장의 실제 크기가 파일명과 일치하며 빈 파일은 없다. 상태 화면 8장의 JSON 크기·단계도 PNG와 일치한다. 배치 화면 6장은 별도 상태 JSON 없이 레이아웃 Play 검사와 연결된다.

| 화면 비율 | 가로 | 세로 |
| --- | --- | --- |
| 16:9 / 9:16 | [1280×720](screens/plan013-layout-1280x720.png) | [720×1280](screens/plan013-layout-720x1280.png) |
| 20:9 / 9:20 | [1600×720](screens/plan013-layout-1600x720.png) | [720×1600](screens/plan013-layout-720x1600.png) |
| 4:3 / 3:4 | [1024×768](screens/plan013-layout-1024x768.png) | [768×1024](screens/plan013-layout-768x1024.png) |

| 상태 | 가로 1280×720 | 세로 720×1280 |
| --- | --- | --- |
| 긴 로딩 오류·스크롤·재시도 | [PNG](screens/plan013-screen-loading-error-scroll-1280x720-20261028-045022-365.png) · [JSON](screens/plan013-screen-loading-error-scroll-1280x720-20261028-045022-365.json) | [PNG](screens/plan013-screen-loading-error-scroll-720x1280-20261028-045022-417.png) · [JSON](screens/plan013-screen-loading-error-scroll-720x1280-20261028-045022-417.json) |
| 저장 복원 실패·명시적 복구 | [PNG](screens/plan013-screen-restore-error-1280x720-20261028-045022-467.png) · [JSON](screens/plan013-screen-restore-error-1280x720-20261028-045022-467.json) | [PNG](screens/plan013-screen-restore-error-720x1280-20261028-045022-507.png) · [JSON](screens/plan013-screen-restore-error-720x1280-20261028-045022-507.json) |
| 멸망 연출·회전 보존 | [PNG](screens/plan013-screen-destruction-rotation-1280x720-20261028-045440-909.png) · [JSON](screens/plan013-screen-destruction-rotation-1280x720-20261028-045440-909.json) | [PNG](screens/plan013-screen-destruction-rotation-720x1280-20261028-045440-952.png) · [JSON](screens/plan013-screen-destruction-rotation-720x1280-20261028-045440-952.json) |
| 결과·저장 실패·재시작/재시도 | [PNG](screens/plan013-screen-results-save-error-1280x720-20261028-045441-007.png) · [JSON](screens/plan013-screen-results-save-error-1280x720-20261028-045441-007.json) | [PNG](screens/plan013-screen-results-save-error-720x1280-20261028-045441-042.png) · [JSON](screens/plan013-screen-results-save-error-720x1280-20261028-045441-042.json) |

종료 화면 4장은 렌더 수정 후 04:54:40~04:54:41 UTC 실행에서 생성했다. Destruction은 양방향 모두 `endingElapsed=0.2`, `currentRequest=true`; Results는 모두 `endingElapsed=0`, `currentRequest=true`다. 정상 시간 진행과 회전을 분리하려고 연출 검사에서는 공유 시계만 정지시켰으며, 실제 GameDataStore·세션·요청 동일성은 테스트가 검사한다.

## 정적 연결 감사

2026-10-28 정적 감사 시점의 확인이다. 빌드 도중 패키지가 쓰는 임시 설정과 최종 변경 목록은 별도로 판정한다.

- 신규 C# 15개(표현 7·Editor 2·검사 6)와 전체 프로젝트 C# 132개 모두 `.meta`가 있으며 누락·고아 script meta가 없다. 빌드 임시 Resources 폴더 정리 후 Assets의 meta 254개에 누락·고아·중복 GUID가 없고 기존 추적 `.meta` 238개는 기준 HEAD와 동일했다. [최종 소스 감사](source-audit.json)를 따른다.
- Core·Data·Gameplay·GameFlow·Persistence·Presentation에 Editor/테스트 의존성이 없다. 실제 Laboratory/Bootstrap의 OnGUI·GUI·GUILayout 조작 경로는 제거됐으며 진단 씬의 IMGUI는 유지한다.
- 기존 실제 Laboratory 씬과 CSV·SO·공통 게임 로직·패키지 파일은 변경하지 않았다. 같은 활성 오브젝트의 View·Bootstrap 스크립트 GUID와 Registry 3개·CSV 5개 참조가 실제 파일로 해결된다.
- [진입 설정 실행](player-entry.json) 후 첫 활성 씬은 Laboratory이며 다음 SampleScene은 보존한다. APK용 씬 배열은 Laboratory 하나를 명시한다. Input System-only와 기존 네 방향 autorotation 설정은 유지한다.
- staging 전 기존 추적 파일의 `git diff --check` 오류는 없었다. 실패 빌드가 남긴 Performance Resources JSON과 자동 설정 차이는 [패치](first-build-generated-settings.patch)로 보존한 뒤 정리했다. 원래 변경이 없던 Rendering 설정 3개·ProjectSettings.asset만 기준 상태로 복원했다. 실제 변경인 EditorBuildSettings의 Laboratory 진입 설정은 유지했다. 원본 457개 파일이 영문 빌드 복사본 준비 시점과 동일함도 재확인했다.

main 커밋 전 신규 파일을 포함한 전체 `git diff --cached --check`는 줄 끝 공백 5곳을 보고했다. `UnityProject/Assets/Scripts/Tests/EditMode/Foundation/Presentation.meta` 6~8행은 Unity가 생성한 빈 속성 값이며, `first-build-generated-settings.patch` 22·139행은 보존한 원본 diff의 context다. 생성 메타와 증거의 원본을 유지했으며 전체 검사를 무경고 통과로 기록하지 않는다. 아래 두 파일 제외 검사는 통과했고, 소스·APK 해시에는 변경이 없다.

```text
git diff --cached --check -- . ':(exclude)UnityProject/Assets/Scripts/Tests/EditMode/Foundation/Presentation.meta' ':(exclude)docs/plans/evidence/PLAN-013/first-build-generated-settings.patch'
```

## Android 빌드 결과

원래 한글 경로의 빌드는 [BuildReport 증거](android-build-20261028-045459-075.json)에서 `Failed`, 오류 1, APK 크기 0, SHA-256 없음으로 확정됐다. 원인은 Android 도구의 비 ASCII 프로젝트 경로 제한이다. [비동기 job 결과](android-job-first-result.json)의 `completed`·`success=true`는 메뉴 호출의 완료이며 APK 성공을 뜻하지 않는다.

영문 경로에 독립 복사본을 만들고 원본 457개 파일의 SHA-256이 동일함을 [android-ascii-source.json](android-ascii-source.json)에 기록했다. 이 복사본에서 같은 Editor 빌드 메서드를 batch 실행하여 성공했다. 경로·프로세스·로그 위치는 [실행 정보](android-ascii-run.json), 정상 종료 코드 0은 [프로세스 종료](android-ascii-exit.json)에 있다. 원본의 경로·에셋·패키지·서명/보안 설정은 변경하지 않았다.

| 확인 항목 | 결과·증거 |
| --- | --- |
| BuildReport | `Succeeded`, 오류 0·경고 0, 2026-10-28 14:07:41~14:18:38 KST, 약 652초. [최종 보고서](android-build-20261028-050741-324.json) |
| 시작/포함 씬 | `Assets/Scenes/Gameplay/Laboratory/Temp_Laboratory.unity` 하나. Editor 설정의 SampleScene은 보존하지만 검증 APK에는 포함하지 않음 |
| 플레이어 | Development, IL2CPP, ARM64, Input System-only, 네 방향 autorotation |
| 패키지 | `com.DefaultCompany.urp_2d`, 최소 SDK 25·대상 SDK 36, `UnityPlayerGameActivity` 시작, `arm64-v8a`. [APK 직접 검사](android-package.txt) |
| 서명 | Android Debug 인증서, APK v2 서명 검증 통과. [서명 검사](android-signature.txt) |
| 실제 파일 | `UnityProject/Builds/PLAN-013/Temp_Laboratory-20261028-050741-324.apk`, 45,701,637 bytes. 작업 폴더로 복사 후 원본·보고서와 해시 일치. [산출물 검사](android-artifact.json) |
| SHA-256 | `e1678afd444741b7e43590707f5b5224324c950023bbca59610d5e60c2f2fe64` |

BuildReport의 `bytes`는 전체 빌드 보고 크기이며 APK 실파일 크기는 산출물 검사의 값이다. APK와 복사본의 Library는 Git 추적 대상이 아니다. 복사본과 로컬 빌드 로그는 재현용으로 보존했다. 초기 임포트에서 나온 Editor 전용 SpriteAtlas 스타일 파일 경로 예외·Editor Burst JIT 정책 차단·진단 전송 제한은 [별도 로그 발췌](android-import-diagnostics.txt)에 보존한다. 최종 APK BuildReport 성공과 초기 Editor 임포트 메시지를 구분하며, 로그를 없애려고 패키지나 OS 보안 정책을 변경하지 않았다.

## 남은 실기기 확인

2026-10-28 연결된 Android 기기가 없었고, 사용자가 “실기기 검증은 나중에 진행하겠습니다”라고 답했다. 실제 기기 1종의 APK 최초 실행·오프라인 Bootstrap → GameDataStore Ready → 플레이 진입, 가로·세로 터치·노치 표시·회전·백그라운드 복귀는 미검증이다. 이 항목은 빌드/PC 테스트 통과로 대체하지 않으며 PLAN은 active를 유지한다. 기기 준비 후 APK 해시와 기종·OS·화면 조건·로그/영상 및 조작 결과를 기록하여 PLAN의 필수 기기 시나리오를 수행한다.

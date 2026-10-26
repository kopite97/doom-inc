---
id: PLAN-004
type: development
status: completed
title: 공간·공통 조작 기반
updated: 2026-10-26
---

# PLAN-004 공간·공통 조작 기반

## 목표와 범위

Thing의 배치 변경을 공통 Map에 일관되게 반영하고, 최소 화면과 하네스가 같은 명령 처리·규칙 검사·상태 조회를 사용한다.
로드맵 0단계의 두 번째 묶음에 한정한다.

- 포함: 고정 직사각형 격자, Thing의 배치 여부·위치·90도 회전 정보와 Map 연결.
- 포함: 시설 정의의 점유 크기·고정 상호작용 상대 위치, 시설 중첩/영역 밖 검사, 이동 차단 정보.
- 포함: 배치·이동·회전 검사와 확정/취소를 처리하는 공통 명령·조회 경로.
- 포함: 대표 Thing을 준비하여 실제 배치 상태를 표시하고 같은 조작 경로를 호출하는 최소 PC 화면과 하네스.
- 제외: 자원 비용·작업을 포함한 실제 건설, 생산·수집·운반·예약·Job·경로 탐색 알고리즘.
- 제외: 침공·외부 활동, GameFlow·파일 저장/복원, 모바일 최종 화면과 성능 검증.

검증용 Thing 준비를 실제 초기 시작 흐름이나 무료 건설로 해석하지 않는다.
조작 중 상태는 공통 경로에서 관찰 가능하게 하되, 실제 연구소 일시정지는 담당 진행 시스템을 구현할 때 연결한다. 현재 없는 생산·이동의 정지 검증을 완료했다고 표시하지 않는다.

## 기준 문서와 선행 조건

- 선행: [PLAN-003](PLAN-003-thing-foundation.md)의 첫 기반 묶음 구현·검증 완료.
- 기획: [로드맵](../game-design/05-development/02-implementation/roadmap.md)의 D-086, [Thing 모델](../game-design/05-development/01-architecture/thing-model.md)의 D-076·D-077·D-079·D-084.
- 공간·화면: [시설 배치](../game-design/02-systems/03-space/placement.md)의 D-027~D-034, [화면과 조작](../game-design/03-player-experience/03-interface/controls.md).
- 기존 구조: [TECH-001](../decisions/TECH-001-thing-foundation.md)의 식별·컴포넌트·단일 자원 소속.
- 지침: [작업 절차](../agents/workflow.md), [아키텍처](../agents/architecture.md), [에셋](../agents/unity-assets.md), [하네스](../agents/harness.md), [검증](../agents/verification.md), [Unity 도구](../agents/unity-tools.md).

## 작업 목록

- [x] 기존 정의·Thing과 Map 배치 정보의 소유 경계 구체화
- [x] 격자·시설 점유·회전·고정 상호작용 위치와 배치 검사 구현
- [x] 공통 명령·검사·조회와 확정/취소 경로 구현
- [x] 최소 화면 연결과 실제 객체를 사용하는 하네스 시나리오 작성
- [x] Edit Mode 및 필요한 Play Mode 검증·증거 보관·문서와 로드맵 갱신

## 완료 기준과 검증 방법

| 기준 | 확인할 결과 |
| --- | --- |
| 배치 상태와 Map 일치 | 배치·이동·회전 후 점유 칸·상호작용 위치 일치, 이전 점유 제거 |
| 거부·취소 시 기존 상태 유지 | 시설 중첩·영역 밖 배치 거부, 확정 전 원래 배치 유지, 취소 시 원상태 |
| 승인된 공간 규칙 유지 | 직사각 시설의 네 방향 회전, 통로 차단 허용, 시설 차단과 생명체 비차단 구분 |
| 기존 Thing 상태 보존 | 이동·회전 후 ID·생성 순서·보관 자원의 소속과 수량 유지 |
| Map 배치와 존재 분리 | 배치 해제 후 Thing 존재 유지, 보관 자원에 Map 좌표를 강제하지 않음 |
| 화면과 하네스의 동일 경로 | 같은 조작의 허용/거부·결과 상태 일치, 화면은 실제 조회 결과 표시 |

실제 접근 가능성·접근 불가 시 생산 정지·운반 예외는 각 기능을 구현할 때 검증한다.
추가된 공간 상태의 파일 저장/복원은 세 번째 기반 묶음에서 확인한다. 이 PLAN만 완료해 1단계 실제 건설로 넘어가지 않는다.

## 현재 상황과 다음 행동

- 완료: Map·Thing 연결, 시설 크기·상호작용 회전, 공통 배치 명령과 검증용 화면을 구현했다. Edit Mode 29개·Play Mode 1개 최종 통과.
- 다음 기반 묶음은 한 판·저장이다. 새 PLAN을 작성하면 최초 상태는 proposed이며 사용자 명시적 승인 전 활성화하지 않는다.
- 이번 작업에서 실제 건설·생산·경로 탐색·파일 저장이나 Android 빌드·실기기 검증은 수행하지 않았다.

## 구현 위치와 사용법

| 담당 | 코드·에셋 |
| --- | --- |
| 공간 정의 | [FacilityDefinition](../../UnityProject/Assets/Game/Foundation/Definitions/FacilityDefinition.cs): 회전 전 점유 크기·고정 상호작용 상대 좌표 |
| 공간 원본과 조회 | [GridMap](../../UnityProject/Assets/Game/Foundation/Spatial/GridMap.cs), [MapPlacement](../../UnityProject/Assets/Game/Foundation/Spatial/MapPlacement.cs): Thing ID별 배치와 점유·상호작용·통행 조회 |
| 공통 조작 | [PlacementCommands](../../UnityProject/Assets/Game/Foundation/Spatial/PlacementCommands.cs), [PlacementResult](../../UnityProject/Assets/Game/Foundation/Spatial/PlacementResult.cs): 배치·해제·미리보기·확정·취소와 거부 이유 |
| Thing 연결 | [Thing](../../UnityProject/Assets/Game/Foundation/Things/Thing.cs)의 Map/배치 조회, [registry](../../UnityProject/Assets/Game/Foundation/Things/ThingRegistry.cs)의 논리적 제거 시 Map 정리 |
| 최소 PC 화면 | [MapFoundationView](../../UnityProject/Assets/Game/Presentation/MapFoundationView.cs), [준비 구성](../../UnityProject/Assets/Game/Presentation/MapFoundationScenario.cs), [검증 장면](../../UnityProject/Assets/Scenes/MapFoundation.unity) |
| 에셋 작성 | [Editor 메뉴](../../UnityProject/Assets/Game/Editor/MapFoundationSceneAssets.cs): Unity API로 씬 생성·기존 저장소 공간 필드 추가. 기존 GUID 유지 |
| 하네스 | [Map Edit Mode 검사](../../UnityProject/Assets/Tests/FoundationEditMode/GridMapTests.cs), [실제 씬 Play Mode 검사](../../UnityProject/Assets/Tests/SpatialPlayMode/MapFoundationViewTests.cs) |

검증용 장면은 12×8 칸, 저장소 2개, 보조형 2개, 유기물 5개를 사용한다. 실제 게임 시작 구성이나 건설 완료 흐름이 아니다.
대표 저장소의 초기 크기 3×2와 상호작용 상대 좌표 (1,-1)는 개발 위임 값이다. 최종 밸런스·건설 영역 크기를 확정하지 않는다.
화면의 편집 상태와 실제 Map 배치를 분리했고, 확정 전·실패·취소 시 기존 상태를 보존한다. `IsEditing`은 후속 진행 시스템의 정지 연결점이며 실제 생산 일시정지 검증은 아직 없다.

Unity 메뉴 `Tools > Mobile Game > Open Map Foundation Scene`으로 장면을 열고 Play한다. 시설 선택 → Move / Rotate → 격자 클릭·Rotate → Confirm/Cancel 순서로 조작한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command editor invoke_menu --path 'Tools/Mobile Game/Open Map Foundation Scene' --project-path $projectPath --json
unity command test run --mode editor --project-path $projectPath --json
unity command test run --mode playmode --filter MapFoundationViewTests --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
```

코드 변경 후 컴파일 완료 확인과 테스트 XML 보관은 [Unity 도구 절차](../agents/unity-tools.md)를 따른다.

## 검증 결과

| 날짜 | 대상·방법 | 실제 결과·증거 |
| --- | --- | --- |
| 2026-10-26 | Unity 6000.3.27f1·Android 대상 Editor / CLI·Pipeline 재컴파일 | 오류 없음. [최종 컴파일](evidence/PLAN-004/2026-10-26-view-repair-compile-result.json) |
| 2026-10-26 | 전체 Edit Mode / 실제 SO·Thing·Map | 총 29개 모두 통과: 신규 공간 15개 + 기존 기반 13개 + 환경 1개. [JSON](evidence/PLAN-004/2026-10-26-editmode-tests.json), [XML](evidence/PLAN-004/2026-10-26-editmode-tests.xml) |
| 2026-10-26 | 저장된 씬과 UI 조작 함수 / 같은 명령을 보내는 별도 실제 Map 하네스 | 최종 1개 통과. 이동·회전·거부·취소, 자원·ID 유지, 실제 OnGUI 렌더링 확인. [최종 JSON](evidence/PLAN-004/2026-10-26-playmode-final-status.json), [XML](evidence/PLAN-004/2026-10-26-playmode-final-tests.xml), [화면](evidence/PLAN-004/2026-10-26-map-view.png) |
| 2026-10-26 | 소스·메타·기존 설정·종료 상태 | 메타 누락·중복 GUID 없음, 기존 메타 변경 없음, 버전·패키지·SampleScene·URP 등 9개 기준 파일 유지. [통합 검사](evidence/PLAN-004/2026-10-26-foundation-verification.json) |
| 2026-10-26 | 문서 검사와 완료 상태 전환 | 완료 전 링크·줄 수·상태 검사 통과. 완료 후 승인 대기로 active가 0개여서 해당 조건만 오류로 보고됨. [완료 전](evidence/PLAN-004/2026-10-26-doc-validation-before-completion.log), [완료 후](evidence/PLAN-004/2026-10-26-doc-validation.log) |

최초 Play Mode 실행은 테스트 자체의 진단 `Debug.Log`가 `LogAssert.NoUnexpectedReceived`에 걸려 실패했다([원본](evidence/PLAN-004/2026-10-26-playmode-status.json), [XML](evidence/PLAN-004/2026-10-26-playmode-first-run.xml)). 불필요한 진단 로그를 제거했고 검사 조건은 유지했다. 시설 표시 대비도 조정한 후 재컴파일·동일 통합 테스트를 통과했다.
마지막 Editor는 ready·Play 종료·컴파일 완료·runInBackground=false이며 현재 Unity 콘솔 오류/경고는 0개다([상태](evidence/PLAN-004/2026-10-26-editor-final.json), [콘솔](evidence/PLAN-004/2026-10-26-console-final.json)). 과거 Pipeline QuickInstaller 오류 이력은 보존하며 근본 해결로 기록하지 않는다.
UI 테스트는 실제 버튼이 사용하는 조작 함수를 호출하고 렌더링을 확인했다. OS 마우스 이벤트 주입·모바일 터치·전체 게임 사이클 검증은 아니다.
파일 저장/복원은 다음 기반 묶음에서 ID·생성 순서·Map 배치·자원 소속과 함께 검증한다. 다음 PLAN을 자동 활성화해 문서 검사 오류를 없애지 않는다.

## 관련 결정과 상태 전환

- 관련 기술 결정: [TECH-001](../decisions/TECH-001-thing-foundation.md), [TECH-003](../decisions/TECH-003-map-and-placement.md).
- 2026-10-26: 승인된 구현 순서에 따라 후보를 구체화하고 PLAN-003 완료와 함께 proposed → active. 기능 구현 미착수.
- 2026-10-26: 사용자 요청의 저장소 설정을 위해 active → on hold. PLAN-005 완료 후 재개한다.
- 2026-10-26: PLAN-005 저장소 설정·검증 완료 후 on hold → active. 기능 구현은 시작하지 않았다.
- 2026-10-26: 최초 커밋·업로드 요청에 따라 active → on hold. PLAN-006 완료 후 복원한다.
- 2026-10-26: PLAN-006 최초 업로드·검증 완료 후 on hold → active. 기능 구현은 시작하지 않았다.
- 2026-10-26: 사용자 명시적 승인: “PLAN-004의 실행을 승인할게. 기존 확정 기획을 기준으로 Map과 공통 조작 기반 구현을 시작하고, 검증 결과와 진행 상황을 PLAN에 반영해줘.” 기존 active 상태를 유지하고 승인 범위의 구현을 시작한다.
- 2026-10-26: Map·공통 명령·최소 화면·최종 테스트·증거·문서 정리 완료로 active → completed. 다음 PLAN 실행 승인이 없으므로 active 0개로 대기하며 자동 활성화하지 않는다.

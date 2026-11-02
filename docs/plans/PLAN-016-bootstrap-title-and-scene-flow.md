---
id: PLAN-016
type: development
status: completed
title: Bootstrap·Title·Laboratory 씬 흐름과 Editor 직접 진입
updated: 2026-11-02
---

# PLAN-016 Bootstrap·Title·Laboratory 씬 흐름과 Editor 직접 진입

## 목표

제품을 실행하면 Bootstrap에서 공통 데이터를 준비하고 Title에서 새 게임·이어하기를 선택해 Laboratory로 진입한다. 씬 전환은 영속 싱글톤 SceneFlowManager가 조율하고 한 판의 진행은 기존 GameFlowManager가 담당한다. 개발 중에는 Editor에서 Title 또는 Laboratory를 바로 실행하면서 제품과 동일한 초기화·세션 경로를 검증할 수 있게 한다.

## 범위

- 포함: Bootstrap·Title 씬 추가, 기존 Laboratory 연결 정비, 공통 초기화 구성, 단일 씬 전환 관리자와 세션 수명 정리.
- 포함: 새 게임·이어하기·설정·메뉴 복귀 UI, 초기화/저장/전환 실패와 재시도, 가로·세로 및 Safe Area 대응. 기존 UI와 임시 에셋을 우선 재사용한다.
- 포함: 초기 데이터 준비와 씬 전환에 재사용하는 공용 LoadingScreen, SceneFlowManager의 코루틴 기반 로딩 조율, 전환 UI·입력 시스템과 미저장 표시 설정의 씬 간 수명 보장. 별도 Loading 씬은 추가하지 않는다.
- 포함: Editor 현재 씬/전체 시작 흐름 선택, 테스트 슬롯과 초기 상태 선택, 기존 Diagnostics·씬 생성 도구·테스트의 새 초기화 경로 연결.
- 포함: 제품 빌드 씬 목록과 빌드 도구의 씬 참조 정비, 정적 검사·Editor 컴파일·Edit Mode·Editor Play Mode 런타임 회귀 검증.
- 제외: 빌드 테스트·APK/AAB·PC Player 생성·Player 스크립트 컴파일·IL2CPP·새 실기기 설치/검증. 빌드 설정과 도구를 수정해도 빌드를 실행하지 않는다.
- 제외: 정식 아트 제작·교체, 로비·스테이지 선택·계정·클라우드 저장·다중 저장 슬롯 UI·Google Sheets 연결, 저장 스키마/밸런스 변경, 종합 성능 최적화.
- 요청 근거: 사용자가 씬별 책임·싱글톤 전환 관리자·Editor 직접 진입 원칙을 지침에 반영하도록 승인했고, 이어 이 흐름 구현을 위한 PLAN 작성을 요청했다. 2026-11-02 사용자가 구현 실행을 승인하고 빌드 테스트 제외를 재확인했다.
- 후속 요청: 검토의 두 보완 사항 반영과 공용 LoadingScreen 용어를 확인했다. 사용자가 기존에 사용하던 코루틴 방식을 이번 로딩 조율 설계에 반영한다. PLAN 수정 요청이며 구현 실행 승인과 구분한다.

## 기준 문서와 선행 조건

- 선행: [PLAN-007](PLAN-007-gameflow-and-persistence.md)·[PLAN-011](PLAN-011-destruction-results-and-restart.md)·[PLAN-014](PLAN-014-mobile-ui-ux-and-facility-status.md)·[PLAN-015](PLAN-015-topic-json-definition-data.md) completed.
- 로드맵: [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md)의 기존 게임 흐름과 데이터 기반 후속 정비다. 이 PLAN 완료를 4단계 종합 성능·실기기 최종 판정으로 해석하지 않는다.
- 기획: [GameFlow](../game-design/05-development/01-architecture/game-flow.md)의 D-085, [플레이 환경](../game-design/04-delivery/01-platform/environment.md)의 D-069·D-090, [화면과 조작](../game-design/03-player-experience/03-interface/controls.md), [Thing과 정의 데이터](../game-design/05-development/01-architecture/thing-model.md).
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](AGETNS.md), [씬 흐름](../agents/scene-flow.md), [프로젝트 구조](../agents/project-structure.md), [명명](../agents/naming.md), [스크립트 구조](../agents/script-structure.md), [아키텍처](../agents/architecture.md), [Unity 에셋](../agents/unity-assets.md), [하네스](../agents/harness.md), [검증](../agents/verification.md), [Unity 도구](../agents/unity-tools.md), [아트](../agents/art-assets.md).
- 선행 조건: 사용자 실행 승인을 받아 active로 전환했다. 아래 메뉴·저장 UX 승인 근거를 기획의 해당 규칙·결정 기록에 반영한다.

## 확인한 현재 구조와 변경 지점

| 착수 시 위치 | 확인한 사실·계획한 변경 |
| --- | --- |
| [GameBootstrap.cs](../../UnityProject/Assets/Scripts/Presentation/Bootstrap/GameBootstrap.cs) | JSON·Registry 결합 후 같은 GameObject의 Laboratory 또는 Diagnostics를 자동 초기화한다. 공통 데이터 준비와 씬별 세션 진입을 분리하고 제품 코드의 Diagnostics 타입 탐색을 제거한다. |
| [GameDataStore.cs](../../UnityProject/Assets/Scripts/Presentation/Bootstrap/GameDataStore.cs) | 영속 싱글톤, 동시 초기화 공유·실패 재시도·세대별 결과 무효화가 있다. 데이터 계약과 수명을 재사용한다. |
| [LaboratoryView.cs](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.cs) | Initialize에서 GameFlowManager를 만들고 StartOrContinue를 자동 호출한다. 명시적인 시작 요청과 성공/실패 결과로 바꾼다. |
| [GameFlowManager.cs](../../UnityProject/Assets/Scripts/GameFlow/Session/GameFlowManager.cs) | 새 판은 저장 교체 성공 후 공개하고 이어하기는 저장 단계로 복원한다. 씬 전환과 구분하며 종료·정지 경계를 보강한다. |
| [LaboratoryView.Navigation.cs](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Navigation.cs) | UiSettingsStore를 게임 저장 경로에서 파생한다. Title에서도 같은 설정을 읽고 쓰며 테스트 설정도 격리한다. |
| [EditorBuildSettings.asset](../../UnityProject/ProjectSettings/EditorBuildSettings.asset) | Laboratory가 첫 씬이고 SampleScene도 활성화돼 있다. Bootstrap을 첫 씬으로 하고 제품의 세 씬만 포함한다. |
| [LaboratorySceneAssets.cs](../../UnityProject/Assets/Scripts/Editor/Scenes/LaboratorySceneAssets.cs) 및 Editor/Validation | 생성 도구와 APK 도구의 기존 Laboratory 단독 경로를 조사·정비한다. 전용 Diagnostics 검증 목록은 제품 목록과 구분한다. |

## 설계 계약

### 씬과 공통 구성

아래는 생성·정비할 위치 명세다. 새 경로의 구현 완료를 뜻하지 않는다. `Assets/`는 `UnityProject/Assets/` 기준이다.

| 대상 | 예정 위치·처리 |
| --- | --- |
| Bootstrap | `Scenes/Gameplay/Bootstrap/Temp_Bootstrap.unity`: 공통 초기화와 로딩·실패 재시도 |
| Title | `Scenes/Gameplay/Title/Temp_Title.unity`: 새 게임·이어하기·기존 표시 설정 |
| Laboratory | 기존 `Scenes/Gameplay/Laboratory/Temp_Laboratory.unity`와 GUID 유지 |
| 앱 초기화·씬 전환 | `Scripts/Presentation/Bootstrap/`, `Scripts/Presentation/SceneFlow/`: Unity 연결·SceneFlowManager |
| 메뉴 UI | `Scripts/Presentation/Title/`와 필요한 주제별 UI 에셋 |
| 공용 로딩 화면 | `Scripts/Presentation/Loading/`, `Prefabs/UI/Loading/Temp_LoadingScreen.prefab`: 초기화·씬 전환·실패 재시도에 재사용 |
| 직접 실행 도구·테스트 | `Scripts/Editor/Scenes/`, `Scripts/Tests/EditMode/`·`PlayMode/` 아래 해당 주제 |

- 공통 초기화 프리팹/구성 에셋 한 곳에서 JSON·Registry·씬 식별 참조를 공급한다. Bootstrap과 Editor 진입 도구가 같은 정규 구성을 사용하고 씬마다 목록을 복제하지 않는다. 정확한 에셋 형태·참조 방식은 구현 초기에 기술 결정으로 기록한다.
- Bootstrap은 GameDataStore 준비 성공 후 Title 진입만 요청한다. Title에서 기다리는 동안 한 판을 생성하거나 생산·자동 저장을 진행하지 않는다.
- 제품은 Bootstrap에서 시작한다. Editor 도구는 현재 씬에서 같은 초기화 기능을 호출하고 Title/Laboratory를 바로 준비한다. 정상적인 Editor 직접 실행에 Bootstrap·Title 화면 경유를 요구하지 않는다.
- 제품 빌드 씬 순서와 빌드 도구의 목록은 Bootstrap·Title·Laboratory로 일치시킨다. SampleScene·Diagnostics·템플릿을 제외하며, 도구의 런타임 검사 전제도 새 흐름에 맞추되 빌드 호출은 하지 않는다.

### SceneFlowManager와 세션 수명

- SceneFlowManager는 DontDestroyOnLoad 기반 싱글톤 MonoBehaviour로 구성한다. GameDataStore와 각각 하나만 유지하고 중복 객체가 정상 인스턴스의 참조·수명을 파괴하지 않게 한다.
- 새 게임/이어하기·대상·저장 슬롯·요청 식별자를 명시한다. 중복 버튼 입력과 오래된 콜백은 새 전환을 만들거나 다른 세션에 적용되지 않는다.
- 데이터 준비 → 세션 준비/복원 → 화면 사용 가능의 성공 경계를 구분한다. Laboratory Initialize 호출 완료만으로 세션 성공을 판정하지 않는다.
- 전환 중 입력·시뮬레이션·자동 저장을 적절히 정지하고, 저장 처리와 이전 세션 해제 순서를 보장한다. 앱 pause/focus와 전환 정지의 원인을 구분해 복귀 이벤트가 전환 중 플레이를 재개하지 않게 한다.
- 기존 세션의 이벤트·Ending 요청·입력·자동 저장·씬 참조를 해제한다. GameDataStore 카탈로그는 유지하고 일반 C# 게임 시스템이 씬 싱글톤에 의존하지 않게 한다.
- 취소 가능한 비동기 작업을 취소하고 취소 불가능한 씬 로드의 늦은 완료는 요청 식별·세대 검사로 처리한다. 실패 후 입력 잠금과 로딩 UI를 복구하고 재시도 가능한 상태를 남긴다.
- 제품 초기화 코드에서 Diagnostics 컴포넌트를 찾지 않게 한다. Diagnostics 측 어댑터가 공통 데이터 준비 후 각 진단 세션을 시작한다.

### 검토 보완 1: 공용 LoadingScreen·코루틴·입력 수명

- LoadingScreen은 Bootstrap 초기 준비, Title → Laboratory, Laboratory → Title의 공용 화면이다. 씬 전환에도 유지되는 앱 수명의 UI 루트 아래에 두고 이전/대상 씬 오브젝트를 부모로 삼지 않는다. Editor 직접 진입도 같은 화면을 사용한다.
- SceneFlowManager가 코루틴으로 화면 표시 → 공통 데이터 준비/필요한 저장 처리 → 씬 비동기 로드 → 대상 세션·화면 준비 확인 → 화면 숨김과 입력 허용을 조율한다. LoadingScreen은 상태·오류·재시도 입력을 표현하고 자체적으로 씬을 로드하지 않는다.
- 로딩 UI를 먼저 그릴 프레임을 확보한다. 기존 GameDataStore·데이터 소스의 비동기 Task 계약을 유지하고 코루틴에서 완료를 기다린 뒤 성공·오류·취소를 판정한다. 메인 스레드에서 Wait/Result로 미완료 작업을 블로킹하지 않는다. 코루틴 도입만으로 JSON 처리 등이 백그라운드 실행된다고 가정하지 않는다.
- 씬 로드 완료와 세션·UI 준비 완료를 모두 확인한 뒤 로딩 화면을 닫는다. 진행량을 알 수 없는 단계는 단계 문구/대기 표시를 쓰고 씬 로드 진행률을 전체 준비 완료율로 표시하지 않는다.
- 코루틴 중단은 Task나 이미 시작한 씬 로드의 취소 완료가 아니다. 관리자 종료·Play 종료·요청 교체 시 취소 신호와 요청 세대를 함께 관리하고 늦은 결과를 차단한다. 정상·실패·취소 경로마다 잠금·화면·작업 참조를 명시적으로 정리한다.
- 이전 씬 언로드 후 다음 씬 로드/세션 준비가 실패해도 오류·재시도 UI와 입력 시스템을 유지한다. 실패한 대상 씬의 UI나 데이터 카탈로그 준비 성공에 복구 화면 표시를 의존시키지 않는다.
- 앱 수명의 공용 UI 루트가 EventSystem·입력 모듈을 소유하도록 통일하고, 기존 LaboratoryUiCanvas의 생성/사용자 수/Dispose 경로를 이관한다. 제품 화면과 직접 실행에서 활성 EventSystem·UI 입력 모듈을 하나로 유지하고 개별 Canvas 파괴가 공용 입력을 제거하지 않게 한다.
- 전환 시작 시 포인터 누름·드래그·선택·Submit/뒤로 입력을 무효화한다. 다음 화면은 이전부터 눌린 입력의 해제/반복으로 동작하지 않으며 새로운 입력부터 받는다. 로딩 중에는 뒤 화면 입력을 막되 오류 화면의 재시도·복구 조작은 허용한다.
- Domain Reload 비활성의 Play 재진입에서도 공용 UI·EventSystem·코루틴·정적 참조가 중복되거나 이전 입력을 전달하지 않게 한다.

### 사용자 흐름과 저장 보호

| 상황 | 승인 동작·보존 조건 |
| --- | --- |
| 저장 없음 | Title에서 이어하기를 비활성화한다. 새 게임으로 초기 세션을 생성한다. |
| 정상 저장 | 이어하기를 선택해야 복원한다. Playing·Destruction·Results 등 저장된 단계와 기존 개체 관계를 유지한다. |
| 저장 탐색/복원 오류 | 오류와 재시도 경로를 제공한다. 검사 자체로 세션을 시작·저장하거나 손상된 저장을 지우지 않는다. 새 게임은 별도 명시 선택으로만 가능하다. |
| 기존 저장이 있는 새 게임 | 덮어쓰기 확인 후 기존 Restart의 저장 교체 성공 경계를 사용한다. 확인 취소·기록 실패 시 기존 저장을 보존한다. |
| 메뉴 복귀 | 플레이 및 결과 화면에서 확인 후 현재 상태 저장에 성공하면 Title로 돌아간다. 저장 실패 시 세션을 유지하고 재시도 또는 복귀 취소를 제공한다. |
| 배치·건설 미리보기 중 복귀 | 복귀 확인 취소 시 미리보기 유지. 복귀를 확정하면 공통 취소 명령으로 미확정 편집을 정리한 뒤 저장하며 시설·자원을 추가 확정하지 않는다. |
| 결과 화면 새 판 | 기존 종료 요청 유효성 검사와 재시작 경로 유지. 공통 데이터는 재사용하고 새 판의 상태만 교체한다. |
| 씬 로드/화면 준비 실패 | 오류 표시 후 재시도 또는 Title 복구. 새 판 저장이 이미 확정된 경우 해당 저장을 유지하고 이어하기로 복구하며, 이전 저장으로 되돌아갔다고 표시하지 않는다. |

- 플레이 환경 D-069의 “다시 실행하면 마지막 저장 상태에서 이어한다”는 자동 진입을 Title의 명시적 이어하기 선택으로 바꾸는 부분만 기록한다. 단일 슬롯·자동 저장·오프라인 진행 없음·기존 저장 형식과 의미 기반 호환 해시는 유지한다.
- 메뉴 복귀 전 확인·전환 중에는 게임 시간을 정지하고 취소 시 원래 단계·정지 조건으로 복귀한다. 편집 취소 뒤 저장이 실패한 경우 확정된 게임 상태를 유지하며 미리보기를 자동 재확정하지 않는다.
- 표시 설정은 기존 시설 상태 ON/OFF·큰 글자 옵션만 공통 패널로 제공한다. 타이틀↔인게임·새 판에서도 유지하고 저장 실패는 게임 저장과 구분해 재시도를 안내한다.
- 새 메뉴·확인창·오류·로딩 화면도 가로·세로·Safe Area·큰 글자에 대응한다. 회전·앱 복귀가 확인 중인 작업을 실행하거나 중복 요청하지 않게 한다.
- Android 뒤로/Editor Escape는 최상위 설정·확인창 닫기와 기존 인게임 취소 우선순위를 유지한다. Title에서 저장 삭제·새 판·앱 종료를 묵시적으로 실행하지 않는다. 전환 중에는 중복 이동을 막는다.

### 검토 보완 2: 미저장 표시 설정의 보존

- 같은 저장 슬롯의 UiSettingsStore 상태를 앱 실행 중 한 곳에서 소유하고 Title·Laboratory가 공유한다. 씬 진입마다 파일을 다시 읽어 메모리의 변경값을 덮어쓰지 않는다. 설정 소유는 읽기 전용 정의 카탈로그인 GameDataStore와 분리한다.
- 설정 변경은 즉시 화면에 적용한다. 파일 쓰기 실패 시 Current·HasUnsavedChanges·LastError와 재시도 가능한 최신 값을 씬 왕복·새 판 시작 후에도 유지한다. 게임 저장 성공을 설정 저장 성공으로 간주하지 않는다.
- 어느 화면에서든 재시도는 최신 변경값을 기록한다. 성공 후 오류와 미저장 상태를 해제하고, 이전 씬의 오래된 콜백이 설정을 되돌리거나 이전 값을 저장하지 못하게 한다.
- 테스트 슬롯 변경·Play Mode 재시작 시 이전 슬롯의 값·오류·구독을 분리한다. 이전 슬롯의 미저장 값을 새 슬롯에 기록하지 않는다. 파일 기록 실패 후 앱/Play를 종료했다면 다음 실행까지 보존됐다고 표시하지 않는다.

### Editor 직접 진입

- 현재 씬 실행 / 전체 시작 흐름 실행을 선택하고, Laboratory에서는 새 테스트 판 / 테스트 저장 이어하기를 명시한다. 저장 없음·복원 실패는 제품과 같은 처리 경로를 따른다.
- 기본 초기 상태와 필요 시 기존 하네스로 준비한 운영/종료 근접 시나리오를 선택한다. 준비용 주입을 사용자 조작 검증으로 집계하지 않고 선택한 시나리오·슬롯을 기록한다.
- 모든 Editor 실행 모드에서 게임 저장과 표시 설정을 제품 경로와 격리한다. 일반 Play 버튼 경로도 안전한 테스트 기본값으로 진입하며 실제 저장을 기본 슬롯으로 사용하지 않는다.
- 초기화 도구와 씬 Start가 경쟁하지 않게 시작 요청을 한 번만 소비한다. 제품 구성 누락은 오류로 보여 주고 임의의 다른 데이터로 대체하지 않는다.
- Domain Reload 활성/비활성, 지원하는 Scene Reload 설정에서 반복 진입해 정적 참조·이벤트·카탈로그·요청 수명을 확인한다. 이전 Play의 실패·비동기 완료가 새 Play에 적용되지 않게 한다.
- 시작 씬 임시 설정은 종료·실패·에디터 재시작 시 복구 가능하게 하고, 편집 중인 씬 구성을 보존한다. 미저장 변경을 버리거나 제품 씬/빌드 목록을 디버깅 편의로 저장하지 않는다.
- Editor 전용 설정과 준비 도구를 런타임 어셈블리에서 분리한다. 제품에 포함되는 에셋 참조와 코드에 Editor/Tests/Diagnostics 의존이 없는지 정적으로 확인한다.

## 작업 목록

- [x] 기존 초기화·세션·저장·표시 설정·Diagnostics·씬 생성/빌드 도구의 호출부와 테스트 전제를 조사하고 이관 목록을 확정한다.
- [x] 승인된 진입·메뉴·저장 UX를 D-091과 플레이 환경에 반영하고 [TECH-013](../decisions/TECH-013-app-scenes-and-loading-screen.md)에 공통 구성·수명 선택을 기록했다.
- [x] 공통 데이터 준비와 세션 시작을 분리하고 GameDataStore·Diagnostics의 기존 계약을 유지한다.
- [x] SceneFlowManager와 명시적 시작 요청, 전환 잠금·실패 복구·종료 정리를 구현한다.
- [x] 공용 LoadingScreen과 코루틴 기반 로딩 조율, EventSystem 소유·이전 입력 무효화, 씬 제거 후에도 가능한 오류 복구를 구현한다.
- [x] Bootstrap·Title 씬과 가로·세로 임시 UI를 만들고 Laboratory의 새 게임·이어하기·메뉴 복귀·결과 재시작을 연결한다.
- [x] 공통 표시 설정·오류/확인/뒤로 처리, 미리보기와 pause/focus 경계를 연결한다.
- [x] 표시 설정을 슬롯별 공통 상태로 연결하고 저장 실패 후 씬 왕복·새 판·최신 값 재시도와 슬롯/Play 격리를 검증한다.
- [x] Editor 직접 실행·전체 흐름 선택·슬롯/설정 격리·시나리오 준비·반복 Play 초기화를 구현한다.
- [x] 씬 생성 도구·제품 씬 목록·빌드 도구 참조와 기존 테스트를 이관하고 깨진 GUID/참조를 검사한다. 빌드는 실행하지 않는다.
- [x] 아래 Edit/Play 회귀·실패 시나리오·양방향 화면 검증을 수행하고 증거와 미검증 범위를 기록한다.
- [x] PLAN·목록·로드맵·기획 MAP·실제 구현과 달라진 지침을 동기화한다. 필요한 Sprite 요청은 아트 지침에 따라 별도로 기록한다.

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 제품 전체 흐름 | Editor에서 Bootstrap → Title → 새 게임/이어하기 → Laboratory → Title → 재진입 | 씬 순서·세션 단계·초기화 횟수·저장 결과 |
| 초기화·전환 안전성 | 로딩 지연/실패/재시도, 연속 탭, 씬 준비 실패, 전환 중 pause/focus, 늦은 완료 | 단일 영구 객체·요청 소비·실패 후 복구 결과 |
| 공용 로딩·입력 수명 | 이전 씬 제거 후 실패/재시도, 코루틴 중단·Task/씬 로드 지연 완료, 누른 채 전환·드래그·Submit 유지, 반복 왕복/Play | 로딩·오류 UI 유지, 활성 EventSystem/입력 모듈 각 1개, 다음 화면 오조작 없음·잠금 복구 |
| 미저장 표시 설정 | Title에서 큰 글자/상태 표시 변경 후 쓰기 실패 → Laboratory → 새 판 → Title → 최신 값 저장 재시도, 테스트 슬롯/Play 변경 | 설정값·오류·미저장 상태 유지, 재시도 후 파일 일치, 슬롯 간 오염 없음 |
| 저장 보호 | 저장 없음·손상·호환 불일치·읽기/쓰기 실패, 덮어쓰기 취소·성공·실패, 편집 중 복귀 | 저장 파일 전후 비교·세션/설정 유지·재시도 결과 |
| 수명 정리 | 반복 메뉴 왕복·새 판, 이전 세션/Ending 콜백 호출, Title 체류 | 이전 세션 진행·자동 저장 없음, 구독·참조 정리 |
| 직접 실행 | 현재 Title/Laboratory와 전체 흐름, 테스트 새 판/이어하기, Play 반복·Domain Reload 양쪽 및 Scene Reload 조건 | 초기화/세션 단일성·슬롯/설정 격리·Editor 설정 복구 |
| 기존 게임 회귀 | 생산·운반·배양·배정·침공·보고·저장 복원·멸망·결과·재시작 후 두 번째 종료, Diagnostics 회귀 | Edit/Play 결과·실행 수·상태 관찰·오류 로그 |
| 화면·입력 | Editor의 가로/세로·Safe Area·큰 글자, 메뉴/설정/확인/오류/로딩, 기존 맵·아이콘·시설 상태 UI | 화면과 조작 관찰 기록, 회전/복귀 중 요청 보존 |
| 참조·문서 일관성 | Editor 임포트/컴파일, 제품 씬 목록·도구 참조·Editor 의존 정적 검사, 문서 검사 | 누락 참조 없음·검사 결과·빌드 미실행 기록 |

development 완료는 위 구현과 필수 Editor 검증 충족으로 판정한다. 정식 아트와 새 Android 빌드·실기기 확인은 완료 조건에서 제외하고 한계를 명시한다. 실제 터치·회전 센서·플랫폼 패키징은 이번 Editor 결과로 검증됐다고 기록하지 않는다.

## 현재 상황과 다음 행동

- 마지막 확인: PLAN-016 completed. Bootstrap·Title·Laboratory, 공용 코루틴 LoadingScreen, 저장 보호·설정 공유·실패 복구, Editor 직접 진입/시나리오 준비를 구현하고 필수 Editor 검증을 완료했다.
- 이번 작업: 공통 AppConfiguration과 영속 SceneFlowManager/AppUiLifetime을 연결했다. 제품 씬 3개만 포함하도록 정비했으며 기존 Laboratory GUID·저장 형식·JSON 데이터 계약을 유지했다. 착수 전 미커밋 지침·검사기 변경은 보존했다.
- 다음 행동: 사용자 후속 요청 대기. 새 Android 빌드·실기기 검증은 이번 완료 범위에 포함하지 않았다.
- 막힌 점: 기능·필수 검증에는 없음. 임시 Editor SlotB 파일 정리는 자동 승인 검토에서 정책상 차단되어 파일을 남겼고 원래 Default 슬롯 선택은 복원했다. 상세는 검증 기록에 있다.
- 아트: 기존 임시 UI·에셋을 우선 재사용한다. 현재 신규 Sprite 요청은 없으며 정식 아트 교체는 별도 art PLAN이다.

## 검증 결과

| 날짜 | 대상·환경·실행 방법 | 기대 결과·실제 결과 | 판정·증거 |
| --- | --- | --- | --- |
| 2026-11-02 | 문서·코드 읽기 | 현재 자동 진입·데이터/세션 경계·제품 씬 목록 확인 | 조사 완료, 위 변경 지점 참조 |
| 2026-11-02 | `node scripts/validate-agent-docs.mjs`, `git diff --check` | PLAN 16개·active 0개, 목록/상태·문서 링크·줄 수·공백 검사 정상 | PASS; 로드맵·기획 MAP 후보 상태도 대조 |
| 2026-11-02 | 초기 통합 Play 검증 | 첫 실행 0/5: 포커스 입력 무효화 반복과 실패 주입 타이밍 수정의 재컴파일 전 실행. 명시적 재컴파일 후 5/5 통과 | [초기 실패](evidence/PLAN-016/app-flow-first-result.json), [재검증](evidence/PLAN-016/app-flow-second-result.json) |
| 2026-11-02 | 전체 Play Mode, Domain Reload ON | 46/46 통과: 신규 앱 흐름 10개 + 기존 게임 회귀 36개 | [결과](evidence/PLAN-016/play-domain-on-result.json) |
| 2026-11-02 | 최종 Play Mode, Domain Reload ON/OFF | ON 전체 47/47, OFF 전체 46/46 후 추가한 취소 경계를 포함한 앱 흐름 11/11 통과 | [최종 ON](evidence/PLAN-016/play-final-result.json), [OFF 전체](evidence/PLAN-016/play-domain-off-result.json), [최종 OFF 앱](evidence/PLAN-016/app-final-off-result.json) |
| 2026-11-02 | 실제 Editor 메뉴·반복 Play·화면 | 현재 Title/Laboratory, 새 판/이어하기, Full Bootstrap Flow, Domain+Scene Reload OFF 반복, 가로·세로 큰 글자 화면 검증 | [실행·화면·실패 수정 기록](evidence/PLAN-016/README.md) |
| 2026-11-02 | 최종 Edit Mode | 294/294 통과. 초기 실패와 수정은 상세 기록에 보존 | [최종 Edit](evidence/PLAN-016/edit-final.json) |
| 2026-11-02 | 에셋·문서 마감 | 제품 3개 씬과 다섯 씬 JSON 참조 정상, 최종 Console 오류/경고 0, 문서·공백 검사 통과 | [상세 증거](evidence/PLAN-016/README.md), [Console](evidence/PLAN-016/final-console.json) |
| 2026-11-02 | 빌드·실기기 | 사용자 지시에 따라 미실행 | 범위 제외 |

## 관련 결정과 상태 전환

- 기술 결정: [TECH-013](../decisions/TECH-013-app-scenes-and-loading-screen.md).
- 2026-11-02: 신규 → proposed. 사용자 PLAN 작성 요청에 따른 후보 등록이며 실행 승인으로 간주하지 않는다.
- 2026-11-02: proposed 유지. 사용자 요청으로 전환 UI/입력 수명과 미저장 표시 설정 보존을 보완하고 공용 LoadingScreen·코루틴 로딩 방식을 명시했다.

- 2026-11-02: proposed → active. 사용자 “PLAN을 승인할게. 빌드 테스트는 필요 없어”에 따라 실행 승인과 빌드 제외를 기록한다.

- 2026-11-02: active → completed. 구현·필수 Editor 검증과 문서 상태 동기화 완료. 빌드·실기기·정식 아트 교체는 제외했다.

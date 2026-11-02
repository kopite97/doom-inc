---
id: PLAN-017
type: development
status: completed
title: 생산 판정·파일 저장·상태 계약·공용 UI 추상화 정비
updated: 2026-11-02
---

# PLAN-017 생산 판정·파일 저장·상태 계약·공용 UI 추상화 정비

## 목표

추상화 검토에서 확인한 네 영역을 정비하여 같은 규칙의 중복 수정과 특정 구현·표시 문구에 대한 의존을 줄인다.
기존 게임 규칙·저장 호환성·가로/세로 UI·씬 흐름을 유지하고 Editor 런타임 회귀로 확인한다.

## 범위

- 포함: 생산 실행/상태 조회의 공통 판정, 파일 교체 절차와 저장 조회 결과 공통화, LoadingScreen·작업자 대기 상태의 명시적 계약, 이미 공용으로 사용 중인 UI의 의존성·이름·위치 정리.
- 포함: 영향받는 호출부·하네스·테스트·Editor 생성 도구·씬/프리팹 참조와 현행 기술 문서의 갱신, Editor 임포트/재컴파일·Edit Mode·Play Mode 및 실제 Editor 플레이 흐름 검증.
- 제외: APK/AAB·PC Player 생성, Player 스크립트 컴파일, IL2CPP 등 모든 빌드 테스트와 새 APK 설치·실기기 검증. 과거 APK 결과로 이번 변경을 검증했다고 기록하지 않는다.
- 제외: 게임 규칙·밸런스·콘텐츠·저장 형식 변경, 원격 데이터 공급원 구현, 성능 최적화·종합 성능 판정, 정식 아트 교체, 모든 Manager의 기반 클래스화, 범용 상태 머신·DI 프레임워크 도입.
- 작업 요청 근거: 2026-11-02 사용자 요청 — 검토의 네 가지 정비 사항으로 PLAN-017 작성, 런타임 테스트 포함·빌드 테스트 제외.
- 실행 승인: 2026-11-02 사용자가 검토 사항 반영과 PLAN 실행을 승인했다. 빌드 테스트 제외 조건을 유지한다.
- 아트: 기존 임의(Temp) 에셋을 유지한다. 신규·교체 Sprite 요청은 없으며 정식 아트 교체는 별도 작업이다.

## 기준 문서와 선행 조건

- 기획: [Thing·정의·공통 실행 경로](../game-design/05-development/01-architecture/thing-model.md), [GameFlow](../game-design/05-development/01-architecture/game-flow.md), [생산시설](../game-design/02-systems/01-production/facilities.md), [저장·운반](../game-design/02-systems/02-logistics/storage-and-transport.md), [보조형 작업](../game-design/02-systems/05-creatures/support-models.md), [화면·조작](../game-design/03-player-experience/03-interface/controls.md), [플레이 환경](../game-design/04-delivery/01-platform/environment.md).
- 로드맵: [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md)의 기존 1~3단계와 데이터·화면·씬 기반에 대한 후속 구조 정비다. 이 작업 완료로 4단계 종합 성능·모바일 최종 판정을 완료하지 않는다.
- 선행: completed인 [PLAN-009](PLAN-009-processing-cultivation-and-workers.md), [PLAN-014](PLAN-014-mobile-ui-ux-and-facility-status.md), [PLAN-015](PLAN-015-topic-json-definition-data.md), [PLAN-016](PLAN-016-bootstrap-title-and-scene-flow.md)의 동작과 검증 기준.
- 설계 지침: [추상화](../agents/abstraction.md), [객체 설계](../agents/object-design.md), [아키텍처](../agents/architecture.md), [씬 흐름](../agents/scene-flow.md).
- 작업 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](AGETNS.md), [프로젝트 구조](../agents/project-structure.md), [스크립트 구조](../agents/script-structure.md), [명명](../agents/naming.md), [Unity 에셋](../agents/unity-assets.md), [하네스](../agents/harness.md), [검증](../agents/verification.md), [Unity 도구](../agents/unity-tools.md).
- 착수 조건: 사용자 실행 승인 후 유일한 active로 전환하고 PLAN 목록·로드맵·기획 MAP을 함께 갱신한다. 기존 미커밋 지침 변경은 보존한다.

## 확인한 구조와 정비 방향

경로는 `UnityProject/Assets/Scripts/` 기준이다. 새 타입명은 구현 시 책임에 맞춰 정하며 아래 방향을 범용 프레임워크 요구로 해석하지 않는다.

| 영역 | 현재 근거 | 정비 방향·줄어들 작업 |
| --- | --- | --- |
| 생산 판정 | `Gameplay/Production/ProductionManager.cs`의 CanOperate·WaitReason과 `FacilityStatusQuery.cs`가 생산 조건과 대기 우선순위를 각각 검사 | 같은 생산 조건을 한곳에서 판정하여 규칙 변경 시 실행·표시의 동시 수정 누락 방지 |
| 파일 저장·조회 | `Persistence/Storage/JsonFileSessionStore.cs`와 `UiSettingsStore.cs`의 임시 파일 교체 반복, TitleView·SceneFlowManager의 직접 파일 읽기 반복 | 파일 교체 도우미와 명시적인 조회 결과를 각각 분리하여 파일 처리 수정 위치 축소 |
| 상태 계약 | `Presentation/Loading/LoadingScreen.cs`가 버튼 문구로 화면 종류를 판단, `LaboratoryView.Details.cs`가 작업자 영문 메시지로 한글 설명 선택 | 의미가 명확한 상태 코드와 표시 문구를 분리하여 문구 변경 시 로직 수정 제거 |
| 공용 UI | Title·Loading·Bootstrap 오류 화면도 LaboratoryUi 계열을 사용하며 LaboratoryUiList가 부모 LaboratoryView를 탐색 | 이미 공유된 화면 구성요소의 공용 책임을 드러내고 스타일 값을 전달하여 특정 화면 탐색 제거 |

### 1. 생산 판정 통합

- ProductionManager가 사용하는 실행 가능 여부와 차단 사유를 하나의 판정 경로로 정리한다. 작은 함수·결과 값 또는 구체 클래스를 우선하며 인터페이스를 의무화하지 않는다.
- CanOperate·WaitReason·FacilityStatusQuery의 호출부와 관련 테스트를 먼저 대조한다. 건설 미완료·미입고 완료품·대상 미선택·접근 불가·현재 회차 재료 대기의 우선순위를 보존한다.
- 회차 진행 중에는 이미 소비한 재료를 다시 요구하지 않는다. 배양 대상 변경은 다음 회차에만 적용하고 완료품 수령과 실제 입고 완료를 구분한다.
- 입력 수량의 기존 불변 조건과 실행/표시 조건의 차이를 확인한다. 잘못된 상태를 정상으로 완화하거나 비교 연산자를 임의로 바꾸어 판정을 통일하지 않는다.
- 상태 조회는 공통 판정에 작업자 수·진행률·재료/예약 관찰을 결합한다. 실행 로직이 UI 문구나 화면 전용 타입에 의존하지 않게 한다.
- 생산 속도·갱신 순서·실행 상태 소유는 유지한다. 판정 결과를 별도 장기 상태로 저장하거나 캐시하여 두 원본을 만들지 않는다.

### 2. 파일 저장·조회 공통화

- 임시 파일 생성·UTF-8 쓰기·Flush·기존 파일 교체 또는 최초 이동·잔여 임시 파일 정리를 작은 공통 도우미로 분리한다. 실패 시 기존 파일을 먼저 삭제하는 우회 처리를 추가하지 않는다.
- 예외 계약을 통일하고 정리 실패가 원래 쓰기 실패를 가리지 않게 한다. 게임 저장과 표시 설정의 JSON 형식·경로·기본값·오류 표시·재시도 정책은 각 담당 객체에 유지한다.
- TitleView와 SceneFlowManager의 읽기 검사를 저장 계층의 `없음 / 읽기 성공 / 읽기 실패` 결과로 공통화한다. 경로는 명시적으로 전달하며 조회에 파일 생성·수정·삭제를 숨기지 않는다.
- 읽기 성공은 유효한 세션 복원을 뜻하지 않는다. 손상되었지만 읽을 수 있는 저장은 기존처럼 이어하기에서 복원 실패를 표시하고 새 게임 교체 전 확인을 받는다.
- 접근 거부·I/O 오류를 저장 없음으로 취급하지 않는다. 표시 이후 실제 실행 사이에 파일이 달라질 수 있으므로 조회 결과만 믿고 복원·새 게임의 오류 처리를 생략하지 않는다.
- 기존 ISessionStore와 UiSettingsStore의 주입 경계를 우선 재사용한다. 서로 다른 저장 정책을 하나의 범용 Store나 기반 클래스로 합치지 않는다.
- 새 게임/메뉴 복귀 저장 실패 시 기존 파일·세션 보존, 표시 설정 저장 실패 시 현재 선택·미저장 상태 유지, 슬롯 격리를 보존한다.

### 3. 문자열 상태 계약 정비

- LoadingScreen에 로딩·확인·오류 등 화면 의미를 명시적으로 전달하고 버튼 제목으로 화면 종류를 추론하는 비교를 제거한다.
- SceneFlowManager의 정상 준비·새 게임 확인·메뉴 복귀 확인·실패/재시도·복귀 취소 호출을 모두 이관한다. 버튼 동작·취소 경로·뒤 화면 입력 차단은 유지한다.
- WorkerComponent·WorkerManager·LogisticsManager와 표시/테스트/하네스의 대기 사유 사용처를 조사하여 의미가 명확한 코드로 연결한다. 초기/작업 중/대상 소멸/경로 차단/예약 변경 등 사유 설정·해제 시점을 보존한다.
- 영문 진단 메시지가 필요하면 코드에서 변환한다. 코드와 수정 가능한 문자열을 각각 상태 원본으로 보관하지 않는다. UI는 문구 역변환 없이 코드로 표시를 선택한다.
- 현재 작업자 대기 문구는 저장되지 않고 LaboratorySnapshotCodec 복원에서 비워 다음 판단으로 재계산한다. 코드 전환 후에도 해당 초기화·재계산 계약과 저장 호환성을 유지하며 저장 버전을 올리지 않는다.
- 미정의 코드의 처리와 기본 표시를 명시하고 테스트한다. LoadingScreen 상태와 작업자 대기를 하나의 범용 상태 체계로 합치지 않는다.

### 4. 공용 UI 경계 정리

- LaboratoryUiCanvas·LaboratoryUiWidgets·LaboratoryUiList·LaboratoryUiButton 및 연관 텍스트 구성요소의 실제 사용처를 조사한다. 이미 여러 화면이 공유하는 부분만 `Presentation/UI/` 아래 책임별 위치와 공용 이름으로 정리한다.
- LaboratoryUiList의 `GetComponentInParent<LaboratoryView>()`를 제거하고 스타일·최소 터치 크기·입력 무효화 세대를 호출부가 명시적으로 제공하게 한다. 연구소·결과·Title·Loading·Bootstrap 오류 화면의 호출부를 모두 연결한다.
- 안전 영역·화면 배율·글자 크기·최소 터치 크기의 갱신이 회전과 설정 변경 후에도 반영되게 한다. 생성 시 값만 전달하여 오래된 크기가 남지 않게 한다.
- 연구소 맵·시설 표식·도구 아이콘 등 연구소 전용 구성은 그대로 분리한다. 공용 UI가 LaboratoryView나 게임 세션을 탐색·소유하지 않게 한다.
- 기존 화면 레이아웃과 시각 표현은 보존한다. 행 키 재사용·스크롤 위치·누를 때의 동작/맥락 캡처·회전/비활성화/씬 전환 후 오래된 입력 차단을 유지한다.
- 앱 공용 EventSystem과 Canvas별 소유/Dispose 경계를 보존한다. Bootstrap의 구성 누락·진단 진입용 오류 화면을 제품 LoadingScreen과 무조건 합쳐 복구 의존성을 만들지 않는다.
- 이동·명명 변경은 기존 `.meta`·GUID를 보존한다. MonoBehaviour 클래스/파일·씬/프리팹·Editor 생성 도구·테스트의 참조를 확인하고 누락 스크립트가 없도록 검증한다.

## 작업 목록

### 검토 보완: 공용 텍스트 스타일과 반복 로딩 요청

- 공용 텍스트도 LaboratoryView 탐색 없이 기본 크기·폰트·현재 배율을 전달받는다. 기본 크기 × 배율 적용 책임을 한곳으로 모아 중복 확대를 방지한다.
- 보통→크게→보통 반복, 설정 변경 후 새 행 생성, 숨김/재표시와 지정 폰트 없는 오류 화면에서 글자 크기·기본 폰트를 확인한다.
- LoadingScreen은 같은 화면·같은 문구라도 새로운 Show 요청마다 이전 누름을 무효화한다. 화면을 숨기지 않은 요청 교체·동일 오류 반복에서도 이전 콜백이 실행되지 않고 새 입력은 한 번만 동작해야 한다.
- 확인→로딩→오류→재시도 전환에서 이전 버튼/콜백 제거와 입력 차단을 Play Mode로 검증한다.

- [x] 실행 승인·active 전환 및 관련 MAP 동기화, 기존 변경·환경·기준 동작 확인.
- [x] 네 영역의 호출부와 기존 테스트를 조사하고 변경 전/후 타입·경로·계약·대안을 기록.
- [x] 생산 공통 판정과 실행·상태 조회 연결, 관련 로직 회귀 검증.
- [x] 파일 교체 도우미·저장 조회 결과 공통화, 성공·실패·기존 파일 보존 검증.
- [x] LoadingScreen·작업자 대기의 명시적 상태 계약과 모든 호출부/복원 경로 이관.
- [x] 공용 UI 의존성·이름·위치 정리와 Unity 참조/생성 도구 갱신.
- [x] Editor 임포트·재컴파일, 영향받는 Edit Mode·Play Mode 테스트와 아래 런타임 시나리오 실행.
- [x] 기존 게임 사이클·저장 호환·씬 왕복·가로/세로·입력 회귀와 Domain Reload 양쪽 확인.
- [x] 실제 검증 결과·증거·한계·추상화 효과를 기록하고 완료 조건 충족 시 상태와 관련 MAP 갱신.

## 완료 기준과 검증 방법

런타임 테스트는 Unity Editor Play Mode와 실제 플레이 화면에서 수행한다. 정적 검사나 Edit Mode 통과만으로 런타임 완료로 판정하지 않는다.
빌드 테스트는 사용자 지시로 범위·필수 완료 조건에서 제외한다. Editor 결과를 Android 패키징·실제 터치·기기 성능 검증으로 확대 해석하지 않는다.

| 완료 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 생산 실행·표시가 같은 판정을 사용하며 기존 회차 규칙 유지 | ProductionTests·FacilityStatusQueryTests·관련 물류/저장 회귀. 대상 미선택, 재료 부족, 접근 차단, 소비 후 재개, 다음 회차 대상 변경, 완료품 수령/부분·최종 입고, 유인/무인 속도 | Edit Mode 결과·상태/수량/진행도 관찰 |
| 공통 파일 교체가 기존 파일과 정책 보존 | SessionPersistenceTests·UiSettingsStoreTests 및 필요한 공통 도우미 검사. 최초/기존 파일 쓰기, 실제 파일 교체 실패와 임시 파일 정리, 주입한 I/O 실패/재시도, 설정·게임 파일 독립성 | 테스트 결과·격리된 임시 경로의 전후 내용·예외 |
| 저장 조회와 시작/복귀 흐름 보존 | 없음·정상·읽을 수 있는 손상 파일·읽기 실패를 구분. 새 게임 확인/취소, 이어하기 복원 실패, 메뉴 복귀 저장 실패/재시도/취소, 조회 후 파일 변경 | Edit/Play 결과·기존 저장/세션 보존 기록 |
| 문구 변경이 상태 동작에 영향 없음 | LoadingScreen 버튼 문구를 바꿔도 로딩/확인/오류 제목·버튼 행동 유지. 작업자 대기 코드별 표시·사유 해제·운반/경로 변화·저장 복원 후 재계산 확인 | 코드 계약 검사·Play 화면/상태 기록 |
| 공용 UI의 특정 화면 의존 제거와 참조 정상 | 잔여 구체 화면 탐색·이전 이름 검사, Unity 임포트/재컴파일, 대상 씬·프리팹 로드 및 생성 도구 참조 확인 | 검색·컴파일 결과·누락 스크립트/참조 검사 |
| 가로/세로 표시·입력 보존 | Title/설정/로딩/확인/오류/연구소/결과를 양방향·큰 글자로 확인. 작은 도구 버튼, 시설 상태 ON/OFF·표식 재탭, 패널 스크롤·배치 미리보기 유지, 누른 뒤 회전/화면 교체/포커스 상실 시 오래된 입력 차단 | 관련 LaboratoryViewTests·AppSceneFlowTests 및 Editor 화면 캡처/조작 기록 |
| 공통 초기화·수명·전체 흐름 회귀 없음 | Bootstrap→Title→Laboratory, Title/Laboratory 직접 진입, 진단/구성 실패 오류 UI, 저장 후 메뉴 왕복, 멸망→결과→새 판. Domain Reload ON/OFF 반복 Play에서 입력 시스템·영구 객체 단일성·정리 확인 | Play Mode 결과·세션/저장 상태·로그 |
| 기존 저장·정의 계약 유지 | 기존 호환 테스트와 변경 전 저장을 격리 슬롯에서 복원. 정의 ID·해시·스냅샷 스키마·설정 형식 유지 | 호환 검사·저장 복원 결과 |
| 문서·상태·범위 일치 | 문서 검사기, git diff --check, 변경 파일·PLAN/로드맵/MAP 대조 | 검사 출력·변경 내역 확인 기록 |

- 기존 테스트를 우선 실행하고 이번 계약의 빈틈만 보강한다. 테스트 수 증가나 내부 클래스 분할 자체를 완료 기준으로 삼지 않는다.
- 실제 파일 연결과 실패 시 보존을 확인한다. 테스트 대역만 통과하고 실제 저장 경계 검증을 생략하지 않는다.
- 테스트 저장은 제품 저장과 분리하고 Editor 실행 설정은 종료·실패 후 복원한다. 실제 검증 시 결과 파일·로그·화면을 증거 위치에 연결하며 작성 단계에서는 빈 증거 폴더를 만들지 않는다.
- 각 영역에서 수정 위치·호출부 의존성·문구 변경 영향이 어떻게 줄었는지 실제 변경을 기준으로 기록한다. 영향이 큰 설계 선택만 기술 결정으로 분리한다.

## 구현 결과와 추상화 효과

| 영역 | 적용 결과 | 변경 작업이 줄어드는 지점 |
| --- | --- | --- |
| 생산 | ProductionReadiness·ProductionBlockReason과 ProductionManager.Evaluate를 실행/대기 설명/시설 상태 조회에서 공유 | 생산 차단 조건과 우선순위를 한곳에서 수정하며, UI는 판정 결과와 작업자/진행 관찰만 결합 |
| 파일 | AtomicTextFile과 SaveFileProbe·SaveFileState 도입 | 임시 파일 교체/정리 절차는 한곳, Title/SceneFlow의 없음·읽기 실패 분류도 한곳에서 수정 |
| 상태 | WorkerWaitReason·LoadingScreenKind 도입, 모든 설정/해제/복원/표시 호출 이관 | 번역·버튼 제목 변경 시 게임 상태 판정과 콜백 선택을 함께 수정할 필요 없음 |
| UI | UiCanvas·UiWidgets·UiList·UiButton·UiText를 Presentation/UI/Widgets로 정리 | 화면별 탐색 없이 폰트·배율·최소 터치 크기·입력 세대를 전달하며 새 행과 정적 글자가 같은 크기 적용 계약 사용 |

- 공용 UI 네 기존 스크립트와 meta를 함께 옮겨 GUID를 보존했고 UiList만 별도 파일로 분리했다. 씬/프리팹/Editor 도구/테스트 호출을 갱신했다.
- 게임 규칙·밸런스·JSON 정의·저장 버전/해시·영속 데이터 책임은 유지했다. 비정상 입력 과다는 실행 허용으로 완화하지 않고 상태 조회에서도 오류로 드러낸다.
- 범용 Manager 기반 클래스·Store 상속·상태 머신·DI 프레임워크를 추가하지 않았다. 구체 함수/작은 결과 값으로 실제 중복만 정리했다.
- 검증에서 발견한 다중 Game View 선택 문제를 검증 도구에서 수정했고 캡처를 Temp에 출력하도록 하여 과거 PLAN 증거를 보호한다.
- 실제 Play 종료의 화면 선행 파괴 예외는 SceneFlowManager의 Unity null 검사로 보완하고 해당 파괴 순서를 Play 회귀에 추가했다.

## 현재 상황과 다음 행동

- 마지막으로 확인한 사실: 네 영역과 두 검토 보완을 구현했다. Edit 302/302, 최종 ON Play 51/51, OFF 전체 50/51과 외부 진단 로그로 실패한 대상 1/1 재검증을 통과했다. 별도 실제 Play에서 이전 저장·큰 글자 양방향·결과/재시작·반복 진입/종료를 확인했다.
- 다음 행동: 없음. 완료 후 active 0개의 대기 상태이며 다음 작업은 별도 사용자 승인으로 진행한다.
- 막힌 점·미완료 필수 항목: 없음. 빌드·실기기·종합 성능 검증은 범위 밖이며 실행하지 않았다.
- Sprite 요청: 없음.

## 검증 결과

| 날짜 | 대상·환경·실행 방법 | 기대 결과·실제 결과 | 판정·증거 |
| --- | --- | --- | --- |
| 2026-11-02 | 작성 전 문서 검사기 | 기존 PLAN 16개·active 0개, 지침/상태/링크 검사 통과 | 통과: 작성 턴 명령 출력 |
| 2026-11-02 | 작성 후 문서 검사기·git diff --check·MAP 대조 | PLAN 17개·active 0개·목록 17개·파일 링크 2158개 검사 통과, proposed 및 빌드 제외 표기 일치 | 통과: 작성 턴 명령 출력 |
| 2026-11-02 | Editor 임포트·재컴파일·Edit Mode | 네 영역 구현, 최종 Edit 302/302 통과 | [실행 기록](evidence/PLAN-017/README.md) |
| 2026-11-02 | Domain Reload ON/OFF Play Mode | ON 51/51. OFF 전체 50/51, 외부 설정 로그가 끼어 실패한 종료 테스트는 로그 없이 1/1 재검증 통과. 건너뜀 없음 | [전체·수정 이력](evidence/PLAN-017/README.md) |
| 2026-11-02 | 실제 Editor Play·이전 저장·가로/세로·결과/새 판·종료 | 이전 저장 ID 보존, 14개 화면 확인, 세대 1→2·점수 0, 반복 OFF 진입에서 영속 객체 각 1개, 종료 오류/경고 0 | [실행 증거](evidence/PLAN-017/README.md#실제-editor-화면이전-저장-확인) |
| 2026-11-02 | Unity 참조·정적·문서 검사 | 씬 6개·프리팹 1개 Missing Script 0, GUID/원본 저장/Editor 설정 보존, 문서·diff 검사 통과 | [참조 검사](evidence/PLAN-017/asset-reference-audit.json), [정적 검사](evidence/PLAN-017/static-audit.json) |
| 2026-11-02 | 빌드 테스트·새 APK·실기기 | 사용자 범위 지정에 따라 제외 | 범위 제외, 플랫폼 동작 확인 아님 |

## 관련 결정과 상태 전환

- 이번 결정: [TECH-014 생산·저장·상태·공용 UI의 최소 공통 계약](../decisions/TECH-014-production-storage-and-shared-ui-contracts.md).
- 기존 결정: [TECH-013 앱 씬과 LoadingScreen](../decisions/TECH-013-app-scenes-and-loading-screen.md), [TECH-012 주제별 JSON 카탈로그](../decisions/TECH-012-topic-json-catalog.md)의 책임·호환 경계를 유지한다.
- 2026-11-02: 신규 → proposed. 사용자 요청으로 네 가지 추상화 정비와 Editor 런타임 검증을 계획했다. 실행 승인은 아직 받지 않았으며 빌드 테스트는 제외한다.
- 2026-11-02: proposed → active. 사용자 “검토 사항을 반영하고 PLAN을 승인할게”에 따라 두 보완을 반영하고 실행을 시작한다.

- 2026-11-02: active → completed. 구현·필수 Editor 회귀·실제 화면/저장/종료 확인과 설정 복원을 마쳤다. 검증 실패/수정/재검증을 증거에 남겼으며 빌드 테스트는 제외했다. 관련 MAP도 completed와 active 0개 대기로 동기화했다.

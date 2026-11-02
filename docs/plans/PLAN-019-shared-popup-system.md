---
id: PLAN-019
type: development
status: completed
title: 공용 Popup 시스템과 기존 확인·설정 화면 통합
updated: 2026-11-02
---

# PLAN-019 공용 Popup 시스템과 기존 확인·설정 화면 통합

## 목표

PopupBase 상속·프리팹·SO Registry·Action 호출 방식을 유지하면서 Title과 Laboratory에서 공용 확인·알림·설정 팝업을 사용한다. 팝업 추가 시 화면별 수명·입력·번역 처리를 반복하지 않고, 씬 전환·새 판·중첩·언어 변경에서도 오래된 동작이나 입력 누출이 발생하지 않게 한다.

## 범위

- 포함: PopupManager·PopupHost·PopupBase, 요청/결과/Handle·소유 범위, 공용/씬 Registry 결합·검증, ConfirmPopup·NoticePopup·SettingsPopup, 기존 확인·설정 UI 이관, LoadingScreen 연계, 한국어·영어와 가로·세로, 유지보수 설명 및 Editor 런타임 검증.
- 제외: 게임 규칙·저장 정책 변경, 모든 HUD/상세 패널/보고/결과 화면의 팝업화, 새 설정 항목, 토스트·툴팁 프레임워크, 무조건적인 요청 대기열·우선순위 스케줄러·풀링·DI 프레임워크, 새 패키지, 원격 에셋 로드, 정식 아트 제작·교체, 종합 성능 최종 판정.
- 빌드 테스트 제외: APK/AAB·PC Player·Player 스크립트 컴파일·IL2CPP·Addressables 콘텐츠 빌드를 실행하지 않는다. 새 빌드·실기기 검증은 필수 완료 조건에 포함하지 않으며 Editor 검증의 한계를 기록한다.
- 작업 요청·위임 근거: 사용자가 RNG의 Popup 구현과 비교한 설계 검토를 바탕으로 PLAN 작성을 요청했고, 2026-11-02 두 검토 보완 반영 및 PLAN-019 실행을 승인했다. 런타임 테스트를 포함하며 빌드 테스트는 명시적으로 제외했다.
- 아트: 기존 임시 UI·폰트를 재사용하고 새 임시 프리팹은 Temp_로 식별한다. 정식 아트 교체는 사용자 요청대로 별도 art PLAN에서 진행한다. 현재 신규 Sprite 요청은 없으며 실제 필요가 확인되면 아트 지침에 따라 요청을 기록한다.

## 기준 문서와 선행 조건

- 기획: [화면과 조작](../game-design/03-player-experience/03-interface/controls.md), [플레이 환경·저장 보호](../game-design/04-delivery/01-platform/environment.md), [GameFlow](../game-design/05-development/01-architecture/game-flow.md), [Thing과 정의](../game-design/05-development/01-architecture/thing-model.md). D-059·D-090과 기존 씬 흐름·저장 보호 규칙을 유지한다.
- 로드맵: [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md)의 4단계 종합 검증 전 공용 UI 기반 정비. [PLAN-014](PLAN-014-mobile-ui-ux-and-facility-status.md), [PLAN-016](PLAN-016-bootstrap-title-and-scene-flow.md), [PLAN-017](PLAN-017-production-storage-status-and-shared-ui.md), [PLAN-018](PLAN-018-localization-and-csv-text-management.md)은 completed다.
- 설계 지침: [아키텍처](../agents/architecture.md), [객체 설계](../agents/object-design.md), [추상화 기준](../agents/abstraction.md), [씬 흐름](../agents/scene-flow.md), [Localization](../agents/localization.md).
- 작업 지침: [절차](../agents/workflow.md), [PLAN 관리](AGETNS.md), [프로젝트 구조](../agents/project-structure.md), [스크립트 구조](../agents/script-structure.md), [명명](../agents/naming.md), [Unity 에셋](../agents/unity-assets.md), [아트](../agents/art-assets.md), [하네스](../agents/harness.md), [검증](../agents/verification.md), [Unity 도구](../agents/unity-tools.md).
- 기존 결정: [TECH-013 씬·LoadingScreen](../decisions/TECH-013-app-scenes-and-loading-screen.md), [TECH-015 Localization](../decisions/TECH-015-localization-and-csv-text.md). 구현 시 공용 팝업 수명과 기존 표시 책임의 변경 관계를 별도 기술 결정으로 기록한다.
- 비교 근거: [RNG PopupManager](https://github.com/S-D-Project/RNG/blob/develop/Assets/Game/Scripts/Presentation/UI/Popup/PopupManager.cs), [PopupBase](https://github.com/S-D-Project/RNG/blob/develop/Assets/Game/Scripts/Presentation/UI/Popup/PopupBase.cs), [ConfirmPopup](https://github.com/S-D-Project/RNG/blob/develop/Assets/Game/Scripts/Presentation/UI/Popup/ConfirmPopup.cs). 외부 구현은 비교 자료이며 현재 프로젝트의 의존성·지침에 맞게 설계한다.

## 설계와 구현 계약

### 1. 책임·객체 형태·배치

| 구성 | 형태·소유 | 책임 |
| --- | --- | --- |
| PopupManager | 앱이 소유하는 일반 C# 공유 객체 | 요청 검증, 스택·중복·종료 결과·소유 범위 관리. 별도 Instance나 서비스 로케이터를 추가하지 않음 |
| PopupHost | 앱 영속 UI 아래 MonoBehaviour | 프리팹 생성·제거, 표시 계층·배경 차단, 코루틴·Unity 수명 연결 |
| PopupBase와 구체 팝업 | MonoBehaviour 기반 클래스와 프리팹 | 공통 표시 절차, 개별 문구·컨트롤과 사용자 선택 전달 |
| PopupResource·PopupRegistry | 읽기 전용 구성 SO | ID와 프리팹 참조, 공용/씬별 목록. 콜백·스택·열림 상태를 저장하지 않음 |
| 요청·PopupHandle·종료 결과 | 일반 C# 값/객체 | 문구·인자·콜백·중복 키·소유자, 개별 요청 식별과 종료 관찰/취소 |
| 팝업 소유 범위 | 앱·씬·세션·부모 팝업에 연결 | 범위 종료 시 해당 요청·자식·참조 정리, 늦은 결과 차단 |

- 공통 초기화가 Manager와 Host를 생성·연결하고 화면에 참조를 전달한다. SceneFlowManager는 초기화·전환 경계를 연결하며 팝업 스택 알고리즘이나 개별 게임 동작을 소유하지 않는다.
- 추상화 근거는 Title/Laboratory의 확인·설정 반복, 공통 입력·번역 수명, Unity 표시와 요청 정책의 독립 검증이다. PopupBase는 공통 절차의 불변 조건을 보호한다. 인터페이스는 실제 Host 대체·검증 경계에 필요한 최소 범위만 검토한다.
- 예정 배치: 관리·표시는 Assets/Scripts/Presentation/Popup/, SO 클래스는 Assets/Scripts/Data/Popup/, SO 인스턴스는 Assets/SO/UI/Popup/, 프리팹은 Assets/Prefabs/UI/Popup/, 테스트는 Scripts/Tests의 해당 EditMode/PlayMode 기능 폴더다. 실제 생성 전 기존 어셈블리와 하위 분류를 확인한다.
- Data의 SO는 GameObject 프리팹 에셋을 참조하고 Presentation/Editor에서 PopupBase 유무·타입을 검증한다. Foundation에서 Presentation으로 역참조하거나 순환 어셈블리 의존을 만들지 않는다. UI Registry를 밸런스 JSON이나 GameDataStore로 복제하지 않는다.

### 2. Registry·준비·열기/닫기

- AppConfiguration에서 공용 Registry/Host를 연결한다. 씬 전용 Registry는 씬 진입 시 등록·퇴장 시 해제하며, 전용 팝업이 없는 씬에는 빈 에셋을 만들지 않는다. 공용 Registry는 Confirm·Notice·Settings를 제공한다.
- 공용/씬 합성 목록의 중복 ID·빈 항목·누락 프리팹·PopupBase 누락·예상 타입 불일치를 준비 단계에서 검사한다. 충돌을 임의 덮어쓰거나 일부 누락을 정상 준비로 공개하지 않는다. 잘못된 씬 등록은 기존 유효 목록을 훼손하지 않는다.
- 정상 UI 준비 전 요청은 명시적으로 거절한다. 단순 조회에서 초기화·씬 검색·파일 로드를 숨기지 않는다. 준비 실패는 독립된 초기 오류/재시도 경로로 연결한다.
- 생성 → 비활성 상태에서 데이터/의존성 연결 → 스택 등록·입력 차단 → 표시/열기 애니메이션 → 조작 허용 순서를 지킨다. Awake는 참조 준비만 수행하며 OnEnable/OnOpen에서 미설정 데이터에 접근하지 않게 한다.
- 공통 Open/Close 절차는 재정의하지 못하게 하고 필요한 OnOpen/OnClose 등만 확장한다. 팝업은 직접 Destroy/Close로 스택을 우회하지 않고 Manager에 선택·닫기를 요청한다.
- Opening/Open/Closing/Closed를 구분한다. 확인 즉시 입력을 잠그고 닫기 애니메이션 완료·스택 정리 후 결과를 전달한다. 코루틴은 unscaled 시간을 사용한다. 범위 종료·Host 파괴·Play 종료에서는 애니메이션을 기다리지 않고 멱등하게 정리한다.
- 생성·초기화·열기·닫기 실패 시 임시 인스턴스/구독/잠금을 회수하고 기존 팝업의 조작 가능 상태를 복구한다. 불완전한 요청을 스택에 남기지 않으며 핸들에 실패 결과를 남긴다.

### 3. 요청·콜백·중첩·소유 범위

- ShowConfirm 같은 편의 메서드에 LocalizedMessage·Action·소유 범위를 한 번에 전달한다. 전용 팝업은 전용 데이터로 구성하고 Open 후 SetData가 필요한 공개 사용 절차를 없앤다. Handle은 필요한 호출부만 보관한다.
- 종료는 확인·사용자 취소·호출자 취소·소유 범위 종료·표시 실패 등 원인을 구분한다. 모든 요청은 한 번만 종료하며 성공/사용자 취소 콜백과 자원 정리 알림을 구분한다. 강제 정리에서 사용자 취소용 게임 동작을 실행하지 않는다.
- 확인 콜백 실행 직전에 소유 범위·대상 세션/개체의 유효성을 재확인한다. 표시 중 상태가 바뀔 수 있으므로 실제 명령의 기존 규칙 검사도 유지한다. 닫기 완료와 후속 게임 동작의 성공은 별개이며 콜백 예외로 스택·입력 정리가 누락되지 않게 한다.
- 검토 보완 1: 확인 후 닫기 도중 앱 정지/포커스 상실이 발생하면 확인 의도를 보존하고 복귀 후 소유 범위를 재검증해 한 번만 전달한다. 대기 중 범위가 종료되면 무효화한다. 인계 전까지 입력 잠금을 유지하되 무효화·예외 시 반드시 회수하며, 기존 요청 함수의 포커스/프롬프트 검사 때문에 조용히 유실되지 않도록 연결한다. 일반 팝업 대기열과 구분한다.
- 비동기 저장/전환은 닫힌 팝업의 코루틴에 맡기지 않고 해당 기능 소유자가 수행한다. 취소 가능한 작업은 소유자 종료 시 취소하고, 취소 불가능한 늦은 완료는 세대 검사 등으로 차단한다. 이번 범위에서는 범용 비동기 작업 실행기를 추가하지 않는다.
- 동일 소유 범위+요청 키의 중복 열기는 기존 Handle을 반환하며 새 콜백으로 덮어쓰지 않는다. 무조건 대기열에 넣지 않는다. 중첩은 부모를 명시한 자식 요청에 허용하고 최상위만 조작한다. 무관한 요청은 Busy 등 명시적 결과로 거절해 호출부가 처리한다.
- 사용자는 최상위만 닫는다. 부모/범위의 강제 종료는 자식부터 정리한다. 콜백에서 새 팝업 열기·씬 전환을 요청하는 재진입에도 이미 정리된 상태를 보장한다.
- 공용 프리팹 사용과 요청 수명을 구분한다. Title 확인은 Title, 현재 판의 재시작/불러오기는 세션, 설정 위의 알림은 부모 팝업에 속한다. 앱 범위 요청은 초기 오류 복구 등 명시한 사용처로 제한한다.
- 씬 전환 및 같은 씬의 새 판/복원 성공 시 이전 범위를 종료한다. 실패하여 현재 판을 유지하면 현재 유효 범위를 무조건 폐기하지 않는다. 씬 연결·Registry 해제 순서와 실제 세션 교체 확정 시점을 기존 전환 코드에 맞춘다.

### 4. 입력·로딩·게임 진행

- 화면 전체 차단 영역은 Safe Area 바깥까지 덮고 팝업 내용은 Safe Area 안에 둔다. Canvas 차단과 별도로 맵 이동·줌·선택·건설·시설 표식·뒤쪽 단축키 경로를 공통 잠금에 연결한다.
- 뒤로가기/Escape는 로딩·전환 → 최상위 팝업 → 기존 화면 순서로 한 번만 처리한다. 닫을 수 없는 팝업도 입력을 소비한다. 각 Update에서 같은 입력을 중복 소비하지 않도록 기존 PollBack 경로를 정리한다.
- 바깥 터치는 기본적으로 차단만 한다. 바깥 터치 닫기는 명시한 화면에만 허용하고 확인 동작으로 해석하지 않는다. 키보드 포커스는 열린 팝업으로 제한하며 종료 후 유효한 이전 대상으로 복구한다.
- AppUiLifetime의 입력 세대·모든 포인터 해제 대기와 연결한다. 열기/닫기·레이아웃/언어 변경·앱 복귀 때 오래된 클릭과 드래그/핀치 상태를 무효화한다. 입력 무효화와 모달이 열린 동안의 지속 차단은 별도로 보장한다.
- 중첩 팝업·로딩·포커스 상실의 잠금은 소유자별로 해제한다. 한 팝업 종료가 다른 잠금을 풀지 않는다. 확인에서 LoadingScreen으로 넘어갈 때 뒤쪽 입력이 한 프레임도 열리지 않게 인계한다.
- PopupManager는 Time.timeScale이나 게임 진행을 직접 바꾸지 않는다. 메뉴 복귀 확인 등 기존 정지 정책과 편집/멸망 정지 원인을 보존하며 팝업 종료로 다른 원인의 정지를 해제하지 않는다.
- 멸망 단계 진입 시 세션 조작용 팝업을 무효화하고 연출/결과가 가려지거나 오래된 명령이 실행되지 않게 한다. 닫기 애니메이션 중 단계 변화도 같은 규칙을 적용한다.
- 검토 보완 2: 플레이 중 열린 Settings·그 자식 Notice·Confirm을 모두 플레이 범위에 연결하고 멸망 진입 경계에서 한 번만 정리한다. 이미 적용한 설정 및 앱 소유 언어 변경 작업은 보존하고 화면 구독만 해제한다. 결과 화면에서 새로 여는 메뉴 복귀 확인은 새 유효 범위에서 허용하며 매 프레임 반복 정리하지 않는다.
- 일반 확인은 Popup으로 이관하되 LoadingScreen은 로딩·초기화 실패·전환 실패 복구를 계속 담당한다. Popup/Localization 초기화 실패 표시가 Popup에 다시 의존하지 않게 한다. 로딩 중 일반 요청은 거절하고 복구 UI의 입력 우선권을 보장한다.

### 5. 기존 화면 이관·Localization·유지보수

| 사용처 | 이관 결과와 보존할 계약 |
| --- | --- |
| Title 새 게임 | 기존 저장 덮어쓰기 확인을 ConfirmPopup으로 이관. 취소·저장 읽기 실패 시 기존 저장 보존 |
| Laboratory 메뉴 복귀 | 확인 후 기존 저장/전환 경로 사용. 저장 실패 시 현재 판·저장 정책 보존, 재시도/복귀 취소 제공 |
| Laboratory 새 판·불러오기 | pendingSessionAction 표시를 공통 확인 요청으로 이관. 실패 시 현재 상태·오류 표시 유지, 성공 시 이전 세션 요청 정리 |
| Title·Laboratory 설정 | SettingsPopup에서 기존 시설 상태 표시·큰 글자·언어 선택을 재사용. 기존 저장 범위·실패/재시도·즉시 적용 의미 유지 |
| 일반 알림 | NoticePopup을 기존 세션 작업 실패 안내 등 대표 실제 사용처에 연결. 설정 저장/언어 준비 오류의 재시도 기능은 기존 설정 경로를 재사용 |
| 로딩·초기/전환 오류 | LoadingScreen 및 준비 전 내장 문구 경로 유지. 같은 확인을 구/신 UI가 동시에 소유하지 않게 제거/연결 |

- 실행 시 실제 이관/유지 목록을 확정하고 기존 GameFlow·저장·설정 명령을 호출한다. 저장 형식·슬롯 정책·생산/운반/방출 규칙은 변경하지 않는다. 설정에 새 Apply/Cancel 의미를 도입하지 않는다.
- 기존 uGUI·UiCanvas/UiButton/UiText·폰트 및 설정 행 구성을 재사용한다. TMP 전환이나 기존 모든 화면의 프리팹화는 하지 않는다. 프리팹과 Registry 참조를 바꾸어 외형을 교체할 수 있게 한다.
- 문구는 LocalizedMessage의 키·스냅샷 인자를 유지하고 기존 TextManager/Localization 연결을 사용한다. 새 문구만 주제별 CSV에 추가하고 미검토 번역·키 누락·에셋 참조 검사도 확장한다. SO에 번역 완성 문장을 복제하지 않는다.
- 가려진 부모 팝업도 언어 변경을 반영하고 입력값·선택·스크롤·확인 대상·콜백·애니메이션 진행을 보존한다. 구독은 화면 수명과 함께 해제하며 프레임 갱신과 구독의 이중 텍스트 소유를 피한다.
- 한국어/영어·가로/세로·큰 글자에서 긴 본문 스크롤과 버튼 접근, Safe Area, 회전 중 중첩·미리보기 보존을 확인한다. 언어 준비 실패·빠른 변경·팝업 종료 뒤 늦은 완료는 기존 Localization 계약을 따른다.
- 구현 완료 시 실제 폴더 README에 팝업 추가·Registry 등록·호출 예시·종료/소유 범위·문구 변경·임시 프리팹 교체 절차를 적고 기존 지침을 링크한다. Confirm 문구만 다른 경우 새 하위 클래스를 만들지 않는다.

## 작업 목록

- [x] 현재 확인·알림·설정·뒤로가기·입력 잠금·정지 이유의 이관/유지 목록을 작성하고 기술 결정에 책임·대안을 기록한다.
- [x] Registry·Resource·요청/결과/소유 범위·중복/실패 계약과 에셋 검증을 구현한다.
- [x] PopupManager·Host·Base, 코루틴 열기/닫기와 중첩·예외 정리를 구현한다.
- [x] Confirm·Notice·Settings 임시 프리팹을 만들고 공통 초기화·직접 씬 진입에 연결한다.
- [x] Title/Laboratory 확인·설정·대표 알림을 이관하고 기존 저장·정지·오류 복구를 보존한다.
- [x] LoadingScreen 우선권·공통 입력 차단·뒤로가기·세션/씬 수명 종료를 통합한다.
- [x] Localization·가로/세로·큰 글자·포커스·스크롤을 연결하고 CSV/테이블 참조를 검증한다.
- [x] Edit/Play 테스트와 실제 Editor 흐름을 실행하고 실패 수정 후 필요한 회귀를 확인한다.
- [x] 유지보수 설명·임시 에셋/교체 지점·Sprite 요청 유무·미검증 한계를 기록한다.
- [x] 완료 기준 충족 후 상태와 PLAN 목록·로드맵·기획 MAP을 함께 갱신한다.

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 공용 호출과 Registry 연결이 유효함 | 공용+씬 목록, 중복 ID/누락/타입 오류, 준비 전 요청, 등록 실패 후 기존 목록 보존 | Edit/Editor 검사·참조 결과 |
| 표시와 요청 상태가 일치함 | 초기 데이터 연결 순서, 열기/닫기 연타, 닫기 애니메이션·강제 제거·Host 파괴, 생성/훅 실패 복구 | Edit/Play 결과·잔여 객체/잠금 확인 |
| 콜백·Handle이 한 번만 완료됨 | 중복 요청, 확인/취소 경쟁, 닫는 중 범위 종료, 콜백 예외·재진입, 사용자 취소와 강제 정리 구분 | 상태/실행 횟수 비교 |
| 중첩·입력 차단이 안전함 | 자식 닫기 후 부모 복귀, 부모 강제 종료, 비최상위 조작 차단, Back 한 번 소비, Raycast와 실제 맵/키보드 입력, 누른 채 열기/닫기·핀치 전환·포커스 복귀 | Play 테스트·Editor 조작 기록 |
| 씬·세션 수명을 보존함 | Title↔Laboratory, 같은 씬 새 판/복원 성공·실패, 부모 종료, 늦은 비동기 결과, 멸망 단계 진입 중 확인창 | 실제 Gameplay 상태·저장 비교 |
| 확인 의도를 앱 복귀까지 안전하게 보존함 | 확인→닫기 도중 정지/포커스 상실→복귀, 대기 중 범위 종료, 중복 복귀·콜백 거절/예외 | 실행 횟수·요청 종료 결과·잠금 회수 |
| 멸망 경계에서 플레이 팝업만 한 번 정리함 | 설정+자식 알림→멸망→결과→메뉴 복귀 확인, 언어 변경 중 강제 닫기·설정 값 유지 | Play 상태·화면·구독 및 새 확인창 유지 |
| 초기화·로딩 복구가 유지됨 | Bootstrap 전체 흐름, Title/Laboratory 직접 진입, Registry/Localization 준비 실패·재시도, 로딩 중 요청·확인에서 전환으로 잠금 인계 | Play 결과·복구 관찰 |
| 기존 확인/설정 기능이 유지됨 | 덮어쓰기 취소, 메뉴 복귀 저장 실패→재시도/취소, 새 판/불러오기 실패 시 판 보존, 설정 즉시 적용·쓰기 실패/재시도·재진입 | 격리 슬롯의 파일/게임 상태·화면 |
| 텍스트·배치 상태가 보존됨 | ko/en·두 방향·큰 글자·긴 본문, 열린/가려진 팝업 재번역·회전, 빠른 언어 변경/실패, 스크롤/선택 보존, CSV 검토/가져오기/참조 검사 | Editor 화면·Play 및 텍스트 검사 |
| 기존 플레이와 정지 정책에 회귀가 없음 | 배치 미리보기 취소/복귀, 생산·배양·방출·보고, 멸망→결과→새 판, 팝업 종료 후 다른 정지 이유 유지, 기존 저장 복원 | 영향받은 기존 Edit/Play·상태 비교 |
| 앱 재실행에서 상태가 누적되지 않음 | Domain Reload ON/OFF·지원 Scene Reload 조건, 반복 Play·종료, 영구 Host 단일성·구독/Handle/잠금 잔존 검사 | 실행 조건·객체/구독 결과 |
| 추가·유지보수 절차가 재현 가능함 | 테스트용 구체 팝업/씬 Registry 등록·해제, 문구 변경, 프리팹 참조 교체와 누락 검출, 테스트 에셋 정리 | 절차·검사 기록 |
| 문서·상태가 일치함 | node scripts/validate-agent-docs.mjs, git diff --check 및 변경 범위 확인 | 검사 결과 |

검증은 Editor 임포트/컴파일·Edit Mode·Editor Play Mode와 실제 Gameplay 화면 관찰을 포함한다. 입력은 가능한 실제 이벤트 경로로 검증하고 직접 메서드 호출과 구분한다. 제품 저장과 분리된 테스트 슬롯을 사용하고 언어·Play/화면 설정을 복구한다. 새 빌드·실기기 센서/터치·Player 패키징은 미검증으로 기록한다. 정식 아트 대기는 development 완료를 막지 않지만 위 필수 기능 검증의 미실행/실패는 완료로 처리하지 않는다.

## 현재 상황과 다음 행동

- 현재 상태: completed. 두 검토 보완·공용 Popup 구현·기존 확인/설정 이관과 Editor 검증을 완료했다. PLAN-001~019 completed, active 0이다.
- 다음 행동: 이번 PLAN의 남은 필수 작업 없음. 후속 작업은 별도 PLAN 작성·승인 후 진행한다.
- 검증 범위: Edit 343/343·Play 65/65, 실제 씬 직접 진입·반복 Play·언어/두 방향/큰 글자 화면을 확인했다. 상세 과정과 한계는 [실행 증거](evidence/PLAN-019/README.md)를 따른다. 빌드 테스트·실기기는 실행하지 않았다.
- Sprite 요청: 없음. 기존 임시 UI/폰트와 Temp_ 프리팹을 사용하며 정식 아트 교체는 별도 art PLAN이다.

## 검증 결과

| 날짜 | 대상·환경·실행 방법 | 기대 결과·실제 결과 | 판정·증거 |
| --- | --- | --- | --- |
| 2026-11-02 | 작성 전 node scripts/validate-agent-docs.mjs·git status --short | 18 PLAN·active 0·상태/목록/링크 검사 통과, 작업 트리 변경 없음 | 통과·작성 전 상태 확인 |
| 2026-11-02 | 작성 후 node scripts/validate-agent-docs.mjs·git diff --check | 19 PLAN·active 0·목록 19개·로컬 링크 2350개 통과, 추적 문서 diff 공백 오류 없음 | 통과·문서 검사, 구현 검증 아님 |
| 2026-11-02 | 작성 단계의 Unity 런타임·빌드·실기기 | 당시 계획 작성만 수행, 런타임 실행 전 | 작성 시점 기록·이후 실행은 아래 결과 참고 |
| 2026-11-02 | Unity Editor 전체 Edit/Play | Edit 343/343·Play 65/65, 실패·건너뜀 0. 초기 실패와 수정/재검사도 보존 | 통과·[실행 증거](evidence/PLAN-019/README.md) |
| 2026-11-02 | Bootstrap/Title/Laboratory 직접 진입, Domain/Scene Reload 조건별 반복 Play | 프레임/시간 증가·Host 단일성·열린 팝업 종료/재진입·Time.timeScale=0 애니메이션·설정 원복 확인 | 통과·[직접 실행·화면](evidence/PLAN-019/README.md#실제-editor-재진입과-화면) |
| 2026-11-02 | 에셋/Localization·ko/en·가로/세로·큰 글자 | Registry/프리팹 연결·CSV 248개 키·화면/스크롤/확인 유지 확인, 새 오류·경고 0 | 통과·[실행 증거](evidence/PLAN-019/README.md) |
| 2026-11-02 | 빌드·실기기 | 사용자 조건에 따라 실행하지 않음 | 범위 제외·Player 패키징/기기 조작 미검증 |
| 2026-11-02 | 문서 검사·git diff --check·변경 범위 | PLAN 19개·active 0·목록/링크 검사 통과, 공백 오류 없음·Unity .meta 확인 | 통과·[문서 검사](evidence/PLAN-019/docs-validation-final.txt) |

## 관련 결정과 상태 전환

- 기술 결정: [TECH-016](../decisions/TECH-016-popup-lifetime-and-presentation.md). 유지보수: [Popup README](../../UnityProject/Assets/Scripts/Presentation/Popup/README.md). 기존 TECH-013의 정상 확인 표시를 Popup으로 이관하고 TECH-015의 Localization 계약을 재사용한다.
- 2026-11-02: 새 PLAN을 proposed로 생성했다. 사용자의 요청은 계획 작성이며 실행 승인은 아직 받지 않았다. active 0을 유지한다.
- 2026-11-02: proposed → active. 사용자가 두 검토 보완 반영과 실행을 명시적으로 승인했다. Editor 런타임 검증 포함·빌드 제외 조건을 유지한다.
- 2026-11-02: active → completed. 필수 구현·Editor 런타임 검증과 유지보수 기록을 완료하고 PLAN 목록·로드맵·기획 MAP을 동기화했다. 빌드/실기기 제외 조건과 active 0을 유지한다.

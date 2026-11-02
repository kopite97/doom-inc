---
id: PLAN-018
type: development
status: completed
title: Unity Localization·CSV 텍스트 관리와 한국어·영어 지원
updated: 2026-11-02
---

# PLAN-018 Unity Localization·CSV 텍스트 관리와 한국어·영어 지원

## 목표

제품 화면의 문구를 주제별 CSV에서 유지보수하고 Unity Localization과 TextManager를 통해 한국어·영어로 표시한다. 언어 변경은 게임 진행과 독립적으로 적용하며, 문구 수정·추가·삭제와 추가 언어 등록을 반복해도 코드·테이블·화면 사이에 누락이나 중복 원본이 생기지 않게 한다.

## 범위

- 포함: Unity Localization 패키지 도입, CSV 원본·String Table 가져오기/검증 도구, TextManager·공통 Localization 연결, 앱 초기화·언어 설정 저장, 제품 화면의 문구 이관, 한국어·영어 번역, 추가 언어 등록 경계, 유지보수 지침, Editor 런타임 회귀 검증.
- 검증 포함: 정적 검사, Editor 임포트·스크립트 재컴파일, Edit Mode, 실제 Gameplay 씬의 Editor Play Mode·가로/세로 화면 관찰. 런타임 검증은 구현 후 실행한다.
- 검증 제외: APK/AAB·PC Player 생성, Player 스크립트 컴파일, IL2CPP, Addressables 콘텐츠 빌드·콘텐츠 업데이트 빌드, 새 빌드 설치·실기기 검증. 사용자가 빌드 테스트 제외를 명시했다. 패키지 도구나 검증 메뉴의 간접 빌드도 실행하지 않는다.
- 기능 제외: 실제 Google Sheets/API·원격 번역 공급원, 런타임 CSV 직접 파싱, 세 번째 제품 언어의 번역, 음성 현지화, 정식 아트 교체, TMP 일괄 전환, 게임 밸런스·규칙 변경, 종합 성능 최종 판정.
- 요청 근거: 2026-11-02 사용자가 Unity Localization·CSV 기반 PLAN-018 작성과 TextManager·공통 연결, 한국어·영어·확장성 및 잦은 유지보수 고려를 요청했다. 런타임 테스트 포함·빌드 테스트 제외 조건을 유지한다.
- 실행 권한: 2026-11-02 사용자가 두 검토 보완 사항의 반영과 PLAN 실행을 승인했다. 직후 정정한 '두 가지 보완사항'을 기준으로 둘 다 반영한다. 빌드 테스트 제외는 유지한다.
- 아트: 기존 임시 에셋과 사용 가능한 폰트를 사용한다. 신규 Sprite 요청은 없으며 정식 아트 교체는 별도 PLAN 범위다.

## 기준 문서와 선행 조건

- 기획: [Thing·정의 데이터](../game-design/05-development/01-architecture/thing-model.md), [GameFlow](../game-design/05-development/01-architecture/game-flow.md), [화면·조작](../game-design/03-player-experience/03-interface/controls.md), [이해와 피드백](../game-design/03-player-experience/02-learning/guidance.md), [플레이 환경](../game-design/04-delivery/01-platform/environment.md).
- 로드맵: [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md)의 화면·데이터·씬 기반 후속 정비다. 완료하더라도 4단계 종합 성능·실기기 최종 판정을 완료하지 않는다.
- 선행: completed인 [PLAN-014](PLAN-014-mobile-ui-ux-and-facility-status.md), [PLAN-015](PLAN-015-topic-json-definition-data.md), [PLAN-016](PLAN-016-bootstrap-title-and-scene-flow.md), [PLAN-017](PLAN-017-production-storage-status-and-shared-ui.md)의 화면·입력·저장·씬·상태 계약을 보존한다.
- 설계 지침: [객체 설계](../agents/object-design.md), [추상화](../agents/abstraction.md), [아키텍처](../agents/architecture.md), [씬 흐름](../agents/scene-flow.md).
- 작업 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](AGETNS.md), [프로젝트 구조](../agents/project-structure.md), [스크립트 구조](../agents/script-structure.md), [명명](../agents/naming.md), [Unity 에셋](../agents/unity-assets.md), [하네스](../agents/harness.md), [검증](../agents/verification.md), [Unity 도구](../agents/unity-tools.md).
- 패키지 근거: [CSV 가져오기·내보내기](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/CSV.html), [언어 전환·문구 구독·테이블 공급](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/Scripting.html), [Smart Strings](https://docs.unity3d.com/Packages/com.unity.localization@1.5/manual/Smart/SmartStrings.html). 실제 설치 버전과 API는 착수 시 Unity 6000.3.27f1 호환성 및 lock 파일로 확인한다.
- 착수 조건: 사용자 실행 승인에 따라 유일한 active로 전환하고 PLAN 목록·기획 MAP·로드맵을 함께 갱신한다.

## 현재 확인한 구조

코드 경로는 `UnityProject/Assets/Scripts/` 기준이다.

| 대상 | 현재 상태와 영향 |
| --- | --- |
| `Data/Loading/GameDataBundle.cs`, `Presentation/Bootstrap/LocalJsonDataSource.cs` | IGameDataSource가 취득부를 분리하지만 GameDataComposer는 정해진 다섯 주제와 JsonDefinitionParser를 사용한다. 번역 CSV를 이 묶음에 추가하지 않는다. |
| `Data/Loading/DefinitionDtos.cs`, `Data/Models/ThingData.cs` | 표시 이름은 JSON displayName에서 복사한다. displayName은 저장 호환성 계산에서 제외되어 있어 이름 키 이관 시 이 성질을 보존해야 한다. |
| `Presentation/UI/Widgets/UiText.cs`, `UiList.cs` | legacy uGUI Text의 폰트·크기와 재사용 행을 관리한다. 행 키는 번역 키가 아니며 기존 동작 식별자로 유지한다. |
| Title·Loading·Bootstrap·Laboratory 화면과 상태 텍스트 변환 | 버튼·설명·대기 사유·오류·결과에 하드코딩이 존재한다. 코드뿐 아니라 씬·프리팹과 생성 도구도 확인한다. |
| `Gameplay/Invasion/InvasionReport.cs` | 구조화된 보고 값과 영문 Text 표시가 함께 있다. 보고 내용은 상태·수치로 보존하고 표시 시 번역한다. |
| `Presentation/SceneFlow/AppUiSettings.cs` | 기존 UI 설정은 저장 슬롯에 연결된다. 앱 언어를 슬롯별 설정이나 세션 저장에 섞지 않는다. |
| 패키지 | 작성 시 packages-lock.json에서 Localization·Addressables 항목이 확인되지 않았다. 설치는 실행 승인 이후이며 현재 구현된 것으로 취급하지 않는다. |

## 설계와 유지보수 계약

### 1. CSV 원본과 가져오기

- CSV를 번역 문구의 유일한 편집 원본으로 둔다. Common·Title·Loading·Laboratory·Definitions·Reports·Results 등 실제 사용 주제별로 분리하고, 번역 테이블은 CSV에서 반영되는 결과로 관리한다. 주제 경계는 문구 목록을 조사한 뒤 확정한다.
- 제안 배치: `Assets/CSV/Localization/<주제>.csv`, `Assets/SO/Localization/`의 Locale·String Table·Shared Table Data, `Assets/Settings/Localization/`의 설정 에셋. 패키지가 요구하는 고정 경로는 보존한다. 새 분류는 구현 시 프로젝트 구조 지침에 반영한다.
- Unity 내보내기 형식을 기준으로 Key·안정적인 Id·언어 열·문맥 설명과 Smart String 메타데이터의 매핑을 명시한다. 지원 언어 코드를 매핑하고 패키지 열 이름을 추측해 임의 포맷으로 만들지 않는다. CSV로 왕복할 때 Smart 플래그·문맥·ID가 보존되는지 검증한다.
- 키는 원문이나 번역문에서 자동 생성하지 않는다. 테이블과 키의 조합을 안정적으로 유지하고 같은 단어라도 의미·문맥이 다르면 분리한다. 공통 버튼 등 의미가 같은 항목만 공유한다.
- UTF-8·BOM·쉼표·따옴표·여러 줄·한글·의미 있는 공백을 처리한다. 단순 Split 파서는 사용하지 않으며 패키지 공개 CSV API와 검증 계층을 우선 사용한다.
- 명시적인 Editor 가져오기 명령을 제공한다. 전체 입력을 먼저 읽고 중복 키/ID, 필수 언어 누락·공백 문구, 미지원 열, 키/ID 충돌, 문장 포맷 오류를 검증한 뒤 적용한다. 불필요한 언어 열을 조용히 무시하지 않는다.
- 오류는 파일·행·키·언어·원인을 함께 보고한다. 실패한 입력이 기존 정상 테이블 일부를 덮어쓰지 않도록 적용 전 검증과 적용 중 실패 복구를 마련한다. 반복 실행은 ID·GUID·내용을 불필요하게 바꾸지 않는다.
- 삭제·이름 변경은 변경 목록과 참조 검사를 거친다. CSV 행 삭제를 단순 Merge로 처리해 오래된 키를 남기지 않으며, 사용 중 키를 삭제하거나 ID를 다른 의미에 재사용하지 않는다. 변경된 키의 코드·프리팹 참조도 같은 작업에서 갱신한다.
- 별도 읽기 전용 검사 명령으로 CSV와 테이블의 불일치, 누락·고아 키, 사용처의 미등록 키를 탐지한다. 자동으로 CSV를 역수정하거나 빌드를 실행하지 않는다. 정렬·줄바꿈을 정해 문구 하나 변경 시 관련 차이만 남도록 한다.
- 검토 보완 2: 한국어를 기준 언어로 두고 원문·문맥·인수 계약의 변경을 원문 해시/개정 정보로 추적한다. 언어별 검토 기준과 비교해 재검토 대상을 표시하며, 필수 언어의 미검토 항목은 완료 검사 실패로 보고한다. 자동 가져오기는 검토 완료를 기록하지 않는다.
- 번역 수정이 불필요한 변경도 명시적인 검토 완료 명령으로 확인할 수 있게 한다. 검토 기록은 CSV 원본과 함께 추적하며 '한국어만 수정 → 영어 재검토 필요 → 번역 수정 또는 명시적 확인 → 검사 통과'를 검증한다. 원문과 번역이 함께 바뀌어도 검토 확인을 자동 추정하지 않는다.

### 2. TextManager와 공통 Localization 연결

| 구성 | 책임·형태·수명 |
| --- | --- |
| TextManager | 게임의 문구 요청·키 규칙·누락 정책·화면 연결을 담당하는 일반 C# 공유 객체. 앱 수명 소유자가 생성하고 화면에 전달한다. |
| ILocalizationService | 초기 준비·지원/선택 언어 조회·언어 변경·문구 구독/조회에 필요한 최소 계약. 패키지 격리와 독립 검증이 도입 근거다. |
| UnityLocalizationService | Unity Localization 공개 API를 호출하는 일반 C# 연결 객체. Unity 의존성은 허용하지만 MonoBehaviour·별도 싱글톤은 요구하지 않는다. |
| Unity Localization | SelectedLocale·String Table·Smart Strings의 실제 원본. 별도 currentLanguage·번역 사전으로 원본을 중복 소유하지 않는다. |
| 화면의 텍스트 연결 | 키·인수와 Text 표시를 연결하고 화면/재사용 행 수명에 맞춰 구독·해제한다. 기존 UiText의 폰트·크기 책임을 보존한다. |

- 공통 초기화에서 객체를 만들고 앱 수명 소유자가 유지한다. Bootstrap 씬 파괴와 분리하며 SceneFlowManager는 준비를 조율하고 번역·파싱을 직접 수행하지 않는다. 전용 서비스 로케이터·Singleton 기반 클래스·DI 프레임워크는 도입하지 않는다.
- TextManager는 Unity 패키지 타입을 화면에 퍼뜨리지 않는 창구로 둔다. 실제 문장 포맷은 Smart Strings에 위임하며 자체 복수형 엔진·중복 언어 상태를 만들지 않는다.
- 구독 방식과 일회성 조회를 구분한다. 계속 표시되는 문구는 언어 변경을 따라 갱신하고, 일회성 결과 string이 자동으로 갱신된다고 가정하지 않는다. 숫자 등 인수가 바뀌면 명시적으로 갱신한다.
- LocalizedString/StringChanged 또는 LocalizeStringEvent 연결 중 각 표시의 소유 경로를 하나로 정한다. 같은 Text에 두 경로를 동시에 연결하지 않는다. 비동기 문구 완료는 화면·행의 요청 세대를 검사해 재사용된 다른 항목을 덮어쓰지 않게 한다.
- 문구 조회에 동기 대기·파일 읽기·씬 탐색을 숨기지 않는다. UI는 비동기 준비·실패를 처리하며 매 프레임 전체 테이블 재조회·전체 UI 재생성을 하지 않는다.
- JSON + SO → GameDataStore → Runtime 경로는 유지한다. 번역 테이블을 GameDataStore에 복제하거나 기존 IGameDataSource를 범용 파일 로더로 개편하지 않는다.
- 향후 원격 공급은 Unity의 테이블 공급 경계 등을 통해 연결할 수 있게 책임을 분리한다. 실제 원격 구현·인증·캐시·런타임 CSV 파싱은 이번에 만들지 않는다.

### 3. 초기화·언어 설정·실패 복구

- 공통 초기화가 Localization 초기화, 저장된 언어 선택, 최초 필수 테이블 준비까지 기다린 뒤 제품 UI의 정상 조작을 허용한다. 기존 코루틴 LoadingScreen 흐름과 연결하고 선택 언어 변경 알림만으로 테이블 준비 성공을 판정하지 않는다.
- Bootstrap·Title·Laboratory 및 기존 진단 진입이 같은 초기화 계약을 사용한다. 중복 준비는 공유하고 실패 후 재시도할 수 있게 한다. 앱 종료·Play 재진입·Domain Reload OFF에서 구독·정적 상태·오래된 작업을 정리한다.
- 초기 선택은 유효한 저장 언어 → 지원하는 기기 언어/상위 언어 → 한국어로 한다. 언어 코드로 지원 목록을 구성하고 Title과 인게임 설정에서 한국어·영어를 변경할 수 있게 한다.
- 언어 설정은 앱 범위 파일/설정으로 저장하고 게임 슬롯·새 판과 분리한다. 기존 파일 저장 공통 처리의 재사용을 검토한다. 손상·미지원 코드에는 기본 선택을 적용하고, 저장 실패는 현재 화면 선택과 재시작 후 보존 여부를 구분해 알리며 재시도 경로를 제공한다.
- 빠른 연속 변경에서는 마지막 요청만 유효하게 적용한다. 테이블 준비 실패 시 기존 정상 언어로 복구하고 입력 잠금·로딩이 남지 않게 한다. 언어 변경 때문에 세션을 재생성하거나 저장 데이터를 삭제하지 않는다.
- 초기화 전/번역 로딩 실패 화면에는 최소 로딩·오류·재시도 문구를 제공한다. 이 문구도 CSV 원본에서 Editor가 준비하는 작은 내장 결과로 관리해 번역 서비스에 의존하는 순환을 피하고 수동 문구 원본을 늘리지 않는다.
- 한국어·영어 필수 번역 누락은 가져오기/완료 검증 실패다. 런타임에는 선택 언어 → 한국어 → 내장 일반 오류 문구의 대체 경로를 정의하고 진단을 별도 기록한다. 키나 패키지의 개발용 오류 문구를 정상 제품 문구로 노출하지 않는다.

### 4. 제품 문구 이관과 기존 계약 보존

- 코드·씬·프리팹·생성 도구를 함께 조사해 이관 목록을 만든다. Title, 설정, 로딩/확인/오류/재시도, 연구소 HUD·메뉴·상세·안내, 생산/작업자 상태·표식 설명, 배치/이동/회전/취소, 전과 보고, 멸망·결과·새 판을 포함한다.
- 사용자에게 보이는 오류는 안정적인 오류 종류와 번역 키로 연결하고 원래 예외는 진단 로그에 보존한다. 정의 ID·상태 코드·저장 키·개체 이름 식별자·하네스/Editor 전용 진단을 무조건 번역하지 않는다. 제외한 문자열과 이유도 목록으로 남긴다.
- 시설·생명체·자원 표시 이름은 JSON displayName 대신 안정적인 번역 키를 연결하는 방향으로 이관한다. DTO·엄격한 JSON 검증·Data·화면·기준 문서를 함께 갱신하고 정의 ID·수치·SO 연결은 유지한다.
- 표시 이름 키가 저장 호환성 해시에 새로 들어가지 않게 한다. 이전 실제 저장의 복원과 번역만 변경했을 때 CompatibilityHash 불변을 확인한다. 원본 내용 해시와 게임 저장 호환성 해시를 혼동하지 않는다.
- 보고는 사건 종류·수치 등 구조화된 값을 유지하고 표시 시 번역한다. 언어 변경 후 과거 보고와 결과도 갱신하며 번역된 완성 문장을 세션 원본으로 저장하지 않는다.
- 문장은 템플릿과 명명된 인수로 구성한다. 수량·시간·백분율 등 표시 포맷, 영어 단복수와 한국어 조사 문맥을 확인한다. 입력/저장/밸런스 숫자 형식에는 표시 언어를 적용하지 않는다.
- 문구 변경은 상태·명령·버튼 동작을 바꾸지 않는다. 기존 UiList 행 식별자·스크롤·선택·시설 상태 ON/OFF·설명 펼침 상태를 보존하고 누르는 중 언어/배치가 바뀌면 오래된 클릭을 차단한다.
- 검토 보완 1: 표시 중 메시지는 번역 키와 인수로 유지하고 시설·자원 이름 등 인수 내부의 번역 참조를 구분한다. 바깥 문장과 내부 이름을 같은 언어로 갱신해 혼합 언어를 방지한다. 이미 발생한 알림의 수량·시각·유효 기간은 보존하며 재번역으로 알림 수명이나 확인창 동작을 다시 시작하지 않는다.
- 알림·오류/확인창·이름을 포함한 상세 화면을 유지한 상태에서 언어를 바꾸고, 중첩된 이름과 바깥 문장이 함께 바뀌며 명령·버튼 콜백이 실행되지 않는지 확인한다.

### 5. 언어 확장·폰트·장기 유지보수

- 지원 목록은 Locale 등록으로 구성한다. 코드의 언어 enum/switch나 열 개수 상수에 ko/en을 고정하지 않는다. 추가 언어에 필요한 CSV 열 매핑·Locale·fallback·폰트·문법·화면 검증 절차를 문서화한다.
- 기존 uGUI Text·허가된 폰트를 우선 사용한다. 한국어·영어 글리프와 숫자·문장부호를 확인하고 언어별 폰트가 필요하면 에셋 연결 책임에서 처리한다. 다른 언어는 폰트·줄바꿈·복수형·RTL 등의 별도 검증이 필요하다는 한계를 명시한다.
- 한국어·영어 가로/세로에서 긴 버튼·상태·보고·오류 문구와 최대 글자 크기 설정을 확인한다. 텍스트 갱신 후 필요한 레이아웃만 재계산하고 맵 영역·터치 크기·패널 스크롤을 보존한다.
- 구현 시 `docs/agents/localization.md`에 텍스트/키 변경·CSV 원본·검증·언어 확장 원칙을 분리한다. 루트 MAP과 관련 구조·검증 지침은 이를 참조하고 같은 규칙을 복제하지 않는다. 150줄 제한을 지킨다.
- CSV 디렉터리 README에는 실제 도구 위치/명령, 컬럼/Smart 설정 예제, 추가·수정·이름 변경·삭제·언어 추가·오류 복구 절차를 적고 지침을 연결한다. 작업 규칙과 사용 설명의 원본을 구분한다.
- 반복 유지보수의 기준은 'CSV 수정 → 읽기 전용 검증/변경 확인 → 가져오기 → 불일치 검사 → 영향 화면 런타임 확인'이다. 단순 번역 수정에 코드 변경·수동 다중 테이블 편집·빌드를 요구하지 않는다.

## 작업 목록

- [x] 표시 문자열·패키지·저장/호환성·화면 연결을 조사하고 이관/제외 목록과 기술 선택을 기록한다.
- [x] 호환 Localization 패키지와 필요한 의존성을 설치하고 버전을 고정한다. Editor에서 에셋을 직접 사용하는 검증 모드로 설정하며 콘텐츠 빌드는 실행하지 않는다.
- [x] 주제별 CSV·Locale·String Table·설정과 내장 초기 오류 문구를 준비한다.
- [x] CSV 검증·변경 미리보기·가져오기·불일치 검사 및 실패 복구를 구현한다.
- [x] TextManager·공통 서비스·화면 구독과 앱 수명/공통 초기화 연결을 구현한다.
- [x] 앱 범위 언어 설정·선택 UI·실패/재시도·빠른 변경 계약을 구현한다.
- [x] 제품 문구·표시 이름·보고·오류를 이관하고 한국어·영어를 검수한다.
- [x] 전용 유지보수 지침·작업 설명·참조 경로와 관련 기획/기술 결정을 정리한다.
- [x] 아래 Edit/Play 회귀·유지보수 시나리오를 실행하고 실제 결과·미검증 한계를 기록한다.
- [x] 완료 기준 충족 시 PLAN 상태와 목록·로드맵·기획 MAP을 함께 갱신한다.

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| CSV가 단일 원본이며 반복 반영이 안정적 | 정상 다중 주제 가져오기·재실행·왕복, ID/Smart/문맥 보존, 변경 없는 재실행의 diff 확인 | Editor 검사 결과·변경 목록 |
| 잘못된 입력이 기존 데이터를 훼손하지 않음 | 중복 키/ID·빈 번역·변수 불일치·잘못된 포맷·쉼표/따옴표/개행·BOM, 적용 실패 주입과 정상 테이블 보존 | Edit 테스트·오류 보고 |
| 수정·추가·삭제를 유지보수 가능 | 한 문구 수정, 키 추가·이름 변경·삭제, 사용 중 삭제 차단, 테이블 수동 변조 탐지 후 CSV로 복구 | 재현 절차·검사 결과·화면 |
| 원문 변경 후 번역 재검토를 추적함 | 원문·문맥·인수 변경 감지, 자동 가져오기 후 미검토 유지, 번역 수정 또는 명시적 검토 확인 후 통과 | Edit 테스트·변경/검토 기록 |
| 복합 문구·기존 알림을 일관되게 재번역함 | 열린 알림/확인창/자원 상세에서 언어 변경, 내부 이름·문장 동시 변경과 수량/시각/유효 기간·콜백 불변 | Play 테스트·상태 비교 |
| 한국어·영어 제품 문구가 완전함 | 이관 목록의 모든 제품 화면/동적 분기 대조, 필수 번역·참조 키·인수/Smart 문법 검사 | 누락 0 결과·제외 목록 |
| 초기화·씬·앱 수명이 안전함 | Bootstrap 정상/실패/재시도, Title·Laboratory 직접 진입, 씬 왕복, 반복 Play·Domain Reload ON/OFF 및 지원 Scene Reload 조건 | Play 테스트·실제 흐름 로그 |
| 언어 선택과 표시가 일치함 | Title·인게임 전환, 빠른 연속 변경·로드 실패·늦은 완료·행 재사용·화면 종료, 앱 재실행·슬롯/새 판 변경, 설정 손상/쓰기 실패 | Play 테스트·설정/관찰 기록 |
| 게임 진행·입력 회귀가 없음 | 언어 변경 중 생산/배양·배치/이동 미리보기·표식 토글·패널 스크롤·보고, 멸망/결과/재시작, 확인창 동작·오래된 클릭 차단 | 실제 Gameplay Play·상태 비교 |
| 가로·세로와 폰트가 정상임 | ko/en 두 방향·최대 글자 크기·긴 문구·줄바꿈·숫자·글리프, 폰트 적용/레이아웃 후 잘림 확인 | Editor 화면·관찰 기록 |
| 저장·밸런스 계약을 보존함 | 이전 실제 저장 복원, 이름/번역만 수정 시 CompatibilityHash 불변, 언어 전환 전후 정의 ID·생산 값·진행 상태 동일 | Edit/Play 회귀·해시/상태 비교 |
| 추가 언어가 코드 분기 없이 등록됨 | 테스트 전용 세 번째 Locale/번역 fixture로 지원 목록·조회·fallback을 확인하고 정리. 제품 제공 언어는 ko/en 유지 | 확장 테스트·정리 확인 |
| 문서·추적 상태가 일치함 | 문서 검사기, 관련 지침 링크·원본 경계·도구 설명 확인 | 검사 결과 |

사용자 요청에 따라 빌드 테스트는 완료 조건이 아니다. Addressables/Localization의 Editor 동작만 확인하며 Player 패키징·실제 배포 콘텐츠·실기기 폰트/성능은 검증하지 않았다고 명시한다. 테스트 또는 패키지 메뉴가 자동 빌드를 요구하면 해당 경로를 실행하지 않고 Editor 검증 경로와 한계를 기록한다.

## 현재 상황과 다음 행동

- 마지막으로 확인한 사실: PLAN-001~017은 completed다. Unity 6000.3.27f1·Localization 1.5.13·Editor 에셋 직접 사용 모드로 진행한다. 사용자 Hub 로그인 후 UPM 설치가 정상 완료됐다.
- 현재 상태: completed. TextManager·공통 서비스·앱 초기화·언어 저장/설정 UI·9개 주제 CSV/247개 키·제품 문구·정의 이름 키·구조화된 보고를 구현했다. 전체 Edit 330/330, 최종 전체 Play 57/57 통과. 생산/배양·보고·미리보기·열린 확인창·멸망/결과·새 판 중 언어 변경과 실패/재시도/추가 Locale, 실제 Editor 직접 진입과 재실행을 확인했다.
- 다음 행동: 새 승인 작업을 기다린다. 문구 유지보수는 아래 지침/CSV 도구를 사용한다. 빌드·실기기 검증과 새 기능은 해당 요청·승인 범위에서 진행한다.
- 완료 근거: [구현·이관·검증 기록](evidence/PLAN-018/README.md). 한국어/영어 가로·세로 최대 글자 크기 화면을 확인했고 CSV/테이블 일치·참조 누락/미사용 0건이다. 테스트 공급자·키·Locale는 제품 에셋에 남기지 않았다. 임시 에셋을 유지하며 신규 Sprite 요청은 없다.
- 유지보수 문서: [텍스트 지침](../agents/localization.md), [CSV 사용 설명](../../UnityProject/Assets/CSV/Localization/README.md), [TECH-015](../decisions/TECH-015-localization-and-csv-text.md). 문서화와 실제 완료 검증은 구분한다.
- 후속 범위: 원격 번역 공급, 추가 제품 언어, 빌드·실기기 검증, 정식 아트 및 종합 성능 최종 판정.

## 검증 결과

| 날짜 | 대상·환경·실행 방법 | 기대 결과·실제 결과 | 판정·증거 |
| --- | --- | --- | --- |
| 2026-11-02 | 작성 전 `node scripts/validate-agent-docs.mjs` | 17 PLAN·active 0, 상태/목록·링크 검사 통과 | 통과·작성 전 상태 확인 |
| 2026-11-02 | 작성 후 `node scripts/validate-agent-docs.mjs`, `git diff --check` | 18 PLAN·active 0·목록 18개·로컬 링크 2243개 통과, 추적 문서 diff 공백 오류 없음 | 통과·문서 검사만 실행 |
| 2026-11-02 | Unity UPM Localization 설치 | 최초 인증 실패 → 사용자 Hub 로그인 → 1.5.13 설치 완료·lock 확인 | 설치 통과·게임 동작 판정 아님 |
| 2026-11-02 | Editor Edit Mode `TextManagementTests` | 9개 실행·9 통과·실패/건너뜀 0. 중첩 문구·구독 수명·검토 추적 기본 계약 확인 | [결과](evidence/PLAN-018/text-core-edit.json) |
| 2026-11-02 | Unity Localization Play 기본 조회/전환 | CSV 테이블 직접 로드, ko/en 및 중첩 인수 전환 1/1 통과 | [초기 결과](evidence/PLAN-018/localization-play-core.json), [통합 후 재검사](evidence/PLAN-018/localization-play-integration.json) |
| 2026-11-02 | CSV 유지보수 Edit | 인용/BOM/개행·ID·인수·미검토·세 번째 언어 열·사용 중 삭제 차단·적용 실패 복구·반복 반영 11/11 통과 | [결과](evidence/PLAN-018/csv-edit-rerun.json) |
| 2026-11-02 | 전체 Edit 첫 회귀 → 수정 후 두 번째 | 313개 중 308 통과 → 329개 중 327 통과. 이전 표시 이름/테스트 초기화 계약 수정. 남은 손상 언어 설정 예외 2건 수정 후 재검사 필요 | [첫 검사](evidence/PLAN-018/edit-regression-initial.json), [두 번째](evidence/PLAN-018/edit-regression-second.json)·아직 전체 통과 아님 |
| 2026-11-02 | 전체 Edit 최종 | 330/330 통과·실패/건너뜀 0. 키 추가/이름 변경/삭제, ID 유지, 수동 테이블 변조 탐지·CSV 복구 포함 | [결과](evidence/PLAN-018/edit-regression-final.json) |
| 2026-11-02 | 전체 Play 첫 검사 → 두 번째 | 46/56 → 55/56 통과. 진단 경고·번역 오류 계약·비동기 초기화 대기 수정. 남은 Editor 포커스 관련 사례와 추가 언어 흐름 재검사 중 | [첫 검사](evidence/PLAN-018/play-regression-initial.json), [두 번째](evidence/PLAN-018/play-regression-second.json)·완료 판정 보류 |
| 2026-11-02 | 전체 Play 최종 | 57/57 통과·실패/건너뜀 0. 모든 Game 창의 검증 해상도 고정/복원, 비동기 메뉴 대기, 공급자 설정 복제본 격리 후 재실행 | [최종 결과](evidence/PLAN-018/play-regression-final.json)·앞선 실패 기록은 보존 |
| 2026-11-02 | 실제 Editor 진입/반복 Play·두 언어 화면 | Bootstrap 전체 흐름, Title/Laboratory 직접 진입, Domain Reload ON/OFF 및 Scene Reload OFF 새 판/이어하기 정상. 프레임/시간 증가·언어 유지·단일 영구 객체 확인·원래 설정/슬롯 복원 | [시나리오·화면 증거](evidence/PLAN-018/README.md) |
| 2026-11-02 | 최종 CSV/테이블·키 검사 | 247개 키 유효, CSV/테이블 일치, 참조 누락 및 미사용 키 0건 | [검사 로그](evidence/PLAN-018/csv-validation-final.json) |
| 2026-11-02 | Player·Addressables 콘텐츠 빌드·실기기 | 사용자의 빌드 제외 요청에 따라 범위에서 제외 | 미실행·성공 판정 아님 |

## 관련 결정과 상태 전환

- 관련 결정: [주제별 JSON](../decisions/TECH-012-topic-json-catalog.md), [앱 씬·LoadingScreen](../decisions/TECH-013-app-scenes-and-loading-screen.md), [Localization·CSV 원본·데이터/언어 수명](../decisions/TECH-015-localization-and-csv-text.md).
- 2026-11-02: 신규 → proposed. 사용자의 PLAN-018 작성 요청을 반영했다. 실행 승인과 빌드 요청으로 간주하지 않는다.
- 2026-11-02: proposed → active. 사용자가 검토의 두 가지 보완 사항 반영과 실행을 승인했다. 두 번째만 반영하라는 앞선 표현은 후속 정정에 따라 두 가지 모두로 해석한다. 런타임 테스트 포함·빌드 제외를 유지한다.
- 2026-11-02: active → completed. 구현·Edit 330/330·Play 57/57·유지보수·실제 Editor 재진입/화면 검증을 완료했다. 빌드·실기기·정식 아트·종합 성능은 이번 완료 판정에 포함하지 않는다.

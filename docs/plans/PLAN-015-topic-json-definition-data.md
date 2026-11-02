---
id: PLAN-015
type: development
status: completed
title: 주제별 JSON 정의 데이터와 초기 로딩 전환
updated: 2026-11-02
---

# PLAN-015 주제별 JSON 정의 데이터와 초기 로딩 전환

## 목표

현재 CSV로 관리하는 정의·밸런스 값을 전용 디렉터리의 주제별 JSON으로 전환한다. `JSON DTO + Registry SO → 읽기 전용 Data → Runtime`을 구성하고, 게임 초기 로딩에서 전체 카탈로그를 영속 GameDataStore에 준비한다. 기존 게임 값·정의 ID·에셋 바인딩·플레이 저장을 보존한다.

## 범위

- 포함: 주제별 JSON 원본·DTO·엄격한 입력 검증, 취득부 추상화와 로컬 어댑터, Data 조립·설정·호환성 해시 정비.
- 포함: Bootstrap·씬·Editor 생성/검사 도구·테스트의 CSV 의존 제거와 참조 교체, 현행 지침·기획·기술 결정 갱신.
- 포함: Edit Mode 로직 검사와 **Editor Play Mode 런타임 테스트**, 실제 초기화·생산·저장 복원·두 판 종료/재시작 회귀.
- 제외: **모든 빌드 테스트**. APK/AAB·PC Player 빌드, Player 스크립트 빌드 검증·IL2CPP 검사·실기기 설치/검증을 실행하지 않는다. 빌드 도구의 경로/참조 코드는 정비하되 실행하지 않는다.
- 제외: 진행 저장 파일의 주제별 분할·스키마 교체, 밸런스/게임 규칙 변경, 정식 아트, 런타임 데이터 핫 리로드, 실제 Google Sheets/API·캐시·인증·원격 에셋 배포.
- 요청 근거: 2026-11-02 사용자가 CSV를 전용 디렉터리의 주제별 JSON으로 바꾸기 위한 PLAN 작성을 요청했다. 후속 지시로 실행 시 runtime 테스트는 포함하고 빌드 테스트는 제외했다. 이후 검토 보완 두 항목의 반영과 구현 실행을 명시적으로 승인받았다.

## 기준 문서와 선행 조건

- 선행: [PLAN-012](PLAN-012-csv-so-data-and-game-data-store.md)·[PLAN-014](PLAN-014-mobile-ui-ux-and-facility-status.md) completed. [로드맵](../game-design/05-development/02-implementation/roadmap.md)의 데이터 기반 후속 정비이며 4단계 종합 최적화 완료를 뜻하지 않는다.
- 기획: [Thing과 정의 데이터](../game-design/05-development/01-architecture/thing-model.md), [GameFlow](../game-design/05-development/01-architecture/game-flow.md), [플레이 환경](../game-design/04-delivery/01-platform/environment.md).
- 기존 기술: [TECH-009](../decisions/TECH-009-csv-data-catalog.md)의 ID 결합·취득부 분리·원자적 공개·저장 호환성 원칙을 유지하고 CSV 표현/파서 부분을 대체한다. 과거 PLAN과 증거는 당시 사실로 보존한다.
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](AGETNS.md), [프로젝트 구조](../agents/project-structure.md), [명명](../agents/naming.md), [스크립트 구조](../agents/script-structure.md), [아키텍처](../agents/architecture.md), [Unity 에셋](../agents/unity-assets.md), [하네스](../agents/harness.md), [검증](../agents/verification.md), [Unity 도구](../agents/unity-tools.md).

## 이관 전 구조와 변경 지점

| 현재 위치 | 확인한 사실·예정 변경 |
| --- | --- |
| `UnityProject/Assets/CSV/Balance/` | resources·creatures·facilities·invasion·session 5개 CSV. 같은 값과 ID를 JSON으로 이관한다. |
| 기존 CsvBundle.cs → [GameDataBundle.cs](../../UnityProject/Assets/Scripts/Data/Loading/GameDataBundle.cs) | ICsvDataSource·원문 묶음·CsvRow·CSV 파서가 함께 있다. 취득 계약과 JSON 해석/검증 책임을 분리한다. |
| [GameDataCatalog.cs](../../UnityProject/Assets/Scripts/Data/Catalog/GameDataCatalog.cs) 및 `Data/Models/` | Composer·GameSettings·Data 생성자가 CsvRow를 사용한다. 형식에 독립적인 검증된 DTO로 교체한다. |
| [GameBootstrap.cs](../../UnityProject/Assets/Scripts/Presentation/Bootstrap/GameBootstrap.cs)·[LocalJsonDataSource.cs](../../UnityProject/Assets/Scripts/Presentation/Bootstrap/LocalJsonDataSource.cs) | 직렬화된 TextAsset 목록으로 로딩하고 Registry와 결합한다. JSON 참조·어댑터로 교체한다. |
| [GameDataStore.cs](../../UnityProject/Assets/Scripts/Presentation/Bootstrap/GameDataStore.cs) | 완성된 카탈로그만 공개하는 영속 싱글턴. 기존 동시 초기화·실패 재시도·씬/새 판 재사용 수명을 유지한다. |
| [GameDataAssets.cs](../../UnityProject/Assets/Scripts/Editor/Definitions/GameDataAssets.cs)·`Editor/Validation/` | CSV 고정 경로·메뉴·씬 참조 설정·빌드 의존 검사 문구가 존재한다. JSON 기준으로 정비하고 빌드 검사 자체는 실행하지 않는다. |

## 설계 계약

### 전용 디렉터리와 파일

예정 원본 위치는 `UnityProject/Assets/JSON/Balance/`다. 아래는 생성할 경로의 명세이며 아직 파일이 존재하거나 이관됐다는 뜻이 아니다.

| 파일 | 내용·개수 계약 |
| --- | --- |
| `Resources.json` | 자원 정의 목록: 현재 유기물·배양액 |
| `Creatures.json` | 생명체 정의 목록: 현재 프로토타입·보급형·침공형 |
| `Facilities.json` | 시설 정의 목록: 현재 채집·저장·가공·배양 |
| `Invasion.json` | 침공 설정: 현재 정확히 1개 |
| `Session.json` | 초기 Map·시작/선택 ID·생산 속도·연출 시간 설정: 현재 정확히 1개 |

- 각 파일은 UTF-8 JSON 객체이며 `schemaVersion`과 `entries` 배열을 가진다. 5개 파일을 한 거대 JSON에 다시 합쳐 편집 원본으로 두지 않는다. 현재 정의 항목·필드는 빠짐없이 이관한다.
- `entries`의 각 항목은 `definitionId`로 식별한다. 파일명·배열 인덱스를 정의 ID로 사용하지 않는다. 주제 키와 TextAsset을 명시적으로 연결하여 파일명 추론과 실행 중 디렉터리 탐색에 의존하지 않는다.
- 기존 필드명을 가능한 유지하며 숫자는 JSON number, 컴포넌트 목록은 문자열 배열, 역할 enum은 명시적 문자열로 표현한다. 사용하지 않는 시설 수치/연결 필드의 생략·기본값은 역할별 계약으로 문서화한다. 필수 필드의 누락을 0이나 빈 문자열로 조용히 바꾸지 않는다.
- 예시 형식(자원 파일의 한 항목이며 전체 이관본이 아님):

```json
{
  "schemaVersion": 1,
  "entries": [
    {
      "definitionId": "resource.organic",
      "displayName": "유기물",
      "components": ["ResourceStack"]
    }
  ]
}
```

### 취득 → DTO → Data

검토 보완 1: [JSON 필드 계약표](../../UnityProject/Assets/JSON/Balance/README.md)에 JSON 타입·필수 여부·역할별 생략 기본값·호환 해시 입력을 고정한다. 사용하지 않는 시설 필드는 기존 CSV의 0/빈 문자열로 복원하여 해시에 포함한다. 숫자는 손실 없는 원문 수치에서 해시를 계산하고 Runtime float 변환과 분리한다. 생략/명시적 기본값·소수 정밀도·정수/실수 표기 등가성을 검사한다.

1. 취득 인터페이스는 일반 C#의 비동기·취소 가능한 계약으로 주제별 원문과 출처 정보를 반환한다. 예시 명칭은 `IGameDataSource`·`GameDataBundle`이며 확정 구현명은 실행 시 명명 지침에 맞춘다. Unity 타입·파일 경로·HTTP 응답 타입을 공개 계약에 노출하지 않는다.
2. 로컬 어댑터만 직렬화된 JSON TextAsset 참조에 접근한다. 장차 API 어댑터가 같은 계약을 구현할 수 있어야 한다. 원격 통신 구현은 이번 범위에 포함하지 않는다.
3. JSON 해석기는 주제별 타입 DTO를 생성하고 구조·필수 값·범위·참조를 검증한다. CSV 문자열로 재변환하거나 CsvRow를 영구 중간 표현으로 사용하는 우회 구조를 남기지 않는다.
4. 자원·생명체·시설 DTO와 해당 Registry SO를 동일한 정의 ID로 일대일 결합한다. 기존 SO GUID·선택적 Sprite/Prefab 참조를 유지한다. 순수 값만 있는 침공/세션에 불필요한 Registry를 추가하지 않는다.
5. 전체 JSON 및 모든 Registry·상호 참조 검증이 성공한 후 GameDataStore가 카탈로그를 원자적으로 공개한다. 로딩 중/실패 시 부분 Data로 세션·자동 저장을 시작하지 않는다. 같은 앱 실행의 씬 전환·새 판은 준비된 카탈로그를 재사용한다.

파서 선택은 현재 사용 가능한 런타임 의존성을 먼저 조사하고 엄격한 검증을 충족하는 최소 구성을 택한다. Editor 전용 라이브러리를 런타임에 사용하지 않으며, 새로운 패키지가 필요하면 이유·버전·대안을 기술 결정에 기록한다. 필드 기본값만 채우는 역직렬화 성공을 데이터 유효성으로 간주하지 않는다.

### 검증·진단 계약

- 필수 주제 누락/중복/알 수 없는 주제, 비지원 schemaVersion, 잘못된 최상위 구조·entries 타입·개수를 거부한다. 모든 필수 파일이 같은 지원 스키마 계약을 따라야 한다.
- 잘못된 JSON 문법·중복 속성명·알 수 없는 필드·필수 필드 누락·null·잘못된 타입을 거부한다. 주석/후행 쉼표 등 비표준 확장을 허용하지 않는다.
- ID의 공백·중복(주제 간 포함)·Registry 불일치·잘못된 종류·존재하지 않는 참조를 검사한다. enum·컴포넌트 목록의 미지원 값과 중복도 검사한다.
- 정수 소수부/범위 초과, long 점수 정밀도 손실, 비유한 수·실수 범위, 기존 양수/역할/초기 운반량/Map 조건을 검사한다. 정수 값을 double 경유로 읽어 반올림하지 않는다.
- 오류는 주제/파일·JSON 경로 또는 항목 인덱스·알 수 있는 정의 ID·원인을 제공한다. 문법 오류는 식별 가능한 위치를 남긴다. 실패·취소 후 기존 저장과 준비된 정의를 보호하고 재시도 경로를 유지한다.

### 저장 호환성과 이관

- 원문 출처 해시와 플레이 저장의 의미 기반 호환성 해시를 구분한다. JSON 들여쓰기·속성/항목 순서·표시 이름만 바뀌어도 진행 저장이 거부되지 않아야 한다. 값·참조·컴포넌트 의미가 바뀌면 기존 보호 규칙대로 거부한다.
- 현행 해시는 CSV 셀 문자열을 정렬/정규화해 계산하므로, DTO 도입만으로 새 해시를 계산하면 같은 값의 기존 저장도 거부될 수 있다. 실행 초기에 기존 카탈로그/해시를 고정 fixture로 보존한다.
- JSON DTO를 기존 호환성 필드/값 표현으로 결정적으로 투영하는 호환 해시 계층을 마련한다. 현재 배포 값의 해시는 `LegacyDataBaseline.Hash`와 정확히 같아야 한다. 의미가 같은 JSON 숫자 표기(예: `1`/`1.0`)는 같은 결과가 되도록 처리하고, 컴포넌트의 기존 의미/순서 계약도 보존한다.
- 기존 해시 판정과 맞지 않는 경우 값을 보지 않고 임의의 이전 해시를 허용하거나 `LegacyDataBaseline.Hash`를 새 값으로 덮어쓰지 않는다. 사용자 데이터의 자동 삭제·묵시적 새 판·저장 버전 강제 변경도 하지 않는다. 기존의 임의 수정 데이터 저장까지 복원할 수 있다고 약속하지 않으며 지원 범위와 거부 결과를 기록한다.
- 테스트용 CSV 비교 fixture는 이관 등가성 증거로만 유지할 수 있다. 전환 완료 후 제품 실행·씬·생성 도구는 JSON만 사용하고 기존 CSV 원본/파서/어댑터의 활성 경로를 제거한다. 과거 PLAN·기술 결정·검증 증거 속 CSV 기록은 보존한다.

## 작업 목록

- [x] 기존 5개 CSV의 항목·값·ID·현재 호환 해시와 저장 복원 기준 fixture를 고정하고 DTO/파일 스키마를 확정한다.
- [x] 전용 JSON 디렉터리·5개 주제 파일·편집/등록 안내를 준비하고 CSV와 값/관계 등가성을 확인한다.
- [x] 형식 독립 취득 계약·로컬 JSON 어댑터·파서·검증 DTO·오류 위치 진단을 구현한다.
- [x] Data/설정 생성자의 CsvRow 의존을 제거하고 Registry 결합·호환 해시·GameDataStore 초기화를 연결한다.
- [x] Bootstrap 직렬화 필드·실제 플레이/진단 씬·생성/검사 도구·테스트를 JSON 참조로 교체한다. 관련 빌드 도구의 경로도 정비하되 실행하지 않는다.
- [x] 검토 보완 2: AssetDatabase 의존성 검사와 Player 컴파일을 분리한다. 저장된 플레이/진단 씬 3개를 새로 열어 JSON 참조를 검사하고, 테스트가 참조를 주입하기 전에 실제 Bootstrap 초기화를 확인한다. 빌드 API는 호출하지 않는다.
- [x] 이관/실패/저장 호환성 Edit 검사와 Editor Play Mode 런타임·기존 게임 회귀를 실행한다.
- [x] 제품의 CSV 의존을 제거하고 프로젝트/스크립트 지침·현행 기획·TECH-009의 대체 관계·새 기술 결정·PLAN/목록/로드맵/MAP을 동기화한다.

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 주제별 JSON이 유일한 제품 원본 | 파일 5종·참조·DTO/값/ID/SO 결합 정적 검사 및 CSV 기준과 전체 필드 비교 | 파일/정의 수·값·참조 등가성 결과 |
| 엄격한 입력 처리 | Edit: 정상 문서·순서/서식 차이, 누락/중복/알 수 없는 값·타입·스키마·숫자 범위/정밀도·참조 오류 | 테스트 수·성공/실패·위치 진단 |
| 취득부 교체·원자적 공개 | 대기/실패/취소 가능한 가짜 취득부로 검증. Editor Play Mode에서 전체 준비 전 Ready/세션 미공개, 실패 재시도·동시 초기화·Store 수명 확인 | 실제 초기화 순서·저장 보존·로그 |
| 기존 저장과 정의 의미 유지 | 기존 기준 카탈로그/저장과 JSON의 동일 해시·복원 결과, 표시만 변경 시 호환·게임 값 변경 시 거부, 해시 없는 기존 저장 이관 조건 검사 | 현재 해시 동일·동일 ID/관계·실패 시 원본 보존 |
| Runtime 회귀 | **Editor Play Mode 필수**: 로컬 JSON 실제 초기화, 건설/물류/가공/배양/배정/침공/보고·저장 복원·멸망/결과/새 판의 기존 두 판 회귀. Domain Reload ON/OFF에서 Store 중복/이전 상태 유출 확인 | 실행 환경·테스트 수·실제 결과 JSON/로그·필요한 화면 |
| Unity 참조와 문서 | 누락 meta/GUID·씬/생성 도구 참조·런타임 Editor 의존 없음·CSV 활성 경로 없음·문서 검사 | 정적 검사와 MAP 일치, 과거 증거는 보존 |

**빌드 테스트 제외는 사용자의 명시적 검증 범위다.** 위 Edit/Editor Play Mode·정적 검사로 이 PLAN을 판정하며 빌드 미실행 자체를 완료 장애로 삼지 않는다. APK/Player 동작·플랫폼별 패키징·IL2CPP 호환을 검증했다고 기록하지 않는다. 빌드/실기기 확인은 추후 사용자 요청 시 별도 범위로 진행한다. PLAN 작성 중에는 런타임 테스트도 실행하지 않으며 승인 후 구현 단계에서 수행한다.

## 현재 상황과 다음 행동

- 마지막 확인: PLAN-001~014 completed, active 0. main 작업 트리는 작성 전 깨끗했다. 로컬 데이터는 CSV 5종이며 Data 생성과 호환 해시가 CsvRow에 결합돼 있다.
- 현재 상태: completed. 두 검토 보완을 반영하고 주제별 JSON·DTO·취득부·씬 이관·호환성 해시·CSV 제거를 마쳤다. Edit 294/294·Domain Reload ON/OFF Play 각각 36/36 및 저장된 씬 검사 1/1을 확인했다. 원래 Editor 설정 복원·컴파일 완료·콘솔 오류/경고 0을 확인했다.
- 다음 행동: 이 PLAN의 필수 작업은 없다. 빌드/실기기는 사용자 지시로 미실행이며 추후 요청 범위로 남긴다. 다음 PLAN은 사용자 요청·승인 후 진행한다.
- 아트 요청: 없음. 신규 Sprite·폰트·정식 아트 교체가 필요하지 않아 요청 폴더를 만들지 않는다.
- 제한: 빌드/실기기 검증은 사용자 지시로 제외한다. 기존 완료 PLAN의 테스트 수치를 이번 전환의 통과 결과로 재사용하지 않는다.

## 검증 결과

| 날짜 | 대상·방법 | 기대 결과·실제 결과 | 판정 |
| --- | --- | --- | --- |
| 2026-11-02 | 지침·PLAN 목록·Git 상태·CSV/로더/조립기/기획 읽기 전용 조사 | 기존 14개 PLAN completed, 작업 트리 깨끗함. 문서 검사에는 active 0 오류만 존재. 5개 테이블·CsvRow 생성자 의존·문자열 기반 호환 해시·씬 참조 교체 범위를 확인 | 계획 조사 완료, 구현/런타임 검증 아님 |
| 2026-11-02 | 작성 후 문서 검사·MAP 상태 대조·공백 검사 | PLAN/목록 15개 일치, 로컬 링크 1,940개·지침 줄 수 정상. PLAN-015 proposed와 MAP 후보 표기 일치. 기존 파일 diff와 신규 PLAN의 공백/끝 개행 정상 | active 0은 실행 승인 대기 상태이며 검사기의 해당 오류만 남음. 전체 검사 PASS로 기록하지 않음. Unity 테스트·빌드는 미실행 |
| 2026-11-02 | JSON 원본·DTO·Registry 결합·해시 이관 | 5개 주제 파일·정의 9개/설정 2개를 구성하고 전체 필드를 동결 CSV와 대조했다. 현행 호환 해시와 저장 복원 기준을 유지했다. 기존 CSV 제품 원본/파서·CsvRow·어댑터 제거 | [이관 증거](evidence/PLAN-015/README.md), [필드 계약](../../UnityProject/Assets/JSON/Balance/README.md), [TECH-012](../decisions/TECH-012-topic-json-catalog.md) |
| 2026-11-02 | 전체 Edit 검사와 입력 경계 보완 | 첫 실행 292개 중 fixture 오류 1건을 수정했고, 미사용 시설 실수의 Runtime 범위 검사도 보완했다. 최종 294/294 통과·실패/건너뜀 0 | [최종 Edit](evidence/PLAN-015/editmode-03.json). 실패 이력은 증거 README에 보존 |
| 2026-11-02 | 저장된 씬 3개의 의존성·직접 로딩 | Player 컴파일 없는 AssetDatabase 검사를 분리했다. JSON/Registry 주입 없이 실제 세 씬을 로드하는 검사 1/1 통과. 테스트 저장 슬롯만 격리했다 | [씬 검사](evidence/PLAN-015/saved-scenes-result.json), [최종 의존성 검사](evidence/PLAN-015/scene-dependencies-final.json) |
| 2026-11-02 | 전체 Editor Play·Domain Reload ON/OFF | 최종 ON 36/36(64.65초)·OFF 36/36(62.48초), 실패/건너뜀 0. 초기화·동시 로딩/실패/취소·생산/물류/배양/침공·저장 복원·두 판 종료/재시작과 UI 회귀 포함 | [ON](evidence/PLAN-015/playmode-on-final.json), [OFF](evidence/PLAN-015/playmode-off.json). APK/Player/IL2CPP 검증 아님 |
| 2026-11-02 | 종료·정적 대조 | 원래 Play 옵션 복원, Editor ready/stopped·runInBackground false·컴파일 오류 없음·콘솔 오류/경고 0. 누락 meta 0·동결 CSV 해시 일치·SO/게임 규칙/진행 저장 코드 변경 없음 | [정적 검사](evidence/PLAN-015/static-final.json), [Editor](evidence/PLAN-015/editor-final.json), [콘솔](evidence/PLAN-015/console-final.json). 빌드 테스트 미실행 |
| 2026-11-02 | completed 전환·문서/목록/MAP 검사 | PLAN-015 completed와 목록·로드맵·기획 MAP을 동기화했다. active 0은 다음 실행 승인 대기이며 단일 active 요구 오류와 나머지 문서 검사를 구분한다 | [최종 문서 검사](evidence/PLAN-015/docs-final.json). 전체 검사 PASS로 기록하지 않음 |
| 2026-11-02 | 사용자 요청에 따른 main 커밋·푸시 준비 | 원격 main 동기화 상태에서 변경 93개 경로·동결 CSV 5개의 스테이징 바이트/해시·이름 변경한 스크립트 meta 3개의 원본 일치·누락 meta 0을 확인했다. 작성 파일 공백 검사 통과. Unity 생성 meta의 빈 YAML 필드는 보존했다 | 캐시·APK·로컬 도구·개인 저장·인증 정보는 포함하지 않음. 코드 변경이나 테스트 재실행 없이 검증된 작업을 저장소에 반영 |

## 관련 결정과 상태 전환

- 2026-11-02: 미등록 → proposed. 사용자 요청으로 후보를 작성했다. runtime 테스트 포함·빌드 테스트 제외를 계획에 반영했으며 실행 승인으로 해석하지 않는다.
- 2026-11-02: proposed → active. 사용자가 두 보완 사항 반영과 실행을 명시적으로 승인했다. 필드/기본값/해시 계약과 빌드 없는 씬 검사를 보완하고 Edit/Editor Play만 실행한다.
- 2026-11-02: active → completed. 두 보완 사항의 구현과 필수 Edit/Editor Play·정적 검사 및 설정 복원을 마쳤다. 사용자 지시로 빌드/실기기를 제외한 완료 판정이며 다음 PLAN을 임의로 활성화하지 않는다.
- 실행 시 TECH-009의 CSV 취득/표현 부분에 대한 대체 결정을 기록한다. ID 결합·불변 Data·영속 Store·저장 보호 원칙은 유지한다.
- 구현 선택과 대체 관계는 [TECH-012](../decisions/TECH-012-topic-json-catalog.md), 필드 타입·생략·해시 입력 계약은 [JSON 편집 안내](../../UnityProject/Assets/JSON/Balance/README.md), 실행 결과는 [증거 README](evidence/PLAN-015/README.md)에 기록한다.

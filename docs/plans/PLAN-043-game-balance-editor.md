---
id: PLAN-043
type: development
status: completed
title: 게임 밸런스 전용 편집창과 SO Inspector 연결
updated: 2026-11-16
---

# PLAN-043 게임 밸런스 전용 편집창과 SO Inspector 연결

## 목표

Unity Editor에서 기존 JSON의 게임 밸런스 수치를 비교·편집·검증·저장하고, 시설·생명체·자원 SO Inspector에서 대응하는 항목을 바로 연다. JSON을 단일 원본으로 유지하며 연출·애니메이션 설정은 편집 대상에서 제외한다.
기존 형식의 JSON 항목은 자동으로 목록에 나타나고, DTO 필드에 선언한 편집 메타데이터를 읽어 입력 UI를 생성한다. 현재 값은 JSON 원문에서 읽고 변경한 값만 원문에 반영한다. DTO 객체 전체를 JSON으로 재직렬화하지 않는다.

## 범위

- 포함: UI Toolkit 전용 EditorWindow, 검색·주제별 목록·수치 비교·선택 항목 상세, Inspector 진입 버튼, 공통 편집 메타데이터, JSON 초안·검증·저장·되돌리기, 사용 안내와 기능 검증.
- 제외: 연출 시간·애니메이션 속도·프레임·이동 보간·셰이더·외곽선·Sprite·오디오·UI 표시 설정, 프리팹화, 런타임 핫 리로드, QA 상황 편집기, 자동 최적 밸런스 산출, 외부 시트 연동.
- 제외: 콘텐츠 추가/삭제·정의 ID/역할/레시피 종류 변경 UI, SO로 수치 원본 이전, JSON 스키마 완화, 실제 기본 밸런스 값 변경. 테스트용 수치 변경은 검증 후 복원한다.
- 요청 근거: 사용자가 게임 밸런스 전용 창·SO Inspector 연결·자동 확장을 요청했다. 후속 지시로 별도 스키마/필드 목록 없이 DTO에 메타데이터를 붙이고 JSON의 값만 수정하는 방식을 지정했다. 새 JSON 종류도 게임 로더 등록 정보를 함께 사용해 편집창에 별도 등록하지 않도록 한다.
- 승인 상태: 2026-11-16 사용자 「PLAN을 승인할게 작업 진행해줘」에 따라 실행 승인. 이 PLAN만 active로 전환했다.

## 기준 문서와 선행 조건

- 기획: [Thing과 정의 데이터](../game-design/05-development/01-architecture/thing-model.md), [생산시설](../game-design/02-systems/01-production/facilities.md), [저장과 운반](../game-design/02-systems/02-logistics/storage-and-transport.md), [반복 흐름](../game-design/01-direction/02-experience/core-loop.md).
- 구조 결정: [TECH-012 JSON 카탈로그](../decisions/TECH-012-topic-json-catalog.md), [JSON 편집 계약](../../UnityProject/Assets/JSON/Balance/README.md). D-059의 수치 조정 위임과 D-084의 정의/실행 상태 분리를 유지한다.
- 로드맵: [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md)의 프로토타입 완료 이후 개발 도구 개선. 선행 PLAN-015·016·022는 completed이며 완료 기록을 수정하지 않는다. 아트 PLAN-033~042와 독립이다.
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](AGETNS.md), [프로젝트 구조](../agents/project-structure.md), [명명](../agents/naming.md), [스크립트 구조](../agents/script-structure.md), [객체 설계](../agents/object-design.md), [추상화](../agents/abstraction.md), [아키텍처](../agents/architecture.md), [Unity 에셋](../agents/unity-assets.md), [Localization](../agents/localization.md), [검증](../agents/verification.md), [Unity 도구](../agents/unity-tools.md).
- 확인된 환경: Unity 6000.3.27f1, UI Toolkit 기반 TranslationReviewWindow, Newtonsoft JSON 3.2.2, 기존 JSON 파서·GameDataComposer·Editor 검증 메뉴를 재사용한다. 별도 유료 에셋은 필요하지 않다.

## 편집 범위와 초기 필드 분류

숫자 여부가 아닌 게임 규칙에 미치는 역할로 분류한다. 아래 표의 DTO 필드에 최초 BalanceField 메타데이터를 선언하며 화면별 필드 목록/조건문으로 중복 구현하지 않는다. 제외 필드도 파일 안에서는 그대로 보존한다.

| 주제 | 편집 가능한 필드 | 조건·설명 |
| --- | --- | --- |
| Facilities | buildCost, buildSeconds | 건설 비용·실제 건설 시간 |
| Facilities | storageCapacity | 저장소 역할의 용량 |
| Facilities | gatheringAmount, gatheringSeconds | 획득시설의 생산량·작업 시간 |
| Facilities | inputAmount, outputAmount, productionSeconds | 가공·배양의 투입량·산출량·실제 작업 시간. 현재 역할별 제약을 그대로 검사 |
| Creatures | carryCapacity, moveSecondsPerCell | 운반량·논리적 한 칸 이동 시간. 표시 보간/걷기 애니메이션 속도와 구별 |
| Creatures | smallScore, majorScore, supplyAmount, supplyEvery | Raider 행동의 성과·보급 수량/주기만 활성화. Standard 성과는 Invasion 설정에서 편집 |
| Invasion | activitySeconds, smallScore, majorScore, majorEvery, targetScore | 외부 활동 주기·성과·목표 점수 |
| Session | staffedRate, unattendedRate | 실제 생산 속도 배율. 확정 규칙과 현재 기본값을 설명하고 이번 구현에서 값을 임의 변경하지 않음 |
| Resources | 없음 | 현재 수치 필드가 없으므로 조회 전용. Inspector 연결·참조 탐색은 제공 |

- `Session.endingSeconds`는 멸망 연출 시간으로 제외한다. `Invasion.detailedUntilScore`, `reportLimit`는 전과 보고 표시/보관 설정으로 제외한다.
- `definitionId`, `schemaVersion`, `displayNameKey`, `components`, `role`, `activityBehavior`, 자원/생명체 참조 ID·배양 목록은 식별·구조/규칙 정보로 조회 전용이다. UI에서 콘텐츠 정의를 변경하지 않는다.
- 시설 footprint/상호작용 좌표와 맵 크기/시작 위치는 이번 수치 조정 범위에서 제외한다. 게임 공간 설정임을 구분하며 연출 값으로 오분류하지 않는다.
- 비적용 역할의 0/빈 값은 편집하지 않는다. 모든 필드를 펼치는 원시 JSON 편집 기능은 제공하지 않는다.
- 편집 제외 필드와의 교차 제약도 기존 게임 검증을 따른다. 예를 들어 `targetScore`를 현재 `detailedUntilScore`보다 작게 바꾸면 저장을 차단하고, 두 필드의 이름/값과 「보고 기준 점수보다 목표 점수를 낮출 수 없음」을 표시한다. 제외 필드는 오류 원인을 설명하는 읽기 전용 정보로만 제공하며 자동 수정하거나 임시로 편집을 허용하지 않는다.
- 교차 오류에는 가능한 범위에서 주제·정의 ID·관련 필드·원인과 수정 가능한 위치를 제공한다. 기존 검증 코드가 일반 예외만 반환하면 해당 검증 경로의 진단 정보를 최소 보완한다. UI에서 게임 조건을 재구현하거나 예외 메시지 문자열을 해석해 별도 판정하지 않는다.

## 자동 목록과 메타데이터 기반 UI

| 변경 상황 | 자동화 범위 | 추가로 필요한 작업 |
| --- | --- | --- |
| 기존 주제·형식의 JSON entry 추가 | 재읽기 시 ID 기준 목록·검색·상세 자동 생성 | 실제 게임 콘텐츠 추가는 별도 범위. 필요한 SO·Registry·게임 규칙을 정상 등록해야 검증 통과 |
| 기존 DTO에 int/long/double 밸런스 필드 추가 | 자료형에 맞는 입력창·단위·설명·비교 열을 공통 렌더러로 생성 | DTO 필드에 BalanceField 선언, 실행 Data/게임 로직/검증 연결. 별도 편집 필드 목록 등록 없음 |
| 연출·구조 필드 추가 | BalanceField가 없으면 편집 대상으로 노출하지 않음 | 일반 데이터 정의·게임 연결만 수행. 제외 목록을 별도 관리하지 않음 |
| 기존 schemaVersion/entries/definitionId 형식의 새 JSON 주제·DTO 추가 | 게임 로더의 공통 주제 등록을 읽어 목록과 DTO 기반 편집 UI 자동 생성 | 게임 측 파일·주제·DTO 등록과 파싱/실행/검증 연결, DTO에 BalanceField 선언. 편집창용 중복 주제 등록 없음 |
| 지원하지 않는 복합 자료형·새 문서 구조 | 자동 지원으로 간주하지 않고 지원 불가 이유 표시 | 필요한 데이터 로더와 공통 편집기 확장을 별도 판단 |

- 기존 `DefinitionDtos.cs`의 필드에 `BalanceFieldAttribute`를 붙인다. 한국어 이름·단위·설명·그룹/순서·역할/행동 적용 조건을 해당 필드 선언 위치에서 관리한다. 필드명과 자료형은 reflection으로 읽고 메타데이터에 다시 쓰지 않는다. 역할/행동 조건은 기존 DTO 필드를 nameof로 참조하며 중복 게임 규칙은 만들지 않는다.
- BalanceField는 System.Attribute 기반의 순수 C# 메타데이터다. Unity의 HeaderAttribute는 제목 장식일 뿐 편집 허용/단위/조건 계약이 아니므로 대신 사용하지 않는다. Data 어셈블리에 UnityEditor/UI Toolkit 의존성을 추가하지 않는다.
- Editor의 `BalanceMetadataReader`는 DTO 선언으로부터 화면·저장 허용 필드 정보를 도출한다. 별도 스키마 JSON, 수동 BalanceFieldCatalog, DTO와 병행 관리하는 필드 이름 목록은 만들지 않는다. 반사 결과 캐시는 원본 메타데이터가 아니며 재컴파일 시 다시 구성한다.
- BalanceField가 없는 필드는 기본적으로 편집 제외다. 단순히 미표시 필드가 추가된 이유로 저장을 막지 않는다. 속성이 붙은 필드의 이름/설명 누락, 잘못된 조건 참조, 중복/지원하지 않는 편집 타입은 진단하고 저장을 차단한다.
- 창의 읽기 전용 개발자 진단에서 DTO의 필드별 「편집 가능 / 속성 없음 / 역할·행동 조건 불일치 / 미지원 자료형 / JSON 값 없음 / 메타데이터 오류」를 구분한다. 복수 원인은 함께 표시하며 DTO 타입·필드명·자료형·현재 항목 ID·적용 조건을 확인할 수 있게 한다. 진단은 reflection·원문·기존 검증 결과로 자동 생성하며 수동 필드 목록이나 원시 값 수정 기능은 추가하지 않는다.
- 새 주제의 파일·주제·DTO 대응은 게임 로딩에 사용하는 한 등록 경로를 원본으로 삼고 파서·파일 로더·편집창이 함께 참조한다. 현재 JsonDefinitionParser의 Topics/Types와 GameDataAssets.Files의 대응을 확인해 필요한 범위에서 공통 등록을 노출한다. 런타임의 명시적 파일 연결은 유지하며 임의 폴더 검색으로 미등록 JSON을 실행 데이터로 취급하지 않는다.
- 동일 schemaVersion/entries/definitionId와 지원 자료형을 사용하는 새 주제는 공통 등록과 DTO 메타데이터만으로 편집창에 나타나야 한다. 게임 측 새 규칙/카탈로그 결합 코드는 별도이며 폴더에 파일을 놓는 것만으로 게임 동작이 생기지 않는다.
- 공통 주제 등록뿐 아니라 실제 실행에 사용하는 `AppConfiguration.jsonFiles`의 주제·TextAsset 연결과 DTO 대응도 확인한다. 편집 대상 원문이 실제 실행 파일과 동일한지 비교하고 누락·중복·잘못된 파일/타입 연결은 저장을 차단한다. 창을 열었다는 이유로 AppConfiguration이나 씬을 자동 수정하지 않는다. 등록 정보와 실제 에셋 연결을 맞추는 개발 절차를 사용 안내에 기록한다.
- 게임 적용 가능 판정에는 실제 설정으로 구성한 전체 GameDataComposer 검증을 포함한다. 창에 새 주제가 나타나는 것과 새 주제의 게임 로직/카탈로그 연결 완료를 별도로 확인한다. 파서/편집창만 통과한 테스트를 게임 연결 완료 증거로 사용하지 않는다. 일반 Composer 검증은 유지하고 새 게임 주제의 실질 구현은 해당 후속 개발 범위로 둔다.
- 게임 유효성 규칙은 파서·Composer를 원본으로 사용한다. UI 메타데이터는 라벨·노출·입력 편의를 맡고 별도의 게임 규칙 엔진을 만들지 않는다.
- JSON에 임의 키만 추가해 게임 동작이 생기는 시스템은 아니다. 현행 unknown field 오류 정책을 유지한다. 개발자의 새 필드 연결 작업과 사용자의 기존 값 조정을 구분해 안내한다.
- 동일 렌더러를 새 테스트용 항목·지원 타입 필드·공통 등록된 테스트 주제에 적용해 자동 확장을 증명한다. 테스트를 위해 제품 JSON에 가상 필드를 남기지 않는다.

### 데이터 흐름

1. 게임 로더의 등록 정보에서 JSON 파일·주제·DTO 타입을 찾는다.
2. DTO 타입의 필드명·자료형·BalanceField를 읽어 편집 UI를 구성한다. DTO 인스턴스 값은 저장 원본으로 사용하지 않는다.
3. JSON에서 definitionId로 항목을 식별하고 대응하는 필드의 실제 값과 원문 위치를 읽는다. 배열 순서나 표시 이름으로 항목을 식별하지 않는다.
4. 입력값은 원문 기반 초안의 「주제·definitionId·필드명·변경 값」으로 관리한다. DTO의 기본값을 JSON에 채워 넣지 않는다. 생략된 필드는 생략 상태를 표시하고 이번 값 수정 도구에서 새 키를 추가하지 않는다.
5. 저장 후보 원문을 기존 JSON → DTO → GameDataComposer 검증에 통과시킨다. 이 과정의 DTO는 검증 입력이며 저장 직렬화 대상으로 사용하지 않는다.
6. 검증 성공 후 변경한 JSON 값 토큰만 교체한다. DTO → JSON 전체 직렬화, 전체 DOM 재출력, 메타데이터의 JSON 삽입은 하지 않는다.

## 사용 흐름과 Inspector 연결

1. 메뉴 `Tools/Mobile Game/Balance/Open Balance Editor`에서 창을 연다. 시설·생명체·침공·공통 설정·자원 조회를 나누고 이름/ID로 검색한다.
2. 주제별 비교 목록과 선택 항목 상세를 표시한다. 한국어 이름·단위·적용 역할·현재 저장 값·초안 값을 구별한다. 번역 이름을 구할 수 없으면 ID로 식별하며 앱 실행을 요구하지 않는다.
3. `ThingDefinition` 계열 SO Inspector에 「밸런스 편집창에서 열기」 버튼을 제공한다. 기존 ID·icon·prefab Inspector 기능과 직렬화 동작은 보존한다.
4. 버튼은 SO 종류와 definitionId로 정확한 주제/행을 선택한다. 자원은 조회 전용 화면으로 열고, JSON 미등록·중복 ID·다중 SO 선택은 이유를 표시하며 임의 첫 항목으로 이동하지 않는다.
5. 기존 창의 미저장 초안은 다른 항목을 열어도 유지한다. 중복 창·독립 초안으로 같은 JSON을 경쟁 편집하지 않는다. 단일 창의 Editor 수명 내 상태를 사용하며 런타임 싱글톤은 추가하지 않는다.
6. 변경 목록·검증·저장·다시 불러오기·Undo/Redo를 제공한다. 저장은 명시적으로 수행하며 닫기/재읽기/Play 진입 시 미저장 변경 처리(저장/버리기/취소)가 분명해야 한다. 스크립트 재컴파일 후에도 초안을 복구하거나 소실을 막을 수 있도록 Editor 전용 복구 상태를 보존한다.
7. Play 중에는 조회만 제공한다. 저장·편집은 정지 상태에서 하고, 다음 Play 시작에 적용됨을 알린다. 기존 저장과 호환되지 않을 수 있음을 변경 상태와 함께 표시하며 저장 슬롯을 자동 삭제/수정하지 않는다.

### DTO 변경과 초안 복구

- 초안에는 원본 파일 식별자/해시·주제·definitionId·필드명·자료형·기존 값·입력값을 보존한다. 재컴파일·재열기 후 현재 DTO/메타데이터/등록과 원문에 다시 대조한다. 타입명·필드명·배열 위치를 추측해 다른 대상으로 옮기지 않는다.
- 필드 이름 변경/삭제·자료형 변경·항목 ID 변경·원본 교체·편집 속성 제거·적용 조건 변경으로 수정 대상이 달라지면 충돌을 표시한다. 일치하는 초안만 복구 후보로 삼고 미해결 충돌이 있는 동안 저장하지 않는다.
- 이름/설명 같은 표시 메타데이터만 바뀌었다면 현재 메타데이터로 UI를 다시 만들고 동일 대상의 초안을 유지한다. 이름 변경 매핑이나 타입 자동 변환은 이번 범위에 포함하지 않는다.
- 충돌 초안은 사용자가 확인 후 버리거나 올바른 항목에 값을 다시 입력할 수 있게 보존한다. 복구 실패를 이유로 원본 JSON을 수정하거나 기존 초안을 조용히 삭제하지 않는다.

## 저장·검증 계약

- 현재 JSON 5개가 값의 원본이고 향후 공통 등록된 지원 형식의 주제에도 같은 계약을 적용한다. SO에 밸런스 값을 복제하지 않고, DTO Attribute는 JSON 필드나 호환 해시 입력에 포함하지 않는다. DTO의 값 보관·검증 역할과 카탈로그·런타임 수명 계약은 유지한다.
- 전체 원문과 파일별 해시를 읽어 초안을 구성한다. JSON 문자열의 단순 검색/정규식 치환 대신 구조와 원문 토큰 위치를 대응시켜 변경 허용 필드의 값만 수정한다. ID·참조·연출·비편집 필드, 항목 순서, 생략 상태, 미수정 숫자, 공백·줄바꿈·인코딩을 보존한다. 무변경 저장은 파일을 쓰지 않는다.
- int/long/double의 범위·문화권 독립 JSON 숫자·정밀도를 지킨다. 특히 long을 float/double 입력으로 우회하지 않는다. 새 값도 기존 엄격 파서로 재검사하고 호환 해시를 임의 보정하지 않는다.
- 저장 전 초안과 미변경 주제를 합친 GameDataBundle을 실제 Registry와 GameDataComposer로 검사한다. 잘못된 참조, 비양수 시간, 초기 운반량 조건 등 교차 규칙 실패 시 파일을 쓰지 않는다.
- 쓰기 직전 관련 원본 변경을 다시 확인한다. 외부 수정·삭제를 발견하면 충돌 위치를 표시하고 저장을 중단한다. 사용자의 재읽기/초안 버리기 없이 외부 내용을 덮어쓰지 않는다.
- 저장 완료는 「모든 변경 파일 쓰기 → 재임포트 → 실제 실행 설정을 사용한 전체 카탈로그 최종 검증」이 성공한 시점이다. 그전까지 원문 복구본·후보 원문·처리 단계를 보존하고 초안 기준선을 갱신하지 않는다. 여러 파일을 쓰는 중 실패뿐 아니라 재임포트·최종 검증 실패도 동일 복구 대상으로 다룬다.
- 임시본/복구본/저장 기록은 Assets 밖의 `UnityProject/Library/BalanceEditor/Recovery/<작업 ID>/`에 둔다. 원본의 `.meta`는 교체하지 않는다. 복구 시 현재 파일이 도구가 마지막으로 쓴 후보 해시와 일치하는 경우에만 원문으로 되돌리고 재임포트·검증한다. 현재 파일이 이미 원문이면 유지한다. 외부 변경/삭제 등 다른 상태이면 덮어쓰지 않고 충돌과 복구 자료 위치를 표시한다.
- 완료·실패 후 복구 완료·복구 미완료를 구분한다. 최종 성공이나 확인된 복구 완료 전에는 자료를 지우지 않는다. 복구 미완료 상태에서는 추가 저장을 막고 원문·후보·초안을 보존한다. 정상 완료한 해당 작업의 임시 자료만 정리한다.
- 비정상 종료 뒤 창을 열면 미완료 저장 기록을 감지한다. 처리 단계·원본/후보/현재 해시를 대조해 상태와 복구 방법을 표시하고, 사용자가 복구를 실행하기 전에는 자동으로 원본을 덮어쓰지 않는다. 재시작 후에도 같은 외부 수정 보호 조건을 적용한다. Library 삭제에 따른 복구 자료 소실은 보장 범위 밖임을 사용 안내에 적는다.
- 초안 기준선은 최종 성공 때만 갱신한다. 창의 Undo/Redo는 초안 이력이며 저장된 파일 변경은 다시 명시적으로 저장해야 함을 표시한다.

## 결과물 저장 위치

아래 경로의 기준 루트는 `C:/Dev/Unity/Doom_Inc`다. 구현·검증 결과의 실제 위치를 기록한다. Unity 신규 스크립트의 `.meta`는 동일 경로에 Unity가 생성하며 기존 GUID는 보존한다.

| 산출물 | 기준 루트 아래 경로·파일명 | 상태 |
| --- | --- | --- |
| 전용 창 | `UnityProject/Assets/Scripts/Editor/Definitions/Balance/BalanceEditorWindow.cs` | 생성 완료 |
| DTO 필드별 메타데이터 선언 | `UnityProject/Assets/Scripts/Data/Loading/DefinitionDtos.cs` | 변경 완료. 기존 필드에 속성만 추가, 값 스키마 유지 |
| 메타데이터 속성 정의 | `UnityProject/Assets/Scripts/Data/Loading/BalanceFieldAttribute.cs` | 생성 완료. 순수 C#, UnityEditor 의존 없음 |
| DTO 메타데이터 조회 | `UnityProject/Assets/Scripts/Editor/Definitions/Balance/BalanceMetadataReader.cs` | 생성 완료. 수동 필드 목록 없음 |
| 공통 주제·DTO 등록 | `UnityProject/Assets/Scripts/Data/Loading/JsonDefinitionTopics.cs` | 생성 완료. 기존 주제/타입 대응을 추출해 파서·Editor가 함께 사용 |
| 공통 주제 등록 소비 | `UnityProject/Assets/Scripts/Data/Loading/JsonDefinitionParser.cs` | 변경 완료. 주제/타입 대응 공유, 엄격한 JSON 검증 계약 유지 |
| 초안·Undo/복구 상태 | `UnityProject/Assets/Scripts/Editor/Definitions/Balance/BalanceEditSession.cs` | 생성 완료. Editor 수명 한정, 제품 SO 인스턴스 생성 없음 |
| JSON 읽기·변경·저장 | `UnityProject/Assets/Scripts/Editor/Definitions/Balance/BalanceDocumentStore.cs` | 생성 완료 |
| 원문 토큰 위치/문자열 해석 | `UnityProject/Assets/Scripts/Editor/Definitions/Balance/BalanceJsonToken.cs` | 생성 완료 |
| 실제 설정/Composer 연결 | `UnityProject/Assets/Scripts/Editor/Definitions/Balance/BalanceEditorData.cs` | 생성 완료 |
| SO Inspector 진입 | `UnityProject/Assets/Scripts/Editor/Definitions/Balance/ThingDefinitionEditor.cs` | 생성 완료 |
| 파일 등록·전체 데이터 검증 진입 | `UnityProject/Assets/Scripts/Editor/Definitions/GameDataAssets.cs` | 변경 완료. 공통 주제 등록 소비·초안 검증 진입 최소 변경 |
| 실제 실행 설정과 대조 | `UnityProject/Assets/Scripts/Presentation/Bootstrap/AppConfiguration.cs`, `UnityProject/Assets/SO/Application/AppConfiguration.asset` | 재사용·조회/검증. 도구가 설정을 자동 수정하지 않음 |
| 교차 오류의 진단 정보 | `UnityProject/Assets/Scripts/Data/Definitions/Invasion/InvasionSettings.cs`, `UnityProject/Assets/Scripts/Data/Catalog/GameDataCatalog.cs` | InvasionSettings 진단 보완 완료, GameDataCatalog는 재사용. 게임 조건 변경 없음 |
| 자동 검증 | `UnityProject/Assets/Scripts/Tests/EditMode/Foundation/Definitions/BalanceEditorTests.cs` | 생성 완료 |
| Play 검증 | `UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/BalanceEditorPlayTests.cs` | 생성 완료 |
| 기존 파서 회귀 | `UnityProject/Assets/Scripts/Tests/EditMode/Foundation/Definitions/JsonGameDataTests.cs` | 재사용 |
| 게임 수치 원본 | `UnityProject/Assets/JSON/Balance/Facilities.json`, `Creatures.json`, `Invasion.json`, `Session.json`, `Resources.json` | 모두 같은 Balance 폴더의 기존 파일 재사용. 기본값 변경 없음 |
| 사용·확장 안내 | `UnityProject/Assets/JSON/Balance/README.md` | 변경 완료. 필드/entry 추가 절차와 제외 범위 설명 |
| PLAN·MAP | `docs/plans/PLAN-043-game-balance-editor.md`, `docs/plans/README.md`, `docs/game-design/README.md`, `docs/game-design/05-development/02-implementation/roadmap.md` | 이번 작성/변경 |
| 검수 색인 | `docs/plans/evidence/PLAN-043/README.md` | 생성 완료 |
| 문서 검사기 | `scripts/validate-agent-docs.mjs` | evidence Markdown의 PLAN 오인 집계 수정. PLAN 본문은 지침상 위치만 검사하고 전체 링크 검사는 유지 |
| 기능·복구 검증 | `docs/plans/evidence/PLAN-043/run-001/editmode-results.xml`, `editor.log`, `manual-checks.md`, `restoration.md` | 모두 같은 run-001 폴더, 생성 완료 |
| 최종/회귀/Play 자동 결과 | `docs/plans/evidence/PLAN-043/run-001/editmode-completion.xml`, `balance-tests-completion.json`, `json-regression.xml`, `json-regression.json`, `play-reload-off.xml`, `play-reload-on.xml` | 같은 run-001 폴더, 생성 완료. 초기 결과도 보존 |
| 원본 해시 비교 | `docs/plans/evidence/PLAN-043/run-001/baseline-hashes.json`, `restoration-hashes.json` | 같은 run-001 폴더, 70개 일치 |
| 화면 증거 | `docs/plans/evidence/PLAN-043/run-001/balance-window.png`, `inspector-entry.png`, `validation-error.png` | 같은 run-001 폴더, 생성 완료 |
| 다섯 보완사항 검증 기록 | `docs/plans/evidence/PLAN-043/run-001/review-cases.md` | 생성 완료. 교차 오류·실행 연결·초안 충돌·복구 단계·필드 진단 결과 |
| 저장 작업의 임시 복구 자료 | `UnityProject/Library/BalanceEditor/Recovery/<작업 ID>/journal.json`, `original/<순번>.json`, `candidate/<순번>.json` | 저장 실행 중 생성. 완료 후 snapshot 정리. 같은 작업 폴더 기준, 작업 ID는 매회 새 GUID. Git 추적/제품 데이터 아님 |

- 추가 검증은 `run-002`부터 증가시키고 이전 증거를 덮어쓰지 않는다. 미실행 자료는 미생성 이유를 색인에 적는다. 테스트 임시 파일은 테스트별 격리 임시 경로를 사용하고 제품 JSON·저장 슬롯을 복구한 결과를 남긴다.
- UI는 UI Toolkit C# 구성으로 시작하며 UXML/USS·별도 패키지·새 어셈블리는 기본 산출물에 포함하지 않는다. 필요해지면 구현 전에 이 표에 구체 경로·이유를 갱신한다.
- Sprite·원화·애니메이션 산출물과 아트 요청은 해당 없음이다.

## 작업 목록

- [x] 요청 범위·게임 수치/표현 필드 경계·산출물 경로·자동 확장 계약 지정
- [x] 실행 승인 후 유일한 active로 전환하고 기존 작업 상태 보존 확인
- [x] DTO의 대상 필드에 BalanceField를 선언하고 공통 메타데이터 조회·유효성 검사를 구현
- [x] 파서·게임 파일 로더의 공통 주제/DTO 등록을 공유하고 편집창의 중복 주제 목록을 제거
- [x] JSON 초안·정밀도/비편집 필드 보존·교차 검증·충돌 감지·저장 실패 복원을 구현
- [x] 비교/상세 편집 UI·Undo/Redo·미저장 처리·재컴파일 복구·Play 읽기 전용을 구현
- [x] 기존 SO Inspector 유지 및 ID 기반 창 선택 연결을 구현
- [x] 편집 제외 필드와의 교차 오류에 관련 필드/값/원인을 표시하고 제외 값의 불변을 확인
- [x] 공통 등록·DTO·실제 AppConfiguration 파일 연결 일치 검사와 카탈로그 검증을 연결
- [x] DTO/원문 변경 이후 초안 재검증·충돌 보존·잘못된 대상 적용 차단을 구현
- [x] 재임포트/최종 검증 실패·비정상 종료까지 저장 복구 범위를 완성하고 외부 수정 보호를 구현
- [x] 필드가 표시되지 않는 이유와 잘못된 메타데이터를 읽기 전용 개발자 진단에 표시
- [x] 사용 안내·새 entry/필드 추가 절차를 작성
- [x] 자동·수동 Editor 검증과 테스트 데이터/환경 복원을 완료하고 증거 기록

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 게임 밸런스만 편집 가능 | 초기 분류·역할별 입력·수정 차이 확인. endingSeconds/보고 표시/에셋 필드 비노출·보존 검사 | EditMode 결과·manual-checks.md |
| 데이터 증가 시 공통 UI 재사용 | 기존 형식 entry, 속성을 붙인 지원 타입 DTO 필드, 공통 등록된 새 테스트 주제를 추가. 별도 필드/주제 목록이나 창 수정 없이 탐색·편집 확인. 속성 없는 필드는 비노출, 잘못된 속성은 진단/차단 | EditMode 결과 |
| Inspector 대상 정확성 | 시설·생명체·자원, 미등록/중복 ID·다중 선택, 창 재사용·초안 보존·기존 에셋 필드 확인 | inspector-entry.png·manual-checks.md |
| 검증이 실제 게임과 일치 | 정상 변경, 타입/범위 오류, 역할 조건, 초기 비용 합계>운반량 등 교차 오류. 런타임 파서·Composer와 같은 판정 | EditMode 결과·validation-error.png |
| 저장 무손실·실패 보호 | 원문 diff에서 지정 ID/필드의 값 토큰만 변경됨을 확인. 동일 필드가 있는 다른 ID·문자열 속 필드명·고정밀 숫자/Int64·생략 필드·공백/인코딩 보존. 무변경 저장·외부 수정/삭제·두 번째 파일 쓰기 실패/복원·재임포트 확인 | EditMode 결과·restoration.md |
| DTO 메타데이터와 값의 분리 | Attribute만 추가/수정해도 JSON 키·내용·실행 값·호환 해시 불변. JSON → DTO 검증은 유지되고 DTO 전체 직렬화·생략 필드의 기본값 추가가 없는지 확인 | EditMode 결과 |
| 초안 수명·조작 | Undo/Redo·저장·버리기·취소·창 재열기·스크립트 재컴파일·Play 진입, Domain Reload ON/OFF 확인 | manual-checks.md·editor.log |
| 다음 실행에서만 적용 | 대표 건설/생산 수치를 임시 변경·저장하고 새 Play에서 카탈로그/실제 작업 시간 확인. Play 중 편집/저장 차단, 비호환 저장 자동 변경 없음 | manual-checks.md·restoration.md |
| 기존 계약 유지 | 기존 JsonGameDataTests 및 변경 영향에 해당하는 저장 호환 테스트. JSON/SO·씬·사용자 저장·Editor 설정의 검증 전후 복원 | EditMode 결과·restoration.md |
| 제외 필드 의존 오류 설명 | targetScore를 detailedUntilScore보다 낮춘 초안의 저장 거부·두 값/원인 표시·제외 값 불변. 교차 오류 판정을 UI에 복제하지 않았는지 확인 | EditMode 결과·review-cases.md·validation-error.png |
| 편집 대상과 실행 파일 일치 | 공통 등록/실행 jsonFiles의 누락·중복·같은 주제의 다른 파일·DTO 불일치 감지와 저장 차단. 정상 설정에서 실제 카탈로그 생성 및 다음 Play 적용 확인 | EditMode 결과·review-cases.md |
| 변경된 DTO의 초안 보호 | 필드 삭제/이름/타입 변경, 속성 제거/조건 변경, JSON 항목/원본 변경 시 충돌·저장 차단·초안 보존. 표시 이름/설명만 바뀌면 정상 복구 | EditMode 결과·review-cases.md |
| 저장 종료까지 복구 보장 | 파일 쓰기·재임포트·최종 검증 실패를 단계별 주입. 미완료 journal로 재시작 상태를 재현하고 복구·외부 수정/삭제 보호·복구 실패 시 자료 유지/추가 저장 차단 확인. 실제 Editor 강제 종료는 요구하지 않음 | EditMode 결과·review-cases.md·restoration.md |
| 필드 진단의 정확성 | 편집 가능·속성 없음·역할 불일치·미지원 타입·JSON 값 없음·메타데이터 오류를 각각 재현. 원인/대상 식별·읽기 전용·별도 필드 목록 없음 확인 | EditMode 결과·review-cases.md |
| 문서 일관성 | node scripts/validate-agent-docs.mjs, PLAN·목록·기획 MAP·로드맵 대조 | PLAN 검증 결과 |

Editor 도구 범위에 맞춘 일반 검증을 수행한다. Player/Android 빌드·실기기·성능/고부하 검증은 요청되지 않아 제외한다. 문서 작성 단계에서는 게임 테스트를 실행하지 않는다.

## 후속 개선사항 — 이번 구현·완료 기준에서 제외

현재 자동 편집 지원은 기존 문서 구조의 최상위 int/long/double 필드다. 아래는 필요가 확인될 때 별도 PLAN으로 구체화할 후보이며, 이번에 기반 구현을 미리 추가하거나 완료 조건으로 요구하지 않는다.

| 후보 | 검토할 시점·필요 | 후속 범위와 경계 |
| --- | --- | --- |
| 배열·중첩 객체 편집 | 레벨별 비용·확률표·복수 레시피 같은 구조가 실제 도입될 때 | 항목 식별·중첩 경로·부분 수정·배열 순서 변경 계약과 UI 확장. 현재 수치 필드 자동 지원과 구별 |
| 데이터 마이그레이션 | 새 필드·이름 변경·필수 값 추가로 기존 JSON 갱신이 필요할 때 | 명시적 버전/변환·미리보기·복구·호환성 검증. 일반 밸런스 저장에서 키/기본값을 자동 추가하지 않음 |
| 대량 비교·일괄 편집 | 항목 수가 늘어 개별 입력의 반복 부담이 확인될 때 | 열 정렬·조건 필터·일괄 증감·붙여넣기·변경 미리보기. 현재 기본 검색/비교 목록은 유지 |
| 여러 밸런스안 관리 | 실험안 A/B 비교·재적용이 필요할 때 | 기준 대비 차이·실험안 저장/불러오기·적용. 초안 Undo/Redo나 장애 복구 자료를 실험안 저장소로 사용하지 않음 |
| 표시 단위 변환 | 확률 0~1↔퍼센트, 초↔분 등 변환 입력이 필요할 때 | 저장/표시 단위·반올림·정밀도·왕복 변환 규칙. 이번 Unit 메타데이터는 라벨이며 수치를 변환하지 않음 |

후속 작업은 JSON 단일 원본·DTO 메타데이터·게임 검증 재사용 원칙을 유지하고, 저장 스키마/게임 규칙 변경이 필요한 부분은 그 작업에서 별도로 명시한다.

## 현재 상황과 다음 행동

- 현재: 범위 안의 구현·Editor 검증·원본 복원을 완료했다. DTO 기반 입력, JSON 값 토큰 저장, Inspector 연결, Undo/Redo·초안 복구, 실행 연결 검사·저장 실패 복구·개발자 진단을 제공한다.
- 구현 선택: UI Toolkit C# 구성, 정밀도 보존용 지연 TextField 입력과 DTO 숫자 검증, 기존 Composer 재사용. Editor의 Newtonsoft 중복 타입 충돌을 피하기 위해 원문 토큰 디코딩과 Unity JsonUtility 기반 복구 journal을 분리했다. 게임 JSON은 재직렬화하지 않는다.
- 완료 근거: [검증 색인](evidence/PLAN-043/README.md). EditMode 64개·PlayMode 1개(ON/OFF 각각 실행) 통과, 원본 70개 해시 일치. 사용자 기본 밸런스·씬·SO는 변경하지 않았다.
- 다음 행동: 승인된 후속 PLAN은 없으므로 active 0개로 대기한다. 필요 시 위 후속 개선사항을 별도 PLAN으로 구체화한다.
- 막힌 점: 없음. 빌드·실기기·성능/고부하는 범위 밖이며 미측정이다. Sprite 요청 없음.

## 검증 결과

| 날짜 | 대상·환경·실행 방법 | 기대 결과·실제 결과 | 판정·증거 |
| --- | --- | --- | --- |
| 2026-11-16 | 작성 전 문서 검사 | PLAN 42개·active 0개·목록/링크 검사 통과 | PASS, 작성 전 검사 출력 |
| 2026-11-16 | node scripts/validate-agent-docs.mjs, git diff --check | PLAN 43개·active 0개·목록 43개·로컬 링크 3926개 검사 통과, 공백 오류 없음 | PASS, 문서 검사 출력. 기존 작업 파일의 줄바꿈 경고는 내용 오류와 구분 |
| 2026-11-16 | 구현·Editor 기능 검증 | 승인 전이므로 실행하지 않음 | 미실행 |
| 2026-11-16 | DTO Attribute 방식 보완 후 문서 검사·관련 문서 diff --check | PLAN 43개·active 0개·로컬 링크 3926개 통과. DTO 전체 재직렬화/별도 필드 목록 대신 값 토큰 수정 계약과 MAP 일치 확인 | PASS, 문서 검사 출력. Unity 구현 검증은 미실행 |
| 2026-11-16 | 다섯 보완사항·후속 후보 분리 후 문서 검사 | PLAN 43개·active 0개·로컬 링크 3926개 통과. 다섯 항목의 명세/작업/검증 대응과 후속 기능의 범위 제외 확인 | PASS, 문서 검사 출력. 코드/에셋 변경 및 Unity 검증 없음 |

| 2026-11-16 | Unity 6000.3.27f1 / 설치된 CLI 1.0.0-beta.17, Editor 재컴파일 | 컴파일 성공, 최종 현재 Console 오류/경고 0 | PASS, [로그](evidence/PLAN-043/run-001/editor.log) |
| 2026-11-16 | BalanceEditorTests / JsonGameDataTests | 최종 30/30 + 기존 회귀 34/34 통과. 초기 26개 실행과 최종 30개를 중복 합산하지 않음 | PASS, [최종](evidence/PLAN-043/run-001/editmode-completion.xml), [회귀](evidence/PLAN-043/run-001/json-regression.xml) |
| 2026-11-16 | 새 Play 적용·건설 진행·Play 읽기 전용, Domain Reload OFF/ON | 동일 테스트 각각 1/1 통과. 저장한 건설 시간 6초가 실제 로딩/진행에 반영 | PASS, [OFF](evidence/PLAN-043/run-001/play-reload-off.xml), [ON](evidence/PLAN-043/run-001/play-reload-on.xml) |
| 2026-11-16 | 창/Inspector·Undo/Redo·재컴파일·닫기 안내·다섯 보완사항 | 수동 Editor/자동 검증 통과. 테스트 초안 폐기·기본 수치 복원 | PASS, [수동](evidence/PLAN-043/run-001/manual-checks.md), [보완사항](evidence/PLAN-043/run-001/review-cases.md) |
| 2026-11-16 | 제품 JSON·SO·씬·Editor 설정 원본 비교 | 시작/종료 SHA-256 70/70 일치, 사용자 진행 저장 수정 없음 | PASS, [복원](evidence/PLAN-043/run-001/restoration.md) |
| 2026-11-16 | 완료 전 문서 검사·변경 파일 diff --check | PLAN 43개·active 0개·목록 43개·로컬 링크 3955개 통과, 공백 오류 없음. evidence 문서의 PLAN 오인 집계를 지침상 본문 위치 기준으로 수정 | PASS, 검사 출력. 줄바꿈 정규화 경고와 내용 오류를 구분 |

## 관련 결정과 상태 전환

- 결정: JSON 단일 원본과 기존 카탈로그 검증을 유지한다. DTO 필드의 순수 C# Attribute를 Editor 공통 UI가 읽고 JSON의 지정 값만 수정한다. 게임과 편집창의 주제 등록도 공유하며 런타임에 UI 의존성을 추가하지 않는다.
- 2026-11-16: 신규 → proposed. 사용자 PLAN 작성 요청 및 자동 확장 질의를 반영했다. 구현 미착수, active 0개 유지.
- 2026-11-16: proposed 유지. 사용자 요청에 따라 별도 BalanceFieldCatalog/스키마 관리 대신 DTO Attribute로 보완하고, DTO 전체 재직렬화 금지·JSON 값만 수정·게임 등록 공유에 따른 새 주제 자동 노출을 명시했다. 구현 승인으로 해석하지 않았다.
- 2026-11-16: proposed 유지. 사용자가 승인한 문서 보완 범위로 교차 의존 오류·실행 파일 연결·DTO 변경 초안·저장 완료/복구·필드 진단의 다섯 항목을 필수 작업/검증에 반영하고 미래 확장은 후속 후보로 분리했다. 구현 미착수·active 0개 유지.

- 2026-11-16: proposed → active. 사용자 실행 승인에 따라 PLAN-043만 활성화. 다른 PLAN·기존 사용자 변경을 유지한다.

- 2026-11-16: active → completed. 승인 범위 구현·필수 Editor 검증·원본 복원 완료. 후속 후보는 미착수, active 0개 유지.

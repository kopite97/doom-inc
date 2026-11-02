# 주제별 게임 정의 JSON

이 디렉터리가 정의·밸런스 값의 제품 원본이다. SO Registry는 같은 정의 ID의 Unity 에셋 참조를 담당한다. 진행 저장과 UI 환경설정 파일은 이 디렉터리에 두지 않는다.

## 파일과 편집

| 주제 키 | 파일 | 항목 |
| --- | --- | --- |
| resources | [Resources.json](Resources.json) | 자원 2종 |
| creatures | [Creatures.json](Creatures.json) | 생명체 3종 |
| facilities | [Facilities.json](Facilities.json) | 시설 4종 |
| invasion | [Invasion.json](Invasion.json) | 침공 설정 1개 |
| session | [Session.json](Session.json) | 시작/공통 설정 1개 |

각 파일은 UTF-8 객체 `{ "schemaVersion": 1, "entries": [...] }`다. 주제 키와 TextAsset을 Bootstrap에 명시적으로 연결한다. 파일 이름·배열 순서가 정의 ID를 대신하지 않는다.

- 일반 값 변경은 JSON을 수정한다. 자원·생명체·시설을 추가하면 같은 ID의 SO와 Registry 등록을 함께 준비한다. 기존 ID와 SO GUID를 표시 문구 수정용으로 바꾸지 않는다.
- 주석·후행 쉼표·중복 속성·알 수 없는 필드·null은 오류다. enum은 이름 문자열, components는 이름 문자열 배열이다. 정수/실수는 따옴표 없는 JSON number다.
- 아래 필수 필드와 역할별 조건을 따른다. 필수 값 누락을 기본값으로 복구하지 않는다. 문자열 ID는 앞뒤 공백 없이 입력한다.
- `Tools/Mobile Game/Validate JSON and Registries`로 데이터, `Validate JSON Scene References`로 저장된 씬 3개의 의존성을 검사한다. 두 메뉴는 Player 컴파일/빌드를 수행하지 않는다.
- 편집 후 Play를 다시 시작한다. 같은 Play 실행의 새 판·씬 전환은 이미 준비된 GameDataStore를 재사용한다. 실제 원격 취득·핫 리로드는 제공하지 않는다.
- 값이 달라 호환되지 않는 진행 저장은 보존하고 이어하기를 거부한다. 표시 이름·속성/항목 순서·서식만 바꾸면 호환성을 유지한다.

## 필드 타입·필수 여부

모든 항목의 `definitionId`는 필수 문자열이며 전체 주제에 걸쳐 유일하다. 자원·생명체·시설에는 `displayNameKey`([Definitions 번역](../../CSV/Localization/README.md)의 비어 있지 않은 키)와 `components`(중복 없는 enum 문자열 배열)가 추가로 필수다. 설정 항목에는 이 두 필드를 넣지 않는다. 표시 이름 키는 저장 호환성 해시에서 제외한다.

| 주제 | 필드 | JSON 타입 → DTO/실행 타입 | 필수·생략 규칙 |
| --- | --- | --- | --- |
| creatures | role | 문자열 → CreatureRole | 필수: Prototype/Support/Invader |
| creatures | carryCapacity | number → int | 필수, 양수 |
| creatures | moveSecondsPerCell | number → double → float | 필수, 양의 유한 실행 값 |
| facilities | role | 문자열 → FacilityRole | 필수: 현재 Gathering/Storage/Processing/Cultivation |
| facilities | width, height, interactionX, interactionY | number → int | 전 역할 필수. 크기는 양수, 작업 위치는 footprint 외곽 인접 칸 |
| facilities | resource | 문자열 → 자원 ID | 전 역할 필수 |
| facilities | buildCost, buildSeconds | number → int / double→float | 전 역할 필수, 양수 |
| facilities | storageCapacity | number → int | Storage 필수, 다른 역할은 생략 시 0 |
| facilities | gatheringAmount, gatheringSeconds | number → int / double→float | Gathering 필수, 다른 역할은 생략 시 0 |
| facilities | inputResource, inputAmount, outputAmount, productionSeconds | 문자열 / number → ID / int / int / double→float | Processing/Cultivation 필수. 다른 역할은 ID 빈 문자열·숫자 0 |
| facilities | outputResource | 문자열 → 자원 ID | Processing 필수, 다른 역할은 빈 문자열 |
| facilities | supportOutput, invaderOutput | 문자열 → 생명체 ID | Cultivation 필수, 다른 역할은 빈 문자열 |
| invasion | activitySeconds | number → double | 필수, 양수 |
| invasion | smallScore, majorScore, majorEvery, reportLimit | number → int | 필수, 기존 침공 설정 유효성 규칙 적용 |
| invasion | targetScore, detailedUntilScore | number → long | 필수, 반올림 없이 Int64 범위/규칙 검사 |
| session | mapWidth, mapHeight, startX, startY | number → int | 필수, 양의 크기·Map 내 시작 위치 |
| session | organicId, cultureId, gatheringId, storageId, prototypeId, processingId, cultivationId, supportId, invaderId | 문자열 → 해당 종류 정의 ID | 모두 필수, 참조 종류·역할 검사 |
| session | staffedRate, unattendedRate, endingSeconds | number → double | 필수, 양수·자동 속도≤작업자 속도 |

미사용 필드를 명시하면 그 타입과 값도 검사하고 해시에 포함한다. `null`로 생략을 표현하지 않는다. 자원은 ResourceStack만 가지며 시설에는 ResourceContainer가 필요하다. 기존 생산·초기 운반량·자원 관계 검증도 적용한다.

## 해시 입력 계약

- 원문 SourceHash는 파일 내용의 출처 확인용이다. 저장 호환성 CompatibilityHash와 용도가 다르다.
- 주제/정의 ID/필드 이름을 Ordinal 정렬하고, 표시 이름을 제외한 전체 필드에 기존 길이 접두 토큰 계약을 적용한다. 역할별 생략 필드도 위 기본값으로 채워 포함한다.
- ID·enum은 문자열, components는 배열 순서대로 `|`로 연결한다. 숫자는 원문 토큰을 정규화해 `1.0`/`1e0`→`1`, `25e-2`→`0.25`, `-0`→`0`으로 만든다. 해시 계산은 double/float 반올림 전에 수행한다.
- 정수는 소수부가 없는지와 int/long 범위를 원문 정규화 값으로 검사한다. 실수는 유한 double이며, float 저장 필드는 최종 실행 값도 검증한다. 비유한 수·지원 범위 밖 크기/지수·0으로 언더플로하는 양은 거부한다.
- 현행 배포 값의 CompatibilityHash는 `2578869b88b49ba4855785cf587d8cf2f0cfd0dd8ba1fbfc2cd3fb8d2ca7f195`다. 밸런스 수정 때문에 이 기준 상수를 갱신하지 않는다. 임의로 수정했던 과거 CSV의 숫자 표기까지 모두 호환한다고 보장하지 않는다.

구현 선택·검증 범위는 [PLAN-015](../../../../docs/plans/PLAN-015-topic-json-definition-data.md)를 따른다. 이번 전환에서는 Editor 테스트만 수행하며 빌드·실기기 검증은 제외한다.

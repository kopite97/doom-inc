# 밸런스 편집창 사용과 데이터 확장

적용: 게임 수치 조정·QA, 기존 JSON 항목 추가, DTO 밸런스 필드·새 JSON 주제 연결.
탐색: [작업 MAP](../../AGENTS.md), [JSON 형식·필수 필드·호환 해시](../../UnityProject/Assets/JSON/Balance/README.md), [아키텍처](architecture.md), [검증](verification.md).
구현·검증 이력은 [PLAN-043](../plans/PLAN-043-game-balance-editor.md)을 참조한다. 아래 예시는 사용법이며 기본 밸런스 변경 지시가 아니다.

## 기존 수치를 수정하고 게임에 적용하기

1. Unity의 Play를 정지한다. 실행 중에는 밸런스 창을 조회만 할 수 있다.
2. `Tools → Mobile Game → Balance → Open Balance Editor`를 연다. 시설·생명체·자원 SO Inspector의 `밸런스 편집창에서 열기`로도 해당 ID를 바로 연다. SO는 하나만 선택한다.
3. 주제 탭에서 항목을 선택한다. 목록은 정의 ID와 수치 요약이며 검색은 ID/표시 이름 키로 한다.
4. 수치를 입력하고 **Enter 또는 다른 입력칸으로 이동**해 초안에 반영한다. 소수점은 `.`을 쓰고 숫자에 단위·쉼표·따옴표를 넣지 않는다.
5. `저장 값`과 `변경 내역`을 비교한 뒤 `검증`을 누른다. 다른 항목으로 이동해도 초안은 유지되며 저장 대상은 창에 쌓인 전체 변경이다.
6. 오류가 없으면 `JSON 저장`을 누르고 저장·재임포트·최종 검증 완료 안내를 확인한다. SO나 씬에 별도로 Apply할 필요는 없다.
7. Play를 다시 시작해 새 게임에서 확인한다. 같은 Play 안의 재시작·씬 이동만으로는 이미 로딩한 수치가 갱신되지 않는다.

| 조정할 내용 | 창의 항목 | JSON 필드 | 대상 |
| --- | --- | --- | --- |
| 건설 비용·시간 | 건설 비용 / 건설 시간 | buildCost / buildSeconds | 모든 시설 |
| 회당 획득량·주기 | 획득량 / 획득 시간 | gatheringAmount / gatheringSeconds | Gathering |
| 회당 재료 소비·산출 수량 | 투입량 / 산출량 | inputAmount / outputAmount | Processing·Cultivation |
| 회당 생산 기준 시간 | 생산 시간 | productionSeconds | Processing·Cultivation |
| 저장 가능한 수량 | 저장 용량 | storageCapacity | Storage |
| 작업자 유무에 따른 생산 속도 | 작업자 생산 배율 / 무인 생산 배율 | staffedRate / unattendedRate | 공통 설정 탭 |

예: Gathering의 `건설 시간`을 3→5로 입력·검증·저장하면 다음 Play의 건설 작업 필요 시간은 5초다. 재료 운반·이동·작업 대기는 별도이므로 클릭부터 완공까지의 총 시간과 구분한다.
가공시설의 `산출량`을 바꾸면 한 회차의 결과 수량이 바뀐다. `생산 시간`은 기준 작업 시간이며 실제 속도는 작업자 배율·재료/공간 대기 등에 영향을 받는다. 배양 등 역할별 수량 제약은 기존 게임 검증을 따른다.

## 초안·오류·복구 처리

- `Undo/Redo`는 초안 이력이다. 저장이 성공하면 이력을 비우므로 저장된 값을 되돌리려면 이전 값을 다시 입력·저장한다.
- `다시 읽기 / 버리기`는 초안을 버리고 디스크 JSON을 다시 읽는다. 창 닫기·Play 진입·Editor 종료 시 미저장 변경의 저장/버리기/취소를 선택한다.
- 외부에서 JSON 또는 DTO 구조를 변경하기 전 초안을 정리한다. 재컴파일 후에도 초안은 유지되지만 원본·필드·타입·조건이 달라지면 충돌로 저장을 막는다. 필요한 입력값을 확인한 뒤 버리고 다시 입력한다.
- 검증은 실제 게임의 파서·Composer를 사용한다. 예: 목표 점수가 읽기 전용 보고 기준 점수보다 낮거나 초기 건설 비용 합계가 시작 생명체의 운반량을 넘으면 오류다. 관련 값과 규칙을 확인해 편집 가능한 값을 수정한다.
- 파일 쓰기·재임포트·최종 검증 중 실패하면 원본 복구를 시도한다. 외부 수정/삭제는 덮어쓰지 않는다. `미완료 저장 기록`이 있으면 표시된 경로와 충돌 원인을 확인하고 `원본 복구`를 사용한다.
- 복구 자료는 `UnityProject/Library/BalanceEditor/Recovery/<작업 ID>/`의 journal·원본·후보 파일이다. Library 삭제 시 사라지므로 장기 백업이나 밸런스 실험안 저장소로 사용하지 않는다.
- 값이나 데이터 구조 변경으로 기존 진행 저장과 호환되지 않을 수 있다. 도구는 사용자 저장을 자동 삭제/변환하지 않는다. QA는 새 게임에서 수행하고 기존 진행 보존 여부를 별도로 확인한다.

## 자동으로 처리되는 범위

JSON은 값의 단일 원본이다. DTO는 로딩·검증 입력이며, DTO 필드의 `BalanceField`는 편집 허용·표시 이름·설명·단위·조건을 제공한다. Unity의 `Header` 속성은 이 계약을 대신하지 않는다.
창은 JSON 원문을 읽고 변경한 숫자 토큰만 저장한다. DTO 인스턴스 전체를 JSON으로 저장하거나 생략된 키를 기본값으로 채우지 않는다. 설명용 JSON/별도 스키마 JSON을 만들지 않는다.

| 변경 | 편집창의 자동 처리 | 개발자가 연결할 것 |
| --- | --- | --- |
| 기존 항목의 값 수정 | 검증·토큰 저장·다음 Play 로딩 | 창에서 입력·검증·저장 |
| 기존 JSON의 entries에 항목 추가 | 다시 읽으면 ID 기준 목록 생성 | 유효한 항목·SO/Registry·게임 내 사용처 |
| 기존 DTO에 수치 필드 추가 | 지원 타입+BalanceField+JSON 값이면 입력 생성 | DTO·JSON·실행 Data·사용 로직·검증 |
| 새 JSON 주제 추가 | 공통 등록에서 탭/DTO 입력 생성 | 주제 등록·파일 연결·카탈로그/게임 동작 |

현재 자동 입력은 entry 최상위의 public 인스턴스 필드 `int`·`long`·`double`이다. 문자열·enum·ID·참조·배열·중첩 객체는 이 창의 편집 대상이 아니다. 자원은 현재 편집 수치가 없어 조회 전용이다.
연출·애니메이션·보고 표시·Sprite·에셋 참조에는 편집 허용 목적으로 BalanceField를 붙이지 않는다. 배열 편집·마이그레이션·일괄 편집·실험안 관리·단위 변환은 미지원이다.

## 기존 JSON에 새로운 항목 추가하기

1. 같은 역할의 기존 entry와 [JSON 계약](../../UnityProject/Assets/JSON/Balance/README.md)을 참고해 대상 주제 파일의 `entries`에 객체를 추가한다. 기존 항목의 ID를 바꾸지 말고 전체 주제에서 고유한 `definitionId`를 부여한다.
2. 해당 DTO의 필수 필드·역할·참조 ID·components를 채운다. 주석·알 수 없는 키·null·중복 키는 넣지 않는다. 표시 이름 키가 필요한 종류는 [Localization 절차](localization.md)에 따라 번역을 등록한다.
3. 시설·생명체·자원은 동일 ID와 종류의 정의 SO를 만들고 해당 Registry에 등록한다. 수치 원본은 JSON에 두고 SO에는 ID·Unity 에셋 참조를 연결한다. 설정 주제는 해당 카탈로그의 항목 수/ID 계약을 따른다.
4. 건설 목록·배양 대상·초기 선택·생성 명령 등 실제 사용처가 새 정의를 소비하도록 연결한다. 편집창 목록에 보이는 것만으로 게임 메뉴에 자동 추가되는 것은 아니다.
5. Unity 임포트 후 창에서 `다시 읽기 / 버리기`를 실행하고 ID·역할별 필드를 확인한다. 기존 주제의 DTO와 형식이 같으면 창 코드나 주제 등록을 추가하지 않는다.
6. 아래 개발 검증을 수행한다. 항목 추가/삭제 자체는 편집창이 제공하지 않으므로 JSON·SO·Registry는 개발 작업에서 준비한다.

## 기존 DTO에 새로운 밸런스 필드 추가하기

1. [DefinitionDtos.cs](../../UnityProject/Assets/Scripts/Data/Loading/DefinitionDtos.cs)의 해당 DTO에 JSON 키와 같은 이름의 public 필드를 추가한다. 편집 지원 타입과 저장 범위를 선택한다.
2. 필드 바로 위에 `BalanceField`를 선언한다. 다음은 현재 FacilityDto의 실제 선언 형태이며 같은 필드를 중복 추가하는 예제가 아니다.

```csharp
[BalanceField("획득량", "획득 작업 한 회차의 산출량", Unit = "개", Order = 3,
    ConditionField = nameof(role), ConditionValues = new[] { "Gathering" })]
public int gatheringAmount;
```

| 메타데이터 | 작성 방법·현재 동작 |
| --- | --- |
| 이름·설명 | 생성자의 두 문자열. 비어 있으면 오류. 사용자가 조정 효과를 이해하도록 작성 |
| Unit | 표시 라벨만 지정. 초→분 같은 수치 변환은 수행하지 않음 |
| Order | 작은 값부터 정렬. 같은 값이면 필드명 순서 |
| ConditionField / ConditionValues | 같은 DTO의 string/enum 필드를 nameof로 참조하고 JSON 문자열과 정확히 일치하는 허용 값 지정. 값 목록 중 하나에 해당하면 표시 |
| Group | 속성은 존재하지만 현재 화면의 별도 그룹 구분에는 사용하지 않음 |

3. 실제 JSON 항목에 새 키와 값을 추가한다. **DTO 초기값이나 표시 조건은 필수 값 생략 허용 규칙이 아니다.** 현재 파서는 새 필드를 기본적으로 필수로 읽는다. 특정 역할에서 생략하려면 파서의 명시적 생략/기본값 계약과 검증도 함께 설계·수정한다.
4. [실행 Data](../../UnityProject/Assets/Scripts/Data/Models/FacilityData.cs)·[GameDataComposer](../../UnityProject/Assets/Scripts/Data/Catalog/GameDataCatalog.cs)·해당 게임 로직에 값을 전달하고 실제 사용한다. 유효 범위·역할·다른 필드와의 제약은 실행 데이터/기존 검증 경로에 추가한다. UI 속성에 별도 게임 규칙을 복제하지 않는다.
5. 컴파일 후 창을 다시 읽고 `개발자 진단`에서 필드 상태를 확인한다. 지원 형식이라면 별도 수동 필드 목록·Editor 입력 코드·스키마 JSON은 추가하지 않는다.
6. 새 값의 게임 효과·경계값·저장 호환 영향을 검증한다. 속성의 이름/설명만 바꾸는 것은 값/호환 해시를 바꾸지 않지만, DTO 필드와 JSON 값 추가는 구조/호환 해시에 영향을 줄 수 있다.

## 새로운 JSON 주제·파일 추가하기

1. `UnityProject/Assets/JSON/Balance/<주제 파일명>.json`에 기존 `{ "schemaVersion": 1, "entries": [...] }` 구조를 사용한다. 빈 entries는 현재 게임 파서가 허용하지 않는다.
2. DefinitionDto를 상속하는 DTO를 만들고 지원 수치에 BalanceField를 선언한다. Thing 종류라면 공통 ThingDefinitionDto 계약과 SO/Registry 연결도 검토한다.
3. [JsonDefinitionTopics.cs](../../UnityProject/Assets/Scripts/Data/Loading/JsonDefinitionTopics.cs)의 `All`에 고유한 주제 키·파일명·DTO 타입·탭 이름을 등록한다. 파서·Editor 파일 로더·밸런스 창이 이 등록을 공유한다.
4. [AppConfiguration.asset](../../UnityProject/Assets/SO/Application/AppConfiguration.asset)의 `jsonFiles`에 같은 주제 키와 해당 JSON TextAsset을 한 번 연결한다. 현재 도구는 이 설정과 편집 파일의 일치를 검사하며 설정을 자동 수정하지 않는다.
5. Composer의 해당 DTO→실행 데이터 결합, 카탈로그 노출·검증·게임 소비 경로를 구현한다. 주제 등록과 파싱 성공만으로 새 주제의 게임 동작이 완성되지는 않는다. 새 SO 종류의 Inspector 진입도 현재 지원 종류와 별도로 검토한다.
6. 아래 개발 검증을 수행한다. 기존 문서 구조/지원 타입이면 창의 탭·입력을 별도 등록하지 않는다. 새 구조·복합 타입이면 공통 로더/편집 지원의 확장이 필요한 별도 작업이다.

## 개발 검증과 진단

1. Unity 컴파일을 통과하고 `Tools → Mobile Game → Validate JSON and Registries`로 전체 JSON/정의 참조를 검사한다. 파일·씬 연결을 변경했다면 `Validate JSON Scene References`도 실행한다.
2. 밸런스 창의 `검증`으로 실제 AppConfiguration 연결·전체 카탈로그를 검사한다. 주제 누락/중복·다른 TextAsset 연결이 있으면 원래 등록과 설정을 맞춘다.
3. 새 항목/필드 표시, 역할 조건, 허용/거부 값, 저장 diff에서 해당 값만 바뀌는지 확인한다. 외부 수정이 있는 상태에서 초안을 덮어쓰지 않는다.
4. 새 Play·새 게임에서 실제 건설/생산 등 소비 경로를 확인하고 변경 영향에 맞는 파서·밸런스·저장 호환 테스트를 수행한다. 단순 문서 수정에는 게임 테스트를 추가하지 않는다.
5. 검증용 수치를 복원하고 승인된 실제 밸런스 변경만 남긴다. 테스트 통과를 위해 기존 호환 해시 기준을 임의로 덮어쓰지 않는다. 빌드·고부하는 [별도 실행 조건](verification.md)을 따른다.

| 개발자 진단 | 확인할 위치 |
| --- | --- |
| 속성 없음 | 편집이 필요한 게임 수치인지 판단 후 DTO의 BalanceField 선언 확인 |
| 역할·행동 조건 불일치 | JSON의 조건 필드 값과 ConditionField/ConditionValues 확인 |
| JSON 값 없음 | 해당 entry의 키 존재 여부 확인. 창은 새 키를 자동 삽입하지 않음 |
| 미지원 자료형 | 현재 지원 int/long/double인지 확인. 배열 등을 억지로 숫자로 바꾸지 않음 |
| 메타데이터 오류 | 이름/설명·조건 참조·enum 이름 확인 |

새 기능·콘텐츠 구현은 [작업 절차](workflow.md)의 승인된 PLAN 범위에서 진행한다. 이 사용 지침 자체는 새 게임 규칙이나 콘텐츠 추가 승인을 대신하지 않는다.

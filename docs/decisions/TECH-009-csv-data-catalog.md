# TECH-009 CSV·Registry·Data와 초기 로딩

- 날짜: 2026-10-28
- 상태: 채택, PLAN-012에서 구현·검증 완료
- 실행 기록: [PLAN-012](../plans/PLAN-012-csv-so-data-and-game-data-store.md)

## 결정과 대체 범위

사용자가 지정한 CSV + SO → Data → Runtime 구조를 적용한다. 기존 SO 클래스와 에셋 GUID는 보존하고 SO는 정의 ID·Unity 에셋 참조만 보유한다. 자원·시설·생명체 Registry SO가 같은 주제의 SO를 등록한다. 공통 정의를 직접 SO로 소비하던 TECH-001의 저장/참조 위치를 대체하며 Thing·컴포넌트·자원 소유와 GameFlow의 책임은 유지한다.

일반 C# Data가 CSV 값을 복사하고 SO의 선택적 Sprite/Prefab 참조를 바인딩한다. 공유 Data는 읽기 전용이다. 생산 진행·재고·예약·한 판 상태는 기존 Runtime에 남긴다. 불필요한 새 아트나 숫자 전용 침공 Registry는 만들지 않는다.

CSV 취득 인터페이스는 Unity 타입을 노출하지 않는다. 로컬 어댑터 내부의 TextAsset 접근은 허용한다. 파서/검증기/조립기는 HTTP·파일 경로에 의존하지 않는다. 로컬 원본은 Assets/CSV/Balance에 두고 초기화 클래스에 직렬화한 TextAsset 참조를 통해 Player에 포함한다. 실제 원격 연결과 원격 에셋 배포는 후속 범위다.

GameBootstrap이 Registry·로컬 CSV 참조와 세션 시작을 연결한다. GameDataStore는 DontDestroyOnLoad 싱글턴이며 완성된 카탈로그만 원자적으로 공개한다. 읽기 전용 카탈로그를 게임 로직에 전달하므로 Core/Gameplay가 Presentation의 싱글턴에 의존하지 않는다. 같은 실행 중에는 카탈로그를 교체하지 않는다.

저장에는 카탈로그의 게임 값/관계 지문을 기록한다. 표시 이름과 행 순서는 지문에서 제외한다. 지문이 다른 저장은 보존하고 명시적 새 게임 또는 올바른 데이터로 앱 재실행을 안내한다. 해시가 없는 v1~v4는 기준 배포본으로 생성되었다고 전제하며 출처를 실제 판별할 수 없음을 명시한다. 기준 카탈로그에서 기존 검증을 통과한 저장만 이관한다. v1 진단/플레이 슬롯 구분은 유지한다.

## 대안과 이유

- CSV를 SO 밸런스 필드에 덮어쓰는 방식은 두 원본과 가변 공유 정의를 남겨 사용하지 않는다.
- 실행 시 폴더 검색은 등록 원본과 빌드 의존성을 불명확하게 하므로 Registry의 명시적 목록을 사용한다.
- 모든 Manager의 싱글턴화, Addressables·원격 캐시·일반 DI 프레임워크 도입은 현재 요구에 필요하지 않다.

구현·테스트 결과와 제한은 PLAN에 기록하며 이 결정의 채택을 검증 완료로 간주하지 않는다.

## CSV 편집과 등록

1. `UnityProject/Assets/CSV/Balance`의 CSV를 UTF-8로 편집한다. 첫 행의 헤더를 유지한다. 쉼표/줄바꿈/큰따옴표를 포함하는 표시 문구는 CSV 인용 규칙을 따른다.
2. 기존 정의 ID는 저장 연결 키다. 표시 이름을 바꾸기 위해 ID를 바꾸지 않는다. ID는 대소문자를 구분하고 양끝 공백을 허용하지 않는다. 일반 숫자는 소수점 `.`으로 쓴다.
3. 새 자원·시설·생명체는 같은 ID의 SO를 만들고 해당 Registry에 한 번만 등록한다. 기존 밸런스 변경에는 SO를 수정하지 않는다. 선택적 Icon/Prefab은 비워 둘 수 있다.
4. Unity 메뉴 `Tools/Mobile Game/Validate CSV and Registries`로 검사한다. 오류의 테이블·행·열·ID를 기준으로 원본을 수정한다. 잘못된 값을 기본값으로 덮어쓰지 않는다.
5. Play를 종료하고 다시 시작한다. 같은 Play 실행의 씬 전환·한 판 재시작은 이미 준비한 카탈로그를 재사용한다. Player에 포함된 CSV 변경은 새 Player 패키징이 필요하다. 실제 Google Sheets/API 배포는 아직 제공하지 않는다.
6. 진행에 영향을 주는 값이 달라진 저장은 이어하기를 차단하고 보존한다. 이전 데이터로 앱을 재실행하거나 화면에서 새 게임을 명시적으로 선택한다. 오류 상태의 Reload는 이미 Ready인 데이터 버전을 교체하지 않는다.

| 테이블 | 열·의미 |
| --- | --- |
| resources | `definitionId`, `displayName`, `components` (`ResourceStack`) |
| creatures | 공통 3열 + `role` (`Prototype`, `Support`, `Invader`), `carryCapacity` (개), `moveSecondsPerCell` (초/칸) |
| facilities | 공통 3열 + `role`, `width/height` (칸), `interactionX/Y` (상대 칸), `resource` (건설/획득 자원 ID), `buildCost` (개), `buildSeconds` (작업초), `storageCapacity` (개), `gatheringAmount` (개), `gatheringSeconds` (작업초), `inputResource/outputResource/supportOutput/invaderOutput` (정의 ID), `inputAmount/outputAmount` (개), `productionSeconds` (작업초) |
| invasion | `definitionId`, `activitySeconds` (초), `smallScore/majorScore`, `majorEvery` (활동 회수), `targetScore/detailedUntilScore`, `reportLimit` (개) |
| session | `definitionId`, `mapWidth/mapHeight/startX/startY` (칸), `organicId/cultureId/gatheringId/storageId/prototypeId/processingId/cultivationId/supportId/invaderId` (초기화/선택 정의 ID), `staffedRate/unattendedRate` (배율), `endingSeconds` (연출초) |

시설 역할은 `Gathering`, `Storage`, `Processing`, `Cultivation`이다. 컴포넌트 목록은 `|`로 구분한다. 현재 시설·생명체는 `ResourceContainer`를 사용한다. 시설의 미사용 수치 열은 0, 미사용 연결 ID는 빈 값으로 둔다. 필요한 연결의 빈 값은 오류다. 표시 문구는 비워 둘 수 없다.

숫자 전용 침공/시작 테이블은 각각 한 행이며 Registry가 필요하지 않다. 초기 지급 자원은 시작 획득시설과 저장소의 건설비 합으로 계산한다. 그 합이 시작 프로토타입 운반량을 넘으면 로딩 오류다.

전체 원문 해시는 출처 확인용이며, 저장 호환성 지문은 테이블·행 ID·열 순으로 정규화한 게임 값이다. 표시 문구·행 순서·CSV 인용 방식은 제외하고 게임 값과 연결 ID를 포함한다. 현재 숫자 문자열의 다른 표기(예: `1`과 `1.0`)는 보수적으로 다른 지문이 될 수 있으므로 표기를 유지한다. `LegacyDataBaseline.Hash`는 이관 전 기준으로 고정하며 밸런스 편집 때 재생성하지 않는다.

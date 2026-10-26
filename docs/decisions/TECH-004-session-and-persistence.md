# TECH-004 세션 전환과 단일 슬롯 저장·복원

- 결정일: 2026-10-26
- 근거: PLAN-007 실행 승인, D-059의 클래스·저장 형식·주기·복원 절차·검증 구성 위임.
- 관련 작업: [PLAN-007](../plans/PLAN-007-gameflow-and-persistence.md).
- 기준: [GameFlow](../game-design/05-development/01-architecture/game-flow.md), [Thing 저장·복원](../game-design/05-development/01-architecture/thing-model.md), [플레이 환경](../game-design/04-delivery/01-platform/environment.md).
- 관계: [TECH-001](TECH-001-thing-foundation.md)의 식별·단일 소속, [TECH-003](TECH-003-map-and-placement.md)의 공간·편집 기반을 확장한다. 기존 기획과 상태 원본은 유지한다.
- 후속 확장: [TECH-005](TECH-005-initial-laboratory-and-jobs.md)가 실제 플레이의 Job·회차·예약과 v2 저장을 추가한다. 이 문서의 v1 진단 호환과 파일 실패 보존은 유지한다.

## 책임과 저장 범위

| 담당 | 적용 |
| --- | --- |
| 세션 상태 묶음 | [SessionState](../../UnityProject/Assets/Scripts/Core/Session/SessionState.cs)의 Registry·Map 참조. 재고·좌표를 복사해 소유하지 않음 |
| 단계·명령·진행 제어 | [SessionController](../../UnityProject/Assets/Scripts/GameFlow/Session/SessionController.cs)의 준비·플레이·멸망 연출·결과, 시작·이어하기·재시작·저장·배치 조작 |
| 저장 계약 | [SessionSnapshot](../../UnityProject/Assets/Scripts/Persistence/Snapshots/SessionSnapshot.cs)의 버전 1 DTO. 실행 객체와 분리 |
| 수집·검사·복원 | [SessionSnapshotCodec](../../UnityProject/Assets/Scripts/Persistence/Serialization/SessionSnapshotCodec.cs)의 정의 ID 조회·JSON 변환·새 객체 그래프 구성 |
| 파일 입출력 | [JsonFileSessionStore](../../UnityProject/Assets/Scripts/Persistence/Storage/JsonFileSessionStore.cs), 주입 가능한 [ISessionStore](../../UnityProject/Assets/Scripts/Persistence/Storage/ISessionStore.cs) |
| Unity 연결·화면 | [SessionFoundationView](../../UnityProject/Assets/Scripts/Presentation/Diagnostics/Session/SessionFoundationView.cs)의 Update·앱 생명주기·공통 명령 전달·관찰 표시 |

저장 항목은 진행 단계, 생성 순서 최댓값, Map 크기, Thing의 ID·정의 ID·생성 순서·자원 수량·HolderId와 확정 배치의 ID·위치·회전이다.
SO·Unity InstanceID·화면 객체·보관처별 재고 복제본을 저장하지 않는다. 현재 없는 생산·예약·Job·외부 활동 성과도 미리 구현하지 않는다.
시설 편집 후보·선택·편집 일시정지는 저장하지 않는다. 저장 중 편집을 확정하거나 취소하지 않으며, 복원 후에는 확정 배치와 편집 없음 상태로 시작한다.

## 복원과 세션 전환

1. JSON 버전·진행 단계·ID·생성 순서·배열·Map 크기를 확인한다. 준비 단계와 지원하지 않는 버전은 저장된 세션으로 받아들이지 않는다.
2. 정의 ID로 실제 SO를 조회하고 새 Registry에 일반 Thing 전체를 복원한 뒤 자원 Thing과 HolderId를 복원한다.
3. 삭제된 마지막 개체의 순서도 보존하도록 저장된 `lastCreationOrder`를 복원한다. 살아 있는 개체만으로 다시 계산하지 않는다.
4. 새 Map에 생명체를 먼저, 시설을 나중에 기존 배치 명령으로 복원한다. 시설 아래 생명체가 남은 유효 상태를 유지하면서 시설 중첩·영역 밖은 거부한다.
5. 전부 성공하면 현재 세션과 저장된 진행 단계로 전환한다. 실패한 후보를 현재 세션에 노출하지 않는다.

신규 시작은 주입한 초기 상태 생성 함수를 사용한다. 다시 시작은 새 상태 준비·검증 → 파일 교체 성공 → 현재 세션 교체 순서다.
현재 세션 교체 시 화면 선택·편집 연결을 초기화한다. 전환 완료 알림의 오류를 저장 실패나 롤백 성공으로 오인하지 않도록 파일 처리의 예외 범위와 구분한다.
손상 저장·누락 정의·잘못된 보관 관계·읽기 실패는 저장 없음과 구분한다. 자동으로 새 게임을 만들어 기존 파일을 덮어쓰지 않는다. 사용자의 명시적 다시 시작 명령은 초기 상태로 교체한다.

## 시간·저장 시점·실패

- `Advance(seconds)`는 유한한 0 이상 입력만 받는다. 벽시계나 저장 파일 시각으로 진행량을 계산하지 않는다.
- 플레이 중 연구소 갱신은 편집 중이 아닐 때, 외부 갱신은 편집 여부와 무관하게 전달한다. 준비·멸망 연출·결과·앱 일시정지에서는 두 갱신을 전달하지 않는다.
- 실제 생산·성과 계산 대신 담당 시스템이 연결할 갱신 이벤트를 제공한다. 화면의 누적 시간은 진단 관찰값이며 게임 진행 저장 항목이 아니다.
- 자동 저장의 개발 초기값은 실행 중 5초 간격이다. 새 게임·재시작, 단계 전환, 앱 일시정지·종료 시에도 저장한다. 테스트는 짧은 간격과 별도 경로를 주입한다.
- View의 Update는 공통 Advance를 호출한다. 앱 재개 직후 첫 프레임의 delta는 버려 종료·중단 동안의 경과 시간을 보상하지 않는다.
- 같은 디렉터리의 고유 임시 파일에 전체 JSON을 쓰고 flush한 뒤, 기존 슬롯은 File.Replace, 최초 슬롯은 File.Move로 반영한다. 기존 저장을 먼저 삭제하는 대체 경로를 두지 않는다.
- 자동 저장 실패는 현재 세션과 마지막 정상 파일을 유지하고 오류를 표시한다. 다음 주기에 재시도한다. 목표 도달은 저장 실패와 무관하게 진행을 멈춘다.
- 재시작 저장 실패는 기존 현재 세션·편집 상태를 유지한다. 새 상태로 바뀐 것처럼 보고하지 않는다.

## 대안과 선택 이유

- 범용 직렬화 프레임워크나 추가 패키지 대신 Unity JsonUtility와 현재 두 컴포넌트의 명시적 DTO를 사용한다.
- 현재 세션을 직접 수정하며 복원하는 방식보다 별도 후보를 완성한 뒤 교체하는 방식이 실패 시 상태 보존을 확인하기 쉽다.
- Core·Data·GameFlow·Persistence는 폴더 책임을 나누되 기존 Foundation 어셈블리와 GUID asmref를 유지한다. 폴더마다 새 어셈블리와 순환 의존을 만들지 않는다.
- 파일·초기 상태·시간 입력 경계를 주입해 실제 플레이 경로를 테스트한다. 하네스 전용 저장/진행 계산을 만들지 않는다.

## 영향과 검증 범위

Capture는 저장할 그래프가 복원 가능한지도 확인한다. 작은 프로토타입의 일관성을 우선하며 향후 규모에 따른 비용은 실제 측정 후 조정한다.
진단 씬은 기존 MapFoundation의 12×8·저장소 2개·보조형 2개·유기물 5개를 재사용한다. 실제 게임의 초기 건설·지급 자원 규칙을 대체하지 않는다.
진단 기본 슬롯은 Application.persistentDataPath 아래 `Diagnostics/PLAN-007/session.json`이다. 테스트는 임시 디렉터리의 별도 슬롯을 사용한다.
현재 기능·씬과 파일 동작은 PC Editor에서 검증한다. Android 파일 교체·앱 강제 종료·실기기 UI·성능은 미검증이며 후속 단계에서 확인한다.
대표 검증은 객체 독립성·생성 순서·단일 소속·합법 배치 복원, 손상/쓰기 실패 보존, 실제 Update 자동 저장, 단계 유지, 재시작과 편집 초기화다. 실행 결과와 증거는 PLAN-007에 기록한다.

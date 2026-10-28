# TECH-006 Manager 책임과 생산 확장

- 결정일: 2026-10-28
- 관련 작업: [PLAN-009](../plans/PLAN-009-processing-cultivation-and-workers.md)의 사용자 실행 승인, D-059.
- 관계: [TECH-004](TECH-004-session-and-persistence.md)·[TECH-005](TECH-005-initial-laboratory-and-jobs.md)의 담당 클래스 이름과 조율 경계를 아래와 같이 변경한다. 기존 상태 소유·저장 계약·기획 규칙은 유지한다.

## 이름과 책임 이관

| 이전 | 현재 | 책임 |
| --- | --- | --- |
| SessionController | GameFlowManager | 단계 전환·세션 교체·진행 허용·저장 시점 |
| LaboratorySimulation | LaboratoryManager | 세션 의존성 연결·공통 갱신 순서·담당 명령 위임 |
| WorkerSystem | WorkerManager | 작업 선택·전환·이동·개체 우선순위 설정 |
| LogisticsSystem | LogisticsManager | 운반·예약·수령·입고·저장 허용 설정 |
| LaboratorySimulation의 건설 후보 처리 | ConstructionManager | 건설 미리보기·배치 검사·확정·취소 |
| LogisticsStateValidation.cs | LogisticsManager.Validation.cs | 동일 물류 Manager의 저장 관계 검사 partial |
| SessionControllerTests | GameFlowManagerTests | 동일 GameFlow 동작 회귀 검사 |

기존 파일과 meta를 함께 이동하여 GUID를 유지했다. 일반 C#·namespace·어셈블리 경계를 유지한다. Definition·Component·Factory·Codec·Store·Commands·View·GridMap·Registry는 실제 역할 이름이므로 Manager로 일괄 변경하지 않는다.
연구소의 기존 공개 명령은 호출 호환을 위해 담당 Manager로 위임한다. 명령 처리 결과만 전달하며 규칙과 원본 상태를 복제하지 않는다. Manager는 세션별로 구성하며 전역 싱글턴이나 MonoBehaviour를 추가하지 않는다.

## 검증

새 기능을 추가하기 전에 기존 Edit·Play 회귀와 저장 왕복을 실행한다. 실제 결과·GUID 대조·이관 목록은 [PLAN-009 증거](../plans/evidence/PLAN-009/README.md)에 기록한다.

## 생산·작업·물류 경계

- `ProductionComponent`가 선택 대상·진행 중 대상·소비 후 진행 여부·작업량·미입고 수량을 소유한다. `WorkerComponent.AssignedFacilityId`는 현재 Job과 독립된 배정이다.
- `LaboratoryCommands → LaboratoryManager → WorkerManager.Assign`이 배정을 바꾼다. 정상 운반은 유지하고 다른 행동은 취소한다. 자동 복귀는 실제 수집 회차·정상 운반을 완료한 뒤 우선순위로 다시 선택한다.
- 공통 갱신은 0.05초 이하 단계에서 물류 관계 조정 → 개체 Job 수행 → 시설 생산 → 작업 유효성 조정 순서다. `WorkerManager`는 그 단계에 실제 생산 작업한 시설 ID만 넘긴다. `ProductionManager`가 100% 또는 20% 중 하나로 한 번 진행한다.
- 시설 회차는 완납·선택·접근 조건에서 한 번 소비한다. 실제 작업자 없이도 시작하며, 실제 Worker가 Map 경로로 도달할 수 없는 상호작용 위치는 저속 생산도 멈춘다. 연구소 시간 허용은 기존 `GameFlowManager`가 담당한다.
- 가공 결과는 물리 자원과 미입고 수량으로 남는다. 운반자가 집어도 회차를 풀지 않으며 `LogisticsManager.Deliver → ProductionManager.AdmitOutput`의 저장소 실제 입고만 차감한다. 기존 수집 완료품에도 같은 입고 경계를 쓴다.
- 배양은 회차 시작 시 선택을 복사해 현재 대상을 고정한다. 완료 시 상호작용 위치에 1개체를 만들고 다음 회차를 준비한다. 보급형은 Worker로 미배정 합류하며 침공형은 Worker 없이 Map에서 기다린다.
- 공급은 저장소에서 생산시설로만, 완료품은 생산시설에서 저장소로만 이동한다. `InputNeeded`에서 도착 재료를 빼고 목적지 예약을 다시 빼 한 회차분만 약속한다. 저장 용량은 두 자원 수량의 합이다.
- 저장 허용은 자원별 입고 조건이다. 제외 재고는 다른 허용 저장소로 재배치한다. 이미 선택한 정상 출고는 공급처 허용 변경으로 취소하지 않는다. 목적지 불가 시 기존 집기 전 취소·집기 후 저장소 재탐색/보유 대기를 따른다.

## 위임된 수치·선택 순서

D-059 범위에서 가공/배양도 건설 유기물 2·3초·2×2칸·고정 작업 위치 `(0,-1)`를 사용한다. 저장소의 기존 3×2칸·용량 20, 보조형 운반량 5·칸당 이동 0.25초를 유지한다. 생산 작업은 시설당 실제 1개체이며 복수 배정은 허용하되 Job 점유로 중복 가속을 막는다.

가공 유기물 5→배양액 1·10초, 배양 배양액 2→개체 1·20초는 승인 기획 그대로다. 무인 완료 시간은 각각 50초·100초이며 공급·이동·입고 시간은 별도다.

작업 순서는 건설·수집·운반·생산이다. 작은 양의 우선순위가 먼저이며 0은 끈다. 기본값은 1·2·3·4, 기존 3항목 설정/저장은 앞 3값을 유지하며 빠진 생산 우선순위는 4로 해석한다. 배정된 작업에 우선권을 강제하지 않는다.

동일 공급처에 복수 자원이 있으면 유효 목적지의 경로 거리 → 목적지 생성 순서 → 자원 생성 순서로 고른다. 공급처 자체의 거리·생성 순서가 항상 먼저다. 허용 저장 재고는 생산 수요로, 제외 재고는 다른 저장소로만 신규 운반 후보를 만든다.

유기물 수집으로 공용 저장소가 차면 배양액 입고가 대기할 수 있다. 저장 공간을 예약해 비우는 새 규칙을 추가하지 않는다. 검증의 실제 진입에서는 수집 자원을 지불해 두 번째 저장소를 건설하고 유기물/배양액 허용을 나누어 운용한다.

## 저장 확장과 호환

이름 정비 자체는 버전 2 계약을 유지한 채 기존 133 Edit·6 Play를 먼저 통과했다. 생산 확장부터 버전 3으로 저장하며 `runtimeKind = laboratory`, 기존 enum 값·정의 ID·Thing ID·생성 순서는 유지한다.

버전 3은 배양액 정의 ID, 생산 회차/선택, 개체 배정, 자원별 허용 상태를 추가한다. 버전 2는 기존 상태를 복원하고 현재 가공 정의의 배양액을 연결하며 저장소 배양액 허용을 기본 true로 이관한다. 진단용 버전 1은 이전처럼 진단 슬롯에서만 허용한다. 알 수 없는 버전·잘못된 배정/회차/자원/예약 그래프는 새 세션 공개 전에 거부한다.

복원 과정에서 수량·소비·생성·예약을 추측해 고치지 않는다. 저장 전 캡처도 같은 복원 검사를 거쳐 정상 파일을 보호한다. 실제 파일 중간 지점과 실패 보존 검사는 [검증 기록](../plans/evidence/PLAN-009/README.md)에 있다.

## 화면·콘텐츠·유지 예외

기존 `LaboratoryView.Controller`/`Simulation` 공개 조회 이름은 호출 호환을 위해 유지하고 반환 타입만 새 Manager로 바꿨다. `simulation` 지역 변수와 순수 상태/조회 API까지 일괄 변경하지 않는다. 과거 검증 결과의 타입 이름과 소스 해시는 당시 기록으로 남긴다.

새 SO는 `ProductionDefinitionAssets.CreateMissing`, 실제 씬 연결은 `LaboratorySceneAssets.UpgradeProduction`의 Unity Editor API로 생성·저장한다. 기존 SO·씬·스크립트 GUID를 유지한다. 신규 자원 1·시설 2·생명체 2가 더해져 자원 2·시설 종류 4·생명체 모델 3이 된다.

PC 가로 화면은 기존 Temp 격자·문자·단색 표현을 확장한다. 새 Sprite 없이 시설/모델 기호, 자원별 재고·허용, 회차/다음 대상, 속도·대기 원인과 배정 조작을 표시한다. 최종 시각 디자인이나 정식 Sprite 교체 범위를 이번 기능 구현에 추가하지 않는다.

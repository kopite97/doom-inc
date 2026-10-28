# TECH-007 외부 활동·목표 시각·저장 경계

- 날짜: 2026-10-28
- 상태: 채택, [PLAN-010](../plans/PLAN-010-invasion-activity-and-reports.md) 실행 승인 범위.
- 선행: [TECH-004](TECH-004-session-and-persistence.md), [TECH-006](TECH-006-production-and-manager-boundaries.md).
- 기준: [침공 규칙](../game-design/01-direction/02-experience/core-loop.md), [상태 소유](../game-design/05-development/01-architecture/thing-model.md), [GameFlow](../game-design/05-development/01-architecture/game-flow.md).

## 소유와 명령

- `Gameplay/Invasion/InvaderComponent`는 침공형 Thing의 대기/외부 여부·현재 활동 진행·완료 회차를 소유한다. Worker를 붙이지 않는다.
- 세션 Runtime의 `InvasionManager`는 같은 Registry/Map을 참조한다. 별도 외부 개체 목록을 원본으로 유지하지 않는다.
- `InvasionState`는 세계 성과·전체 활동 순번·명시적으로 진행한 시간·한정된 보고 이력을 소유한다. 보고를 합쳐 성과를 복구하지 않는다.
- `LaboratoryCommands.ReleaseInvaders`는 현재 세션/플레이/앱 정지를 검사한다. Manager가 전체 그래프를 먼저 검증한 뒤 모든 대기 침공형을 Unplace하고 같은 Thing을 외부 상태로 전환한다.
- 편집 중 새 방출은 거부한다. 이미 외부인 개체는 편집 중에도 계속 활동한다. 0개 방출은 효과 없는 false다.

## 위임된 초기값과 보고

- `Data/Definitions/Invasion`의 `InvasionSettings` 한 곳에 기본값을 둔다. 실제 씬은 `SO/Session/Invasion.asset`의 `InvasionDefinition`을 참조한다.
- 시작 시 설정을 복사하고 저장에 포함한다. 기존 한 판의 설정은 SO 조정으로 소급 변경하지 않는다. 구버전/기존 테스트의 미지정 설정에는 같은 기본값을 적용한다.
- 5초마다 활동 1회, 보통 성과 2, 4번째마다 큰 공적 10, 목표 120. 난수·레벨·해금 없음. 성과는 완료 활동별로 반영한다.
- 활동 직전 누적 성과가 60 미만이면 상세 보고, 이후에는 큰 공적만 보고한다. 최신 20개를 유지한다. 보고는 활동 순번·개체 ID·회차·성과·게임 경과 시간·당시 누적 성과를 기록한다.
- 문구는 보통 `Local defenses disrupted.`, 큰 공적 `A major stronghold has fallen.`이며 화면의 접힘/열림은 저장하지 않는다.

## 시간과 목표 전환

- Core의 `IExternalSessionRuntime`은 다음 활동까지 시간·외부 갱신·목표 도달 조회만 제공한다. Core에서 Gameplay를 참조하지 않는다.
- GameFlow는 입력 delta를 다음 외부 활동 완료 시각에서 분할한다. 그 구간만 연구소와 외부에 전달하고, 목표에 도달하면 남은 delta를 버린다.
- 같은 시각의 활동들은 최초 생성 순서로 한 묶음 확정한다. 묶음의 회차·진행·성과·보고가 모두 확정된 뒤 `ReachGoal`을 호출해 자동 저장한다. 같은 시각의 성과로 목표를 초과할 수 있으며 화면 게이지만 100%로 제한한다.
- UI 이벤트 구독으로 활동을 구동하지 않는다. 세션 교체 시 Runtime 조회도 교체되며, GameFlow의 기존 Generation 검사를 유지한다.
- 실제 연구소 세션의 수동 `ReachGoal`은 미달이면 거부한다. Runtime 없는 버전 1 진단 세션의 수동 목표 신호는 유지한다.

## 버전 4와 복원

- v4는 연구소 상태와 별도 침공 스냅샷을 함께 저장한다. 전체 후보를 복원·검증한 뒤 공개/파일 교체한다.
- Unity JsonUtility는 null 중첩 객체를 빈 객체로 복원하므로 침공 스냅샷의 명시적 schemaVersion으로 실제 확장을 구분한다. 새 활동 시계는 double 왕복 정밀도를 보존하는 invariant 문자열로 저장한다.
- 모든 침공형 ID가 정확히 한 번 있어야 한다. 대기=Map 안/회차·진행 0, 외부=Map 밖. 잘못된 모델·재고·진행·순번·보고 관계를 거부한다.
- 누적 성과와 전체 활동 수는 개체별 완료 회차에 따른 결과와 교차 검사한다. 보고 보관 여부는 성과 검증의 근거가 아니다.
- Playing은 목표 미달, Destruction/Results는 목표 도달이어야 한다. 저장된 목표 플래그도 게이지와 일치해야 한다.
- 정상 v2/v3의 Playing은 침공형 대기·성과/보고 0으로 이관한다. 종료 단계는 `legacyEndVersion=2/3`을 남기고 그대로 보존한다. 이 예외는 외부 개체·성과·회차·보고·시간이 전부 0일 때만 유효하며 v4 재저장에도 유지한다.
- 정상 구버전 종료 상태를 Playing으로 되돌리거나 목표치만큼 가짜 성과를 만들지 않는다. v1 진단 슬롯과 실제 플레이 슬롯 분리는 유지한다.

## 검증 연결

적용 위치: [InvasionManager](../../UnityProject/Assets/Scripts/Gameplay/Invasion/InvasionManager.cs), [InvasionState](../../UnityProject/Assets/Scripts/Gameplay/Invasion/InvasionState.cs), [GameFlowManager](../../UnityProject/Assets/Scripts/GameFlow/Session/GameFlowManager.cs), [InvasionSnapshotCodec](../../UnityProject/Assets/Scripts/Persistence/Serialization/InvasionSnapshotCodec.cs), [실제 설정 SO](../../UnityProject/Assets/SO/Session/Invasion.asset), [화면](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs).

대안인 UI 이벤트 구독 구동은 세션 재연결과 저장 순서에 의존해 채택하지 않았다. 전체 delta를 연구소에 먼저 적용한 뒤 목표를 처리하는 방식은 목표 이후 시간이 생산에 반영되어 사용하지 않는다. 저장 복원 시 종료 단계를 강제로 Playing으로 바꾸거나 목표 성과를 지급하는 대안도 기존 단계/실제 성과를 훼손하므로 제외했다.

추가 수동 저장 없는 목표 자동 저장 복원, 새 형식 단계/목표 불일치 거부, 구버전 종료 재저장, 큰/분할 delta, 실제 유료 생산 진입과 화면·공통 명령 비교는 PLAN-010의 완료 기준을 따른다. 최종 실행 결과는 PLAN의 증거에 기록한다.

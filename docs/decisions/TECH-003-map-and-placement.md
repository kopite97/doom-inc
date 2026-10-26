# TECH-003 공통 Map과 배치 명령

- 결정일: 2026-10-26
- 근거: PLAN-004 사용자 실행 승인, D-059의 구조·좌표·크기·화면 구현 위임.
- 관련 작업: [PLAN-004](../plans/PLAN-004-map-and-commands.md).
- 기준: [Thing 모델](../game-design/05-development/01-architecture/thing-model.md)의 D-076·D-077·D-079, [시설 배치](../game-design/02-systems/03-space/placement.md)의 D-027~D-034.
- 관계: [TECH-001](TECH-001-thing-foundation.md)의 정의·실행 상태 분리를 공간으로 확장한다.

## 상태와 책임

| 담당 | 적용 |
| --- | --- |
| 시설 SO | [FacilityDefinition](../../UnityProject/Assets/Game/Foundation/Definitions/FacilityDefinition.cs)의 회전 전 점유 크기와 고정 상호작용 상대 좌표 |
| 공간 원본 | 일반 C# [GridMap](../../UnityProject/Assets/Game/Foundation/Spatial/GridMap.cs)의 Thing ID별 불변 배치 레코드 |
| Thing 연결 | Map 참조와 파생 `Placement`·`IsPlaced` 조회. Thing에 좌표 복사본을 두지 않음 |
| 조회 | [MapPlacement](../../UnityProject/Assets/Game/Foundation/Spatial/MapPlacement.cs)의 읽기 전용 위치·회전·점유·상호작용 정보와 생성 순서로 정렬한 스냅샷 |
| 변경·편집 | Map당 하나의 [PlacementCommands](../../UnityProject/Assets/Game/Foundation/Spatial/PlacementCommands.cs). Place·Unplace·BeginEdit·Preview·Confirm·Cancel |
| 화면 | [MapFoundationView](../../UnityProject/Assets/Game/Presentation/MapFoundationView.cs)는 명령을 전달하고 실제 조회 결과를 표시 |

## 배치와 생명주기

- 좌표는 `Vector2Int` 값이며 원점은 건설 영역 왼쪽 아래다. Map 크기는 생성 후 바뀌지 않는다.
- 배치 원점은 회전 후 점유 사각형의 왼쪽 아래다. 0·90·180·270도는 시계 방향이며 상호작용 좌표도 같은 변환을 거친다.
- 정의의 상호작용 위치는 회전 전 점유 영역의 외곽 한 칸이다. 대표 저장소는 개발 초기값 3×2, 상대 좌표 (1,-1)로 설정했다.
- 영역 밖·시설 중첩을 거부한다. 통로 차단이나 상호작용 칸이 영역 밖/차단 상태인 이유만으로 시설 배치를 거부하지 않는다.
- `IsWalkable`은 영역과 시설 점유만 검사한다. 생명체끼리 중첩 가능하며 생명체를 시설 칸에 새로 배치/이동하는 명령은 거부한다.
- 시설 배치 검사에 생명체 점유 금지를 새 조건으로 추가하지 않는다. 시설 아래 남은 생명체를 자동 소멸·이동시키지 않으며 실제 이동·탈출 경로는 Job/경로 단계에서 연결한다.
- 보관 자원은 Map 배치 대상이 아니다. Map 해제는 Thing 존재나 자원 소속을 변경하지 않는다.
- registry에서 Thing을 논리적으로 제거하면 Map 연결도 해제한다. 자원을 가진 보관처 제거는 기존 규칙대로 먼저 거부한다.
- 서로 다른 Map 사이 이동은 기존 Map 해제 후 새 Map 배치로 수행한다. 하나의 Thing이 두 Map에 동시에 속하지 않는다.

## 공통 조작

- 모든 배치 변경은 같은 검증을 거친 뒤 반영한다. 실패하면 기존 배치와 소속을 유지한다.
- 편집은 시설 한 곳을 대상으로 한다. 미리보기는 후보만 보관하고 실제 Map 점유를 바꾸지 않는다.
- 확정 시 다시 검사하고 한 번에 적용한다. 거부하면 편집을 유지하며, 취소하면 후보만 버린다.
- 편집 중 다른 직접 배치 명령은 거부한다. 대상이 제거된 뒤 확정해도 복구하거나 새 Thing을 생성하지 않는다.
- `IsEditing`은 후속 연구소 진행 시스템이 정지 조건으로 참조할 상태다. 실제 생산·이동 시뮬레이션의 일시정지는 이번 구현에 없다.

## 대안·한계·검증

점유 전용 격자 배열과 별도 Thing 좌표를 동시에 갱신하는 구조 대신, 단일 배치 레코드로 점유를 파생한다. 현재 조회 비용은 O(n)이며 실제 규모에 따라 파생 인덱스를 추가할 수 있다.
실행 객체와 SO의 공개 상태는 조회 전용이며 Map·registry는 같은 시뮬레이션 스레드에서 사용한다. 실행 중 SO 변경은 지원하지 않는다.
검증용 장면의 12×8 영역·저장소 둘·보조형 둘·유기물 5개는 [테스트 준비 구성](../../UnityProject/Assets/Game/Presentation/MapFoundationScenario.cs)이다. 승인된 게임 시작 상태를 대체하지 않는다.
임시 PC 화면은 IMGUI로 시설 선택·후보 위치·회전·확정·취소를 제공한다. 모바일 최종 UI·세로 대응·건설 비용·생산·경로 탐색·저장은 후속 범위다.
저장 단계에서는 Thing의 기존 ID·최초 생성 순서와 Map의 위치·회전을 함께 복원하고 공간 제약을 검증해야 한다.
실제 SO·공통 명령을 사용한 Edit Mode 검사와 저장된 장면의 Play Mode 검사·화면 증거는 PLAN-004에 기록한다.

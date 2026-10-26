# TECH-001 최소 SO 정의와 Thing 소유 관계

- 결정일: 2026-10-26
- 결정 주체·위임 근거: 개발 에이전트, D-059의 클래스·필드·컴포넌트·식별자·자료구조 위임
- 관련 PLAN: [PLAN-003](../plans/PLAN-003-thing-foundation.md)
- 기준 기획: [Thing 모델](../game-design/05-development/01-architecture/thing-model.md)의 D-072·D-073·D-075·D-078·D-082~D-084, [로드맵](../game-design/05-development/02-implementation/roadmap.md)의 첫 기반 묶음
- 적용: `UnityProject/Assets/Game/Foundation/`와 Editor 전용 정의 작성기
- 대체 관계: 없음
- 후속 확장: 공간 좌표·배치·Map 연결은 [TECH-003](TECH-003-map-and-placement.md)에 추가했다. 아래 미구현 범위는 PLAN-003 완료 당시 기준이다.

## 문제와 제약

종류별 SO는 공유하면서 개체의 상태는 분리하고, 실물 자원에 두 보관처가 생기지 않아야 한다.
첫 기반 묶음에서는 생산·운반·Map·GameFlow·저장 파일을 구현하지 않는다.

## 결정

| 책임 | 적용 구조 |
| --- | --- |
| 종류별 공통 정의 | [ThingDefinition](../../UnityProject/Assets/Game/Foundation/Definitions/ThingDefinition.cs) 아래 자원·시설·생명체 SO. ID·표시 이름·컴포넌트 종류만 우선 포함 |
| 개별 상태·컴포넌트 소유 | 일반 C# [Thing](../../UnityProject/Assets/Game/Foundation/Things/Thing.cs). 정의 참조·개별 ID·최초 생성 순서·존재·소속·읽기 전용 컴포넌트 목록 |
| 생성·조회·관계 변경 | 인스턴스 단위 [ThingRegistry](../../UnityProject/Assets/Game/Foundation/Things/ThingRegistry.cs). 정적 게임 상태를 두지 않음 |
| 자원 수량 | [ResourceStackComponent](../../UnityProject/Assets/Game/Foundation/Things/ResourceStackComponent.cs)의 개별 수량 |
| 보관 기능 | [ResourceContainerComponent](../../UnityProject/Assets/Game/Foundation/Things/ResourceContainerComponent.cs). 재고는 자원 Thing의 HolderId를 기준으로 조회 |
| 에셋 작성 | Editor 전용 [FoundationDefinitionAssets](../../UnityProject/Assets/Game/Editor/FoundationDefinitionAssets.cs). 누락된 대표 정의만 Unity API로 생성 |

- 컴포넌트 설정은 현재 필요한 `ResourceStack`·`ResourceContainer` 두 종류의 enum 목록이다. 정의를 검사한 뒤 Thing마다 새 C# 컴포넌트를 생성한다.
- 자원 정의는 수량 컴포넌트만 가진다. 시설·생명체에는 보관 컴포넌트를 선택적으로 둘 수 있다. 컴포넌트 소유자는 생성 후 바뀌지 않는다.
- SO의 직렬화 필드는 비공개이고 공개 조회는 읽기 전용이다. 런타임 코드가 SO를 수정하지 않는다. Unity Inspector까지 불변으로 잠그는 구조는 아니다.
- 정의 ID는 문자열이며 동일 registry에서 같은 ID를 다른 SO에 연결할 수 없다. 개별 ID는 `Guid`, 최초 생성 순서는 registry 안에서 증가하는 별도 `long`이다.
- 자원 생성에는 양의 수량과 같은 registry의 유효한 보관처가 필요하다. 소속 없는 실물을 외부에 반환하지 않는다.
- 자원 소속의 원본은 자원 Thing의 `HolderId` 하나다. 보관 컴포넌트에 별도 mutable 재고 목록을 저장하지 않는다.
- 생성·이동·소비·제거는 registry의 공개 API를 거치며 검증 후 변경한다. 같은 보관처로의 재이동은 중복 재고를 만들지 않는다.
- 수량 전부를 소비하면 해당 실물의 존재·소속을 종료한다. 자원을 보유한 보관처의 논리적 제거는 거부하여 고아 자원을 방지한다.

위 제거 API는 기반 객체의 생명주기 경계이며 플레이어 시설 철거·전투 손실 기능을 확정한 것이 아니다.

## 대안과 선택 이유

- 보관처마다 별도 재고 리스트를 보유하는 방식보다 단일 소속과 파생 조회가 작은 프로토타입에서 관계 일관성을 확인하기 쉽다.
- 범용 컴포넌트 플러그인 등록·직렬화 프레임워크 대신 두 기능의 명시적 생성 분기를 사용한다. 다음 기능이 필요할 때 그 설정과 컴포넌트를 추가한다.
- SO를 별도 실행 정의로 전부 복사하지 않는다. 현재 기획이 허용한 직접 정의 참조를 유지하고 실행 상태만 분리한다.
- Unity InstanceID와 정적 전역 증가값을 개별 ID로 쓰지 않는다. 향후 저장·복원과 세션 초기화에 사용할 식별·순서 의미를 분리한다.

## 영향과 검증

- 현재 보관 조회는 전체 등록 개체를 순회하며 비용은 O(n)이다. 실제 성능 문제가 생기면 단일 소속 원본을 유지하는 파생 인덱스를 고려한다.
- registry는 시뮬레이션 스레드에서 사용한다. `Things`·`Contents`는 실시간 열거이므로 변경하며 순회할 때 호출자가 먼저 스냅샷을 취한다.
- 저장 파일 형식·복원 API는 다음 기반 묶음에서 구현한다. 그때 기존 ID·생성 순서·소속을 복원해야 하며 현재 생성 API로 새 ID를 다시 부여하면 안 된다.
- 대표 정의는 유기물·범용 저장소·시작용 보조형 3개다. 전체 콘텐츠의 정의 필드·에셋을 완성한 것은 아니다.
- 저장 용량·허용 자원·예약·운반량·건설 비용·레시피·공간 좌표는 아직 구현하지 않았다. 기반 컨테이너가 수용한다고 게임의 입고 규칙까지 허용된 것으로 해석하지 않는다.
- [Edit Mode 검사](../../UnityProject/Assets/Tests/FoundationEditMode/ThingFoundationTests.cs)와 [소속 검사](../../UnityProject/Assets/Tests/FoundationEditMode/ResourceOwnershipTests.cs)는 실제 공개 API로 상태 독립·단일 소속·실패 시 원본 보존을 확인한다. 실제 실행 결과는 PLAN-003에 기록한다.

# 구현 순서와 선행 기반 작업

- 문서 ID: GD-IMPLEMENTATION-ROADMAP
- 분류: 개발 구조 > 구현 계획 > 구현 순서
- 상태: 선행 기반·후속 4단계 순서 확정 / 정의·Thing 및 공간·공통 조작 기반 완료, 한 판·저장 기반·게임 사이클 구현 전
- 검색어: 개발 순서, 구현 순서, 로드맵, 기반 시스템, 선행 기반, 완료 기준, 데이터 구조, Thing, 컴포넌트, SO, Unity, GameFlow, 하네스, 저장, 복원, CLI, MCP, 에이전트, 개발 결정, 검증 기록
- 근거: S2 / D-059·D-071·D-072~D-090 — [출처](../../00-management/04-evidence/sources.md), [결정 기록](../../00-management/02-decisions/record.md)
- 갱신일: 2026-10-26
- 탐색: [기획 MAP](../../README.md), [Thing과 정의 데이터](../01-architecture/thing-model.md), [GameFlow와 시스템 책임](../01-architecture/game-flow.md), [플레이 환경](../../04-delivery/01-platform/environment.md)

## 현재 작업

환경 준비 결과와 도구 호환 주의점은 [PLAN-002](../../../plans/PLAN-002-unity-environment.md)에 있다. 정의·Thing 최소 기반은 [PLAN-003](../../../plans/PLAN-003-thing-foundation.md), 공간·공통 조작·최소 화면은 [PLAN-004](../../../plans/PLAN-004-map-and-commands.md)에서 구현·검증했다. 다음은 한 판·저장 기반이며 새 PLAN은 proposed로 작성하고 사용자 승인 후 활성화한다. 전체 게임 사이클·저장/복원·Android 빌드·실기기 검증은 아직 수행하지 않았다.

개별 작업 상태·다음 행동·검증 증거는 [PLAN MAP](../../../plans/README.md)에서 찾는다. 작업 대상은 유일한 active PLAN이며, 이 문서는 전체 단계·기획상 완료 기준과 요약 상태를 관리한다.

확정한 Unity 버전·Universal 2D(URP)·Android 대상과 연동 방향은 [플레이 환경](../../04-delivery/01-platform/environment.md)을 따른다. 구현할 작업의 필수 참조를 아래에서 읽고, 승인 규칙 안에서 필요한 세부 설계를 구체화한다.

## 구현 순서와 작업별 참조 (D-080·D-086)

**데이터 구조 설계와 기반 시스템 작업을 먼저 진행하고, 세 묶음의 완료 기준을 통과한 뒤 기존 1~4단계를 이어간다.** 0은 선행 단계를 뜻하며 1~4의 상대 순서를 유지한다.

| 순서 | 작업 범위 | 필수 참조 | 완료 확인 |
| --- | --- | --- | --- |
| 0 | 정의·Thing / 공간·공통 조작 / 한 판·저장 기반 | [Thing 구조](../01-architecture/thing-model.md), [GameFlow](../01-architecture/game-flow.md), [플레이 환경](../../04-delivery/01-platform/environment.md) | 아래 선행 기반 3묶음의 완료 기준 |
| 1 | 초기 건설·수집·저장: 프로토타입 1개체와 초기 유기물로 획득시설·저장소를 건설하고 수집·운반·입고 연결 | [시작 상태와 진입 흐름](../../04-delivery/02-release/scope.md), [건설·배치](../../02-systems/03-space/placement.md), [획득 생산](../../02-systems/01-production/facilities.md), [저장·운반](../../02-systems/02-logistics/storage-and-transport.md), [작업 선택](../../02-systems/05-creatures/support-models.md) | 해당 시작 흐름과 규칙을 실행하고 상태·저장·하네스로 확인 |
| 2 | 가공·배양·보급형 운용: 배양액 생산, 생명체 배양, 배정, 20%·100% 속도 전환 연결 | [생산 회차·배양](../../02-systems/01-production/facilities.md), [배정·작업 전환](../../02-systems/05-creatures/support-models.md), [공급·예약](../../02-systems/02-logistics/storage-and-transport.md) | 생산·배양·작업 전환 규칙과 추가 상태의 저장·복원 확인 |
| 3 | 침공부터 한 판 종료: 방출·지속 활동·전과 보고·멸망 게이지·연출·결과·다시 시작 연결 | [반복 흐름과 침공](../../01-direction/02-experience/core-loop.md), [보고의 변화](../../03-player-experience/01-progression/flow.md), [GameFlow](../01-architecture/game-flow.md), [저장](../../04-delivery/01-platform/environment.md) | 가로 화면에서 한 판의 기본 흐름을 끝까지 연결하고 외부 활동·다시 시작 확인 |
| 4 | 반복 플레이와 모바일 최종 검증: 세로 대응, 병목 개선, 전체 사이클 반복, 실제 Android 기기 확인 | [최종 완료 기준](../../04-delivery/02-release/scope.md), [병목](../../02-systems/04-optimization/bottlenecks.md), [화면·가로세로](../../03-player-experience/03-interface/controls.md), [실행·기기 검증](../../04-delivery/01-platform/environment.md) | D-070의 네 기준과 가로·세로 표시·터치·성능 확인 |

모든 단계에서 필요한 최소 화면과 상태 확인, 저장·복원 및 하네스 검증을 함께 진행한다. UI 조작은 [화면과 조작](../../03-player-experience/03-interface/controls.md), 공통 책임은 [Thing 구조](../01-architecture/thing-model.md)를 참조한다. 저장·하네스·상태 표시를 4단계에서 처음 구현하는 것으로 해석하지 않는다.

## 선행 기반의 범위와 완료 기준 (D-086)

| 묶음 | 최소 구현 범위 | 완료 확인 | 현재 상태 |
| --- | --- | --- | --- |
| 정의·Thing 기반 | SO 정의 확인, Thing 생성, 컴포넌트 연결·조회, ID와 소속 관리 | 같은 정의를 사용하는 두 개체가 독립된 상태를 갖고 필요한 기능을 조회할 수 있다. | [완료·검증](../../../plans/PLAN-003-thing-foundation.md) |
| 공간·공통 조작 기반 | Thing과 Map의 기본 배치 정보 연결, 공통 명령·규칙 검사·상태 조회 | 배치 변경이 Map에 일관되게 반영되고, 화면과 하네스가 같은 처리 경로를 사용한다. | [완료·검증](../../../plans/PLAN-004-map-and-commands.md) |
| 한 판·저장 기반 | 신규 시작·초기화·복원 흐름, 현재 구현된 상태와 관계의 저장·복원 | 자원 소속과 Map 밖 개체를 복원하고, 다시 시작하면 이전 개체·관계가 남지 않는다. | 미구현·미검증 |

기반 단계에서는 같은 SO를 공유하는 시설 두 개의 실행 상태 독립성, 복원 후 자원이 정확히 한 보관처에 속하는지 등을 확인한다. 현재 구현한 기반 상태와 관계가 검증 대상이며, 후속 생산·운반 기능 전체를 먼저 완성할 필요는 없다.

실제 건설·수집·운반·생산·배양·침공은 1~4단계에서 구현한다. 해당 기능의 진행도·예약·Job 등도 그 단계에서 저장·복원과 하네스 검증에 추가한다.

세 묶음의 검증 결과를 해당 PLAN에 남기고 증거 링크와 함께 위 요약 상태를 갱신한다. **모든 기반 완료 기준을 실제로 통과한 뒤 1단계로 넘어간다.** 최종 프로토타입 완료 판정 D-070과 구분한다.

## 가로 기본 사이클 후 세로 대응 (D-090)

프로토타입은 가로·세로 모두 지원한다. 1~3단계에서 가로 화면의 초기 건설부터 한 판 종료·다시 시작까지 기본 게임 사이클을 먼저 완성하고 세로 대응을 추가한다. 최종 모바일 검증을 마치기 전에 두 방향을 모두 포함한다.

두 방향은 같은 기능·화면 요소를 재배치하여 사용한다. 정식 표현 범위는 [화면과 조작](../../03-player-experience/03-interface/controls.md)을 따른다. 가로에서만 동작하는 상태는 최종 프로토타입 완료가 아니다.

## 설계·연동·검증 적용 원칙 (D-059·D-080·D-088)

- 컴포넌트·Thing·SO·GameFlow와 담당 시스템의 세부 설계를 첫 개발 작업으로 구체화한다. 정확한 Manager 목록·클래스명·씬·조회 API·필드·갱신 순서를 미리 확정한 것은 아니다.
- 기존 정의와 상태 분리, Job과 생산 회차 책임, 자원 소유와 예약, 공통 Map, 저장·복원, UI·하네스 공통 경로를 유지한다.
- 선행 기반을 정식 출시용 전체 프레임워크나 후속 콘텐츠까지 먼저 구현하는 범위로 확대하지 않는다.
- 공식 CLI·MCP를 연결하고 하네스는 선행 기반부터 사용한다. 같은 명령 처리·규칙 검사·상태 조회로 실제 게임 상태를 검증하며 기능 추가에 따라 범위를 확장한다.
- 구체적인 명령·API·테스트 도구·검증 구성·반복 횟수·성능 수치는 개발에서 정한다. 실제 실행 결과로만 연결 성공과 검증 통과를 기록한다.

## 개발 기록의 위치

| 내용 | 원본 위치 |
| --- | --- |
| 작업 단위·상태·진행·다음 행동 | [docs/plans](../../../plans/README.md)의 해당 PLAN |
| 실행한 방법·환경·결과·증거·남은 문제 | 해당 PLAN의 검증 결과 |
| D-059 안의 주요 기술 선택·이유·대안·적용 위치 | [docs/decisions](../../../decisions/README.md) |
| 작은 구현 선택·일회성 조정 | 해당 PLAN |
| 승인된 게임 규칙·콘텐츠·완료 기준의 변경 | 기존 [기획 결정 기록](../../00-management/02-decisions/record.md)과 주제 문서 |

이 문서에 개별 PLAN 상태·기술 결정·검증 로그를 다시 작성하지 않는다. 단계 요약은 증거 PLAN을 연결하여 갱신한다.
문서 정비나 PLAN 활성화는 게임 구현·검증 완료를 의미하지 않는다.

## 준비 작업 연결

- [PLAN-001 지침 체계 정비](../../../plans/PLAN-001-agent-guidance.md)
- [PLAN-002 Unity 개발 환경 준비](../../../plans/PLAN-002-unity-environment.md)
- [PLAN-003 SO 정의와 Thing·컴포넌트 기반](../../../plans/PLAN-003-thing-foundation.md)
- [PLAN-004 공간·공통 조작 기반](../../../plans/PLAN-004-map-and-commands.md)

나머지 기반 묶음과 이후 1~4단계의 개별 PLAN은 착수할 작업 단위가 구체화되면 작성한다.

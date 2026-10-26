# 남은 사항과 개발 위임

- 갱신일: 2026-10-26
- 탐색: [기획 MAP](../../README.md), [문서 관리 규칙](../01-workflow/document-rules.md)

**현재 프로토타입의 필수 사전 기획에 남은 핵심 결정은 없다.** 아래의 개발 위임·후속 개발·과거 자료 확인은 개발 착수를 막는 필수 질문 목록이 아니다. 기존 Q-ID를 유지하되 해결 과정과 과거 규칙은 반복하지 않는다.

## 개발 중 결정할 상세 (D-059)

| 관련 Q-ID | 개발에서 정할 범위 | 기준 문서 |
| --- | --- | --- |
| Q-004 | 보고 주기·진행별 표현 전환·활동 성과 수치·멸망 및 결과 연출 상세 | [목표와 반복 흐름](../../01-direction/02-experience/core-loop.md) |
| Q-005 | 생산 기능의 구현 상세와 승인한 초기 수치의 미세 조정 | [생산시설](../../02-systems/01-production/facilities.md) |
| Q-006·Q-006-02·Q-006-03 | 저장 용량·운반량·이동 속도·물류 구현 상세 | [저장과 운반](../../02-systems/02-logistics/storage-and-transport.md) |
| Q-007 | 건설 영역 칸 수·시설 크기·상호작용 좌표·건설 비용과 시간·작업 처리 상세 | [시설 배치](../../02-systems/03-space/placement.md) |
| Q-008 | 상태·대기 이유의 표현과 병목 개선 검증의 구체적인 구성 | [이해와 피드백](../../03-player-experience/02-learning/guidance.md), [병목 해결](../../02-systems/04-optimization/bottlenecks.md) |
| Q-009 | 패키지·렌더링·화면 배치와 방향 전환·저장 주기와 형식·Android 검증 기종 및 OS 등 세부 설정 | [플레이 환경](../../04-delivery/01-platform/environment.md), [화면과 조작](../../03-player-experience/03-interface/controls.md) |
| Q-010·Q-010-05 | 확정한 시작 흐름이 성립하는 초기 지급 수량·위치·검증 반복 횟수와 성능 수치 | [최초 제공 범위](../../04-delivery/02-release/scope.md) |
| Q-011 | 확정된 작업 선택 규칙의 구현·우선순위 초기값·동시 작업 인원 등 개체 운용 상세 | [보조형 모델](../../02-systems/05-creatures/support-models.md) |
| Q-012 | 클래스·컴포넌트 분할·API·씬·호출과 갱신 순서·저장 자료구조·도구와 하네스 구성 | [Thing과 정의 데이터](../../05-development/01-architecture/thing-model.md), [구현 계획](../../05-development/02-implementation/roadmap.md) |

새로운 핵심 제한이나 콘텐츠를 추가하는 변경은 이 위임에 포함하지 않는다. 개발 중 주요 선택은 [기술 결정](../../../decisions/README.md)에, 진행과 실제 검증 결과는 [PLAN](../../../plans/README.md)에 기록한다. 전체 단계는 [구현 계획](../../05-development/02-implementation/roadmap.md)을 따른다.

## 후속 개발과 확인 필요

| 관련 Q-ID | 남은 내용 | 현재 처리 |
| --- | --- | --- |
| Q-003 | 정식 게임명 | 승인된 이름이 없다. 과거 이름 제안은 확정명으로 사용하지 않는다. |
| Q-004·Q-010 | 여러 세계 진행·환생·영구 보너스·침공형 귀환·수명·전투 손실의 상세 | [프로토타입 범위](../../04-delivery/02-release/scope.md) 밖의 후속 개발 |
| Q-011 | 고급형·특화형 운용, 특화형의 특수 기능 | 후속 개발. 특수 기능은 사용자가 명시적으로 미정으로 지정했다. |
| Q-009·Q-010 | Apple(iOS/App Store)·ONE store 배포 시점과 절차, 정식 출시 범위 | 후속 개발. 이번 프로토타입의 완료 조건에 포함하지 않는다. |

수익화는 [수익 모델](../../04-delivery/03-business/model.md)에서 후속 참고로 관리한다.

## 과거 자료의 확인 한계

| Q-ID | 확인되지 않은 자료 | 현재 구현에 적용하는 원칙 |
| --- | --- | --- |
| Q-001 | 과거 ‘그 방안’ 승인 직전의 제안 본문 | 내용을 추측하지 않는다. 현재 명시적으로 승인한 구조와 규칙을 사용한다. |
| Q-002 | 과거 완료 기획 목록 요청에 대한 답변 본문 | 현재 MAP과 주제 문서를 사용한다. 과거 목록 복구를 착수 조건으로 두지 않는다. |

출처의 범위는 [출처와 확인 범위](../04-evidence/sources.md)에서 확인한다.

## 해결된 항목의 위치

해결된 규칙은 해당 주제 문서로 통합했다. 아래 ID는 기존 참조를 찾기 위한 색인이며 재승인을 요구하는 항목이 아니다.

| 기존 Q-ID | 현재 규칙의 위치 |
| --- | --- |
| Q-005-01~Q-005-06 | [생산시설](../../02-systems/01-production/facilities.md) |
| Q-006-01·Q-006-04~Q-006-07 | [저장과 운반](../../02-systems/02-logistics/storage-and-transport.md) |
| Q-007-01~Q-007-06 | [시설 배치](../../02-systems/03-space/placement.md) |
| Q-010-01~Q-010-05 | [최소 콘텐츠와 시작 상태](../../04-delivery/02-release/scope.md) |
| Q-011-01~Q-011-05 | [보조형 모델](../../02-systems/05-creatures/support-models.md) |
| Q-012 | [개발 구조](../../05-development/01-architecture/thing-model.md)·[GameFlow](../../05-development/01-architecture/game-flow.md)·[기반 구현 범위](../../05-development/02-implementation/roadmap.md) |

# 모바일 게임 기획 MAP

- 갱신일: 2026-10-26
- 기획 상태: **프로토타입 개발 전 1차 기획 완료**. 필수 1·2·3·4·6·7은 완료이며, 필수 5의 기본 규칙은 확정하고 초기값·조정 수치는 개발에 위임했다.
- 개발 상태: **환경 준비 완료 / 선행 기반 첫째·둘째 묶음 완료**. 환경 결과는 [PLAN-002](../plans/PLAN-002-unity-environment.md), 정의·Thing 최소 구현은 [PLAN-003](../plans/PLAN-003-thing-foundation.md), Map·공통 조작·최소 화면과 검증 결과는 [PLAN-004](../plans/PLAN-004-map-and-commands.md)에 있다. 한 판·저장 기반과 전체 게임 사이클은 구현 전이다.
- 목표: 에셋 종류를 제한하면서 기본 게임 사이클을 충분히 반복할 수 있는 프로토타입을 완성한다.

## 에이전트의 읽기 순서

작업은 [루트 에이전트 MAP](../../AGENTS.md)에서 시작하고 [PLAN MAP](../plans/README.md)의 유일한 active PLAN 범위만 진행한다. 아래는 해당 PLAN에서 기획을 조회하는 순서다.

1. [문서 관리 규칙](00-management/01-workflow/document-rules.md)에서 확정 범위와 개발 위임의 구분을 확인한다.
2. [구현 계획](05-development/02-implementation/roadmap.md)에서 현재 단계·참조 문서·완료 기준을 확인한다.
3. 아래 MAP에서 작업에 필요한 **주제 문서의 현재 규칙**을 읽는다. 수치와 예외의 기준은 해당 주제 문서다.
4. 승인 배경이나 변경 관계를 확인해야 할 때만 [결정 기록](00-management/02-decisions/record.md)과 [출처](00-management/04-evidence/sources.md)를 조회한다.

개발에 위임된 상세는 확정된 규칙 안에서 결정한다. 작업 진행·검증은 [PLAN](../plans/README.md), 주요 기술 선택은 [기술 결정](../decisions/README.md)에 기록한다. 새로운 핵심 규칙이나 콘텐츠 범위 변경은 사용자와 한 항목씩 논의한다. [남은 사항](00-management/03-questions/open-questions.md)은 개발 위임·후속 개발·확인 필요로 구분한다.

## 필수 기획 MAP

필수 1~7은 승인한 기획 묶음이다. 문서의 분류 번호는 기존 대분류 → 중분류 → 소분류를 뜻하며 폴더와 문서 ID를 유지한다. 여러 묶음에서 같은 문서를 참조할 수 있다.

### 필수 1. 최소 콘텐츠와 시작 상태 — 완료

| 분류 | 문서 ID | 기준 문서와 담당 범위 |
| --- | --- | --- |
| 1.1.1 기획 방향 > 게임 정체성 | GD-CONCEPT | [콘셉트](01-direction/01-identity/concept.md): 게임 정체성·분위기·표현 방향 |
| 1.2.1 기획 방향 > 핵심 경험 | GD-CORE-FUN | [핵심 재미](01-direction/02-experience/core-fun.md): 운영과 병목 개선의 재미 |
| 1.2.2 기획 방향 > 핵심 경험 | GD-CORE-LOOP | [목표와 반복 흐름](01-direction/02-experience/core-loop.md): 게임 사이클과 목표 |
| 3.1.1 플레이 경험 > 진행 | GD-PROGRESSION | [초반부터 후반까지의 흐름](03-player-experience/01-progression/flow.md): 학습과 보고 표현의 변화 |
| 4.2.1 제공 범위 > 출시 | GD-RELEASE | [최초 제공 범위](04-delivery/02-release/scope.md): 최소 콘텐츠·시작 상태·생산 진입 |

### 필수 2. 생산 흐름 — 완료

| 분류 | 문서 ID | 기준 문서와 담당 범위 |
| --- | --- | --- |
| 2.1.1 게임 시스템 > 생산 | GD-PRODUCTION | [생산시설](02-systems/01-production/facilities.md): 레시피·회차·생산 속도·배양 운용 |

### 필수 3. 생명체 운용 — 완료

| 분류 | 문서 ID | 기준 문서와 담당 범위 |
| --- | --- | --- |
| 2.5.1 게임 시스템 > 생명체 | GD-SUPPORT-MODELS | [보조형 모델](02-systems/05-creatures/support-models.md): 모델 기능·작업 우선순위·배정·행동 전환 |

건설·공간 규칙은 필수 5의 시설 배치, 배양 회차와 출현은 필수 2의 생산시설을 참조한다.

### 필수 4. 침공과 결과 — 완료

| 분류 | 문서 ID | 기준 문서와 담당 범위 |
| --- | --- | --- |
| 1.2.2 기획 방향 > 핵심 경험 | GD-CORE-LOOP | [목표와 반복 흐름](01-direction/02-experience/core-loop.md): 방출·외부 활동·성과·멸망·결과·다시 시작 |

### 필수 5. 작동에 필요한 크기·수치 — 기본 규칙 확정 / 수치 개발 위임

| 분류 | 문서 ID | 기준 문서와 담당 범위 |
| --- | --- | --- |
| 2.2.1 게임 시스템 > 물류 | GD-LOGISTICS | [저장과 운반](02-systems/02-logistics/storage-and-transport.md): 저장·후보 선택·예약·운반 예외 |
| 2.3.1 게임 시스템 > 공간 | GD-PLACEMENT | [시설 배치](02-systems/03-space/placement.md): 건설·배치·상호작용 위치·회전·이동·일시정지 |

크기·용량·운반량 등의 초기값을 모두 사전 승인받아야 개발을 시작할 수 있는 것은 아니다(D-059).

### 필수 6. 최소 화면·조작·실행 환경 — 완료

| 분류 | 문서 ID | 기준 문서와 담당 범위 |
| --- | --- | --- |
| 3.2.1 플레이 경험 > 학습 지원 | GD-LEARNING | [이해와 피드백](03-player-experience/02-learning/guidance.md): 작업 상태·대기 이유와 학습 지원 |
| 3.3.1 플레이 경험 > 인터페이스 | GD-CONTROLS | [화면과 조작](03-player-experience/03-interface/controls.md): 화면 구성·조작·가로와 세로 대응 |
| 4.1.1 제공 범위 > 플랫폼 | GD-PLATFORM | [플레이 환경](04-delivery/01-platform/environment.md): Unity·플랫폼·도구·실행·자동 저장 |

### 필수 7. 프로토타입 완료 기준 — 완료

| 분류 | 문서 ID | 기준 문서와 담당 범위 |
| --- | --- | --- |
| 2.4.1 게임 시스템 > 효율 개선 | GD-BOTTLENECK | [병목 해결](02-systems/04-optimization/bottlenecks.md): 병목 파악과 개선 경험 |
| 4.2.1 제공 범위 > 출시 | GD-RELEASE | [최초 제공 범위](04-delivery/02-release/scope.md): 실제 플레이로 확인할 최종 완료 기준 |

핵심 규칙·범위·검증 방법이 승인되고 나머지 상세가 개발 위임으로 구분되면 1차 기획 완료다(D-035). 프로토타입 완료는 실제 구현과 검증 결과로 판정한다(D-070).

## 개발 구조와 구현 계획 MAP

| 분류 | 문서 ID | 기준 문서와 담당 범위 |
| --- | --- | --- |
| 5.1.1 개발 구조 > 아키텍처 | GD-THING-MODEL | [Thing과 정의 데이터](05-development/01-architecture/thing-model.md): SO·Thing·컴포넌트·Job·Map·소유·저장·UI와 하네스 |
| 5.1.2 개발 구조 > 아키텍처 | GD-GAME-FLOW | [GameFlow와 시스템 책임](05-development/01-architecture/game-flow.md): 한 판의 전환과 시스템 책임 |
| 5.2.1 개발 구조 > 구현 계획 | GD-IMPLEMENTATION-ROADMAP | [구현 순서와 선행 기반 작업](05-development/02-implementation/roadmap.md): 단계·필수 참조·완료 기준·요약 상태와 PLAN 연결 |

## 관리와 후속 참고 MAP

| 분류 | 문서 | 용도 |
| --- | --- | --- |
| 0.1 기획 관리 > 작성 절차 | [문서 관리 규칙](00-management/01-workflow/document-rules.md) | 문서의 역할·상태·수정 및 개발 원칙 |
| 0.2 기획 관리 > 의사결정 | [결정 기록](00-management/02-decisions/record.md) | D-ID별 현재 효력·최소 변경 기록·기준 문서 |
| 0.3 기획 관리 > 질문 관리 | [남은 사항](00-management/03-questions/open-questions.md) | 개발 위임·후속 개발·확인 필요 |
| 0.4 기획 관리 > 근거 자료 | [출처와 확인 범위](00-management/04-evidence/sources.md) | 승인 근거와 과거 자료의 확인 한계 |
| 4.3.1 제공 범위 > 운영 | [수익 모델](04-delivery/03-business/model.md) (GD-BUSINESS) | 후속 참고. 프로토타입 필수 범위 아님 |

## 검색

먼저 주제 문서의 제목·문서 ID·검색어를 찾는다. 과거 결정은 필요한 D-ID만 조회한다. 문서 번호나 ID를 최신 규칙의 우선순위로 해석하지 않는다.

~~~powershell
rg -n 'GD-LOGISTICS|예약' docs/game-design/02-systems
rg -n 'D-058' docs/game-design/00-management/02-decisions/record.md
~~~

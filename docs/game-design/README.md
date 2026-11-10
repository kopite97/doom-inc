# 모바일 게임 기획 MAP

- 갱신일: 2026-11-10
- 기획 상태: **프로토타입 개발 전 1차 기획 완료**. 필수 1·2·3·4·6·7은 완료이며, 필수 5의 기본 규칙은 확정하고 초기값·조정 수치는 개발에 위임했다.
- 개발 상태: **환경·선행 기반 세 묶음·1~4단계 및 D-070 완료**. 단계별 구현과 검증 근거는 [구현 로드맵](05-development/02-implementation/roadmap.md)을 따른다.
- 3단계 완료: [PLAN-010 침공형 방출·외부 활동·성과 누적·전과 보고](../plans/PLAN-010-invasion-activity-and-reports.md)와 [PLAN-011 멸망 연출·결과 화면·한 판 종료와 재시작 완결](../plans/PLAN-011-destruction-results-and-restart.md)은 completed다. PC 가로에서 유료 생산부터 목표·연출·결과·재시작·두 번째 판 종료까지 검증했다.
- 데이터 기반 완료: PLAN-012는 Edit 236/236·Domain Reload 양쪽 Play 각 13/13·오류/경고 0으로 데이터 이관·저장 호환성과 기존 두 판 흐름을 확인했다. Android 스크립트 컴파일·CSV 참조 검사까지 완료했다.
- 현재 개발 상태: PLAN-001~026은 completed이며 active는 0개다. 아트 5-1의 실제 게임 화면 승인까지 완료했다. 최신 Android 통합 검증과 4단계·D-070 최종 판정은 [PLAN-025](../plans/PLAN-025-android-integration-and-prototype-completion.md)에서 완료했다. SM-G977N/Android 12·현재 임시 표현 기준이며 경미한 표시 결함과 측정 한계는 해당 증거에 남겼다. 정식 아트 교체는 후속 5단계에서 진행하며 실제 Google Sheets 연결도 별도 후속 범위다.
- 데이터 전환 완료: [PLAN-015 주제별 JSON 정의 데이터와 초기 로딩 전환](../plans/PLAN-015-topic-json-definition-data.md)은 completed다. 전용 디렉터리의 주제별 JSON·DTO로 전환하고 SO 결합·영속 GameDataStore·저장 호환성을 유지한다. Editor 런타임 테스트는 포함하며 빌드 테스트는 사용자 지시로 제외한다. 두 검토 보완을 반영하고 구현·Edit 294/294·Domain Reload 양쪽 Play 각각 36/36을 확인했다. 빌드 테스트는 실행하지 않았다.
- 최근 작업: [PLAN-016 Bootstrap·Title·Laboratory 씬 흐름과 Editor 직접 진입](../plans/PLAN-016-bootstrap-title-and-scene-flow.md)은 completed다. 공용 LoadingScreen·싱글톤 씬 전환·직접 실행·저장/설정 실패 복구를 구현하고 Editor 검증을 마쳤다. 빌드 테스트는 사용자 지시에 따라 실행하지 않았다.
- 최근 구조 정비: [PLAN-017 생산 판정·파일 저장·상태 계약·공용 UI 추상화 정비](../plans/PLAN-017-production-storage-status-and-shared-ui.md)는 completed다. 두 검토 보완과 네 영역의 구현·Editor 회귀·실제 화면/저장/반복 종료 검증을 마쳤다. 빌드 테스트는 실행하지 않았다.
- 목표: 에셋 종류를 제한하면서 기본 게임 사이클을 충분히 반복할 수 있는 프로토타입을 완성한다.
- 텍스트 정비: [PLAN-018 Unity Localization·CSV 텍스트 관리와 한국어·영어 지원](../plans/PLAN-018-localization-and-csv-text-management.md)은 completed다. TextManager·공통 Localization·앱 언어 설정·CSV 9개 주제/247개 키·추가 언어 확장과 번역 재검토 추적을 구현했다. Edit 330/330·Play 57/57·직접 씬/반복 실행·두 언어 화면을 확인했다. Player·Addressables 콘텐츠 빌드와 실기기는 제외했으며 후속 공용 Popup 정비 결과는 아래 PLAN-019를 따른다.

- 최근 완료: [PLAN-019 공용 Popup 시스템과 기존 확인·설정 화면 통합](../plans/PLAN-019-shared-popup-system.md)은 completed다. Confirm·Notice·Settings와 공용/씬 Registry·소유 범위·입력 차단·앱 복귀 인계·멸망 경계 정리를 구현했다. Edit 343/343·Play 65/65·실제 씬 직접 진입/반복 실행·ko/en·가로/세로 화면을 검증했고 빌드/실기기는 제외했다. 후속 Editor 검증은 PLAN-020에서 완료했다.

- 반복·성능 검증: [PLAN-020 Editor 런타임 운영 병목·반복 플레이·성능 검증](../plans/PLAN-020-editor-bottlenecks-repeated-play-and-performance.md)은 completed다. 운영 병목·반복 플레이와 성능을 Editor에서 검증했으며, 당시 발견한 고부하 작업 탐색 비용과 CSV 가져오기 회귀는 PLAN-021에서 개선·검증했다. 최신 Android 성능과 4단계·D-070 최종 판정은 PLAN-025에서 완료했다.

- [PLAN-021 작업자 경로 탐색 최적화와 번역 CSV 반복 가져오기 안정화](../plans/PLAN-021-worker-pathfinding-and-localization-import.md)는 completed다. 유휴 배회·일반/실패 탐색 간격·거리 재사용과 CSV 반복 가져오기 및 실패 복원을 Editor에서 검증했다.
- [PLAN-022 약탈형 배양·침공과 연구소 자원 수신](../plans/PLAN-022-raider-and-resource-receiving.md)은 completed다.
- [PLAN-023 로딩 진행률·Feel 연출과 전체 게임 UI 물마루 TMP 전환](../plans/PLAN-023-loading-progress-feel-and-mulmaru-tmp.md)은 completed다. 실제 씬 진행률과 기본 1초 로딩, Feel 웨이브, 물마루 Dynamic TMP를 Editor에서 검증했다.
- [PLAN-024 침공형 방출 성공 피드백과 화면 밖 시설 선택 시 지도 이동](../plans/PLAN-024-release-feedback-and-map-reveal.md)은 completed다.
- 최근 완료: [PLAN-025 최신 버전 Android 통합 검증과 프로토타입 완료 판정](../plans/PLAN-025-android-integration-and-prototype-completion.md). Editor 회귀·승인된 Android 빌드/기기 검증·환경 복원을 완료했다. 다음 PLAN은 활성화하지 않았다.
- 후속 5단계: [정식 아트 제작·적용 로드맵](05-development/02-implementation/art-roadmap.md)의 5-1은 [PLAN-026](../plans/PLAN-026-art-style-scale-and-sample-integration.md) `art` / `completed`다. 바닥 r003·획득시설 r003·프로토타입 r002·유기물/Build r001, 106 Sprite의 적용·시설/4방향 보조형 동작·머리 위 운반·Editor 검증·사용자 게임 화면 승인을 완료했다. 5-2 이후와 새 아트의 Android 검증은 별도이며 PLAN-025의 표시/GC 개선 후보는 보류한다.
- 표현 기반 후보: [PLAN-027 연구소 Tilemap·SpriteRenderer 전환](../plans/PLAN-027-laboratory-tilemap-and-sprite-rendering.md)은 `development` / `proposed`다. 바닥·시설/개체를 월드 표현으로 전환하고 UI는 uGUI로 유지하는 계획이며 실행하지 않았다. 선행 PLAN-026은 완료했으며 별도 활성화 승인을 기다린다. 현재 active는 0개다.

## 에이전트의 읽기 순서

작업은 [루트 에이전트 MAP](../../AGENTS.md)에서 시작하고 PLAN 작업은 [PLAN MAP](../plans/README.md)의 유일한 active PLAN 범위만 진행한다. [독립 Sprite 작업](../agents/art-assets.md#sprite-requests)은 별도 절차를 따르며, 아래는 PLAN에서 기획을 조회하는 순서다.

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
| 5.2.2 개발 구조 > 구현 계획 | GD-ART-ROADMAP | [정식 아트 제작·적용 로드맵](05-development/02-implementation/art-roadmap.md): 프로토타입 이후 5단계의 제작 묶음·적용 순서·검수 기준 |

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

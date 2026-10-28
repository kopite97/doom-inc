# 기술 결정 MAP

개발 중 선택한 구조·도구·구현 방법과 그 이유를 기록한다.
작업 상태와 검증 결과의 원본은 [PLAN](../plans/README.md)이다.

## 기록할 때

- 여러 기능에 영향을 주거나 나중에 변경 비용이 큰 기술 선택을 기록한다.
- 예: 상태 저장 형식, 컴포넌트 구성 방식, 시간 진행 방식, 패키지·도구 선택.
- 사소한 변수명·메서드 분할·일회성 수정은 PLAN에 필요한 만큼만 남긴다.
- 승인 기획 안의 개발 위임은 에이전트가 결정하고 근거를 기록할 수 있다.
- 새 게임 규칙·콘텐츠 범위 변경은 사용자 승인 후 [기획 결정 기록](../game-design/00-management/02-decisions/record.md)에 반영한다.
- 기술 결정으로 기획의 미정 항목을 확정하거나 승인된 기획을 덮어쓰지 않는다.

## 작성 방법

1. [_template.md](_template.md)를 복사해 `TECH-NNN-주제.md`로 작성한다.
2. 관련 active PLAN, 기준 기획, 결정 내용·이유·비교한 대안·영향·검증 방법을 적는다.
3. 실제 적용 코드·설정과 검증 결과 PLAN을 연결한다. 제안과 적용 사실을 구분한다.
4. 결정을 바꾸면 새 결정에서 이전 문서를 연결하고 이전 문서에도 대체 관계를 남긴다.
5. PLAN의 다섯 상태를 기술 결정의 진행 상태로 복제하지 않는다.

기술 결정은 `TECH-` ID를 사용하고, 게임 기획의 기존 `D-` ID와 PLAN의 `PLAN-` ID를 재사용하지 않는다.
게임 규칙·수치와 전체 기획 설명은 기준 문서에 링크하여 중복을 줄인다.

## 결정 목록

| 결정 | 적용 내용 | 관련 작업 |
| --- | --- | --- |
| [TECH-001](TECH-001-thing-foundation.md) | 최소 SO 분류·Thing/컴포넌트·식별·단일 자원 소속 | [PLAN-003](../plans/PLAN-003-thing-foundation.md) |
| [TECH-002](TECH-002-repository-layout.md) | 루트 단일 저장소·생성 파일 제외·Unity 바이너리 LFS 범위 | [PLAN-005](../plans/PLAN-005-repository-setup.md) |
| [TECH-003](TECH-003-map-and-placement.md) | Map 단일 공간 상태·회전·공통 배치 명령·검증용 화면 | [PLAN-004](../plans/PLAN-004-map-and-commands.md) |
| [TECH-004](TECH-004-session-and-persistence.md) | 한 판 단계·공통 갱신·JSON 단일 슬롯·검증 후 복원·실패 보존 | [PLAN-007](../plans/PLAN-007-gameflow-and-persistence.md) |
| [TECH-005](TECH-005-initial-laboratory-and-jobs.md) | 실제 시작·건설/수집 Job·운반 예약·전량 입고 회차·플레이 저장 v2 | [PLAN-008](../plans/PLAN-008-initial-construction-and-logistics.md) |
| [TECH-006](TECH-006-production-and-manager-boundaries.md) | Manager 책임·명명 정비와 가공·배양 확장 | [PLAN-009](../plans/PLAN-009-processing-cultivation-and-workers.md) |
| [TECH-007](TECH-007-invasion-time-and-save-boundaries.md) | 외부 활동·보고 분리·목표 시각 분할과 저장 v4/종료 이관 | [PLAN-010](../plans/PLAN-010-invasion-activity-and-reports.md) |
| [TECH-008](TECH-008-run-ending-presentation.md) | 일시적 멸망 연출 시계·화면 요청 식별·결과 조회·종료 복원과 재시작 | [PLAN-011](../plans/PLAN-011-destruction-results-and-restart.md) |

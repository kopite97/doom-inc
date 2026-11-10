# 병목 해결

- 문서 ID: GD-BOTTLENECK
- 분류: 게임 시스템 > 효율 개선 > 병목 해결
- 상태: 병목 개선 기준 확정 / Editor·최신 Android 운영 병목 검증 및 D-070 판정 완료
- 검색어: 병목, 효율, 문제, 개선, 완료 기준, 검증, 작업 상태, 대기 이유
- 근거: S1 / D-001, S2 / D-059·D-067·D-070 — [출처](../../00-management/04-evidence/sources.md), [결정 기록](../../00-management/02-decisions/record.md)
- 상세 결정 관리: [개발 중 결정과 후속 범위](../../00-management/03-questions/open-questions.md)
- 갱신일: 2026-11-07
- 탐색: [기획 MAP](../../README.md)

<a id="current-rules"></a>

## 병목 파악과 개선

병목이 발생하면 어떻게 해결할지 고민하는 것이 주요 재미다(D-001). 시설·생명체 선택 패널에서 작업 상태와 대기 이유를 보여준다(D-067). 구체적인 피드백은 [이해와 피드백](../../03-player-experience/02-learning/guidance.md)을 따른다.

프로토타입 검증에서는 실제 플레이로 대표적인 병목을 파악하고, 배치·배정·저장 설정 등을 바꿔 개선할 수 있는지 확인한다(D-070). 기존 생산·물류·배치·보조형 운용 규칙을 바탕으로 검증한다. 정식 완료 기준은 [프로토타입 범위와 검증](../../04-delivery/02-release/scope.md)에서 관리한다.

Editor의 대표 운영 병목 검증은 PLAN-020에서 완료했다. 이후 경로 탐색 성능 개선은 PLAN-021에서 검증했다. 최신 Android의 실제 저장 공간 병목 개선과 동일 게임 시간 단위의 전후 비교, D-070 종합 판정은 [PLAN-025](../../../plans/PLAN-025-android-integration-and-prototype-completion.md)에서 완료했다. 단계별 상태와 증거는 [구현 로드맵](../../05-development/02-implementation/roadmap.md)을 따른다.

<a id="development-details"></a>

## 개발 중 결정할 상세

반복 횟수·조정 수치·표현·세부 구현은 개발하면서 결정한다(D-059·D-070). 기준의 승인은 실제 플레이 검증을 통과했다는 뜻이 아니다.

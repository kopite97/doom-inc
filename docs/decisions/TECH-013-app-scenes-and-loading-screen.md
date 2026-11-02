# TECH-013 앱 씬 전환과 공용 로딩 화면

- 상태: 채택·PLAN-016 구현 완료
- 날짜: 2026-11-02
- 작업: [PLAN-016](../plans/PLAN-016-bootstrap-title-and-scene-flow.md)
- 기준: [씬 흐름 지침](../agents/scene-flow.md), [GameFlow](../game-design/05-development/01-architecture/game-flow.md)

## 선택과 책임

- Bootstrap → Title → Laboratory를 제품 흐름으로 사용한다. 초기 데이터 준비와 한 판 시작을 분리한다.
- SceneFlowManager는 Unity 씬과 입력/로딩을 조율하는 영속 싱글톤이다. 일반 C# GameFlowManager는 한 판의 생성·복원·종료 규칙을 계속 소유한다.
- AppConfiguration 에셋은 JSON·Registry·UI·로딩 프리팹·제품 씬 경로의 공통 바인딩이다. 씬 경로는 Unity 로딩 대상의 식별 참조이며 밸런스 데이터가 아니다. Bootstrap과 Editor 직접 실행은 같은 에셋을 사용한다.
- LoadingScreen은 앱 수명 아래에 있는 재사용 프리팹이다. 별도 Loading 씬을 거치는 대안 대신 기존 씬 해제 후에도 오류·재시도를 유지하는 화면을 선택했다.
- SceneFlowManager의 코루틴은 초기화 Task와 Unity 씬 로드를 기다린다. 기존 데이터 취득의 Task/취소 계약은 유지하며 코루틴 중단을 실제 작업 취소로 간주하지 않는다.
- 공통 AppUiLifetime이 EventSystem을 소유한다. Canvas 참조 수로 입력 시스템을 제거하던 방식은 씬 교체와 공용 로딩 화면에 맞춰 대체한다.
- AppUiSettings는 현재 슬롯의 UiSettingsStore를 공유한다. 미저장 선택·오류를 씬 간 유지하며 카탈로그/플레이 스냅샷에 포함하지 않는다. 새 슬롯/Play 수명은 분리한다.

## 기존 결정과의 관계

- TECH-009·TECH-012의 카탈로그 초기화·취득부 추상화·저장 호환성을 유지한다. 씬별 JSON/Registry 직렬화 목록은 공통 구성 참조로 이관한다.
- TECH-008의 종료 단계·연출 요청 식별과 TECH-011의 독립 표시 설정 형식은 유지한다. 표시 설정의 소유 수명만 씬에서 앱의 현재 슬롯으로 확장한다.
- TECH-010의 입력/안전 영역과 기존 임시 UI를 재사용한다. 공용 UI 소유와 전환 입력 무효화를 추가한다.
- Diagnostics는 공통 초기화 계약을 호출하는 어댑터를 두고 제품 Bootstrap에서 진단 타입을 찾지 않는다.

## 검증과 한계

정확한 구현 상태·검증 결과는 PLAN-016에 기록한다. Editor의 전체 흐름/직접 진입·저장 실패·설정 보존·반복 Play를 검증하며 사용자의 지시에 따라 빌드 테스트는 실행하지 않는다.

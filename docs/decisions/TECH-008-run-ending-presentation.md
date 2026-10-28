# TECH-008 멸망 연출과 결과 재시작 경계

- 결정일: 2026-10-28. [PLAN-011](../plans/PLAN-011-destruction-results-and-restart.md) 실행 승인 범위.
- 선행: [TECH-004](TECH-004-session-and-persistence.md), [TECH-007](TECH-007-invasion-time-and-save-boundaries.md).
- 기준: [한 판 종료](../game-design/01-direction/02-experience/core-loop.md#run-end), [GameFlow 책임](../game-design/05-development/01-architecture/game-flow.md), D-059 개발 위임.

## 책임과 신호

- GameFlow는 단계와 저장 경계를 유지한다. Presentation의 일반 C# `RunEndingPresentation`은 일시적인 연출 시간과 화면 수명을 소유하며 실제 View와 하네스가 같은 구현을 사용한다.
- `GameFlowManager.EndingRequest`는 발급된 요청 객체·현재 세션 참조·현재 단계가 모두 같아야 유효하다. 새 화면 발급·화면 비활성·세션 교체·완료 시 이전 요청을 무효화한다. 서로 다른 Manager에서도 단순 Generation 값 일치로 요청을 허용하지 않는다.
- `EndingBinding`은 현재 연결된 화면을 구분한다. 화면 교체로 해제된 이전 연결은 Reload/Restart 후에도 요청을 다시 발급받지 못한다. 현재 연결만 세션 교체를 따라 새 요청을 발급받는다.
- 실제 연출은 자기 요청으로 `FinishDestruction(request)`를 호출한다. 결과 버튼은 그 화면의 요청을 캡처해 `RestartFromResults(request)`로 전달한다. 재시작 실패는 요청과 세션을 유지해 재시도할 수 있고, 성공은 새 세션 공개로 이전 요청을 무효화한다.
- 인수 없는 `FinishDestruction()`은 Runtime 없는 기존 v1 진단 세션에만 남긴다. 실제 게임에 진단 완료 우회 경로를 노출하지 않는다.
- 실제 플레이 중 범용 Save/Continue/Restart는 기존 개발 화면에 유지한다. 종료 화면에서는 결과 전용 재시작과 저장 재시도만 노출한다. 진단 저장 슬롯은 기존 분리를 유지한다.

## 표현 시간과 입력

- 초기 연출은 4초의 임의(Temp) 도형·색·문구로 구성한다. `LaboratoryView.Ending.cs`의 세계 도형을 정식 아트로 교체할 수 있다. 새로운 Sprite 파일이나 게임 규칙은 없다.
- 실제 Update는 게임 delta에만 SimulationRate를 적용하며 연출에는 별도 unscaled delta를 준다. 목표 진입/복원/재개 첫 연출 입력은 버리고 이후 한 프레임의 연출 증가량은 최대 0.1초다. 큰 게임 delta와 긴 프레임으로 전체 연출을 건너뛰지 않는다.
- 앱 정지·포커스 상실·View 비활성 중 연출을 진행하지 않는다. 같은 View의 비활성/재활성은 진행량을 유지하고 첫 delta를 버린다. 새 View의 연결은 연출을 처음부터 시작한다.
- 종료 화면만 렌더링하여 배경 조작을 노출하지 않는다. 명령의 Playing 검사도 유지한다. 목표 시 미확정 건설/이동 편집과 선택을 취소하되 확정 배치·Job·예약·생산 상태를 바꾸지 않는다.

## 결과와 저장

- 결과는 실제 InvasionState의 누적 성과/목표·완료 활동 수·외부 개체 수·명시적으로 진행한 시간을 조회한다. 보고 합산·추가 지급·새 통계 저장은 없다. 초과 성과는 그대로 표시한다.
- 구버전 종료 이관은 `legacyEndVersion`을 존중하고 기록 부재를 표시한다. 통계를 0 성과의 새 승리처럼 꾸미거나 목표량으로 채우지 않는다.
- 연출 시간·요청·화면 선택은 저장하지 않는다. v4를 유지하며 Destruction 복원은 처음부터 재생, Results 복원은 결과 직행이다. v2/v3 이관 후에도 같은 정책을 적용한다.
- 실제 마지막 활동 확정 후 목표 저장, 실제 연출 완료 후 Results 저장을 유지한다. 파일 실패와 메모리 단계 전환은 별개이며 실패해도 게임 진행을 재개하지 않는다. 종료 상태의 주기 저장 재시도와 명시적 재시도를 유지한다.
- 재시작은 새 그래프 검증·파일 교체가 성공한 뒤 새 판을 공개한다. 실패한 복원/재시작은 현재 화면과 정상 파일을 유지한다.

## 대안과 검증

전체 게임 시간을 다시 켜는 방식은 종료 후 생산을 진행시키므로 제외했다. 연출 진행 저장은 재생 시 성과가 발생하지 않는 단순 표현에 불필요하여 추가하지 않았다. 단계나 Generation만 비교하면 같은 세션의 교체된 화면 또는 다른 Manager의 신호가 섞일 수 있어 현재 요청 객체도 함께 검사한다.

Edit 테스트는 Presentation을 참조해 실제 연출 시간을 검사한다. 기존 런타임 어셈블리 의존성·GUID는 유지하며 테스트 어셈블리에만 Presentation 참조를 추가했다. 실제 한 판·두 번째 판과 화면/실패 표시 검증은 PLAN-011의 실행 증거에 기록한다.

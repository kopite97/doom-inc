# TECH-016 공용 팝업의 요청 수명과 표시 분리

- 날짜: 2026-11-02
- 작업: [PLAN-019](../plans/PLAN-019-shared-popup-system.md)
- 기준: [객체 설계](../agents/object-design.md), [추상화](../agents/abstraction.md), [씬 흐름](../agents/scene-flow.md), [Localization](../agents/localization.md).

## 선택과 이유

Title/Laboratory 확인·설정의 반복과 씬/세션/부모 종료 시 입력·콜백 정리 책임을 공통화한다. 일반 C# PopupManager가 요청 정책을, MonoBehaviour PopupHost가 프리팹·코루틴·표시를 맡는다. 단일 구현에 불필요한 서비스 계층을 추가하지 않으며 IPopupHost/IPopupView는 Unity 애니메이션의 완료/실패를 독립적으로 검증하는 경계다.

PopupBase의 공통 초기화·열기·닫기는 비가상으로 고정하고 표시별 Populate/OnOpen/OnClose를 확장한다. Confirm/Notice는 문구 데이터와 콜백만 달라지는 공용 화면이며 Settings는 기존 설정 서비스를 사용한다. PopupBase를 상속하는 프리팹 제작 방식은 유지한다.

## 소유와 의존

- 앱 진입점이 PopupHost와 Manager를 하나 생성하여 전달한다. 별도 전역 Instance는 없다. 격리된 Diagnostics/테스트 진입은 자체 Host를 소유한다.
- PopupResource/Registry는 ID와 비활성 프리팹 GameObject를 참조한다. Data가 Presentation 타입을 참조하지 않으며 UI 설정은 GameDataStore/밸런스 JSON에 복제하지 않는다.
- Registry는 제작 가능 목록, PopupScope는 요청 유효 범위다. 씬별 등록은 검증 완료 후 교체하며 중복 ID를 덮어쓰지 않는다.
- PopupHandle.Children은 부모 팝업 수명이다. 부모 정리는 자식을 먼저 종료한다. 화면에 전달된 게임 동작은 팝업 구현 안에서 하드코딩하지 않는다.
- 소유 범위 종료는 사용자 취소와 구분한다. 이전 판/씬의 동작은 실행하지 않으며 콜백·구독·인스턴스·잠금을 회수한다.

## 결과와 입력

- 요청은 Opening/Open/Closing/WaitingForForeground/Closed로 진행한다. 사용자가 확정한 뒤 앱 포커스가 사라지면 인계를 보류하고 복귀 후 범위를 재검증한다.
- 결과는 한 번만 확정한다. 팝업 정리 후 콜백을 실행하며 예외가 나도 잠금 정리는 완료한다. 확인 결과는 후속 저장/씬 전환 성공을 의미하지 않는다.
- 모달 스택과 로딩/전환의 입력 차단을 조합한다. 확인 콜백이 로딩을 시작하는 동안에도 차단을 유지한다. AppUiLifetime의 세대 무효화는 닫은 손가락/키의 재사용을 차단한다.
- 시뮬레이션 정지는 기존 기능의 소유자가 담당한다. 팝업은 Time.timeScale을 변경하지 않는다.
- 멸망/결과의 단계 경계에서 플레이 범위를 교체한다. 결과 단계에서 새로 여는 메뉴 복귀 확인은 매 프레임 정리하지 않는다.

## 기존 결정과 대체 관계

[TECH-013](TECH-013-app-scenes-and-loading-screen.md)의 공용 LoadingScreen 중 정상 사용자 확인을 Popup으로 옮긴다. 초기화·전환 오류와 재시도 경로는 독립 LoadingScreen에 유지한다. Popup/Localization 준비가 실패해도 오류 화면을 사용할 수 있어야 한다.

[TECH-015](TECH-015-localization-and-csv-text.md)의 LocalizedMessage·TextManager와 CSV 검토 절차를 그대로 사용한다. 팝업의 키/인자를 유지해 언어 변경 시 재표시하며 화면 종료가 앱 소유 언어 변경 작업을 취소하지 않는다. uGUI Text·기존 폰트/UiWidgets를 사용하고 TMP/패키지 전환은 하지 않는다.

풀링·범용 비동기 동작 실행기·전역 우선순위 대기열은 현재 사용처에 필요하지 않아 도입하지 않는다. 정식 아트는 임시 프리팹의 참조/계층을 교체하는 별도 작업이다.

## 검증

상태별 예외·연타·중첩·포커스/소유자 종료는 Edit 테스트로, 실제 프리팹·씬·저장 복구·언어/레이아웃·맵 입력은 Play와 실제 Editor 화면으로 검증한다. 완료 여부·실행 수·한계의 원본은 PLAN-019이며 빌드 테스트는 제외한다.

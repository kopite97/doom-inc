# TECH-010 가로·세로 UI와 모바일 입력 경계

- 결정일: 2026-10-28
- 결정 주체·위임 근거: PLAN-013 실행에 대한 사용자 승인과 D-059의 화면·입력 구현 위임.
- 관련 작업: [PLAN-013](../plans/PLAN-013-responsive-ui-and-mobile-input.md).
- 기준: [화면과 조작](../game-design/03-player-experience/03-interface/controls.md)의 D-006·D-033·D-064·D-067·D-079·D-090, [플레이 환경](../game-design/04-delivery/01-platform/environment.md)의 D-068~D-070·D-089.
- 적용: 실제 Laboratory·로딩 화면 구현과 컴파일·입력/회귀 검증 및 Android APK 빌드를 완료했다. 사용자 요청으로 유예한 실기기 확인은 PLAN의 실행 기록에서 별도로 판정한다.
- 관계: [TECH-003](TECH-003-map-and-placement.md)의 Map·공통 명령, [TECH-008](TECH-008-run-ending-presentation.md)의 연출·종료 요청, [TECH-009](TECH-009-csv-data-catalog.md)의 Data 준비·재사용 경계를 유지한다. 기존 진단 씬의 IMGUI는 이 결정의 교체 범위에 포함하지 않는다.

## 문제와 제약

기존 실제 플레이 화면의 IMGUI 격자·고정 폭 패널은 좁은 세로 화면, 안전 영역, 터치 스크롤과 다중 포인터를 함께 다루기 어렵다. 로딩·오류·멸망·결과까지 같은 입력 규칙이 필요하며, 표시 방식 변경으로 세션·배치 초안·저장 규칙을 새로 만들면 안 된다.

엔진과 패키지는 업그레이드하지 않는다. 현재 [패키지 구성](../../UnityProject/Packages/manifest.json)의 uGUI 2.0.0·Input System 1.20.1과 `activeInputHandler=1`을 사용한다. 실제 플레이 조작을 처리하는 IMGUI와 uGUI 경로를 동시에 남기지 않는다.

## 결정

### UI와 화면 좌표

- uGUI의 Screen Space Overlay Canvas·GraphicRaycaster·ScrollRect·RectMask2D로 실제 화면을 구성한다. 입력은 InputSystemUIInputModule을 사용한다.
- [LaboratoryUiCanvas](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryUiCanvas.cs)가 안전 영역과 논리 좌표 변환을 소유한다. `Screen.safeArea`가 비어 있으면 전체 화면을 사용한다.
- 안전 영역의 짧은 변 600 논리 단위를 기준으로 `clamp(min(width, height) / 600, 0.5, 3)` 배율을 적용한다. 600은 기준 크기이며 배율 제한 밖의 화면까지 고정 최소 논리 크기를 보장하는 값은 아니다. 크기 검증은 실제 화면·안전 영역·배율을 함께 기록한다.
- 조작 목표는 최소 44 논리 단위, 기본 버튼 높이는 48 논리 단위로 잡는다. 배치 도구는 글자 폭을 포함해 최소 90 논리 단위 폭·6 단위 간격으로 배치하고, 편집 6개가 한 행에 들어가지 않으면 3+3행 또는 더 적은 열로 재배치한다. 늘어난 도구 높이를 Map 영역 계산에도 반영한다. 긴 버튼·보고·오류는 줄바꿈과 행 높이 계산을 사용하며 목록은 스크롤한다. 작은 격자 칸은 줌으로 확대하고 별도 전체 개체 목록으로도 선택한다.
- [LaboratoryView.Gui](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs)는 가로에서 연구소·측면 패널, 세로에서 연구소·하단 패널로 재배치한다. 패널 접기와 배치 도구를 제공하며 회전·확정·취소는 목록 스크롤과 분리한다.
- [LaboratoryMapView](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.cs)는 셀 그리기와 hit test에서 같은 원점·셀 크기·RectTransform 변환을 사용한다. Map의 왼쪽 아래 원점 규칙을 유지하고 Canvas 배율을 화면 이동량에 반영한다. 줌은 맞춤 배율 1부터 8까지이며 중심을 맵 경계로 제한한다.

### 터치와 마우스

- UI 버튼·스크롤은 InputSystemUIInputModule, Map은 EnhancedTouch의 `onFingerDown/Move/Up` 콜백을 통해 [LaboratoryGesture](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryGesture.cs)에 전달한다. 두 경로의 명령 대상과 소유 영역을 분리한다.
- `Touchscreen.touches`의 현재 상태만 Update에서 읽는 방식은 사용하지 않는다. 설치 Input System의 `Touchscreen.cs`는 같은 update에 여러 상태 변화가 발생하면 짧은 터치를 잃을 수 있음을 명시한다. EnhancedTouch 콜백은 시작·종료가 같은 update에 도착한 빠른 탭도 순서대로 처리하기 위한 선택이다.
- Map은 포인터가 시작한 위치를 GraphicRaycaster로 검사한다. UI에서 시작한 포인터, UI와 Map에 걸친 다중 터치, 세 번째 손가락은 Map 명령을 차단한다. 모든 손가락을 해제한 뒤 새 제스처를 시작한다.
- 탭은 release에서 선택 또는 배치 후보 위치만 전달한다. 드래그 임계치는 `max(10px, 12 × Canvas 배율)`이며 임계치를 넘으면 탭을 취소하고 화면을 이동한다. 핀치는 중심 이동과 거리 비율을 사용하며 남은 손가락을 새 탭으로 바꾸지 않는다.
- PC 마우스는 같은 gesture를 사용하고 휠·화면 버튼으로 확대/축소한다. 터치 이벤트가 발생한 프레임 또는 터치가 남은 동안 마우스 경로를 억제하여 터치 에뮬레이션을 중복 명령으로 해석하지 않는다.
- 화면·안전 영역·세션·GameFlow 단계 변화, 앱 pause/focus 변화, 컴포넌트 비활성, 입력 취소는 진행 중 gesture를 무효화한다. 취소한 포인터의 뒤늦은 release는 탭을 발생시키지 않는다. 구독은 활성 수명에 맞춰 한 번 연결하고 비활성 때 해제한다.

### 버튼 요청과 UI 수명

- [LaboratoryUiButton](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryUiButton.cs)은 press 시 포인터 ID·액션·의미 문맥·layout epoch를 보관하고 click에서 다시 검사한다. 화면 재배치·세션 교체·선택 대상 변경 이후의 이전 요청을 거부한다. epoch 조회 시 현재 화면·안전 영역을 동기화하여 LateUpdate 이전 release도 검사한다.
- [LaboratoryUiWidgets](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryUiWidgets.cs)의 목록은 의미 키로 행을 재사용한다. 매 프레임 버튼을 교체하여 스크롤과 누른 상태를 끊지 않는다. 사라진 행은 즉시 비활성화한 뒤 제거한다.
- 런타임에서 Canvas·버튼·목록을 생성한다. 현재 임의(Temp) 화면은 코드와 생성 도구가 같은 구성을 사용하며 별도 씬 YAML·프리팹에 중복 UI 원본을 두지 않는다. 정식 아트와 프리팹 제작은 후속 범위다.
- EventSystem이 이미 있으면 재사용하고, 없을 때만 InputSystemUIInputModule과 기본 UI 액션을 가진 소유 오브젝트를 만든다. 로딩·게임 Canvas는 사용 수를 공유하며 마지막 소유 화면이 해제되면 직접 만든 EventSystem도 제거한다. 외부 EventSystem은 파괴하지 않는다.
- [BootstrapLoadingView](../../UnityProject/Assets/Scripts/Presentation/Bootstrap/BootstrapLoadingView.cs)는 Data 준비 전의 로딩·오류·재시도를 담당한다. [GameBootstrap](../../UnityProject/Assets/Scripts/Presentation/Bootstrap/GameBootstrap.cs)은 Ready 이후 로딩 화면을 해제하고 기존 View·GameDataStore 준비 경로를 연결한다.
- [LaboratoryView.Ending](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Ending.cs)은 같은 Canvas 안에서 종료 화면을 표시한다. 화면 방향 변경이 RunEndingPresentation의 시간이나 EndingRequest를 다시 생성하지 않는다.

### 게임 상태와 Android 진입

- 모든 게임 조작은 기존 LaboratoryView → LaboratoryCommands/GameFlow/Map 명령을 거친다. UI는 Data와 Runtime 상태를 조회하고 비용·조건·저장·재시작 규칙을 복제하지 않는다.
- 방향 전환은 표현 재배치다. GameDataStore·읽기 전용 카탈로그·Controller·현재 세션과 배치 초안을 유지한다. 줌·스크롤·포인터 상태는 저장 형식이나 CSV 호환성 해시에 추가하지 않는다.
- [LaboratoryPlayerBuild](../../UnityProject/Assets/Scripts/Editor/Validation/LaboratoryPlayerBuild.cs)의 `Configure Laboratory Player Entry` 메뉴가 실제 `Temp_Laboratory`를 첫 활성 빌드 씬으로 지정하고 기존 나머지 씬 설정을 보존한다. 기존 SampleScene 환경 검사는 유지한다.
- `Build PLAN-013 Android APK` 메뉴는 별도 BuildPlayerOptions의 씬 배열을 실제 Laboratory 하나로 명시한다. 현재 Android 대상·autorotation·Input System·Bootstrap/View·CSV 의존성을 검사하고 AAB/export 임시 플래그는 빌드 후 복원한다. 프로젝트 입력·회전 설정이나 사용자 서명 설정을 자동으로 변경하지 않는다.
- 검증 APK는 추적 제외된 `UnityProject/Builds/PLAN-013`에 저장한다. 빌드 시도가 실행되면 BuildReport 요약·포함 씬·Player 설정·데이터 호환성 해시·APK SHA-256을 `docs/plans/evidence/PLAN-013`의 고유 JSON으로 기록한다. 빌드 성공과 기기에서의 최초 진입 성공은 별도로 판정한다.

## 대안과 선택 이유

| 대안 | 비교 결과 |
| --- | --- |
| IMGUI를 유지하며 터치 처리를 추가 | 안전 영역·스크롤·다중 포인터·UI 차단을 직접 구현해야 하므로 실제 화면에서는 교체한다. 진단 씬까지 확대하지 않는다. |
| 기존 IMGUI 패널과 새 터치 Map 혼용 | 같은 입력이 두 경로에 도달할 여지가 있고 화면별 수명·배율 규칙이 달라져 선택하지 않는다. |
| UI Toolkit 또는 외부 UI 프레임워크 신규 도입 | 현재 설치된 uGUI·Input System으로 필요한 경로를 구성할 수 있어 새 전환·도구 의존성을 추가하지 않는다. |
| Touchscreen 현재 상태 polling | 같은 update에 끝난 터치와 슬롯 재사용을 놓칠 수 있어 EnhancedTouch 콜백을 선택한다. |
| 화면 방향별 새 세션·UI 상태 저장 | 재배치가 게임 상태 변경과 저장 이관을 유발하므로 기존 세션을 조회하는 표현 계층을 유지한다. |

## 영향과 검증

Presentation 어셈블리에 설치된 `Unity.InputSystem`·`Unity.ugui` 참조를 추가한다. 공통 런타임의 UI 의존성, 게임 규칙, CSV 구조, 저장 스키마는 바꾸지 않는다. 현재 셀·목록 조회는 직관적인 전체 순회를 사용하며 가상화·메시 일괄 처리 같은 종합 성능 개선은 후속 로드맵 범위다.

제스처 단위 검사에 더해 실제 Input System에 주입한 터치·마우스가 UI/Map 어댑터를 통과하는 Play 검사를 수행한다. 빠른 탭·핀치·스크롤·회전 중 release·앱 복귀·세션 교체와 기존 한 판/두 판 회귀를 구분한다. 가로·세로 대표 화면과 안전 영역, 작은 화면·긴 문구를 확인하고 APK 최초 실행 및 실제 Android 기기 조작을 별도로 남긴다.

구현 후 전체 Edit 259/259·Play 23/23을 통과했다. 캡처에서 찾은 custom Graphic의 메시 경로 문제를 수정하고 종료 화면 대상 1/1과 실제 표시를 다시 확인했다. 실행 결과·빌드 환경·남은 문제·증거 파일의 원본은 [PLAN-013의 검증 기록](../plans/PLAN-013-responsive-ui-and-mobile-input.md)에 둔다. 문서 검사나 코드 검토만으로 자동 검사·빌드·실기기 완료를 대신하지 않는다.

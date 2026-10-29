# TECH-011 모바일 UI 상태 조회·설정·임시 에셋

- 결정일: 2026-10-29
- 결정 주체·위임 근거: 사용자의 PLAN-014 실행 승인 안에서 기존 규칙을 유지하는 구현 선택.
- 관련 PLAN: [PLAN-014](../plans/PLAN-014-mobile-ui-ux-and-facility-status.md)
- 기준 기획: [화면과 조작](../game-design/03-player-experience/03-interface/controls.md), [시설](../game-design/02-systems/01-production/facilities.md)
- 적용 여부: 아래 코드에 적용했으며 검증 결과는 PLAN에서 관리한다.
- 대체 관계: [TECH-010](TECH-010-responsive-ui-and-mobile-input.md)의 입력·좌표·안전 영역 경계를 유지하고 표시 책임을 확장한다.

## 문제와 제약

시설 위 표시와 상세 패널의 진행·대기 원인이 같아야 한다. 생산 규칙을 UI에 복제하지 않고, 표시 설정 때문에 게임 저장·정의 해시를 바꾸지 않아야 한다. 정식 아트는 별도로 승인한 art PLAN에서 교체한다.

## 결정

- [FacilityStatusQuery](../../UnityProject/Assets/Scripts/Gameplay/Production/FacilityStatusQuery.cs)가 실제 컴포넌트·Job·예약·입고 상태를 읽어 불변 조회값을 만든다. 패널과 오버레이가 이 조회를 공유한다. 시간 진행이나 명령 실행은 담당하지 않는다.
- [LaboratoryView.Navigation](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Navigation.cs)이 열린 패널·선택 상세·확인 단계·패널별 스크롤을 소유한다. 세션 교체 시 폐기하고 표시 설정은 유지한다. 공통 명령과 기존 입력 문맥 검사를 재사용한다.
- [UiSettingsStore](../../UnityProject/Assets/Scripts/Persistence/Storage/UiSettingsStore.cs)는 게임 저장 경로에 `.ui-settings.json`을 붙인 독립 파일을 사용한다. 표시 ON/OFF와 보통/125% 글자 크기를 저장하며 게임 snapshot·DataHash와 분리한다. 누락/잘못된 값은 기본값, 쓰기 실패는 현재 선택 유지와 재시도 안내로 처리한다.
- [LaboratoryMapView.Status](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.Status.cs)는 시설 ID별 표식을 재사용한다. 같은 맵 변환으로 앵커·클리핑·충돌을 계산하고, 일반 모드의 표식 탭은 선택, 배치 모드의 탭은 미리보기 이동으로 전달한다. 별도 Raycast 버튼으로 명령을 중복 실행하지 않는다.
- 선택한 시설의 표식을 다시 누르면 설명만 접거나 펼친다. 선택·패널·스크롤을 다시 초기화하지 않으며 명시적으로 접은 상태는 줌·회전 뒤에도 유지한다. 이 상태는 표식의 일시적 수명에 속하고 게임 저장이나 전역 UI 설정에 넣지 않는다.
- [LaboratoryUiAssets](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryUiAssets.cs)를 씬의 View에 직렬화하여 아이콘·한글 폰트·라이선스를 연결한다. 에셋 외형과 명령 의미를 enum으로 분리한다. 별도 전역 싱글턴이나 UI 프레임워크를 추가하지 않는다.
- Android의 실제 display density와 기존 Canvas 배율로 최소 터치 영역을 계산한다. 48 Canvas 단위가 자동으로 48dp가 된다고 가정하지 않는다. 화면 글자 확대는 게임 메뉴의 명시적 설정이다.
- 축소·확대·전체 보기·상태의 작은 아이콘 판과 실제 터치 영역을 분리한다. 가로에서는 상단에 배치하여 맵 높이를 확보하고, 기능 설명은 게임 메뉴에서 제공한다.

## 대안과 선택 이유

패널과 오버레이에서 상태를 따로 추론하면 완료품 운반·실제 작업자 여부가 어긋날 수 있어 공통 조회를 선택했다. UI 설정을 게임 저장에 포함하면 새 판 초기화와 결합되므로 독립 파일을 사용한다. 기존 uGUI 런타임 생성 구조를 유지하고 직렬화된 에셋 참조만 추가해, 프레임워크 전환과 정식 아트 교체를 이번 범위에 섞지 않았다.

## 영향과 검증

게임 규칙·CSV·저장 스키마는 변경하지 않는다. 조회 경계·실패 복구는 Edit, 실제 버튼/포인터·양방향·반복 객체 수는 Play, 밀도·센서 회전·핀치·복귀는 새 Android APK로 검증한다. 밀집 조건의 비용 측정은 전체 게임 최적화 완료를 뜻하지 않는다. 실행 결과와 남은 항목은 [PLAN-014](../plans/PLAN-014-mobile-ui-ux-and-facility-status.md), 정식 교체 규격은 [아트 요청](../../UnityProject/Assets/Sprites/Require/PLAN-014/README.md)에 기록한다.

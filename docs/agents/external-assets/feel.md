# Feel 사용 지침

적용: 게임 사건에 반응하는 효과음·진동·화면 점멸·흔들림 등 여러 피드백의 재생.
탐색: [외부 연출 에셋 MAP](../presentation-effects.md), [객체 설계](../object-design.md), [검증](../verification.md).

## 사용 기준

- 성공·실패·완료 등 사건이 확정된 지점에서 재생한다. 매 프레임의 상태 표시를 사건처럼 반복 재생하지 않는다.
- 하나의 사건에 필요한 피드백을 `MMF_Player`로 묶는다. 재생 대상과 수명은 해당 화면·오브젝트가 소유한다.
- 목표 상태로의 일반 UI 전환은 [DOTween Pro](dotween-pro.md)를 따른다. Feel이 같은 대상의 크기·색 등을 제어하는 동안 별도 Tween을 겹치지 않는다.

## 사용 절차

1. [저장소 복원 절차](../repository.md)에 따라 패키지와 실제 참조 음원을 준비한다.
2. 사건과 표시 대상을 연결하고 필요한 피드백만 구성한다. 현재 프로젝트는 `MoreMountains.Feedbacks`의 `MMF_Player`, `MMF_Sound`, `MMF_Scale`을 사용한다.
3. 코드로 구성할 때 현재 사용례처럼 Script 초기화 모드에서 피드백을 추가한 뒤 `Initialization()`을 호출하고 `PlayFeedbacks()`로 재생한다.
4. 연속 사건은 재시작·무시·중첩 중 해당 기능의 정책을 정한다. 동일 Player를 매번 새로 추가하지 않는다.
5. 닫기·비활성화·씬 종료 시 재생을 중단하고 변경한 표시값을 복원한다.

## 시간과 정리

- UI 피드백에는 필요한 경우 Player와 개별 Feedback의 unscaled 시간 설정을 맞춘다. 시뮬레이션 정지·배속과의 관계는 별도로 확인한다.
- `StopFeedbacksOnDisable`과 `RestoreInitialValuesOnDisable`을 확인한다. 이 옵션만으로 모든 사용자 정의 효과가 정리된다고 가정하지 않는다.
- `StopFeedbacks()` 뒤에도 프로젝트가 직접 바꾼 값이나 별도 생성 자원의 복원이 필요한지 확인한다.
- 비활성 화면·삭제된 대상·과거 세션의 사건이 현재 화면에 재생되지 않도록 호출 수명을 제한한다.
- 소리·진동 등은 해당 사용자 설정을 존중한다. 효과 재생 실패가 게임 명령의 성공 결과를 바꾸지 않게 한다.

## 현재 참고 코드

- [LaboratoryView.Gui](../../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs)의 `PlayReleaseFeedback`·`StopReleaseFeedback`: 방출 성공 시 소리와 아이콘 크기 효과, Player 재사용, 중단과 크기 복원.
- 음원 경로와 GUID 의존성은 [저장소 지침](../repository.md)을 기준으로 복원한다. 데모 원본을 임의 이동하지 않는다.

## 확인 항목

- 사건 한 번에 의도한 횟수만 재생되는지, 빠른 연속 호출 시 소리와 시각 효과가 과도하게 쌓이지 않는지 확인한다.
- 화면 닫기·다시 열기·씬 종료 후 소리나 변형이 남지 않는지 확인한다.
- 일시정지·배속·관련 사용자 설정과 정상적으로 동작하는지 확인한다. 실행 범위는 [검증 지침](../verification.md)을 따른다.

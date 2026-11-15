# DOTween Pro 사용 지침

적용: UI·오브젝트의 위치·크기·투명도 또는 재질 효과 수치를 시간에 따라 목표 상태로 전환할 때.
탐색: [외부 연출 에셋 MAP](../presentation-effects.md), [객체 설계](../object-design.md), [검증](../verification.md).

## 사용 기준

- 목표값·재생 시간·중단을 관리하는 전환에 사용한다. 즉시 상태 반영은 기본 API로 처리한다.
- 사건에 여러 피드백을 묶는 책임은 [Feel](feel.md), 글자별 변형은 [Text Animator](text-animator.md)에 둔다.
- 시뮬레이션 위치와 이동 보간에 별도 Tween을 덧씌우지 않는다. 현재 생명체 이동의 시간·보정 기준은 [TECH-018](../../decisions/TECH-018-creature-movement-interpolation.md)을 따른다.

## 사용 절차

1. 새 체크아웃의 임포트·Setup은 [저장소 복원 절차](../repository.md)를 따른다. 설치된 모듈과 호출 어셈블리 참조를 확인한다.
2. 전환 대상·시작값·목표값·시간·Ease를 정한다. 현재 코드처럼 `DG.Tweening`의 `DOTween.To`를 사용할 수 있다.
3. 소유 객체가 `Tween` 또는 `Sequence` 참조를 보관한다. 같은 속성의 새 전환을 시작하기 전에 이전 전환을 중단한다.
4. UI가 게임 정지와 무관하게 움직여야 할 때는 현재 사용례의 `SetUpdate(true)`처럼 시간을 명시한다. 이것이 프로젝트의 자체 시뮬레이션 배속을 따르는 설정이라고 간주하지 않는다.
5. 닫기·해제·파괴 시 소유한 전환을 중단하고 참조를 정리한다. 취소 시 최종값 적용 여부와 완료 콜백 실행 여부를 구분한다.

## 소유와 재사용

- 전역 전체 Tween 중단으로 다른 화면의 전환까지 끊지 않는다. 소유한 핸들로 제어한다.
- 게임 상태를 전환의 입력으로 사용한다. 시뮬레이션 결과나 저장 상태를 연출 완료 콜백만으로 확정하지 않는다.
- 재사용 대상은 이전 콜백이 새 요청을 처리하지 않도록 정리하고, 시작할 표시값을 다시 지정한다.
- 재질 값을 전환할 때는 [Sprite Shader 지침](all-in-1-sprite-shader.md)의 재질 소유 규칙을 함께 적용한다.
- 단일 전환만 필요한 곳에 공통 연출 관리자나 복잡한 Sequence를 미리 추가하지 않는다.

## 현재 참고 코드

- [PopupBase](../../../UnityProject/Assets/Scripts/Presentation/Popup/PopupBase.cs): CanvasGroup alpha 전환, 이전 Tween 중단, unscaled 시간, Release 정리.
- [LaboratoryMapView](../../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.cs): 화면 밖 선택 대상을 보여주는 지도 이동 전환.
- 기존 코드의 시간·수치는 해당 기능의 사례다. 다른 연출의 고정 기본값으로 복사하지 않는다.

## 확인 항목

- 빠른 열기·닫기·재선택 시 이전 전환이나 콜백이 남지 않는지 확인한다.
- 중단 후 재시작과 대상 파괴 시 표시·입력 상태가 일치하는지 확인한다.
- 게임 정지·배속에서 의도한 시간이 적용되는지 확인한다. 실행 범위는 [검증 지침](../verification.md)을 따른다.

# Text Animator 사용 지침

적용: 글자·단어 단위의 등장·퇴장·움직임. 정적인 문구는 기존 TMP 라벨을 사용한다.
탐색: [외부 연출 에셋 MAP](../presentation-effects.md), [Localization](../localization.md), [검증](../verification.md).

## 사용 기준

- 문구의 내용과 번역은 기존 TextManager·Localization 경로가 소유하고, Text Animator는 표현을 담당한다.
- 라벨 전체의 위치·투명도 전환은 [DOTween Pro](dotween-pro.md), 글자별 변형은 Text Animator로 나눈다.
- 단순 상태 문구에 자동으로 타이핑·움직임을 추가하지 않는다. 해당 기능에 필요한 효과만 활성화한다.

## 사용 절차

1. 설치·기본 콘텐츠 복원은 [저장소 지침](../repository.md)을 따른다. 버전에 따라 네임스페이스와 API가 달라질 수 있으므로 설치본과 현재 코드를 대조한다.
2. 대상 TMP 라벨에 `TextAnimator_TMP`를 연결한다. 화면을 다시 켤 때 기존 컴포넌트를 재사용한다.
3. 기본 효과·등장·퇴장 애니메이션 사용 여부와 시간을 명시한다. 현재 로딩 문구는 `defaultBehaviorTags`의 `wave`와 `TimeScale.Unscaled`를 사용한다.
4. 번역된 문구 변경과 연출 시작·중단 시점을 연결한다. 오류 표시 등 정적 문구로 전환할 때 TMP가 다시 정상 렌더링하도록 복원한다.
5. 화면 비활성화·재사용 시 이전 재생 상태를 정리하고, 언어 변경 후 새 문구에도 필요한 효과가 적용되는지 확인한다.

## 문구와 렌더링 소유

- 효과를 위해 번역 원문을 코드에 복제하지 않는다. 효과 태그를 문구에 넣는 경우 번역·서식 처리와 충돌하는지 확인한다.
- 같은 TMP 메시·글자 위치를 다른 효과 코드가 동시에 변경하지 않는다.
- 컴포넌트 비활성화만으로 정적 TMP 표시가 복원되는지 확인한다. 현재 사용례는 렌더 모드와 메시 갱신 상태도 복원한다.
- UI의 unscaled 시간은 게임의 자체 시뮬레이션 시간과 다르다. 게임 상태를 표현하는 문구 효과에 그대로 적용하지 않는다.

## 현재 참고 코드

- [LoadingTextWave](../../../UnityProject/Assets/Scripts/Presentation/Loading/LoadingTextWave.cs): `TextAnimator_TMP` 재사용, unscaled Wave, 등장·퇴장 효과 비활성화, 중단 시 TMP 렌더링 복원.
- [LoadingScreen](../../../UnityProject/Assets/Scripts/Presentation/Loading/LoadingScreen.cs): 로딩 화면 수명과 문구 연출 연결.
- 현재 사용하는 3.20.1 패키지의 복원 기준은 저장소 지침에 둔다. 다른 버전의 예제 API를 확인 없이 혼용하지 않는다.

## 확인 항목

- 한국어·영어와 문구 길이 변경 시 누락·잘림·태그 노출 없이 표시되는지 확인한다.
- 오류 전환·숨김·다시 표시할 때 정적 문구가 사라지거나 컴포넌트가 중복 추가되지 않는지 확인한다.
- 연출 중 진행률·문구 갱신과 일시정지 동작을 확인한다. 실행 범위는 [검증 지침](../verification.md)을 따른다.

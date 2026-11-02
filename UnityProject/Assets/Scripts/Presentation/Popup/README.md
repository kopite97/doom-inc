# 공용 Popup 사용과 유지보수

기준: [객체 설계](../../../../../docs/agents/object-design.md), [Localization](../../../../../docs/agents/localization.md), [Unity 에셋](../../../../../docs/agents/unity-assets.md), [TECH-016](../../../../../docs/decisions/TECH-016-popup-lifetime-and-presentation.md).

## 호출

앱이 전달한 PopupManager와 현재 화면/세션 PopupScope를 사용한다. 화면 자신이 전역 싱글톤을 찾거나 임시 Manager를 만들지 않는다.

```csharp
var handle = popups.ShowConfirm(scope, "return-title",
    new LocalizedMessage("Loading", "confirmation"),
    new LocalizedMessage("Loading", "return_prompt"),
    RequestReturnToTitle,
    confirmText: new LocalizedMessage("Loading", "save_return"));
```

- 동일 scope+key 호출은 기존 Handle을 반환하며 콜백을 교체하지 않는다. 서로 다른 대상의 요청은 대상 식별자가 포함된 key를 사용한다.
- 즉시 반환된 Handle.Result가 Busy/NotReady/Failed/OwnerEnded이면 열리지 않은 요청이다. 이를 정상 표시로 취급하지 않는다.
- 자식 요청은 parent를 지정하고 owner에 parent.Children을 전달한다. 최상위 부모가 Open일 때만 중첩할 수 있다.
- Handle.Cancel은 호출자 취소이며 사용자 취소 콜백을 실행하지 않는다. scope.Dispose는 해당 범위와 자식 요청을 종료한다.
- 확인 콜백에는 후속 동작을 전달한다. 저장/씬 로드 성공 여부와 오류 복구는 그 기능의 소유자가 처리한다. 앱 정지 중 인계는 복귀까지 기다리고 종료된 scope의 동작은 버린다.
- 팝업 안에서 자신의 Close/Destroy를 직접 호출하지 않는다. 선택은 공통 선택 경로로 전달한다. 실행 상태/콜백은 SO에 저장하지 않는다.

## 팝업 종류 추가

1. 문구만 다른 확인/알림이면 기존 DialogPopupRequest를 사용한다. 별도 화면 동작이 필요할 때 PopupBase 하위 클래스와 전용 PopupRequest를 추가한다.
2. 비활성 프리팹 루트에 PopupBase 하위 컴포넌트 하나를 연결하고 heading/scroll/confirm/cancel 참조를 구성한다. Unity가 .meta를 생성하게 한다.
3. PopupResource에 고유 ID와 프리팹을 연결하고 공용 또는 씬 PopupRegistry에 등록한다. GameBootstrap의 선택적 Popup Registry 필드로 씬 목록을 연결한다.
4. Tools/Mobile Game/Popup/Validate Assets로 공용 에셋을 확인하고 씬 전용 목록은 실제 진입에서 합성 검증한다. 공용/씬 ID 충돌은 오류다.
5. 실제 열기·닫기·중첩·씬 종료·언어 변경·가로/세로를 검증한다. GameDataStore에 팝업 에셋을 등록할 필요는 없다.

## 문구와 표시

- 제품 문구는 LocalizedMessage로 전달한다. CSV 변경은 Localization 지침의 미리보기→번역 검토→검증→가져오기→테이블 검증을 따른다.
- 현재 팝업은 프레임 표시 갱신을 사용한다. 같은 Text에 Observe 구독을 추가해 이중으로 갱신하지 않는다. 언어 변경은 콜백·애니메이션을 다시 시작하지 않는다.
- SettingsPopup은 즉시 적용하는 기존 설정 서비스를 호출한다. 화면을 닫아도 설정을 되돌리지 않는다. 언어 변경 작업은 앱이 소유한다.
- 기존 UiList가 행을 재사용하고 ScrollRect 위치를 유지한다. UI의 장식/프리팹을 바꿀 때 Safe Area와 최대 글자 크기, 아래 버튼 접근을 확인한다.

## 임시 에셋

Assets/Prefabs/UI/Popup/의 Temp_PopupHost·Temp_ConfirmPopup·Temp_NoticePopup·Temp_SettingsPopup을 사용한다. 공용 목록은 Assets/SO/UI/Popup/CommonPopupRegistry.asset이며 AppConfiguration의 popups/popupHost가 연결점이다.

Create Missing Assets는 없는 에셋만 만들고 기존 사용자 프리팹을 덮어쓰지 않는다. 정식 아트 교체는 별도 art PLAN이며, 기존 Script·ID·요청 수명을 유지한 채 프리팹의 표시 계층/참조를 교체한다. 신규 Sprite 요청은 현재 없다.

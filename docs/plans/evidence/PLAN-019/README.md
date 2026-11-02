# PLAN-019 실행·검증 기록

2026-11-02, Unity 6000.3.27f1의 열린 Editor/Pipeline에서 실행했다. 대상은 `UnityProject`, Android가 선택된 Editor의 Edit/Play Mode다. Player·Player 스크립트·APK/AAB·IL2CPP·Addressables 콘텐츠 빌드는 실행하지 않았다. 새 APK/실기기 검증도 이번 범위에 포함하지 않는다.

## 구현과 이관

- 일반 C# `PopupManager`는 요청·스택·중복·결과·소유 범위를 관리한다. 앱 소유 `PopupHost`는 프리팹·Canvas·입력 차단·unscaled 코루틴을 담당한다. 별도 Singleton/Instance를 추가하지 않았다.
- `PopupResource`/`PopupRegistry`는 정의 ID와 비활성 프리팹을 참조한다. AppConfiguration의 공용 Registry와 GameBootstrap의 선택적 씬 Registry를 합성 검증하고, 오류 시 기존 목록을 유지한다. 씬 Registry 검증은 새 세션 초기화/저장보다 먼저 한다.
- Title 새 게임, Laboratory 메뉴 복귀/새 판/불러오기 확인을 ConfirmPopup으로 이관했다. 두 화면의 시설 상태·큰 글자·언어 선택은 SettingsPopup, 세션 작업 실패는 NoticePopup을 사용한다.
- LoadingScreen은 초기 준비·전환·저장 실패와 재시도/취소를 유지한다. 실제 제품 경로의 정상 확인 표시를 이관했으며 기존 LoadingScreen API의 호환성 테스트는 유지했다.
- 플레이 범위 종료와 멸망 경계에서 설정/자식 알림/확인을 정리한다. 결과 화면의 새 메뉴 복귀 확인은 새 범위에 속한다. 확인 후 닫는 중 포커스가 사라지면 의도를 보존하고 유효한 소유자에게 복귀 후 한 번만 전달한다.
- 신규 문구는 Common/confirm(ko: 확인, en: Confirm) 한 개다. CSV 검토·가져오기·테이블 검증을 거쳐 전체 9개 주제/248개 키가 된다. 기존 LocalizedMessage의 키와 인자를 재사용한다.
- 임시 프리팹 4개와 공용 Registry/Resource SO를 생성했다. 새 Sprite는 필요하지 않으며 정식 아트 교체는 별도 art PLAN이다. 추가/교체 절차는 [Popup README](../../../../UnityProject/Assets/Scripts/Presentation/Popup/README.md), 책임·대안은 [TECH-016](../../../decisions/TECH-016-popup-lifetime-and-presentation.md)에 있다.

## 자동 검증과 수정 과정

- [초기 핵심 Edit](popup-core-edit-initial.json): 9/9 통과. 중복 요청, 부모·자식 정리, 사용자 취소/강제 종료 구분, 앱 복귀 인계, 생성/콜백 예외를 확인했다.
- [앱 Play 첫 검사](app-play-initial.json): 20/21. 설정 화면 이관 후 과거 Title의 Close 버튼을 찾던 테스트를 공용 Popup 취소 버튼으로 수정했다. [재검사](app-play-repeat.json) 21/21 통과.
- [전체 Edit](edit-regression-initial.json): 343/343 통과, 실패·건너뜀 0. 핵심 팝업 11개/Registry 2개 외 기존 게임 데이터·명령·저장·Localization 유지보수 검사를 포함한다.
- [전체 Play 첫 검사](play-regression-initial.json): 63/64. 설정 팝업 뒤의 Restart 버튼을 바로 누르던 기존 테스트에 실제 Back 조작을 추가했다. 입력 차단을 우회하거나 약화하지 않았다.
- [전체 Play 두 번째](play-regression-repeat.json): 64/65. 추가한 긴 본문 테스트에서 Game View 원복 직후 입력 준비 대기를 보완했다. [해당 검사 재실행](long-body-repeat.json) 1/1 통과. 동일 버튼의 실제 조작 가능 상태·전경 복귀 후 Submit을 보내며, 오래된 언어의 클릭 무효화 검사는 유지한다.
- 최초 화면 검수에서 프리팹으로 저장되지 않는 UiText의 기본 크기를 발견해 팝업 초기화에서 제목 26/버튼 18을 명시하고 큰 글자 배율을 적용했다. 설정 캡처는 열기 애니메이션 완료 후 실행하도록 수정했다.
- [전체 Play 세 번째](play-regression-third.json): 64/65. 기존 시설 개별 배정 검사에서 마지막 선택/해제 입력이 적용되지 않은 실행을 보존했다. UI 위치·실제 Raycast 대상·AppUiLifetime 입력 준비를 3프레임 확인한 뒤 새 물리 클릭을 시작하고, 해제 대상 선택을 별도 검증하도록 테스트 준비를 보완했다. 동작 재시도나 실패 무시는 하지 않는다. [시설 입력 재검사](facility-input-repeat.json) 1/1 통과.
- [전체 Play 최종](play-regression-final.json): **65/65 통과, 실패·건너뜀 0**. 기존 생산·배양·방출·보고·멸망/결과·새 판·저장 복구와 실제 입력 경로를 함께 확인했다. 새 팝업 준비 실패 복구, 씬 전용 팝업 확장/등록 실패/해제, 멸망 중 설정+자식 정리, 결과의 새 확인창 유지, 앱 복귀 확인 인계, 앱 소유 언어 변경, 긴 본문/스크롤/이전 클릭 무효화를 포함한다.

## 실제 Editor 재진입과 화면

아래는 Test Runner 밖에서 메뉴로 실행했다. 각 두 시점에서 프레임·게임 시간 증가, 준비 완료·오류 없음과 SceneFlowManager/GameDataStore/EventSystem/PopupHost 각각 1개를 확인했다.

| 진입·설정 | frame / time 변화 | 증거 |
| --- | --- | --- |
| Bootstrap 전체 흐름, Domain/Scene Reload ON | 1245→7648 / 3.75→20.54 | [첫 시점](direct-bootstrap-on-1.json), [두 번째](direct-bootstrap-on-2.json) |
| Title 직접 진입, Domain Reload OFF | 4845→11477 / 12.27→30.30 | [첫 시점](direct-title-off-1.json), [두 번째](direct-title-off-2.json) |
| Laboratory 새 판, Domain Reload OFF | 5600→9277 / 18.52→32.26 | [첫 시점](direct-lab-off-1.json), [두 번째](direct-lab-off-2.json) |
| Laboratory 새 판, Domain/Scene Reload OFF | 2952→11202 / 9.56→39.33 | [첫 시점](direct-no-reloads-new-1.json), [열린 설정 포함 두 번째](direct-no-reloads-new-2.json) |
| Laboratory 이어하기, Domain/Scene Reload OFF | 5716→14702 / 19.05→48.57 | [첫 시점](direct-no-reloads-continue-1.json), [두 번째](direct-no-reloads-continue-2.json) |

Title Settings의 실제 UiButton Submit으로 [열기](direct-settings-open.json)·[닫기](direct-settings-close.json)를 호출했다. Time.timeScale=0에서도 [Open](direct-settings-unscaled-open.json)→[제거/잠금 해제](direct-settings-unscaled-closed.json)가 완료됐다. 게임 시간은 21.185에 멈췄고 unscaled 코루틴은 진행했다. Laboratory의 [설정 요청](direct-lab-settings.json)과 [Back](direct-lab-back.json)도 정상이다.

[Title 설정을 연 상태](direct-open-before-stop.json) 및 위 no-reloads 새 판의 열린 설정 상태에서 Play를 종료했다. 다음 진입에서는 팝업/입력 잠금이 0개였고, [최종 종료](direct-stop-cleanup.json) 시 Host/View도 0개다. 제품 저장 대신 SlotB를 사용했으며 [원래 파일 3개](restored-files.json)와 [Editor 옵션·씬·슬롯·시작 모드·백그라운드 설정](direct-restored.json)을 복원했다.

큰 글자 상태의 [한국어 가로](popup-ko-1280x720.png)·[한국어 세로](popup-ko-720x1280.png)·[영어 가로](popup-en-1280x720.png)·[영어 세로](popup-en-720x1280.png), 설정 [가로](settings-1280x720.png)·[세로](settings-720x1280.png), 확인 [가로](confirm-1280x720.png)·[세로](confirm-720x1280.png)를 검수했다. [실제 Laboratory 설정](direct-runtime-settings.png)도 관찰했다. 본문 스크롤과 아래 버튼 접근을 검사하며 임시 화면의 여백/장식은 정식 아트 확정이 아니다.

[팝업 에셋](asset-validation.json)·[Localization 테이블](localization-validation.json) 검사, [최종 컴파일 상태](compile-final.json)를 확인했다. [최종 Editor](editor-final.json)는 Play 정지·컴파일 중 아님·백그라운드 실행 원복이다. 이전 실패/의도한 오류 주입 구간의 [콘솔](console-before-direct.json)을 보존한 뒤 직접 실행 구간을 분리했고 [마지막 콘솔](console-final.json)의 새 오류·경고는 0개다. 캡처 도구가 Assets/Temp에 생성한 임시 PNG는 증거로 복사 후 Editor API로 제거했다. 테스트 씬·확장 Registry·임시 Locale/키는 제품 에셋에 남기지 않았다.

## 검증 경계

Edit의 대체 Host는 애니메이션 완료·예외 순서를 통제한다. Play의 앱 흐름은 실제 씬/프리팹/저장 파일을 사용하며 Submit과 수명 이벤트를 호출한다. 기존 모바일 입력 검사는 격리 Input System 기기로 마우스·터치·Escape를 주입해 EventSystem/Raycast/맵 경로를 통과한다. 이는 실제 기기 터치/회전 센서 검증과 다르다.

Play 검사는 테스트 슬롯/설정 파일과 Game View 크기를 복구한다. 언어 변경 테스트는 앱 언어 파일의 원본 바이트/부재 여부를 복구한다. 팝업은 언어 변경 작업을 소유하지 않으며, 표시 종료 뒤 앱 언어/저장 값 갱신과 팝업 비재생성을 검사했다. 테스트용 구체 Popup과 씬 Registry는 메모리에서 생성·해제하여 제품 SO에 남기지 않는다.

## 재현

열린 Editor가 해당 프로젝트인지 확인하고 이전 실행 완료 후 다음 실행을 시작한다.

```powershell
unity command script recompile --project-path UnityProject --caller plugin --skill unity-cli --json
unity command script recompile_status --project-path UnityProject --caller plugin --skill unity-cli --json
unity command test run --mode editor --project-path UnityProject --caller plugin --skill unity-cli --json
unity command test run --mode playmode --async_tests true --project-path UnityProject --caller plugin --skill unity-cli --json
unity command test status --project-path UnityProject --caller plugin --skill unity-cli --json
unity command editor invoke_menu --path 'Tools/Mobile Game/Popup/Validate Assets' --project-path UnityProject --caller plugin --skill unity-cli --json
node scripts/validate-agent-docs.mjs
```

상태 응답의 `running` 및 0개 Summary는 완료 결과가 아니다. 최종 실행 수와 개별 결과를 함께 확인한다. 빌드 메뉴/명령을 실행하지 않는다.

최종 [문서 검사](docs-validation-final.txt)는 19 PLAN·active 0·목록/링크/지침 길이 검사를 통과했다. `git diff --check` 공백 오류가 없고, 신규 Unity 에셋/스크립트의 .meta와 변경 범위를 확인했다. 커밋·푸시는 이번 요청에 포함하지 않았다.

---
id: PLAN-023
type: development
status: completed
title: 씬 로딩 진행률·Feel 웨이브와 전체 게임 UI 물마루 TMP 전환
updated: 2026-11-04
---

# PLAN-023 씬 로딩 진행률·Feel 웨이브와 전체 게임 UI 물마루 TMP 전환

## 목표와 범위

플레이어가 다음 씬을 불러오는 동안 실제 씬 로드율과 물결처럼 움직이는 `Loading...`을 본다. 빠른 전환에서도 문구를 읽을 시간을 보장한다. 게임에 표시되는 모든 UI 문구를 `TextMeshProUGUI`로 전환하고 물마루 Dynamic 폰트 하나를 공유한다.

- 사용자 지정 범위는 프로그레스 바, 최소 로딩 시간, 보유한 Feel을 활용한 글자별 웨이브, 제품 텍스트 전체의 TMP 전환이다. 최소 표시 시간의 **기본값은 1초**이며 Inspector에서 조정할 수 있다.
- Bootstrap·Title·Laboratory와 공용 팝업, 초기화 실패 화면, 런타임에 생성되는 지도·HUD·결과·도움말·라이선스 문구까지 포함한다. 타이틀 배경에 그려진 워드마크는 이미지로 취급한다. Editor 도구·진단 전용 UI·외부 에셋 데모는 제품 텍스트 집계에서 제외한다.
- 기존 임시 씬·프리팹 이름을 사용한다. 막대는 uGUI 도형으로 구성하며 신규 Sprite 요청은 없다.

## 기준과 출발점

- 로드맵 4단계의 UI 후속 작업이다. D-091의 공용 로딩 및 실패 복구, D-090의 가로·세로 지원, 기존 입력·저장 수명 규칙을 따른다. [화면과 조작](../game-design/03-player-experience/03-interface/controls.md) · [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md) · [씬 전환 결정](../decisions/TECH-013-app-scenes-and-loading-screen.md)
- 현재 전환 코루틴은 데이터·번역 준비, 조건부 비동기 씬 로드, 대상 초기화, 보호 화면 아래 한 프레임 렌더 순서로 동작한다. `LoadSceneAsync`의 완료만 기다리고 있다. 기존 `SceneFlowManager`가 진행률과 전환 수명을 소유한다. [전환 코드](../../UnityProject/Assets/Scripts/Presentation/SceneFlow/SceneFlowManager.cs) · [공용 로딩 UI](../../UnityProject/Assets/Scripts/Presentation/Loading/LoadingScreen.cs)
- 제품 씬·프리팹과 게임 소스에서 기존 TMP 텍스트는 0개다. 팝업 프리팹 3개의 직렬화된 legacy Text 9개 외에 대부분의 라벨을 실행 중에 만든다. 현재 공용 에셋과 생성기는 나눔고딕 `Font`와 고지를 참조한다. [위젯 생성기](../../UnityProject/Assets/Scripts/Presentation/UI/Widgets/UiWidgets.cs) · [팝업 프리팹](../../UnityProject/Assets/Prefabs/UI/Popup) · [폰트 연결 코드](../../UnityProject/Assets/Scripts/Editor/Presentation/LaboratoryUiAssetBuilder.cs)
- Unity `6000.3.27f1`과 프로젝트의 uGUI 2.0.0에 포함된 TMP를 사용한다. Feel은 계정 보유·미다운로드 상태로 전달받았다. 실행 단계에서 해당 Unity ID의 My Assets에 제공되는 호환판을 확인한다. 문서상 Feel 5.9.1은 지원 범위이고 6.0은 Unity 6000.5 이상을 요구한다. [Feel 릴리스](https://feel.moremountains.com/feel-releases)
- 구현 시 [씬 흐름](../agents/scene-flow.md), [Unity 에셋](../agents/unity-assets.md), [현지화](../agents/localization.md), [검증 지침](../agents/verification.md)을 적용한다. 선행 작업은 [PLAN-016](PLAN-016-bootstrap-title-and-scene-flow.md), [PLAN-018](PLAN-018-localization-and-csv-text-management.md), [PLAN-019](PLAN-019-shared-popup-system.md)이다.

## 진행률과 최소 표시 시간

| 구간 | 플레이어에게 보이는 것 | 전환 조건 |
| --- | --- | --- |
| 데이터·언어 준비 | `Loading...`과 현지화된 단계 문구. 아직 씬 로드 요청이 없어 퍼센트는 감춘다. | 현재 준비 작업의 성공을 기다리고 비동기 씬 로드를 즉시 시작한다. |
| 다음 씬 로드 | 기존 코루틴에서 해당 요청의 `AsyncOperation.progress`를 읽어 막대에 반영하고 같은 값의 정수 백분율을 표시한다. 0.9는 90%다. | 미완료 상태는 최대 99%로 표시하고 `isDone`에서 100%를 확정한다. 한 프레임 사이에 완료돼 중간값을 관측하지 못하면 관측된 값만 표시한다. [Unity 진행률 API](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/asyncoperation/progress) |
| 씬 로드 완료·대상 준비 | 씬 로드가 끝나면 막대에 100%를 표시하고 대상 UI 또는 세션을 준비하는 동안 단계 문구로 이를 알린다. | 대상 준비 성공, 보호 화면 아래 렌더 한 프레임, 최소 표시 시간 경과, 앱 전경 복귀를 모두 만족하면 로딩을 닫고 입력을 푼다. |
| 현재 씬 직접 진입·실패 | 직접 진입(`UseCurrent`)은 단계 문구와 웨이브만 표시한다. 실패하면 즉시 웨이브와 바를 걷고 오류·복구 UI를 표시한다. | 직접 진입 성공에도 공용 로딩의 최소 시간을 적용한다. 재시도는 새 요청으로 진행률과 시계를 초기화한다. |

`AppConfiguration`에 0 이상으로 조정 가능한 최소 표시 시간을 두고 기존 구성 에셋에도 기본값 1초를 반영한다. 전환 요청마다 보호 화면이 **처음 그려진 프레임부터** 앱이 전경에 있는 시간만 게임 배속과 무관하게 누적한다. 일시정지와 포커스 상실이 겹쳐도 한 구간으로 처리하고, 복귀 첫 프레임에 배경 체류 시간을 더하지 않는다.

기본 설정에서 0.2초 만에 준비되면 약 0.8초만 더 표시한다. 준비에 3초가 걸리면 대상 화면의 보호 렌더가 끝나는 첫 전경 프레임에 연다. 최소 시간을 채우는 동안 뒤 화면의 입력·연구소 시뮬레이션·주기적 자동 저장은 계속 잠근다.

씬 활성화는 Unity 기본 흐름에 맡긴다. 데이터 준비와 화면 준비 사이의 두 번째 `Loading.Show`와 프레임별 진행 갱신이 타이머·웨이브를 재시작하거나 입력 잠금을 갱신하지 않게 분리한다. 기존 요청 토큰·진행 중인 씬 로드의 소유권 검사를 유지해 이미 폐기된 작업이 새 화면을 닫지 못하게 한다.

## 작업 목록

- [x] **의존성과 글꼴.** 사용자가 임포트한 호환 Feel 패키지에서 제품이 참조하는 런타임 피드백과 의존성·버전을 기록한다. 공식 물마루 원본과 저작권 고지·OFL을 포함하고 게임의 글꼴 라이선스 화면에도 연결한다. 제품·생성기·테스트 어셈블리에 필요한 TMP 참조와 필수 셰이더·리소스를 확인한다. [물마루 배포처](https://github.com/mushsooni/mulmaru) · [라이선스](https://github.com/mushsooni/mulmaru/blob/main/LICENSE)
- [x] **공유 Dynamic TMP 폰트.** 원본을 참조하는 `TMP_FontAsset` 하나를 만들고 Atlas Population Mode를 `Dynamic`으로 설정한다. 새 글자는 실행 중 채운다. 개발용 고정 글자 아틀라스를 굽지 않는다. 공용 구성 SO와 연구소 씬의 직렬화된 참조, 최초 Bootstrap 로딩 경로, 에셋 생성기를 함께 바꾼다. 기존 에셋이 이미 있는 프로젝트에서도 재생성 시 참조가 보존되어야 한다. [Unity 동적 폰트](https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/TextMeshPro/FontAssetsDynamicFonts.html)
- [x] **제품 텍스트 전수 이관.** 공용 라벨·목록·버튼, 초기 및 공용 로딩·오류, 타이틀, 연구소 HUD·지도 셀과 상태 캡션·보고·결과, Confirm·Notice·Settings 팝업 자식까지 바꾼다. 팝업 직렬화 필드와 실제 프리팹, 프리팹 생성기, 테스트의 타입 조회도 이관한다. 독립 생성 라벨과 비활성·재사용 행을 포함한다.
- [x] **가독성과 현지화.** TMP에서 지정 폭 기준의 선호 높이를 다시 계산한다. 기존 일반 문구는 `richText=false`로 표시하고, 큰 글씨 1.25배, 줄바꿈·마스크·스크롤·버튼 터치 영역과 라벨의 입력 통과를 확인한다. 지도 도움말 `−`·`＋`는 한·영 CSV에서 물마루가 지원하는 ASCII `-`·`+`로 정리한다.
- [x] **첫 프레임의 문구.** 웨이브용 `Loading...`은 [Bootstrap.csv](../../UnityProject/Assets/CSV/Localization/Bootstrap.csv)에 ko/en이 같은 전용 키로 둔다. 기존 CSV 가져오기 절차로 String Table과 내장 `BootstrapText.json`을 함께 갱신한다. 번역 준비 전의 첫 진입과 번역 초기화 실패를 각각 재현해 정상 문구와 오류 안내가 해당 상태에 맞게 표시되는지 확인한다.
- [x] **전환 코루틴·막대.** 위 표의 입력값과 종료 조건을 기존 공용 오버레이에 연결한다. 최초 진입, 이어하기, 새 게임, 메뉴 복귀에 같은 규칙을 적용한다. 로딩·오류 상태가 바뀌어도 기존 확인 팝업과 저장 복구 동작을 보존한다.
- [x] **Feel 웨이브.** 전용 TMP 라벨 하나에서 `L`부터 마지막 점까지 글자별로 위상이 다른 수직 파동을 만든다. Feel의 `MMF_Player`와 Property 피드백이 파동의 위상을 게임 배속과 독립적으로 반복 구동하고, 라벨 효과가 각 글자의 원래 정점에 오프셋을 적용한다. 최초 표시 때 한 번 시작하고 오류·숨김·해제 시 중지하고 원위치를 복원한다. 크기·언어가 바뀌면 기준 정점을 다시 구한다. 진폭·주기는 조정 가능하게 둔다. [Feel 피드백 목록](https://feel-docs.moremountains.com/list_mmfeedbacks.html)
- [x] **통합 확인.** 씬 흐름·팝업·연구소·번역의 기존 Edit/Play 회귀와 아래 시나리오를 실행하고 결과 수치·로그·화면 증거를 이 PLAN에 기록한다. UI 공통 기술 결정에는 실제 채택한 TMP 바인딩과 Feel의 책임 범위를 반영한다.

## 완료 기준과 검증 방법

| 확인할 결과 | 실행과 증거 |
| --- | --- |
| 실제 진행률과 최소 시간이 독립적으로 동작한다 | Play Mode에서 기본값 1초와 대기 해제값 0초를 검사한다. 빠른 전환은 전경 표시 누적 1초 전에 닫히면 실패다. 느린 전환은 테스트의 준비 작업을 1초 넘게 보류해 재현하고, 대상 준비·보호 렌더 후 첫 가능한 프레임에 닫히는지 확인한다. 원시 진행률·표시값과 요청별 첫 표시·씬 로드 완료·닫힘 프레임을 기록한다. |
| 화면을 떠났다가 돌아와도 남은 시간만 적용된다 | `timeScale = 0`과 일시정지·포커스 상실이 겹치는 경우를 검증한다. 이 테스트에서는 매 프레임 포커스를 되살리는 `AppSceneFlowTests.KeepFocused()`를 제어한다. 전경에서 약 0.4초 본 뒤 1초 이상 배경에 머물고 돌아오면 남은 약 0.6초 동안 입력이 잠겨야 한다. |
| 조작·저장·오류 복구가 유지된다 | Bootstrap → Title → Laboratory → Title 및 직접 실행을 검증한다. 1초 안에 실패를 감지하면 시간 대기 없이 오류 UI로 전환하고 재시도는 새 시계로 시작해야 한다. 연속 탭·저장 실패·메뉴 복구·로드 중 소유자 파괴를 재현하고 이전 요청의 진행률·웨이브 및 잠긴 화면의 조작이 다음 요청으로 넘어가지 않는지 확인한다. |
| 모든 제품 텍스트가 공유 폰트를 쓴다 | Edit Mode에서 제품 씬·프리팹의 비활성 자식, 생성기와 원본 폰트 참조를 검사한다. 빈 동적 아틀라스에서 시작해 Play Mode의 실제 화면을 열고 legacy Text 0개, 모든 제품 TMP 라벨의 물마루 참조 일치, ko/en·런타임 기호·라이선스 문구의 누락 글리프 0건을 확인한다. 기본 TMP 폰트의 대체 표시로 통과하지 않도록 글리프 출처도 대조한다. |
| 문구를 읽을 수 있고 도트·웨이브가 선명하다 | 가로 1280×720·세로 720×1280, ko/en, 보통·큰 글씨에서 로딩·오류·타이틀·지도·가장 긴 팝업 및 결과 화면을 확인한다. 웨이브는 서로 다른 시점의 프레임으로 개별 글자 움직임·단계 간 연속성·재시도 후 중복 재생 여부를 확인한다. |

## 현재 상황과 다음 행동

공용 전환 코루틴·로딩 UI·게임 전체의 폰트 이관을 적용했다. 프로젝트에 함께 임포트된 DOTween Pro와 Text Animator도 확인했다. 로딩 웨이브의 재생은 Feel 하나가 소유한다. 검증 환경은 Unity 6000.3.27f1 Editor의 Game View다.

## 검증 결과

| 날짜 | 대상·방법 | 결과 |
| --- | --- | --- |
| 2026-11-04 | PLAN 작성 전 코드·에셋·기존 폰트 설정 확인 | 씬 진행값 미사용, legacy Text 기반 제품 UI, Feel·물마루 미도입을 확인했다. 구현·Play 검증은 대기 중이다. |
| 2026-11-04 | 저장소 루트에서 `node scripts/validate-agent-docs.mjs` 실행 | PLAN 23개와 목록 23개 일치, active 0개, 로컬 문서 링크 검사 통과. |
| 2026-11-04 | 사용자가 임포트한 Feel 버전과 제품 어셈블리 참조 확인 | Feel 5.9.1의 `MoreMountains.Tools`를 제품 Presentation이 직접 참조한다. `LoadingTextWave`가 `MMF_Player`의 Property 피드백으로 글자별 파동을 구동한다. 배속과 무관한 반복·오류·재시도 복원 Play 테스트 통과. [연출 코드](../../UnityProject/Assets/Scripts/Presentation/Loading/LoadingTextWave.cs) · [개별 Play 결과](evidence/PLAN-023/play.json) |
| 2026-11-04 | 물마루 v1.0 원본·OFL·직렬화된 바인딩 대조 | `Mulmaru.ttf`를 원본으로 쓰는 Dynamic TMP 폰트 하나가 TMP 기본 설정·공용 구성·연구소 씬에 연결됐다. 게임의 글꼴 고지는 원본 OFL에 연결됐다. Confirm·Notice·Settings 프리팹의 TMP 라벨 9개 모두 같은 폰트와 머티리얼을 사용하며 legacy Text는 0개다. 교체 전후 RectTransform도 일치했다. [물마루 폰트와 고지](../../UnityProject/Assets/Fonts/Mulmaru) |
| 2026-11-04 | Unity 6000.3.27f1 Editor의 Edit Mode 및 개별 Play Mode 사례 | Edit 357/357 통과, 실패·건너뜀 0. 빈 물마루 아틀라스에서 `Loading...`·한글·숫자·기호를 렌더하고 글리프 출처를 검사한 Play 테스트가 통과했다. CSV 테이블 로드·언어 전환 및 열린 확인창 재번역도 통과했다. [Edit 결과](evidence/PLAN-023/edit.json) · [개별 Play 결과](evidence/PLAN-023/play.json) |
| 2026-11-04 | 실제 Game View 캡처 | 영어 큰 글씨 연구소의 HUD·지도·시설 상세를 1280×720에서 확인했다. 720×1280에서는 한글 팝업의 도움말·닫기 버튼과 결과 화면의 점수·저장 실패 안내·재시도 버튼을 확인했다. [가로 연구소](evidence/PLAN-023/laboratory-en-large-landscape.png) · [세로 팝업](evidence/PLAN-023/popup-ko-portrait.png) · [세로 결과](evidence/PLAN-023/results-portrait.png) |
| 2026-11-04 | 공용 전환과 통합 Play Mode | 최종 71/71 통과, 실패·건너뜀 0. 처음에는 Text Animator의 기본 콘텐츠 임포트가 Play 중 시도되어 지도 테스트 1개가 실패했다. 해당 테스트 단독 재실행과 이후 전체 회귀 모두 통과했다. [첫 실행](evidence/PLAN-023/play.json) · [지도 재실행](evidence/PLAN-023/play-map-rerun.json) · [최종 Play 결과](evidence/PLAN-023/play-final.json) |
| 2026-11-04 | 전경 시간·입력·오류·재시도 | 게임 시간 정지와 포커스·일시정지 중첩, 최소값 0초, 느린 준비 후 추가 지연 없는 전환, 로드 소유자 교체와 저장 복구를 검증했다. 필수 로캘 하나가 없는 격리 환경에서 초기 Loading...과 번역 실패 직후 내장 오류 제목·재시도 버튼까지 확인했다. [초기 번역 실패 검증](evidence/PLAN-023/play-localization-cold.json) |
| 2026-11-04 | 개발용 폰트 마무리 | 검사에 사용한 글리프를 비웠다. 저장된 공유 TMP 폰트의 문자·글리프 테이블은 각각 0개이며 원본 TTF와 Dynamic 모드가 유지된다. |

### 씬 전환 프레임 기록

- 한국어 가로 1280×720과 세로 720×1280의 실제 전환에서 준비 중 백분율 숨김, 이후 0% → 90% → 100%를 관측했다. 첫 표시부터 닫힘까지 각각 약 1.002초·1.003초였다. 로그의 `endOfFrameOperation`은 해당 프레임이 끝날 때 다시 읽은 값이므로 코루틴이 앞서 읽은 값과 한 프레임 차이가 날 수 있다. [가로 프레임](evidence/PLAN-023/landscape-frames.txt) · [세로 프레임](evidence/PLAN-023/portrait-frames.txt) · [가로 90%](evidence/PLAN-023/loading-progress-landscape.png) · [세로 90%](evidence/PLAN-023/loading-progress-portrait.png)
- 100% 유지 구간에서 웨이브가 계속 움직이고, 언어·크기가 바뀌어도 백분율과 안내 문구가 읽히는 것을 확인했다. [100% 첫 프레임](evidence/PLAN-023/loading-complete-landscape.png) · [다음 웨이브 프레임](evidence/PLAN-023/loading-wave-landscape.png) · [영어 큰 글씨](evidence/PLAN-023/loading-en-large-portrait.png) · [오류 복구](evidence/PLAN-023/loading-error-portrait.png)

## 상태 전환

2026-11-04 사용자 요청에 따라 `development / proposed`로 등록했다. 같은 날 사용자가 전체 게임 문구의 TextMeshProUGUI 전환과 물마루 적용을 선택했다.

2026-11-04 사용자의 실행 승인으로 `proposed`에서 `active`로 전환했다.

2026-11-04 제품 구현과 Editor 검증을 마쳐 `active`에서 `completed`로 전환했다.

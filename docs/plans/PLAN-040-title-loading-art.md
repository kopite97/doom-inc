---
id: PLAN-040
type: art
status: completed
title: 가로·세로 타이틀과 로딩 화면 아트 제작·적용
updated: 2026-11-17
---

# PLAN-040 가로·세로 타이틀과 로딩 화면 아트 제작·적용

## 최신 통합 상태 — 2026-11-17

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

## 제작 우선·마지막 일괄 적용

2026-11-16 사용자 「모든 아트리소스를 먼저 작업한 다음 한번에 Unity에 적용」「검증은 마지막 Art플랜에서 한번에」 요청을 반영한다. [PLAN-042 실행 절차](PLAN-042-art-integration-review.md#batch-art-workflow)를 이 묶음의 공통 운영 기준으로 사용한다.

- 이 PLAN 단계는 Art 작업 원본·리비전별 PNG/시트·명세·미리보기 제작과 제출 목록 작성까지다. 파일 저장/열기·출력 누락 같은 제작 작업 확인 및 문서 검사는 가능하지만, 정식 기술 검증·통합 시각 검수의 통과로 기록하지 않는다.
- 파일은 Art의 지정 리비전에 보관한다. Unity Assets로 PNG를 복사하지 않고, 임포트·슬라이싱·SO/씬/프리팹/재질·연결 코드·생성 도구 변경·Edit/Play·게임 캡처는 PLAN-042로 유보한다. 기존 화면/참조는 일괄 적용 전까지 유지한다.
- 아래 디자인·파일 수·요청 ID·Art 원본/리비전·Unity 최종 경로·적용 요구·검증 항목은 유지한다. 사용처별 적용과 모든 정식 검증은 PLAN-042가 담당하며, 이 PLAN의 체크는 해당 통합 증거 링크로 충족한다.
- 제작이 끝나면 `on hold`(일괄 적용·검증 대기)로 전환한다. 제작 완료를 Unity 아트 PLAN의 `completed`로 간주하지 않는다. 제작 묶음의 모든 에셋에 대해 아래 사용자 확인을 마친 뒤, 승인된 순차 실행 범위에서 다음 제작 PLAN으로 넘어간다. active는 최대 1개다.
- **제작 후 사용자 확인 필수:** 에셋별 정면/시트 미리보기와 애니메이션 GIF, 리비전·변경 내용을 제시하고 사용자 응답을 기다린다. 수정 요청은 새 리비전에 반영해 다시 보여준다. 해당 묶음의 외형·동작 확인 전 다음 Unity 제작 PLAN을 시작하지 않는다. 승인된 리비전·응답·날짜를 Art PLAN과 요청 README에 기록한다. 이 원본 확인은 PLAN-042의 기술 검사·게임 화면 검증과 별도이며, 기존 승인 외형은 통합 단계에서 변경된 경우에만 재확인한다.
- 앞 PLAN의 리소스를 참고할 때는 Art 제출본·디자인/규격을 사용한다. 앞 PLAN의 Unity 적용·게임 검증·completed 상태를 후속 제작의 선행 조건으로 요구하지 않는다.
- 사용자 PLAN-040 제작 요청에 따라 원본·미리보기만 제작한다. Unity 적용·정식 검증은 PLAN-042로 유보한다.

## 목표와 범위

정식 연구소와 같은 세계로 느껴지는 가로/세로 타이틀과 최소 로딩 장식을 적용한다. 시작·이어하기·설정·오류/재시도·실제 로딩 진행은 유지한다.
정식 게임명은 [콘셉트](../game-design/01-direction/01-identity/concept.md)에서 미확정이다. 이름이 들어가는 로고/워드마크는 필수 산출물에서 제외하고 기존 TMP 제목을 유지한다. 이름 확정 후 필요하면 별도 범위를 정한다.

## 기준 문서와 현재 확정 사항

- [TitleView](../../UnityProject/Assets/Scripts/Presentation/Title/TitleView.cs)·[LoadingScreen](../../UnityProject/Assets/Scripts/Presentation/Loading/LoadingScreen.cs)·[씬 생성](../../UnityProject/Assets/Scripts/Editor/Scenes/AppSceneAssets.cs).
- 현재 Temp_NightShiftLandscape/Portrait는 정식 스타일 승인본이 아니다.
- 선행: [PLAN-038](PLAN-038-shared-ui-skin-art.md)의 공용 버튼/패널. 기존 승인 아트는 모티프로 참고하며 원본 픽셀을 임의 수정하지 않는다.

## 디자인 의도와 원하는 모습

지하 연구소 한 구석을 낮은 조도로 보여주고 작은 배양조·작업등·익숙한 시설 실루엣으로 운영 게임임을 전한다. 복잡한 전경보다 메뉴가 들어갈 빈 영역을 확보한다.
가로는 장면/메뉴 좌우 구성, 세로는 상단 장면/하단 메뉴를 제안한다. 가로 그림을 강제 잘라 세로에 맞추지 않는다. 어두운 회청색 바탕과 장치/생체의 작은 강조색으로 시선을 제한하고 텍스트보다 배경 대비를 낮춘다.
기존 월드에서 천장광·재질·귀엽고 기괴한 분위기를 가져오고 정확한 타일 배열을 확대 복제하지 않는다. 로딩은 작은 배양 용기/연구소 표식 하나를 제안한다. 가짜 진행률·문구는 그림에 넣지 않는다.

## UI 산출물과 표시 명세

| 대상 | 제안 출력 | 안전 영역 |
| --- | --- | --- |
| TitleLandscape | 원본1→16셀 PNG 시트1·2560×1440, 셀640×360 | 왼쪽 메뉴/제목 여백, 중심 Contain·크롭 없음 |
| TitlePortrait | 원본1→16셀 PNG 시트1·1440×2560, 셀360×640 | 상단 장면·하단 메뉴, 중심 Contain·크롭 없음 |
| LoadingDecoration | 원본1→16셀 PNG 시트1·256×256, 셀64×64 투명 | 실제 진행/문구와 떨어진 작은 영역 |

총 원본/PNG 시트 각3개, 각16프레임·1태그·150ms/프레임·2.4초 반복이다. 4열×4행, 행 우선 순서·간격0·여백0. 기존 r001에서 정한 Rect·크롭 앵커·핵심 보존 영역을 유지한다. 비율 유지 Fill/여백 조합을 비교하고 비균등 확대는 하지 않는다. 공용 버튼/프레임은 별도 그림으로 중복 제작하지 않는다.

| 상태 | 표시·입력 |
| --- | --- |
| 새 게임/이어하기 가능·불가 | 같은 배경, 기존 저장 상태와 공용 버튼 |
| 로딩 | 반복 기포 장식+실제 진행률·기존 Feel 웨이브 |
| 오류/재시도 | 기존 문구·버튼 영역, 장식은 비조작 |

## 동작별 움직임과 재생 계약

- 타이틀 Ambient(1~16): 기포 세 갈래가 위로 올라오며 작은 생명체 몸·눈·그림자가 함께 약 ±2px 떠다닌다. 방·장비·유리 반사·작업등·메뉴 여백은 고정한다.
- 로딩 Bubbles(1~16): 용기·생명체는 고정하고 작은 기포만 서로 다른 높이에서 반복 상승한다.
- 각150ms·총2.4초 정방향 무한 반복. 타이틀 표시 중 재생·숨김/씬 이탈 시 정지, 가로세로 변경 시 같은 위상 유지 권장. Loading일 때만 기포 재생, 오류/확인/숨김에서는 정지·숨김, 재진입은 첫 프레임.
- 실제 로딩 진행과 기포 주기를 연결하지 않는다. 기존 Feel 문구 웨이브와 역할을 분리한다. 런타임 재생 연결·중복 재생/해제·반복 경계·실제 시간 확인은 PLAN-042.

## PLAN-042에서 실행할 사용처별 적용 계획

`Assets/Sprites/UI/Title/TitleLandscape.png`·`TitlePortrait.png`, `Assets/Sprites/UI/Loading/LoadingDecoration.png`를 실제 직렬화 참조·AppSceneAssets에 연결한다. TitlePortrait도 같은 Title 디렉터리다.
Sprite Mode Multiple로 4×4 셀을 분할하고 Sprite 배열/프레임 시간 재생을 연결한다. 타이틀 시트 최장변2560을 축소하지 않도록 Max Size4096 이상·Point를 사용한다. 상세 셀 좌표·이름·시간은 Art r002 명세를 따른다.
그림은 Raycast 비활성, 제목/문구는 Localization·물마루 TMP 유지. 실제 진행·최소 로딩 표시 시간·씬 구조를 바꾸지 않는다. 기존 로딩 효과를 복제하거나 새 패키지를 넣지 않는다.

## 결과물 저장 위치

아래는 이번 PLAN에서 사용할 **예정 경로**다. 위치 지정은 제작·수령 완료를 뜻하지 않으며 파일/빈 폴더는 지금 만들지 않는다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-040/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r002 원본 보존본 | r002 PNG 제출본 | r002 제출 명세 |
| --- | --- | --- | --- | --- |
| `TitleLandscape` | `assets/ui/TitleLandscape/source/TitleLandscape.aseprite` | `assets/ui/TitleLandscape/revisions/r002/source/TitleLandscape.aseprite` | `assets/ui/TitleLandscape/revisions/r002/export/TitleLandscape.png` | `assets/ui/TitleLandscape/revisions/r002/README.md` |
| `TitlePortrait` | `assets/ui/TitlePortrait/source/TitlePortrait.aseprite` | `assets/ui/TitlePortrait/revisions/r002/source/TitlePortrait.aseprite` | `assets/ui/TitlePortrait/revisions/r002/export/TitlePortrait.png` | `assets/ui/TitlePortrait/revisions/r002/README.md` |
| `LoadingDecoration` | `assets/ui/LoadingDecoration/source/LoadingDecoration.aseprite` | `assets/ui/LoadingDecoration/revisions/r002/source/LoadingDecoration.aseprite` | `assets/ui/LoadingDecoration/revisions/r002/export/LoadingDecoration.png` | `assets/ui/LoadingDecoration/revisions/r002/README.md` |

### Unity 수령과 최종 적용

아래 경로는 PLAN-042에서 일괄 수령·적용할 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-040/README.md`에 둔다. 아래 요청 ID로 상세 요청서를 발행했다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r002 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `TitleLandscape` | `PLAN-040-SPR-001` | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-001/r002/TitleLandscape.png` | `Assets/Sprites/UI/Title/TitleLandscape.png` |
| `TitlePortrait` | `PLAN-040-SPR-002` | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-002/r002/TitlePortrait.png` | `Assets/Sprites/UI/Title/TitlePortrait.png` |
| `LoadingDecoration` | `PLAN-040-SPR-003` | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-003/r002/LoadingDecoration.png` | `Assets/Sprites/UI/Loading/LoadingDecoration.png` |

수령 PNG는 GUID/참조를 보존해 최종 경로로 옮기거나 교체한다. Require는 장기 보관/런타임 참조 위치가 아니며 과거 파일은 Art 리비전에 보존한다. 기존 완료 에셋을 재사용할 때는 원본/요청을 복제하지 않는다.

### 검수 자료

원본 미리보기·리비전 인계 목록은 이 PLAN/Art 지정 위치에 보관한다. 정식 검증·게임 캡처·환경 기록은 아래 `../PLAN-042/` 경로에 한 번만 저장하고 이 PLAN에서 링크한다. 이전에 지정한 개별 게임 실행 폴더는 생성하지 않는다.

이 PLAN에서 제출받아 비교하는 원본 미리보기와 Unity 적용 검수는 E 아래에서 구분한다. Art 자체 제작 PLAN의 기존 검수 증거가 있으면 그 실제 경로를 연결하고 같은 자료를 불필요하게 복사하지 않는다.

| 자료 | E 기준 저장 경로 |
| --- | --- |
| 전체 검수 색인·최신 적용/승인 대응 | `README.md` |
| 에셋별 실제 파일·리비전·GUID/해시·사용처 목록 | `asset-manifest.csv` |
| 기술 검증·원본/게임 화면 사용자 응답·날짜 | `../PLAN-042/validation.md` |
| 작업 전 상태와 원본/참조·저장/설정·Editor 복원 | `../PLAN-042/environment.md` |
| 첫 실행의 조건·언어·화면/배율·대상 리비전 | `../PLAN-042/runs/run-001/README.md` |
| 첫 실행의 가로/세로 대표 게임 화면 | `../PLAN-042/runs/run-001/screenshots/PLAN-040-landscape.png`, `../PLAN-042/runs/run-001/screenshots/PLAN-040-portrait.png` |
| 필요한 경우 실제 재생 검수 영상 | `../PLAN-042/runs/run-001/videos/game-flow.mp4` |
| 실행한 검증 결과·로그 | `../PLAN-042/runs/run-001/tests/editmode.xml`, `../PLAN-042/runs/run-001/tests/playmode.xml`, `../PLAN-042/runs/run-001/logs/editor.log` |

실행하지 않는 검증의 파일은 만들지 않고 validation.md에 해당 없음/미실행을 기록한다. 다음 실행은 run-002부터 증가하여 이전 증거를 덮어쓰지 않는다. 장면/언어별 추가 캡처는 해당 실행 README에 파일명과 조건을 등록한다.

| 제출받은 원본 비교 대상 | E 기준 리비전별 시각 비교 | 비교 조건·원본 승인 기록 |
| --- | --- | --- |
| TitleLandscape | `source-reviews/TitleLandscape/r002/design.png` | `source-reviews/TitleLandscape/r002/README.md` |
| TitlePortrait | `source-reviews/TitlePortrait/r002/design.png` | `source-reviews/TitlePortrait/r002/README.md` |
| LoadingDecoration | `source-reviews/LoadingDecoration/r002/design.png` | `source-reviews/LoadingDecoration/r002/README.md` |

## 작업 책임과 문서 경계

- 2026-11-16 사용자 요청으로 제작 우선·PLAN-042 일괄 적용/검증 방식으로 보완했다. 후속 사용자 요청으로 원본·제작 미리보기를 준비하고 Unity 적용/정식 검증은 유보했다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 독립3종의 Art PLAN-028~030을 작성했으며 완료 문서는 수정하지 않는다.
- 신규 요청3종 명세를 발행했다. `UnityProject/Assets/Sprites/Require/PLAN-040/README.md`에 요청별 명세·최종 경로·상태를 기록한다. 위 표의 예정 요청 ID·파일 위치를 사용하며 경로 지정은 실제 수령을 뜻하지 않는다.

## 리소스 준비와 진행 요약

승인본 수령·Unity 적용·일반 Editor 기술 검증·사용자 실제 게임 화면 승인 완료(2026-11-17). 리비전/수령/최종 경로는 [최종 대응표](evidence/PLAN-042/asset-manifest.csv), 개별 완료 기준은 [49개 기준 대응](evidence/PLAN-042/plan-verification-matrix.csv)에 기록했다. 기존 승인본과 저장/설정을 보존했다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [x] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 에셋별 원본·리비전·수령·최종 적용·검수 자료의 예정 경로 지정
- [x] 수량·사용처·규격 확정, 상세 요청/Art 제작 기록 연결
- [x] 전체 필수 Art 원본·리비전 PNG·제출 명세·미리보기 제작
- [x] Art 파일 경로·요청 ID·리비전 인계 목록 작성, 제작 종료 후 on hold 전환
- [x] 사용자에게 원본 미리보기·재생 GIF 제시 → 피드백 반영 → 해당 묶음의 모든 에셋 리비전 확인 기록(다음 제작 전 필수)
- [x] PLAN-042: 원본/출력 기술 검증 후 Unity 일괄 수령·정규 경로·씬/SO·생성 도구 연결
- [x] PLAN-042: 전체 적용 후 일괄 기술 검증·원본/게임 화면 사용자 확인
- [x] PLAN-042: 필요한 수정본·재검증·환경 복원·완료 범위 증거 연결 후 completed 판정

## PLAN-042에서 확인할 완료 기준과 검증 방법

다음 항목의 검증 실행 주체는 PLAN-042다. 원본·출력·알파·시간·좌표 등 리소스 기술 검증도 마지막 단계에서 모아 수행하며, 제작 중 저장/미리보기 확인 및 제작 직후 사용자 원본 확인과 구분한다.

| 기준 | 실행할 검증·기대 결과 |
| --- | --- |
| 디자인 일치 | 원하는 실루엣·구성·재질·명암·금지 요소를 실제 최소 표시 크기에서 대조, 사용자 확인 |
| 제작/참조 | 원본·출력·알파·셀/시간·기준점·임포트/플랫폼 override·누락 Sprite와 전체 사용처 확인 |
| 크롭 | 가로/세로·긴 화면·안전 영역에서 핵심 장면과 메뉴/제목 대비 유지 |
| UI | ko/en·큰 글씨·이어하기 유무·오류/재시도·진행률/기존 웨이브 유지 |
| 씬 | Title→Loading→Laboratory·홈 복귀·직접 진입에서 중복 장식/참조 유실 없음 |
| 보존/재생성 | 기존 승인본 해시/GUID·하위 Sprite 참조 유지, 씬/정의 생성 도구 후 정식 연결 보존 |
| 완료 판정 | 전체 필수 대상 적용·기술 검증·사용자 게임 화면 승인과 환경 복원 완료 |

- 기본 검증은 일반 Editor 범위다. Player 컴파일·빌드·실기기·고부하 계측은 현재 명시적 요청이 없어 기본 실행/필수 완료 조건에서 제외한다. 요청 시 범위와 승인 근거를 기록한다.
- 연결 코드 변경 전 [추상화](../agents/abstraction.md)·[객체 설계](../agents/object-design.md)·[Unity 에셋](../agents/unity-assets.md)·[Localization](../agents/localization.md)의 해당 경계를 읽고 최소 변경만 수행한다.
- 원본/리비전·GUID·저장/설정·Editor 실행 상태를 작업 전 기록하고 종료 시 복원한다. 원본을 자동 리사이즈/재색칠해 규격에 맞추지 않는다.

## 검증 결과와 사용자 확인

- 2026-11-16: 문서·현행 코드/정의의 정적 조사로 후보 작성. 신규 리소스 제작·수령·게임 적용·게임 테스트는 미실행.
- 최신 원본 승인: r002 사용자 확인 완료(2026-11-16). 게임 화면 승인은 PLAN-042 대기. 실행 시 대상 리비전·화면/배율·사용자 응답/날짜·수정/재확인을 각각 기록한다.
- 문서 검사: 2026-11-16 문서 검사 통과(PLAN 42개·active 0개·목록 42개·로컬 링크 3,888개). 변경 형식 검사 통과. 기존 Unity/Art PLAN 40개가 작업 전 해시와 같음을 확인했다.
- 경로 보완 검사(2026-11-16): 문서 검사 통과(42 PLAN·active 0·로컬 링크 3,904개), 25개 제작 후보의 원본/제출/수령/최종 적용/원본 비교 경로 누락·중복 없음, 기존 완료 PLAN 40개 해시 불변 확인. 예정 결과물은 생성하지 않았다.
- 환경 복원: 문서만 작성하여 게임·Editor·원본 아트·저장/설정을 변경하지 않았다.

## 현재 상황과 다음 행동

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

- 2026-11-16: 사용자 요청에 따라 제작 선행, Unity 적용·정식 검증은 PLAN-042로 통합. 미제작/proposed 유지. 제작 후 on hold 및 최종 증거에 의한 completed 전환을 구분했다.

- 2026-11-16: 사용자 「작업이 끝나고 내가 확인하는 과정도 필요해」 반영. 각 제작 묶음 직후 미리보기/재생 제시와 원본 확인을 필수로 두며, 응답 전 다음 제작으로 넘어가지 않는다. Unity 적용·정식 통합 검증은 PLAN-042에 유지한다.

- 2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-040 아트 작업을 진행해줘」에 따라 proposed → active. 글자/로고 없는 배경2종·장식1종 제작 위임.

## r001 제작 결과 — 2026-11-16

[인계·미리보기](evidence/PLAN-040/README.md), [파일·해시·리비전·경로](evidence/PLAN-040/asset-manifest.csv), [요청 기록](../../UnityProject/Assets/Sprites/Require/PLAN-040/README.md).
TitleLandscape640×360·TitlePortrait360×640·LoadingDecoration64×64, 각 원본/PNG1·정적1프레임·0태그·3레이어.
지하 배양조·작업대·작업등 모티프로 새로 그렸으며 기존 승인 원본을 재색칠하지 않았다. 배경2종은 불투명, 장식은 투명. 모든 글자/로고는 제외했다.
배경은 전체SafeRoot에 중심 Contain, 남는영역 #0D1828인 안으로 확정했다. 현재 TitleView의 독립 그림Rect를 전체배경Rect로 연결하는 작업은 PLAN-042에 명시했다. 제목/메뉴/TMP·저장/오류 흐름은 유지한다.
Loading 장식은 문구위64이하·간격12, 공간부족시32까지 축소 후 숨김 가능, Error/Confirmation에서는 숨김. 실제Rect연결/검증은 마지막에 진행한다.
원본 보기·글자 없는 공용버튼 배치·화면비 합성을 보존했다. 제작 미리보기이며 실제게임/정식 검증 결과가 아니다.
Unity PNG 복사·임포트·코드/씬/SO/프리팹·Edit/Play·빌드·게임캡처는 수행하지 않았다.

- 2026-11-16: active → on hold. Art3종 r001 제작·인계 준비, 원본 확인 대기. Unity 일괄 적용/정식 검증은 PLAN-042.

- 제작 종료 문서 검사(2026-11-16): Unity43 PLAN·active0·목록43·로컬 링크4,176 통과, git diff --check 통과. Art30 PLAN·active0·목록30·링크899 검사, 기존 완료 PLAN 링크 오류9건만 유지. 신규 문서 링크 오류 없음. 완료 PLAN은 수정하지 않았고 Art 검사 전체 통과로 기록하지 않는다. 정식 기술/게임 검증은 미실행.

- 2026-11-16: on hold → active. 사용자 타이틀 기포·작은 생명체 움직임 및 로딩 반복 기포 r002 제작 요청. Unity 적용·정식 검증은 PLAN-042로 유보.

## r002 제작 결과 — 2026-11-16

사용자 기포·작은 생명체 움직임 요청을 F-001로 반영했다. Art PLAN-028~030을 순차 활성화해 3종 각16프레임·150ms·2.4초 반복, PNG 시트/원본/GIF를 보존했다. r001은 유지했다. [제출 명세·GIF](evidence/PLAN-040/README.md). 원본 승인 대기, Unity 미수령·미적용, 정식 검증 PLAN-042 유보.

- 2026-11-16: active → on hold. 사용자 타이틀 기포·작은 생명체 움직임 및 로딩 반복 기포 r002 제작 요청. Unity 적용·정식 검증은 PLAN-042로 유보.

- r002 제작 종료 문서 검사(2026-11-16): Unity43 PLAN·active0·목록43·로컬 링크4,175 통과, git diff --check 통과. Art30 PLAN·active0·목록30·링크920 검사, 기존 완료 PLAN 링크 오류9건만 유지. 이번 변경의 신규 링크 오류 없음. 정식 기술/게임 검증은 PLAN-042까지 미실행.

- 2026-11-16 사용자 「좋아. r002로 진행하자. 확인 완료로 기록하고 다음 PLAN-041 아트 작업을 진행해줘」에 따라 타이틀/로딩3종 r002 외형·동작 원본 확인 완료. Unity 수령/적용·정식 기술/게임 검증은 PLAN-042 대기이며 completed 판정은 아님.

## 최종 완료 — 2026-11-17

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

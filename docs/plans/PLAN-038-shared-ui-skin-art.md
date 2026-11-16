---
id: PLAN-038
type: art
status: completed
title: 모바일 HUD·공용 UI 프레임과 게이지 아트 적용
updated: 2026-11-17
---

# PLAN-038 모바일 HUD·공용 UI 프레임과 게이지 아트 적용

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
- 2026-11-16 사용자 PLAN-038 아트 제작 요청에 따라 원본 제작을 실행한다. Unity 적용·정식 검증은 PLAN-042로 유보한다.

## 목표와 범위

HUD·시설/생명체/침공 목록·상세·설정/확인/알림 팝업에 얇고 일관된 외형을 적용한다. 공용 프레임·버튼·스크롤·선택 상태·멸망 게이지를 포함한다.
시설 위 작은 진행 바는 PLAN-032 완성본을 재사용한다. 카드·아이콘·퍼센트·ON/OFF 조작을 복원하지 않는다. 메뉴 아이콘은 PLAN-037, 타이틀/결과 그림은 PLAN-040/041이다.

## 기준 문서와 현재 확정 사항

- [화면/조작](../game-design/03-player-experience/03-interface/controls.md), [Popup](../../UnityProject/Assets/Scripts/Presentation/Popup/README.md).
- [UiWidgets](../../UnityProject/Assets/Scripts/Presentation/UI/Widgets/UiWidgets.cs), [UiButton](../../UnityProject/Assets/Scripts/Presentation/UI/Widgets/UiButton.cs), [연구소 UI](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs).
- 선행: [PLAN-037](PLAN-037-laboratory-menu-icons-art.md)의 아이콘 선/색/무게. 다른 미완료 PLAN 리소스를 이 PLAN의 필수로 끌어오지 않는다.

## 디자인 의도와 원하는 모습

낮은 대비의 어두운 판에 얇은 금속 테두리와 작은 모서리 절삭만 남긴다. 텍스트·수치·주요 버튼이 먼저 보이고 장식은 가장자리에 한정한다. 회청색 판·중성 외곽·청록 선택선을 시작안으로 하되 색 이외에 선/명도 차이로 상태를 구분한다.
팝업은 본문 여백을 넉넉히 두고 헤더·볼트·반짝임을 줄인다. Build/메뉴 아이콘의 선 무게는 가져오되 건물의 복잡한 금속 패턴·월드 입체 명암은 사용하지 않는다.
멸망 게이지는 가는 트랙과 단색 채움으로 누적량을 먼저 읽게 한다. 장식 점멸은 넣지 않는다. 정확한 팔레트·선·모서리 크기는 작은 화면 비교로 확정한다.

## UI 산출물과 표시 명세

| 세트 | 제안 출력 | 크기 조절 |
| --- | --- | --- |
| PanelFrame/ButtonFrame/SelectionFrame/ScrollThumb | 독립 원본/PNG 각 4개(제작 완료) | 9-slice·고정 모서리/테두리 |
| HudFrame/DestructionTrack | 독립 원본/PNG 각 2개 | HUD/Track 9-slice, Fill은 기존 단색 Image 재사용 |

제작 확정: 독립 PNG6개(프레임32×32 5종·게이지 Track16×16 1종). DestructionFill은 기존 단색 Image를 재사용하여 신규 원본/PNG를 만들지 않는다. border/padding/최소 Rect는 요청 README와 Art PLAN-020~025의 명세를 따른다. 텍스트·터치 영역을 그림 크기에 맞춰 줄이지 않는다.

| 상태 | 원하는 모습 | 수단 |
| --- | --- | --- |
| 기본 | 낮은 대비 판·얇은 테두리 | 공용 Sprite |
| 선택/포커스 | 얇은 안쪽 강조선 | 기존 상태+SelectionFrame |
| 눌림 | 기존 Tint로 판과 선을 함께 어둡게 표시, 라벨은 별도 | Tint/기존 전환 |
| 비활성 | 낮은 대비·기존 라벨 | Tint+interactable |
| 팝업 | 본문은 안정적 불투명 판 | 기존 PopupHost·배경 입력 차단 |

## PLAN-042에서 실행할 사용처별 적용 계획

아래 경로 표의 신규6개 PNG와 기존 단색 Fill을 프로젝트 소유의 최소 스타일 참조로 묶어 공용 위젯/PopupHost에 연결한다. 여러 실제 화면이 같은 프레임을 공유하는 요구에만 데이터를 추가하고 전역 Theme 프레임워크로 확장하지 않는다.
HUD·목록·상세·팝업의 기존 색/배경 예외를 적용표로 확정한다. 타이틀/결과는 공용 버튼을 재사용하되 전용 그림·배치 명세는 PLAN-040/041에서 제작하고 실제 연결은 PLAN-042가 담당한다. 안전 영역·스크롤·Localization·입력 차단을 보존한다.

## 결과물 저장 위치

아래 중 신규6종의 Art 원본·리비전 파일은 제작·보존 완료다. Unity 수령·최종 적용은 예정이며 파일을 복사하지 않았다. DestructionFill 행은 기존 구현 재사용과 미생성 사유를 기록한다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-038/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `PanelFrame` | `assets/ui/PanelFrame/source/PanelFrame.aseprite` | `assets/ui/PanelFrame/revisions/r001/source/PanelFrame.aseprite` | `assets/ui/PanelFrame/revisions/r001/export/PanelFrame.png` | `assets/ui/PanelFrame/revisions/r001/README.md` |
| `ButtonFrame` | `assets/ui/ButtonFrame/source/ButtonFrame.aseprite` | `assets/ui/ButtonFrame/revisions/r001/source/ButtonFrame.aseprite` | `assets/ui/ButtonFrame/revisions/r001/export/ButtonFrame.png` | `assets/ui/ButtonFrame/revisions/r001/README.md` |
| `SelectionFrame` | `assets/ui/SelectionFrame/source/SelectionFrame.aseprite` | `assets/ui/SelectionFrame/revisions/r001/source/SelectionFrame.aseprite` | `assets/ui/SelectionFrame/revisions/r001/export/SelectionFrame.png` | `assets/ui/SelectionFrame/revisions/r001/README.md` |
| `ScrollThumb` | `assets/ui/ScrollThumb/source/ScrollThumb.aseprite` | `assets/ui/ScrollThumb/revisions/r001/source/ScrollThumb.aseprite` | `assets/ui/ScrollThumb/revisions/r001/export/ScrollThumb.png` | `assets/ui/ScrollThumb/revisions/r001/README.md` |
| `HudFrame` | `assets/ui/HudFrame/source/HudFrame.aseprite` | `assets/ui/HudFrame/revisions/r001/source/HudFrame.aseprite` | `assets/ui/HudFrame/revisions/r001/export/HudFrame.png` | `assets/ui/HudFrame/revisions/r001/README.md` |
| `DestructionTrack` | `assets/ui/DestructionTrack/source/DestructionTrack.aseprite` | `assets/ui/DestructionTrack/revisions/r001/source/DestructionTrack.aseprite` | `assets/ui/DestructionTrack/revisions/r001/export/DestructionTrack.png` | `assets/ui/DestructionTrack/revisions/r001/README.md` |
| `DestructionFill` | 기존 `UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs`의 Progress Image | 미생성: 기존 단색 Image 재사용 | 미생성: Sprite 없는 사각형 | [재사용 계약](../../UnityProject/Assets/Sprites/Require/PLAN-038/README.md#destructionfill-재사용) |

### Unity 수령과 최종 적용

아래 경로는 PLAN-042에서 일괄 수령·적용할 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-038/README.md`에 둔다. 아래 요청 ID로 상세 요청서를 발행했다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `PanelFrame` | `PLAN-038-SPR-001` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-001/r001/PanelFrame.png` | `Assets/Sprites/UI/Frames/PanelFrame.png` |
| `ButtonFrame` | `PLAN-038-SPR-002` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-002/r001/ButtonFrame.png` | `Assets/Sprites/UI/Frames/ButtonFrame.png` |
| `SelectionFrame` | `PLAN-038-SPR-003` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-003/r001/SelectionFrame.png` | `Assets/Sprites/UI/Frames/SelectionFrame.png` |
| `ScrollThumb` | `PLAN-038-SPR-004` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-004/r001/ScrollThumb.png` | `Assets/Sprites/UI/Frames/ScrollThumb.png` |
| `HudFrame` | `PLAN-038-SPR-005` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-005/r001/HudFrame.png` | `Assets/Sprites/UI/Frames/HudFrame.png` |
| `DestructionTrack` | `PLAN-038-SPR-006` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-006/r001/DestructionTrack.png` | `Assets/Sprites/UI/Frames/DestructionTrack.png` |
| `DestructionFill` | `PLAN-038-SPR-007` | 미수령: 신규 PNG 없음 | 기존 `Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs`의 Sprite 없는 Progress Image 재사용 |

수령 PNG는 GUID/참조를 보존해 최종 경로로 옮기거나 교체한다. Require는 장기 보관/런타임 참조 위치가 아니며 과거 파일은 Art 리비전에 보존한다. 기존 완료 에셋을 재사용할 때는 원본/요청을 복제하지 않는다.

DestructionFill의 기존 도형/리소스 재사용을 채택하면 해당 신규 파일은 만들지 않는다. 표에 재사용 원본의 실제 경로와 미생성 사유를 기록하며, 위 경로는 신규 제작을 선택했을 때의 목적지다.

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
| 첫 실행의 가로/세로 대표 게임 화면 | `../PLAN-042/runs/run-001/screenshots/PLAN-038-landscape.png`, `../PLAN-042/runs/run-001/screenshots/PLAN-038-portrait.png` |
| 필요한 경우 실제 재생 검수 영상 | `../PLAN-042/runs/run-001/videos/game-flow.mp4` |
| 실행한 검증 결과·로그 | `../PLAN-042/runs/run-001/tests/editmode.xml`, `../PLAN-042/runs/run-001/tests/playmode.xml`, `../PLAN-042/runs/run-001/logs/editor.log` |

실행하지 않는 검증의 파일은 만들지 않고 validation.md에 해당 없음/미실행을 기록한다. 다음 실행은 run-002부터 증가하여 이전 증거를 덮어쓰지 않는다. 장면/언어별 추가 캡처는 해당 실행 README에 파일명과 조건을 등록한다.

| 제출받은 원본 비교 대상 | E 기준 리비전별 시각 비교 | 비교 조건·원본 승인 기록 |
| --- | --- | --- |
| PanelFrame | `source-reviews/PanelFrame/r001/design.png` | `source-reviews/PanelFrame/r001/README.md` |
| ButtonFrame | `source-reviews/ButtonFrame/r001/design.png` | `source-reviews/ButtonFrame/r001/README.md` |
| SelectionFrame | `source-reviews/SelectionFrame/r001/design.png` | `source-reviews/SelectionFrame/r001/README.md` |
| ScrollThumb | `source-reviews/ScrollThumb/r001/design.png` | `source-reviews/ScrollThumb/r001/README.md` |
| HudFrame | `source-reviews/HudFrame/r001/design.png` | `source-reviews/HudFrame/r001/README.md` |
| DestructionTrack | `source-reviews/DestructionTrack/r001/design.png` | `source-reviews/DestructionTrack/r001/README.md` |
| DestructionFill 재사용 | Art `docs/plans/reviews/PLAN-025/r001/overview-no-text.png`, `slice-study-no-text.png`에 기존 단색 합성 | 신규 PNG 없음, 묶음 조합 확인 대기 |

## 작업 책임과 문서 경계

- 2026-11-16 사용자 요청으로 제작 우선·PLAN-042 일괄 적용/검증 방식으로 보완했다. 이번 사용자 요청으로 Art 원본 제작을 실행하며 적용·테스트는 유보한다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 독립6종의 Art PLAN-020~025를 신규 작성했다. 완료 문서는 수정하지 않는다.
- 신규 요청001~006 명세 발행, 007은 기존 단색 Image 재사용. `UnityProject/Assets/Sprites/Require/PLAN-038/README.md`에 요청별 명세·최종 경로·상태를 기록한다. 위 표의 예정 요청 ID·파일 위치를 사용하며 경로 지정은 실제 수령을 뜻하지 않는다.

## 리소스 준비와 진행 요약

승인본 수령·Unity 적용·일반 Editor 기술 검증·사용자 실제 게임 화면 승인 완료(2026-11-17). 리비전/수령/최종 경로는 [최종 대응표](evidence/PLAN-042/asset-manifest.csv), 개별 완료 기준은 [49개 기준 대응](evidence/PLAN-042/plan-verification-matrix.csv)에 기록했다. 기존 승인본과 저장/설정을 보존했다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [x] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 에셋별 원본·리비전·수령·최종 적용·검수 자료의 예정 경로 지정
- [x] 수량·사용처·규격 확정, 상세 요청/Art 제작 기록 연결
- [x] 전체 필수 Art 원본·리비전 PNG·제출 명세·미리보기 제작
- [x] Art 파일 경로·요청 ID·리비전 인계 목록 작성, 제작 종료 후 on hold 전환
- [x] 글자 없는 미리보기 제시 및 r001 원본6종·Fill 재사용 확인 완료(정적, GIF 해당 없음)
- [x] PLAN-042: 원본/출력 기술 검증 후 Unity 일괄 수령·정규 경로·씬/SO·생성 도구 연결
- [x] PLAN-042: 전체 적용 후 일괄 기술 검증·원본/게임 화면 사용자 확인
- [x] PLAN-042: 필요한 수정본·재검증·환경 복원·완료 범위 증거 연결 후 completed 판정

## PLAN-042에서 확인할 완료 기준과 검증 방법

다음 항목의 검증 실행 주체는 PLAN-042다. 원본·출력·알파·시간·좌표 등 리소스 기술 검증도 마지막 단계에서 모아 수행하며, 제작 중 저장/미리보기 확인 및 제작 직후 사용자 원본 확인과 구분한다.

| 기준 | 실행할 검증·기대 결과 |
| --- | --- |
| 디자인 일치 | 원하는 실루엣·구성·재질·명암·금지 요소를 실제 최소 표시 크기에서 대조, 사용자 확인 |
| 제작/참조 | 원본·출력·알파·셀/시간·기준점·임포트/플랫폼 override·누락 Sprite와 전체 사용처 확인 |
| 반응형 | 가로/세로·ko/en·큰 글씨에서 9-slice 왜곡/잘림/터치 영역 축소 없음 |
| 공용 적용 | HUD·목록·상세·설정/확인/알림과 재진입/팝업 재사용 참조 대조 |
| 단순화 | 시설 진행 바 크기/비조작 유지·멸망 게이지 값 일치·팝업 뒤 입력 차단 |
| 보존/재생성 | 기존 승인본 해시/GUID·하위 Sprite 참조 유지, 씬/정의 생성 도구 후 정식 연결 보존 |
| 완료 판정 | 전체 필수 대상 적용·기술 검증·사용자 게임 화면 승인과 환경 복원 완료 |

- 기본 검증은 일반 Editor 범위다. Player 컴파일·빌드·실기기·고부하 계측은 현재 명시적 요청이 없어 기본 실행/필수 완료 조건에서 제외한다. 요청 시 범위와 승인 근거를 기록한다.
- 연결 코드 변경 전 [추상화](../agents/abstraction.md)·[객체 설계](../agents/object-design.md)·[Unity 에셋](../agents/unity-assets.md)·[Localization](../agents/localization.md)의 해당 경계를 읽고 최소 변경만 수행한다.
- 원본/리비전·GUID·저장/설정·Editor 실행 상태를 작업 전 기록하고 종료 시 복원한다. 원본을 자동 리사이즈/재색칠해 규격에 맞추지 않는다.

## 검증 결과와 사용자 확인

- 2026-11-16: 문서·현행 코드/정의의 정적 조사로 후보 작성. 신규 리소스 제작·수령·게임 적용·게임 테스트는 미실행.
- 원본 승인: 글자 없는6종 r001과 Fill 재사용 확인 완료(2026-11-16). 게임 화면 승인·정식 검증은 PLAN-042 대기.
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

- 2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-038 아트 작업을 진행해줘」에 따라 proposed → active. Art 원본·미리보기 제작만 실행하며 적용·정식 검증은 마지막으로 유보한다.

- 제작 명세: [PLAN-038 요청](../../UnityProject/Assets/Sprites/Require/PLAN-038/README.md). DestructionFill 원본은 LaboratoryView.Gui.cs의 Sprite 없는 Progress Image(색 RGB .85/.3/.2)이며 신규 파일 없음. 6단위 트랙의 안쪽1px 맞춤과 실제9-slice border scale은 PLAN-042에서 적용·검증한다.

## r001 제작 결과 — 2026-11-16

[제작 인계·확인 자료](evidence/PLAN-038/README.md), [파일·해시·리비전·경로 목록](evidence/PLAN-038/asset-manifest.csv).
PanelFrame/ButtonFrame/SelectionFrame/ScrollThumb/HudFrame는32×32, DestructionTrack은16×16. 각각 정적1프레임·3레이어·원본/PNG1개, 총6종.
DestructionFill은 LaboratoryView.Gui.cs의 기존 Sprite 없는 Image를 재사용한다. 신규 원본/PNG 없음, 요청007은 다른 에셋에 재배정하지 않는다.
Art PLAN-020~025를 각각 proposed → active → on hold로 순차 제작했다. 원본 확대와9-slice 조합/상태 미리보기를 저장·열어 제작 확인했다.
이 확인은 정식 파일/게임 검증이 아니다. r001 사용자 외형 확인 완료이며 approved_revision은 모두1이다.
Unity 파일 복사·임포트·코드/씬/SO/프리팹·Edit/Play·게임 캡처·빌드·정식 검증은 수행하지 않았다. 시설 위 작은 진행 바·기존 승인 아트는 유지했다.

- 2026-11-16: active → on hold. 원본6종·명세·미리보기·인계 목록 준비, 사용자 확인 및 PLAN-042 일괄 적용·정식 검증 대기.

- 2026-11-16 제작 종료 문서 검사: Unity43 PLAN·active0·목록43·링크4,142 통과(이후 재사용 계약 링크 추가), git diff --check 통과. Art25 PLAN·active0·목록25·링크773 검사, 기존 완료 PLAN 링크 오류9건만 유지. 정식 기술/게임 검증은 미실행.

- 2026-11-16: 사용자 프레임/배경만 유지·모든 글자 제거 요청으로 미리보기 보완 재개(on hold → active). 원본6종의 그림은 변경하지 않으며 설명 글자/예시 아이콘 없는 보조 미리보기를 준비한다.

2026-11-16 F-001: 사용자 「UI는 프레임과 배경 등만, 글자는 모두 제거; TextMeshProUGUI로 관리」 요청 반영. UI 원본/PNG6종에는 처음부터 글자가 없었으며 원본 픽셀은 변경하지 않았다. 글자는 이전 확인용 합성에만 있었고, 최신 미리보기에서는 설명 글자·숫자·예시 아이콘까지 모두 제외했다. 모든 제품 텍스트는 Unity TextMeshProUGUI와 기존 Localization으로 관리한다. 원본 미변경으로 r001 유지, 기존 제출 미리보기는 이력으로 보존한다.

- active → on hold: 글자 없는 보조 미리보기 작성·외형 확인 대기. Unity 적용·정식 검증은 PLAN-042 미실행.

- 2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-039 아트 작업을 진행해줘」. 6종 r001 원본과 기존 Fill 재사용 확인 완료. Unity 미적용·정식 검증 미실행으로 on hold 유지.

## 최종 완료 — 2026-11-17

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

---
id: PLAN-038
type: art
status: proposed
title: 모바일 HUD·공용 UI 프레임과 게이지 아트 적용
updated: 2026-11-16
---

# PLAN-038 모바일 HUD·공용 UI 프레임과 게이지 아트 적용

[사용 양식](_template-art-ui.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
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
| PanelFrame/ButtonFrame/SelectionFrame/ScrollThumb | 독립 원본/PNG 각 4개 | 9-slice·고정 모서리/테두리 |
| HudFrame/DestructionTrack/DestructionFill | 독립 원본/PNG 각 3개 | HUD/Track 9-slice, Fill 기존 채움 |

총 7 PNG 제안. 단색 Fill은 기존 도형으로 충분하면 제작 전 재사용으로 확정한다. 프레임 32×32·게이지 16×16 캔버스를 제안하며 border/padding/최소 Rect는 실제 UI 치수로 확정한다. 텍스트·터치 영역을 그림 크기에 맞춰 줄이지 않는다.

| 상태 | 원하는 모습 | 수단 |
| --- | --- | --- |
| 기본 | 낮은 대비 판·얇은 테두리 | 공용 Sprite |
| 선택/포커스 | 얇은 안쪽 강조선 | 기존 상태+SelectionFrame |
| 눌림 | 내부 면만 약하게 어두워짐 | Tint/기존 전환 |
| 비활성 | 낮은 대비·기존 라벨 | Tint+interactable |
| 팝업 | 본문은 안정적 불투명 판 | 기존 PopupHost·배경 입력 차단 |

## 사용처별 적용 계획

아래 경로 표에 지정한 7개 PNG를 프로젝트 소유의 최소 스타일 참조로 묶어 공용 위젯/PopupHost에 연결한다. 여러 실제 화면이 같은 프레임을 공유하는 요구에만 데이터를 추가하고 전역 Theme 프레임워크로 확장하지 않는다.
HUD·목록·상세·팝업의 기존 색/배경 예외를 적용표로 확정한다. 타이틀/결과는 공용 버튼을 재사용하되 전용 그림과 배치는 후속 PLAN이 담당한다. 안전 영역·스크롤·Localization·입력 차단을 보존한다.

## 결과물 저장 위치

아래는 이번 PLAN에서 사용할 **예정 경로**다. 위치 지정은 제작·수령 완료를 뜻하지 않으며 파일/빈 폴더는 지금 만들지 않는다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

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
| `DestructionFill` | `assets/ui/DestructionFill/source/DestructionFill.aseprite` | `assets/ui/DestructionFill/revisions/r001/source/DestructionFill.aseprite` | `assets/ui/DestructionFill/revisions/r001/export/DestructionFill.png` | `assets/ui/DestructionFill/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-038/README.md`에 둔다. 아래 ID는 이 PLAN의 예정 요청 ID로 배정했으며 상세 요청서는 아직 미발행이다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `PanelFrame` | `PLAN-038-SPR-001` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-001/r001/PanelFrame.png` | `Assets/Sprites/UI/Frames/PanelFrame.png` |
| `ButtonFrame` | `PLAN-038-SPR-002` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-002/r001/ButtonFrame.png` | `Assets/Sprites/UI/Frames/ButtonFrame.png` |
| `SelectionFrame` | `PLAN-038-SPR-003` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-003/r001/SelectionFrame.png` | `Assets/Sprites/UI/Frames/SelectionFrame.png` |
| `ScrollThumb` | `PLAN-038-SPR-004` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-004/r001/ScrollThumb.png` | `Assets/Sprites/UI/Frames/ScrollThumb.png` |
| `HudFrame` | `PLAN-038-SPR-005` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-005/r001/HudFrame.png` | `Assets/Sprites/UI/Frames/HudFrame.png` |
| `DestructionTrack` | `PLAN-038-SPR-006` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-006/r001/DestructionTrack.png` | `Assets/Sprites/UI/Frames/DestructionTrack.png` |
| `DestructionFill` | `PLAN-038-SPR-007` | `Assets/Sprites/Require/PLAN-038/PLAN-038-SPR-007/r001/DestructionFill.png` | `Assets/Sprites/UI/Frames/DestructionFill.png` |

수령 PNG는 GUID/참조를 보존해 최종 경로로 옮기거나 교체한다. Require는 장기 보관/런타임 참조 위치가 아니며 과거 파일은 Art 리비전에 보존한다. 기존 완료 에셋을 재사용할 때는 원본/요청을 복제하지 않는다.

DestructionFill의 기존 도형/리소스 재사용을 채택하면 해당 신규 파일은 만들지 않는다. 표에 재사용 원본의 실제 경로와 미생성 사유를 기록하며, 위 경로는 신규 제작을 선택했을 때의 목적지다.

### 검수 자료

이 PLAN에서 제출받아 비교하는 원본 미리보기와 Unity 적용 검수는 E 아래에서 구분한다. Art 자체 제작 PLAN의 기존 검수 증거가 있으면 그 실제 경로를 연결하고 같은 자료를 불필요하게 복사하지 않는다.

| 자료 | E 기준 저장 경로 |
| --- | --- |
| 전체 검수 색인·최신 적용/승인 대응 | `README.md` |
| 에셋별 실제 파일·리비전·GUID/해시·사용처 목록 | `asset-manifest.csv` |
| 기술 검증·원본/게임 화면 사용자 응답·날짜 | `validation.md` |
| 작업 전 상태와 원본/참조·저장/설정·Editor 복원 | `environment.md` |
| 첫 실행의 조건·언어·화면/배율·대상 리비전 | `runs/run-001/README.md` |
| 첫 실행의 가로/세로 대표 게임 화면 | `runs/run-001/screenshots/landscape.png`, `runs/run-001/screenshots/portrait.png` |
| 필요한 경우 실제 재생 검수 영상 | `runs/run-001/videos/playback.mp4` |
| 실행한 검증 결과·로그 | `runs/run-001/tests/editmode.xml`, `runs/run-001/tests/playmode.xml`, `runs/run-001/logs/editor.log` |

실행하지 않는 검증의 파일은 만들지 않고 validation.md에 해당 없음/미실행을 기록한다. 다음 실행은 run-002부터 증가하여 이전 증거를 덮어쓰지 않는다. 장면/언어별 추가 캡처는 해당 실행 README에 파일명과 조건을 등록한다.

| 제출받은 원본 비교 대상 | E 기준 리비전별 시각 비교 | 비교 조건·원본 승인 기록 |
| --- | --- | --- |
| PanelFrame | `source-reviews/PanelFrame/r001/design.png` | `source-reviews/PanelFrame/r001/README.md` |
| ButtonFrame | `source-reviews/ButtonFrame/r001/design.png` | `source-reviews/ButtonFrame/r001/README.md` |
| SelectionFrame | `source-reviews/SelectionFrame/r001/design.png` | `source-reviews/SelectionFrame/r001/README.md` |
| ScrollThumb | `source-reviews/ScrollThumb/r001/design.png` | `source-reviews/ScrollThumb/r001/README.md` |
| HudFrame | `source-reviews/HudFrame/r001/design.png` | `source-reviews/HudFrame/r001/README.md` |
| DestructionTrack | `source-reviews/DestructionTrack/r001/design.png` | `source-reviews/DestructionTrack/r001/README.md` |
| DestructionFill | `source-reviews/DestructionFill/r001/design.png` | `source-reviews/DestructionFill/r001/README.md` |

## 작업 책임과 문서 경계

- 현재 범위는 2026-11-16 사용자가 요청한 남은 아트 PLAN 작성이다. 제작·Unity 적용·게임 테스트는 미승인/미실행이다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 신규 요청은 미발행이다. 제작 규격 확정 시 `UnityProject/Assets/Sprites/Require/PLAN-038/README.md`에 요청별 명세·최종 경로·상태를 기록한다. 위 표의 예정 요청 ID·파일 위치를 사용하며 경로 지정은 실제 수령을 뜻하지 않는다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| 패널/버튼/선택/스크롤 프레임 세트 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| HUD·멸망 게이지 프레임 세트 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |

- 전체 준비 판정: 미충족. 전체 필수 리소스의 제작·수령·적용·참조 연결을 정적으로 확인한 날짜와 근거를 기록한 뒤 테스트를 시작한다.
- 준비 중에는 파일·임포트·참조 확인만 수행한다. Edit/Play·수동 게임 실행·검수 캡처는 전체 준비 후 진행한다. 원본 승인과 게임 화면 승인을 구분한다.
- 임포트는 [아트 지침](../agents/art-assets.md)의 PPU 32·Point·원본 해상도 유지 기준을 따른다. 좌표·프레임 시간은 PNG와 별도로 기록한다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [ ] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 에셋별 원본·리비전·수령·최종 적용·검수 자료의 예정 경로 지정
- [ ] 수량·사용처·규격 확정, 상세 요청/Art 제작 기록 연결
- [ ] 전체 필수 리소스 제작·수령·원본 승인·리비전 대조
- [ ] 정규 경로·씬/SO·생성 도구의 실제 참조 전체 연결
- [ ] 전체 준비 판정 후 기술 검증·사용자 게임 화면 확인
- [ ] 수정 리비전 반영·환경 복원·채택 규격·로드맵 완료 범위 기록

## 완료 기준과 검증 방법

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
- 원본 승인·게임 화면 승인: 모두 대기. 실행 시 대상 리비전·화면/배율·사용자 응답/날짜·수정/재확인을 각각 기록한다.
- 문서 검사: 2026-11-16 문서 검사 통과(PLAN 42개·active 0개·목록 42개·로컬 링크 3,888개). 변경 형식 검사 통과. 기존 Unity/Art PLAN 40개가 작업 전 해시와 같음을 확인했다.
- 경로 보완 검사(2026-11-16): 문서 검사 통과(42 PLAN·active 0·로컬 링크 3,904개), 25개 제작 후보의 원본/제출/수령/최종 적용/원본 비교 경로 누락·중복 없음, 기존 완료 PLAN 40개 해시 불변 확인. 예정 결과물은 생성하지 않았다.
- 환경 복원: 문서만 작성하여 게임·Editor·원본 아트·저장/설정을 변경하지 않았다.

## 현재 상황과 다음 행동

- status: proposed. active PLAN은 0개이며 이 문서는 실행 승인이 아니다.
- 다음 행동: PLAN-037 기준을 받아 프레임 7종의 실제 사용처·최소 치수·재사용 항목을 확정한다.
- 로드맵: 5-4 공용 UI. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/승인 리비전·채택 규격·기준 화면은 실행 후 기록한다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

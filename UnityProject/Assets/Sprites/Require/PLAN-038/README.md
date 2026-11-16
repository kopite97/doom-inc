# PLAN-038 공용 UI Sprite 요청

요청·제작: [Unity PLAN-038](../../../../../docs/plans/PLAN-038-shared-ui-skin-art.md). 적용·정식 검증: [PLAN-042](../../../../../docs/plans/PLAN-042-art-integration-review.md).
2026-11-16 사용자 제작 위임. 원본6종을 Art에서 순차 제작한다. Unity PNG 수령/임포트/참조 변경은 마지막까지 유보한다.

## 명세와 경로

정적1프레임·태그0·RGB/sRGB 원본·RGBA PNG·중심 피벗·Single/Full Rect. PPU32·Point·Clamp·밉맵OFF·무압축 예정.
border는 L/B/R/T source px이며 PPU/Canvas 배율과 별개다. 실제1논리단위 테두리 환산과 화면 검증은 PLAN-042.
선택선 외 중앙은 불투명 단색, 모서리만 투명. 원본/보존본은 Art에 둔다. 원본PNG의 재색칠 대신 최종 Tint 적용표를 PLAN-042에서 결정한다.

| 요청 | 에셋·Art PLAN | 크기 | border | 최소 Rect | 상태 |
| --- | --- | --- | --- | --- | --- |
| PLAN-038-SPR-001 | [PanelFrame / Art PLAN-020](../../../../../../Art/docs/plans/PLAN-020-panel-frame.md) | 32×32 | 4 all | 9×9 | r001 제작·원본 확인 대기·미수령 |
| PLAN-038-SPR-002 | [ButtonFrame / Art PLAN-021](../../../../../../Art/docs/plans/PLAN-021-button-frame.md) | 32×32 | 3 all | 7×7 | r001 제작·원본 확인 대기·미수령 |
| PLAN-038-SPR-003 | [SelectionFrame / Art PLAN-022](../../../../../../Art/docs/plans/PLAN-022-selection-frame.md) | 32×32 | 3 all | 7×7 | r001 제작·원본 확인 대기·미수령 |
| PLAN-038-SPR-004 | [ScrollThumb / Art PLAN-023](../../../../../../Art/docs/plans/PLAN-023-scroll-thumb.md) | 32×32 | 3 all | 7×7 | r001 제작·원본 확인 대기·미수령 |
| PLAN-038-SPR-005 | [HudFrame / Art PLAN-024](../../../../../../Art/docs/plans/PLAN-024-hud-frame.md) | 32×32 | 4 all | 9×9 | r001 제작·원본 확인 대기·미수령 |
| PLAN-038-SPR-006 | [DestructionTrack / Art PLAN-025](../../../../../../Art/docs/plans/PLAN-025-destruction-track.md) | 16×16 | 1 all | 3×3 | r001 제작·원본 확인 대기·미수령 |
| PLAN-038-SPR-007 | DestructionFill | 기존 Image 사각형 | 해당 없음 | 가변 너비 | 기존 단색 표현 재사용·신규 파일 없음 |

각 에셋의 수령 예정은 `Assets/Sprites/Require/PLAN-038/<요청ID>/r001/<에셋명>.png`, 최종은 `Assets/Sprites/UI/Frames/<에셋명>.png`.
정확한 개별 경로·padding·의도·source는 각 Art PLAN을 따른다.

## DestructionFill 재사용

원본 구현은 [LaboratoryView.Gui.cs](../../../Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs)의 WorldProgress/Progress: WorldProgress/Progress Image 생성과 표시 갱신의 worldGaugeFill 폭 갱신이다.
실제 이미지 파일은 없다. UiWidgets.Panel에서 Sprite 없는 Image 사각형에 RGB(.85,.3,.2)를 지정한다. r001 PNG/Art PLAN을 새로 만들지 않고 요청007은 재사용 전용으로 유지한다.
PLAN-042에서 Track 안쪽1px에 맞춰 채움 Rect만 정렬한다. 기존 Score/TargetScore 비율·0/100% 표시·높이6논리단위·읽기 전용 의미는 유지한다. 해당 코드 변경과 검증은 이번 제작에 포함하지 않는다.

## 사용처와 제외

PanelFrame: 목록/상세/설정/확인/알림. ButtonFrame: 기존 버튼과 아이콘판. SelectionFrame: 기존 UI 선택/포커스 오버레이, 입력 비차단.
ScrollThumb: 스크롤 손잡이의 외형 자원. 현재 UiWidgets.Scroll에는 Scrollbar 연결이 없어 새 조작 기능 도입 없이 사용하는 연결 범위를 PLAN-042에서 명시한다.
HudFrame: 자원·개체 수 배경. DestructionTrack: 현재6단위 높이 멸망 게이지.
기존48단위 터치 최소·버튼 문구 여백8/4·팝업 여백·안전영역·Localization·뒤 입력 차단 유지. 시설 위 작은 진행 바는 변경하지 않는다.

## 현재 상태

6종 r001 원본·보존본·PNG·명세·미리보기 제작 완료, r001 사용자 원본 확인 완료. PNG 미수령·미적용·정식 검증 미실행. 제작 후 사용자 확인 전 PLAN-039를 시작하지 않는다.

- [6종 개요](../../../../../../Art/docs/plans/reviews/PLAN-025/r001/overview-no-text.png), [조합 예시](../../../../../../Art/docs/plans/reviews/PLAN-025/r001/context-no-text.png), [인계 목록](../../../../../docs/plans/evidence/PLAN-038/asset-manifest.csv).

| 에셋 | 보존 명세·원본·PNG | 최신 승인 | 최신 수령·실제 적용 |
| --- | --- | --- | --- |
| PanelFrame | [r001](../../../../../../Art/assets/ui/PanelFrame/revisions/r001/README.md) | r001 확인 완료 | 없음 |
| ButtonFrame | [r001](../../../../../../Art/assets/ui/ButtonFrame/revisions/r001/README.md) | r001 확인 완료 | 없음 |
| SelectionFrame | [r001](../../../../../../Art/assets/ui/SelectionFrame/revisions/r001/README.md) | r001 확인 완료 | 없음 |
| ScrollThumb | [r001](../../../../../../Art/assets/ui/ScrollThumb/revisions/r001/README.md) | r001 확인 완료 | 없음 |
| HudFrame | [r001](../../../../../../Art/assets/ui/HudFrame/revisions/r001/README.md) | r001 확인 완료 | 없음 |
| DestructionTrack | [r001](../../../../../../Art/assets/ui/DestructionTrack/revisions/r001/README.md) | r001 확인 완료 | 없음 |

2026-11-16 F-001: 사용자 「UI는 프레임과 배경 등만, 글자는 모두 제거; TextMeshProUGUI로 관리」 요청 반영. UI 원본/PNG6종에는 처음부터 글자가 없었으며 원본 픽셀은 변경하지 않았다. 글자는 이전 확인용 합성에만 있었고, 최신 미리보기에서는 설명 글자·숫자·예시 아이콘까지 모두 제외했다. 모든 제품 텍스트는 Unity TextMeshProUGUI와 기존 Localization으로 관리한다. 원본 미변경으로 r001 유지, 기존 제출 미리보기는 이력으로 보존한다.

- 2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-039 아트 작업을 진행해줘」. 6종 r001 원본과 기존 Fill 재사용 확인 완료. Unity 미적용·정식 검증 미실행으로 on hold 유지.


## PLAN-042 일괄 수령·적용 — 2026-11-17

[통합 기록](../../../../../docs/plans/evidence/PLAN-042/README.md) · [전체 경로/GUID](../../../../../docs/plans/evidence/PLAN-042/asset-manifest.csv)

| asset_id | 리비전 | 최종 적용 경로 | 상태 |
| --- | --- | --- | --- |
| PanelFrame | r001 | `Assets/Sprites/UI/Frames/PanelFrame.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| ButtonFrame | r001 | `Assets/Sprites/UI/Frames/ButtonFrame.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| SelectionFrame | r001 | `Assets/Sprites/UI/Frames/SelectionFrame.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| ScrollThumb | r001 | `Assets/Sprites/UI/Frames/ScrollThumb.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| HudFrame | r001 | `Assets/Sprites/UI/Frames/HudFrame.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| DestructionTrack | r001 | `Assets/Sprites/UI/Frames/DestructionTrack.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |

수령 후 AssetDatabase.MoveAsset으로 최종 경로에 배치했으므로 Require 아래 PNG가 없는 것은 유실이 아니다. 제출본은 Art의 해당 리비전에 보존한다. 이 기록은 위 제작 당시 미수령/미적용 기록 이후의 최신 상태다.

## 통합 최종 사용자 확인 — 2026-11-17

2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 이 요청의 PLAN-042 적용 리비전과 재사용 대상 모두 실제 게임 화면 확인 완료. 수령·적용·기술 검증·사용자 확인을 충족했다. [통합 완료 기록](../../../../../docs/plans/evidence/PLAN-042/completion.md). 앞선 대기 기록은 당시 이력이다.

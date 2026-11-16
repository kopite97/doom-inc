# PLAN-038 공용 UI 제작 인계

2026-11-16 / [PLAN](../../PLAN-038-shared-ui-skin-art.md) / [요청 기록](../../../../UnityProject/Assets/Sprites/Require/PLAN-038/README.md) / [파일·해시·리비전 목록](asset-manifest.csv).

신규 원본/보존본/PNG6종 r001·명세·미리보기 제작 완료. r001 사용자 원본 확인 완료, Unity 미수령·미적용·정식 기술/게임 검증 미실행.
DestructionFill은 기존 Sprite 없는 단색 Image를 재사용한다. 새 원본/PNG 없음. 시설 위 작은 진행 바도 변경하지 않는다.

| 에셋 | Art PLAN | 제출 명세·원본·PNG |
| --- | --- | --- |
| PanelFrame | [PLAN-020](../../../../../Art/docs/plans/PLAN-020-panel-frame.md) | [r001](../../../../../Art/assets/ui/PanelFrame/revisions/r001/README.md) |
| ButtonFrame | [PLAN-021](../../../../../Art/docs/plans/PLAN-021-button-frame.md) | [r001](../../../../../Art/assets/ui/ButtonFrame/revisions/r001/README.md) |
| SelectionFrame | [PLAN-022](../../../../../Art/docs/plans/PLAN-022-selection-frame.md) | [r001](../../../../../Art/assets/ui/SelectionFrame/revisions/r001/README.md) |
| ScrollThumb | [PLAN-023](../../../../../Art/docs/plans/PLAN-023-scroll-thumb.md) | [r001](../../../../../Art/assets/ui/ScrollThumb/revisions/r001/README.md) |
| HudFrame | [PLAN-024](../../../../../Art/docs/plans/PLAN-024-hud-frame.md) | [r001](../../../../../Art/assets/ui/HudFrame/revisions/r001/README.md) |
| DestructionTrack | [PLAN-025](../../../../../Art/docs/plans/PLAN-025-destruction-track.md) | [r001](../../../../../Art/assets/ui/DestructionTrack/revisions/r001/README.md) |

## 사용자 원본 확인 자료

- [6종 개요](../../../../../Art/docs/plans/reviews/PLAN-025/r001/overview-no-text.png).
- [조합 예시](../../../../../Art/docs/plans/reviews/PLAN-025/r001/context-no-text.png).
- [크기·상태·게이지 예시](../../../../../Art/docs/plans/reviews/PLAN-025/r001/slice-study-no-text.png).
- 원본 테두리1px=합성1px 가정으로9-slice를 구성한 뒤2배 최근접 확대했다. Aseprite Lua로 저장한 제작 미리보기이며 Unity 화면·게임 동작·정식 검증 증거가 아니다.
- 최신 미리보기는 글자·숫자·아이콘 없이 프레임·배경만 표시한다. 제품 텍스트는 TextMeshProUGUI/Localization에서 별도로 관리한다.
- 36px 아이콘판·32px 보조버튼 등은 그림의 표시 예시이며 실제48논리단위 터치 최소와 구분한다. 실제 UI 크기를 이 그림으로 덮어쓰지 않는다.
- 게이지 크기 비교는 왼쪽부터0/50/100% 채움만 표현하며 숫자/퍼센트 문구를 그리지 않는다.
- 사용자 r001 원본 확인 완료. 요청에 따라 PLAN-039 제작 진행, Unity 적용·정식 검증은 PLAN-042 대기.

## PLAN-042에 남긴 적용·검증

6종 수령/최종 경로와 border는 manifest, padding·상태 계약은 각 Art PLAN과 요청 README를 따른다.
1. 원본/보존본/PNG 대응·알파·여백·border/padding·기하학 최소·9-slice 확인.
2. PPU32/Canvas referencePixelsPerUnit/Image pixelsPerUnitMultiplier의 실제 border scale. 기본 이미지색은white 기준으로 두고 기존 진한 배경 Tint가 중복 곱해지지 않게 적용표 확정.
3. 기존 UiWidgets.Button의 눌림(.55,.8,1)/비활성(.42,.45,.5) Tint는 예시 합성에만 사용했다. 실제 입력 상태·선/라벨 대비는 미검증.
4. ScrollThumb는 외형 리소스다. 현재 UiWidgets.Scroll에 Scrollbar 참조가 없으므로 연결 범위를 명시하고 새 조작 기능 없이 기존 스크롤 입력을 보존한다.
5. 기존6단위 멸망 게이지를 유지하고1px 트랙 안쪽에 단색 채움을 정렬한다. 기존Score/TargetScore·0/100% 의미 보존.
6. 공용 위젯·PopupHost·HUD 연결·가로세로·ko/en·큰 글씨·안전영역·터치/스크롤/입력차단·게임 화면 확인.

일괄 적용 후 기술 검사와 최종 게임 화면 확인을 마쳐야 completed로 전환한다.

- 2026-11-16 제작 종료 문서 검사: Unity43 PLAN·active0·목록43·링크4,142 통과(이후 재사용 계약 링크 추가), git diff --check 통과. Art25 PLAN·active0·목록25·링크773 검사, 기존 완료 PLAN 링크 오류9건만 유지. 정식 기술/게임 검증은 미실행.

2026-11-16 F-001: 사용자 「UI는 프레임과 배경 등만, 글자는 모두 제거; TextMeshProUGUI로 관리」 요청 반영. UI 원본/PNG6종에는 처음부터 글자가 없었으며 원본 픽셀은 변경하지 않았다. 글자는 이전 확인용 합성에만 있었고, 최신 미리보기에서는 설명 글자·숫자·예시 아이콘까지 모두 제외했다. 모든 제품 텍스트는 Unity TextMeshProUGUI와 기존 Localization으로 관리한다. 원본 미변경으로 r001 유지, 기존 제출 미리보기는 이력으로 보존한다.

- 2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-039 아트 작업을 진행해줘」. 6종 r001 원본과 기존 Fill 재사용 확인 완료. Unity 미적용·정식 검증 미실행으로 on hold 유지.

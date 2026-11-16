# PLAN-041 생체 군락 점령·결과 Sprite 요청

[제작 PLAN-041](../../../../../docs/plans/PLAN-041-destruction-results-art.md) / [마지막 적용 PLAN-042](../../../../../docs/plans/PLAN-042-art-integration-review.md).
2026-11-16 사용자 제작 위임. Art 원본·시트·미리보기만 제작하고 Unity 수령/적용·정식 검증은 PLAN-042에 유보한다. 글자·수치·버튼 문구는 TextMeshProUGUI/Localization.

| 요청 | 제작/재사용 | 규격 | Unity 수령 → 최종 예정 |
| --- | --- | --- | --- |
| PLAN-041-SPR-001 | [Art PLAN-031 WorldDestruction r002](../../../../../../Art/docs/plans/PLAN-031-world-destruction.md) 신규 | 128×128셀×32, 8×4, 시트1024×512 | Assets/Sprites/Require/PLAN-041/PLAN-041-SPR-001/r002/WorldDestruction.png → Assets/Sprites/UI/Ending/WorldDestruction.png |
| PLAN-041-SPR-002 | ResultFrame 신규 제작 없음, [PanelFrame r001](../../../../../../Art/assets/ui/PanelFrame/revisions/r001/README.md) 재사용 | 32×32, border L/B/R/T=4/4/4/4 | 원래 PLAN-038-SPR-001 수령 → Assets/Sprites/UI/Frames/PanelFrame.png |

ResultFrame 이름의 새 파일/폴더/Art PLAN은 만들지 않고 요청 ID는 다른 에셋에 재배정하지 않는다. PanelFrame은 사용자 원본 승인 r001이며 Unity 적용·검증은 별도 대기다.

## 세계 재생·슬라이싱 계약

RGB/sRGB, RGBA·투명 배경, 셀128×128·8열×4행·행 우선, 간격/여백0. 중심pivot(.5,.5), Multiple/Full Rect·PPU32·Point·Clamp·밉맵OFF·무압축.
태그 Colonization, 원본1~32, Sprite WorldDestruction_Colonization_01~32. i=0~31에 rect x=(i%8)*128, Unity y=512-(floor(i/8)+1)*128.
Ending.Progress를 min(31,floor(clamp01(progress)*32))로 대응한다. 현재 JSON Session endingSeconds=4이므로 각125ms·총4초. 값 변경 시 Duration/32, 아트가 종료 시간을 강제하지 않는다.
Results는32 고정, 게임에서 반복/역재생 없음. 가로세로 전환에도 진행률 유지, 이전 세션 표시 참조 정리. 결과집계·세이브·완료를 아트 콜백에서 수행하지 않는다.

## UI 적용 계약

현재 LaboratoryView.Ending의 세계Rect는 가로 min(w*.43-32,h-180), 세로 min(220,w-64,h*.25) 정사각형이다. 기존 Rect 안에서 등비 Fit·중심 배치, Raycast OFF.
공용 PanelFrame을 결과 목록 뒷면에 사용하되 기존 스크롤/마스크/행/버튼 영역·오류 문구 공간을 유지한다. 전체화면 배경은 기존 단색 유지, 본문 프레임 Tint white·Sliced·내용padding12 기준.
현재 UiList 내용폭 detailsWidth-16과 모서리/내용 여백이 충돌하지 않는지 PLAN-042에서 실제 Rect·PPU border scale과 함께 조정/검증한다.
최소터치크기·스크롤·오류/재시도·홈 복귀·큰 글씨·번역과 기존 종료시간/입력 차단을 보존한다.

## 제작·확인 상태

WorldDestruction r002 제작 완료·사용자 확인 대기. PanelFrame r001은 원본 확인 완료·재사용, 새 출력 없음.
Unity PNG 수령/적용과 정식 검증은 모두 미실행. 제작 후 사용자 확인 전에 PLAN-042를 시작하지 않는다.

[WorldDestruction r002 제출본](../../../../../../Art/assets/ui/WorldDestruction/revisions/r002/README.md) · [4초 GIF](../../../../../../Art/docs/plans/reviews/PLAN-031/r002/motion.gif) · [제작 인계](../../../../../docs/plans/evidence/PLAN-041/README.md). 원본 승인0·사용자 확인 대기, Unity/Art PLAN on hold.

## 2026-11-17 F-001 r002 변경

균열/소멸→생체 군락 확산/행성 점령. 32프레임·4초,6레이어, 원본/시트/GIF·r001 비교 보존. 파일명/ID는 기존 WorldDestruction 유지, 태그/셀수/배열은 위 r002 계약으로 교체한다. 기존 r001은 Art에 보존하며 원본 승인0 유지. PanelFrame r001에는 변경 없음.
기존 종료 조건/결과집계는 유지한다. 기존 캡션 의미와 점령 표현의 정합성은 마지막 통합에서 Localization과 함께 확인한다. GIF는120/130ms 교대·4초, 원본/게임은125ms 기준. 제작용 GIF만 반복, 게임은 Results에서32 고정. Unity 수령/적용·정식 검증은 PLAN-042.

- 최신 확인(2026-11-17): 사용자 r002 외형·동작 원본 확인 완료. Unity 적용/정식 검증은 승인된 PLAN-042에서 진행한다. 원본 승인은 실제 게임 화면 최종 승인이 아니다.


## PLAN-042 일괄 수령·적용 — 2026-11-17

[통합 기록](../../../../../docs/plans/evidence/PLAN-042/README.md) · [전체 경로/GUID](../../../../../docs/plans/evidence/PLAN-042/asset-manifest.csv)

| asset_id | 리비전 | 최종 적용 경로 | 상태 |
| --- | --- | --- | --- |
| WorldDestruction | r002 | `Assets/Sprites/UI/Ending/WorldDestruction.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |

수령 후 AssetDatabase.MoveAsset으로 최종 경로에 배치했으므로 Require 아래 PNG가 없는 것은 유실이 아니다. 제출본은 Art의 해당 리비전에 보존한다. 이 기록은 위 제작 당시 미수령/미적용 기록 이후의 최신 상태다.

## 통합 최종 사용자 확인 — 2026-11-17

2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 이 요청의 PLAN-042 적용 리비전과 재사용 대상 모두 실제 게임 화면 확인 완료. 수령·적용·기술 검증·사용자 확인을 충족했다. [통합 완료 기록](../../../../../docs/plans/evidence/PLAN-042/completion.md). 앞선 대기 기록은 당시 이력이다.

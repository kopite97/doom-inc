# PLAN-040 타이틀·로딩 Sprite 요청

[제작 PLAN-040](../../../../../docs/plans/PLAN-040-title-loading-art.md) / [일괄 적용 PLAN-042](../../../../../docs/plans/PLAN-042-art-integration-review.md).
2026-11-16 사용자 요청으로 3종 반복 애니메이션을 Art에서 제작한다. 모든 텍스트는 TextMeshProUGUI/Localization에서 관리하고 PNG/미리보기에 글자·로고를 넣지 않는다.
각16프레임·1태그·150ms/프레임·총2.4초·RGB/sRGB 원본 → 4열×4행 RGBA PNG 시트, Multiple/Full Rect·중심피벗·PPU32·Point·Clamp·밉맵OFF·무압축 예정. 타이틀 불투명, 장식 투명.

| 요청 | 에셋·Art PLAN | 크기 | 수령·최종 예정 |
| --- | --- | --- | --- |
| PLAN-040-SPR-001 | [TitleLandscape / Art PLAN-028](../../../../../../Art/docs/plans/PLAN-028-title-landscape.md) | 640×360 | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-001/r002/TitleLandscape.png` → `Assets/Sprites/UI/Title/TitleLandscape.png` |
| PLAN-040-SPR-002 | [TitlePortrait / Art PLAN-029](../../../../../../Art/docs/plans/PLAN-029-title-portrait.md) | 360×640 | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-002/r002/TitlePortrait.png` → `Assets/Sprites/UI/Title/TitlePortrait.png` |
| PLAN-040-SPR-003 | [LoadingDecoration / Art PLAN-030](../../../../../../Art/docs/plans/PLAN-030-loading-decoration.md) | 64×64 | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-003/r002/LoadingDecoration.png` → `Assets/Sprites/UI/Loading/LoadingDecoration.png` |

## 배치와 마지막 적용

현재 TitleView는 landscape를 오른쪽 별도Rect, portrait를 제목/메뉴 사이Rect에 Contain한다.
이번 그림은 여백을 포함한 전체 배경으로 제작한다. PLAN-042에서 그림 Rect를 SafeRoot 전체로 연결하고 중심 Contain+남는 영역 #0D1828을 사용한다. 메뉴 위치·스크롤·터치/입력·제목 TMP는 유지한다. Fill crop/비균등 확대는 사용하지 않는다.
가로 메뉴는 왼쪽, 세로 메뉴는 아래쪽. 안전영역/핵심 좌표는 각 Art PLAN을 따른다. 긴 화면/큰 글씨/오류시 메뉴 확장은 실제 Unity 검증 전이다.
LoadingDecoration은 Loading 상태의 문구 위에64이하 논리단위로 배치,12단위 간격. 공간부족시32까지 줄이고도 겹치면 장식만 숨긴다. 오류/확인에서는 숨긴다. 신규 직렬화 참조·실제Rect연결은 PLAN-042에서 수행한다.
기존 실제 진행률·최소 로딩 시간·Feel 웨이브·뒤 입력차단 유지. 새 게임명/로고·가짜 진행·텍스트 이미지 없음.
3종 r002 제작·보존 완료, 원본 확인 완료·미수령/미적용·미검증. 원본 확인 완료, 사용자 요청으로 PLAN-041 제작 진행.

## r002 제작 인계

[파일·해시·경로 목록](../../../../../docs/plans/evidence/PLAN-040/asset-manifest.csv), [세 원본 애니메이션](../../../../../../Art/docs/plans/reviews/PLAN-030/r002/overview.gif).

| 에셋 | 원본/PNG·명세 | 원본 확인 | 수령/적용 |
| --- | --- | --- | --- |
| TitleLandscape | [r002](../../../../../../Art/assets/ui/TitleLandscape/revisions/r002/README.md) | r002 확인 완료 | 미실행 |
| TitlePortrait | [r002](../../../../../../Art/assets/ui/TitlePortrait/revisions/r002/README.md) | r002 확인 완료 | 미실행 |
| LoadingDecoration | [r002](../../../../../../Art/assets/ui/LoadingDecoration/revisions/r002/README.md) | r002 확인 완료 | 미실행 |

Art PLAN-028~030과 Unity PLAN-040 on hold. 원본 확인 완료, PLAN-041 제작 진행, Unity 적용·정식 검증은 PLAN-042.

## r002 슬라이싱·재생 인계

타이틀 Ambient / 로딩 Bubbles, 각1~16 정방향 반복·프레임150ms·2.4초. 기존 캔버스는 셀 크기이며 전체 시트는 TitleLandscape2560×1440, TitlePortrait1440×2560, LoadingDecoration256×256이다. 좌상단부터 행 우선4×4, 여백/간격0, 중심pivot(.5,.5), {asset_id}_{tag}_01~16. 타이틀 Max Size4096이상으로 픽셀 축소 방지.
타이틀은 기포·생명체 약 ±2px 움직임, 로딩은 기포만 반복. 화면 표시 중 재생·이탈 시 정지, 로딩 오류/확인에서는 정지·숨김, 재진입 첫 프레임. 실제 진행률과 독립. 배열·시간/태그 연결은 PLAN-042에서 구현/검증한다. r001 정적본은 Art에 보존하며 최신 제출 r002의 사용자 승인은 완료했다.

- 2026-11-16 사용자 「좋아. r002로 진행하자. 확인 완료로 기록하고 다음 PLAN-041 아트 작업을 진행해줘」에 따라 타이틀/로딩3종 r002 외형·동작 원본 확인 완료. Unity 수령/적용·정식 기술/게임 검증은 PLAN-042 대기이며 completed 판정은 아님.


## PLAN-042 일괄 수령·적용 — 2026-11-17

[통합 기록](../../../../../docs/plans/evidence/PLAN-042/README.md) · [전체 경로/GUID](../../../../../docs/plans/evidence/PLAN-042/asset-manifest.csv)

| asset_id | 리비전 | 최종 적용 경로 | 상태 |
| --- | --- | --- | --- |
| TitleLandscape | r002 | `Assets/Sprites/UI/Title/TitleLandscape.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| TitlePortrait | r002 | `Assets/Sprites/UI/Title/TitlePortrait.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| LoadingDecoration | r002 | `Assets/Sprites/UI/Loading/LoadingDecoration.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |

수령 후 AssetDatabase.MoveAsset으로 최종 경로에 배치했으므로 Require 아래 PNG가 없는 것은 유실이 아니다. 제출본은 Art의 해당 리비전에 보존한다. 이 기록은 위 제작 당시 미수령/미적용 기록 이후의 최신 상태다.

## 통합 최종 사용자 확인 — 2026-11-17

2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 이 요청의 PLAN-042 적용 리비전과 재사용 대상 모두 실제 게임 화면 확인 완료. 수령·적용·기술 검증·사용자 확인을 충족했다. [통합 완료 기록](../../../../../docs/plans/evidence/PLAN-042/completion.md). 앞선 대기 기록은 당시 이력이다.

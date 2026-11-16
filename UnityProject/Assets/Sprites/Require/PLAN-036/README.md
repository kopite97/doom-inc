# PLAN-036 Sprite 요청

[출처 PLAN-036](../../../../../docs/plans/PLAN-036-laboratory-boundary-art.md) · [아트 지침](../../../../../docs/agents/art-assets.md) · [일괄 적용 PLAN-042](../../../../../docs/plans/PLAN-042-art-integration-review.md)

## PLAN-036-SPR-001

- 구분: 연구소 끝을 마감하는 신규 정적 외곽. 파일 LaboratoryBoundary.png.
- 필요한 모습: 높은 벽 없이 기존 바닥 밖10px의 낮은 금속 띠, 직선4·바깥 모서리4. 바깥 단색은 재사용. 문/통로/충돌/지도확장 없음.
- 규격: 64×64셀·4열2행·256×128 RGBA/sRGB, Multiple 예정. Top/Right/Bottom/Left/TopLeft/TopRight/BottomRight/BottomLeft, 중심 피벗(0.5,0.5). PPU/필터 등은 아트 지침 참조.
- 정적8프레임·8단일 태그, 100ms는 편집 슬롯 값이고 게임에서 재생하지 않는다. 상세 접합·방향·좌표/색은 제출 명세를 따른다.
- 사용처: LaboratoryWorldMap 외곽 표시 레이어·모서리, Collider/Raycast 없음. 실제 연결·전체 보기 여백·끝 칸 시설·입력 검증은 PLAN-042.
- 수령 예정: `Assets/Sprites/Require/PLAN-036/PLAN-036-SPR-001/r001/LaboratoryBoundary.png`.
- 최종 예정: `Assets/Sprites/World/LaboratoryBoundary/LaboratoryBoundary.png`.
- 상태: 요청 / Art r001 제작 보존 / Unity 미수령·미적용 / 원본 r001 승인 / 정식 검증·게임 화면 확인 대기.
- 최신 수령·실제 적용 리비전: 없음. 원본 승인: r001 확인 완료(2026-11-16). 게임 화면 승인: 없음.

## Art 원본·제출·확인 자료

- [Art PLAN-013](../../../../../../Art/docs/plans/PLAN-013-laboratory-boundary.md)
- [작업 원본](../../../../../../Art/assets/backgrounds/LaboratoryBoundary/source/LaboratoryBoundary.aseprite)
- [보존 원본](../../../../../../Art/assets/backgrounds/LaboratoryBoundary/revisions/r001/source/LaboratoryBoundary.aseprite)
- [Art PNG](../../../../../../Art/assets/backgrounds/LaboratoryBoundary/revisions/r001/export/LaboratoryBoundary.png)
- [r001 명세](../../../../../../Art/assets/backgrounds/LaboratoryBoundary/revisions/r001/README.md)
- [사용자 확인 미리보기](../../../../../../Art/docs/plans/reviews/PLAN-013/r001/README.md)

원본 확인 후 다음 제작으로 넘어간다. 수정 시 r002부터 보존한다. Unity 적용·정식 기술/게임 검증·최종 게임 화면 확인은 `docs/plans/evidence/PLAN-042/validation.md`, `plan-verification-matrix.csv`, `runs/run-001/`에서 마지막에 수행한다.

## 원본 확인 완료

2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-037 아트 작업을 진행해줘」. r001 외곽 원본 확인 완료, Unity 수령/적용 및 기술/게임 화면 검증은 PLAN-042로 유보한다.


## PLAN-042 일괄 수령·적용 — 2026-11-17

[통합 기록](../../../../../docs/plans/evidence/PLAN-042/README.md) · [전체 경로/GUID](../../../../../docs/plans/evidence/PLAN-042/asset-manifest.csv)

| asset_id | 리비전 | 최종 적용 경로 | 상태 |
| --- | --- | --- | --- |
| LaboratoryBoundary | r001 | `Assets/Sprites/World/LaboratoryBoundary/LaboratoryBoundary.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |

수령 후 AssetDatabase.MoveAsset으로 최종 경로에 배치했으므로 Require 아래 PNG가 없는 것은 유실이 아니다. 제출본은 Art의 해당 리비전에 보존한다. 이 기록은 위 제작 당시 미수령/미적용 기록 이후의 최신 상태다.

## 통합 최종 사용자 확인 — 2026-11-17

2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 이 요청의 PLAN-042 적용 리비전과 재사용 대상 모두 실제 게임 화면 확인 완료. 수령·적용·기술 검증·사용자 확인을 충족했다. [통합 완료 기록](../../../../../docs/plans/evidence/PLAN-042/completion.md). 앞선 대기 기록은 당시 이력이다.

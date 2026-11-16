# PLAN-039 탄생·방출 Sprite 요청

[제작 PLAN-039](../../../../../docs/plans/PLAN-039-birth-release-effects-art.md) / [일괄 적용 PLAN-042](../../../../../docs/plans/PLAN-042-art-integration-review.md).
2026-11-16 사용자 제작 위임. 정적32×32 흰색 alpha 마스크2종과 움직임 명세·GIF를 Art에서 준비한다. Unity PNG 복사·재질/프리팹·임포트·게임 검증은 마지막에 진행한다.

| 요청 | 에셋·Art PLAN | 출력·재생 | 상태 |
| --- | --- | --- | --- |
| PLAN-039-SPR-001 | [BirthPulse / Art PLAN-026](../../../../../../Art/docs/plans/PLAN-026-birth-pulse.md) | BirthPulse.png 정적1장 + 450ms 표시 전환 | r001 제작·확인 대기·미수령 |
| PLAN-039-SPR-002 | [ReleaseTrail / Art PLAN-027](../../../../../../Art/docs/plans/PLAN-027-release-trail.md) | ReleaseTrail.png 정적1장 + 350ms 표시 전환 | r001 제작·확인 대기·미수령 |

## 출력·좌표와 실제 연결

32×32 RGBA·흰색 #FFFFFF alpha0/255·Single/Full Rect·피벗(.5,.5)·PPU32·Point·Clamp·밉맵OFF·무압축·sRGB 예정.
애니메이션 시트가 아니라 한 장의 마스크다. 런타임 시간은 Art PLAN의 구간별 Scale/Position/Alpha를 따른다. 원본100ms 편집 시간을 재생 시간으로 쓰지 않는다.
BirthPulse는 #A2D0B8 청록 Tint와 바닥 원형 확산, ReleaseTrail은 #B5C8CE 회청 Tint와 우상향 잔상. 소멸은 전체 alpha 감소, 독립 파편 물리/경로 없음.
GIF는 기존 아트와 합성한 제작 예시이며 정확한 Unity 동작·색/배율·합성 순서 검증은 PLAN-042.
수령/최종 예정:
- PLAN-039-SPR-001: `Assets/Sprites/Require/PLAN-039/PLAN-039-SPR-001/r001/BirthPulse.png` → `Assets/Sprites/VFX/Birth/BirthPulse.png`.
- PLAN-039-SPR-002: `Assets/Sprites/Require/PLAN-039/PLAN-039-SPR-002/r001/ReleaseTrail.png` → `Assets/Sprites/VFX/Release/ReleaseTrail.png`.

## 사건·수명

실제 생성·배치 성공 이후만 탄생1회, 성공한 방출 직전 개체 위치 스냅샷에서 방출1회. 실패·복원 제외. 방출 논리 제거와 게임 결과를 연출 종료까지 지연하지 않는다.
본체와 별도Renderer의Transform/alpha를 관리하며 선택 재질/이동 보간을 덮어쓰지 않는다. 사건 중복 방지·동시 상한8 제안, 정지/홈 복귀/멸망/씬 종료 정리는 PLAN-042에서 연결·검증한다.
원본 확인 전 PLAN-040 미착수. 필수 기술/게임 검증·최종 게임 화면 확인은 PLAN-042.

## r001 제작·인계

[파일·리비전·해시·경로 목록](../../../../../docs/plans/evidence/PLAN-039/asset-manifest.csv).

| 에셋 | 원본/PNG·명세 | 확인용 재생 | 원본 승인 | 수령/적용 |
| --- | --- | --- | --- | --- |
| BirthPulse | [r001](../../../../../../Art/assets/vfx/BirthPulse/revisions/r001/README.md) | [단독](../../../../../../Art/docs/plans/reviews/PLAN-026/r001/preview.gif) · [합성](../../../../../../Art/docs/plans/reviews/PLAN-026/r001/context.gif) | r001 확인 완료 | 미실행 |
| ReleaseTrail | [r001](../../../../../../Art/assets/vfx/ReleaseTrail/revisions/r001/README.md) | [단독](../../../../../../Art/docs/plans/reviews/PLAN-027/r001/preview.gif) · [합성](../../../../../../Art/docs/plans/reviews/PLAN-027/r001/context.gif) | r001 확인 완료 | 미실행 |

Art PLAN-026/027 및 Unity PLAN-039는 on hold. 사용자 원본·움직임 확인과 PLAN-042 적용·정식 검증 대기. 정적 mask와 재생 합성을 별도로 보존했으며 Unity 파일을 복사하지 않았다.

- 2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-040 아트 작업을 진행해줘」. 2종 r001 확인 완료. Unity 적용·정식 검증·게임 화면 확인은 PLAN-042 대기.


## PLAN-042 일괄 수령·적용 — 2026-11-17

[통합 기록](../../../../../docs/plans/evidence/PLAN-042/README.md) · [전체 경로/GUID](../../../../../docs/plans/evidence/PLAN-042/asset-manifest.csv)

| asset_id | 리비전 | 최종 적용 경로 | 상태 |
| --- | --- | --- | --- |
| BirthPulse | r001 | `Assets/Sprites/VFX/Birth/BirthPulse.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| ReleaseTrail | r001 | `Assets/Sprites/VFX/Release/ReleaseTrail.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |

수령 후 AssetDatabase.MoveAsset으로 최종 경로에 배치했으므로 Require 아래 PNG가 없는 것은 유실이 아니다. 제출본은 Art의 해당 리비전에 보존한다. 이 기록은 위 제작 당시 미수령/미적용 기록 이후의 최신 상태다.

## 통합 최종 사용자 확인 — 2026-11-17

2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 이 요청의 PLAN-042 적용 리비전과 재사용 대상 모두 실제 게임 화면 확인 완료. 수령·적용·기술 검증·사용자 확인을 충족했다. [통합 완료 기록](../../../../../docs/plans/evidence/PLAN-042/completion.md). 앞선 대기 기록은 당시 이력이다.

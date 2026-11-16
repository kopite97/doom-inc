# PLAN-034 Sprite 요청

[출처 PLAN-034](../../../../../docs/plans/PLAN-034-invader-raider-art.md) · [아트 지침](../../../../../docs/agents/art-assets.md) · [일괄 적용 담당 PLAN-042](../../../../../docs/plans/PLAN-042-art-integration-review.md)

2026-11-16 사용자 지정으로 제작만 수행했다. Art r002 보관본을 연결하며 **요청 / 미수령·미적용 / 원본 r002 승인 / 정식 검증·게임 화면 확인 대기**다. Unity에 PNG를 복사하지 않았다.

## PLAN-034-SPR-001

- 구분: 기존 대체 도형/목록 표시를 정식 아트로 교체. 요청 파일 `Invader.png`.
- 용도: `creature.invader` 연구소 본체·4방향 대기/이동 프레임과 배양/생명체/침공 목록. 첫 셀은 목록 대표 프레임. 네 방향 Idle/Move 제작, Work/전투/귀환/운반물 없음.
- 제작 기록: [Art PLAN-011](../../../../../../Art/docs/plans/PLAN-011-invader.md), asset_id `Invader`, r002. [제출 명세](../../../../../../Art/assets/characters/Invader/revisions/r002/README.md).
- 원본: [작업용 Aseprite](../../../../../../Art/assets/characters/Invader/source/Invader.aseprite), [리비전 보존본](../../../../../../Art/assets/characters/Invader/revisions/r002/source/Invader.aseprite), [Art PNG 보관본](../../../../../../Art/assets/characters/Invader/revisions/r002/export/Invader.png).
- 규격: 64×64셀·10열4행·640×256 RGBA PNG, Multiple 예정. Down/Left/Right/Up 각 Idle4(250ms×4)·Move6(120ms×6), 총40프레임·8태그, 행 우선 정방향 반복. 트리밍/회전/중복 병합 없음.
- 위치: 좌상단 원점 (+X 오른쪽/+Y 아래) 접지 (32,54)px, Unity 하단 원점 정규화 피벗 (0.5,0.15625). PPU와 임포트 기본값은 상단 아트 지침을 따른다.
- 미리보기: [방향별 외형](../../../../../../Art/docs/plans/reviews/PLAN-011/r002/design.png), [프레임](../../../../../../Art/docs/plans/reviews/PLAN-011/r002/frames.png), [재생 GIF](../../../../../../Art/docs/plans/reviews/PLAN-011/r002/Move-all-directions.gif). 제작 확인용이며 검증 통과 증거가 아니다.
- 수령 예정: `Assets/Sprites/Require/PLAN-034/PLAN-034-SPR-001/r002/Invader.png`.
- 최종 예정: `Assets/Sprites/Creature/Invader/Invader.png`. SO `Invader.asset` Icon 및 정의별 표시/생성 도구 연결은 PLAN-042가 수행한다.
- 최신 수령/실제 적용 리비전: 없음. 원본 승인: r002 확인 완료(2026-11-16). 게임 화면 승인: 없음.
- 정식 검증 예정: `docs/plans/evidence/PLAN-042/validation.md`, `plan-verification-matrix.csv`, `runs/run-001/`. 실패/피드백 수정은 같은 요청 ID의 새 리비전으로 보존한다.

## PLAN-034-SPR-002

- 구분: 기존 대체 도형/목록 표시를 정식 아트로 교체. 요청 파일 `Raider.png`.
- 용도: `creature.raider` 연구소 본체·4방향 대기/이동 프레임과 배양/생명체/침공 목록. 첫 셀은 목록 대표 프레임. 네 방향 Idle/Move 제작, Work/전투/귀환/운반물 없음.
- 제작 기록: [Art PLAN-012](../../../../../../Art/docs/plans/PLAN-012-raider.md), asset_id `Raider`, r002. [제출 명세](../../../../../../Art/assets/characters/Raider/revisions/r002/README.md).
- 원본: [작업용 Aseprite](../../../../../../Art/assets/characters/Raider/source/Raider.aseprite), [리비전 보존본](../../../../../../Art/assets/characters/Raider/revisions/r002/source/Raider.aseprite), [Art PNG 보관본](../../../../../../Art/assets/characters/Raider/revisions/r002/export/Raider.png).
- 규격: 64×64셀·10열4행·640×256 RGBA PNG, Multiple 예정. Down/Left/Right/Up 각 Idle4(200ms×4)·Move6(100ms×6), 총40프레임·8태그, 행 우선 정방향 반복. 트리밍/회전/중복 병합 없음.
- 위치: 좌상단 원점 (+X 오른쪽/+Y 아래) 접지 (32,54)px, Unity 하단 원점 정규화 피벗 (0.5,0.15625). PPU와 임포트 기본값은 상단 아트 지침을 따른다.
- 미리보기: [방향별 외형](../../../../../../Art/docs/plans/reviews/PLAN-012/r002/design.png), [프레임](../../../../../../Art/docs/plans/reviews/PLAN-012/r002/frames.png), [재생 GIF](../../../../../../Art/docs/plans/reviews/PLAN-012/r002/Move-all-directions.gif). 제작 확인용이며 검증 통과 증거가 아니다.
- 수령 예정: `Assets/Sprites/Require/PLAN-034/PLAN-034-SPR-002/r002/Raider.png`.
- 최종 예정: `Assets/Sprites/Creature/Raider/Raider.png`. SO `Raider.asset` Icon 및 정의별 표시/생성 도구 연결은 PLAN-042가 수행한다.
- 최신 수령/실제 적용 리비전: 없음. 원본 승인: r002 확인 완료(2026-11-16). 게임 화면 승인: 없음.
- 정식 검증 예정: `docs/plans/evidence/PLAN-042/validation.md`, `plan-verification-matrix.csv`, `runs/run-001/`. 실패/피드백 수정은 같은 요청 ID의 새 리비전으로 보존한다.

## 사용자 원본 확인 완료

사용자 지정으로 제작 직후 각 에셋의 정면·재생 GIF를 제시하고 확인받는다. 현재 두 r002 모두 사용자 원본 확인 완료다. 이후 수정은 r003부터 보존·재제시하고, 두 에셋 확인을 마쳐 사용자 요청의 PLAN-036 제작을 진행한다. Unity 적용·정식 기술/게임 검증은 PLAN-042까지 유보한다.

## F-001 리비전 갱신

사용자 네 방향 이동 프레임 요청으로 각40프레임·8태그 r002를 준비했다. 기존 r001 정면 대기4프레임·256×64 제출 이력은 Art revisions/r001/README.md에 보존한다. 현재 수령/적용 리비전은 여전히 없다. 실제 이동 상태와 방향 연결·속도 검증은 PLAN-042에서 처리하며 제작 요청만으로 Worker/이동 AI를 변경하지 않는다.

- 승인 근거: 2026-11-16 사용자 「좋아. r002로 진행하자. 확인 완료로 기록하고 다음 PLAN-036 아트 작업을 진행해줘」. 외형·동작 원본 승인만 기록하며 게임 적용/승인을 뜻하지 않는다.


## PLAN-042 일괄 수령·적용 — 2026-11-17

[통합 기록](../../../../../docs/plans/evidence/PLAN-042/README.md) · [전체 경로/GUID](../../../../../docs/plans/evidence/PLAN-042/asset-manifest.csv)

| asset_id | 리비전 | 최종 적용 경로 | 상태 |
| --- | --- | --- | --- |
| Invader | r002 | `Assets/Sprites/Creature/Invader/Invader.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |
| Raider | r002 | `Assets/Sprites/Creature/Raider/Raider.png` | 수령·적용·Editor 검증 / 게임 화면 사용자 확인 대기 |

수령 후 AssetDatabase.MoveAsset으로 최종 경로에 배치했으므로 Require 아래 PNG가 없는 것은 유실이 아니다. 제출본은 Art의 해당 리비전에 보존한다. 이 기록은 위 제작 당시 미수령/미적용 기록 이후의 최신 상태다.

## 통합 최종 사용자 확인 — 2026-11-17

2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 이 요청의 PLAN-042 적용 리비전과 재사용 대상 모두 실제 게임 화면 확인 완료. 수령·적용·기술 검증·사용자 확인을 충족했다. [통합 완료 기록](../../../../../docs/plans/evidence/PLAN-042/completion.md). 앞선 대기 기록은 당시 이력이다.

# PLAN-034 제작 인계

2026-11-16 · [PLAN-034](../../PLAN-034-invader-raider-art.md) · [통합 담당 PLAN-042](../../PLAN-042-art-integration-review.md#batch-art-workflow).

**제작 인계 준비 / Unity 미수령·미적용 / 정식 기술·사용자 시각 검증 대기.**

- Invader: Art PLAN-011 r002, Down/Left/Right/Up 각 Idle4·250ms/Move6·120ms, 총40프레임.
- Raider: Art PLAN-012 r002, Down/Left/Right/Up 각 Idle4·200ms/Move6·100ms, 총40프레임.
- 각 원본 64×64 5레이어, PNG 640×256 한 장. 원본 보존본·명세·정면/프레임/GIF 미리보기 포함.
- [전체 파일·요청·리비전 목록](asset-manifest.csv). SHA-256은 제출 파일 식별용이며 검증 통과를 뜻하지 않는다.
- Aseprite 1.3.18.6 batch로 새 파일만 제작했다. Live Bridge는 이 세션에서 연결되지 않았고 열린 사용자 탭을 변경하지 않았다.
- 제작 중 저장/열기·미리보기 확인과 문서 검사만 수행했다. 원본/PNG 대응·알파·타이밍·피벗 정식 검사, Unity 임포트/슬라이싱/연결, Edit/Play/게임 화면 검증은 PLAN-042에 남긴다.
- 승인 리비전 없음. Unity PLAN-034 및 Art PLAN-011/012는 사용자 지정 통합 대기로 on hold이며 완료 판정은 하지 않았다.
- 기존 완료 PLAN·원본·Unity 코드/에셋을 수정하지 않았다. 새 요청 README는 문서이며 PNG 수령을 뜻하지 않는다.

## 실제 제작 자료

- [Invader Art PLAN](../../../../../Art/docs/plans/PLAN-011-invader.md) · [r002 명세](../../../../../Art/assets/characters/Invader/revisions/r002/README.md) · [미리보기](../../../../../Art/docs/plans/reviews/PLAN-011/r002/README.md)
- [Raider Art PLAN](../../../../../Art/docs/plans/PLAN-012-raider.md) · [r002 명세](../../../../../Art/assets/characters/Raider/revisions/r002/README.md) · [미리보기](../../../../../Art/docs/plans/reviews/PLAN-012/r002/README.md)

[Unity 요청 README](../../../../UnityProject/Assets/Sprites/Require/PLAN-034/README.md)

## 문서 확인

- Unity 문서 검사: PLAN43·active0·목록43·로컬 링크4087 통과. `git diff --check` 통과.
- Art 문서 검사: PLAN12·active0·목록12. 기존 완료 PLAN-002~005/008의 링크 오류9건이 남아 전체 검사는 실패다. 이번 새 PLAN의 상태/목록 오류 및 새 링크 오류는 보고되지 않았다.
- 위 결과는 문서 검사이며 아트 정식 기술·시각 검증 결과가 아니다.

## 제작 후 사용자 확인

2026-11-16 사용자가 제작 후 확인 절차를 요청했다. Invader/Raider r002 방향별 외형·대기/이동 미리보기를 제시하고 응답 대기 중이다. 이 요청 자체는 외형 승인이 아니다. 두 에셋의 확인과 필요한 수정을 마치기 전 PLAN-036 제작을 시작하지 않는다. 기술 검증·Unity 적용·게임 화면 확인은 PLAN-042에서 진행한다.

## F-001 제작 변경

2026-11-16 사용자 상하좌우 이동 프레임 요청으로 두 에셋을 r002로 확장했다. 기존 r001과 작업 직전 디스크 원본을 보존하고 새 원본/시트/태그/방향 미리보기를 작성했다. Aseprite batch 새 원본 작업이며 Unity 실행/임포트·정식 기술 검증은 하지 않았다. 요청한 범위는 반영했고 사용자 최종 외형/동작 확인을 기다린다.

- r002 문서 확인: Unity PLAN43·active0·링크4088 및 변경 공백 검사 통과. Art PLAN12·active0·링크479, 기존 완료 PLAN의 링크 오류9건만 남는다. 새 산출물의 정식 기술/게임 검증 통과를 뜻하지 않는다.

## r002 원본 확인 완료

2026-11-16 사용자 「좋아. r002로 진행하자. 확인 완료로 기록하고 다음 PLAN-036 아트 작업을 진행해줘」. 두 종류의 외형·4방향 대기/이동 원본 확인 완료. 이전 확인 대기 문장은 당시 이력이며 최신 원본 승인 상태는 이 기록/manifest를 따른다. 기술 검증·Unity 적용·게임 화면 확인은 PLAN-042에 남아 세 PLAN 모두 on hold를 유지한다.

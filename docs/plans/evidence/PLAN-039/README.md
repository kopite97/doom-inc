# PLAN-039 탄생·방출 제작 인계

2026-11-16 / [PLAN](../../PLAN-039-birth-release-effects-art.md) / [요청](../../../../UnityProject/Assets/Sprites/Require/PLAN-039/README.md) / [파일·리비전·해시·경로](asset-manifest.csv).

정적32×32 마스크2종 r001·원본·보존본·재생 명세·미리보기 제작 완료. r001 원본/움직임 사용자 확인 완료, Unity 미수령·미적용·정식 검증 미실행.
PNG는 각1셀이고 게임용 프레임 시트가 아니다. Shape는 흰색alpha 마스크, 최종 Tint/Transform/alpha 전환은 PLAN-042에서 연결한다.

| 효과 | Art 기록 | 제출 명세 | 재생 예시 |
| --- | --- | --- | --- |
| BirthPulse 450ms | [PLAN-026](../../../../../Art/docs/plans/PLAN-026-birth-pulse.md) | [r001](../../../../../Art/assets/vfx/BirthPulse/revisions/r001/README.md) | [단독](../../../../../Art/docs/plans/reviews/PLAN-026/r001/preview.gif) · [본체/바닥 합성](../../../../../Art/docs/plans/reviews/PLAN-026/r001/context.gif) |
| ReleaseTrail 350ms | [PLAN-027](../../../../../Art/docs/plans/PLAN-027-release-trail.md) | [r001](../../../../../Art/assets/vfx/ReleaseTrail/revisions/r001/README.md) | [단독](../../../../../Art/docs/plans/reviews/PLAN-027/r001/preview.gif) · [본체/바닥 합성](../../../../../Art/docs/plans/reviews/PLAN-027/r001/context.gif) |

## 사용자 확인할 모습

- BirthPulse: 발밑의 작은 청록 링이 가로 방향으로 넓어지고 alpha가 줄어든다. 본체는 효과보다 위에 보인다.
- ReleaseTrail: 성공 시 본체가 제거된 자리에 선/작은 파편이 우상향으로 흐르다 빠르게 사라진다. 실제 출구·개체 이동 경로를 추가하지 않는다.
- GIF는 기존 Support/Invader r002의 첫 Idle_Down 프레임과 LaboratoryFloorSet r003 Base를 읽기 합성했다. 96px 장면을4배 확대했고 50ms 간격의 중간값 샘플이다.
- 앞350ms와 뒤700ms의 정지 구간은 관찰용이다. 실제 효과는 각각450/350ms·1회이며 GIF 반복/합성 본체 생멸은 게임 구현이 아니다.
- 글자·숫자·본문/바닥은 제출 마스크PNG에 포함하지 않는다. GIF·합성 Aseprite는 Art 검토 자료로만 보존한다.
- r001 원본·움직임 확인 완료. 사용자 요청에 따라 PLAN-040 제작 진행.

## PLAN-042 인계

원본/출력 대응·알파·피벗·PPU/크기·정확한색/시간·합성순서·상한·사건 성공/실패/복원·정지/홈 복귀·씬 종료/새 판·잔류콜백·선택재질/이동보간 충돌·게임 화면을 마지막에 검증한다.
BirthPulse는 실제 생성·지도 배치 성공 후1회, ReleaseTrail은 방출 성공 직전 위치 스냅샷으로1회. 논리 생성/제거를 연출 종료까지 지연하지 않는다.
재질/프리팹 예정 경로는 PLAN-039, source/PNG/수령/최종 예정 경로는 manifest를 따른다. 공급사 에셋·렌더러 전역 설정·Unity 참조는 변경하지 않았다.
GIF 양자화와 최근접 보간 근사는 최종 Unity 화면과 차이가 있을 수 있다. 정식 검증 통과를 의미하지 않는다.

- 제작 종료 문서 검사(2026-11-16): Unity43 PLAN·active0·목록43·로컬 링크4,158 통과, git diff --check 통과. Art27 PLAN·active0·목록27·링크833 검사, 기존 완료 PLAN 링크 오류9건만 남음. 신규 문서 링크 오류 없음. 사용자 요청에 따라 완료 PLAN은 수정하지 않았으며 Art 검사 전체 통과로 기록하지 않는다. 정식 기술/게임 검증은 미실행.

- 2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-040 아트 작업을 진행해줘」. 2종 r001 확인 완료. Unity 적용·정식 검증·게임 화면 확인은 PLAN-042 대기.

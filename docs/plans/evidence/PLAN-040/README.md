# PLAN-040 타이틀·로딩 r002 제작 인계

2026-11-16 / [PLAN](../../PLAN-040-title-loading-art.md) / [요청](../../../../UnityProject/Assets/Sprites/Require/PLAN-040/README.md) / [파일·해시·리비전 목록](asset-manifest.csv).

사용자 요청: 타이틀 기포 상승·작은 생명체 움직임과 로딩 장식 반복 기포.
3종 각16프레임×150ms=2.4초 반복, 4열×4행 PNG 시트. 최신 r002 사용자 확인 완료·Unity 미수령/미적용·정식 검증 PLAN-042 유보.
r001 정적 원본·출력·미리보기는 보존했다. 모든 글자는 기존 TextMeshProUGUI/Localization에서 관리한다.

| 에셋 | 셀 / 시트 | 제출 명세 | 실제 시간 GIF |
| --- | --- | --- | --- |
| TitleLandscape | 640×360 / 2560×1440 | [r002](../../../../../Art/assets/ui/TitleLandscape/revisions/r002/README.md) | [GIF](../../../../../Art/docs/plans/reviews/PLAN-028/r002/animation.gif) |
| TitlePortrait | 360×640 / 1440×2560 | [r002](../../../../../Art/assets/ui/TitlePortrait/revisions/r002/README.md) | [GIF](../../../../../Art/docs/plans/reviews/PLAN-029/r002/animation.gif) |
| LoadingDecoration | 64×64 / 256×256 | [r002](../../../../../Art/assets/ui/LoadingDecoration/revisions/r002/README.md) | [GIF](../../../../../Art/docs/plans/reviews/PLAN-030/r002/animation.gif) |

## 사용자 확인 자료

- [전체3종 GIF](../../../../../Art/docs/plans/reviews/PLAN-030/r002/overview.gif): 가로 위·세로 오른쪽·로딩 아래.
- [움직임 확대 GIF](../../../../../Art/docs/plans/reviews/PLAN-030/r002/motion-detail.gif): 왼쪽부터 가로 타이틀·세로 타이틀·로딩.
- [4개 시점 비교](../../../../../Art/docs/plans/reviews/PLAN-030/r002/keyframes.png): 프레임1/5/9/13, 왼쪽 위부터 행 우선.
- [이전 정적 구성과 화면비 자료](../../../../../Art/docs/plans/reviews/PLAN-030/r001/README.md): 배치/여백 계약은 동일.
- 모두 Aseprite 제작 미리보기이며 Unity 캡처나 정식 검증 결과가 아니다.

## 마지막 적용·검증

- 타이틀 Ambient, 로딩 Bubbles. 각1~16·150ms·정방향 반복. 방/유리/장비/조명 고정, 타이틀 생명체만 약 ±2px 부유. 로딩 생명체는 고정.
- Multiple 4×4 분할·중심pivot·명시한 Sprite 이름/시간 배열로 연결한다. 타이틀 시트는 Max Size4096이상으로 원본 유지.
- TitleView illustration을 전체SafeRoot 중심 Contain, 여백 #0D1828로 연결한다. 가로세로 변경 시 동일 재생 위상 권장. 숨김/씬 이탈에서 재생 해제, 재진입 첫 프레임.
- Loading 장식은 문구 위12단위·64이하, 부족하면32까지 축소 후 숨김. Loading에서만 재생, Error/Confirmation에서 정지·숨김. 실제 진행·Feel 웨이브·입력 차단 유지.
- 원본/PNG/알파·시트/시간·반복 경계·가로세로/SafeArea·ko/en/큰 글씨·오류/재시도·씬 흐름·생성 도구/참조 보존은 PLAN-042에서 함께 검증한다.
- 현재 원본 승인 r002. 사용자 요청으로 PLAN-041 제작 진행. 이번에 Unity PNG 복사·임포트·코드/씬 변경·Edit/Play·게임 캡처는 수행하지 않았다.

- r002 제작 종료 문서 검사(2026-11-16): Unity43 PLAN·active0·목록43·로컬 링크4,175 통과, git diff --check 통과. Art30 PLAN·active0·목록30·링크920 검사, 기존 완료 PLAN 링크 오류9건만 유지. 이번 변경의 신규 링크 오류 없음. 정식 기술/게임 검증은 PLAN-042까지 미실행.

- 2026-11-16 사용자 「좋아. r002로 진행하자. 확인 완료로 기록하고 다음 PLAN-041 아트 작업을 진행해줘」에 따라 타이틀/로딩3종 r002 외형·동작 원본 확인 완료. Unity 수령/적용·정식 기술/게임 검증은 PLAN-042 대기이며 completed 판정은 아님.

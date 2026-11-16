# PLAN-041 멸망·결과 아트 제작 인계

2026-11-17 / [PLAN](../../PLAN-041-destruction-results-art.md) / [요청](../../../../UnityProject/Assets/Sprites/Require/PLAN-041/README.md) / [파일·리비전·해시 목록](asset-manifest.csv).

- 신규 WorldDestruction r002: [Art PLAN-031](../../../../../Art/docs/plans/PLAN-031-world-destruction.md), [원본·시트·명세](../../../../../Art/assets/ui/WorldDestruction/revisions/r002/README.md).
- 재사용 ResultFrame: 승인된 [PanelFrame r001](../../../../../Art/assets/ui/PanelFrame/revisions/r001/README.md), 원래 요청 PLAN-038-SPR-001과 최종 Assets/Sprites/UI/Frames/PanelFrame.png 공유. 새 Art PLAN/파일/리비전 없음.
- 세계 r002 사용자 확인 대기. Unity 수령/적용·정식 기술/게임 검증 모두 PLAN-042 유보. Art PLAN-031/Unity PLAN-041은 on hold.

## 확인 자료

[4초 GIF](../../../../../Art/docs/plans/reviews/PLAN-031/r002/motion.gif) · [32프레임 시트](../../../../../Art/docs/plans/reviews/PLAN-031/r002/sheet.png) · [글자 없는 결과 배치 합성](../../../../../Art/docs/plans/reviews/PLAN-031/r002/context.png).
미리보기 배경·공용 패널/버튼은 세계PNG에 포함하지 않았다. 실제 Unity 화면이 아닌 Aseprite 합성이다.
GIF는 확인 편의를 위해 반복한다. 게임에서는 Progress에 따라1회 변화 후 마지막 상태를 유지한다.

## 마지막 적용·검증 인계

- 128×128셀×32·8열×4행·1024×512시트·중심pivot·투명 배경. Multiple/Full Rect·PPU32·Point·Clamp·밉맵OFF·무압축.
- 태그 Colonization1~32, Sprite WorldDestruction_Colonization_01~32. 현재 EndingSeconds4초, 각125ms. min(31,floor(clamp01(Ending.Progress)*32)), Results=32 고정.
- 별도FPS/아트 타이머로 진행을 구동하지 않는다. 설정시간 변경 시 Duration/32, 가로세로 전환에도 같은 Progress, 새 판/세션 교체 시 이전 참조 정리.
- 현재 코드 도형 LaboratoryUiWorld의 시각 표현을 Sprite로 교체한다. 종료 진행·결과 수치·저장 오류/재시도·최소터치·스크롤·뒤 입력 차단·홈/재시작 흐름 유지.
- 결과 목록에 공용 PanelFrame을 Sliced·white Tint로 연결하고 padding12와 기존 목록폭을 함께 확인한다. 전체화면 바탕은 단색, 텍스트는 기존 TMP/Localization.
- 원본/출력 대응·알파·시간/순서·기준점·최소 표시 크기·ko/en/큰 글씨·가로세로·오류·Progress와캡션·종료1회/Results정지·새 판·참조 보존은 PLAN-042에서 함께 검증한다.
- WorldDestruction의 사용자 원본 확인 전 PLAN-042를 시작하지 않는다.

- 제작 종료 문서 검사(2026-11-16): Unity43 PLAN·active0·목록43·로컬 링크4,189 통과, git diff --check 통과. Art31 PLAN·active0·목록31·링크946 검사, 기존 완료 PLAN 링크 오류9건만 유지. 신규 문서 링크 오류 없음. 문서 검사는 정식 아트/Unity 검증이 아니며 PLAN-042 검증은 미실행.

## r002 확인 자료와 변경

[대표8시점](../../../../../Art/docs/plans/reviews/PLAN-031/r002/stages.png) · [r001/r002 비교 GIF](../../../../../Art/docs/plans/reviews/PLAN-031/r002/comparison.gif) · [최종 상태 비교](../../../../../Art/docs/plans/reviews/PLAN-031/r002/comparison.png).
소멸/균열 대신 씨앗 정착·연결 조직·군락 확산·점령 완료, 곡면 명암과 지형 개선. 파일명/ID 유지, r001 보존·원본 승인0. 기존 캡션 정합성은 PLAN-042 Localization 검토에 포함. GIF는120/130ms 교대·32프레임·합계4초를 읽기로 확인, 원본125ms. 정식 기술/게임 검증과 별개인 제작 미리보기 확인이다.

- r002 제작 종료 문서 검사(2026-11-17): Unity43 PLAN·active0·목록43·링크4,194 통과, git diff --check 통과. Art31 PLAN·active0·목록31·링크957 검사, 기존 완료 PLAN 링크 오류9건만 유지. 신규 문서 링크 오류 없음. 원본 사용자 확인 대기, 정식 아트/Unity 검증은 PLAN-042까지 미실행.

- 최신 확인(2026-11-17): 사용자 r002 외형·동작 원본 확인 완료. Unity 적용/정식 검증은 승인된 PLAN-042에서 진행한다. 원본 승인은 실제 게임 화면 최종 승인이 아니다.

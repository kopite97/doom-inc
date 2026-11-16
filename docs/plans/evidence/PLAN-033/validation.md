# r002 검증과 승인

2026-11-16 · 대상 Support r002 · Unity 6000.3.27f1 Editor, Android 대상.

## 기술 결과

| 실행 | 대상 | 결과·증거 |
| --- | --- | --- |
| run-001 Edit Mode | LaboratoryArtAssetTests | [8/8 통과](runs/run-001/editmode.json). 시트·임포트·64개 Sprite·정의/화면 재생성 후 참조와 Sprite ID 보존 |
| run-001 Play Mode | SupportArt | [1/2 통과](runs/run-001/playmode-status.json). 실제 건설·수집·운반·머리 좌표·정지·저장 복원·새 판 통과. 생산 테스트는 UI 조작 단계 실패 |
| run-002 Play Mode | SupportArtProductionDirectionsWaitingSelectionAndIcons | [1/1 통과](runs/run-002/playmode-status.json). 네 방향 생산 Work 6프레임, 재료/출력 대기 Idle, 선택 재질, 목록/배양 대상 Icon |
| run-003 Play Mode | ApprovedArt | [5/5 통과](runs/run-003/playmode-status.json). 기존 프로토타입·시설 동작, 방향 전환·개체 독립성·운반·누락 대응·저장 회귀 |

최종 고유 테스트 기준 Edit Mode 8개 + Play Mode 7개 통과, 건너뜀 0개. run-001 실패를 삭제하거나 전체 통과로 표현하지 않는다.

- 생산 테스트에서 가로 화면에 없는 Details 버튼을 찾던 조작을 기존 ExpandSelection 경로로 수정했다. 제품 변경 없이 해당 테스트만 재실행해 통과했다.
- 작성 중 테스트 파일의 LaboratoryGameViewSize 네임스페이스 누락 컴파일 오류를 using MobileGame.TestSupport로 수정했다. [Console 누적 오류 이력](runs/run-003/console-errors.json)의 1건은 이 과거 오류다. 최종 groundTruth는 compilationFailed false, Console 오류·경고 0이다.
- r002 원본과 작업 원본 SHA-256 동일, r002 제출 PNG와 Unity 최종 PNG SHA-256 동일. 프로토타입 PNG/meta/SO의 Git 변경 없음. 동적 폰트 캐시는 Editor API로 초기화하여 Git 차이 0.
- 명령: `unity command test run --mode editor --filter LaboratoryArtAssetTests --caller plugin --skill unity-cli --project-path C:/Dev/Unity/Doom_Inc/UnityProject --json`. Play Mode는 `--mode playmode --filter SupportArt` 또는 해당 개별 테스트명/`ApprovedArt`, `--async_tests true` 후 `test status`로 완료를 확인했다. 각 JSON에 실제 인수를 보존한다.

## 사용자 확인과 제한

- 원본 승인: 2026-11-16 「좋아. 수정된 모습으로 진행하자. r002를 작업 원본에 반영하고 Unity에 적용해줘.」 r002 외형 승인 및 적용 요청으로 기록한다.
- 게임 화면 승인: 2026-11-16 완료. 사용자 「좋아. 게임 화면도 확인했어. PLAN-033과 Art PLAN-009를 완료 처리하고 다음 아트 플랜 방금과 같은 플로우로 실행해줘」. [최신 가로/세로 화면과 조건](runs/run-002/README.md)을 제공한다. 테스트의 프레임/좌표 검사는 실제 속도에서의 움직임에 대한 사용자 시각 판단을 대신하지 않는다.
- Player 빌드·실기기·고부하 계측·재생 영상 녹화는 미실행. 일반 Editor 검증 범위이며 실기기 성능/터치 검증을 주장하지 않는다. 결과는 CLI JSON으로 보관하며 별도의 XML/영상 파일은 생성하지 않았다.
- Unity 문서 검사 통과, active 1개. Art 문서 검사는 기존 완료 PLAN 002~005·008의 과거 수령 링크 오류 9건이 남아 전체 실패다. 사용자 지시에 따라 완료 PLAN은 수정하지 않았다.

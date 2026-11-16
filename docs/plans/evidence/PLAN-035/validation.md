# 배양액 r001 검증

2026-11-16, Unity 6000.3.27f1 Android 대상 Editor. [실제 파일/해시](asset-manifest.csv), [시작 환경](environment-before.json).

## 리소스 준비와 검증

PNG 1장·SO Icon·CargoSupport·HUD·실제 자원 행의 전체 연결과 [임포트](runs/run-001/install.json)·[정적 참조](runs/run-001/bindings.json)를 확인한 뒤 테스트했다.

| 실행 | 범위 | 결과 |
| --- | --- | --- |
| run-001 Edit Mode | LaboratoryArtAssetTests | [10/10 통과](runs/run-001/editmode.json). 임포트·Sprite/받침·정의/화면 재생성 보존 |
| run-001 Play Mode | 최초 배양액 통합 검사 | [0/1](runs/run-001/playmode-status.json). 프로토타입을 생산에 배정한 테스트 장면 오류. 제품은 올바르게 거부 |
| run-002 Play Mode | CultureArt | [2/2 통과](runs/run-002/playmode-status.json). 보급형 실제 생산→프로토타입 운반·저장 복원·UI, 별도 실제 저장소 재배치의 부분 입고/빈손 |
| run-003 Play Mode | ApprovedArt | [5/5 통과](runs/run-003/playmode-status.json). 기존 프로토타입·시설·유기물 동작/운반·개체 독립·회귀 |
| run-004 Play Mode | 배율·운반 위치 조정 후 생산/운반/UI | [1/1 재통과](runs/run-004/playmode-status.json). 8조건 최신 캡처, 제품 변경 없음 |

최종 고유 테스트 기준 Edit 10개·Play 7개 통과. 첫 실패 원인 및 결과를 삭제하지 않았다. 생산 배정은 보급형으로 수정했으며 생산량 1은 실제 밸런스를 따른다. 부분 입고 검사는 별도로 기존 배양액 5개의 저장소 재배치에서 2개 입고·3개 잔류 후 빈손을 검증했다. 게임 밸런스는 변경하지 않았다.

- 준비 중 테스트 코드의 LaboratoryGameViewSize namespace 누락은 Foundation.Editor using 추가로 수정했다. 이후 컴파일 오류 없이 테스트를 실행했다.
- 한국어/영어 × 보통/큰 글씨 × 1280×720/720×1280의 8조건에서 HUD·자원 행·텍스트 높이·아이콘 참조를 검증하고 캡처했다.
- Art 기술 검사: [원본/PNG 차이 0·알파·8색·128프레임 운반 합성](../../../../../Art/docs/plans/reviews/PLAN-010/r001/verification.json). 원본 32px, 축소 비교 24px·16px; Unity 시각 검수와 구분한다.
- Localization 새 3개 키의 한/영 내용을 검토하고 ConfirmReview·CSV Validate·Import·ValidateImported 통과. 원래 키·저장/밸런스 계약은 보존했다.

## 재현과 제한

각 JSON에 실제 호출 인수·대상 프로젝트·실행 결과를 보존했다. 기본 명령은 `unity command test run --mode editor --filter LaboratoryArtAssetTests --caller plugin --skill unity-cli --project-path C:/Dev/Unity/Doom_Inc/UnityProject --json`이다. Play 검사는 `--mode playmode --filter CultureArt` 또는 `ApprovedArt`, `--async_tests true` 후 `test status`로 완료를 조회한다.

원본 및 실제 게임 화면의 사용자 시각 확인은 2026-11-16 완료됐다. 사용자 「배양액도 확인했어. PLAN-035와 Art PLAN-010을 완료 처리」를 최종 승인 근거로 기록한다. 기술 통과로 외형을 승인하지 않는다. Player 빌드·실기기·고부하·영상 녹화는 미실행이며 Editor 검증만 수행했다. 테스트 결과는 CLI JSON으로 보관하며 미생성 XML/영상 경로를 완료 증거로 쓰지 않는다.

[최신 화면](runs/run-004/README.md)을 검수 기준으로 사용한다. [환경·기존 원본 보존](environment.md), [최종 Console](console-final.json)을 확인했다. Unity 문서 검사 통과, Art 검사에는 기존 완료 PLAN의 링크 오류9건이 남는다.

# PLAN-043 검증 증거

2026-11-16, Windows / Unity 6000.3.27f1 / 설치된 Unity CLI 1.0.0-beta.17 / 실행 중 Editor Pipeline에서 검증했다. CLI 버전 교체·Player 빌드·실기기·성능/고부하 검증은 수행하지 않았다.

| 검증 | 결과 | 증거 |
| --- | --- | --- |
| 밸런스 도구 EditMode 최종 | 30/30 PASS | [XML](run-001/editmode-completion.xml), [CLI 결과](run-001/balance-tests-completion.json) |
| 기존 JSON/저장 호환 회귀 | 34/34 PASS | [XML](run-001/json-regression.xml), [CLI 결과](run-001/json-regression.json) |
| 다음 Play 적용·건설 시간·읽기 전용 | 동일 테스트 Domain Reload OFF/ON 각각 1/1 PASS | [OFF](run-001/play-reload-off.xml), [ON](run-001/play-reload-on.xml) |
| 창·Inspector·초안 수명 | PASS | [기능 확인](run-001/manual-checks.md), [편집창](run-001/balance-window.png), [Inspector](run-001/inspector-entry.png), [오류 안내](run-001/validation-error.png) |
| 다섯 보완사항 | PASS | [검토 기록](run-001/review-cases.md) |
| 원본·환경 복원 | 70개 파일 SHA-256 일치 | [복원 기록](run-001/restoration.md), [비교](run-001/restoration-hashes.json) |
| 최종 Editor 상태 | 컴파일 실패 없음, 현재 Console 오류/경고 0 | [조회 로그](run-001/editor.log)의 groundTruth |

서로 다른 테스트는 EditMode 64개·PlayMode 1개다. PlayMode 두 환경 실행을 서로 다른 테스트로 합산하지 않았다. 초기 26개 테스트 결과도 `balance-tests-first.json`, `balance-tests-final.json`, `editmode-results.xml`에 보존했다. 최종 30개 결과를 완료 근거로 사용한다.

run-001의 `.cs` 파일은 CLI `script eval_file`로 실행한 검증/화면 캡처 본문이며 제품 스크립트가 아니다. 화면 캡처는 Unity를 전면으로 둔 상태에서 수행하고 직접 확인했다. 실제 Editor 강제 종료 대신 미완료 journal과 재생성한 저장소로 재시작 복구를 검증했다. 신규 Sprite·아트 요청은 없다.

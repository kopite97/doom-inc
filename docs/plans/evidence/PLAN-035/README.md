# PLAN-035 배양액 r001 검수

- 2026-11-16 사용자 다음 아트 동일 흐름 실행 요청. PLAN-033/Art PLAN-009 최종 게임 화면 승인 및 완료 후 권장 순서의 PLAN-035/Art PLAN-010을 활성화했다.
- [Art 제출본·규격·원본 검사](../../../../../Art/assets/resources/CultureMedium/revisions/r001/README.md), [Unity 요청](../../../../UnityProject/Assets/Sprites/Require/PLAN-035/README.md).
- 필수 리소스 1장 r001 제작·수령·임포트·SO Icon·CargoSupport·HUD·자원 행 연결 완료. [임포트·Localization 검사](runs/run-001/install.json). 전체 참조 정적 확인을 추가 기록한 뒤 테스트를 시작한다.
- 전체 준비 판정: 2026-11-16 충족. [실제 참조](runs/run-001/bindings.json)에서 Icon 일치·받침 (16,28)·PPU32·32×32 및 GUID를 확인했다. 이후 테스트를 진행한다. 준비 중의 검사 도구 네임스페이스 누락을 수정하고 재컴파일한다.
- 원본·게임 화면 최종 승인: 2026-11-16 사용자 배양액 확인 및 PLAN-035/Art PLAN-010 완료 요청.
- 환경: [시작](environment-before.json), [기존 에셋 해시](assets-before.json). Unity 6000.3.27f1 Android 대상, Temp_Laboratory clean·Play 정지·Domain Reload OFF·background false. CLI1.0.0. Live Aseprite bridge는 다른 인스턴스의 포트 점유로 연결되지 않아 batch로 새 파일만 제작했다.
- 작은 구현 선택: 기존 ResourceDefinition.Icon/CargoSupport를 재사용했다. HUD의 두 자원 이름·수치를 별도 TMP로 배치해 언어·글씨 크기별 폭을 계산한다. UiList는 실제 자원 행에도 작은 아이콘을 표시한다. 새 패널·게임 상태·자원 전용 렌더러는 추가하지 않는다.
- Localization CSV의 새 hud_resource/hud_summary/hud_summary_paused 한국어·영어를 직접 검토하고 ConfirmReview 후 Import/ValidateImported를 통과했다. 기존 키는 보존했다.
- 완료한 기술 검증: Edit Mode 10개·Play Mode 7개 통과. [결과·최초 실패 원인](validation.md), [최신 8조건 화면](runs/run-004/README.md), [환경 복원·보존](environment.md), [실제 파일·해시](asset-manifest.csv).
- 원본·게임 화면의 최종 사용자 시각 확인을 마쳐 Unity PLAN-035와 Art PLAN-010은 completed다. 이전 PLAN-033/Art PLAN-009는 사용자의 게임 화면 확인에 따라 completed다.
- Unity 문서 검사 통과. [Art 문서 검사](art-doc-validation.txt)는 이전 완료 PLAN의 기존 링크 오류9건만 남으며, 사용자의 과거 지시에 따라 완료 문서를 수정하지 않았다.

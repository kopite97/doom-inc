# PLAN-033 보급형 r002 적용 검수

- Art [PLAN-009](../../../../../Art/docs/plans/PLAN-009-support-creature.md), [r002 제출 명세](../../../../../Art/assets/characters/Support/revisions/r002/README.md).
- 2026-11-16 사용자: 「수정된 모습으로 진행하자. r002를 작업 원본에 반영하고 Unity에 적용해줘」. r002 원본 외형 승인·적용 요청으로 기록하며 실제 게임 화면 승인은 별도다.
- 작업 원본을 r002로 동기화했다. 기존 디스크 원본은 Art `docs/plans/reviews/PLAN-009/r002/Support-before-sync.aseprite`에 보존했다. 열린 Aseprite의 메모리상 탭은 변경하지 않았다.
- 수령 경로 `Assets/Sprites/Require/PLAN-033/PLAN-033-SPR-001/r002/Support.png`에서 Unity AssetDatabase로 최종 `Assets/Sprites/Creature/Support/Support.png`에 이동했다.
- 전체 준비 판정: 2026-11-16 충족. 필수 시트 1장·64프레임·12태그 대응, 보급형 본체/목록/배양 대상 Icon·머리 좌표 연결 완료. [임포트](runs/run-001/install.json), [연결 확인](runs/run-001/bindings.json). 이후부터 테스트를 실행한다.
- 프로토타입 직렬화 필드·스프라이트는 보존하고 추가 생명체 세트를 정의 ID로 조회한다. 재생 상태는 기존 ThingArt가 소유한다. 생산은 실제 작업 단계·접근 완료·생산 가능 여부를 함께 확인한다.
- 적용 PNG SHA-256 `6a5cc62a095cadb869a1226c7d89acc227c605d0b4470724e46d5f3da53340a9`. PPU 32, Point, FullRect, 중앙 피벗, 무압축, 512×512, 8×8, 프레임 64×64.
- 실행 환경: Unity 6000.3.27f1, Android 대상 Editor. 작업 전 Unity 미실행에서 시작했으며 게임 저장 파일을 변경하지 않는 격리 테스트를 사용한다. CLI 1.0.0으로 갱신했다.
- 기술 검증 완료: Edit Mode 8개, Play Mode 7개 통과. [검증 결과](validation.md), [환경과 복원](environment.md), [실제 파일 목록](asset-manifest.csv).
- 최신 화면은 [run-002](runs/run-002/README.md)의 [가로](runs/run-002/screenshots/landscape.png)·[세로](runs/run-002/screenshots/portrait.png)다. 2026-11-16 사용자가 게임 화면 확인 및 두 PLAN 완료를 요청했다. PLAN-033과 Art PLAN-009는 completed다.

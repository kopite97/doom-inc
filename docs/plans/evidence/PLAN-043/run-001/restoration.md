# 원본과 환경 복원

- 시작 시 JSON 5개, SO 에셋, 씬 및 EditorSettings 등 70개 파일의 SHA-256을 [baseline-hashes.json](baseline-hashes.json)에 기록했다. 종료 비교 [restoration-hashes.json](restoration-hashes.json)은 70/70 일치다. 기존 사용자 변경을 Git HEAD로 되돌린 것이 아니라 작업 시작 상태와 비교했다.
- Gathering buildSeconds만 창에서 3→6 저장해 다음 Play 적용을 검증하고 동일 창에서 3으로 복원했다. targetScore=10 오류 확인 초안은 저장 없이 폐기했다. 제품 JSON 기본 수치는 변경되지 않았다.
- 실제 AppConfiguration·SO·씬 참조는 자동 수정하지 않았다. Domain Reload는 원래 `enterPlayModeOptionsEnabled=true`, `DisableDomainReload`로 복원하고 설정을 저장했다. 최종 Play 정지 상태다.
- EditMode 저장 실패 검증은 OS 임시 폴더 `DoomBalanceTests-<GUID>`의 JSON 복제본을 사용하고 테스트 종료 시 정리했다. Play 테스트의 진행 저장도 격리 경로를 사용했다. 사용자 진행 저장 삭제·호환 해시 상수 변경은 하지 않았다.
- 정상 완료된 도구 저장 journal은 Library/BalanceEditor/Recovery 아래 완료 상태로 남고 원본/후보 snapshot은 정리된다. 미완료 제품 저장 journal은 없다. Library는 Git 산출물이 아니다.
- 신규 `.meta`는 Unity가 생성했다. 기존 GUID·씬·SO·EditorSettings 원본을 유지했다. 프로젝트의 기존 미커밋 변경은 보존했다.

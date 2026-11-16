# 환경 보존

작업 전 상태: [파일 해시/dirty 목록](before-state.json), [저장 경로 백업 목록](save-before.json). 이번 작업 이전 Support/Culture 등 미커밋 변경을 유지했다. Git commit/push는 하지 않았다.

Unity6000.3.27f1/Pipeline0.9.1-exp.1/CLI1.0.0. Editor 일반 테스트만 수행했다. 테스트별 임시 저장 슬롯·입력 장치를 사용했고 제품 저장 슬롯을 새 판으로 덮어쓰지 않았다.

복원 후 Play 정지, 원래 Temp_Laboratory 씬1개 활성, Domain Reload 비활성 옵션 유지, runInBackground=false. 테스트용 background override 해제. Auto-tick은 원래 활성 상태이며16ms 세션 설정으로 유지했다. GameView 크기/테스트 입력 장치는 기존 테스트 scope가 복원한다.

persistentDataPath의 사전 파일3개 중 TestResults.xml은 테스트 러너가 갱신한 결과 파일로, 이번 XML/JSON을 별도 증거에 보존한 뒤 원래 백업으로 복원했다. 나머지2개는 Analytics 파일이며 모두 해시 일치. ProjectSettings 전체 사전 파일 해시 일치. [복원 검증](preservation-check.json).

기존 승인 Sprite PNG/.meta는 모두 작업 전과 동일하며, 변경은 이번에 교체해 참조0건을 확인한 임시 이미지8개와 그 meta의 삭제뿐이다. 원본/revision Aseprite는 읽기 전용 기술 검사만 수행했다.

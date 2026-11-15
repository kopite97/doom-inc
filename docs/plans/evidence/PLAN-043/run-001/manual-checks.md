# Editor 기능 확인

- 메뉴로 단일 밸런스 창을 열고 시설별 수치·한국어 이름/단위/설명·저장 값·초안 값을 확인했다. 역할 조건에 맞는 필드만 입력 가능하다. 목록은 ID와 수치 요약이며 검색은 ID/표시 이름 키를 지원한다.
- 실제 창의 Gathering 건설 시간 입력을 3→6으로 변경했다. Undo로 초안 0개, Redo로 6 복구를 확인했다. `EditorUtility.RequestScriptReload()` 후 같은 초안이 유지됐다.
- Support·Resource·Gathering SO로 이동해도 기존 초안이 유지됐다. 다중 SO Inspector 버튼은 비활성, Resource 화면은 수치 입력이 없는 조회 전용이었다. 미등록 ID의 임시 SO는 진입을 거부했다. 임시 SO는 즉시 해제했다.
- 기존 Inspector의 Definition Id/Icon/Prefab을 보존했다. 실제 Inspector 버튼 클릭으로 `facility.gathering` 상세가 열린 것을 확인했다([화면](inspector-entry.png)).
- 저장 버튼 경로로 건설 시간 6을 저장하고 실제 카탈로그가 6을 읽는 것을 확인했다. Play 테스트는 새로운 GameDataStore와 실제 AppConfiguration을 사용하여 GameFlow 건설 명령을 실행하고 시간 경계 전/후 완료 상태를 검사했다. 사용자 터치 플레이 대신 자동 GameFlow 진행 검증이다.
- 같은 Play 테스트에서 창의 수치 입력 비활성·저장 차단을 확인했다. Domain Reload OFF와 ON에서 각각 통과했다. 테스트 후 창에서 6→3을 저장해 복원했다.
- 침공 targetScore 120→10 초안 검증 시 현재 detailedUntilScore=60과 두 필드/값/원인이 표시됐다([화면](validation-error.png)). JSON은 저장되지 않았다.
- dirty 창을 닫으면 Unity Save/Discard/Cancel 안내가 표시됐다. Cancel로 창/초안 유지, 다시 닫고 Discard로 폐기·닫힘을 실제 Windows 조작으로 확인했다. 재열기 후 변경 0개였다.
- 표시 정보가 아닌 초안 서명 규칙 수정으로 재컴파일했을 때 기존 초안은 보존되고 충돌 안내/입력 차단이 나타났다. 원본은 변경되지 않았다.
- 사용 안내는 Assets/JSON/Balance/README.md에 기록했다. 최종 상태는 Play 정지·미저장 초안 없음이다.

초기 컴파일에서 Newtonsoft와 Localization의 중복 타입이 충돌했다. 새 Editor 도구의 JsonConvert 의존을 제거하고 토큰 문자열 디코딩/복구 journal을 분리한 뒤 컴파일과 테스트가 통과했다. 최종 로그의 과거 이벤트와 현재 groundTruth(오류 0)를 구분한다.

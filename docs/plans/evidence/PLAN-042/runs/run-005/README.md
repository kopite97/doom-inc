# run-005 — 2026-11-17

화면 캡처에서 발견한 ScrollThumb 최초 배치 문제 수정. 패널 크기 변경 즉시 갱신과 가로/세로 우측 정렬 assertion 추가. Play6/6 통과. 최종 화면16장 보존.

Unity 6000.3.27f1 Editor / CLI1.0.0 / Pipeline0.9.1-exp.1 / Windows. 별도 임시 저장 슬롯·테스트용 입력 장치, 일반 기능 시나리오. 고부하/Player/콘텐츠 빌드 제외.

## 종료 로그

`logs/editor.log`는 작업 종료 시 Editor.log 마지막2000줄이며 전체 실행 로그가 아니다. `logs/console-errors.json`은 누적 오류 이력이다. 앞선 작업의 컴파일 오류2건·이번 작성 중 수정된 컴파일 오류3건·설정 직렬화 시도에서 나온 도구 오류3건을 포함한다. 후자는 persistent 객체에 대한 InternalEditorUtility 저장이 거부된 것으로, 디스크 설정은 작업 전 바이트/해시로 별도 복원 확인했다. 미해결 게임 오류로 집계하지 않는다. 마지막 테스트의 예기치 않은 로그는 NUnit/LogAssert 결과로 확인한다. 로그를 보존한 뒤 콘솔을 비웠으며 최종 상태는 `logs/console-final.json`이다.

# 2026-11-10 미사용 Sprite 정리

사용자가 승인된 아트 적용 후 미사용 Sprite 제거와 main 푸시를 요청했다. 완료된 PLAN-026의 독립 Sprite 후속 정리이며 PLAN-026은 completed, PLAN-027은 proposed, active는 0개로 유지한다.

## 삭제 범위

- 정식 Build로 교체된 `Assets/Sprites/UI/Laboratory/Temp_Build.png` 1장.
- Require의 바닥 r001/r002 각 4장, Gathering r001/r002 각 1장, PrototypeSupport r001 1장: 총 11장.
- 위 PNG 12장의 메타데이터와 비어 있는 요청·리비전 폴더 15개의 메타데이터를 AssetDatabase.DeleteAsset으로 함께 정리했다. 요청 README는 유지한다.
- 이전 리비전 11장은 Art의 각 revisions/rNNN/export 보관본과 SHA-256이 일치한다. Art의 PNG·Aseprite 원본과 제출 기록은 유지한다.
- 남은 정식 PNG 5장과 사용 중인 임시 UI 17장은 유지한다. 외부 패키지의 샘플 리소스는 이번 삭제 범위에 포함하지 않는다.

## 참조·기술 검증

- [삭제 전 GUID·참조·보관본 해시](reference-audit-before.json): 모든 프로젝트 Sprite를 조사하고 삭제 후보의 Assets/ProjectSettings 참조가 0개임을 확인했다. 이름으로 파일을 읽는 생성 도구도 정식 Build를 우선하며 Require를 로드하지 않는다.
- [Editor 삭제 명령](delete-unused.cs.txt)과 [실행 결과](delete-result.json): 삭제 전 AssetDatabase.GetDependencies로 전체 Assets의 의존성을 추가 검사했고 후보를 사용하는 에셋은 0개였다.
- [삭제 후 정적 검사](static-verification.json): 삭제 PNG/meta 잔존 0, 삭제 GUID 참조 0. 남은 22 PNG의 픽셀 해시와 GUID가 삭제 전과 동일하다.
- Unity 6000.3.27f1, Android 선택, CLI 1.0.0-beta.17에서 `test run --mode editor --filter LaboratoryArtAssetTests --async_tests true` 실행. [결과](edit-status.json)는 **6/6 통과**, 실패·건너뜀 0이다. 정식 106 Sprite의 임포트·프레임·앵커와 생성 도구의 참조 보존을 검증했다. Temp_Build도 다시 생성되지 않았다.
- 첫 테스트 요청 시 기존 Editor가 종료되어 [EDITOR_NOT_RUNNING](edit-run.json)으로 실행되지 않았다. 같은 버전·프로젝트를 다시 열고 [재실행](edit-retry-run.json)하여 위 결과를 얻었다.
- 종료 시 컴파일 실패·진행 없음, 콘솔 오류·경고 0. 이번 정리에서는 Play Mode·Android 빌드를 다시 실행하지 않았다. 기존 게임 화면 승인과 통합 검증은 [적용 기록](../integration-2026-11-10/README.md)을 따른다.

Unity 실행마다 `--project-path C:/Dev/Unity/Doom_Inc/UnityProject --caller plugin --skill unity-cli --format json`을 지정했다. 문서 링크는 삭제된 Unity 수령 복사본 대신 Art 보관본으로 연결했다. 새 Sprite 제작 요청은 없다.

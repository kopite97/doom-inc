# PLAN-032 검증 증거

2026-11-15 구현·검증, 2026-11-16 완료 기록. [PLAN-032](../../PLAN-032-minimal-facility-progress-bars.md)의 작은 읽기 전용 시설 진행 바 구현·검증이다.

## 구현과 범위

- 시설마다 36×4 UI 단위 Track과 내부 34×2 Fill만 표시한다. 시설 상단에서 2 UI 단위 떨어지며 앵커를 화면 픽셀에 맞춘다. 지도 줌·선택·큰 글씨로 크기를 바꾸지 않는다.
- 정상 채움은 민트 RGBA(0.38, 0.88, 0.70, 1), 대기는 주황(0.95, 0.68, 0.30, 1)이다. Track은 정상(0.08, 0.11, 0.14, 0.9), 대기(0.34, 0.23, 0.10, 0.95)다. 0% 대기는 채움을 만들지 않고 Track으로만 보인다.
- 아이콘·Caption·카드·연결선·빈자리 탐색·재탭 확장·표식 선택 판정과 두 ON/OFF UI를 제거했다. 남은 지도 도구 3개에 맞춰 툴바 폭을 줄였다.
- 기존 FacilityStatusQuery를 그대로 사용한다. 진행·대기는 색으로 구분하고 산출물 회수 대기는 기존 100%를 표시한다. 완공 저장소는 HasProgress=false여서 바가 없으며 건설 중에는 표시한다.
- ShowFacilityStatus 저장 필드와 기존 SetUiPreferences API는 호환을 위해 남겼다. 저장된 OFF는 렌더링에 영향을 주지 않는다. 상태 ON/OFF 알림도 제거했다. 기존 아이콘 enum·공용 에셋·미사용 번역 키는 직렬화/공유 참조 보존을 위해 일괄 삭제하지 않았다.
- 새 Shader·Sprite·외부 에셋·범용 UI 시스템은 없다. 표시 책임은 기존 MapView, 상태 원본은 도메인에 유지한다.

## 환경과 결과

Unity 6000.3.27f1 / URP 17.3 / Windows Editor / Android 활성 타깃. 기존 Unity CLI 1.0.0-beta.17 / Pipeline 0.9.1-exp.1 연결을 사용했다. 아래 최종 통과 실행은 73회, 서로 다른 테스트는 68개다. 중복 실행과 Device 필터에 포함된 신규 입력 테스트를 구분한 수치다.

| 모드 | 필터 | 결과 | 로그 |
| --- | --- | --- | --- |
| Edit | FacilityStatusQueryTests | 12/12 | [상태 조회](status-query-tests.json) |
| Edit | UiSettingsStoreTests | 16/16 | [설정 저장 호환](settings-tests.json) |
| Edit | LocalizationCsvTests | 13/13 | [CSV](localization-tests.json) |
| Play / Domain Reload OFF | FacilityProgress category | 3/3 | [신규 표시·입력·수명](progress-tests.json) |
| Play / OFF | Device | 15/15 | [실제 입력 회귀](device-tests.json) |
| Play / OFF | SelectionOutline category | 3/3 | [외곽선·선택 해제](selection-tests.json) |
| Play / OFF | PlacementPreview category | 3/3 | [미리보기·드래그·오류](placement-tests.json) |
| Play / OFF | LanguageChangePreservesPreviewSessionNoticeAndSavedLanguage | 1/1 | [언어·미리보기 보존](language-tests.json) |
| Play / OFF | FacilityAnimation category | 3/3 | [시설 생산·회전·건설·이동](facilities-tests.json) |
| Play / Domain Reload ON | FacilityProgress category | 3/3 | [ON 환경](progress-domain-on-tests.json) |
| Play / OFF | FacilityArtKeepsConstructionMoveAndReadableOverlays | 1/1 | [도움말 변경 후 화면 재확인](menu-final-tests.json) |

- 신규 검증은 36×4 치수, 이미지 2개·RaycastTarget=false·텍스트 없음, 고정 크기·픽셀 앵커·인접 배치, OFF 무시, 시설 4종의 0/부분/100%, 진행/대기 색, 완공 저장소 제거·시설 삭제·세션 교체를 확인한다.
- 바 위 탭·재탭은 아래 빈 땅 선택 해제로 전달되고, 바에서 시작한 드래그는 지도를 이동한다. 신규 건설과 시설 이동 중 같은 위치의 탭은 후보 좌표만 바꾼다. 기존 선택 외곽선·큰 글씨·핀치·설정 저장·언어 전환은 회귀 필터로 확인했다.
- Device는 Editor 입력 테스트 이름이며 실제 기기 실행이 아니다. 빌드·실기기·고부하·성능 계측은 실행하지 않았다. 일반 배치의 표시 품질을 확인했으며 임의로 많은 시설이나 모든 기기 밀도에 대한 성능 보장을 뜻하지 않는다.

## 실제 화면

최종 신규 ON 실행의 Game View 캡처 8장과 도움말 수정 후 영어 화면 1장을 [screens](screens)에 보관했다.

| 확인 대상 | 화면·관찰 |
| --- | --- |
| 0%·부분 진행·대기 | [가로 4종 시설](screens/partial-and-waiting-1280x720.png): 작은 Track과 민트/주황 Fill, 0% Track만 표시 |
| 산출물 회수 대기 | [100% 주황](screens/output-waiting-1280x720.png): 진행량 보존, 큰 카드 없음 |
| 일반 인접 배치 | [가로](screens/fit-1280x720.png), [세로](screens/fit-720x1280.png): 19개 진행 대상의 바가 시설 상단에서 겹치지 않음 |
| 작은 화면 | [640×360](screens/fit-640x360.png): Canvas 배율을 따르는 얇은 바가 식별됨. 실제 기기 밀도 검증은 아님 |
| 최대 줌·선택 상세 | [세로 8배 확대](screens/selected-zoom8-720x1280.png): 바는 같은 크기, 본체 외곽선·상세 정보 유지, 뷰포트 밖 시설은 잘림 |
| 영어·큰 글씨·지도 경계 | [영어 세로](screens/english-large-portrait.png): 바 크기 유지, 완공 저장소 바 없음, 메뉴 도움말은 남은 세 조작만 안내 |

## 수정 이력·환경 복원

- [초기 신규 Play](progress-initial-tests.json)는 2/3 통과였다. 최대 줌에서 시설 전체 높이가 뷰포트보다 커 상단 바가 화면 밖에 있었는데 시설 중심을 비추는 테스트가 바를 요구했다. 테스트 카메라가 시설 상단 앵커를 비추도록 수정했다. 바를 화면 가장자리로 강제 이동시키지는 않았다. 최종 OFF/ON은 모두 통과했다.
- 개발 중 제거한 테스트 공용 로그 헬퍼 참조와 비공개 카메라 속성 접근의 컴파일 오류를 수정했다. [최종 컴파일](compile.json)은 failed=false다.
- 마지막 실제 화면에서 삭제된 눈 버튼을 안내하는 map_help를 발견해 ko/en 원문을 수정했다. [변경 미리보기](localization-preview.json)에서 해당 키만 확인하고 개별 en 검토 후 [가져오기](localization-import.json)로 원본·테이블 일치를 확인했다. CSV 13/13 및 영향 화면 1/1을 재검증했다.
- 이전 표식 ON/OFF 비용 비교 테스트는 없어진 기능을 전제로 하므로 신규 상태/수명 검증으로 대체했다. 과거 증거는 보존한다. 역사적 Plan020Probe의 status-off 설정 호출도 이제 표시를 끄지 않으므로 종전 ON/OFF 성능 비교 의미로 사용할 수 없다.
- [최종 환경](final-environment.json): Play 정지, 컴파일 종료, 원래 Domain Reload OFF(enabled=true/options=1), runInBackground=false, Localization 원본·테이블 일치. [최종 Console](final-console.json)의 현재 groundTruth 오류·경고 0, compilationFailed=false다.
- 테스트 전 백업한 동적 폰트와 EditorSettings를 복원하고 [해시](restored-files.json) 일치를 확인했다. 작업 전부터 있던 PLAN-030/031 변경과 폰트 변경은 보존했다.

## 재현

공통 CLI 인자는 `--caller plugin --skill unity-cli --project-path C:/Dev/Unity/Doom_Inc/UnityProject --json`이다. Edit는 `unity command test run --mode editor --filter FacilityStatusQueryTests`, Play는 `--mode playmode --filter FacilityProgress --filter_type category --async_tests true`로 실행하고 `test status`의 completed·개별 테스트 이름을 확인한다. 이름 필터는 testName이다. `*-start.json`은 접수 응답이며 완료 증거가 아니다.

2026-11-16 완료 문서 검사 `node scripts/validate-agent-docs.mjs`는 PLAN 32개·목록 32개·active 0개·로컬 링크 3619개 PASS다. `git diff --check`는 공백 오류가 없다. 신규 Sprite 요청은 없다.

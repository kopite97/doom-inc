# PLAN-031 검증 증거

2026-11-15, [PLAN-031](../../PLAN-031-placement-preview-drag-and-validation.md)의 신규 건설·시설 이동 미리보기 개선 검증이다.

## 환경과 실행 범위

- Unity 6000.3.27f1 / URP 17.3 / Windows Editor / Android 활성 타깃. 기존 Unity CLI 1.0.0-beta.17 / Pipeline 0.9.1-exp.1 연결을 사용했다.
- 기존 Domain Reload OFF에서 Edit Mode 46개·Play Mode 25개가 통과했다. 신규 PlacementPreview 3개는 Domain Reload ON에서도 3/3 통과했다. 서로 다른 테스트는 71개, 최종 통과 실행은 74회다.
- `Device`는 Editor 입력 테스트의 이름 필터이며 실제 모바일 기기 실행이 아니다. 빌드·실기기·고부하·성능 계측은 실행하지 않았다.

## 최종 테스트 결과

| 모드 | 필터 | 결과 | 로그 |
| --- | --- | --- | --- |
| Edit | GridMapTests | 19/19 | [공간 판정](grid-tests.json) |
| Edit | LaboratoryGestureTests | 14/14 | [제스처](gesture-tests.json) |
| Edit | LocalizationCsvTests | 13/13 | [CSV](localization-tests.json) |
| Play / OFF | PlacementPreview (category) | 3/3 | [신규 배치](placement-tests.json) |
| Play / OFF | Device (testName) | 15/15 | [실제 입력 회귀](device-tests.json) |
| Play / OFF | SelectionOutline (category) | 3/3 | [선택 강조·해제](selection-tests.json) |
| Play / OFF | FloorArtKeeps | 1/1 | [바닥·미리보기](floor-tests.json) |
| Play / OFF | LanguageChangePreservesPreviewSessionNoticeAndSavedLanguage | 1/1 | [언어 전환 중 후보 보존](language-tests.json) |
| Play / OFF | RealStartBuildsCollectsAndStoresWithAutomaticSaveAndMidConstructionContinue | 1/1 | [건설·저장·재개](construction-flow-tests.json) |
| Play / OFF | SceneAndHarnessShareConstructionSettingsAndPlacementRules | 1/1 | [화면·하네스 공통 규칙](harness-tests.json) |
| Play / ON | PlacementPreview (category) | 3/3 | [Domain Reload ON](placement-domain-on-tests.json) |

- 신규 Play 테스트는 시설 4종의 중앙 초기화, 화면 밖 기존 시설의 원위치 미리보기, 가로/세로·마우스/터치 드래그, 잡은 지점 유지, 드래그 종료 후 미확정 상태를 검증한다.
- UI 위에서 시작·통과·놓기, 화면 밖으로 이탈 후 복귀, 건물 잡기에서 두 손가락 핀치로 전환, 핀치 후 남은 손가락 차단, 빈 지도 드래그·입력 취소·화면 회전·화면 재진입을 확인했다.
- 문제 칸 진단은 네 방향의 직사각형, 여러 시설 겹침과 영역 밖 동시 발생, 이동 대상 자기 제외, 비기하 오류 시 이전 칸 제거를 검사한다. 기존 통로 차단 허용·생명체 겹침·이동/취소 규칙도 공간 테스트에 포함된다.
- 건설/이동·ko/en·가로/세로·큰 글씨 조합에서 구체적인 오류 문구, 문제 칸, 확정 불가, 네 번 회전 및 유효 위치 복귀를 확인했다. 최종 확정은 기존 명령의 재검사를 사용한다.

## 실제 화면

최종 Domain Reload ON 테스트가 캡처한 Game View PNG 28장을 [screens](screens)에 보관했다. 테스트는 실제 플레이 씬과 격리된 저장 경로를 사용한다.

| 확인 대상 | 대표 화면·관찰 |
| --- | --- |
| 건설 선택 직후 | [세로 저장소](screens/initial-storageId-720.png), [가로 배양시설](screens/initial-cultivationId-1280.png): 추가 지도 클릭 없이 보이는 중앙에 후보 표시 |
| 드래그 후 위치 | [신규 건설](screens/build-drag-1280.png), [기존 시설 이동](screens/move-drag-1280.png): 후보만 이동하고 기존 시설은 확정 전 원위치 유지 |
| 부분 충돌 | [한국어 세로](screens/build-overlap-ko-720.png), [영어 가로](screens/move-overlap-en-1280.png): 충돌한 칸만 빨강, 나머지 점유 칸은 옅은 초록, 이유와 확정 불가 표시 |
| 영역 밖 | [영어 가로](screens/build-outside-en-1280.png), [한국어 세로](screens/move-outside-ko-720.png): 보이는 영역 밖 칸을 표시하고 경계 오류를 안내 |
| 뷰포트 경계 | [확대된 이동 후보](screens/move-drag-720.png): 후보가 화면 가장자리에 걸리면 지도 뷰포트에서 잘리며 UI 영역을 덮지 않음 |

- 대표 화면에서 본체·칸 구분·기존 선택 외곽선·회전 방향·안내 문구를 확인했다. 큰 글씨의 오류 문구는 텍스트 영역 안에 들어오는지 자동 단언도 적용했다.
- 화면 가장자리 자동 스크롤은 제외 범위다. 크게 확대한 상태에서 건물을 가장자리로 끌면 일부가 잘릴 수 있으며 빈 지도 드래그·핀치·줌으로 시점을 조절한다.
- 신규 Sprite 제작·아트 교체는 없다. 이 화면 확인은 development 기능의 기술 검증이며 별도 아트 단계의 사용자 시각 승인으로 간주하지 않는다.

## Localization·컴파일·환경 복원

- CSV의 `placement_invalid`에 reason 인수를 추가하고 `placement_outside`, `placement_overlap`, `placement_unavailable`, `placement_move_valid`를 추가했다. 한국어/영어 내용과 인수 계약을 검토한 뒤 해당 키의 en 검토 기록을 명시적으로 갱신했다.
- [CSV 가져오기](localization-import.json)는 미리보기·개별 검토·ImportAll·ValidateImported를 거쳐 `Localization: no changes.`를 확인했다. 생성 테이블은 Editor API로 저장했으며 직접 편집하지 않았다.
- [컴파일](compile.json)은 completed, failed=false다. [최종 Console](final-console.json)의 현재 groundTruth 오류·경고는 0이며 compilationFailed=false다. 누적 telemetry에는 개발 중 수정한 초기 오류가 남을 수 있다.
- [최종 환경](final-environment.json): Play 정지, 컴파일 종료, Domain Reload OFF(enabled=true/options=1), runInBackground=false, CSV/테이블 일치 확인.
- 테스트 전에 백업한 동적 폰트 캐시와 EditorSettings를 복원했다. [해시 비교](restored-files.json)에서 작업 전 파일과 일치한다. PLAN-030 등 작업 전부터 존재한 폰트 변경은 보존했다.

## 초기 실패와 재실행

- 신규 Edit 테스트의 NUnit 단언 구문 컴파일 오류를 수정했다. 첫 공간 테스트는 잘못된 1×1 fixture의 작업 위치로 4건 실패했고, 다음 준비 데이터는 중복 정의 ID로 4건 실패했다. [첫 결과](grid-initial.json), [ID 오류 결과](grid-fixture-id.json)를 보존했다. 유효한 2×1 장애물 정의를 별도 ID로 공유하도록 수정 후 19/19 통과했다.
- [첫 신규 Play 실행](placement-initial-tests.json)은 2/3 통과였다. 핀치 후 빈 지도 드래그 테스트가 뷰포트 밖 좌표에서 시작해 지도 이동이 없었다. 뷰포트 내부 좌표로 수정하고 UI 시작·경계 이탈/복귀 사례를 추가한 최종 OFF/ON 실행은 모두 통과했다.
- [잘못된 필터 인자](rejected-filter.json)의 `filter_type=name`은 서버에서 거절되어 테스트가 실행되지 않았다. 해당 호출 뒤 조회된 이전 결과는 통과로 집계하지 않는다. `testName`으로 고친 device/floor/language 최종 실행 로그를 위 표에 연결했다.

## 재현

- 열린 Editor를 사용한다. 공통 CLI 인자는 `--caller plugin --skill unity-cli --project-path C:/Dev/Unity/Doom_Inc/UnityProject --json`이다.
- `unity command test run --mode editor --filter GridMapTests`처럼 Edit 필터를 지정한다. Play는 `--mode playmode --filter PlacementPreview --filter_type category --async_tests true`를 사용하고 `test status`의 completed와 개별 테스트 이름·결과를 확인한다. 이름 필터는 `testName`이다.
- `*-start.json`은 비동기 접수 응답이며 완료 증거가 아니다. 완료 결과는 표의 `*-tests.json`이다. 캡처 원본 생성 위치는 `UnityProject/Temp/PLAN-031/screens`다.
- 완료 문서 검사 `node scripts/validate-agent-docs.mjs`는 PLAN 31개·목록 31개·active 0개·로컬 링크 3557개 PASS, `git diff --check`는 공백 오류 없음이다. Editor 통과를 Player 셰이더·입력·기기 성능 검증으로 확대하지 않는다.

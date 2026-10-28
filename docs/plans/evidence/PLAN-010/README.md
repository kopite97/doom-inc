# PLAN-010 실행 증거

- 실행일: 2026-10-28. [PLAN-010](../../PLAN-010-invasion-activity-and-reports.md) 사용자 승인과 검토 보완 두 항목의 구현·검증.
- Windows PC, Unity 6000.3.27f1·URP·Android 대상의 기존 단일 Editor, CLI 1.0.0-beta.12·Pipeline 0.9.1-exp.1.
- [TECH-007](../../../decisions/TECH-007-invasion-time-and-save-boundaries.md): 상태 소유·시간 분할·실제 성과·보고 선별·저장 v4와 종료 단계 이관.
- 테스트 파일은 OS 임시 폴더 `MobileGame-PLAN010-*`와 기존 테스트별 격리 슬롯을 사용한다. 실제 `Gameplay/session.json`을 테스트로 덮어쓰지 않는다.

## 실행 결과

| 실행 | 결과 | 증거 |
| --- | --- | --- |
| 첫 컴파일·씬 업그레이드 | 컴파일 완료·실제 씬에 Invasion SO 연결 | [컴파일 요청](2026-10-28-compile-1.json), [메뉴](2026-10-28-upgrade-scene.json) |
| 기존 Edit 첫 회귀 | 163개 중 132 통과·31 실패, 건너뜀 0 | [JSON](2026-10-28-edit-1.json), [XML](2026-10-28-edit-1.xml) |
| 저장 보완·신규 검증 후 Edit | XML 기준 200개 중 199 통과·1 실패, 건너뜀 0. CLI 동기 응답은 60초 제한으로 timeout | [요청/응답](2026-10-28-edit-2.json), [실제 XML](2026-10-28-edit-2.xml), [컴파일 완료](2026-10-28-compile-2-status.json) |
| 최종 컴파일·Edit | 201/201 통과·실패/건너뜀 0 | [컴파일 완료](2026-10-28-compile-3-status.json), [비동기 시작](2026-10-28-edit-3-start.json), [완료 JSON](2026-10-28-edit-3-status.json), [XML](2026-10-28-edit-3.xml) |
| 첫 Play | 8개 중 7 통과·1 실패·건너뜀 0 | [시작](2026-10-28-play-1-start.json), [완료](2026-10-28-play-1-status.json), [XML](2026-10-28-play-1.xml) |
| 최종 Play | 8/8 통과·실패/건너뜀 0 | [컴파일 완료](2026-10-28-compile-4-status.json), [시작](2026-10-28-play-2-start.json), [완료 JSON](2026-10-28-play-2-status.json), [XML](2026-10-28-play-2.xml) |
| 최종 콘솔·Editor | 오류/경고 0, Play 종료·컴파일 완료·runInBackground=false | [콘솔](2026-10-28-final-console.json), [Editor](2026-10-28-final-editor.json) |

첫 회귀의 원인은 null 침공 객체를 JsonUtility가 빈 중첩 객체로 복원해 구버전 확장으로 오인한 것, 새 경과 시계의 double 왕복 정밀도, 종료 상태의 저장 재시도 시간 처리와 기존 테스트의 수동 목표·구버전 fixture였다. 실제 확장 구분자·새 시계의 invariant 왕복 문자열을 추가하고 저장 재시도를 유지했다. 실제 연구소의 목표 검사는 진단 신호 대신 실제 활동 성과로 전환했다.

두 번째 실행의 유일한 테스트 실패는 목표 자동 저장을 복원한 뒤 기존 생산 진행도 `1.0000000000000007`과 `1.0000000000000009`를 JSON 전체 문자열로 비교한 데서 발생했다. 새 활동 스냅샷은 정확히 비교하고, 기존 생산 시간은 1e-8 허용 오차로 검증한다. 성과·회차·보고·목표 단계와 자동 저장 횟수 단언은 유지했다.

동기 테스트 뒤 호출한 [status 응답](2026-10-28-edit-2-status.json)의 7/7은 앞선 Play 비동기 실행의 잔류 상태다. 이 응답을 이번 Edit 증거로 사용하지 않는다. 두 번째 Edit는 당일 갱신된 XML의 실제 실행 수·실패·기간으로 판정했다. 후속 전체 실행은 async_tests로 요청하고 해당 실행의 완료 응답을 확인한다.

첫 Play 실패는 UI와 하네스에서 각각 생산한 새 자원 묶음의 GUID까지 동일해야 한다는 테스트 비교 때문이었다. 기존 침공형 ID·활동·성과·보고는 정확히 비교하고, 독립 생성 자원은 정의/보관처별 수량과 시설 진행을 비교하도록 수정했다. 또한 화면 점검에서 기존 모델 표시명 `침공형 (방출 대기)`가 외부 보고에도 붙는 것을 확인해 공통 표시명을 `침공형`으로 정리했다. 정의 ID·GUID·기능 수치는 보존했다.

## 검증 범위와 재현

| 경계 | 검증 코드 |
| --- | --- |
| 전원 방출·동일 Thing/ID·기존 대상 제외·다음 출현 대기 | `ReleaseAllRetainsIdentityAndExcludesOtherModelsAndFutureBirths` |
| 편집/잘못된 대상 거부·부분 방출 없음 | `InvalidTargetAndEditingRejectWithoutPartialRelease` |
| 목표 자동 저장 순서·남은 delta 제거 | `GoalCommitsActivityBeforeAutomaticFileAndConsumesOnlyTimeToBoundary` |
| 목표 직전/초과·동시 완료·큰/분할 delta | `JustBeforeActivityBoundaryDoesNotAwardOrStopEarly`, `LargeAndSplitDeltaReachSameBoundaryAndSimultaneousBatchIsComplete` |
| 건설/이동/회전 편집·목표 후 취소·앱 정지/종료 | `EditingPausesOnlyLabAndGoalCannotResumeAfterCancel`, `ApplicationPauseQuitAndContinueDoNotAdvanceOfflineOrPermitRelease` |
| 보고 선별·보관 한도·성과 독립·회차 재개 | `ReportsFilterAndRetentionDoNotAlterScoreAndReloadDoesNotReplay` |
| 방출 전/후·보고 전/후·중간 진행·추가 대기 공존 | `FileCheckpointsPreserveWholeGraphAndContinueOnce` 6개 경우 |
| v2/v3의 Playing/Destruction/Results 이관·v4 재저장 | `LegacyPhaseMigratesAndResavesWithoutInventingAchievements` 6개 경우 |
| 누락/중복/모델/Map/시간/성과/보고/단계/목표/버전 거부 | `InvalidCandidateDoesNotPublishOrOverwriteGoodSessionAndFile` 14개 경우, `PlayingGoalReachedAndIncorrectFlagAreRejectedAfterRealGoal` |
| 목표 저장 실패 정지·재시도·재시작 실패 보존/성공 초기화 | `FailedGoalSaveStopsAndRetryCommitsWholeActivityWhileFailedRestartPreservesIt` |
| 유료 생산→보급형→침공형→두 차례 방출→목표 | `PaidInitialProductionReleasesTwoBatchesAndReachesGoal` |
| 실제 씬·UI/하네스 일치·열람 독립·Update·자동 파일·재연결 | `InvasionSceneRunsPaidCycleReleasesReportsStopsAndRebinds` |

희귀 경계의 테스트 준비 개체·완성 시설은 격리 fixture다. 실제 신규 시작 Edit/Play 시나리오는 프로토타입 1·유기물 4·시설 0에서 비용을 지불하고 실제 Worker로 건설·수집·입고·가공·배양한다. 모든 시간 진행은 공통 GameFlow를 통과하며 대기는 게임 시간/실시간 한계를 갖는다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command script recompile --project-path $projectPath --json
unity command script recompile_status --project-path $projectPath --json
unity command test run --mode editor --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
unity command test run --mode playmode --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
```

각 실행의 완료 확인·XML 복사 후 다음 실행을 시작한다. 실제 씬은 `Tools/Mobile Game/Open Laboratory Scene` 메뉴, 기존 씬에 새 SO 연결이 없을 때만 `Tools/Mobile Game/Upgrade Invasion Laboratory` 메뉴를 사용한다. 현재 씬의 기존 참조는 보존했다.

## 참조와 범위

- [기존 meta 181개](2026-10-28-before-metadata.json)를 확인했고 [meta 검사](2026-10-28-meta-audit.json)에서 변경/누락 0, C# 95개에 대응 meta 누락 0이다. 새 meta는 Unity가 생성했다.
- 새 Sprite/교체 Sprite 요청 없음. 기존 Temp 격자·문자·단색 화면을 확장했다. Require 폴더는 생성하지 않았다.
- Android 대상 Editor 검증이며 Android 빌드·실기기·세로 대응·정식 아트 검수는 수행하지 않는다.
- 목표 도달 정지까지가 이번 범위다. 멸망 연출·결과 화면·한 판 종료/재시작 완결은 후속 PLAN이며 3단계 전체 완료로 판정하지 않는다.

## 화면·소스 식별

- [방출 대기](2026-10-28-waiting.png)·[해당 상태](2026-10-28-waiting.json): 실제 생산으로 생성한 침공형·대기 1/외부 0·성과 0.
- [첫 상세 보고](2026-10-28-detailed-report.png)·[해당 상태](2026-10-28-detailed-report.json): Map 밖 동일 개체, 성과 2·보고 1·방출 버튼 비활성·선택 해제.
- [추가 방출과 후반 보고](2026-10-28-later-reports.png)·[해당 상태](2026-10-28-later-reports.json): 복수 외부 개체와 계속 진행하는 연구소.
- [목표 도달 정지](2026-10-28-goal-stopped.png)·[해당 상태](2026-10-28-goal-stopped.json): 성과 120/120·외부 2·보고 19·Destruction·연구소/외부 정지.
- 캡처는 실제 Game View OnGUI다. 공통 명령과 실제 Update 경로를 검증했으며 OS 마우스 이벤트 주입·Android 터치·정식 아트의 사용자 시각 검수와 구분한다.
- [최종 소스/에셋/설정 SHA-256 356개](2026-10-28-source-manifest.json), [PLAN-009 검증 소스에서 바뀐 기존 파일 16개](2026-10-28-changed-from-plan009.json). 선행 미커밋 변경은 유지했다. 마지막 Edit 이후 변경은 Play 비교 방법과 침공형 표시명 업그레이드이며 실제 게임 진행 코드는 같다.

## 완료 전환과 문서 검사

완료 전 [active 문서 검사](2026-10-28-docs-active.txt)는 단일 active·PLAN 목록·줄 수·링크를 통과했다. PLAN-010 completed 전환 후 [최종 문서 검사](2026-10-28-docs-completed.txt)는 active=0 조건만 종료 코드 1이며 나머지는 정상이다. 후속 실행 승인이 없으므로 다른 PLAN을 활성화하지 않았다. `git diff --check` 통과, 최종 소스 식별 356개는 검증 후 변경 0이다.

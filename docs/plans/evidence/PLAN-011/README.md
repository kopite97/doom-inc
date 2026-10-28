# PLAN-011 실행 증거

- 실행일: 2026-10-28. [PLAN-011](../../PLAN-011-destruction-results-and-restart.md) 사용자 실행 승인 범위.
- Windows PC·Unity 6000.3.27f1·URP·Android 대상의 기존 단일 Editor. CLI 1.0.0-beta.12·Pipeline 0.9.1-exp.1.
- [TECH-008](../../../decisions/TECH-008-run-ending-presentation.md): 표현 시계, 현재 화면 연결/세션 요청, 확정 결과 조회, 종료 복원과 재시작 정책.
- 실제 게임 파일 대신 OS 임시 폴더 `MobileGame-PLAN011-*` 및 기존 테스트별 격리 슬롯을 사용한다.

## 실행 기록

| 실행 | 결과 | 증거 |
| --- | --- | --- |
| 첫 구현 컴파일·Edit | 컴파일 성공, 212/212·실패/건너뜀 0 | [컴파일](2026-10-28-compile-2-status.json), [JSON](2026-10-28-edit-1-status.json), [XML](2026-10-28-edit-1.xml) |
| 화면 연결 식별 보강 후 전체 Edit | 212/212·실패/건너뜀 0 | [컴파일](2026-10-28-compile-4-status.json), [JSON](2026-10-28-edit-2-status.json), [XML](2026-10-28-edit-2.xml) |
| 첫 전체 Play | 10개 중 6 통과·4 실패·건너뜀 0 | [시작](2026-10-28-play-1-start.json), [JSON](2026-10-28-play-1-status.json), [XML](2026-10-28-play-1.xml) |
| 활성 상태 준비 보완 후 전체 Play | 10/10·실패/건너뜀 0, 실제 두 판과 파일 교체 실패 보존 확인 | [컴파일](2026-10-28-compile-5-status.json), [JSON](2026-10-28-play-2-status.json), [XML](2026-10-28-play-2.xml) |
| 작은 창의 오류/재시도 배치 보완 후 최종 전체 Play | 10/10·실패/건너뜀 0 | [컴파일](2026-10-28-compile-6-status.json), [시작](2026-10-28-play-3-start.json), [JSON](2026-10-28-play-3-status.json), [XML](2026-10-28-play-3.xml) |
| 최종 콘솔·Editor | 오류/경고 0·컴파일 완료·Play 종료·runInBackground=false | [콘솔](2026-10-28-final-console.json), [Editor](2026-10-28-final-editor.json) |

첫 Play의 실패는 추가된 앱 포커스 상실 정지가 백그라운드 검증 중에도 적용되어 실제 Update 연출과 기존 생산 시나리오가 정지한 것이다. 게임의 정지 규칙을 없애지 않고 격리 시나리오의 활성 상태 준비를 추가했다. `LoadIsolatedScene`의 일반 시나리오는 실제 포커스 콜백에 활성 상태를 제공하며, 생명주기 검사는 이 준비를 끄고 비활성·앱 정지·겹친 정지 원인·재개를 직접 확인한다. 동일한 포커스 통지가 연출의 첫 프레임을 계속 초기화하지 않도록 중복 통지는 무시한다. 초기화 전의 실제 포커스/정지 상태도 새 Controller와 동기화한다.

원본 실패는 보존하며 테스트를 비활성화하거나 기대 결과를 완화하지 않았다. 실제 OS 포커스 자동 조작·마우스/터치 입력 주입 검증과는 구분한다.

두 번째 Play는 모두 통과했지만 1065×500 실제 캡처에서 저장 오류 안내와 재시도 버튼 일부가 아래로 잘렸다. 결과 요약만 스크롤하고 다시 시작·저장 재시도는 고정 영역에 두도록 보완했다. 테스트의 의도적인 배경 명령 거부 문구도 결과 화면의 새 판 요청 오류와 구분했다. 이 화면 변경 후 전체 Play 10/10과 실제 오류 화면의 두 버튼 노출을 재확인했다.

## 시나리오

| 경계 | 검증 |
| --- | --- |
| 목표 프레임·완료 직전/정확 경계·두 자동 저장 | `EndingClockHasVisibleEntryBoundaryAndTwoAutomaticFiles` |
| 긴 연출 delta·앱 정지·화면 비활성/재활성 | `EndingPauseVisibilityAndStallsDoNotCatchUp` |
| 다른 화면·Reload·새 판·두 번째 종료의 이전 요청 | `OldScreensReloadedSessionsAndDuplicateRequestsCannotFinishOrRestartAnotherRun` |
| 목표/Results 쓰기 실패·정지·재시도·재시작 보존 | `EndingSaveFailuresKeepMemoryPhaseAndGoodFileThenRetry` 2개 경우 |
| 읽기 실패·현재 화면/시계·성공 복원 시 재생 | `EndingReloadFailurePreservesScreenClockAndRestartLease` |
| v2/v3 Destruction/Results·v4 재저장 | `LegacyEndingReplaysOrOpensResultsWithoutInventedActivity` 4개 경우 |
| 실제 초과 성과·반복 조회 불변 | `ResultsRetainOvershootAndFilteredReportTotals` 및 기존 보고 선별/보관 한도 회귀 |
| 실제 유료 두 판·편집 정리·Update 연출·결과 저장·파일 잠금 재시작 실패·UI/하네스 | `EndingSceneCompletesTwoPaidRunsAndPreservesFileOnFailedRestart` |
| 실제 View 비활성/재개·포커스/앱 정지 중첩·읽기 실패·화면 파괴/재생성 | `EndingViewPauseDisableReloadAndRecreationPreserveStoppedRun` |

실제 두 판은 각각 프로토타입 1·유기물 4·시설 0에서 획득/저장/가공/배양 시설을 유료로 건설하고 Worker로 수집·운반·공급한다. 보급형 생산·가공시설 배정 후 침공형을 배양·방출하여 목표에 도달한다. 강제 목표 신호나 무료 자원 지급은 사용하지 않는다. 별도의 생명주기/저장 경계 fixture와 이 실제 흐름을 구분한다.

첫 판은 SimulationRate=0이어도 실제 Update의 독립 연출 시간이 전환을 완료하는지 확인한다. 두 번째 판은 같은 저장 그래프의 하네스와 동일한 연출 시간 입력으로 단계/진행/확정 성과를 비교한다. 저장된 Destruction과 실제 완료가 자동 기록한 Results는 추가 수동 Save 없이 새 객체로 복원한다.

## 재현 명령

작업공간 루트에서 기존 Editor에 연결하며 컴파일·각 테스트 완료 후 다음 명령을 실행한다. XML 기본 경로는 Editor 로그에서 확인하고 다음 실행 전에 복사한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command script recompile --project-path $projectPath --json
unity command script recompile_status --project-path $projectPath --json
unity command test run --mode editor --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
unity command test run --mode playmode --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
```

## 에셋과 범위

- [기존 meta 198개](2026-10-28-before-metadata.json)의 [변경/누락 검사](2026-10-28-meta-audit.json)는 0건이다. 신규 C#의 meta는 Unity가 생성했다.
- 새 Sprite 요청 없음. 기존 Temp 도형·색·문구를 확장했으며 정식 아트 제작/사용자 시각 검수 완료를 뜻하지 않는다. Require 폴더를 추가하지 않았다.
- PC 가로 실제 Scene·OnGUI와 공통 명령 검증이다. Android 빌드/실기기·세로 대응·최종 성능/밸런스·대표 병목 종합 검증은 후속 4단계 범위다.

## 화면과 소스 식별

- 첫 판 [연출 시작](2026-10-28-run1-start.png)·[중간](2026-10-28-run1-middle.png)·[결과](2026-10-28-run1-results.png): 실제 활동 32회·성과 128/120·외부 1개체·명시적 진행 571.5초. 초과 성과를 잘라내지 않는다.
- [파일 교체 실패 화면](2026-10-28-restart-error.png): 동일한 결과를 유지하며 다시 시작·저장 재시도 모두 노출한다. [재시작 상태](2026-10-28-restarted.json)는 Playing·프로토타입 1·유기물 4·시설/침공형/성과/보고/시간 0이다.
- 두 번째 판 [연출 시작](2026-10-28-run2-start.png)·[결과](2026-10-28-run2-results.png)·[확정 상태](2026-10-28-run2-results.json): 새 객체/ID로 실제 생산부터 같은 종료 흐름을 다시 완료했다.
- [최종 소스·에셋·설정 SHA-256 364개](2026-10-28-source-manifest.json), [PLAN-010 대비 변경/신규 13개](2026-10-28-changed-from-plan010.json). 기존 meta 198개는 모두 유지, 신규 C# meta 누락 0. 선행 미커밋 변경은 보존했다.
- 마지막 Edit 이후의 변경은 실제 View 포커스 초기화/중복 통지 처리, Play 시나리오 활성 상태 준비와 종료 화면 배치다. GameFlow·RunEndingPresentation은 최종 Edit 212/212 당시와 같고 View 변경은 최종 Play 10/10으로 검증했다.

## 완료와 문서 검사

[active 문서 검사](2026-10-28-docs-active.txt)는 단일 active·목록·링크·줄 수를 통과했다. 기능·화면·회귀 증거를 확인한 뒤 PLAN-011과 로드맵 3단계를 completed로 동기화했다. [완료 후 문서 검사](2026-10-28-docs-completed.txt)는 active=0만 종료 코드 1이며 목록·링크·줄 수는 정상이다. 후속 실행 승인 대상이 없어 새 active를 만들지 않았다. `git diff --check` 통과, 최종 검증 소스 식별 이후 변경은 0개다.

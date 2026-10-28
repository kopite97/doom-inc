# PLAN-009 실행 증거

- 실행일: 2026-10-28. [PLAN-009](../../PLAN-009-processing-cultivation-and-workers.md) 승인 범위.
- Windows PC, Unity 6000.3.27f1·URP, Android 대상 Editor, CLI 1.0.0-beta.12·Pipeline 0.9.1-exp.1. 기존 단일 Editor를 사용했다.
- [Manager·생산 기술 결정](../../../decisions/TECH-006-production-and-manager-boundaries.md), [이관 전 meta](2026-10-28-before-metadata.json), [이름·경로 이관](2026-10-28-renames.json), [최종 meta 검사](2026-10-28-meta-audit.json).
- 테스트 슬롯은 OS 임시 경로 `MobileGame-PLAN009-*`로 격리했다. 실제 `Gameplay/session.json`을 테스트로 덮어쓰지 않았다.

## 순서와 결과

| 구간 | 결과 | 증거 |
| --- | --- | --- |
| 이름·책임 정비만 적용한 컴파일 | 초기 Editor 로딩 중 연결 실패 후 정상 완료 | [초기 응답](2026-10-28-refactor-compile.json), [재실행](2026-10-28-refactor-compile-ready.json) |
| 새 기능 추가 전 Edit | 133/133 통과, 실패·건너뜀 0 | [JSON](2026-10-28-refactor-edit.json), [XML](2026-10-28-refactor-edit.xml) |
| 새 기능 추가 전 Play | 6/6 통과, 실패·건너뜀 0 | [JSON](2026-10-28-refactor-play.json), [XML](2026-10-28-refactor-play.xml) |
| 생산 확장 첫 Edit | 149개 중 125 통과·24 실패 | [JSON](2026-10-28-production-edit-1.json), [XML](2026-10-28-production-edit-1.xml) |
| 에셋 생성/저장 버전 기대값 수정 후 Edit | 149개 중 148 통과·1 실패 | [JSON](2026-10-28-production-edit-2.json), [XML](2026-10-28-production-edit-2.xml) |
| 실제 저장 운용과 예외 검사 확장 후 Edit | 163/163 통과, 실패·건너뜀 0 | [JSON](2026-10-28-production-edit-3.json), [XML](2026-10-28-production-edit-3.xml) |
| 생산 씬 Play | 7/7 통과, 실패·건너뜀 0 | [JSON](2026-10-28-production-play-1-status.json), [XML](2026-10-28-production-play-1.xml) |
| 최종 컴파일·Edit | 컴파일 성공, 163/163 통과 | [컴파일](2026-10-28-final-compile.json), [JSON](2026-10-28-final-edit.json), [XML](2026-10-28-final-edit.xml) |
| 최종 Play·콘솔 | 7/7 통과, 오류·경고 0, Play 종료·컴파일 완료·runInBackground=false | [JSON](2026-10-28-final-play.json), [XML](2026-10-28-final-play.xml), [콘솔](2026-10-28-final-console.json), [Editor](2026-10-28-final-editor.json) |

첫 Edit 실패 24개는 새 배양액 정의의 ResourceStack 구성 누락으로 신규 테스트 16개가 실행 불가했던 것과, 기존 저장 왕복 8개가 버전을 2로 고정 단언했던 것이다. 필수 정의 구성을 수정하고 기존 왕복 검사의 버전 단언만 CurrentVersion으로 갱신했다. 기존 실제 동작 단언은 유지했다.

두 번째 Edit 실패는 유기물로 가득 찬 공용 저장소 때문에 배양액 입고가 막혀 첫 보급형까지 도달하지 못한 실제 운용 시나리오였다. 용량·입고 규칙을 우회하지 않고 수집 자원으로 두 번째 저장소를 건설해 배양액 전용으로 설정하는 조작을 추가했다. 해당 비용·건설도 실제 Worker가 수행한다.

Editor 메뉴 호출의 성공 응답은 내부 에셋 생성 성공과 같지 않다. 첫 [메뉴 응답](2026-10-28-upgrade-scene.json)은 [내부 예외](2026-10-28-asset-creation-failure.txt)가 있었고, 수정 후 [두 번째 호출](2026-10-28-upgrade-scene-2.json)과 실제 SO 존재·Play 로드로 확인했다. [콘솔 원본](2026-10-28-console-history.json)을 보존한 뒤 최종 Play 관찰 구간을 새로 시작했다.

## 검증 범위

| 승인 규칙/경계 | 실행 증거의 테스트 |
| --- | --- |
| 무료 지급/선배치 없이 첫 생산 진입 | `ProductionTests.RealInitialCycleBuildsFourFacilityTypesCultivatesFirstSupportAndWaitingInvader`: 프로토타입 1·유기물 4·시설 0에서 수집/유료 건설, 첫 보급형, 배정 후 100% 작업, 침공형 대기까지 |
| 5→1·10초, 2→1·20초·무인 20% | `UnstaffedCyclesConsumeOnceAndFinishAtTwentyPercent`: 50/100초 경계, 한 번 소비·한 번 생성 |
| 실제 작업·이동·해제·단일 슬롯 | `AssignmentOnlySpeedsActualWorkAndUnassignPreservesConsumedCycle`, `TwoAssignedWorkersClaimOneSlotWithoutAddingAutomaticSpeed` |
| 배양 선택 없음·현재/다음 대상·중첩 출현 | `NoSelectionDoesNotRequestInputsAndMidCycleTargetChangeAppliesNextCycle`: 무선택 공급 없음, 다음 회차 변경, 미배정 보급형과 Worker 없는 침공형 |
| 부족분과 복수 운반 예약 | `CurrentCycleReservationsSubtractPartialInputsAcrossMultipleWorkers`: 도착 2에서 3만 예약, 소비 후 미래 회차 비축 없음 |
| 실제 저장소 입고·보유 대기 | `OutputPickupDoesNotReleaseCycleAndHoldingWaitSurvivesFileReload`, `OutputMustVisitStorageBeforeSupplyingCultivation` |
| 두 자원 용량 합산·허용 변경·출고 유지 | `MixedStorageCapacityExclusionAndExistingOutgoingReservationAreIndependent`와 기존 물류 회귀 |
| 모델 자격·정상 운반 중 배정 | `AssignmentRejectsWrongModelsAndFacilitiesAndPreservesNormalTransport` |
| 명시적 중단/자동 복귀 차이·우선순위 | `ExplicitAssignmentInterruptsGatheringButAutomaticReturnFinishesItsCycle`, `AutomaticReturnReconsidersMovementButRespectsPlayerPriorities` |
| 편집·앱 정지·이동/회전·접근 차단/회복 | `PlacementPauseMovementAndApplicationPausePreserveCycle`, `BlockedInteractionStopsAutomaticProgressAndRecoveryDoesNotConsumeAgain` |
| 실제 파일 중간 복원 | `FileCheckpointsRestoreWholeGraphAndContinueWithoutDuplicatingWork`: 일부 재료, 예약, 20%, 100%, 다음 대상 변경, 출력 운반, 배정 변경 후 정상 운반, 출현 직후 8개 경우 |
| 호환·실패 보존 | `VersionTwoSaveMigratesWithoutChangingIdentityOrOrganicAllowance`, `InvalidProductionGraphIsRejectedWithoutReplacingGoodFile` 5종과 기존 저장 회귀 |
| 씬·공통 명령·화면·초기화 | `LaboratoryViewTests.ProductionSceneRunsPaidInitialCycleMatchesHarnessAndRestartsCleanly`: 실제 SO/씬, 동일 명령 후 전체 스냅샷 비교, 실제 Update 진행, 저장·이어하기·다시 시작 |

희귀 예외의 준비 상태는 테스트에서 명시적으로 구성하며 실제 신규 시작 검사와 구분한다. 시간 주입은 항상 GameFlow/Laboratory 공통 갱신으로 진행하고 별도 생산 계산을 만들지 않는다. 각 대기는 게임 시간/실시간 한계가 있고 초과 시 실패한다.

## 화면과 추적

- [실제 생산 작업 100% 화면](2026-10-28-production-working.png), [관련 ID·명령 조회](2026-10-28-production-working.json).
- [침공형 출현·다음 배양 대기 화면](2026-10-28-cultivation-waiting.png), [관련 ID·명령 조회](2026-10-28-cultivation-waiting.json).
- 두 캡처는 실제 Game 뷰의 OnGUI이며 시설/개체 기호, 진행량·소비, 100%, 대기 원인, 현재/다음 대상을 확인했다. 작은 창에서는 오른쪽 패널을 스크롤해 목록·배정·우선순위를 조작한다.
- 공통 명령/모델 검증이며 OS 마우스 이벤트 주입·Android 터치·사용자 정식 시각 검수 완료로 표현하지 않는다.
- 새 Sprite·교체 Sprite 요청 없음. 기존 Temp 문자/단색 표시를 확장했고 별도 Require README를 만들지 않았다.

## 재현

저장소 루트 PowerShell에서 기존 Editor의 프로젝트 절대 경로를 지정한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command script recompile --project-path $projectPath --json
unity command script recompile_status --project-path $projectPath --json
unity command test run --mode editor --project-path $projectPath --timeout 60 --json
unity command test run --mode playmode --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
```

실제 장면은 `Tools/Mobile Game/Open Laboratory Scene` 메뉴로 연다. 새 SO 연결이 없는 이전 씬에만 `Tools/Mobile Game/Upgrade Production Laboratory`를 사용한다. 기존 정의가 존재하면 값을 덮어쓰지 않고 검증하며, dirty 씬이 있으면 먼저 저장하도록 중단한다.

PC 조작은 획득시설·저장소 건설 → 수집 → 가공·배양·배양액 전용 저장소 건설 → 자원별 허용 설정 → 배양시설에서 Support 선택 → 첫 보급형 선택/우선순위와 가공 배정 → Invader 선택 순서다. 초기 회차는 미배정 20%로 진행한다. 선택한 개체/시설의 상세와 명령은 오른쪽 패널에 있다.

검증 후 Play 종료·컴파일 완료·콘솔을 확인했다. 최종 검증에 사용한 Unity 소스·에셋·패키지/설정 325개 파일의 SHA-256은 [소스 목록](2026-10-28-source-manifest.json)에 있다. Android 대상 Editor 검증을 Android 빌드/실기기 또는 전체 게임 완료로 보지 않는다.

## 완료 시 문서 상태

완료 직전 [문서 검사](2026-10-28-docs-active.txt)는 active=1·목록·줄 수·링크 검사를 통과했다. PLAN-009 completed 전환 후 [문서 검사](2026-10-28-docs-completed.txt)는 active=0 조건만 종료 코드 1이며 나머지는 정상이다. 후속 PLAN 실행 승인이 없으므로 검사 통과를 위한 임의 활성화를 하지 않았다. `git diff --check`도 통과했다.

완료 전환 직후 [첫 문서 검사](2026-10-28-docs-completed-initial.txt)의 목록 상태 불일치는 PLAN MAP을 completed로 동기화해 해결했다. 최종 검사는 아래 결과 파일과 같이 active=0 조건만 남는다.

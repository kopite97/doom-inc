# PLAN-014 유스케이스별 검증 연결

대상: [PLAN-014](../../../PLAN-014-mobile-ui-ux-and-facility-status.md)의 UC-01~23. 이 표는 자동 검사·실기기에서 확인한 범위와 기기에서 전수 재현하지 않은 제한을 구분한다. 테스트 이름의 `Device`는 Editor의 합성 Input System 장치를 뜻하며 Android 기기에서 실행했다는 뜻이 아니다.

## 확인한 실행과 판정 범위

- [Edit 02](../editmode-02.json): 286/286 통과, 실패·건너뜀 0. 상태 파생·물류·생산·설정·저장 규칙의 회귀 증거다.
- [조작부 축소 후 전체 Play](../playmode-compact-controls.json): 34/34 통과, 실패·건너뜀 0, 57.98초. 아래 Play 검사 판정은 이 최신 실행의 개별 결과에 한정한다. 캡션 재탭 실행과 동일한 34개 검사 이름을 대조했고 모두 통과했다. 빌드 전 Editor의 컴파일 완료·Play 종료를 확인했다.
- [관련 대상 링크 추가 Play](../playmode-related-target.json): 1/1 통과, 실패·건너뜀 0, 1.42초. UC-12의 저장소 설정 및 배정 시설↔작업자 링크를 양방향 실제 UI 입력으로 보완했다. 앞선 34개 전체 검사와 별도 실행이며 런타임/APK 변경은 없다.
- [표식 재탭 보완 후 전체 Play](../playmode-caption-toggle.json): 34/34 통과, 실패·건너뜀 0, 56.54초. UC-17의 1·2·3회 재탭과 줌/회전 후 접힘 유지 검사를 포함한 이전 수정본의 회귀 이력이다.
- [Play 04](../playmode-04.json)의 34/34 통과는 재탭 보완 전 회귀 이력으로 보존한다. 이후 추가된 재탭 동작의 통과 근거로 사용하지 않는다.
- [Play 02](../playmode-02.json)의 33 통과·1 실패는 이력으로 보존한다. 실패한 `DeviceBackCancelsPendingRestartAndInvalidatesHeldConfirmWithoutLosingDraft`는 배치 취소 후 복원된 건설 패널을 한 번 더 닫아야 기본 화면에 도달하는 순서로 기대값을 수정했고, Play 04에서 통과했다. Play 03의 결과를 추정해 사용하지 않았다.
- 공통 명령을 직접 호출한 통합 회귀, 실제 UI 이벤트를 거친 검사, 정지 상태를 준비한 표시 fixture를 구분한다. 시설·개체를 준비하거나 시간을 전진시킨 테스트를 첫 건설부터 끝까지 사람이 새 UI로 플레이한 증거로 사용하지 않는다.
- 소스 대조는 연결 경로가 존재함을 확인한 것이며 실행 성공·가독성·조작감 판정이 아니다. Android는 첫 APK 부분 확인, 캡션 재탭 수정 APK의 설치·직접 확인, 조작부 축소 APK의 설치·가로 운영과 남은 미검증 항목을 구분한다.
- [첫 APK 실기기 기록](../android-device-run/README.md): S10에서 ADB 단일 포인터로 오프라인 초기 진입, 4종·총 5개 시설 시공 완료, 배양 대상 선택, 저장소 허용 해제 뒤 재고 유지, 상태 ON→OFF→ON을 확인했다. 사용자는 세로 직접 조작에 대해 “기능적인 이상은 없었어”라고 설명했다. 이 직접 응답은 ADB 기록과 구분하며, 모든 동작의 정량 검증이나 수정 APK 통과로 확대하지 않는다.
- [재탭 보완 APK](../android-build-20261029-131259-324.json)는 2026-10-29 22:13:44 KST에 빌드 성공했다(오류·경고 0). 22:18:00 KST 설치 후 설치 경로 SHA-256을 대조했고 같은 검증 게임의 11개 Thing·점수 0·DataHash와 ON·기본 글씨를 유지했다. ADB 표식 세 번 탭과 사용자의 수정 표식·가로 회전·핀치/한 손가락 전환·맵 이동·패널 스크롤 정상 확인을 별도로 받았다. 자세한 범위는 [기기 기록][android]을 따른다.

- 사용자의 후속 요청인 가로 맵 영역 확보와 네 카메라/상태 조작부 축소를 반영했고 최신 전체 Play 34/34를 통과했다. [조작부 축소 APK](../android-build-20261029-133118-689.json)는 22:32:29 KST에 `Succeeded`, 오류·경고 0, 69.735초로 빌드됐다. 실제 APK 66,262,943바이트·SHA-256 `26946b22789989c5ed38ea41ec6fb45aceec56dcedff80be85a601a957375173`이며 [설치 기록](../android-compact-install.json)의 서명 검증·비스트리밍 재시도 성공 후 22:37:37 KST 설치 경로 해시 일치를 확인했다. 같은 검증 게임으로 가로 조작부·상태 OFF/ON·저장소 추가·전역 채집 우선순위·저장/불러오기 취소·확정·배양 현재/다음·큰 글씨를 확인했고 [기기 기록][android]에 범위를 나눠 남겼다. [소스 537개 일치](../android-source-compact-20261029-133107.json)·[고아 컴파일러 정리](../android-compact-compiler-preparation.json)·[실행 기록](../android-run-compact-20261029-133107.json)과 [새 PC 화면 6개](../README.md)를 연결했다. 이후 실제 종료/재실행·보급형 생성/배정·침공형 2마리 방출·결과128/120·새 판과 OFF 중 저장소 이동까지 확인했고 사용자 원본/기기 설정을 [복원했다](../android-device-run/restore-receipt.json). [최종 소스 대조](../final-source-review.json)는 런타임/APK 불변과 문서 두 개·Editor 검사만의 차이를 확인했다.

## UC별 자동 증거·실기기 결과·제한

| UC | 확인한 자동 증거·소스 연결 | 실기기 확인 범위와 PC 검사·관찰 제한 |
| --- | --- | --- |
| UC-01 최초 진입·이어하기 | [로딩 검사][bootstrap]의 `BootstrapWaitsForWholeBundlePreservesSaveOnErrorAndRetries`, [오류 화면][screens]의 `LoadingAndRestoreFailuresKeepRecoveryControlsReachableInBothOrientations`, [초기 사이클][initial]의 `RealStartBuildsCollectsAndStoresWithAutomaticSaveAndMidConstructionContinue` 통과. | PC에서 Ready 전 세션 미공개·오류 구분·기존 파일 보호·시공 중 복원을 확인했다. 기기에서는 오프라인 최초 진입, 수정 APK의 동일 검증 게임 이어하기, 메뉴 불러오기와 실제 종료/재실행에 성공했다. 역할/비용 발견성과 로딩 중 터치의 모든 조합은 기기 전수 관찰 범위 밖이다. |
| UC-02 맵 관찰·카메라 | [입력][input]의 `DeviceDragAndPinchNeverSelectOnRemainingFingerRelease`, [운영 입력][operations]의 `DeviceTouchScrollOwnsCrossBoundaryAndMixedContactsThenMapCanPanToLastCell`, [읽기 맥락][reading]의 `SelectingOffscreenFacilityRevealsItOnceAndDoesNotUndoSubsequentDevicePan` 통과. [Map][map]의 Zoom/Fit/좌표 변환을 소스 대조했다. | 사용자 핀치/한 손가락 전환·맵 이동 정상 응답을 받았다. 조작부 축소 APK에서 ADB 확대·축소·전체 보기·ON/OFF 및 선택 패널 보존을 확인했고 맵 높이는 동일 해상도에서 약 407→644px로 늘었다. 초안/패널/Fit의 모든 조합은 PC 검사와 소스 대조 범위이며 기기 전수 재현을 주장하지 않는다. PC 휠 실입력도 별도 범위다. |
| UC-03 시설·겹친 개체 선택 | [입력][input]의 `DeviceTouchOpensOverlappingCandidatesAndEditsOnlySelectedWorker`, [운영 입력][operations]의 `DeviceFacilityOperationsAndIndividualAssignmentsWorkInBothOrientations` 통과. [UI][gui]의 `AddRoster`는 Thing ID를 행 키/명령에 사용하고 [버튼 경계 검사][layout-edit]는 이전 바인딩의 release를 거부한다. | 기기에서 [겹친 개체 후보](../android-device-run/overlap-candidates.png)를 열고 침공형을 선택했으며 생성된 보급형을 목록에서 선택해 개별 우선순위를 변경했다. 같은 셀 다중 개체의 ID별 변경·이전 바인딩 거부는 PC로 확인했다. 실시간 행 제거/급증의 모든 조합은 기기에서 전수 재현하지 않았다. |
| UC-04 요약·상세·목록 | [모바일 UX][mobile]의 `DeviceLargeTextIconsAndBundledKoreanRemainUsableInBothOrientations`, [운영 입력][operations]의 스크롤 경계 검사 통과. [탐색][navigation]의 펼치기/접기·닫기·패널별 위치 보존 경로를 소스 대조했다. | PC에서 양방향 상세·접기/펼치기·ScrollRect 및 UI 드래그의 맵 유출 방지를 확인했다. 기기에서 실제 시설/개체/설정 패널과 스크롤을 사용했고 사용자도 정상이라고 확인했다. 모든 패널 조합의 스크롤 복귀 위치를 기기에서 정량 계측한 것은 아니다. |
| UC-05 건설·자재 부족 | [모바일 UX][mobile]의 `DeviceZeroMaterialConstructionStillAcceptsValidSiteAndExplainsWaiting`, [상태 조회][status]의 `ValidSiteWithNoMaterialsCanWaitForDeliveryAndThenConstruction`·`AnotherSitesReservationIsDistinctFromTotalMaterialShortage` 통과. | 기기 첫 판의 4종·총 5개 시설, 최종 APK의 추가 S32, 새 판 저장소를 실제 건설 목록·부지 확정·자연 시공으로 완료했다. 자재 0 부지 확정·자재/예약 경쟁 원인 구분은 PC 검사로 확인했다. 모든 불가 배치·자원 부족 조합을 기기에서 다시 재현하지 않았다. |
| UC-06 이동·회전·취소 | [운영 입력][operations]의 `DeviceFacilityOperationsAndIndividualAssignmentsWorkInBothOrientations`, [공통 배치][initial]의 `SceneAndHarnessShareConstructionSettingsAndPlacementRules` 통과. Edit의 `GridMapTests.CorridorAndInteractionBlockingDoNotBecomePlacementRestrictions` 및 생산 이동/정지 회귀도 통과했다. | 기기의 새 판 저장소 #3에서 90° 이동 미리보기→취소로 (4,3) 원위치를 유지했고, 표시 OFF로 90° 회전→(10,3) 확정 뒤 자동 저장의 rotation=1·시공 완료·전체 유기물 2개를 확인했다. 재고 0인 저장소를 사용했으며 생산 진행/재고가 있는 모든 이동 조합은 PC 회귀 범위다. |
| UC-07 생산·채집 상태 | [상태 조회][status] 11개 검사 통과. 특히 실제 작업/배정 구분, 채집 중단, 부분 재료/예약, 완료품 pickup·부분 입고까지 100%, 같은 정의의 독립 상태를 검사했다. [표시 검사][status-layout]도 통과했다. | 기기에서 자동 생산, 입고 대기 100%, 채집 작업자 대기, 재료 대기와 현재 회차/기준 속도 구분을 관찰했다. 완료품 부분 입고·예약·접근 불가 등의 전체 파생 상태는 Edit/표시 검사로 확인했다. 기기에서 모든 상태·모든 밀집 조합을 전수 재현한 것은 아니다. |
| UC-08 배양 선택·변경·완료 | [운영 입력][operations]의 양방향 배양 선택, [생산 회귀][production]의 `ProductionSceneRunsPaidInitialCycleMatchesHarnessAndRestartsCleanly`, Edit의 `ProductionTests.NoSelectionDoesNotRequestInputsAndMidCycleTargetChangeAppliesNextCycle` 및 [상태 조회][status]의 현재/다음 회차 검사 통과. | 기기에서 보급형 선택, 진행 중 현재 보급형/다음 침공형 변경과 activeTarget=1·selectedTarget=2 저장을 확인했고, 이어 보급형 #41과 침공형들이 자연 생성됐다. 유료 생산·다음 회차 적용 규칙은 PC 회귀와 함께 확인했다. 모든 회차 변경 시점 조합은 기기 전수 재현 범위 밖이다. |
| UC-09 보급형 배정 | [운영 입력][operations]의 두 개체 개별 배정·해제 통과. Edit의 `ProductionTests.AssignmentOnlySpeedsActualWorkAndUnassignPreservesConsumedCycle`·`AssignmentRejectsWrongModelsAndFacilitiesAndPreservesNormalTransport`, [상태 조회][status]의 운반 중 배정 검사 통과. | 기기에서 자연 생성된 보급형 #41을 C12에 배정했고 실제 assignedFacilityId가 일치했다. 생산 끔까지 개별 설정한 저장은 hasJob=false이므로 배정을 곧바로 가속 작업 중으로 판정하지 않는다. 재배정/해제·운반 중 전환·실제 가속 규칙은 PC 검사로 확인했고 해당 모든 조합의 기기 클릭을 주장하지 않는다. |
| UC-10 프리셋·우선순위 | [입력][input]의 개체별 우선순위 변경, [버튼 경계 검사][layout-edit]의 `TwoHeldPriorityButtonsApplyEachChangeToTheLatestValues`, Edit의 `WorkTests.PresetDisabledWorkOverrideAndEqualPriorityOrderUseCommonSettings` 통과. [선택 상세][details]의 프리셋 복귀·우선순위 표기를 소스 대조했다. | 기기에서 전역 채집을 0으로 바꿔 프리셋/사용 작업자 [1,0,3,4]를 확인했다. 보급형 #41은 개별 [1,0,3,0]·usesPreset=false였고 새 판에는 기본 [1,2,3,4]로 복귀했다. 개별→프리셋 복귀의 전체 조합·동시 누름은 PC 증거이며 기기 전수 조작 범위 밖이다. |
| UC-11 저장소 | [운영 입력][operations]의 자원별 허용 토글 통과. Edit의 `ProductionTests.MixedStorageCapacityExclusionAndExistingOutgoingReservationAreIndependent`와 `LogisticsTests.ExcludedStoredStacksRelocateAsOneFullLoadAndKeepOutgoingWorkWhenAllowedAgain` 등 물류 회귀 통과. | 기기 저장소 #5의 유기물 허용 해제 뒤 재고 20개·용량 점유 유지, 추가 저장소 S32 시공, 새 판 저장소 이동 후 재고 0·전체 자원 유지가 화면/저장과 일치했다. 전량 재배치·예약·옮길 곳 없음 등 세부 조합은 PC 물류 회귀로 확인하며 기기 전수 대조를 주장하지 않는다. |
| UC-12 원인·관련 대상 | [상태 조회][status]의 재료/예약/접근/작업/입고 원인 검사와 [관련 대상 UI 추가 검사](../playmode-related-target.json)의 `DeviceRelatedTargetLinksSelectRevealAndExpandCurrentTargetsInBothOrientations` 1/1 통과. [읽기 맥락 검사][reading]에서 실제 UI 입력을 사용했다. | 가로·세로에서 자재 대기→저장소 링크/허용 설정 토글·복구와 배정 시설↔작업자 링크를 클릭해 정확한 SelectedId·상세 열림·시야 표시·맵 탭 누출 없음을 확인했다. Android 직접 링크 클릭은 미기록이며 개별 운반 예약 링크 전체를 직접 클릭한 검사로 확대하지 않는다. |
| UC-13 침공 현황·방출 | [운영 입력][operations]의 `DeviceReleaseReportsAndResultsRestartUseCurrentSessionCommands`, [침공 회귀][invasion]의 `InvasionSceneRunsPaidCycleReleasesReportsStopsAndRebinds` 통과. Edit의 전원 방출/ID 보존/불가 단계 검사 통과. | 기기에서 침공형 2마리를 방출해 직후 외부 2·대기 0을 확인했다. 후속 저장은 동일 외부 2와 자연 생성된 대기 1, 점수 64·보고 16을 기록한다. 외부 목록·미배치 선택 규칙과 0마리/불가 단계/ID 보존의 세부 조합은 PC 증거이며 기기 전수 재현은 아니다. |
| UC-14 보고 | [읽기 맥락][reading]의 `NewReportsPreserveReadingPositionAndPrunedAnchorShowsNearestRetainedReport`, [운영 입력][operations]의 실제 보고 펼치기/접기 통과. | 기기에서 방출 후 실제 보고 16개·점수 64, 결과 단계의 보존 보고 20개를 확인했고 사용자는 보고와 결과 화면이 모두 정상이라고 답했다. 읽던 행 앵커·보존 한도 삭제 후 가까운 행·최신 복귀·열람 무명령은 PC UI 검사로 확인했다. 실제 기기의 모든 읽기/회전/정리 시점을 직접 추적한 증거는 아니다. |
| UC-15 표시 ON/OFF | [모바일 UX][mobile]의 `DeviceDisplayPreferencesSurviveNewRunAndSceneReopenWithoutChangingGameData`, [표시 검사][status-layout]의 ON/OFF·선택 보존·반복 검사, [설정][settings]의 기본값·잘못된 파일·독립 저장·쓰기 실패 검사 통과. | 기기 ON→OFF→ON과 OFF 중 선택/상세 유지, OFF 중 저장소 이동·확정, 게임/표시 설정의 파일 분리를 확인했다. ON·큰 글씨는 실제 종료/재실행과 결과 새 판 뒤에도 유지됐다. 설정 쓰기 실패·잘못된 파일 복구와 모든 조합은 PC 검사 범위다. 마지막 검증 설정 OFF·큰 글씨는 원본 UI 파일 부재 복원 전에 별도 보존했다. |
| UC-16 밀집·가장자리 | [표시 검사][status-layout]의 `DenseFacilityStatusesRemainDistinctAtFitEdgesAndMaximumZoomInBothOrientations` 통과. 시설 20개/표시 19개, 양방향·보통/큰 글자·Fit/최대 줌·회전한 저장소·경계에서 대상 ID/뷰포트/표식 충돌을 검사했다. | PC 정지 fixture에서 20시설/19표식, 양방향·큰 글씨·Fit/최대 줌·경계·충돌을 검사했다. 기기에서는 자연 진행한 6시설 규모의 선택·표식·전체 보기·큰 글씨를 관찰했다. 20시설 밀집·모든 줌/가장자리의 실기기 전수 관찰로 확대하지 않는다. |
| UC-17 제스처·경계 | [입력][input]의 드래그/핀치·빠른 탭·마우스 단일 건설, [운영 입력][operations]의 UI/맵 혼합 접촉 통과. [모바일 UX][mobile]의 `DeviceStatusTapUsesSelectionNormallyAndPreviewInBothEditingModesWithOnAndOff`는 [조작부 축소 후 실행](../playmode-compact-controls.json)에서 첫 표식 탭의 선택/설명 펼침→두 번째 접힘→세 번째 펼침, 아이콘·진행바·선택·패널·스크롤 보존, 명시적 접힘의 확대/회전 후 유지까지 통과했다. | 사용자는 실제 핀치/한 손가락 전환·맵 이동·패널 스크롤과 표식 재탭 정상 확인을 했고, 조작부 축소 뒤에도 가로 배치·조작 정상 응답을 받았다. ADB의 첫/둘째/셋째 표식과 실제 배치 입력은 별도 기록이다. 신규/이동 × ON/OFF의 입력 우선순위·잔류 release는 PC 검사로 확인하며 모든 혼합 접촉을 기기에서 전수 재현했다고 주장하지 않는다. |
| UC-18 Back·방향 전환 | [6개 해상도][layout]의 `ResponsiveLayoutsPreserveSessionAndDraftAcrossSixScreenSizes`, [오류/종료 화면][screens]의 방향 전환, Edit의 이전 layout/binding release 차단 통과. [모바일 UX][mobile]의 `DeviceBackCancelsPendingRestartAndInvalidatesHeldConfirmWithoutLosingDraft`도 조작부 축소 후 전체 Play에서 통과했다. | 사용자의 실제 방향 전환 정상 응답과 양방향 화면, 기기의 새 판 확인 취소 후 같은 건설 초안 복귀를 확인했다. PC는 여섯 해상도·이전 입력 바인딩·Back 계층 및 연출/결과 전환을 검사했다. 실기기 연출 중 세로 회전의 정확한 시각은 직접 관찰하지 못했고, 수집 결과 캡처는 가로다. 모든 모달 단계에서 누른 채 회전하는 조합은 PC 검사 범위다. |
| UC-19 Home·재실행 | [입력][input]의 `DeviceHeldTouchIsCanceledAcrossImmediatePauseAndResume`, [종료 회귀][ending]의 `EndingViewPauseDisableReloadAndRecreationPreserveStoppedRun`, Edit의 `GameFlowManagerTests.AutomaticAndLifecycleSavingUseActualFilesAndPauseDoesNotAccumulateTime`·단계별 오프라인 진행 없음 검사 통과. | 실제 Home→앱 프로세스 종료 전후 정지 저장의 바이트/해시와 경과 시간이 같았고, 재실행은 기동 후 약 10.02초만 반영했다. 10개 Thing·점수 0·DataHash·ON/큰 글씨가 유지됐다. 누른 채 pause/resume와 단계별 오프라인 시간 규칙은 PC 검사로 확인하며 모든 운영체제 앱 전환 조합을 기기에서 전수 재현하지 않았다. |
| UC-20 실패·복구 | [오류 화면][screens]의 로딩/복원 오류·긴 오류 스크롤·저장 오류/재시도 접근, [종료 회귀][ending]의 실패한 재시작 파일 보호, [설정][settings]의 쓰기 실패·기본 복구 검사 통과. | 드문 로딩/복원/저장/설정 실패는 PC의 잘못된 데이터·파일 잠금·저장소 주입으로 보존과 복구를 확인했다. 기기에서는 정상 저장·불러오기·설정 변경 피드백 및 실제 재실행을 확인했다. 실패를 만들기 위해 사용자 원본을 손상시키지 않았으며 희귀 실패의 기기 재현은 PC 증거로 대체한 범위다. |
| UC-21 멸망·결과·새 판 | [종료 회귀][ending]의 `EndingSceneCompletesTwoPaidRunsAndPreservesFileOnFailedRestart`, [운영 입력][operations]의 결과 Restart 1회, [오류/종료 화면][screens]의 연출/결과 회전, [표시 검사][status-layout]의 세션 교체 후 누적 없음 통과. | 동일 검증 게임의 첫 APK 초기 건설부터 UI 수정 APK를 거쳐 최종 APK의 배양·방출·결과128/120·새 판까지 이어졌다. 새 판은 프로토타입1·유기물4·점수/보고/경과0·새 Thing ID·ON/큰 글씨 보존을 확인했다. 사용자는 보고/결과 정상 응답을 했다. 정확한 연출 중 실기기 세로 회전 시각은 직접 미관찰이며 PC 양방향/잔류 입력 검사와 구분한다. |
| UC-22 읽기·터치·아이콘 | [모바일 UX][mobile]의 `DeviceLargeTextIconsAndBundledKoreanRemainUsableInBothOrientations`, [표시 검사][status-layout], Edit의 safe-area 좌표 변환 검사 통과. | 가로 맵 확대와 상단 작은 아이콘, 양방향 한글·큰 글씨 화면 및 사용자 가로 조작 정상 응답을 확인했다. density3에서 최소 터치 계산은144출력px/48dp이며 약65px 시각 배경과 구분한다. 메뉴/접기 화면 크기·PC 기하와 대조했으며 개별 버튼 경계를 실기기에서 전수 계측하지 않았다. 모든 긴 문구/목록 규모 및 폰트 라이선스 메뉴의 직접 클릭도 이번 기기 증거로 주장하지 않는다. |
| UC-23 저장·불러오기·새 판 | [모바일 UX][mobile]의 `DeviceGameMenuSaveAndReloadRequireExplicitConfirmationAndCancelPreservesDraft` 및 Restart/Back 검사 통과. 저장 증가, 확인 전 세션/파일 보존, 취소 시 초안 보존, 확인 후 저장 ID 복원과 새 판의 새 ID를 검사했다. | 기기 게임 메뉴 저장→불러오기 취소→재확정으로 기존 시설/설정 복원을 확인했다. 결과 새 판 뒤 건설 초안 중 게임 메뉴의 새 판 확인을 취소하자 같은 (4,3) 초안·배치 버튼·일시정지 표시가 유지됐고 이어 건설을 확정했다. 게임 메뉴 새 판 확정/실패의 전체 조합은 PC 증거이며 결과 버튼의 실제 새 판 성공과 구분한다. |

## Android 실행 절차의 해석

아래는 이번 검증에 사용한 절차 범위이며 새 필수 대기 목록이 아니다. 실제 수행·통과는 위 UC별 표와 [기기 기록][android]에 한정한다. 핵심 정상 흐름·터치·일반 회전·ON/OFF·재실행 설정·결과/새 판과 사용자 원본 복원을 확인했다. 드문 실패 주입·모든 입력 수명 조합 등은 명시된 PC 검사로 확인했고, 기기에서 전수 관찰하지 않은 항목을 실제 통과 수치로 만들지 않았다. APK·기기·방향과 직접 사용자 응답은 실행 기록에서 구분한다.

1. 새 APK를 오프라인 실행하고 최초 진입/이어하기, 상단 현황·4종 건설 설명, 게임 메뉴 저장 피드백을 확인한다. 준비된 상태를 주입했다면 초기 건설부터 직접 진행한 기록과 구분한다.
2. 가로·세로 각각 아이콘 축소/확대/Fit, 겹친 개체 후보와 목록 선택, 요약/상세/접기/닫기, 큰 글자, 실제 두 손가락 핀치와 한 손가락 전환, 패널 스크롤→맵 경계를 직접 조작한다.
3. 일반 관찰에서 같은 표식 1·2·3회 탭으로 설명 펼침/접힘/펼침, 아이콘·바·선택·패널·스크롤 보존과 줌/회전 뒤 접힘 유지를 확인한다. 신규 건설/이동 × 상태 ON/OFF에서는 표식 위 탭이 설명 전환 없이 미리보기로만 이어지는지 확인하고 회전→취소/확정을 반복한다. 자재 부족 부지 확정, 회전한 시설의 접근 경고, 생산·자동·재료/입고 대기의 오버레이와 선택 상세를 비교한다.
4. 배양 현재/다음 대상, 보급형 배정/해제·프리셋/개별 설정, 저장소 허용 해제·재고 이동, 관련 대상 링크를 조작한다. 드문 상태 fixture는 준비 단계와 실제 조작 결과를 나눠 기록한다.
5. 선택한 침공형을 방출하고 외부 목록/상세·옛 맵 표식 해제를 확인한다. 보고를 읽는 중 새 보고·보존 정리·최근 보고 복귀를 확인하고, 새 UI로 한 판의 초기 건설→생산→배양→방출→멸망→결과→새 판을 연결한다.
6. 실제 Android Back과 센서 회전을 후보/상세/메뉴/확인/배치/연출/결과에서 확인한다. 맵 또는 버튼을 누른 채 회전·Home 후 복귀해 잔류 선택/건설/재시작이 없는지 확인한다.
7. 표시 OFF와 큰 글자를 선택한 뒤 새 판·Home 복귀·프로세스 종료/재실행을 비교한다. 마지막 저장이 복원되고 종료 중 게임 시간이 누적되지 않으며 UI 설정이 독립 유지되는지 확인한다. 미확정 초안은 메모리 유지 복귀에서만 보존한다.
8. 같은 시설 규모에서 ON/OFF를 비교하며 프레임 끊김·입력 지연과 실제 최소 터치 영역·노치/제스처 영역 침범을 기록한다. PC 갱신 루틴 측정값을 Android 프레임 성능으로 대체하지 않는다.

## 비용·누적 검사에 대한 제한

[표시 검사][status-layout]의 `RepeatedStatusPanelOrientationAndSessionChangesDoNotAccumulateUiObjects`는 패널/토글/방향 전환 10회와 새 판 3회에서 오버레이·버튼 행 수가 누적되지 않음을 확인한다. `TwentyFacilityStatusRefreshReportsEditorCpuReferenceWithoutTimingPassClaim`의 통과는 측정 실행·제한 시간·객체 수 조건을 뜻한다. 실제 중앙값/2ms 참고 기준 충족 여부는 실행 로그의 수치로 따로 판정한다. 동일 20시설 중 표시 대상 19개, 1280×720·Fit·정지 상태, OFF/ON 각각 준비 60회와 측정 120회의 동기 상태 갱신 호출을 사용한다. 전체 프레임 CPU/GPU·프레임당 할당·장시간 최적화 완료를 뜻하지 않는다.

[최신 조작부 축소 실행 로그](../ui-observations-compact.txt)는 OFF 0.0044ms·ON 0.87085ms·증가 0.86645ms로 PC 참고 기준 2ms 이내이고, 전환 10회·새 판 3회의 누적은 0개다. 기기에서는 ON/OFF 조작과 사용자 정상 응답을 확인했으며 Android 프레임/입력 지연 시간을 수치로 계측하지 않았다. 이전 `ui-observations.txt`의 0.8491ms는 변경 전 이력이다.

[android]: ../android-device-run/README.md
[bootstrap]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/GameDataStoreTests.cs
[initial]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.cs
[input]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.Input.cs
[operations]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.InputOperations.cs
[mobile]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.MobileUx.cs
[reading]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.ReadingContext.cs
[status-layout]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.StatusLayout.cs
[screens]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.ScreenStates.cs
[layout]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.Layout.cs
[production]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.Production.cs
[invasion]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.Invasion.cs
[ending]: ../../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.Ending.cs
[status]: ../../../../../UnityProject/Assets/Scripts/Tests/EditMode/Foundation/Gameplay/FacilityStatusQueryTests.cs
[settings]: ../../../../../UnityProject/Assets/Scripts/Tests/EditMode/Foundation/Persistence/UiSettingsStoreTests.cs
[layout-edit]: ../../../../../UnityProject/Assets/Scripts/Tests/EditMode/Foundation/Presentation/LaboratoryUiLayoutTests.cs
[gui]: ../../../../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs
[navigation]: ../../../../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Navigation.cs
[details]: ../../../../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Details.cs
[map]: ../../../../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.cs

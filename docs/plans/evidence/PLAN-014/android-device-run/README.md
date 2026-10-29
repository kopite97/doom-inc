# PLAN-014 Android 실기기 검증 기록

대상: [PLAN-014](../../../PLAN-014-mobile-ui-ux-and-facility-status.md). 첫 APK 관찰은 **2026-10-29 21:59:01 KST(12:59:01 UTC)**, 캡션 재탭 수정 APK 관찰은 **22:21:37 KST**, 조작부 축소 APK와 원본 복원 기록은 **22:58:57 KST(13:58:57 UTC)**까지 선별했다. 핵심 실기기 흐름과 복원 결과, PC 검사로 확인한 조합, 이번 기기에서 직접 관찰하지 않은 제한을 구분한다. PLAN 상태 판정은 대상 PLAN 본문을 따른다.

첫 APK에서 오프라인 초기 진입과 세로 UI의 건설·배양 대상 선택·저장소 설정·시설 상태 표시 전환을 진행했다. 아래 동작 파일은 ADB 단일 포인터 조작 기록이다. 이후 사용자는 직접 조작의 기능적 이상은 없었다고 확인하고 상태 설명을 재탭으로 접는 UX를 요청했다. 직접 응답과 자동 입력 증거를 구분하며 수정 APK의 검증 완료로 확대하지 않는다.

## 환경과 실행 자료

| 항목 | 확인 내용 |
| --- | --- |
| 기기 | Samsung Galaxy S10 계열 SM-G977N, Android 12 |
| 화면 | 물리 1440×3040, 설정 출력 1080×2280. 선별 세로 캡처 1080×2280, 가로 배치 조정 전 캡처 2280×1080 |
| 밀도 | `wm density`의 물리 420 / 설정 480. 화면 밀도 조회이며 버튼마다 실제 터치 영역을 계측한 결과와 구분한다. |
| 앱 | `com.DefaultCompany.urp_2d`, `UnityPlayerGameActivity` |
| APK 출처 | [Android 빌드 보고](../android-build-20261029-124427-855.json), [빌드 소스 기록](../android-ascii-source.json) |
| 첫 APK 빌드 | 21:50:24 KST `Succeeded`, Android IL2CPP·ARM64, 오류 0·경고 0. 실제 APK 파일 46,592,314바이트 |
| 첫 설치 APK SHA-256 | `fdfecb08f4c2f8793b568da3632ff7de31ae22cb362c96d33cbd35fb203cae7a` — 21:54:01 KST 설치 확인 기록 |
| 수정 APK 빌드·설치 | [수정 빌드 보고](../android-build-20261029-131259-324.json), 실제 46,682,127바이트. 22:18:00 KST 설치 후 설치 경로의 SHA-256 `c3d0989f5ab90edd438de852087a4a0798eb197a61a5b22586a7957158d8b307` 대조 |
| 조작부 축소 APK 빌드·설치 | [빌드 보고](../android-build-20261029-133118-689.json), 실제 66,262,943바이트. [설치 기록](../android-compact-install.json)의 서명 검증 및 22:37:37 KST 설치 후 SHA-256 `26946b22789989c5ed38ea41ec6fb45aceec56dcedff80be85a601a957375173` 대조 |
| 오프라인 조건 | 21:54:03 KST `wifi=0`, `mobile=0`, `airplane=0` 기록 후 cold launch. 모든 네트워크 호출의 부재를 판정한 것은 아님 |
| 입력과 관찰 | ADB `input touchscreen tap/swipe`, 화면 캡처, 실제 앱이 저장한 게임/표시 설정 파일 조회. 저장 내용 편집이나 시간 주입으로 진행을 만들지 않음 |

전체 로컬 실행 자료 중 `Temp/PLAN-014-device-run-20261029/observations/`와 `actions.jsonl`에서 필요한 항목만 선별했다. [선별 동작 기록](actions-selected.jsonl)에는 APK별 설치·오프라인·시작·입력·캡처·스냅샷·백그라운드 전환·자동 회전 설정·검증 앱 강제 종료 시각이 있으며 UTC로 기록된다. 이 문서의 시각은 KST다. [선별 원본 복사 54개 목록과 SHA-256](selected-evidence.json)으로 일치를 확인했다. 별도로 작성한 [복원 영수증](restore-receipt.json)과 [로그 검사 요약](device-log-summary.json)은 공개 가능한 결과만 담는다.

사용자 원본 저장·원본 UI 설정·원본 APK·기기 일련번호는 공유 자료에 포함하지 않았다. 문서 작성 과정에서 로컬 `userdata/`의 원본 파일을 열거나 복사하지 않았다. 검증 도구는 원본 저장의 존재·정확한 바이트와 UI 설정의 원래 부재, 기기 설정을 복원해 대조했고, 아래 영수증에는 원본 내용·원본 해시·기기 일련번호를 제외했다.

## 첫 APK에서 확인한 동작

| 시각·범위 | 실제 관찰과 판정 범위 | 증거 |
| --- | --- | --- |
| 21:54 초기 진입 | 오프라인 실행 후 한국어 HUD, 카메라 아이콘, 상태 ON, 건설/생명체/침공·보고 메뉴가 표시됐다. 초기 저장은 보조형 프로토타입 1개체·유기물 4개·멸망 점수 0·phase 1이며 표시 설정 파일은 아직 없었다. | [초기 세로 화면](fresh-ready-portrait.png), [초기 게임 저장](fresh-ready-session.json) |
| 21:54~21:59 신규 건설 | 실제 건설 목록과 위치 확정 버튼을 사용했다. 마지막 선별 저장에서 채집 1·가공 1·배양 1·저장소 2개, 총 5개 시설의 `construction.isComplete=true`를 확인했다. 가공은 `inProgress=true`였고 맵에서 자동 표식·진행바를 관찰했다. | [건설 목록](build-menu.png), [신규 건설 초안](gather-draft.png), [5개 시설의 후속 저장](before-human-portrait-session.json), [운영 중 상태 표시](storage-allowance-disabled.png) |
| 21:56~21:57 배양 대상 | 배양 시설 상세에서 보급형 버튼을 선택했다. 후속 저장의 `selectedTarget=1`로 반영을 확인했다. 21:59 저장은 `activeTarget=0`, `inProgress=false`이므로 보급형 생성 완료나 실제 배양 회차 시작을 확인한 증거로 사용하지 않는다. | [대상 선택 UI](cultivation-targets.png), [선택 반영 저장](support-selected-session.json), [후속 저장](before-human-portrait-session.json) |
| 21:58 저장소 허용 설정 | **범용 저장소 #5**의 유기물 허용을 해제했다. 화면에는 유기물 차단 중, 실제 재고 20/용량 20이 표시됐고 후속 저장도 해당 저장소의 `allowsOrganic=false`와 유기물 20개를 유지했다. `#5`는 시설의 생성 순서 표식이며 재고 5개라는 뜻이 아니다. 즉시 삭제되지 않음을 확인했으며 전량 재배치 완료는 아직 판정하지 않았다. | [허용 해제 뒤 실제 재고 화면](storage-allowance-disabled.png), [후속 저장](before-human-portrait-session.json) |
| 21:58~21:59 표시 ON→OFF→ON | OFF 화면에서 시설 위 표식이 숨겨지고 선택 저장소의 강조·요약 카드·상세 진입은 유지됐다. 다시 켠 뒤 별도 UI 설정 파일에 `facilityStatus="on"`, `textSize="normal"`이 저장됐다. 앱 재실행 후 설정 보존은 이 시점에 아직 확인하지 않았다. | [상태 OFF와 선택 요약 유지](status-off-summary.png), [별도 표시 설정](before-human-portrait-ui-settings.json), [선별 동작 기록](actions-selected.jsonl) |

초기 및 후속 검증 저장의 DataHash는 모두 `2578869b88b49ba4855785cf587d8cf2f0cfd0dd8ba1fbfc2cd3fb8d2ca7f195`다. 게임 저장과 UI 설정은 별도 파일로 관찰했으며 UI 설정 변경으로 정의 호환 해시가 바뀌지 않았다.

## 사용자 직접 피드백과 후속 UX 변경

세로 직접 조작 요청 뒤 사용자는 처음 문제를 언급했으나, 이어서 **“기능적인 이상은 없었어”**라고 설명했다. 상태 표식을 눌렀을 때 펼쳐지는 설명을 같은 표식의 두 번째 탭으로 접어 아이콘·바만 남기고 싶다는 UX 요청이었다. 따라서 최초 응답을 핀치·스크롤·Home 복귀 등의 기능 결함 확정으로 분류하지 않는다. 사용자 피드백은 직접 관찰 결과이며 각 동작의 내부 이벤트나 정지 시간을 정량 계측한 증거와는 구분한다.

현재 코드는 첫 표식 탭의 대상 선택·설명 펼침, 같은 선택 표식 재탭의 설명 접기/펼치기를 구현했다. 설명만 바꾸고 선택 대상·패널·스크롤 및 아이콘·바를 보존하며, 접힘은 줌/회전 뒤에도 유지된다. 배치 중에는 기존 좌표 미리보기 입력을 유지하고 이 전환을 실행하지 않는다. 표현 상태는 세션/시설 수명 안에서만 유지한다.

변경 이후 [전체 Play 재검증](../playmode-caption-toggle.json)은 **34/34 통과·실패 0·건너뜀 0, 56.54초**를 기록했다. 이는 Editor에서 실행한 결과다. [수정 소스 복사](../android-source-caption-20261029-131139.json)는 537개 파일 일치를 확인했다. [22:11:40 KST 빌드 실행](../android-run-caption-20261029-131139.json)은 고아 공유 컴파일러의 `CS2011` 재발로 실패했으며, 식별된 고아 VBCSCompiler 프로세스 1760 종료 후 22:12:46 KST에 재시도했다. 실행 메타데이터의 `observedResult`와 [수정 APK 빌드 보고](../android-build-20261029-131259-324.json)를 통해 재시도 성공을 구분한다.

수정 APK 빌드는 `Succeeded`, 오류 0·경고 0, 44.4718836초로 완료됐다. 실제 APK는 **46,682,127바이트**, SHA-256은 `c3d0989f5ab90edd438de852087a4a0798eb197a61a5b22586a7957158d8b307`이다.

ADB가 일시적으로 `unauthorized` 상태였고 사용자는 허용 창이 보이지 않는다고 알렸으나, 재연결 후 **22:18:00 KST 수정 APK 설치와 설치 경로 SHA-256 대조를 완료했다.** 빌드 대기 중 22:03:23 KST에 Home으로 나가 보존했던 같은 검증 게임을 재개했다. 아래는 첫 APK의 결과를 재사용하지 않고 수정 APK에서 새로 수집한 증거다.

| 시각·범위 | 실제 관찰과 판정 범위 | 증거 |
| --- | --- | --- |
| 22:18:00~22:18:10 설치·재실행 | 수정 APK의 설치 해시를 대조했다. 재실행한 실제 저장에서 11개 Thing·점수 0·phase 1·기존 DataHash를 확인했으며, 별도 UI 설정은 ON·기본 글씨였다. 동일 검증 게임과 이 설정 조합의 보존을 확인했다. | [선별 설치/시작 기록](actions-selected.jsonl), [로드 게임 저장](caption-revision-loaded-session.json), [로드 표시 설정](caption-revision-loaded-ui-settings.json) |
| 22:18:18~22:18:37 표식 세 번 탭 | ADB 실제 표식 탭으로 가공 시설 P7을 선택하여 설명을 펼쳤고, 같은 표식 재탭으로 설명만 접은 뒤 다음 탭에 다시 펼쳤다. 화면 대조에서 같은 P7 선택·요약 카드·아이콘·바가 유지됐다. | [첫 탭 펼침](caption-first-open.png), [두 번째 탭 접힘](caption-second-collapsed.png), [세 번째 탭 재펼침](caption-third-open.png), [입력 시각 기록](actions-selected.jsonl) |

수정 APK에서 로드한 게임의 DataHash도 `2578869b88b49ba4855785cf587d8cf2f0cfd0dd8ba1fbfc2cd3fb8d2ca7f195`다. 원본 사용자 저장을 복원한 것이 아니라 이번 검증용 게임을 이어서 실행한 결과다.

자동 회전을 켠 뒤 사용자는 일반 메시지와 확인 질문 응답에서 **수정 표식의 접기/펼치기, 실제 가로 회전, 두 손가락 확대·축소에서 한 손가락으로 전환, 맵 이동과 패널 스크롤이 모두 정상**이라고 확인했다. 직접 조작 응답을 수령했으며 이 항목들은 더 이상 사용자 응답 대기 상태가 아니다. ADB 세 번 탭 기록과 사용자 확인은 별도 증거이며, 모든 메뉴·배치·연출 단계의 회전이나 전체 유스케이스 완료를 뜻하지 않는다.

## 가로 맵·조작부 크기 후속 요청

기능 정상 확인 뒤 사용자는 **가로 맵 영역이 작게 느껴지고 네 카메라/상태 조작 버튼은 그렇게 클 필요가 없다**고 요청했다. [22:21:36 KST 조정 전 가로 화면](landscape-before-compact.png)은 2280×1080에서 맵과 조작부가 차지하던 배치를 보존한다. 요청을 반영한 [전체 Play](../playmode-compact-controls.json) 34/34, [새 APK 빌드](../android-build-20261029-133118-689.json) 성공 후 아래 새 기기 검증을 진행했다. 앞서 기록한 `c3d098…` 해시는 캡션 재탭 수정 APK이며 조작부 축소본은 `26946…`로 구분한다.

[22:21:37 KST 검증 저장](before-compact-controls-session.json)은 11개 Thing·점수 0·phase 1·동일 DataHash, [표시 설정](before-compact-controls-ui-settings.json)은 ON·기본 글씨를 유지했다. [동작 기록](actions-selected.jsonl)에 따라 곧바로 Home으로 나가 같은 검증 게임을 백그라운드에 보존했다. 이때의 검증 게임 보존과 아래 마지막 원본 복원은 별개다.

## 조작부 축소 APK 설치와 가로 운영

22:37:37 KST 설치를 완료했다. [설치 기록](../android-compact-install.json)에 첫 스트리밍 설치의 종료 코드 1·진단 문구 없음, `apksigner verify`의 검증 통과, `adb install --no-streaming -r`의 `Success`·종료 코드 0 및 설치된 APK의 해시 일치를 구분해 남겼다. 저장을 초기화하지 않고 동일 검증 게임을 이어서 실행했다.

| 시각·범위 | 실제 관찰과 판정 범위 | 증거 |
| --- | --- | --- |
| 22:39 가로 배치 | 사용자가 기기를 가로로 돌렸다고 확인했다. 실제 2280×1080 캡처에서 네 아이콘 조작부가 상단으로 이동했고 맵/선택 패널과 겹치지 않았다. 같은 크기의 조정 전 선택 화면 대비 맵 표시 높이는 약 407→644px, 약 1.6배였다. 화면상의 영역 비교이며 터치 영역이나 성능 수치가 아니다. | [조정 전](landscape-before-compact.png), [조정 후 선택 화면](compact-landscape-selected.png) |
| 22:40 카메라·표시 | ADB로 확대·축소·전체 보기를 순서대로 조작하고 표시 OFF→ON을 실행했다. OFF에서 표식은 숨겨졌고 같은 P7 선택 강조·오른쪽 시설 패널은 유지됐다. | [OFF와 선택 패널 유지](compact-status-off.png), [동작 기록](actions-selected.jsonl) |
| 22:40~22:41 우선순위·시공 | 전역 채집 우선순위 버튼을 세 번 눌러 0으로 바꿨다. 실제 건설 메뉴로 저장소 S32를 (13,3)에 확정했으며 후속 저장에서 `construction.isComplete=true`를 확인했다. 시간·자원 주입 없이 기존 게임의 운반·시공을 기다렸다. 후속 저장의 프리셋/프로토타입 작업자 우선순위는 `[1,0,3,4]`다. | [채집 제외 UI](gathering-disabled.png), [후속 저장](after-menu-reload-session.json), [동작 기록](actions-selected.jsonl) |
| 22:41 저장·불러오기 | 게임 메뉴에서 저장 후 불러오기 확인 창을 열어 취소하고, 다시 열어 확정했다. 복원 뒤 선택은 해제됐고 기존 시설 6개를 유지했다. 실제 로드 저장은 전체 Thing 10개·점수 0·phase 1·기존 DataHash였으며 UI 설정 ON·기본 글씨도 유지됐다. 전체 Thing 수에는 변동하는 자원 스택이 포함되므로 시설 보존과 구분한다. | [불러오기 확인](compact-reload-confirmation.png), [복원 후 화면](compact-reloaded.png), [로드 저장](after-menu-reload-session.json), [로드 UI 설정](after-menu-reload-ui-settings.json) |
| 22:42 생산·현재/다음 회차 | C12에서 진행 중인 보급형을 유지한 채 다음 대상을 침공형으로 바꿨다. 화면은 현재 보급형·다음 침공형, 자동 기준 속도 20%·진행 51.8%를 구분했고 후속 저장은 `activeTarget=1`, `selectedTarget=2`, `inProgress=true`였다. P7의 입고 대기·진행 100%도 조정 후 선택 화면에서 확인했다. 개체 생성·배정·방출 완료 증거는 아니다. | [현재 보급형/다음 침공형](current-support-next-invader.png), [후속 저장](compact-large-settings-session.json), [P7 입고 대기](compact-landscape-selected.png) |
| 22:42 큰 글씨 | 실제 메뉴에서 글자 크기를 크게로 바꿨다. 가로 화면의 설정 행과 상태 캡션을 확인했고 별도 설정 파일은 `facilityStatus="on"`, `textSize="large"`였다. 모든 긴 문구·세로 표시·재실행 보존까지 판정한 것은 아니다. | [큰 글씨 가로 화면](compact-large-landscape.png), [표시 설정](compact-large-settings-ui-settings.json) |

이 조작 뒤 검증 저장의 DataHash는 계속 `2578869b88b49ba4855785cf587d8cf2f0cfd0dd8ba1fbfc2cd3fb8d2ca7f195`다. 22:42:41 KST에 Home으로 나갔고 22:43:05 KST에 앱 프로세스를 종료해 아래 실제 재실행 확인을 진행했다.

## 실제 재실행 복원과 직접 조작 확인

22:43:04의 [종료 전 저장](before-cold-restart-session.json)과 22:43:56의 [앱 정지 중 저장](stopped-no-offline-progress-session.json)은 15,156바이트·SHA-256이 같았고 게임 경과 시간도 1053.0971997초로 같았다. 22:43:57에 다시 실행한 뒤 [22:44:12 저장](after-cold-restart-session.json)은 1063.1196993초로 약 10.02초만 증가했다. 앱을 닫아 둔 약 52초를 일괄 가산하지 않았으며, 10개 Thing·점수 0·같은 DataHash와 [ON·큰 글씨](after-cold-restart-ui-settings.json)를 유지했다. [실제 복원 화면](compact-cold-restored.png)은 세로 1080×2280이다.

이후 사용자는 조작부 축소 APK의 **가로 배치와 조작이 정상**이라고 직접 확인했다. [22:45:55 가로 화면](compact-human-confirmed.png)을 보존했다. 이는 사용자의 실제 조작 확인이며 프레임 시간·터치 지연을 수치로 계측한 결과와 구분한다.

밀도 조회는 480dpi, 밀도 배율 3이다. 코드의 최소 터치 크기 `max(48, 48 × density / Canvas.scaleFactor)`를 이 환경에 적용하면 144 출력 px, 48dp 상당이다. 가로 Canvas 배율 약 1.8에서 36 Canvas 단위의 작은 아이콘 배경은 약 65px지만 버튼 입력 영역과는 다르다. 실제 메뉴/접기 버튼의 약 144px 화면 크기·정상 조작과 PC 큰 글씨 기하 검사를 대조했으며, 개별 버튼의 터치 경계를 실기기에서 전수 계측했다고 주장하지 않는다.

## 생성·배정·침공·결과·새 판

| 시각·범위 | 실제 관찰과 판정 범위 | 증거 |
| --- | --- | --- |
| 22:45~22:47 보급형·우선순위 | 보급형 #41이 자연 생성된 뒤 C12에 배정했다. 실제 저장의 `assignedFacilityId`는 C12이며 개별 우선순위 `[1,0,3,0]`, `usesPreset=false`다. 생산 끔 상태까지 UI로 바꿨고 배정 자체와 실제 생산 작업을 구분한다. 이 저장의 `hasJob=false`를 생산 가속 중으로 해석하지 않는다. | [개별 생산 끔](support-production-off.png), [배정/우선순위 저장](support-priority-assigned-session.json) |
| 22:47 겹친 개체 | 같은 셀의 개체 후보를 열고 침공형을 선택한 뒤 방출 메뉴에 접근했다. 모든 개체 수·동시 행 제거 조합을 재현한 것은 아니다. | [겹친 개체 후보](overlap-candidates.png), [동작 기록](actions-selected.jsonl) |
| 22:48~22:49 방출·보고 | 침공형 2마리를 방출했고 화면은 외부 2·방출 대기 0을 표시했다. 22:49 저장에는 외부 2마리와 새 대기 1마리, 점수 64·보고 16개가 있었다. 새 대기 개체가 자연 생성된 이후 저장과 직후 화면의 시점을 구분한다. | [방출 직후 외부 2](released-and-reports.png), [점수/보고 저장](report-observation-session.json) |
| 22:49~22:50 결과 | 목표 도달 뒤 `phase=3`, 점수 128, 보고 20개의 결과 저장을 확인했다. 화면은 128/120·완료 외부 활동 32회·외부 침공형 2마리를 표시했고 사용자는 **“보고와 결과 화면 모두 정상”**이라고 답했다. | [결과 화면](result-after-human.png), [결과 저장](ending-observation-session.json) |
| 22:50 새 판 | 결과의 새 판 버튼으로 돌아왔다. 초기 프로토타입 1개체·유기물 4개, phase 1·점수/경과 시간/보고 0·침공형 0을 확인했다. 이전 판의 Thing ID를 재사용하지 않았고 기본 우선순위 `[1,2,3,4]`로 복귀했으며 UI 설정 ON·큰 글씨는 유지됐다. | [새 판 화면](restarted-fresh-run.png), [새 판 저장](restarted-fresh-run-session.json), [표시 설정](restarted-fresh-run-ui-settings.json) |

이번 동일 검증 게임은 첫 APK에서 초기 건설을 시작하고 두 UI 수정 APK에서도 저장을 보존하여 최종 APK의 결과·새 판까지 이어졌다. 자원·시간·세션 상태를 주입해 이 흐름을 만들지 않았다. 연출 중 세로 회전을 요청했지만 수집한 결과 화면은 2280×1080 가로다. 사용자 정상 응답을 기록하되 **연출 중 회전한 정확한 시각·세로 결과 전환을 직접 관찰했다고 주장하지 않는다.** 일반 실기기 회전 정상과 PC의 연출/결과 양방향 검사는 별도 증거다.

## 새 판 확인 취소와 표시 OFF 중 이동

22:51~22:52 새 저장소 건설 미리보기 중 게임 메뉴의 새 판 확인을 열고 취소했다. [확인 경고](draft-restart-warning.png) 뒤 [같은 초안·위치 (4,3)·배치 버튼·연구소 일시정지 표시](draft-preserved-after-menu-cancel.png)가 유지됐고, 이후 실제 부지를 확정했다.

시공 완료한 저장소 #3을 22:58에 [선택](storage-selected-final.png)하여 [90° 회전 이동 미리보기](storage-move-rotated-preview.png)를 만들고 취소했다. [취소 후 화면](storage-move-cancelled.png)은 원래 위치 (4,3)와 일치했다. 다시 상태 표시를 OFF로 두고 [90° 회전·(10,3) 미리보기](storage-move-off-valid.png)를 [확정](storage-move-off-confirmed.png)했다. [자동 저장 이후 파일](final-device-before-restore-session.json)은 (10,3)·`rotation=1`·2×3 배치·시공 완료, 전체 유기물 2개·저장소 재고 0을 유지했고 [표시 설정](final-device-before-restore-ui-settings.json)은 OFF·큰 글씨였다.

## 원본 복원과 로그

22:58:57 KST [공개 복원 영수증](restore-receipt.json)은 원본 게임 저장의 존재와 정확한 내용 해시 일치, 원래 없던 UI 설정 파일의 부재 복원을 확인한다. 기기 설정은 Wi-Fi 1·모바일 데이터 1·자동 회전 0·회전 0·비행기 모드 0으로 검증 전 값과 모두 같았다. 앱을 종료 상태로 두어 복원 파일이 자동 저장으로 바뀌지 않게 했으며 **최신 검증 APK는 계속 설치된 상태**다. 원본 APK로 되돌린 결과가 아니다.

[최종 수집 로그 요약](device-log-summary.json)에서 Unity 오류/치명 태그·치명 예외/신호·ANR 표식은 각각 0건이다. 수집한 앱 로그 구간의 결과이며 모든 시스템 로그에 오류가 없거나 과거 모든 실행을 검사했다는 의미가 아니다.

## 검사 완료 범위와 관찰 제한

- UC-12는 [별도 Play UI 경유 검사](../playmode-related-target.json) 1/1 통과로 보완했다. 양방향 자재 대기→저장소/설정, 배정 시설↔작업자 링크를 실제 입력으로 확인했으며 Android에서 같은 링크를 직접 클릭한 기록이나 개별 운반 예약 링크 전수 검사로 확대하지 않는다.
- 핵심 기기 흐름·터치/일반 회전·ON/OFF·재실행 설정·결과/새 판·원본 복원은 위 증거 범위에서 확인했다. 사용자의 직접 확인과 ADB 단일 포인터 조작을 구분한다.
- PC에서 오류 주입·조합 검사를 수행한 드문 로딩/저장 실패, 모든 메뉴/배치 단계의 잔류 입력·양방향 조합, 보고 정리 한도의 모든 상황을 기기에서 다시 전수 재현하지 않았다. 이는 이번 기기의 관찰 제한이며 미구현 기능이나 자동 검사 실패가 아니다.
- 실제 연출 중 세로 회전 시각·최대 규모 장시간 프레임 성능·개별 터치 경계 전수 측정은 직접 입증 범위 밖이다. 밀도 기반 목표 계산·PC 기하/비용 검사와 실제 화면·정상 조작을 대조한 범위만 주장한다.
- 이 기록은 선별 세로·가로 관찰과 위 사용자 직접 확인을 뒷받침한다. 모든 시설 상태·최소/최대 줌·밀집·가독성·터치 크기·장시간 성능·다른 기종까지 통과한 것으로 확대하지 않는다.

실행이 이어지면 새 증거와 사용자 직접 응답을 구분하여 이 문서를 갱신한다. 과거 PC 검증이나 PLAN-013 실기기 결과를 이번 APK의 실기기 통과 수치로 재사용하지 않는다.

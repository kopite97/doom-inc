# PLAN-014 검증 기록

대상은 [PLAN-014](../../PLAN-014-mobile-ui-ux-and-facility-status.md)이다. 핵심 실기기 흐름·결과/새 판·사용자 데이터 복원과 관련 대상 링크의 추가 UI 경유 검사를 확인했다. 각 실행의 실제 통과 범위와 관찰 제한을 기록하며 PLAN 상태는 대상 본문을 따른다.

## 환경과 범위

- Unity 6000.3.27f1, Unity CLI beta.12, Pipeline 0.9.1-exp.1. 기존 열린 Editor 연결을 사용한다.
- 패키지·게임 규칙·CSV·저장 snapshot 스키마 변경 없이 UI·상태 조회·독립 설정·임시 에셋을 검증한다.
- 이번 실행의 APK와 실기기 결과를 PLAN-013의 통과 결과로 대신하지 않는다.

## 실행 기록

| 실행 | 결과 | 해석 |
| --- | --- | --- |
| [Edit 1](editmode-01.json) | 286개 중 279 통과·7 실패 | 잘못된 UI 설정의 예외 처리와 JSON null 기본값 복구 결함을 발견했다. |
| [Edit 2](editmode-02.json) | 286/286 통과, 17.03초 | UI 설정 수정 후 전체 Edit 회귀 통과. |
| [Play 1](playmode-01.json) | 32개 중 15 통과·17 실패 | 입력 장치의 Editor 포커스 격리, 한국어 문구 기대값, 테스트 자체 진단 로그의 명시적 기대 처리를 후속 실행에서 수정했다. |
| [Play 2](playmode-02.json) | 34개 중 33 통과·1 실패 | 남은 Back 테스트를 배치 취소 후 원래 건설 패널 복귀 순서에 맞췄다. |
| [Play 3](playmode-03.json) | 34개 중 33 통과·1 실패 | 새 글자 너비 검사에서 큰 글씨의 펼치기 버튼 잘림을 발견하고 버튼 너비를 수정했다. |
| [큰 글씨 재검사](playmode-caption.json) | 1/1 통과 | 아이콘·닫기·접기/펼치기·세로 상세 문구의 실제 폭/높이와 클릭 경로를 확인했다. |
| [Play 4](playmode-04.json) | 34/34 통과 | 첫 APK에 대응하는 전체 Play 회귀. 이후 [Editor](editor-after-tests.json)의 Play 종료·컴파일 완료를 확인했다. 아래 캡션 재탭 변경 이후의 통과 결과로 재사용하지 않는다. |
| [캡션 재탭 변경 후 전체 Play](playmode-caption-toggle.json) | 34/34 통과·실패 0·건너뜀 0, 56.54초 | 같은 표식 재탭의 설명 접기/펼치기와 기존 전체 Play 회귀를 새 실행으로 확인했다. 수정 APK의 실기기 통과와 구분한다. |
| [조작부 축소 변경 후 전체 Play](playmode-compact-controls.json) | 34/34 통과·실패 0·건너뜀 0, 57.98초 | 가로 맵 영역·조작부 배치 변경 후 같은 34개 검사를 새로 실행했다. 빌드 전 Editor가 컴파일 완료·Play 종료 상태임을 확인했으며 기기 검증과 구분한다. |
| [관련 대상 링크 추가 Play](playmode-related-target.json) | 1/1 통과·실패 0·건너뜀 0, 1.42초 | 양방향 실제 UI 입력으로 자재 대기→저장소 설정, 배정 시설↔작업자 링크의 대상 ID·상세 열림·시야 표시·맵 탭 누출 방지를 확인했다. 전체 34개 검사와 별도 실행이며 개별 운반 예약 링크의 전수 검사는 아니다. |

실패 원본을 보존하고 새 실행은 별도 파일로 남긴다. CLI 호출의 `success: true`는 테스트 전체 통과와 다르며 실제 Summary/개별 결과로 판정한다.

현재 테스트 증거는 위 CLI 결과 JSON의 실행 요약과 개별 결과다. 이전 날짜의 단일 테스트 결과 XML이 Play 1~4에 잘못 연결된 것을 확인하여 해당 연결과 사본을 제외했다. 그 XML로 이번 실행을 검증했다고 기록하지 않는다.

[UC-01~23 연결표](use-cases/README.md)는 자동 검사·소스 대조·남은 Android 직접 확인을 구분한다. 테스트 이름의 Device는 Editor의 합성 입력 장치이며 실기기 통과를 뜻하지 않는다.

## 첫 Android APK 빌드·설치

[독립 ASCII 소스 복사](android-ascii-source.json)는 Assets·Packages·ProjectSettings 537개 파일의 원본/복사본 SHA-256을 대조했다. 첫 두 실행([1](android-ascii-run-01.json), [2](android-ascii-run-02.json))은 Unity의 공유 Roslyn 컴파일러가 응답 파일을 열지 못해 빌드 진입 전 종료했다. 같은 Foundation 컴파일 명령은 `/shared` 없이 성공하고 `/shared`에서 재현되어, 실행 중 검사가 없는 상태에서 식별된 Unity의 고아 VBCSCompiler 프로세스만 종료했다.

[재실행](android-ascii-run.json)과 [첫 APK 빌드 보고](android-build-20261029-124427-855.json)는 2026-10-29 21:50:24 KST에 Android IL2CPP·ARM64 빌드 `Succeeded`, 오류 0·경고 0, 352.48초를 기록했다. 실제 APK 파일은 **46,592,314바이트**이며 SHA-256은 `fdfecb08f4c2f8793b568da3632ff7de31ae22cb362c96d33cbd35fb203cae7a`다.

동일 해시의 APK를 기기에 설치한 뒤 Wi-Fi·모바일 데이터를 끄고 실행했다. [진행 중인 실기기 기록](android-device-run/README.md)에 초기 진입·세로 건설·배양 대상·저장소 설정·상태 ON/OFF와 선별 증거를 기록했다. 이 첫 APK 단계에서는 가로 실기기 검증·전체 사이클·원상 복원을 완료 판정하지 않았으며, 이후 직접 응답은 아래의 캡션 재탭 수정 APK 결과와 구분한다.

## 실기기 피드백에 따른 후속 변경

사용자는 세로 직접 조작에 대해 처음 문제를 언급한 뒤 **“기능적인 이상은 없었어”**라고 명확히 했다. 추가 요청은 시설 상태 표식을 눌러 펼친 설명을 **같은 표식을 다시 누르면 아이콘·바로 접고, 다음 탭에서 다시 펼치는 UX**다. 최초 응답을 기능 결함 확정으로 기록하지 않으며, 이 요청과 기존 동작 검증을 구분한다.

이 동작은 현재 코드에 반영됐다. 같은 시설 재탭은 선택·패널·스크롤을 유지하고 설명만 전환하며, 접힘은 줌/회전의 자동 펼침보다 우선한다. 한 판/시설 수명 안의 표현 상태이며 게임 저장이나 사용자 표시 설정에 영속화하지 않는다. 변경 이후 [전체 Play 재검증](playmode-caption-toggle.json)은 34/34 통과했다.

[수정 소스 복사](android-source-caption-20261029-131139.json)는 537개 파일의 일치를 확인했다. [22:11:40 KST 빌드 실행](android-run-caption-20261029-131139.json)은 고아 공유 컴파일러에 의한 `CS2011`이 재발하여 실패했다. 식별·확인된 고아 VBCSCompiler 프로세스 1760을 종료한 뒤 22:12:46 KST에 재시도했으며 실행 메타데이터의 `observedResult`와 후속 보고로 결과를 구분한다.

[수정 APK 빌드 보고](android-build-20261029-131259-324.json)는 `Succeeded`, 오류 0·경고 0, 44.4718836초를 기록했다. 실제 수정 APK는 **46,682,127바이트**, SHA-256은 `c3d0989f5ab90edd438de852087a4a0798eb197a61a5b22586a7957158d8b307`이다. 첫 APK와 구분하여 캡션 재탭 변경을 포함한 산출물로 보존한다.

ADB 권한 재연결을 마친 뒤 **22:18:00 KST에 수정 APK를 설치하고 기기의 설치 경로에서 같은 SHA-256을 확인했다.** 첫 APK의 설치 증거와 별도로 [선별 동작 기록](android-device-run/actions-selected.jsonl)에 남겼다. 빌드 중 Home으로 나가 보존했던 검증 게임을 다시 실행했으며 [실제 로드 저장](android-device-run/caption-revision-loaded-session.json)은 기존 11개 Thing·점수 0·동일 DataHash를 유지했다. [표시 설정](android-device-run/caption-revision-loaded-ui-settings.json)도 ON·기본 글씨로 유지됐다.

수정 APK에서 ADB로 가공 시설 P7 표식을 실제 세 번 탭했다. [첫 탭 설명 펼침](android-device-run/caption-first-open.png) → [두 번째 탭 설명 접힘](android-device-run/caption-second-collapsed.png) → [세 번째 탭 재펼침](android-device-run/caption-third-open.png)을 화면으로 확인했으며 같은 시설의 선택·요약 카드·아이콘·바가 유지됐다. 이 결과는 수정 APK의 단일 포인터 조작 증거이며 사용자의 직접 터치·가로 검증과 구분한다.

자동 회전을 켠 뒤 사용자는 일반 메시지와 확인 질문 응답에서 **수정된 표식, 가로 회전, 두 손가락 확대·축소에서 한 손가락으로 전환, 맵 이동과 패널 스크롤이 모두 정상**이라고 확인했다. 이는 사용자의 직접 조작 응답이며 ADB 단일 포인터 기록 및 모든 유스케이스의 완료 판정과 구분한다.

## 가로 맵 영역·카메라 조작부 후속 조정

사용자는 기능 정상 확인과 별도로 가로 화면의 맵이 작게 느껴지며 네 카메라/상태 조작 버튼은 크게 표시할 필요가 없다고 요청했다. [조정 전 가로 화면](android-device-run/landscape-before-compact.png)은 2280×1080에서의 당시 배치를 보존한다. 가로 맵 영역 확보와 조작부 축소를 반영한 뒤 [전체 Play](playmode-compact-controls.json) 34/34 통과와 아래 새 APK 빌드를 확인했다. 위 `c3d098…` 산출물은 **캡션 재탭 수정 APK**의 기록이며 이번 배치 조정의 산출물과 구분한다.

22:21:37 KST [검증 저장](android-device-run/before-compact-controls-session.json)은 동일한 11개 Thing·점수 0·DataHash, [표시 설정](android-device-run/before-compact-controls-ui-settings.json)은 ON·기본 글씨를 유지했다. 이후 Home으로 나가 같은 검증 게임을 일시정지했다. 남은 유스케이스·전체 흐름과 원본 사용자 저장·기기 설정 복원은 완료 판정하지 않았다.

[소스 복사 기록](android-source-compact-20261029-133107.json)은 원본/복사본 537개 파일 일치를 확인했다. [빌드 준비 기록](android-compact-compiler-preparation.json)에 따라 Editor의 검사·Play 종료 후 부모 프로세스 24208이 없는 고아 VBCSCompiler 3140을 확인해 종료하고 [22:31:07 KST 빌드 실행](android-run-compact-20261029-133107.json)을 시작했다.

[조작부 축소 APK 빌드 보고](android-build-20261029-133118-689.json)는 22:32:29 KST에 `Succeeded`, 오류 0·경고 0, 69.735초를 기록했다. 실제 APK 파일은 **66,262,943바이트**, SHA-256은 `26946b22789989c5ed38ea41ec6fb45aceec56dcedff80be85a601a957375173`이다. [설치 기록](android-compact-install.json)은 첫 스트리밍 설치가 진단 문구 없이 종료 코드 1로 끝난 뒤 APK 서명 검증 통과, `--no-streaming -r` 재시도 성공과 설치 경로 해시 일치를 확인했다. [동작 기록](android-device-run/actions-selected.jsonl)에 22:37:37 KST 설치 완료를 남겼다.

이번 Play 실행의 새 화면도 보존했다: [가로 정상 ON](screens/compact/plan014-status-normal-on-1280x720-20261029-132945-763.png)·[OFF](screens/compact/plan014-status-normal-off-1280x720-20261029-132945-810.png), [가로 밀집 ON](screens/compact/plan014-status-dense-on-1280x720-20261029-132945-907.png)·[OFF](screens/compact/plan014-status-dense-off-1280x720-20261029-132945-953.png), [세로 밀집 ON](screens/compact/plan014-status-dense-on-720x1280-20261029-132946-299.png)·[OFF](screens/compact/plan014-status-dense-off-720x1280-20261029-132946-337.png). PC 화면을 새 APK의 실기기 표시·입력 통과로 대신하지 않는다.

사용자가 기기를 실제 가로로 돌린 뒤 [조정 후 선택 화면](android-device-run/compact-landscape-selected.png)을 수집했다. 같은 2280×1080의 조정 전 화면과 비교해 맵 표시 높이는 약 407→644px, 약 1.6배로 늘었다. 네 조작부는 상단의 작은 아이콘 배경으로 이동했으며 선별 화면에서 겹침이 없었다. ADB로 확대·축소·전체 보기·표시 OFF→ON을 조작했고 [OFF에서도 선택 패널](android-device-run/compact-status-off.png)이 유지됐다.

같은 검증 게임에서 저장소 S32를 실제 시공해 완성하고, 전역 채집 우선순위를 0으로 바꿨다. 게임 메뉴의 저장·불러오기 취소·재확인을 거쳐 [실제 로드 저장](android-device-run/after-menu-reload-session.json)에서 시설을 유지했다. [배양 현재 보급형·다음 침공형](android-device-run/current-support-next-invader.png), 자동 생산과 입고 대기 100% 표시, [큰 글씨 가로 화면](android-device-run/compact-large-landscape.png)도 확인했다. 상세 시각·판정 범위는 [Android 기록](android-device-run/README.md)에 남겼다.

Home 저장 후 프로세스 종료·재실행을 확인했다. [종료 전](android-device-run/before-cold-restart-session.json)과 [정지 중 저장](android-device-run/stopped-no-offline-progress-session.json)은 바이트·해시가 같았고, [재실행 저장](android-device-run/after-cold-restart-session.json)은 기동 후 약 10.02초의 진행만 반영했다. [ON·큰 글씨 설정](android-device-run/after-cold-restart-ui-settings.json)도 유지됐다. 사용자도 조작부 축소 APK의 가로 배치·조작이 정상이라고 확인했다.

같은 판에서 보급형 #41 생성·C12 배정·개별 우선순위 변경, 침공형 2마리 방출과 보고를 이어서 확인했다. [결과 화면](android-device-run/result-after-human.png)은 점수 **128/120**, 외부 활동 32회·외부 침공형 2마리를 표시했고 사용자는 **“보고와 결과 화면 모두 정상”**이라고 답했다. [새 판 저장](android-device-run/restarted-fresh-run-session.json)은 새 Thing ID·프로토타입 1개체·유기물 4개·점수/보고 0, [표시 설정](android-device-run/restarted-fresh-run-ui-settings.json)은 ON·큰 글씨를 유지했다. 최초 건설은 첫 APK에서 시작했고 UI 수정 APK들에서도 같은 검증 게임을 보존해 최종 APK의 결과·새 판까지 연결했다.

새 판에서 [새 판 확인 취소 뒤 건설 초안 유지](android-device-run/draft-preserved-after-menu-cancel.png), 저장소 이동·90° 회전 취소 후 원위치 유지, [표시 OFF 중 이동 확정](android-device-run/storage-move-off-confirmed.png)을 확인했다. [자동 저장 반영 후 파일](android-device-run/final-device-before-restore-session.json)은 저장소 (10,3)·회전 1·시공 완료와 전체 유기물 2개를 유지했다.

22:58:57 KST에 원본 게임 저장의 존재/바이트와 원래 없던 UI 설정 파일의 부재, 네트워크·회전 설정을 검증해 복원했다. [비공개 정보를 제외한 영수증](android-device-run/restore-receipt.json)을 남겼고 앱은 종료 상태로 두었으며 최신 검증 APK는 설치된 상태다. [수집 로그 요약](android-device-run/device-log-summary.json)의 Unity 오류·치명 종료·ANR 표식은 모두 0건이며 수집 구간 밖까지 보장하지 않는다.

실기기의 연출 중 세로 회전 시각은 직접 관찰하지 못했다. 사용자의 보고/결과 정상 응답과 2280×1080 가로 결과 캡처, PC 연출/결과 양방향 검사를 구분한다. **UC-12 관련 대상 링크의 별도 Play UI 경유 검사도 1/1 통과했다.** 기기에서 모든 예외 조합을 재현하지 않은 항목은 [UC 연결표](use-cases/README.md)에 PC 검사와 관찰 제한으로 구분했다.

[최종 소스 대조](final-source-review.json)는 빌드 기준 537개 중 534개가 같고 차이는 폰트/아트 요청 README 두 개와 `UNITY_EDITOR` 전용 추가 검사 한 개뿐임을 확인했다. 런타임·씬·에셋 데이터·패키지/프로젝트 설정 변경과 누락 메타는 없고 APK 해시는 `26946…`로 같다. 이 추가 검사를 위해 Android APK를 다시 생성할 런타임 변경은 없다.

## 표시 비용의 해석

사전 조건은 PLAN 본문에 기록했다. 시설 20개(오버레이 19개), 1280×720·Fit·시뮬레이션 정지에서 OFF/ON 각각 60회 준비·120회 측정한다. `RefreshStatusOverlays` 호출 중앙값 증가 2ms를 PC 참고 기준으로 삼으며 전체 프레임/GPU/Android 성능을 의미하지 않는다. 관리 힙 전후 차이는 프레임당 할당량으로 해석하지 않는다. 반복 객체 검사와 실제 기기의 조작 지연 확인도 별도로 수행한다.

[조작부 축소 후 최신 표시 로그](ui-observations-compact.txt)의 중앙값은 OFF **0.0044ms**, ON **0.87085ms**, 증가 **0.86645ms**로 PC 참고 기준 2ms 이내다. 표식 19개, OFF 0개, 반복 전환 10회·새 판 3회에서 누적 0개와 보고 앵커·선택 시야 보존을 확인했다. 위 `screens/compact/`의 13:29:45~46 화면과 같은 실행이다. [이전 표시 로그](ui-observations.txt)의 증가 0.8491ms와 기존 화면은 변경 전 이력이며 최신 수치로 재사용하지 않는다.

기기에서는 표시 ON/OFF와 실제 조작을 확인하고 사용자에게 정상 응답을 받았다. Android 전체 프레임·입력 지연 시간을 정량 계측하거나 모든 규모의 장시간 성능을 통과했다고 주장하지 않는다.

## 사용자 데이터와 아트

사용자의 원본 저장·UI 설정·기기 식별자·원본 APK는 로컬 Temp에서만 보호하며 공유 증거에 포함하지 않는다. 검증 저장과 화면·필요 로그만 선별한다. 정식 아트는 [요청 README](../../../../UnityProject/Assets/Sprites/Require/PLAN-014/README.md)의 별도 art PLAN 경계를 따른다.

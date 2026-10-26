# PLAN-008 검증 증거

관련 작업: [PLAN-008](../../PLAN-008-initial-construction-and-logistics.md), 기술 선택: [TECH-005](../../../decisions/TECH-005-initial-laboratory-and-jobs.md).

## 환경과 실행

- 날짜: 2026-10-26, Windows PC Editor. Unity 6000.3.27f1·Universal 2D(URP), Android 대상 유지.
- CLI 1.0.0-beta.12 / Pipeline 0.9.1-exp.1 / Test Framework 1.6.0. 열린 Editor 하나에서 순서대로 실행했다.
- 기준 커밋: `3683436a91927c7eb714f30922f556606730726f`에 이번 작업의 미커밋 변경을 적용했다. 이전 커밋 자체의 통과 기록이 아니다.
- 검증한 소스·SO·씬·meta·패키지/설정 297개 파일의 [SHA-256 목록](2026-10-26-source-sha256.json)을 보관했다.
- [최종 소스 재확인](2026-10-26-final-source-check.json)에서 297개 해시 불일치·누락 meta·Scripts 밖 C#·기존 meta 변경이 모두 0임을 확인했다.
- 실제 코드·정의·장면은 `UnityProject/Assets/`, 패키지·설정은 기존 `Packages/`·`ProjectSettings/`를 사용했다. 엔진·패키지 교체는 없다.
- 실제 슬롯은 `Application.persistentDataPath/Gameplay/session.json`. 검사에서는 개별 임시 디렉터리를 사용하여 실제 슬롯을 덮어쓰지 않았다.

## 재현 명령과 직접 실행

저장소 루트의 PowerShell에서 실행한다. 각 비동기 실행은 완료 상태와 실제 테스트 수를 확인한 뒤 다음 작업으로 넘어간다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command editor status --project-path $projectPath --json
unity command script recompile --project-path $projectPath --json
unity command script recompile_status --project-path $projectPath --json
unity command editor invoke_menu --path 'Tools/Mobile Game/Open Laboratory Scene' --project-path $projectPath --json
unity command test run --mode editor --project-path $projectPath --timeout 60 --json
unity command test run --mode playmode --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
unity command console status --project-path $projectPath --json
```

이번에 장면과 정의를 처음 준비할 때는 `Tools/Mobile Game/Create Laboratory Scene` 메뉴를 실행했다. 이미 생성된 장면은 위 Open 메뉴로 연다.
장면은 [Temp_Laboratory](../../../../UnityProject/Assets/Scenes/Gameplay/Laboratory/Temp_Laboratory.unity)다. Play에서 Build gatherer / Build storage → 격자 선택 → 필요 시 Rotate → Confirm 순서로 건설을 지정한다.
개체가 실제 자원을 전달하고 작업한 뒤 시설을 사용할 수 있다. 시설/개체 선택으로 진행·재고·예약·대기 이유·우선순위를 확인하고, 완성 시설의 Move / rotate와 저장소 허용 목록을 조작한다.
오른쪽 패널은 스크롤하여 저장·이어하기·다시 시작을 사용할 수 있다. 테스트의 SimulationRate 변경은 공통 갱신 입력의 속도만 바꾸며 실제 플레이 기본값은 1이다.

## 실행 결과

| 실행 | 결과 | 증거·해석 |
| --- | --- | --- |
| 첫 컴파일·씬 생성 | 현재 컴파일 오류 없음, SO와 씬 생성 성공 | [컴파일 상태](2026-10-26-compile-status-01.json), [생성 메뉴](2026-10-26-create-scene.json) |
| Edit 1차 | 116개: 114 통과·2 실패·0 건너뜀 | [JSON](2026-10-26-editmode-01.json), [XML](2026-10-26-editmode-01.xml). 수집자가 이미 출력 위치에 있어 같은 갱신에서 집고, 집기 전 예약 체크포인트가 관찰되지 않음 |
| Edit 2차 | 127개: 125 통과·2 실패·0 건너뜀 | [JSON](2026-10-26-editmode-02.json), [XML](2026-10-26-editmode-02.xml). JsonUtility 숫자 왕복의 약 2e-16초 차이를 정확한 double 동등 비교가 거부 |
| Edit 3차 | 132개: 132 통과·0 실패·0 건너뜀 | [JSON](2026-10-26-editmode-03.json), [XML](2026-10-26-editmode-03.xml) |
| Edit 최종 | **133개: 133 통과·0 실패·0 건너뜀** | [JSON](2026-10-26-editmode-04.json), [XML](2026-10-26-editmode-04.xml). 만재 후 실제 유료 건설·분할 입고·수집 재개 검사 1개를 추가한 최종 결과 |
| Play | 6개: 6 통과·0 실패·0 건너뜀 | [완료 JSON](2026-10-26-playmode-status-01.json), [XML](2026-10-26-playmode-01.xml). [시작 응답](2026-10-26-playmode-start-01.json)의 0개/running은 통과 결과가 아님 |
| 종료 상태 | Editor ready·Play stopped·runInBackground false, 현재 컴파일/콘솔 오류·경고 0 | [Editor](2026-10-26-editor-final.json), [콘솔 현재 상태](2026-10-26-console-final.json). [Open 메뉴](2026-10-26-open-scene.json)로 연구소 장면을 열어 둠 |
| 구조 | 누락 meta 0, Scripts 밖 C# 0, 기존 meta 변경 0 | [구조 확인](2026-10-26-structure-check.json). 기존 SO·스크립트 GUID 유지 |
| 문서·PLAN 상태 | 완료 후 active=0 조건만 오류. 후속 승인 대기이며 전체 검사 통과로 기록하지 않음 | [active 당시 검사](2026-10-26-docs-active.txt), [완료 후 검사](2026-10-26-docs-completed.txt) |

1차 실패는 원격 운반자를 준비한 별도 검사 상태로 수정했다. 실제 신규 시작은 계속 프로토타입 한 개체이며, 집기 전 상태를 만들기 위해 실제 게임에 대기시간을 추가하지 않았다.
2차 실패는 시간 단언에 1e-9초 오차를 허용하여 수정했다. ID·자원 소속·수량·Job/예약 관계 단언은 완화하지 않았다.
Play 통과 후 마지막으로 추가한 파일은 Edit Mode 검사뿐이다. 런타임·Play 검사·장면이 바뀌지 않았으므로 Play를 중복 실행하지 않았다.
초기 [콘솔 이력](2026-10-26-console-01.json)에는 다른 담당 파일이 아직 작성 중이던 시점의 미정의 타입 오류가 남는다. 이력 누적 건수와 최종 groundTruth의 현재 오류 0을 구분한다.
테스트 결과 XML은 Unity가 보고한 `AppData/LocalLow/DefaultCompany/UnityProject/TestResults.xml`에서 각 실행 직후 복사했다. 다음 실행이 덮어쓴 이전 XML을 새 결과로 사용하지 않았다.

## 규칙과 대표 검사

| 기준 | 대표 증거 코드/시나리오 |
| --- | --- |
| 실제 시작·유료 건설·입고 반복 | `LaboratoryViewTests.RealStartBuildsCollectsAndStoresWithAutomaticSaveAndMidConstructionContinue`: 실제 씬 Update로 초기 4개 사용·건설·유기물 10개 이상 입고 |
| 만재 대기·공간 확보·재개 | `InitialLaboratoryCycleTests`: 실제 시작부터 저장 20+완료품 5에서 대기, 비용 2로 추가 저장소 건설, 2+3 분할 입고 후 총 23·미입고 0·새 수집 재개 |
| 실제 수집 시간·회차 보존 | `WorkTests`: 실제 5초·무인 수집 없음·이동/편집 시간 제외·시설 이동/작업자 교체 시 진행 보존 |
| 우선순위·정상 운반 | `WorkTests`의 동률/개별 설정/도착 직후 재선택, `LogisticsTests`의 정상 운반 유지·건설 배송 후 설정 적용 |
| 대상·수량·예약 | `LogisticsTests`: 공급처 우선과 전체 경로 최소 조합 구분, 우회 경로 거리, 동률 생성 순서, 복수 개체 3+1 예약·부분 입고 |
| 변경 예외·재배치 | `LogisticsTests`: 집기 전 양쪽 예약 해제, 집은 뒤 보유 대기·분산 입고, 허용 제외 재고 2+3 합산 운반, 통로 차단/회복·목적지 이동/회전 |
| 저장과 실제 자원 보존 | `LaboratoryPersistenceTests`: 건설·수집·집기 전/후·보유 대기·부분 입고를 실제 파일로 왕복하고 새 객체에서 완료 |
| 잘못된 저장 거부 | `LaboratoryValidationTests`·`LaboratoryCompatibilityTests`: 미배치/잘못된 작업 위치/중복 작업, v1 진단 호환과 실제 슬롯에서의 거부. 기존 파일 실패 보존 검사도 함께 통과 |
| 화면/하네스 공통 처리 | `LaboratoryViewTests.SceneAndHarnessShareConstructionSettingsAndPlacementRules`: 같은 명령 허용·변화, 실제 씬·SO 참조와 OnGUI 렌더링 |

## 화면과 한계

[실제 Play Mode 화면](2026-10-26-laboratory-playmode.png)은 두 시설 건설 후 유기물 10개가 입고된 상태다. 캡처를 열어 격자·시설·개체·선택 패널·우선순위·총 재고 표시를 확인했다.
마우스 이벤트 주입이나 Android 터치 자동화 검사는 아니다. 실제 장면의 입력 핸들러가 사용하는 공통 조작 메서드와 렌더링을 검증했다.
임의(Temp) 화면은 Unity 기본 도형과 문자만 사용한다. 신규·교체 Sprite 요청은 없으므로 Require 폴더를 만들지 않았다. 정식 아트와 사용자 시각 검수를 완료한 것으로 기록하지 않는다.
Android 빌드·실기기·세로 대응·최종 성능·가공/배양/침공 전체 사이클은 이번 단계의 검증 대상이 아니다.

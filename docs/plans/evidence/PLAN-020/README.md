# PLAN-020 Editor 검증 증거

대상: main `1863d1e7814845a10c89009539b68897a09e5ae4` 제품 코드와 이 PLAN의 Editor 전용 진단 도구. 제품 코드·규칙·UI·밸런스 변경은 없다. 빌드·실기기 검증은 실행하지 않는다.

[최종 검증 대조](validation-summary.json)는 실제 표본·기능/회귀 결과·복원 해시·제품 변경 범위를 확인한 기록이다.

## 재현 환경과 안전한 실행

- 환경 원본: [environment-initial.json](environment-initial.json), [코드·패키지·데이터 식별값](measurement-version.json), 사용자 파일 목록/해시: [files-initial.json](files-initial.json).
- Unity 6000.3.27f1, Unity CLI 1.0.0-beta.15, Pipeline 0.9.1-exp.1. Intel Core 7 240H / RTX 5050 Laptop / RAM 32469 MB.
- 직접 Play는 Alternate Test Slot, Game View 1280×720 또는 720×1280, VSync 1, targetFrameRate -1, Run In Background ON이다. Editor의 실제 프레임 제한은 Player와 다르다.
- 원래 사용자 데이터는 `UnityProject/Temp/PLAN020-Backup/manifest.json`과 바이너리 백업으로 보존한다. 백업은 저장소에 커밋하지 않는다. 종료 시 파일 존재 여부·내용, Play 옵션·슬롯·Game View·열린 씬을 복원한다.
- 진단 도구는 `Assets/Scripts/Editor/Validation/Plan020Probe*.cs`에 있다. CLI 호출은 `unity command script eval --code '<C#>' --project-path UnityProject --caller plugin --skill unity-cli --json`이다. 클래스 전체 이름은 `MobileGame.Foundation.Editor.Plan020Probe`다.
- `Bottlenecks()`는 Edit에서 로직 사례를 수행한다. 직접 Laboratory 새 Play에서 `StartCycles()`, `StartUiRecovery()`, `StartMeasurements()`, `StartUiMeasurements()`, `StartSoak()`를 **순차적으로** 호출한다. 각 작업의 `Status`와 결과 JSON을 확인한 뒤 다음 작업을 시작한다. 측정 중 다른 진단·테스트를 동시에 실행하지 않는다.
- 기능 시나리오에서 `Controller.Advance(0.5)`를 반복해 게임 시간만 가속한다. 실제 UI의 Submit 이벤트와 동일 명령 경로를 사용하며, 물리적 마우스/터치 입력 검사는 기존 Play 회귀와 구분한다. 성능·20분 관찰은 정상 Update, 배율 1이다.
- 추가 진단 입력은 [부하 분리](reproduce/plan020-isolation.cs.txt), [호출 단위 CPU](reproduce/plan020-costs.cs.txt), [표식 조건 비교](reproduce/plan020-caption.cs.txt), [재진입](reproduce/plan020-reentry.ps1)에 보존한다. `.cs.txt`는 별도 에셋으로 임포트하지 않고 `Get-Content -Raw`로 읽어 `script eval --code`에 전달한다. 동기 CPU 진단에는 `--timeout 180` 이상을 사용하고, 종료 파일을 확인해 중복 실행을 피한다.

## 회귀와 운영 검증

- [Edit 회귀](edit-regression.json): 343개, 통과 342 / 실패 1 / 건너뜀 0. 실패는 `LocalizationCsvTests.ImportFailureRestoresExistingAssetsAndRepeatImportHasStableContent`의 Addressables 설정 hash 비교다. [당시 diff](addressables-hash-diff.txt)를 보존하며 제품 수정으로 숨기지 않는다.
- [Play 회귀](play-regression.json): 65개 모두 통과, 건너뜀 0. `Device...` 이름의 테스트도 Editor Input System의 합성 입력 검사이며 실기기 결과가 아니다.
- 회귀가 덮어쓴 PLAN-019 이미지 3개는 이 폴더의 `regression-popup-*`로 복사하고 이전 PLAN의 원본을 복원했다.
- [운영 병목](operating-bottlenecks.json): 재료 공급·저장 허용·운반 거리·생산 배정 4개 사례 통과. 저장/복원 후 공급 재개, 자원·예약 상태 유효성 검사 포함.
- [3판 직접 Play](paid-cycles.json): 제품 침공 설정, 정상 비용 지불·재료 생산·보급형/침공형 배양·방출·결과·새 판을 3회 연속 수행했다. 각 판 게임 시간 572초, 최종 점수 128. 초기 재료/결과를 강제로 주입하지 않았다. [실제 건설 화면](paid-cycle-current.png).
- [UI 병목 해결](storage-ui-recovery.json): 저장소 유기물 허용 버튼으로 입고 대기 5개 → 저장소 입고 5개, 미입고 0개. [대기 화면](storage-ui-blocked.png), [해결 화면](storage-ui-recovered.png). 이 사례만 준비된 병목 저장 상태를 사용했다.
- 도구 준비 중 산출물 fixture의 Progress/Outstanding 불일치와 첫 UI 입력의 포커스 준비 누락을 수정했다. 제품 결함이나 유효 측정 실패로 집계하지 않는다. 컴파일 오류도 진단 코드에서만 발생했고 수정 후 재컴파일 통과했다.

## 계측 해석

- [예비 측정](pilot.json) 후 [판정 기준](measurement-criteria.json)을 본 측정 전에 고정했다. 본 측정은 상태마다 30초 워밍업 + 60초 표본 × 3회다.
- `performance-levelN-runM.json`은 요약, 같은 이름 CSV는 원시 표본이다. 초기 0 / 중간 1 / 고부하 2의 시작 상태를 `fixture-N.json`에 보존한다.
- 성능 fixture의 침공 목표만 100000000으로 늘렸다. 활동 주기·성과·보고 상세 전환·보관 한도는 제품 설정을 유지한다. 전체 사이클 검증은 제품 목표 120을 별도로 사용한다.
- 중간 상태: 시설 6개와 초기 작업자 포함 4명. 고부하: 시설 8개와 작업자 24명, 외부 침공형 16명. 정상 생산이 계속되므로 실제 표본 종료 개체 수는 JSON으로 확인한다. 모든 시설은 현재 맵·정의에 배치 가능한 준비 상태이며 성능 fixture를 유료 건설 성공으로 계산하지 않는다.
- `Main Thread` ProfilerRecorder 값은 Editor와 대기 시간을 포함한다. `GC Allocated In Frame`도 게임에만 귀속되지 않는다. EditorApplication.update에서 서로 다른 렌더 프레임을 수집하며 매 프레임 디스크 기록은 하지 않는다. 시작/끝 메모리는 전체 Editor 프로세스 관련 값으로, 순수 게임 메모리와 같지 않다.
- ProfilerRecorder/관찰 콜백 비용이 포함된다. 관리 메모리의 GC 톱니 패턴과 최초 폰트/테이블 캐시는 누수와 구분한다. GPU와 Android FPS·발열·메모리 한계는 미측정이다.

## 분석

[측정 결과와 후속 후보](analysis/README.md)에 반복 표본, 부하 분리, UI 조건, 호출 단위 CPU와 계측 한계를 정리했다. 원시 데이터와 요약은 이 디렉터리의 같은 이름 CSV/JSON 쌍으로 보존한다.

## 추가 대조와 종료

- [동일 시작 상태·동일 시간 비교](operating-paired-windows.json): 재료 공급 허용 후 20초 생산 진행 0→3.24, 저장 허용 후 20초 입고 0→5, 동선 단축 후 2초 입고 0→5. 4초간 보급형을 생산에 배정하면 생산 진행 4/입고 0, 배정 해제하면 진행 0.8/입고 5였다. 양 분기 모두 저장 상태에서 복원했고 소유·예약 검사를 수행했다.
- [표식 재표시](status-reshow.json): 숨긴 동안 입고를 끝내고 다시 켜자 선택이 유지된 채 ‘입고 대기 · 진행 100%’에서 ‘채집 작업자 대기 · 진행 0%’로 갱신됐다. 같은 시설을 선택한 [설명 열림](caption-expanded.json)·[접힘](caption-collapsed.json)은 각각 10초 워밍업+20초 측정. 표시 표식 6/6, 전체 객체 500/500, p95 116.25/117.64ms, 프레임 GC 중앙값 5754801/5754703바이트. 다른 길이의 90초 고부하 표본과 직접 비교하지 않는다. 보고 열림·닫힘·OFF의 90초 표본 모두 종료 점수 1088/외부 16/보관 20개였고, 행 추가·제거 중 읽기 위치와 대체 앵커는 Play 회귀 `NewReportsPreserveReadingPositionAndPrunedAnchorShowsNearestRetainedReport`에서 확인했다.
- [20분 관찰](soak-final.json): 운영 1057.69초, Title 왕복·이어하기 2회, 팝업/언어/표시 변경. 원래 고부하의 응답성 실패와 [기존 작업 우선순위 조정](soak-worker-policy.json) 이후 구간을 구분한다. 구독은 이전 세션 정지·콜백 단일 전달 등의 회귀 결과로 확인했으며 모든 참조에 대한 힙 덤프를 수행하지 않았다.
- Reload ON(Bootstrap·Laboratory), Domain만 OFF(Title·Laboratory), 두 Reload OFF(Title·Laboratory) 각 2회, 총 6회 통과. `entry-*.json`은 씬/입력/공용 객체 수, `entry-detail-*.json`은 실제 옵션·비활성 포함 View/Popup 수다. Title은 연구소 View 0, Laboratory는 1, 닫힌 Popup View는 모두 0이었다.
- 전체 Edit 1개 실패는 원본 에셋을 다시 임포트한 뒤에도 [같은 해시 차이로 재현](edit-localization-repeat.json)됐다. 직접 Play/재진입과 [최종 콘솔](console-final.json)에서 경고·오류는 0개였다. NUnit 실패를 콘솔 오류 수로 대신 판정하지 않는다.
- [최종 Editor 환경](environment-final.json)은 시작과 동일하다. Play 종료, 원래 Laboratory 씬·1176×957 Game View·Domain Reload OFF/Scene Reload ON·기본 테스트 슬롯·백그라운드 옵션 원복. [사용자 파일 7개 SHA-256 일치](files-restored.json), 검사 중 생긴 임시 파일 3개 제거. 과거 PLAN 이미지와 Addressables 설정도 복원했다. 최종 제품 코드·에셋·프로젝트 설정 diff 없음. 계측 후 추가한 Editor 도구 변경은 동일 시간 분기 비교와 Alternate Slot 안전 검사뿐이다.

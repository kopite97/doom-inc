# PLAN-025 실행 증거

대상: [PLAN-025](../../PLAN-025-android-integration-and-prototype-completion.md). 2026-11-06 사용자가 APK·콘텐츠 빌드와 연결 기기 검증을 명시적으로 승인했고, 2026-11-07(KST) 검증·환경 복원과 완료 판정을 마쳤다. 원시 시각은 UTC이며 실행은 2026-11-06 UTC에 해당한다.

[기기 파일명 정정](device/evidence-file-notes.json)은 원래 캡처 이름과 실제 내용의 차이·후속 이름 변경을 연결한다. 파일명만으로 검증 결과를 판정하지 않는다.

## 기준 상태와 환경 복원

- [초기 기준](baseline-20261106-212340.json): HEAD `8db7bc0`, Unity 6000.3.27f1, active PLAN과 작업 트리·백업 준비 상태.
- [기기 기준](device-baseline.json): USB 디버깅 승인된 SM-G977N / Android 12. 대상 앱 `com.DefaultCompany.urp_2d`는 미설치였다. 기기 식별값은 증거에서 제외했다.
- [최초 컴파일 오류](editor-import-errors.txt): 새 체크아웃에 외부 연출 에셋이 없어 발생한 CS0246 9종. 제품 동작 검사 실패와 구분한다.
- [연출 에셋 복원](presentation-asset-restore.json): My Assets의 초기 Feel 표시는 6.3이었으나 실제 다운로드와 원본 readme는 5.9.1이었다. 기존 버전으로 복원하며 6.3 적용·검증은 수행하지 않았다. Text Animator는 3.20.1, DOTween Pro는 1.0.435다.
- 연구소 방출 음원의 참조 GUID `86aa66a501648034f8ec69ba5d404514`가 복원한 Feel Duck 음원과 일치한다. 실제 방출 효과음과 화면 효과는 두 번째 판에서 사용자 확인을 받았다.
- 기존 Editor 설정과 사용자 데이터 존재 여부를 로컬 `Temp/PLAN-025/` 아래에 보존했다. Unity가 반환한 persistent/temporary 경로와 Editor 슬롯을 확인했으며 실행 전 기존 게임 저장은 없었다. 원본 사용자 백업은 Git에 포함하지 않는다.

## Editor 회귀

- [현재 테스트 탐색](test-discovery.json): Edit Mode 358개, Play Mode 73개, 합계 431개.
- 첫 전체 Edit Mode: 353 통과 / 5 실패 / 0 건너뜀. [원본 JSON](edit-result.json), [원본 XML](edit-results.xml). 실패 결과는 수정 후 결과로 덮어쓰지 않는다.
- [줄바꿈 원인 대조](localization-newline-analysis.json): Git `core.autocrlf=true`가 Localization CSV의 인용된 다중 행 값까지 CRLF로 바꾸어 18개 키의 번역 검토 해시가 달라졌다. Git HEAD 원본과 LF 변환본은 바이트 단위로 같고 모든 검토 해시가 유효했다. 번역 내용·검토 해시를 재작성하지 않고 해당 CSV만 `.gitattributes`에서 LF로 고정한다.
- 줄바꿈 복원 후 [Localization 13/13](edit-localization-result.json)·[JSON 데이터 34/34](edit-json-data-result.json) 통과로 최초 Edit 실패 5개를 재검증했다. 기존 통과 결과와 재검증 범위를 구분하며 전체 Edit 재실행으로 표시하지 않는다.
- 첫 전체 Play Mode: 71/73 통과, 2개 실패. [원본 JSON](play-result.json)·[XML](play-results.xml). Cold Mulmaru 검사는 같은 CSV 원인이다. 겹친 작업자 터치 선택 검사는 line 64에서 Candidates 대신 Selection으로 실패했다.
- 터치 선택 [단독 재현 1/1](play-touch-repro-result.json)은 통과했다. CSV 복원 뒤 해당 실패의 실행 순서 영향을 확인한 [전체 Play 재실행](play-after-newline-result.json)은 **73/73 통과·실패/건너뜀 0**이며 [XML](play-after-newline-results.xml)을 보존했다. 최초 터치 실패의 원인은 확정하지 않는다. 최종 APK의 [추가 선택 검사](device/final-overlap-summary.json)에서 겹친 후보 1·14·16 중 14를 선택하고 우선순위를 바꿨으며, 저장에서 해당 작업자만 변경되고 나머지 23명의 설정은 유지됨을 확인했다. 최초 두 좌표 입력은 이동한 단일 작업자를 선택했으며, 실제 후보 목록 화면과 구분한다. Home 직후 너무 일찍 읽은 저장과 이후 완료된 저장도 구분했다.
- [실제 Editor 진입 요약](editor-entry-summary.json): Domain Reload ON/OFF에서 Bootstrap → Title, Title 직접 진입, Laboratory 직접 진입과 재진입을 확인했다. 관리자·카탈로그·EventSystem·PopupHost는 각각 1개였다. 첫 연구소 관찰의 포커스 정지는 Editor 활성화 후 해제됐고 프레임·게임 시간이 증가했다.
- [Editor 설정 복원](editor-settings-restored.json)·[생성 데이터 격리](editor-data-restored.json): 원래 Domain Reload OFF·Run In Background OFF·시작 씬 없음·슬롯 선택 키 없음으로 복원했다. 테스트 슬롯과 진단 출력은 로컬 백업으로 옮겼고 기존 제품 저장은 없었다. 동적 폰트 캐시와 DOTween 설정도 원래 추적 값과 일치한다.

## 모바일 계측과 상태 준비

- [기기 사전 조회](device-measurement-preflight.json)와 아래 지표별 수집 방법을 따른다. Android 예비 측정 뒤 사전 합격선을 고정했다. 최종·진단 APK 각각 9회의 유효 본 측정과 비교를 마쳤다.
- [진단 계측 코드](../../../../UnityProject/Assets/Scripts/Presentation/Diagnostics/Plan025PlayerProbe.cs)는 최종 Player에서 컴파일 제외한다. Android Development 빌드만 자동 생성하며, 앱 전용 `PLAN-025-Probe/request.json`의 실행 ID·예열/수집 시간을 읽는다. 인스턴스가 앱 수명 동안 카운터와 표본을 소유하고 종료/백그라운드 전환 시 수집을 끝내며 결과를 구분한다.
- [팝업 진단 보완](../../../../UnityProject/Assets/Scripts/Presentation/Diagnostics/Plan025PlayerProbe.Ui.cs)은 별도 `ui-request.json`이 있을 때만 실제 공용 PopupManager·프리팹·Android 입력으로 중첩/닫기 불가/확인 의도/소유 범위 종료 사례를 준비한다. 콜백은 진단 횟수만 기록하며 게임 상태를 바꾸지 않는다. 계측과 팝업 검사는 동시에 실행하지 않으며 최종 APK에는 포함하지 않는다.
- [Editor 계측 예비 실행](probe-editor-smoke-result.json): 1초 예열·2초 수집, 유효 GC 카운터·1253개 [원시 표본](probe-editor-smoke-samples.csv). Player 지원/성능 결과로 사용하지 않는다. `unity_update_interval_ms`는 Unity 갱신 간격이며 렌더 프레임 시간으로 사용하지 않는다.
- [준비 상태](fixture-preparation.json)와 [파일 해시](fixtures/manifest.json): 공통 codec으로 생성·재복원한 v6/DataHash 일치 저장이다. 초기 0시설/1작업자/외부0, 중간 6시설/4작업자/약탈형2, 고부하 8시설/24작업자/일반 침공형16+약탈형4다. 기존 `Plan020Probe.Prepare(level)`에서 시작해 공통 배치·방출 명령으로 약탈형을 추가했다. 수집 지속을 위한 목표 100000000은 검증 슬롯에만 있으며 제품 수치 2판과 구분한다.

## 빌드 준비

[빌드 도구](../../../../UnityProject/Assets/Scripts/Editor/Validation/LaboratoryPlayerBuild.cs)에 PLAN-025 최종/진단 메뉴를 추가했다. 이 메뉴는 사용자 빌드 승인 후에만 호출한다.

- 최종: Development·Script Debugging·Autoconnect Profiler·Deep Profiling OFF.
- 진단: Development ON, 나머지 세 옵션 OFF.
- 두 종류 모두 ARM64·IL2CPP를 확인하고 로컬 Addressables 콘텐츠를 명시적으로 생성한 뒤 APK를 빌드한다. 콘텐츠 실패는 APK 빌드 전에 중단한다.
- 산출물과 보고는 PLAN-025 아래 `{final|diagnostic}/{UTC 실행 ID}/`로 분리한다. 기존 PLAN-013/014 산출물을 덮어쓰지 않는다.
- 빌드 중 바꾼 Editor 옵션과 Addressables 자동 빌드 설정은 `finally`에서 복원한다.

- [Android 사전 검사](android-build-preflight.json): 제품 3개 씬·JSON 연결·번역 테이블·ARM64/IL2CPP·기본 검증 서명을 확인했다.
- [기준 소스](build-source-manifest.json)에 931개 소스/콘텐츠 파일의 해시를 기록했다. [최종 빌드](final/20261106-130332-701/android-build-20261106-130332-701.json)는 오류 0·경고 5, [진단 빌드](diagnostic/20261106-131428-570/android-build-20261106-131428-570.json)는 오류 0·경고 6으로 성공했다.
- APK 실제 크기는 최종 52,072,578 bytes, 진단 63,793,975 bytes다. BuildReport의 `bytes`는 전체 빌드 산출물 크기이며 APK 파일 크기와 구분한다. [최종 APK 검사](final-apk-inspection.json)·[진단 APK 검사](diagnostic-apk-inspection.json)에 SHA-256과 ARM64·계측 타입 제외/포함을 기록했다. 최종에는 ko/en 로컬 Addressables 번들·카탈로그가 포함된다.
- 공통 경고는 OnMouse 계열 코드, Feel의 선택적 PostProcessing/Cinemachine Resources의 미설치 의존 스크립트 3건, Localization Android App Info 메타데이터다. 진단에는 Text Animator UIToolkit의 미사용 필드 경고가 추가됐다. 경고를 숨기기 위한 패키지/제품 설정 변경은 하지 않았으며 실제 연출·번역 동작과 별도로 기록한다.
- [최종 뒤 소스 대조](post-final-source-comparison.json)·[진단 뒤 소스 대조](post-diagnostic-source-comparison.json): C#와 제품 JSON은 변하지 않았다. Balance README 해시 정정 외에 빌드가 URP 프리필터·런타임 목록/필드와 PlayerSettings의 preloadedAssets·Android batching 항목을 직렬화했다. 생성 변경을 정리하고 [최종 설정 대조](build-settings-restored.json)에서 빌드 전 값 복원을 확인했다.
- [설치본 대조](final-installed-identity.json): 최종 APK와 기기 base.apk의 SHA-256이 같다. [네트워크](offline-first-launch-network.json)를 끈 상태에서 새 설치의 타이틀·연구소·한국어 글리프·건설 메뉴가 표시됐다. 첫 실행 전 앱 데이터 경로는 존재하지 않았다.
- 팝업 경계 계측을 더한 [두 번째 진단 빌드](diagnostic/20261106-134953-961/android-build-20261106-134953-961.json)는 오류 0·경고 1로 성공했다. SHA-256 `4f78dd667f06f628733156b1f02eccd5006ea01bef9d1963b4cf3d9c14289614`이며 첫 진단 APK를 대체한다. CLI 호출의 30초 timeout과 이후 실제 빌드 성공을 구분한다. [소스 대조](diagnostic-ui-source-comparison.json)에서 제품 C#/JSON은 같고 진단 계측만 확장됐다. 증분 빌드의 경고 감소를 기존 경고 해결로 해석하지 않는다. 최초 수치 기준은 변경하지 않는다.
- [새 진단 APK 검사](diagnostic-ui-apk-inspection.json)의 파일 크기는 91,774,407 bytes다. [ZIP 목록의 콘텐츠 차이](diagnostic-apk-size-differences.json)는 작지만 [목록 밖 ZIP 구간](diagnostic-apk-zip-layout.json) 27,966,514 bytes가 크기 증가 대부분을 차지한다. 증분 패키징 잔여 공간이라는 원인은 추정이며 런타임 메모리 증가로 단정하지 않는다. 실제 설치 파일은 이 APK 해시로 대조한다.
- 사용자가 실제 두 손가락 확대/축소·한 손가락만 뗀 이동·가로/세로 회전을 모두 정상이라고 확인했다. [가로 화면](device/physical-touch-confirmed.png)을 별도로 보존했다. ADB의 강제 회전에서 잠시 관찰된 좁은 안전 영역은 강제 설정 해제 후 물리 회전에서 재현되지 않았으며 제품 결함으로 확정하지 않았다. 이후 단일 터치 자동 입력과 물리 다중 터치 확인을 구분한다.

## 모바일 지표별 수집 방법

기기 예비 실행으로 지원하는 수집 경로를 확인했다. 실제 본 측정 결과와 유효성은 아래 실행별 증거를 따른다.

### 확인된 환경

- SM-G977N / Android 12, 화면 1080×2280(물리 1440×3040), USB 연결.
- [기기 사전 조회](device-measurement-preflight.json)에 Perfetto 데이터 소스와 배터리·발열 상태를 기록했다.
- 분석 도구: Perfetto v59.1, commit `1e9c61e9844affd7dccd3d42d9ea9f3a3d10cf6d`, RPC 14. 공식 `get.perfetto.dev/trace_processor` 배포 스크립트로 Windows amd64 도구를 로컬 준비했다. 스크립트 SHA-256은 `f4acbe5388d8e5d9f3aac9c509026902a1cf6e50ad0b888bdba9acc12b83d886`이다.

### 지표별 예비 검증

| 지표 | 우선 경로 | 유효성 확인과 한계 |
| --- | --- | --- |
| 실제 화면 프레임 | 최종 APK의 Perfetto/SurfaceFlinger 표본 | 대상 앱의 실제 Surface와 표시 시각인지 확인한다. 데이터 소스 존재만으로 앱 표본 지원을 판정하지 않는다. |
| 입력 반영 지연 | 입력 시각과 화면 기록의 대응 | 수집 시계·화면 기록 프레임 간격·입력 주입과 손가락 입력의 차이를 명시한다. 원격 ADB 왕복 시간을 앱 지연으로 쓰지 않는다. |
| 메모리 | `dumpsys meminfo`의 대상 프로세스 PSS/RSS | 단위와 프로세스 식별을 보존한다. Unity managed heap과 OS 프로세스 메모리를 같은 값으로 취급하지 않는다. |
| GC | 진단 APK에서 유효한 Unity 카운터 또는 검증 가능한 추적 | 지원 여부를 예비 실행에서 확인한다. Android ART GC가 Unity managed GC를 대표한다고 가정하지 않는다. |
| 발열 | `dumpsys thermalservice`, 배터리 온도 | 센서 이름·상태와 값을 함께 보존한다. 기기 자체 열 상태와 게임 실행 구간을 연결한다. |

[Perfetto FrameTimeline 공식 설명](https://perfetto.dev/docs/data-sources/frametimeline)은 Android 버전과 Surface 지원 범위를 구분한다. Unity의 실제 렌더 Surface 표본을 확보하지 못하면 다른 지원 경로를 확인하고 미지원 항목을 미측정으로 남긴다. [공식 trace processor](https://perfetto.dev/docs/analysis/trace-processor)의 로컬 분석을 사용하며 기기 추적 원본을 외부 서비스로 업로드하지 않는다.

실제 예비 실행 뒤 본 측정 전에 상태 구성·30초 예열·60초×3회·판정 수치와 이유를 확정했다. 기준을 정한 시각과 APK/상태 해시를 함께 보존한다.

- [실제 Surface 예비 측정](device/surface-preflight/summary.json): 293개 표시 시각·9.78초, 중앙값 33.49 ms, p95 33.69 ms. 앱 BLAST Surface의 유효한 실제 표시 시각을 수집했다. FrameTimeline은 이 Android 12 앱 Surface 표본을 제공하지 않아 사용하지 않는다.
- [수집 스크립트](collect-surface.py)는 `--adb`, `--out`, `--warmup`, `--seconds`를 인수로 받는다. 원시 조회와 중복 제거한 표시 시각을 함께 보존하며 앱 프로세스 동일성을 확인한다. [Android 원본](https://android.googlesource.com/platform/frameworks/native/+/cb31cabec0411d3647be742d9be0605a4dcf6622/services/surfaceflinger/FrameTracker.cpp)의 두 번째 열 `actualPresentTime` 사이 간격이며 CPU/GPU 실행 시간으로 해석하지 않는다.
- 본 측정 전 [수치 기준](mobile-acceptance-criteria.json)을 고정했다. 30 Hz 정상 운영에서 프레임 중앙값 ≤35 ms·p95 ≤50 ms·최댓값 ≤250 ms, 입력 p95 ≤150 ms·최댓값 ≤250 ms, 같은 상태 복귀 시 PSS 증가 ≤max(32 MiB, 10%)를 적용한다. 기준을 충족해도 미측정 필수 항목을 통과로 바꾸지 않는다.

### 최종 APK의 본 측정

[실행 조건과 APK 연결](performance-run-context.json), [전체 집계](performance-summary.json), [분석 스크립트](analyze-performance.py)를 따른다. 세로·한국어·큰 글씨·시설 상태 ON·선택 없는 전체 지도에서 상태별 30초 예열 뒤 60초를 3회 수집했다. 각 상태의 시작 저장은 고정하고 3회 동안 정상 진행하므로 배양으로 늘어난 수량은 회차별 저장에 별도로 기록한다.

| 시작 상태 | 3회 중 가장 큰 p95 | 가장 긴 프레임 | 프레임 판정 |
| --- | --- | --- | --- |
| 초기 0시설·1작업자 | 33.674 ms | 50.413 ms | 3/3 통과 |
| 중간 6시설·4작업자·약탈형 2 | 33.680 ms | 50.195 ms | 3/3 통과 |
| 고부하 8시설·24작업자·외부 20 | 33.770 ms | 66.833 ms | 3/3 통과 |

모든 정상 운영 표본의 중앙값은 약 33.47 ms다. 메모리 절대량과 관찰 발열도 사전 기준 안에 있다. 장시간 운영과 진단 APK의 GC·메모리 비교 결과는 아래에 기록했다. 화면이 꺼져 앱 Surface를 찾지 못한 최초 초기 상태 예비 시도는 본 측정에서 제외하고 실패 이유를 보존했다.

### 진단 APK의 비교 측정

[설치 식별](diagnostic-installed-identity.json)과 [Android 예비 결과](device/probe-android-smoke/result.json)에서 새 진단 APK 해시 및 GC 카운터의 실제 지원을 확인했다. 1초 예열·2초 수집의 59개 표본은 지원 확인용이며 본 측정에 합치지 않는다. 초기·중간·고부하는 최종과 같은 화면/시작 저장 조건을 사용한다.

중간 상태 첫 시도는 앱 일시정지로 진단 수집이 중단됐고, 같은 구간 ADB 서버 재시작과 5.14초 조회 공백이 관찰됐다. 일시정지 원인은 확정하지 않는다. [제외 사유](performance-exclusions.json)와 원시 자료를 보존하고 기준을 바꾸지 않은 4회차로 보충했다. 유효 비교는 중간 2·3·4회차이며 실제 배양으로 달라진 개체 수는 각 결과에서 확인한다.

[비교 집계](performance-comparison.json)와 [18회 전체 결과](performance-summary.json)에서 각 상태·각 APK 3회씩 모두 유효하며 프레임·절대 PSS·발열 기준을 통과했다. 진단 GC는 16,119개 표본이 모두 유효하고 일시정지 없이 끝났다. 아래 값은 상태별 3회 중 가장 큰 값이다.

| 상태 | 진단 프레임 p95 / 최대 | GC 할당 p95 / 최대 (bytes/frame) | 최종 / 진단 종료 PSS 중앙값 (KiB) |
| --- | --- | --- | --- |
| 초기 | 33.688 / 50.332 ms | 119,790 / 159,509 | 408,441 / 470,832 |
| 중간 | 33.710 / 66.956 ms | 243,530 / 518,243 | 408,092 / 456,434 |
| 고부하 | 33.689 / 83.559 ms | 751,360 / 2,211,984 | 327,690 / 470,675 |

진단 PSS 차이는 Development·계측·프로세스별 회수·실제 개체 수 차이를 포함하므로 계측 코드만의 원인별 오버헤드로 해석하지 않는다. GC 카운터는 할당량이며 수집 정지 시간은 아니다. 최종 APK의 Unity GC/객체 수, 물리 디지타이저·발광 지연, 전환 순간 메모리 최고치는 미측정으로 남긴다. 유효 표시 프레임에 250 ms 초과 정지는 없었다. 고부하 할당량은 후속 최적화 후보이며 이 범위에서 최적화 코드는 변경하지 않았다.

### 고부하 연속 운영

[집계](heavy-operation-summary.json), [분석 스크립트](analyze-heavy-operation.py), [525회 입력 기록](device/heavy-operation/actions.jsonl), [실행 정보](device/heavy-operation/manifest.json)를 보존했다. 동일 최종 APK·PID 5998로 확인된 고부하 로드부터 1482.899초(24분 43초)를 관찰했고, 그중 938.719초(15분 39초) 동안 21회 UI 순환을 실제 ADB 터치/스와이프/Android Back으로 수행했다. 지도 확대·이동·맞춤, 상태 ON/OFF, 보고·생명체·건설 메뉴 스크롤, 저장, 설정 팝업·ko/en 전환 후 전체 지도로 복귀했다. 물리 회전은 사용자 확인과 별도로 연결한다.

- 모든 메뉴·두 언어를 사용한 첫 순환 종료를 메모리 기준으로 삼았다. 같은 지도 복귀 시 PSS는 361,183 → 357,380 KiB로 3,803 KiB 감소했고, 사전 허용 증가량 36,118.3 KiB를 충족했다. Android View/Activity 수는 OS 값이며 Unity 객체 수나 GC로 해석하지 않는다.
- 모든 관찰 발열 상태는 0이며 프로세스가 바뀌지 않았다. 조작 구간의 게임 시간 증가 938.523초는 실제 운영 938.719초와 일치하는 정상 배율이었다.
- [운영 구간 Surface 집계](device/heavy-operation-surface/summary.json): 26,797개 실제 표시 시각·899.899초, 중앙값 33.471 ms·p95 33.719 ms·최댓값 100.186 ms. 언어·메뉴 전환과 저장을 포함한 구간이며 정상 고정 화면 60초×3 측정과 분리한다.
- 메모리는 각 순환 뒤 같은 화면에서 표본을 얻었다. 전환 도중의 순간 메모리 최대값을 연속 측정한 결과로 주장하지 않는다. 시작의 시설 8·작업자 24·외부 20에서 정상 배양으로 늘어난 실제 수량은 각 저장에 남겼다.

## 실기기 운영 검증

- 제품 수치·정상 배율의 가로 첫 판: [결과 화면](device/cycle1-results.png), [결과 저장](device/cycle1-results-session.json). 초기 시설 0·프로토타입 1에서 실제 UI로 5시설·보급형 2·일반 침공형 6·약탈형 2를 만들었다. 방출 당시 일반 침공형 6만 완성되어 있었으므로 해당 방출은 혼합 방출 증거가 아니다. 보고를 열지 않고 점수 120·외부 활동 36회·진행 시간 1221.2544초로 멸망/결과에 도달했다. 혼합 방출은 두 번째 판에서 확인한다.
- 저장 공간 병목은 첫 저장소의 유기물 포화로 가공 결과 입고가 멈춘 상태를 확인한 뒤, 두 번째 저장소를 건설하고 유기물 허용을 해제해 해결했다. [관찰 수치](device/bottleneck-observations.json)와 저장/화면 원본을 보존했다. 비교 구간은 각각 실제 게임 시간이며 정확히 같은 길이라고 표현하지 않는다.
- [결과 프로세스 종료/재실행](device/cycle1-result-relaunch.json): 종료 전 PID 30682, force-stop 뒤 프로세스 부재 확인. [종료 저장](device/cycle1-stopped-session.json)과 [복원 저장](device/cycle1-restored-result-session.json)의 점수·시간·회차·자원·예약·개체가 보존됐다. 작업자 moveProgress의 문자열 표현만 0.24960479140281677 → 0.2496047914028168로 재직렬화됐다(차이 3e-17). 결과 화면에서 새 판 시작 후 [세로 초기 상태](device/cycle2-start.png)는 시설 0·프로토타입 1·유기물 4·점수 0이다.
- 세로 두 번째 판은 일반 침공형 1·약탈형 1을 함께 방출했고, 사용자가 방출 버튼을 직접 누른 뒤 효과음 1회·화면 효과가 정상이라고 확인했다. [보고 목록](device/cycle2-report-list.png)을 열어 일반/약탈형 성과를 확인했으며 [결과](device/cycle2-results.png)는 120점·40회·700.2초·외부 2개체다. 결과 촬영 뒤 저장을 추출하기 전 새 판/타이틀로 바뀌었고 그 사이 입력은 관찰하지 못했다. 따라서 두 번째 결과 저장이나 자동 재시작 조작을 주장하지 않는다. 추출한 [새 판 저장](device/post-cycle2-new-session.json)은 시설 0·프로토타입 1·유기물 4·점수/회차/보급/예약 0이다.
- Android Back은 침공 패널에서 선택 패널로 한 단계 돌아갔고, [확인 팝업](device/cycle2-reload-confirm.png)에서는 [팝업만 취소하고 메뉴 유지](device/cycle2-confirm-back.png)를 확인했다. [세로 큰 글씨/영어](device/cycle2-large-en.png) 전환과 한국어 표시를 확인했다.
- [생산 중 프로세스 종료](device/production-force-stop.json) 전 마지막 성공 저장 SHA-256은 `69bb6482377f65c081fc945a23b1b61807bfb2ba8293dc0ccbf99c510354e051`이다. 종료 후 프로세스 부재와 타이틀까지 저장 바이트 보존을 확인했다. [복원 대조](device/production-restore-comparison.json)에서 실제 전경 2.375944초 동안 가공/배양 진행은 각각 0.475189 증가해 미배정 20% 속도와 일치한다. 재고·예약·배치·선택/진행 중 배양 대상은 보존됐다.
- [보급 대기 종료/복원](device/supplies-relaunch.json)과 [상태 분석](device/lifecycle-summary.json): pendingRound 6·delivered 1을 그대로 복원했다. 실제 UI로 저장소의 유기물 허용을 켜자 수령이 2로 한 번 증가했다. 이후 round 18의 새 보급 대기는 기존 round 6의 중복 지급과 구분한다.
- [홈/팝업](device/home-popup-lifecycle.json)과 [화면 잠금](device/lock-lifecycle.json)은 별도 실행했다. 각각 배경 12초·8초의 저장 바이트가 같고 같은 프로세스로 돌아와 팝업/입력/생산이 복구됐다. 개인 알림이 있는 잠금 화면은 로컬 비추적 경로에만 두었다.
- 병목 전후는 [분석 스크립트](analyze-lifecycle.py)로 같은 60 게임 초당 배양 작업량으로 정규화했다. 개선 전 65.286초 구간은 0, 개선 후 65.307초 구간은 12 작업 초/60 게임 초였다. 회차 수를 보간하거나 두 관찰 길이가 정확히 같다고 주장하지 않는다.
- 경미한 표시 문제: [세로·영어·큰 글씨의 시설 이동 상태](device/cycle2-storage-new-position.png)에서 긴 배치 안내가 잘린다. 좌표·배치 가능 여부·확인/회전/취소 버튼은 읽고 조작할 수 있었고 진행은 막히지 않았다. `LaboratoryView.Gui.cs`의 PlacementStatus 고정 높이 42와 줄바꿈/Truncate 조합을 후속 수정 후보로 기록하며 제품 코드는 변경하지 않았다.
- [운반 중 강제 종료](device/transport-stop/result.json)는 이전 정상 고부하 자동 저장을 재준비한 별도 경계 검사다. 예약 3개 중 1개가 pickedUp인 마지막 성공 저장으로 종료했고 타이틀까지 바이트가 같았다. [복원 비교](device/transport-stop/comparison.json)에서 자원 Thing·예약·점수·활동 회차는 같고, 외부 진행은 복귀 후 게임 시간 0.853439초만 증가했다.
- 저장 호환: [지원 v5](device/save-compatibility/supported-v5/result.json)는 연구소에 진입하고 [다음 자동 저장](device/save-compatibility/supported-v5/migrated-session.json)에서 v6로 전환됐다. [손상](device/save-compatibility/corrupt/result.json)·[미지원 버전](device/save-compatibility/unsupported/result.json)·[DataHash 불일치](device/save-compatibility/hash-mismatch/result.json)는 오류 안내를 표시하며 입력 파일을 바이트 그대로 보존했다. 손상 사례에서 실제 재시도와 [타이틀 복귀](device/corrupt-returned-title.png)를 확인했다. 해시 허용 규칙을 변경하지 않았다.
- [로딩 중 실제 Back/Home](device/loading-boundary/summary.json): 두 사례 모두 90% 로딩 화면을 촬영한 직후 Android 키를 입력했다. Back은 전환을 중복 실행하거나 앱을 종료하지 않았고, Home의 7초 배경 구간은 저장 바이트가 같았다. 복귀 후 연구소와 확대 입력 정상 동작을 화면·시각별 [입력 기록](device/loading-boundary/actions.json)으로 확인했다.

### 실제 Android 팝업 수명 경계

[종합 결과](device/popup-boundaries/summary.json)는 진단 APK에서 명시적으로 준비한 실제 제품 PopupManager·프리팹과 ADB 터치/Android 키를 사용한다. 확인 콜백은 진단 횟수이며 게임 규칙이나 세션을 바꾸지 않는다.

- [중첩](device/popup-boundaries/stack-1/ui-events.json): 수신된 Back 각각에 자식만 닫힘(count 2→1), 다음에 부모 닫힘(1→0)을 확인했다. 최초 키 주입 한 번은 키/상태 변화가 관찰되지 않았고 원인을 확정하지 않았다. 이후 입력의 단일 처리와 구분해 기록한다.
- [닫기 금지](device/popup-boundaries/nondismiss-1/ui-events.json): 실제 Back을 기록했지만 자식·부모는 Open/count 2를 유지했다. 확인 터치 후 자식만 Confirmed/count 1, 다음 Back 후 0이 됐다.
- [확인 직후 Home/복귀](device/popup-boundaries/intent-2/assessment.json): Closing·콜백 0에서 실제 application-pause, 복귀 후 Confirmed·1회다. Home/복귀를 반복해도 1회이며 count 0·blocked=false다. 첫 시도는 Home 전에 확인이 완료되어 이 경계의 증거에서 제외했다.
- [소유 범위 종료](device/popup-boundaries/owner-end-1/assessment.json): Closing·콜백 0에서 실제 Home으로 나갔고, 진단 소유 범위를 종료하자 OwnerEnded로 마쳤다. 두 번 복귀 후에도 0회이며 count 0·blocked=false다. [복귀 뒤 확대 터치](device/popup-owner-ended-input.png)로 입력 복구도 확인했다.
- [진단 Player 경고/오류 조회](device/diagnostic-player-warnings-final.log)는 해당 PID의 Unity 경고 이상·AndroidRuntime 오류·libc 치명 오류를 조회했으며 출력이 없었다. 전체 OS의 오류 부재를 뜻하지 않는다.

## 입력 지연 — 최종 APK

[분석 스크립트](analyze-input-final.py), [10회 원시 대응](device/input-final/latency.csv), [집계](device/input-final/latency-summary.json), [화면 원본](device/input-final/screen.mp4)을 보존했다. Android ViewRootImpl의 ACTION_UP 로그부터 최초 확대/축소 반영 녹화 프레임까지 측정했다. 두 자료는 휴대폰 시계를 사용하며 호스트 ADB 왕복 시간을 제외했다.

- 상한 표본 중앙값 111.652 ms, p95/최댓값 123.907 ms. 10개 표본의 nearest-rank p95는 최댓값과 같다.
- 화면 표본 해상도 최대 34.56 ms, 시각 표시 반올림 약 1 ms, 앞/중간/뒤 시각 앵커 차이 0.234 ms 미만. 사전 기준 p95 150 ms·최댓값 250 ms를 이 측정 경계에서 충족한다.
- ADB 입력을 앱이 전달받은 뒤 녹화 합성 화면까지의 지연이며 실제 손가락 접촉·디지타이저·패널 발광까지의 지연은 측정하지 않았다. 화면 녹화 오버헤드를 포함하며 정상 운영 프레임 측정과 분리한다. 물리 다중 터치는 사용자 확인으로 별도 기록한다.
- 시계 대응은 [Android screenrecord Overlay 원본](https://android.googlesource.com/platform/frameworks/av/+/b46eb8d9f8bff2e6ddec1902acc9d2ad65147d68/cmds/screenrecord/Overlay.cpp)의 프레임 시각/벽시계 표시 방식과 실제 녹화 앵커를 대조했다.
- 중간 부하 세로 화면의 추가 10회: [집계](device/input-final-middle/latency-summary.json), [원시 대응](device/input-final-middle/latency.csv), [영상](device/input-final-middle/screen.mp4). 중앙값 상한 91.989 ms·p95/최댓값 103.422 ms, 프레임 해상도 34.10 ms로 같은 기준을 충족했다. [세로 분석](analyze-input-portrait.py)은 지도 상단 격자 경계의 첫 변화를 검출하며 시계 앵커와 전체 기하 표본을 보존한다.
- 고부하 세로 화면의 추가 10회: [집계](device/input-final-heavy/latency-summary.json), [원시 대응](device/input-final-heavy/latency.csv), [영상](device/input-final-heavy/screen.mp4). 중앙값 상한 102.611 ms·p95/최댓값 125.233 ms, 프레임 해상도 34.60 ms로 같은 기준을 충족했다. 이후 사용자가 고부하 상태를 실제 가로로 회전하고 화면이 정상이라고 확인했다.
- 고정 초기 상태의 세로 추가 10회도 [집계](device/input-final-initial/latency-summary.json)·[원시 대응](device/input-final-initial/latency.csv)·[영상](device/input-final-initial/screen.mp4)을 보존했다. 중앙값 상한 113.367 ms·p95/최댓값 125.917 ms·프레임 해상도 34.38 ms로 같은 기준을 충족했다.

## 재현과 복원 기록

- 부하 재현은 `fixtures/manifest.json`의 파일을 앱이 종료된 상태에서 해당 앱의 `files/Gameplay/session.json`에 전달하고, 타이틀의 이어하기로 공통 codec을 통과한다. 각 `*-load.json`에 전달 전후 해시·PID·시각을 보존했다. 이 준비를 실제 건설 조작으로 집계하지 않는다.
- 정상 프레임 수집은 `python collect-surface.py --adb <현재 SDK adb 경로> --out <새 실행 경로> --warmup 30 --seconds 60`이다. 진단은 앱의 `files/PLAN-025-Probe/request.json`에 고유 id와 warmupSeconds=30, sampleSeconds=60을 전달하고 같은 기간 OS 수집을 병행한다. 기존 실행 경로를 덮어쓰지 않는다.
- 집계는 `python analyze-performance.py`, `python analyze-heavy-operation.py`, `python analyze-lifecycle.py`로 원시 파일에서 재생성한다. 입력 영상 분석에는 OpenCV 환경과 보존된 휴대폰 시계 앵커가 필요하며 `analyze-input-portrait.py final-initial`처럼 대상 이름을 지정한다.
- [소스 최종 대조](final-source-comparison.json): 기준 931개 파일 중 제품 코드·JSON·에셋이 최종 빌드 전과 같다. Balance README 정정과 최종 APK에서 제외되는 진단 코드만 다르다. 진단 UI 코드는 두 번째 진단 빌드 전에 추가했고, 이후 주석 문구만 정리했다.
- 문서 완료 후 [최종 정적 대조](completion-audit.json)에서도 931개 기준 파일을 확인했고 제품 코드·JSON·에셋의 일치를 유지했다. Localization CSV 9개는 Git HEAD와 바이트가 같다. [마지막 개인정보/비밀 값 검사](privacy-scan-final.json)는 명시한 패턴 485개 텍스트 파일에서 일치가 없었으며 전수 보안 감사로 해석하지 않는다. 문서 상태·링크와 diff 검사는 [최종 문서 검증](completion-document-validation.json)을 따른다.
- [빌드 설정 복원](build-settings-restored.json)과 [렌더 에셋 재임포트](render-settings-reimport.json)를 확인했다. [Editor 종료 직전](editor-before-close.json)은 Play/컴파일 OFF·씬 dirty=false·원래 Domain Reload OFF·Run In Background OFF·빌드 디버그 옵션 OFF였다. 최초에 Editor가 없었으므로 정상 종료했고, 생성 데이터/Addressables 캐시는 비추적 백업으로 격리했다. 기기 복원까지 [복원 기록](environment-restoration.json)에서 완료로 판정했다.

- 최종 APK 재설치 후 [보존한 첫 판 결과 다시 불러오기](device/save-compatibility/result-replay/result.json) → 실제 [새 판 시작 터치](device/final-result-replay-restart.png) → [초기 저장](device/final-fresh-session.json)을 추가 확인했다. 시설 0·프로토타입 1·유기물 4·점수/회차/보급/예약 0·제품 목표 120이다. 이는 재시작 경계 재검사이며 세 번째 전체 플레이나 두 번째 판의 관찰하지 못한 전환을 재현한 것으로 집계하지 않는다.
- [기기 복원](device-restoration.json): Wi-Fi/모바일 데이터 ON, 자동 회전 OFF·user_rotation 0, 화면 유지 OFF, 해상도 기존 값, 터치 표시 OFF로 복원했다. 진단 요청/결과·녹화·남은 임시 저장은 로컬 백업과 해시 확인 후 기기에서 정리했다. [최종 설치 해시](final-reinstalled-identity.json)가 원래 최종 APK와 같으며 [타이틀](device/final-restored-title.png)에 두었다. 앱은 처음에 없었으므로 최종 APK·새 판·한국어/보통 글씨/상태 ON은 승인된 검증 후 남기는 차이다. 기존 사용자 저장을 지운 것은 아니다.

## D-070 완료 판정

2026-11-07 KST, SM-G977N/Android 12·기존 임시 표현·명시한 APK/소스에 한정해 네 기준을 충족했다. 필수 기능·플랫폼 검증의 실패나 진행 차단·저장 손상·심각한 성능 문제는 남지 않았다. 미측정 세부 지표와 제한은 아래에 그대로 남긴다.

| 기준 | 실제 근거 | 판정과 한계 |
| --- | --- | --- |
| 전체 흐름 | 제품 목표 120·정상 배율의 가로/세로 두 판 결과, 첫 판의 보고 미열람 성과, 두 번째 판의 일반/약탈형 혼합 방출·보고, 결과 복원·새 판 저장 | 통과. 두 번째 결과 저장과 결과→새 판 사이 입력은 미확보다. 결과 화면과 다음 초기 저장을 각각 보존했고, 첫 판의 직접 재시작 및 최종 APK의 별도 결과→실제 재시작 경계로 보완했다. 관찰하지 못한 입력을 실행한 것으로 주장하지 않는다. |
| 병목 개선 | [실제 UI 조정과 수치](device/lifecycle-summary.json): 저장소 추가·허용 자원 변경 후 배양 작업량 0→12 작업 초/60 게임 초 | 통과. 두 관찰 구간을 같은 게임 시간 단위로 정규화했으며 관찰 길이가 정확히 같다고 주장하지 않는다. 다른 병목은 기존 PLAN-020 결과와 연결한다. |
| 저장·복원 | 생산·보급·운반·결과의 실제 force-stop/프로세스 부재·마지막 성공 저장 대조, 홈/잠금 정지, v5 이관·오류 파일 보존, 재시작 초기화 | 통과. 종료 중 진행과 복귀 후 전경 진행을 구분했다. 결과 재직렬화의 부동소수점 문자열 차이도 보존했다. |
| 모바일 검증 | 새 설치 오프라인, 가로/세로·ko/en·터치·물리 핀치/회전, Back/팝업/로딩 수명, 최종 9회·진단 9회, 상태별 입력, 24분 43초 관찰 중 15분 39초 운영 | 통과. 사전 기준을 변경하지 않았고 중단된 진단 1회는 원시 자료와 제외 사유를 보존한 뒤 보충했다. |
| 통합 회귀·최종 복원 | 전체 Edit 최초 353/358 후 관련 47/47로 실패 5개 해소, 전체 Play 73/73, Domain Reload 양쪽, 소스/설정/APK 해시와 환경 복원 | 완료. 전체 Edit를 다시 실행한 것으로 집계하지 않는다. 최초 터치 검사 실패 원인은 미확정이나 단독/전체 재실행과 실기기 대상 확인에서 재현되지 않았다. |

### 남은 한계와 후속 후보

- 세로·영어·큰 글씨의 긴 시설 이동 안내 일부 잘림은 진행을 막지 않는 표시 결함이다. 적응형 높이/줄 배치를 후속 수정 후보로 남긴다.
- 진단 APK 고부하 GC 할당량은 최적화 후보다. 실제 표시 프레임·장시간 메모리 기준은 통과했으며 이번 범위에서 제품 최적화를 실행하지 않았다.
- 최종 APK의 Unity GC·Unity 객체 수와 전환 순간 메모리 최대값, 물리 접촉→패널 발광 지연은 미측정이다. GC는 본 측정 전 정한 진단 전용 유효 할당량 조건으로 확인했고, 최종 값이나 0으로 대체하지 않는다. 프레임은 실제 표시 간격, 입력은 앱 수신→녹화 화면, 잔존 추세는 동일 화면의 PSS와 프로세스 유지로 판정했다.
- 한 기종과 현재 임시 에셋의 결과다. 정식 아트 성능·다른 기종·iOS·스토어 배포·최종 밸런싱은 포함하지 않는다. 신규 Sprite 요청은 없으며 기존 PLAN-014 요청은 독립 절차를 유지한다.

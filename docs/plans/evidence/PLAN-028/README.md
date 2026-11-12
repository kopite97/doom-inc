# PLAN-028 생명체 이동 보간 검증 증거

- 날짜: 2026-11-12
- 상태: **빌드·고부하 제외 조건으로 기능 완료**. [PLAN-028](../../PLAN-028-creature-movement-interpolation.md)은 completed다. 추가 native 비교는 원본 2회 완료·3회차 취소·변경 후 0회로 보존하며, 전체 화면 비용 증가와 미측정 한계가 해결됐다는 판정은 하지 않는다.
- 환경: Windows, Unity 6000.3.27f1, URP/Direct3D12, Android를 선택한 Editor. 시작 시 Laboratory 씬 clean, Domain Reload OFF, runInBackground false, vSync 1, targetFrameRate -1이었다. [시작 환경](environment-before.json), [원본 무결성 기준](integrity-before.json).
- 범위: 기존 SpriteRenderer에서 논리 위치와 표시 위치를 분리하고 몸체·운반물·선택·입력·정지를 연결한다. 기존 5 PNG·106 Sprite를 재사용하며 신규 아트·저장 스키마·빌드·실기기 검증은 이번 작업에 포함하지 않는다.

## 실제 기능 검증

| 실행 | 확인한 결과 | 증거 |
| --- | --- | --- |
| 최신 Edit Mode | 410/410 통과. 이동 관찰·구간 유효성·보간·보정·정지·복원과 기존 Foundation 회귀 포함 | [JSON](edit-final-result.json), [XML](edit-final-results.xml) |
| 최초 이동 표시 통합 | 2/3 통과. 독립 시뮬레이션이 생성한 새 자원의 GUID 비교 실패를 발견 | [최초 결과](play-movement-first-result.json) |
| 최초 전체 Laboratory, Domain Reload OFF | 45/46 통과. 표시 빈도에 따른 경로 조회 비교의 시작 시점 차이 1개를 발견. 아래 보완 후 최종 재실행에서 통과 | [최초 전체 결과](play-domain-off-first-result.json) |
| 최종 Laboratory, Domain Reload OFF | 46/46 통과. 기존 기능·표시/선택·몸체/운반물·정지/복원·논리 상태 동일성과 비용/수명 포함 | [JSON](play-domain-off-final-result.json), [XML](play-domain-off-final-results.xml) |
| Laboratory, Domain Reload ON | 46/46 통과. 같은 기능·캐시 초기화·세션/화면 수명 회귀 확인 | [JSON](play-domain-on-result.json), [XML](play-domain-on-results.xml) |
| 앱 씬/소유 수명, Domain Reload ON | 29/29 통과. Bootstrap/Title/Laboratory 진입·종료·재개와 화면/세션 소유 경계 확인 | [JSON](app-scenes-result.json), [XML](app-scenes-results.xml) |
| 순수 보간 평가 비용 | 12/24/48개 각각 4096회 평가에서 managed 할당 0 B, 추가 PathQueries/VisitedCells 0, 전후 저장 동일 | [평가 비용](movement-evaluation-costs.json) |
| 화면/세션 수명 | Restart 10회에서 이전 ID·몸체·선택 제거와 이전 세션의 map 구독 해제 확인. ReleaseSession의 캐시/표시 해제, 같은 화면의 Continue 재초기화, 최종 world 제거 통과 | [수명 결과](movement-session-lifetime.json) |
| 변경 전 화면 밖 조건 | 실제 zoom8, 대부분의 작업자가 화면 밖인 조건 1/1 통과 | [실행 결과](baseline-offscreen-result.json), [원시 측정](baseline-level2-offscreen-run1.json) |
| 실제 Editor Update 전후 영상 | 전후 각 150 PNG·30fps 목표·약 5초, 몸체 전체가 모든 캡처에서 viewport 안에 존재. 게임 시간과 실제 렌더 프레임 증가 확인 | [변경 전 결과](video-baseline-30fps-v2-result.json), [변경 후 결과](video-after-result.json) |
| 변경 후 통제 성능 | 기본/중간/고부하 각 3회와 zoom8 1회 완료. 4/4 시나리오 통과는 수집 조건/논리 동일성의 결과이며 아래 비용 증가와 한계를 별도 판정 | [JSON](after-performance-result.json), [XML](after-performance-results.xml), [전후 20개 요약](performance-summary.json) |
| 현재 구현의 Present/active CPU 보조 진단 | 기본/고부하 각 300프레임 예열·600프레임 수집, 1/1 통과. API는 유효하고 전후 active CPU 차이는 미측정 | [JSON](present-diagnostic-result.json), [XML](present-diagnostic-results.xml), [진단 요약](present-diagnostic-summary.json) |

기능·정식 전후 성능 수집과 현재 전용 보조 진단은 완료했다. 기능과 직접 비용 계약은 충족했으나 전체 GC/RenderLoop 증가의 원인·허용 비용 판정을 위한 추가 전후 native 비교는 미완료다. 사용자 요청으로 새 측정을 중단했으며 미측정·취소를 통과로 처리하지 않는다.

## 전후 움직임 자료

- 변경 전: [MP4](movement-baseline.mp4), [GIF](movement-baseline.gif), [좌표/시간 원시 기록](movement-baseline-zoom2-v2-recording.json).
- 변경 후: [MP4](movement-after.mp4), [GIF](movement-after.gif), [좌표/시간 원시 기록](movement-after-zoom2-v2-recording.json).
- [전후 비교 요약](movement-recording-comparison.json): 같은 prototype ID·같은 fixture·priming 1170회·zoom2다. 변경 전에는 소수 지도 위치를 보인 캡처가 0/150, 변경 후에는 140/150이며 캡처 사이 최대 이동은 1칸에서 약 0.1664칸으로 줄었다. 전후 몸체의 서로 다른 위치는 각각 18개와 150개다.

실제 prototype이 자원을 보유하고 ToDestination/양의 MoveProgress에 들어갈 때까지 고정 1/60초 명령으로 준비했다. 녹화 중에는 SimulationRate=1의 실제 Editor Update만 사용한다. 준비 중 기존 Pan을 한 번 호출해 경로 중간과 몸체/머리 높이를 포함했으며 제품에 자동 추적 카메라를 추가하지 않았다. 각 PNG의 몸체·운반물 좌표, 실제 wall/game time, camera clip을 기록했다. 이 영상은 고정 delta 성능 수집이나 실기기 검증과 별개다.

## 통제 성능 비교의 고정 조건

- [변경 전 제품 소스·fixture 해시](source-baseline.json)를 기록했다. 완료된 PLAN-027의 과거 성능 수치를 재사용하지 않고 전환 후 실제 코드에서 새 기준을 수집했다.
- 같은 격리 저장 [기본 1명](movement-fixture-0.json)·[중간 4명](movement-fixture-1.json)·[고부하 24명](movement-fixture-2.json)을 매 회 복원한다. [사전 검증](movement-fixture-preflight.json)에 초기/최종 재고·Job·배정·우선순위·대기 이유와 실제 이동/운반 수가 있다.
- 기본은 완료된 획득시설 1개와 저장소 2개다. 중간/고부하는 기존 준비 구성을 사용하되 실제 가공/배양 작업자를 배정하고 침공형을 배양한다. 600번째 명령마다 기존 ReleaseAll을 호출해 작업자 수를 유지한다. 이 준비와 명령은 테스트 측에만 있으며 제품 정의·게임 규칙을 바꾸지 않는다.
- 모든 회차는 1/60초 × 1800회 예열, 같은 delta × 3600회 수집이다. 렌더 프레임마다 한 명령을 실행하며 vSync 0·targetFrameRate 60으로 wall 30초/60초를 목표로 한다. 실제 wall duration도 각각 기록한다. baseline과 after의 전체 명령 수는 5400회로 같다.
- 1280×720, 상태 UI ON, 보통 글씨, zoom1에서 각 부하 3회다. 고부하의 별도 zoom8/대부분 화면 밖 조건은 1회다. **변경 전 9+1회와 변경 후 같은 9+1회 수집을 완료했다.** 변경 후 실행 전 [환경](environment-performance-after.json)을 원래 Domain Reload OFF·Android·씬 clean으로 맞췄고 각 측정이 vSync 0/60fps를 설정한다.
- 사전 검증의 평균 이동/운반 수는 기본 약 0.742/0.391명, 중간 1.051/0.471명, 고부하 1.896/1.044명이다. 고부하의 최대 이동/운반 수는 9/6명이며 대부분의 나머지 작업자는 NoWork 대기다. 24명 동시 운반 부하로 표현하지 않는다. 초기 prototype의 기본 유기물 4개도 fixture에 남아 있다.
- ProfilerRecorder의 Main Thread·PlayerLoop·RenderLoop·Canvas.BuildBatch·GC·Batches와 사용 가능한 대기 마커를 기록한다. Main Thread에는 프레임 대기와 Editor 비용이 포함된다. PlayerLoop/RenderLoop/대기 마커도 서로 겹칠 수 있으므로 합산·차감하여 독점 CPU 비용으로 간주하지 않는다. 없는 카운터는 -1/invalid로 남긴다.
- RenderLoop는 Present를 포함한다. [Unity 6.3 counter 기준](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/profile-rendering/frame-timing-manager/counter-reference)은 active render-thread work를 RenderLoop에서 Gfx.PresentFrame을 제외한 값으로 정의한다. 이번 raw에는 Gfx.PresentFrame이 없어 RenderLoop를 active 렌더 CPU와 동일시하지 않는다.
- 수집 버퍼와 WorkerComponent/Renderer 참조를 사전 할당한다. 전체 프레임 GC에는 기존 게임·화면·TestRunner 비용이 들어가며 신규 보간 자체의 0 B와 구분한다. raw의 첫 이동-count 행은 예열 전 원점과 비교하는 관찰 한계가 있어 요약에서 그 행만 제외한다. 시간·GC와 다른 카운터는 3600행 전체를 사용한다.

[성능 요약](performance-summary.json)은 다음 명령으로 원시 파일에서 재생성한다. 실제 zoom·작업자 수·명령 수·화면 밖 조건이 맞지 않는 기록은 보존하되 비교에서 제외한다. 최종 20개는 모두 조건이 유효하며 제외된 본 측정은 없다.

```powershell
node scripts/analyze-plan028-performance.mjs
```

최신 보조 순수 평가의 median은 12/24/48개에서 약 0.0066/0.0134/0.0273 ms였다. 각각 독립된 정상 경로/시설 claim을 가진 128×8 보조 Map의 Evaluate 호출만 측정한 결과다. 전체 화면 CPU나 기기 성능을 뜻하지 않는다. 준비·JIT 예열·직렬화·단언은 할당 구간 밖이다. 수명 10회의 명시적 GC 후 메모리는 약 8 KiB 범위에서 오갔으며 지속 증가를 보이지 않았다. 이 값은 Editor 전체 managed heap 관찰이며 제품 전체 메모리 불변을 증명하지 않는다.

### 전후 정량 결과와 측정 한계

아래 median은 반복별 median의 중앙값, p95도 반복별 p95의 중앙값이다. 기본/중간/고부하는 전후 각 3회, 화면 밖은 전후 각 1회다. 10쌍 모두 명령·모델 시간·작업자/이동/운반/대기 수와 PathQueries/VisitedCells가 일치했다. 화면 밖 한 쌍은 반복 편차나 지속 회귀를 판정하는 자료로 확대하지 않는다. 모든 회차의 max와 반복 범위는 [요약 JSON](performance-summary.json)에 보존한다.

| 지표: 변경 전 → 후 | 기본 1명 | 중간 4명 | 고부하 24명 | zoom8 화면 밖 |
| --- | --- | --- | --- | --- |
| Main Thread median, ms | 16.6548 → 16.6582 | 16.6616 → 16.6624 | 16.6565 → 16.6567 | 16.6638 → 16.6609 |
| Main Thread p95, ms | 17.1839 → 17.2219 | 17.2083 → 17.1306 | 17.1134 → 17.1910 | 17.1725 → 17.1635 |
| PlayerLoop median, ms | 1.7509 → 1.8229 | 2.0391 → 1.8757 | 2.6070 → 2.5322 | 2.2642 → 2.2310 |
| PlayerLoop p95, ms | 2.2187 → 2.3180 | 2.5016 → 2.3171 | 3.9363 → 3.5991 | 3.2318 → 3.2430 |
| Canvas.BuildBatch p95, ms | 0.0078 → 0.0070 | 0.0080 → 0.0067 | 0.0090 → 0.0078 | 0.0072 → 0.0070 |
| 전체 GC median, B/frame | 364852 → 364632 | 383236 → 403226 | 445727 → 457860 | 459598 → 461434 |
| 전체 GC p95, B/frame | 394986 → 383980 | 411498 → 431298 | 674000 → 684948 | 683702 → 685664 |
| Batches median / p95 | 25/26 → 25/26 | 33/37 → 33/37 | 55/61 → 55/62 | 15/17 → 15/17 |
| RenderLoop median, ms | 11.1853 → 11.2295 | 10.9145 → 11.0965 | 10.3414 → 10.5059 | 10.7106 → 10.8067 |
| RenderLoop p95, ms | 11.7396 → 11.8494 | 11.5575 → 11.5901 | 10.9630 → 11.1368 | 11.2571 → 11.3231 |

- PlayerLoop p95의 기본 반복 범위는 2.0947~2.3324 → 2.3041~2.3705 ms로 겹친다. 중간은 2.4060~2.5974 → 2.2824~2.3285 ms, 고부하는 3.8598~4.1288 → 3.4886~3.8486 ms로 낮아졌다. 기본 PlayerLoop median의 약 0.0721 ms 증가는 존재한다. Main Thread는 프레임 제한을 포함하며 기본/고부하 p95도 약 0.0380/0.0776 ms 증가해 전체 CPU가 일괄 개선됐다고 표현하지 않는다.
- 신규 위치 평가 scope의 p95는 기본/중간/고부하/화면 밖 0.0083/0.0114/0.0291/0.0274 ms다. 같은 순서의 DrawArt p95는 0.0893/0.1636/0.4332/0.3996 ms, 정렬은 0/0.0022/0.0058/0.0054 ms, 상태는 0.0693/0.1619/0.3732/0.2164 ms다. 평가·정렬은 DrawArt 안의 구간이라 합산하지 않는다. 기존 baseline에는 이 신규 scope가 없어 전후 차이는 미측정이다. 신규 정상 Evaluate 자체의 managed 0 B와 추가 경로 조회 0은 별도 직접 검증 결과다.
- RenderLoop p95는 기본 약 0.1098 ms·고부하 0.1738 ms 증가했고 두 조건 모두 변경 후 세 반복이 변경 전 범위보다 높다. [공식 counter 정의](https://docs.unity.com/en-us/engine/6000.3/manual/analysis/graphics-performance-profiling/profile-rendering/frame-timing-manager/counter-reference)에 따라 이 Present 포함 범위 증가를 active 렌더 CPU 증가나 개선으로 단정하지 않는다. baseline의 Gfx.PresentFrame이 없어 active render-thread work의 전후 차이는 미측정이다.
- [논리 frame별 paired delta/상관](range-correlation.json)에서 RenderLoop delta와 PlayerLoop delta의 Pearson은 기본 약 -0.35~-0.68, 고부하 -0.71~-0.79다. Main Thread delta와의 상관 절댓값은 약 0.15 이하였다. 수집된 WaitForTargetFPS의 p95는 양쪽 약 0.0004 ms이며 그 delta와 RenderLoop delta의 상관은 -0.04~0.005로 약했다. Gfx.WaitForPresentOnGfxThread는 모두 0으로 상관을 정의할 수 없었다. **현재 wait 관찰만으로 RenderLoop 증가가 대기 증가 때문이라고 결론 내리지 않는다.**
- 중간/고부하 전체 GC p95도 각각 19800 B/frame(4.81%)/10948 B/frame(1.62%) 증가했고 변경 후 세 반복이 기준 범위보다 높다. 활성 Transform은 기본 81→81, 고부하 208→210이었다. 이는 활성 표시 수 관찰이며 전체 GC의 발생 위치를 특정하는 자료는 아니다. 순수 보간 0 B를 전체 게임 0 B나 전체 화면 할당 감소로 확대하지 않고 이 증가와 원인 분리의 한계를 남긴다.

직접 요구한 보간 무할당·추가 경로 조회 0·해제 후 잔존 0은 충족했다. 연속 몸체·운반물·선택 표시를 위한 매 프레임 평가/게시의 측정 비용은 위 신규 scope로 한정해 설명한다. 기본 PlayerLoop의 작은 증가, 중간/고부하 PlayerLoop p95와 전 조건 Canvas p95, Batches와 별도 정상 평가의 선형 표본은 비용 허용 검토의 근거다. 이 근거가 RenderLoop/전체 GC 증가의 원인을 특정하거나 그 회귀를 지우지는 않는다. 남은 증가를 숨긴 단순 성능 통과나 전역 할당 최적화 완료로 기록하지 않는다.

### 현재 구현의 Present와 active CPU 보조 진단

[기본 raw](present-diagnostic-level0.json)·[고부하 raw](present-diagnostic-level2.json)는 본 비교가 끝난 뒤 같은 fixture/명령/zoom1에서 각각 300프레임 예열·600프레임 수집한 **현재 구현만의 진단**이다. 본 측정의 30초/60초×3회를 대체하지 않는다. FrameTimingManager가 실제 활성화됐고 두 조건 모두 서로 다른 유효 timestamp 600개를 얻었다. API 값은 이미 ms이며 ProfilerRecorder의 ns와 구분했다. [Unity FrameTiming API](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/frametiming)는 cpuRenderThreadFrameTime을 렌더 작업 제출 시작부터 Present 호출 전까지의 CPU 시간으로 정의한다.

| 현재 API 값, median / p95 / max, ms | 기본 1명 | 고부하 24명 |
| --- | --- | --- |
| CPU main work | 1.4147 / 1.8924 / 35.6538 | 2.2867 / 3.0050 / 22.5003 |
| CPU render work | 0.8376 / 0.9914 / 1.5332 | 0.8534 / 1.0969 / 1.4912 |
| CPU main Present wait | 0.2643 / 0.4245 / 0.7552 | 0.1525 / 0.4110 / 0.6090 |
| GPU frame | 1.2618 / 1.3588 / 1.5811 | 1.2083 / 1.2872 / 1.4707 |

직접 CPU Render Thread Frame Time counter의 p95도 0.9914/1.0967 ms로 API와 일치하는 수준이었다. 하지만 Gfx.PresentFrame 마커는 available/valid여도 전행 0이며 graphicsMultiThreaded=false다. 따라서 RenderLoop에서 이 0을 빼서 exclusive CPU라고 부르지 않는다. API는 지연된 캡처 프레임을 반환하므로 같은 행의 논리 명령/RenderLoop와 일대일 비용 차감도 하지 않는다. baseline에는 이 API/counter 쌍을 수집하지 않아 **active render CPU의 전후 차이와 RenderLoop 지속 증가의 원인은 여전히 미측정**이다. 이번 진단은 현재 실제 렌더 작업 범위의 유효값을 보완하며 이전 값을 추정하지 않는다.

[진단 요약](present-diagnostic-summary.json)은 `node scripts/analyze-plan028-present.mjs`로, [paired 상관 자료](range-correlation.json)는 `python scripts/analyze-plan028-range-correlation.py`로 재생성한다.

### 같은 30~90초 구간의 원본/현재 native CPU 비교 보완

최종 검토에서 현재 전용 진단만으로는 원래 RenderLoop 증가의 원인·허용 비용을 판정할 수 없음을 다시 확인해 원본/현재 구현의 직접 FrameTiming API 비교를 추가 수집했다. 이후 사용자가 고부하 테스트를 정리하고 다음 날 이어가도록 명시해 중단했다. 기존 정식 20회와 현재 전용 진단은 그대로 보존한다.

- [수집 조건과 해시](native-comparison/collection-conditions.json): 기본/고부하 각각 전후 3회, 1800프레임 예열·3600프레임 수집, 같은 논리 30~90초·fixture·명령 1801~5400·주기 방출·해상도·zoom1·상태 UI·프레임 제한이다. 수집 테스트와 명령 helper의 해시도 고정했다.
- [소스/바이트 백업](native-comparison/source-backup-manifest.json) 후 기존 제품 6파일을 원본으로 임시 전환했다. [원본 검증](native-comparison/sources-baseline.json)의 기존 137개 제품 소스는 최초 baseline 해시와 일치했다. 새 제품 3파일과 직접 의존 테스트 4파일은 메타/GUID를 보존하며 임시로 컴파일에서 제외했다. 중단 후 [복원 검증](native-comparison/sources-restore.json)에서 최종 제품 140개·보호한 소스/메타 26개 바이트가 백업과 일치하고 임시 제외도 해제됐음을 확인했다.
- 예비 짧은 원본 진단은 논리 5~15초로 정식 비교와 운전 구간이 달랐다. [예비 기록](native-comparison/initial-short-baseline-run1/test-result.json)은 보존하고 새 30~90초 집계에서 제외한다.
- 첫 긴 진단은 두 부하의 예열/수집 총 180초가 기본 테스트 제한 180000ms에 걸려 약 180.05초에 중단됐다. [실패 결과](native-comparison/initial-timeout-baseline-run1/test-result.json)와 [당시 수집 조건](native-comparison/initial-timeout-collection-conditions.json)을 보존했다. 명시적 Timeout(300000)만 추가하고 제품·fixture·명령·측정 루프는 유지했다. 최종 조건에는 새 테스트 해시를 기록하며, 완료된 1/1 회차만 최종 비교에 사용한다.
- [진행 집계](native-comparison/summary.json)는 `node scripts/analyze-plan028-native-comparison.mjs`로 재생성한다. 3600개 fresh timestamp와 유한 양수 CPU 값, 고정 명령/환경, 소스·fixture·수집 코드 해시, 회차 결과를 확인하며 유효하지 않은 표본을 통과로 대체하지 않는다.
- 직접 API의 median/p95/max와 반복 범위는 원본 2회까지만 집계됐다. 변경 후 같은 구간은 수집하지 않아 전후 차이·지속 회귀·허용 비용을 판정하지 않는다. API 프레임은 지연되어 반환되므로 같은 행의 RenderLoop와 차감하지 않는다. 이 진단은 GC 발생 위치도 수집하지 않아 기존 전체 GC 증가의 한계는 그대로 남는다.

| 보류 시점 native 회차 | 실제 결과 | 증거 |
| --- | --- | --- |
| 원본 1회차 | 기본/고부하 각각 1800/3600, 각 3600 fresh API. 1/1 통과·183.36초 | [JSON](native-comparison/baseline-run1/test-result.json), [XML](native-comparison/baseline-run1/test-results.xml) |
| 원본 2회차 | 기본/고부하 각각 1800/3600, 각 3600 fresh API. 1/1 통과·182.27초 | [JSON](native-comparison/baseline-run2/test-result.json), [XML](native-comparison/baseline-run2/test-results.xml) |
| 원본 3회차 | 시작 후 사용자 요청으로 취소. 완료 raw 없음·통과 아님 | [중단 상태](native-comparison/baseline-run3/test-result.json), [취소 요청](native-comparison/user-cancel.json), [Play 종료](native-comparison/user-stop.json) |
| 변경 후 1~3회차 | 미실행 0회. 전후 비교 미완료 | [진행 집계](native-comparison/summary.json) |

보류 시점 집계는 `collectionComplete=false`다. 완료된 1/1 회차의 원시 기록 4개만 채택하며 취소된 3회차와 예비 짧은/시간 초과 기록은 최종 비교에 넣지 않는다. 제품은 보간이 구현된 최종 소스로 복원했고 Editor·폰트도 시작 상태로 정리했다. 재개 시 원본/현재 소스·수집 코드·fixture·환경 해시를 다시 확인한 뒤 남은 원본 3회차와 변경 후 3회차를 수집할지 판단한다. 현재 추가 성능 테스트는 실행하지 않는다.

## 발견한 문제와 증거 보존

- 최초 기존 Prepare fixture는 완료된 시설이 있어도 생산 작업자 배정이 없어 30초 예열 후 대부분 정지했고 운반이 없었다. 기본은 시설이 없고 초기 자원을 든 상태라 배회도 하지 않았다. [초기 기록](initial-idle/performance-summary.json)과 초기 fixture/raw를 보존하고 실제 생산 배정·운반을 사전 검증한 뒤 본 baseline을 다시 수집했다. 초기 기록을 이동 부하 비교로 사용하지 않는다.
- Reload 직후 Fit/Zoom을 적용하면 다음 CheckLayout의 generation 초기화가 zoom1로 되돌렸다. [잘못된 화면 밖 raw](initial-idle/offscreen-zoom1.json)는 보존한다. 초기화 프레임을 먼저 기다린 뒤 배율을 적용하고 실제 zoom/visible 수를 단언해 zoom8 조건을 재실행했다.
- 최초 12fps 영상은 실제 zoom1이었다. [초기 메타데이터](movement-baseline-recording.json)는 그 조건의 기록으로 보존한다. 최종 영상은 전후 동일한 정식 prototype·150 PNG/30fps/zoom2로 다시 만들었다.
- 첫 30fps/zoom2 영상은 논리 원점 중간만 기준으로 삼아 몸체 위쪽이 30개 캡처에서 잘렸다. [실패 메타데이터](initial-idle/framing-failed-baseline-zoom2-recording.json), [실패 결과](video-baseline-30fps-result.json)를 보존했다. CellScreenPoint의 (.5,.5)와 추가 머리 높이 .4칸을 반영해 고정 카메라를 준비하고 전체 몸체 포함을 재검증했다.
- [최초 Edit 결과](edit-first-result.json)의 아래 방향 fixture는 기대한 첫 이동 구간과 실제 시설 배치가 달랐다. fixture를 보완한 뒤 최신 Edit 410/410을 확인했다.
- 독립 실행의 신규 자원 ID는 Guid.NewGuid로 달라진다. 기존 저장 ID는 그대로 검사하고 새로 생긴 ID와 그 관계만 CreationOrder로 정규화해 전체 저장을 비교하도록 검증 준비를 수정했다. [최초 통합 실패](play-movement-first-result.json)를 보존한다.
- [최초 전체 회귀](play-domain-off-first-result.json)의 경로 조회 55/56 차이는 초기 화면 warming 두 프레임을 한쪽 기준에서 빠뜨린 관찰 구간 문제였다. 기준 counter를 yield 이전으로 옮겨 같은 구간을 포함하며 정확한 동일성 요구는 유지했다. 수정 후 Domain Reload OFF/ON에서 각각 46/46을 통과했다.

## 재현과 보류 정리

긴 성능·영상·사전 검증과 과거 PLAN-027 성능 시나리오는 Explicit다. 일반 기능 회귀가 과거 증거를 덮어쓰거나 긴 측정을 자동 반복하지 않게 구분한다. 성능을 의도적으로 실행할 때는 실제 stage 파일을 baseline/after로 지정하고 다음과 같이 선택한다.

stage는 출력 이름이며 제품 구현을 바꾸는 스위치가 아니다. baseline을 재현하려면 먼저 해당 제품 소스 해시와 fixture가 일치해야 한다.

```powershell
$projectPath = (Resolve-Path UnityProject).Path
unity command test run --mode playmode --filter MovementPerformance --filter_type category --include_explicit true --async_tests true --project-path $projectPath --caller plugin --skill unity-cli --json
unity command test status --project-path $projectPath --caller plugin --skill unity-cli --json
```

정식 회귀·after 성능·현재 전용 보조 진단까지의 자료는 보존했다. 추가 native 전후 비교와 비용 판정은 보류했고 제품 소스·폰트·Editor를 복원했다. 신규 Sprite 요청은 없으며 Player/Addressables/IL2CPP 빌드와 기기 실행은 하지 않았다. 다음 PLAN을 자동 활성화하지 않는다.

아래는 추가 native 비교를 시작하기 전의 종료 확인이다. 보류 시점의 제품 소스 복원은 위 native 복원 검증을 따르며 최신 환경/Console/무결성 확인은 PLAN의 중단 기록을 따른다.

| 추가 native 비교 전 종료 확인 | 당시 실제 결과 | 증거 |
| --- | --- | --- |
| 컴파일 | 완료·실패 false·오류 없음 | [최종 상태](compile-final.json) |
| Console | 최종 오류/경고 0. 과거 예비 측정 중단 2개 오류·앱 실패 복구 테스트의 예상 경고 9개·잘못된 CLI 조회 1개 오류는 원문 보존 후 정리. 콘솔 정리를 원인 해결로 해석하지 않음 | [정리 전 원문](console-before-final-cleanup.json), [최종 상태](console-final.json) |
| Editor | 시작 환경과 값 동일: Play 정지·Laboratory 씬 clean·Domain Reload OFF·background false·vSync 1·targetFrameRate -1·Android·URP/Direct3D12 | [시작](environment-before.json), [종료](environment-after.json) |
| 아트·데이터·씬·설정 | Sprites/JSON/SO/Scenes/ProjectSettings의 259개 해시 변경 0 | [종료 무결성](integrity-after.json) |
| 폰트와 임시 테스트 씬 | TMP ClearFontAssetData(true) 지원 API로 동적 글리프/문자 테이블과 1×1 atlas 복원, 해당 에셋 git diff 0. 임시 InitTestScene 0개 | [폰트 복원](font-cache-restored.json) |
| 사용자 저장/설정 | persistent의 제품 저장/설정 변경 없음. 기존 TestResults.xml만 새 테스트 결과로 바뀜 | [시작](integrity-before.json), [종료](integrity-after.json) |
| 문서 | 당시 완료 시도의 PLAN·목록·구현/아트 로드맵·기획 MAP·관련 PLAN-027 동기화와 검사 통과. 이후 native 비용 검증을 보완하다가 사용자 요청으로 on hold 전환 | [현재 PLAN](../../PLAN-028-creature-movement-interpolation.md) |

## 빌드·고부하 제외 재개와 최종 판정

2026-11-12 사용자가 빌드·고부하 테스트를 제외하고 재개하도록 명시적으로 요청했다. [범위 감사](resume-scope-audit.json)에서 제품 소스 140개·보호 소스/메타가 검증 당시와 동일하고 보존 에셋 259개 변경 0, 임시 제외/테스트 씬 0·폰트 diff 0을 확인했다. 기존 Edit 410/410·Laboratory OFF/ON 각각 46/46·앱 씬 29/29와 실제 움직임 영상은 기존 실행 증거로 유지한다. 코드 변경이 없으므로 이를 다시 실행하지 않았고, Editor는 실행 중이 아니어서 새로 열지 않았다.

기능 구현과 허용된 필수 검증은 완료했다. 추가 native 비교·빌드·실기기는 이번 필수 완료 범위에서 제외했으며 미실행을 통과로 표시하지 않는다. 기존 GC/RenderLoop 증가와 active-render CPU 전후 미측정은 남은 한계다. 이 완료 판정은 전체 성능 무회귀나 비용 원인 해결 판정이 아니다. 신규 Sprite 요청은 없으며 active 0개로 대기한다.

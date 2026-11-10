# PLAN-027 연구소 월드 표시 전환 증거

- 날짜: 2026-11-10
- 상태: 완료 — PLAN-027 completed, PLAN-028 proposed, active 0개
- 계획: [PLAN-027](../../PLAN-027-laboratory-tilemap-and-sprite-rendering.md), 설계: [TECH-017](../../../decisions/TECH-017-laboratory-world-rendering.md)
- 환경: Windows, Unity 6000.3.27f1, Unity CLI 1.0.0-beta.17, URP 17.3 Renderer2D, Android 선택 상태의 Editor. Player·Addressables·IL2CPP 빌드와 실기기 실행은 하지 않았다.

## 기준과 범위

기준 소스는 `7e4fe40c7c36e6c5c43850b156fff395159bbfe7`의 uGUI 지도다. 실행 전에 [원본/설정/저장 경로 해시](baseline-integrity.json)를 기록했다. PLAN-028 문서와 기존 사용자 문서 변경을 보존하며 보간 기능은 실행하지 않는다.

5 PNG·106 Sprite의 프레임/리비전·GUID·임포트는 그대로 사용한다. 기존 제공본의 가로/세로 1·3·8배 비교 기준은 [PLAN-026 승인 화면](../PLAN-026/integration-2026-11-10/README.md#실제-게임-화면)이다. 같은 배치·줌 시나리오의 새 캡처는 아래에 있다. 별도의 반복 성능 시나리오에서는 실제 변경 전 uGUI와 변경 후 월드 화면을 같은 저장으로 새로 캡처한다.

- 바닥: 런타임 소유 Grid/Tilemap, 2×2 월드 단위, 기존 고정 해시. 16×10 변형 수는 116/9/14/21이다.
- 시설/개체: Guid별 SpriteRenderer. 정식 아트가 없는 종류는 공유 단색 Sprite와 기존 역할 식별자/생성 순서 주석을 사용한다. 같은 칸의 개체 주석은 한 번만 표시한다. 상세 이름·상태 UI의 번역은 유지한다.
- 운반: 몸체와 CarryAnchor를 분리하고 실제 보관처·예약/대기·잔량으로 표시한다. 자원 Icon이 없으면 단색 대체를 쓴다. 유기물 하단 `(18,28)`은 아트 SO의 Sprite별 받침 데이터다.
- UI: 기존 uGUI 입력/상태/팝업 유지. 칸별 Image/TMP 배열을 제거했다. 월드 카메라만 활성 렌더하고 지도 바깥을 4개 UI 사각형으로 채운다. 지도 비활성 시 기존 배경 카메라를 복원한다.
- Material: `Assets/Materials/Laboratory/LaboratoryWorld.mat` 하나를 씬/공용 구성·생성 도구가 참조한다. 추가 패키지·PNG·원본 복제·개체별 Material은 없다.

## 기능 검증

| 실행 | 결과 | 증거 |
| --- | --- | --- |
| 첫 전환본, Domain Reload ON 연구소 회귀 | 40/40 통과. 이후 픽셀 경계·정렬·단일 활성 카메라를 보완함 | [결과](play-domain-on-first.json) |
| 추가 월드 계약 | 2/2 통과: 모든 칸의 중심/네 모서리 투영, 네 회전·기존 종류, 비정수 줌, 0 크기, 수명, 실제 배양액 집기/복원/운반 해제 | [결과](world-contract-final.json) |
| 픽셀 보정·정렬 후 Domain Reload OFF | 42/42 통과. 이후 카메라 렌더 비용을 보완함 | [결과](play-domain-off-before-camera-optimization.json) |
| 최종 Edit Mode | 363/363 통과, 106 Sprite 임포트/프레임·공용 Material·생성 도구 참조 포함 | [결과](edit-foundation-final.json) |
| 최종 연구소 회귀, Domain Reload OFF | 42/42 통과. 단일 활성 카메라·좌표·합성·입력·운반·저장·밀집 표시·반복 성능 포함 | [최종 결과](play-domain-off-final.json) |
| 앱 씬/소유 수명, Domain Reload ON | 29/29 통과. Bootstrap/Title/Laboratory·새 판/이어하기·종료·Owner 취소·Loading/Popup/UI 수명 | [결과](app-scenes-domain-on.json) |
| 단일 활성 카메라 화면 시나리오 | 1/1 통과, 전체/중간/최대 확대와 네 방향 작업 캡처 | [결과](single-camera-visual.json) |

이전 uGUI 내부 구조를 직접 찾던 아트/바닥 테스트는 SpriteRenderer·Tilemap·화면 좌표·자원 소유 결과를 확인하도록 이관했다. 기존 저장/입력/재생 검증을 삭제하지 않았다. 실제 터치 대신 Input System 합성 터치를 사용한다.

수정 중 발견한 항목:

- 새 종류 테스트가 존재하지 않는 선택 키 `raiderId`를 사용해 실패했다. 카탈로그의 전체 CreatureData를 열거하도록 준비 코드를 수정했다. [첫 결과](world-contract-first.json).
- 비정수 viewport에서 카메라의 정수 픽셀 폭과 uGUI 실수 좌표가 달라 최대 0.215픽셀의 오차가 검출됐다. 출력 경계를 안쪽 정수 픽셀로 정하고 중심/크기를 보정해 왕복 검증을 통과했다. [실패 결과](world-contract-projection-failure.json).
- 단일 활성 카메라 전환 후 기존 효과음 테스트의 `Camera.main.GetComponent<AudioListener>()` 가정을 발견했다. 카메라와 분리된 기존 Listener의 활성 단일성을 검사하도록 보완했다. 관련 대상 자동 이동 테스트는 고정 .25초 대기 대신 실제 화면 진입을 최대 .6초 안에 확인한다. 제품의 .2초 Tween은 변경하지 않았다. [발견 결과](single-camera-regression-first.json), [최종 전체 통과](play-domain-off-final.json).
- 최초 두 카메라 구성의 RenderLoop 비용 증가를 확인했다. 아래 반복 측정과 단일 활성 카메라 재검증으로 판정하며 최초 수치를 최종 통과로 사용하지 않는다.

## 화면

| 확대 | 가로 | 세로 |
| --- | --- | --- |
| 1 | [1280×720](screens/mixed-1280x720-zoom1.png) | [720×1280](screens/mixed-720x1280-zoom1.png) |
| 3 | [1280×720](screens/mixed-1280x720-zoom3.png) | [720×1280](screens/mixed-720x1280-zoom3.png) |
| 8 | [1280×720](screens/mixed-1280x720-zoom8.png) | [720×1280](screens/mixed-720x1280-zoom8.png) |

[위](screens/work-rotation-0-Up.png)·[오른쪽](screens/work-rotation-1-Right.png)·[아래](screens/work-rotation-2-Down.png)·[왼쪽](screens/work-rotation-3-Left.png) 작업 방향, [부분 입고 뒤 남은 운반물](screens/partial-admission-waiting-cargo.png).

Point/PPU32를 유지한다. Pixel Perfect Camera를 도입하지 않으므로 비정수 줌·팬에서 원본 픽셀의 화면 폭을 모두 같게 보장하지 않는다. 이번 기술 검증이 후속 정식 아트 검수나 모바일 선명도 승인을 대신하지 않는다.

## 성능 비교 방법

- [기본 fixture](fixture-0.json): 보조형 1개체. [고부하 fixture](fixture-2.json): 기존 Plan020Probe의 시설 8개·작업자 24개·외부 침공형 16개 사례. 종료를 피하는 진단용 침공 한도는 fixture에만 존재하며 제품 데이터는 변경하지 않았다.
- 같은 파일을 격리 슬롯에 복원하고 `.05초 × 100회`로 5초 시점에 맞춘다. 시뮬레이션 자동 진행은 0, 상태 표시 ON·보통 글씨·1280×720·zoom1이다.
- 매 회 5초 워밍업 후 10초 수집, 기본/고부하 각각 3회. 최종 비교는 두 구현 모두 Domain Reload OFF다. 샘플 목록 용량 10000을 미리 잡고 프레임당 동일한 `double[5]` 수집 비용을 포함한다.
- Main Thread, RenderLoop, GC Allocated In Frame, Batches Count를 ProfilerRecorder로 수집한다. Main Thread에는 대기/Editor 작업이 포함되며 RenderLoop는 가용 렌더링 마커의 비용이다. GPU 시간·모바일 FPS를 뜻하지 않는다.
- 최초 `Render Thread` recorder는 이 Editor에서 제공되지 않았다. 그 초기 결과는 [초기 기록](initial-measurements/ugui-level0-run1.json)에 남겼다. 사용 가능한 RenderLoop를 확인하고 현재 구현 파일을 임시 백업한 뒤 기준 커밋의 렌더러만 제한적으로 재생해 같은 fixture·조건으로 다시 측정했다. 이후 새 구현을 복원하고 임시 기준 코드를 삭제했다. 영구 이중 렌더러는 없다. [기준 재측정](baseline-renderloop-tests.json).
- 두 카메라 중간 결과는 [중간 측정](two-camera-measurements/world-level0-run1.json)에 별도 보존했다. 최종 수치는 [반복별 요약](performance-summary.json)과 루트의 `ugui/world-level{0,2}-run{1,2,3}.json` 원시 행을 따른다.

### 최종 성능 결과

아래 중앙값/p95는 각 3회 결과의 중앙값이다. 표시 객체는 씬의 **활성 Transform 수**이며 비활성 객체까지 합친 총생성 수가 아니다.

| 지표 | 기본 uGUI → 월드 | 고부하 uGUI → 월드 |
| --- | --- | --- |
| Main Thread 중앙값 | 1.774 → 0.999 ms (43.7% 감소) | 2.322 → 1.392 ms (40.0% 감소) |
| Main Thread p95 | 2.241 → 1.129 ms | 3.136 → 1.869 ms |
| RenderLoop 중앙값 | 0.583 → 0.609 ms | 0.590 → 0.607 ms |
| RenderLoop p95 | 0.741 → 0.718 ms | 0.839 → 0.819 ms |
| GC 중앙값 | 118,096 → 21,270 B/frame (82.0% 감소) | 171,742 → 63,890 B/frame (62.8% 감소) |
| 활성 Transform | 533 → 63 | 569 → 206 |
| Batches 중앙값 | 15 → 15 | 22 → 58 |

- Main 중앙값 3회 범위: 기본 1.722~1.835 → 0.954~1.006ms, 고부하 2.300~2.379 → 1.386~1.431ms. 지속적인 총 CPU/GC 악화는 없었다.
- RenderLoop 중앙값 범위: 기본 0.563~0.613 → 0.582~0.615ms, 고부하 0.582~0.606 → 0.601~0.626ms. 대표값의 +0.017~0.026ms 차이는 반복 분산과 겹치고 p95는 감소했다. 첫 두 카메라 구성의 +0.12ms 수준 증가는 해소했다. 렌더링 CPU 자체가 개선됐다고 주장하지 않는다.
- 고부하 배치 수 증가 원인은 칸별 UI 대체 그림을 개체별 SpriteRenderer·합성 정렬로 바꾼 것이다. 몸체만 보이는 개체는 SortingGroup을 비활성화하고 공유 Material을 사용한다. 표시 수명/겹침·선택 계약을 유지한 상태에서 전체 CPU/GC와 p95 기준은 충족했다. 배치 수 증가는 개선으로 표기하지 않으며 모바일 GPU/드로우콜 영향은 별도 빌드·기기 검증 범위다. Atlas/파이프라인 변경으로 범위를 확대하지 않았다.
- 이 수치는 정지한 동일 시뮬레이션 경계의 **표시 비용 비교**다. 진행 중 게임 전체 처리량이나 모바일 성능을 대표하지 않는다. 실제 진행·상태 변화는 별도 기능 회귀에서 확인했다.

## 종료 검사

- [원본 무결성](final-integrity.json): 모든 기존 PNG/meta와 Assets JSON 불변. ProjectSettings는 의도한 LaboratoryWorld 레이어 추가/Unity 직렬화 갱신 외에 불변이다. 원래 persistent 경로의 session/preferences는 새로 생기지 않았으며 기존 Analytics 파일도 불변이다. TestRunner의 TestResults.xml만 변경됐다.
- [생성 도구 재실행](scene-regeneration.json): 기존 씬 바이트 불변, 씬/공용 구성의 Art·바닥·Material 참조 동일, dirty false.
- [환경 복원](environment-final.json): Play 정지, 원래 Laboratory 씬 clean, Android 대상, Domain Reload OFF, runInBackground false, 잔여 월드 0. 테스트 폰트 캐시를 Editor API로 비워 원래 파일과 일치시켰다. Direct3D12, graphicsMultiThreaded false, vSync 1, targetFrameRate -1인 Editor 환경이다.
- [Console 기록](console-final.json)의 9개 경고는 앱 씬 테스트가 의도적으로 주입한 누락 데이터/저장/Popup 실패다. 최종 컴파일 및 실제 Console 오류는 0이며 기록 보존 후 Console을 정리했다. [복원 후 상태](console-restored.json)는 오류/경고/컴파일 실패가 모두 0이다.
- 필수 완료 조건을 충족해 PLAN-027을 completed로 전환했다. PLAN-028 이동 보간은 별도 proposed이며 실행하지 않았다. 빌드/실기기·모바일 GPU 검증은 제외했다.

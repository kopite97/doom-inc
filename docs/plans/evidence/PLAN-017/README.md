# PLAN-017 Editor 검증 기록

환경: Windows, Unity 6000.3.27f1, Unity CLI 1.0.0-beta.15 / Pipeline 0.9.1-exp.1, 기존 열린 Editor. Android 대상 설정을 유지했다. 사용자 요청에 따라 Player 컴파일·빌드·APK/AAB 생성·설치·실기기 검증은 실행하지 않았다.

## 자동 검증

| 검증 | 실제 결과 | 증거 |
| --- | --- | --- |
| 최종 Edit Mode 전체 | 302/302 통과, 실패·건너뜀 0 | [JSON](edit-final-result.json), [XML](edit-final-results.xml) |
| Domain Reload ON 전체 Play Mode | 최종 51/51 통과, 실패·건너뜀 0 | [JSON](play-on-result.json), [XML](play-on-results.xml) |
| Domain Reload OFF 전체 Play Mode | 50/51 통과. 외부 진단 로그로 실패한 1개는 같은 OFF 조건에서 1/1 재검증 통과 | [전체 JSON](play-off-result.json), [전체 XML](play-off-results.xml), [대상 재검증](play-off-ending-rerun-result.json) |
| 입력 잠금 해제 이후의 이전 클릭 거부 | 1/1 통과. 동일 문구·동일 종류의 새 Show 이후에도 이전 누름을 거부하고 새로운 클릭은 한 번만 처리 | [JSON](loading-stale-off-result.json) |
| 씬·프리팹 참조 | 씬 6개·프리팹 1개의 Missing Script 0, 옮긴 네 스크립트 GUID 유지 | [Unity 참조 검사](asset-reference-audit.json), [GUID/meta](static-audit.json) |

실행 명령은 저장소 루트 기준이며 각각 완료 상태와 실제 결과를 확인한 뒤 다음 실행을 시작했다.

```powershell
unity command script recompile --project-path ./UnityProject --caller plugin --skill unity-cli --json
unity command script recompile_status --project-path ./UnityProject --caller plugin --skill unity-cli --json
unity command test run --mode editor --async_tests true --project-path ./UnityProject --caller plugin --skill unity-cli --json
unity command test run --mode playmode --async_tests true --project-path ./UnityProject --caller plugin --skill unity-cli --json
unity command test status --project-path ./UnityProject --caller plugin --skill unity-cli --json
```

ON은 `enterPlayModeOptionsEnabled=false`, OFF는 `true`와 `DisableDomainReload` 조합이다. OFF는 [시작 직전](domain-off-options-final.json)과 [실행 도중](play-off-observed-options.json)의 실제 설정도 확인했다. Scene Reload는 유지했다. 수정 코드와 추가 테스트는 최종 실행에 포함되며 테스트 필터로 실패를 제외하지 않았다.

## 확인한 계약

- 생산: 건설·미입고 완료품·배양 대상·접근성·재료 우선순위, 회차 시작 후 재료 재요구 없음, 다음 회차 대상, 유인/무인 속도, 운반·부분/최종 입고, 저장 복원. 새 판정 조회의 반복 호출이 진행/재료를 변경하지 않는지도 단언했다.
- 파일: 최초/교체 쓰기, 실제 파일 잠금으로 발생한 교체 실패와 기존 바이트 보존·임시 파일 정리·재시도. 설정과 게임 저장은 기존 정책·경로·오류 처리를 유지한다.
- 저장 조회: 없음·읽기 성공·읽기 실패를 분리한다. 읽을 수 있는 빈/손상 JSON을 유효 저장으로 단정하지 않고, 읽기 실패를 저장 없음으로 바꾸지 않는다. 조회 후 파일 변경도 다시 읽는다.
- 상태: LoadingScreen 종류를 버튼 문구와 분리하고 확인/로딩/오류의 버튼 수명과 뒤 화면 입력 차단을 확인했다. 작업자 사유는 enum으로 전달하며 복원 시 None으로 초기화한다. 알 수 없는 작업자 사유의 기본 표시와 잘못된 Loading 종류 거부도 검사했다.
- UI: 기본 글자 크기 × 현재 배율, 보통/크게 반복, 새 행·숨김/재표시, 폰트 미지정 대체 폰트, 최소 터치 크기. UiList·UiText에서 LaboratoryView 탐색을 제거했다.
- 기존 Play 회귀: Bootstrap/직접 진입·오류 복구·새 게임/이어하기·메뉴 왕복, 생산과 게임 사이클, 상태 표식 ON/OFF·재탭, 작은 버튼·스크롤·이동 미리보기·가로/세로 회전·큰 글자, 모사한 마우스/터치·포커스/정지, 멸망/결과·새 판, 영속 객체 단일성·종료 정리.

## 실제 Editor 화면·이전 저장 확인

테스트 외에 별도 Play 실행으로 이전 PLAN의 Editor 저장을 복사한 격리 슬롯을 사용했다. [원본 정보와 SHA-256](previous-save-source.json), [검증에 사용한 이전 저장](previous-session.json), [초기 진입](runtime-bootstrap.json), [실제 프레임 증가](runtime-frames.json)를 기록했다. `job wait`에서 frameCount 3257→3283, met=true를 확인하여 화면만 갱신된 정지 상태와 구분했다.

1. Bootstrap 역할 진입 → Title → 이어하기. 변경 전 버전 5 저장의 객체 ID·객체 수·Playing 상태가 복원됐고 SceneFlowManager와 EventSystem은 각각 하나였다. [복원 결과](runtime-previous-save.json).
2. 저장하고 메뉴로 돌아온 뒤 동일 격리 슬롯에 AppTestScenario의 정상 명령·시간 진행으로 운영 상태를 준비하여 이어했다. 시설 5개와 큰 글자를 사용하고 가공시설 상세·입고 대기 표식의 가로/세로 화면을 확인했다. [운영 관찰](runtime-operating.json).
3. 종료 직전 시나리오를 같은 경로로 이어하여 결과 화면의 점수 128/120·전과·새 판 버튼을 양방향에서 확인했다. [결과 상태](runtime-results.json). 종료 단계 전환·연출 중 회전과 저장 복원은 자동 Play 시나리오의 단언으로 검증했다.
4. 결과 화면 버튼의 Submit 경로로 새 판을 시작하여 세대 1→2, Playing, 점수 0, 초기 객체 2개, 저장 오류 없음까지 확인했다. [재시작](runtime-restart.json).

[screens](screens)의 14장은 Title·설정·확인·로딩·오류·생산 상세·결과 각각의 1280×720 / 720×1280 Game View 렌더 타깃이다. 실제 이미지를 열어 비율·한글·버튼·레이아웃을 확인했다. 상세 패널의 스크롤 밖 내용은 정상적인 viewport 절단이며, 입력·스크롤 동작은 기존 Play 테스트로 함께 확인했다. 검증용 준비·관찰 코드 원문은 [runtime-scripts](runtime-scripts)에 보관했다. 이 입력은 Editor API·UI 이벤트 및 모사 입력이며 실기기 손가락 조작 결과가 아니다.

종료 수정 후 별도 Play를 반복하여 [첫 진입](runtime-final-live.json)과 [재진입](runtime-repeat-live.json)의 Domain Reload OFF, 영속 객체 각각 1개를 확인하고, [Play 종료 오류/경고 0](runtime-shutdown-fixed-console.json)을 확인했다.

## 실패와 수정 이력

- [첫 Play 결과](play-on-first-result.json)는 34/50이었다. 신규 로딩 테스트가 Show 직후 입력 잠금 해제를 기다리지 않았고, 여러 Game View 중 실제 Screen을 구동하지 않는 창의 크기를 변경해 기존 해상도/입력 검사도 실패했다.
- [두 번째 실행](play-on-second-status.json)은 렌더 타깃이 요청 비율과 다른 것을 직접 확인해 [중단](play-on-second-cancel.json)했다. 통과로 계산하지 않았다.
- LaboratoryGameViewSize를 실제 주 PlayModeView에 적용하도록 수정했다. 크기/입력 단언은 완화하지 않았다. 화면 캡처의 출력 위치는 과거 PLAN 증거 폴더에서 프로젝트 Temp로 바꿔 재실행이 기존 증거를 덮어쓰지 않게 했다. 이번 첫 실행이 갱신한 PLAN-016 이미지는 HEAD 원본으로 복원하고 새 캡처만 PLAN-017에 보관했다.
- 이후 [ON 50/50](play-on-before-shutdown-fix.json)·[OFF 50/50](play-off-before-shutdown-fix.json)을 확인했다. 반복 로딩 검사는 입력 잠금이 풀린 후 이전 클릭을 전달하도록 추가 강화했다.
- 실제 Play 종료에서 LaboratoryView가 관리자보다 먼저 파괴된 경우 SceneFlowManager.OnDestroy의 null 조건부 호출이 MissingReferenceException을 발생시켰다. [종료 오류 기록](runtime-final-console.json). Unity 객체의 파괴 상태를 검사하도록 수정하고 `SceneViewDestroyedBeforePersistentOwnerCanShutDownCleanly`를 추가했다.
- 최종 OFF 실행 중 에이전트가 출력한 `Read Play Options` 진단 로그가 엄격한 로그 검사를 사용하는 종료 테스트에 들어가 1건 실패했다. [외부 로그 실패](play-off-diagnostic-log-result.json). 해당 테스트는 조건/단언을 바꾸지 않고 외부 로그 출력 없이 [1/1 통과](play-off-ending-rerun-result.json)했으며 제품 예외와 구분한다.
- [초기 도구 오류](initial-console-errors.json)는 Pipeline eval 임시 어셈블리를 조회하는 Unity QuickInstaller의 `Assembly.GetCodeBase` 예외였다. 제품 스택과 구분하여 기록했다. 실행 중 새로운 제품 오류와 테스트 실패를 이 도구 문제로 간주하지 않았다.

## 복원·정적 확인·한계

[원래 Editor 상태](original-editor-state.json)와 [복원 상태](restored-editor-state.json)를 대조했다. Laboratory 씬·미수정 상태·Play 종료·Domain Reload OFF·background=false·Default 테스트 슬롯·시작 씬 없음으로 복원했다. 이전 저장 원본의 SHA-256은 유지했으며 제품 저장은 사용하지 않았다. 일회성 캡처 에셋은 Assets 밖 프로젝트 Temp로 이동해 보관했다.

최종 [재컴파일 상태](final-compile.json)는 오류 없음이며, [Console](final-console.json)과 [반복 Play 종료](runtime-repeat-shutdown-console.json)에서 오류/경고 0을 확인했다. [정적 검사](static-audit.json)에서 GUID·meta·이전 저장·과거 증거·Editor 설정 보존을 확인했고 [문서 검사](document-validation.txt)와 git diff --check도 통과했다. APK·실제 터치·회전 센서·플랫폼 성능·종합 최적화는 이번 검증 범위가 아니다. 정식 아트 변경과 신규 Sprite 요청은 없다.

# PLAN-012 검증 증거

2026-10-28, Windows Unity 6000.3.27f1 / Android 활성 대상 / Unity CLI 1.0.0-beta.12 / Pipeline 0.9.1-exp.1.
기준 커밋은 `302a4117922ec5c37ddde5d55098527334d6765b`이며 이번 작업은 아직 커밋하지 않았다.
소스 식별은 `before-manifest.json`과 `final-source-manifest.json`의 SHA-256으로 기록한다.

## 데이터 이관

- `before-so.json`: 기존 10개 SO 원문. `baseline-data.json`: 이관 CSV 전체 행과 고정 지문.
- `field-migration-audit.json`: 사용 중인 94개 필드 일치, 역할상 미사용 21개 필드의 0/빈 값 정규화. `node docs/plans/evidence/PLAN-012/audit-migration.mjs`로 재검사한다.
- 모든 기존 자원/시설/생명체 ID와 9개 SO GUID를 유지한다. 숫자 전용 Invasion SO와 해당 스크립트만 제거했다.
- 코드 값은 session.csv로 이관: Map 16×10, 시작 (1,1), 유인/무인 배율 1/0.2, 멸망 연출 4초. 초기 유기물은 두 시설 건설비 합으로 계산한다.
- 저장 버전, 수치 오차, 프레임 처리 상한, 자동 저장 간격은 기술 정책이므로 CSV화하지 않는다.
- `meta-audit-final.json`: 기존 meta 변경·누락 검사. `capture-source.mjs`로 재생성한다.

## 실행과 결과

명령의 `--project-path`에는 저장소 아래 UnityProject의 **절대 경로**를 사용한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command script recompile --project-path $projectPath --json
unity command script recompile_status --project-path $projectPath --json
unity command test run --mode editor --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
unity command test run --mode playmode --async_tests true --project-path $projectPath --json
```

컴파일 완료와 실패 없음, 이전 테스트 완료를 각각 확인한 뒤 다음 테스트를 실행한다. start 응답의 0개는 성공 결과가 아니며 result의 최종 summary를 사용한다.

- Edit05: 235/235 통과. 침공 설정 오류 위치 테스트 1개 추가 후 **최종 Edit06: 236/236 통과**, 실패·건너뜀 0.
- Play01: 13/13 통과. Play02: Domain Reload 활성 설정에서 13/13 통과. Play03: 비활성 설정에서 13/13 통과. 각 설정은 실행 전 Editor API로 확인했으며 원래 설정으로 복원했다. 각 실행 실패·건너뜀 0.
- `domain-reload-*-observation.json`은 실제 EditorSettings getter를 읽은 기록이다. 디스크의 `play-options-*.txt`는 변경 직후 저장되지 않아 실제 설정 판정에 사용하지 않는다.
- `player-compilation.json`: Android용 스크립트 어셈블리 34개 컴파일 성공, 실제 플레이 씬 의존성에 CSV 5개 포함. 메뉴 `Tools/Mobile Game/Validate Player Data Compilation`으로 재실행한다.
- 로컬 CSV 편집 검사는 facilities.csv의 건설 시간을 실제로 편집·임포트하고 새 카탈로그/세션에서 완료 시점 변화를 관찰한 뒤 finally에서 파일을 복원한다. 기준 지문도 다시 확인한다.
- 메모리 공급자 교체, 지연/실패/취소, Registry 중복·누락, 파싱·역할·참조 오류, 구버전 기준 지문, 신규 저장 불일치, 저장 보존, 중복 초기화와 늦은 완료 차단을 자동 검사한다.
- 기존 Play 시나리오는 격리된 저장 슬롯에서 실제 씬 Bootstrap으로 초기화한다. 초기 건설·가공·배양·배정·방출·멸망·결과·재시작·두 번째 판 종료와 화면/공통 명령 상태 일치를 검증한다. `pc/`의 화면과 Snapshot은 이번 실행에서 복사하며 기존 PLAN-011 증거는 변경하지 않는다.
- `pc/plan011-run2-results.png`를 직접 확인했다. 두 번째 판 결과(128/120, 32회 활동, 외부 1개체)와 새 게임 버튼이 표시된다. 같은 이름의 JSON은 버전 5·기준 해시·Results 상태를 기록한다. 파일명은 재사용한 시나리오의 이름이며 이번 PLAN-012 실행에서 새로 생성된 파일이다.
- `final-console.json`의 현재 groundTruth는 컴파일 오류 없음·Console 오류 0·경고 0이다. `final-source-manifest.json`은 403개 파일을 식별한다. 기존 meta 변경 0, 새 파일 meta 누락 0, 의도한 제거 2개다.
- 코드/문서의 `git diff --check` 통과. 전체 diff에서는 Unity가 생성한 3개 씬의 빈 `m_Name: ` 직렬화 줄을 공백으로 보고한다. Unity 표준 출력이므로 임의 재직렬화하지 않았다.

## 실패 시도와 정정

- Edit01: 테스트 asmdef 설정 오류로 Play 테스트가 Edit에 포함되어 실패. 플랫폼 분류 수정 후 재검증.
- Edit02: NUnit Task 테스트가 Editor 문맥에서 대기하며 취소 요청도 시간 초과. 해당 프로젝트 Editor 프로세스의 무응답을 확인하고 재시작했다. Coroutine과 제한시간 검사로 전환. 완료 결과 없음; `edit-02-hang-tail.txt` 보존.
- Edit03: 234개 중 6개 실패. 저장 버전 4의 오래된 기대값을 현재 버전 및 신규 해시 검사로 수정하고, legacy fixture의 해시 없음 조건을 명시했다.
- Edit04: 컴파일 오류 상태에서 시작하여 재컴파일로 무효화된 실행을 취소했다. 통과 증거로 사용하지 않는다.
- 최종 유효 결과만 완료 판정에 사용한다. 과거 오류가 남은 CLI 이벤트 버퍼와 현재 Unity Console의 groundTruth 오류/경고 수를 구분한다.

## 한계

실제 Google Sheets API·인증·원격 배포·캐시는 구현하지 않았다. Android 스크립트 컴파일과 씬 의존성 검사는 APK 빌드·실기기 실행 증거가 아니다. 세로 대응·최종 모바일 검증은 로드맵 4단계에 남는다. 신규 Sprite 요청은 없다.

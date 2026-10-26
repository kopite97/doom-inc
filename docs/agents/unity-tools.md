# Unity 도구와 실행

적용: 설치 상태 확인, 프로젝트 준비, Editor·CLI·MCP·테스트·빌드 실행.
탐색: [작업 절차](workflow.md), [에셋](unity-assets.md), [검증](verification.md).

## 실행 전

1. 유일한 active PLAN이 해당 실행을 포함하는지 확인한다. [독립 Sprite 적용·검증](art-assets.md#sprite-requests)은 요청 README의 대상·범위를 확인한다.
2. [플레이 환경](../game-design/04-delivery/01-platform/environment.md)의 승인 버전·템플릿·대상을 확인한다.
3. 실제 프로젝트 경로와 Editor·Android 모듈·CLI·패키지 설치 여부를 확인한다.
4. 프로젝트가 있으면 ProjectVersion.txt·패키지 설정·대상 플랫폼을 읽고 승인 기준과 비교한다.
5. 기존 Editor 세션과 실행 중 작업을 확인한다. 같은 프로젝트에 충돌하는 Editor 작업을 중복 실행하지 않는다.

설치되어 있다고 추정하거나 다른 버전으로 자동 대체하지 않는다.
프로젝트 생성 전에는 실행 파일 경로·설치 완료·테스트 명령을 검증된 사실로 쓰지 않는다.

## 연결과 명령

독립 Sprite 작업에서 아래 PLAN 실행 기록은 요청 README에 남긴다. 도구 실행·증거 확인 기준은 동일하다.

- 공식 CLI·MCP를 우선 활용한다. 설치된 버전의 도움말과 공식 문서로 실제 제공 기능을 확인한다.
- MCP 연결 대상·프로젝트가 맞는지 확인하고, 변경 후 임포트·컴파일 완료를 기다려 결과를 읽는다.
- 도구가 지원하지 않는 작업은 Editor의 지원 API·명령줄·직접 조작 중 재현 가능한 경로를 사용한다.
- 사용할 수 없는 연결 기능은 PLAN에 기록한다. 대체 경로 성공을 MCP 연결 성공으로 표시하지 않는다.
- 씬·프리팹·SO 변경은 [에셋 지침](unity-assets.md)을 따른다. 도구 호출 성공과 저장·검증 성공을 구분한다.
- 환경 구성·버전·패키지 선택이 다른 작업에 영향을 주면 [기술 결정](../decisions/README.md)에 기록한다.

## 테스트·빌드 실행과 증거

- 실제 설치한 Test Framework의 실행 옵션을 확인하고 검증된 명령 또는 스크립트를 사용한다.
- 실행마다 대상 프로젝트·Editor 버전·플랫폼·필터·결과 경로를 명시한다.
- 실행 식별자별 로그·결과 경로를 사용해 이전 실행 결과를 새 성공으로 읽지 않는다.
- 프로세스 종료와 결과 파일·실행 테스트 수·실패·오류를 함께 확인한다.
- 타임아웃·라이선스·패키지 해결·컴파일 실패와 게임 규칙 검증 실패를 구분한다.
- 그래픽 출력 검증에 필요한 환경을 확인한다. 화면을 그리지 않는 실행만으로 UI·렌더링을 판정하지 않는다.
- 장시간 실행은 기존 작업의 상태를 추적하고, 결과가 안 보인다는 이유로 중복 실행하지 않는다.
- 검증의 적용 범위와 완료 판정은 [검증 지침](verification.md)을 따른다.

## 재현 가능한 실행법 유지

- 처음 성공한 실행 방법은 PLAN에 환경·명령·결과를 기록하고 재사용할 스크립트가 생기면 여기서 연결한다.
- 긴 명령·설치 절차가 늘면 별도 지침으로 분리하고 루트 MAP에 추가한다.
- 개인 PC 절대 경로·기기 식별값은 공용 실행법에 하드코딩하지 않고 필요한 입력으로 받는다.
- 비밀값·서명 키를 지침이나 PLAN에 기록하지 않는다.

현재 문서 체계 검사 명령은 저장소 루트의 `node scripts/validate-agent-docs.mjs`다.
이는 문서·PLAN 검사이며 Unity 컴파일·테스트·빌드 실행을 대신하지 않는다.
확인된 설치·Editor 실행·MCP 연결·최소 테스트와 적용 한계는 [PLAN-002](../plans/PLAN-002-unity-environment.md)를 참조한다. Android 빌드·실기기 검증은 후속 작업이다.

## 현재 프로젝트 열기와 CLI 제어

작업공간 루트에서 실행한다. Unity 프로젝트는 `UnityProject/`이며 [고정 버전](../../UnityProject/ProjectSettings/ProjectVersion.txt)과 [패키지](../../UnityProject/Packages/manifest.json)를 먼저 확인한다.
이미 열린 프로젝트라면 `open`을 반복하지 않고 연결 상태부터 확인한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity open $projectPath --editor-version 6000.3.27f1 --build-target Android
unity command editor status --project-path $projectPath --json
unity command editor.playmode start --project-path $projectPath --json
unity command editor status --project-path $projectPath --json
unity command editor.playmode stop --project-path $projectPath --json
unity command console status --project-path $projectPath --json
```

- 그룹과 하위 명령은 별도 인수로 전달한다. `editor status` 전체를 하나의 따옴표 문자열로 전달하면 이 CLI에서 실패한다.
- 현재 연결은 CLI 1.0.0-beta.12·Pipeline 0.9.1-exp.1로 확인했다. 새 기능은 연결된 서버의 명령 목록과 인수를 먼저 조회한다.
- 비활성 창에서도 프레임을 관찰해야 하면 `editor.playmode set_run_in_background --enable true`를 사용하고, 종료 시 `--enable false`로 원래 값을 복원한다. 두 명령에도 같은 `--project-path`를 지정한다.
- Play 상태만 보지 말고 두 시점의 프레임·게임 시간 증가와 콘솔 오류를 확인한다. 검증을 마치면 Play를 종료한다.
- 초기 로그를 보존한 뒤 관찰 구간을 나눈다. 콘솔을 비웠다는 사실을 오류 원인 해결로 해석하지 않는다.
- 이 조합에서 Pipeline의 임시 C# 평가 어셈블리를 Editor QuickInstaller가 읽다 예외를 낸 사례가 있다. 가능한 작업은 전용 명령으로 처리하고, 재발 시 원본을 남긴 뒤 [PLAN-002 진단](../plans/PLAN-002-unity-environment.md)을 참조한다. 도메인 재로드는 임시 상태 정리이며 근본 해결로 간주하지 않는다.

## 현재 프로젝트의 최소 테스트

열린 Editor의 Pipeline 연결을 사용한다. 같은 프로젝트에 배치 Editor를 추가로 실행하지 않는다.
코드를 추가한 뒤에는 `script recompile`을 요청하고 `script recompile_status`의 완료·실패 여부부터 확인한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command test list --mode all --project-path $projectPath --json
unity command test run --mode editor --filter ProjectEnvironmentTests --project-path $projectPath --json
unity command test run --mode playmode --filter SampleSceneExecutionTests --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
```

- 위 필터는 [Edit Mode 환경 검사](../../UnityProject/Assets/Scripts/Tests/EditMode/Environment/ProjectEnvironmentTests.cs)와 [Play Mode 실행 검사](../../UnityProject/Assets/Scripts/Tests/PlayMode/Environment/SampleSceneExecutionTests.cs)를 대상으로 한다. 게임 규칙 검증은 해당 구현과 함께 추가한다.
- 테스트가 발견됐는지 확인하고, 동기 실행의 `data.result.Summary`, 비동기 완료의 `data.result.summary`와 개별 결과를 저장한다. `running` 응답의 0개는 완료 결과가 아니다.
- 비동기 실행은 `test status`가 완료될 때까지 확인한다. 새 테스트 실행은 이전 실행의 완료 후에 한다.
- 원본 JSON을 실행별 증거 경로로 보관한다. Unity가 출력한 XML은 콘솔의 실제 저장 위치를 확인해 다음 실행 전에 복사한다. 결과 XML의 기본 위치는 덮어써질 수 있다.
- 검증 후 Editor의 Play 종료·컴파일 완료와 콘솔을 확인한다. 테스트 결과 이후 발생한 Editor 오류도 별도 기록한다.

## 정의·Thing 기반 검증

대표 정의가 없을 때만 Editor 메뉴로 생성한다. 기존 정의가 기대값과 다르면 중단하므로 값을 임의로 덮어쓰지 않는다.

```powershell
unity command editor invoke_menu --path 'Tools/Mobile Game/Create Foundation Definitions' --project-path $projectPath --json
unity command test run --mode editor --filter MobileGame.Foundation.EditMode.Tests --filter_type assembly --project-path $projectPath --json
```

`$projectPath`는 위와 같이 작업공간의 `UnityProject` 절대 경로다. 검사 범위와 증거는 [PLAN-003](../plans/PLAN-003-thing-foundation.md), 코드 경계는 [TECH-001](../decisions/TECH-001-thing-foundation.md)을 참조한다.

## 공간·공통 조작 기반 검증

제공된 검증 장면은 실제 게임 시작 구성이 아니다. 아래 명령은 기존 Editor에서 실행하며 `$projectPath`는 위와 같다.

```powershell
unity command editor invoke_menu --path 'Tools/Mobile Game/Open Map Foundation Scene' --project-path $projectPath --json
unity command test run --mode editor --project-path $projectPath --json
unity command test run --mode playmode --filter MapFoundationViewTests --async_tests true --project-path $projectPath --json
unity command test status --project-path $projectPath --json
```

수동 조작은 해당 씬을 열고 Play에서 시설 선택 → Move / Rotate → 격자 클릭·Rotate → Confirm/Cancel 순서다.
장면이 없을 때만 `Tools/Mobile Game/Create Map Foundation Scene` 메뉴를 실행한다. 기존 장면은 덮어쓰지 않는다.
Play Mode 검사는 실제 씬·공통 명령·OnGUI 렌더링과 Temp의 화면 캡처를 확인한다. 마우스 이벤트 주입이나 실기기 터치 검사와는 구분한다. 결과·제한은 [PLAN-004](../plans/PLAN-004-map-and-commands.md)에 있다.

GameFlow·저장 진단 씬의 생성/열기 메뉴와 전체 Edit·Play 실행법은 [PLAN-007 검증 기록](../plans/evidence/PLAN-007/README.md#재현-환경과-명령)을 따른다. 테스트 슬롯은 실제 진단 슬롯과 격리한다.

실제 초기 건설·수집·저장 장면은 [PLAN-008 검증 기록](../plans/evidence/PLAN-008/README.md#재현-명령과-직접-실행)의 Open Laboratory Scene 메뉴·조작·Edit/Play 명령을 따른다. 기존 진단 장면과 실제 플레이 슬롯을 구분한다.

## Codex의 Unity MCP 연결

`unity-mobile-game`은 이 PC의 Codex 사용자 설정에 등록했으며 프로젝트 경로를 고정했다.
다른 PC에서는 실행 파일 경로와 프로젝트 경로를 확인한 뒤 아래와 같이 등록한다. 기존 연결은 먼저 `get`으로 확인한다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
$unityCli = (Get-Command unity -ErrorAction Stop).Source
codex mcp get unity-mobile-game --json
codex mcp add unity-mobile-game -- $unityCli mcp --project-path $projectPath --no-banner --non-interactive
```

- 등록과 실제 연결은 구분한다. Codex 클라이언트의 도구 검색과 MCP의 프로젝트 정보 조회 결과는 [연동 증거](../plans/evidence/PLAN-002/2026-10-26-mcp-verification.json)에 있다.
- [연결 확인 스크립트](../../scripts/verify-unity-mcp.mjs)는 `codex|unity`, 실행 파일, 프로젝트, 결과 JSON 경로를 인수로 받는다. `unity` 모드의 마지막 선택 인수는 실제 검색한 도구의 호출 JSON이다.
- 도구 이름만 보고 의미를 추정하지 말고 해당 연결에서 반환한 설명과 입력 스키마를 확인한다. 그룹 구분이 필요한 작업은 검증된 CLI 명령을 사용한다.
- 이미 실행 중인 채팅의 도구 목록 자동 갱신은 검증하지 않았다. 도구가 보이지 않으면 Codex에서 연결을 다시 로드한 뒤 목록을 확인하고, 그동안 CLI 연결을 사용할 수 있다.
- 연결 확인용 MCP 호출은 게임 하네스 검증을 대신하지 않는다. 사용자 설정 전체나 토큰을 증거에 복사하지 않는다.

## 공식 참고

- [Unity CLI와 MCP](https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli)
- [Unity Pipeline 요구사항](https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package)
- [Unity Test Framework 명령줄 안내](https://docs.unity3d.com/Packages/com.unity.test-framework@1.4/manual/reference-command-line.html)
- [Codex MCP 연결 안내](https://developers.openai.com/codex/mcp/)

문서의 버전 예시를 패키지 버전 확정으로 해석하지 않는다. 실제 사용 버전의 안내와 실행 결과를 확인한다.

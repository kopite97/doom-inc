---
id: PLAN-002
status: completed
title: Unity 개발 환경과 실행·검증 경로 준비
updated: 2026-10-26
---

# PLAN-002 Unity 개발 환경과 실행·검증 경로 준비

## 목표와 범위

승인된 환경으로 Unity 프로젝트를 열고 에이전트가 실행·검증할 실제 경로를 확인한다.

- 근거: [플레이 환경](../game-design/04-delivery/01-platform/environment.md)의 D-081·D-087·D-088·D-089와 [로드맵](../game-design/05-development/02-implementation/roadmap.md).
- 포함: 설치 상태 확인, 필요한 환경 구성, 프로젝트 생성, CLI·MCP 연결, 최소 실행·검증 경로 확인.
- 제외: Thing·생산·운반 등 게임 기능 구현, 후속 플랫폼·스토어 배포, 엔진 버전 변경.
- 선행: [PLAN-001](PLAN-001-agent-guidance.md)의 지침 체계 정비.
- 적용 지침: [작업 절차](../agents/workflow.md), [Unity 도구](../agents/unity-tools.md), [에셋](../agents/unity-assets.md), [검증](../agents/verification.md).

## 작업 목록

- [x] 실제 Unity·Android 모듈·CLI 및 C# 개발 도구 설치 상태와 현재 작업공간 확인
- [x] Unity 6000.3.27f1과 Android Build Support·SDK·NDK·OpenJDK 설치 및 도구 확인
- [x] 프로젝트 생성 위치와 필요한 환경 구성 방법 확정
- [x] 승인된 엔진·템플릿·대상으로 필요한 환경과 프로젝트 구성
- [x] 공식 CLI·Pipeline의 실제 프로젝트 연결과 상태 조회·Play Mode 제어 확인
- [x] MCP 클라이언트의 실제 연결 확인
- [x] 기본 프로젝트의 임포트·컴파일·Play Mode 시작과 종료 확인
- [x] 최소 테스트를 실제로 실행해 결과·로그 수집 경로 확인
- [x] 확인된 프로젝트 열기·CLI 제어 절차를 지침에 연결하고 결과를 이 PLAN에 기록

## 완료 기준과 검증 방법

| 기준 | 필요한 증거 |
| --- | --- |
| 승인된 버전·템플릿·대상과 실제 구성이 일치 | 프로젝트 설정·사용 패키지·실행 환경 확인 |
| 실제 프로젝트가 임포트·컴파일되고 실행됨 | 실행 방법과 오류 확인 결과 |
| 공식 연동으로 대상 프로젝트의 상태를 확인할 수 있음 | 연결 대상·버전·실행 결과 |
| 테스트 실행 및 성공·실패를 구분할 수 있음 | 실제 실행한 최소 테스트와 결과 파일·로그 |

도구 설치나 에디터 실행만으로 선행 기반 3묶음이 구현됐다고 판단하지 않는다.
게임 하네스는 로드맵의 선행 기반부터 구현하며, 이 PLAN의 최소 테스트는 실행 경로 확인용이다.

## 현재 상황과 다음 행동

- 현재: `UnityProject/`의 Universal 2D Android 환경 구성, CLI/Pipeline·MCP 연결, Editor 실행 및 최소 자동 테스트를 완료했다. 에디터는 프로젝트를 연 채 `ready`·`stopped`, `compiling=false`, `runInBackground=false` 상태다.
- 준비됨: 지정 Editor, SDK Build Tools 36.0.0·Platform Tools 37.0.1·Command-line Tools 16.0·플랫폼 34/35/36/37.0, NDK r27c(27.2.12479018), OpenJDK 17.0.18+8, CMake 3.22.1. 기존 Hub·CLI·C# 개발 도구는 [최초 조사](evidence/PLAN-002/2026-10-26-environment.json)를 참조한다.
- 다음 행동: 승인된 로드맵의 첫 기반 묶음을 [PLAN-003](PLAN-003-thing-foundation.md)에서 진행한다. 현재 작업 대상으로 전환했으며 게임 기반 구현은 아직 착수하지 않았다.
- 미검증: Android APK/AAB 빌드·실기기 동작, 게임 시스템·하네스. 현재 채팅 도구 목록의 자동 갱신도 확인하지 않았으며 Codex MCP 클라이언트 연결 검증과 구분한다.
- 남은 환경 주의점: 첫 임포트에서 Windows 코드 무결성 정책에 따른 Burst DLL 로드 경고 11건과 URP 셰이더 변환 경고 18건을 관찰했다. 원본 메시지를 보존했으며 보안 정책과 패키지 코드는 변경하지 않았다. 이번 Play Mode 구간의 오류·경고는 0건이지만 Burst 최적화와 Android 빌드까지 검증한 것은 아니다.
- 도구 호환 주의점: 테스트 완료 후 QuickInstaller가 임시 `PipelineEval_*` 어셈블리를 읽는 과정에서 문자열 변환 예외 1건이 발생했다. 도메인 재로드 후 CLI·MCP 연결과 준비 상태를 확인했으며 새 오류는 관찰하지 않았다. 기존 오류는 콘솔과 증거에 보존했다. 근본 원인 해결이나 재발 방지를 완료했다고 판정하지 않는다.
- 실행 환경: ADB의 최초 버전 조회는 샌드박스에서 사용자 `.android` 폴더를 생성하지 못해 실패했다. 실제 사용자 환경에서 재실행하여 종료 코드 0과 버전 출력을 확인했다. 최초 CLI 상태 조회의 프로세스 접근 제한도 별도 기록으로 보존한다.

## 확인된 설치와 점검 방법

- 설치 위치: `C:\Program Files\Unity\Hub\Editor\6000.3.27f1`. 기존 두 Editor는 유지했다.
- 공식 Unity CLI 1.0.0-beta.12에서 아래 명령을 실행했다. 설치에는 네트워크·설치 폴더 접근이 가능한 환경을 사용했다.

```powershell
unity install 6000.3.27f1 --architecture x86_64 --module android --child-modules --yes --accept-eula --no-wait --json --non-interactive --no-banner
unity editors --installed --json --non-interactive --no-banner
unity editors verify 6000.3.27f1 --json --non-interactive --no-banner
```

- 설치 명령은 종료 코드 0, 에디터와 하위 모듈 12개를 `installed`로 보고했다. 구조 검사는 13개 구성 요소 모두 `ok`였다.
- `Unity.exe -version` 및 Java·javac·ADB·Clang·SDK Manager·AAPT2·CMake 버전 조회를 실행했다. 경로·인수·출력·종료 코드는 [설치 검증 증거](evidence/PLAN-002/2026-10-26-installation-verification.json)에 기록했다.
- SDK Manager 점검에는 해당 프로세스의 `JAVA_HOME`만 번들 OpenJDK로 지정하고 복원했다. 사용자·시스템 환경 변수는 변경하지 않았다. SDK 플랫폼별 `android.jar`도 확인했다.

## 프로젝트 구성과 실행 결과

- 프로젝트 루트: `UnityProject/`. 문서와 지침은 기존 작업공간 루트에 유지한다. [캐시 제외 규칙](../../UnityProject/.gitignore)으로 Library·Temp·Logs·UserSettings와 생성된 IDE 파일을 소스 관리 대상에서 제외한다.
- 생성: 공식 CLI `projects create`, 지정 에디터·`com.unity.template.universal-2d`·`--with-pipeline --no-cloud` 사용. Unity Cloud 프로젝트는 연결하지 않았다.
- 템플릿: 에디터에 포함된 Universal 2D 6.1.6. 에디터가 패키지를 호환 버전으로 정리했으며 실제 [manifest](../../UnityProject/Packages/manifest.json)와 [lock](../../UnityProject/Packages/packages-lock.json)은 URP 17.3.0·Pipeline 0.9.1-exp.1·Test Framework 1.6.0을 포함한다.
- 대상과 씬: 실행 중 `activeBuildTarget=Android`, 기본 씬 `Assets/Scenes/SampleScene.unity`, URP 에셋과 `Renderer2DData` 연결을 확인했다. 기본 카메라와 2D 조명만 있는 빈 씬이며 게임 기능은 추가하지 않았다.
- Android Player Settings는 템플릿 기본값인 ARM64·IL2CPP, 최소 API 25·목표 API 자동, 가로·세로 자동 회전을 유지했다. 배포·성능 최종값으로 확정한 것은 아니다.
- 실행: Play Mode 두 표본에서 프레임 14,502 → 364,490, 게임 시간 약 25.82 → 503.41초로 증가했다. URP 실행과 [960×540 Game View](evidence/PLAN-002/2026-10-26-game-view.png)를 확인했다. 이 관찰값은 성능 측정 결과가 아니다.
- 검증용 Run In Background override를 해제하고 원래 값 `false`로 복원했다. 종료 후 `compiling=false`, `playMode=stopped`, 콘솔 오류·경고 0건을 확인했다.
- 초기 명령 구문 오류 2건은 그룹과 하위 명령을 별도 인수로 전달하여 해결했다. 초기 메시지를 증거로 저장한 뒤 콘솔을 비워 Play 구간을 따로 관찰했다. 재현 방법은 [Unity 도구 지침](../agents/unity-tools.md), 전체 결과는 [프로젝트 검증 증거](evidence/PLAN-002/2026-10-26-project-verification.json)를 참조한다.

## 검증 결과

| 날짜 | 대상·방법 | 결과와 증거 |
| --- | --- | --- |
| 2026-10-26 | Unity CLI 설치 목록·버전, 실행 파일·등록 정보, Android 도구 경로, vswhere, 개발 도구 버전 조회 | 읽기 전용 조사 완료. [환경 확인 결과](evidence/PLAN-002/2026-10-26-environment.json)에 버전·경로·명령·한계 기록. |
| 2026-10-26 | `unity status --json --non-interactive --no-banner` | `STATUS_NO_INSTANCES` 및 실행 프로세스 조회 경고. 연결 성공으로 처리하지 않음. |
| 2026-10-26 | `node scripts/validate-agent-docs.mjs`, 증거 JSON 파싱, 메모리 입력으로 목록 검사 검증 | 지침 줄 수·단일 active·README status 일치·로컬 파일 링크·JSON 형식 통과. 목록 정상 허용 및 상태 불일치·행 누락·중복 거부 확인. |
| 2026-10-26 | 공식 CLI로 지정 Editor와 Android 하위 모듈 설치 | 종료 코드 0, 모든 설치 항목 성공. [설치 로그](evidence/PLAN-002/2026-10-26-unity-install.log). 앞부분의 `--resume` 호출은 재개할 다운로드가 없어 아무 작업도 하지 않았으며, 이후 신규 설치 명령으로 완료했다. |
| 2026-10-26 | 설치 등록·구조 검사, Editor와 Android 도구 버전 조회, SDK 플랫폼 파일 확인 | Editor `6000.3.27f1`, 구조 13개와 도구 7종 확인 완료. ADB는 샌드박스 실패 후 사용자 환경에서 통과. [통합 증거](evidence/PLAN-002/2026-10-26-installation-verification.json), [구조 검사 원본](evidence/PLAN-002/2026-10-26-unity-verify.log). 프로젝트 빌드·테스트는 미실행. |
| 2026-10-26 | 설치 결과 반영 후 문서 검사·증거 JSON 파싱 및 핵심 결과 대조 | 줄 수 제한, 유일한 active PLAN-002, 목록 status, 로컬 파일 링크와 증거 JSON 5개 확인 통과. [문서 검사 로그](evidence/PLAN-002/2026-10-26-post-install-doc-validation.log). |
| 2026-10-26 | 공식 CLI 프로젝트 생성·Android 대상 열기, 에디터 내부 설정 조회 | 생성 명령 성공, 지정 버전과 Android·URP 2D 구성 확인. [생성 로그](evidence/PLAN-002/2026-10-26-project-create.log), [실행 중 설정](evidence/PLAN-002/2026-10-26-project-runtime-config.json). |
| 2026-10-26 | CLI/Pipeline Play 시작·두 시점 조회·Game View 캡처·종료·설정 복원 | 프레임과 게임 시간 증가, Play 구간 및 종료 후 콘솔 오류·경고 0건. [통합 증거](evidence/PLAN-002/2026-10-26-project-verification.json), [종료 상태](evidence/PLAN-002/2026-10-26-editor-after-play.json). 초기 경고와 수정한 명령 오류는 [별도 보존](evidence/PLAN-002/2026-10-26-console-initial-entries.json). NUnit 테스트 실행으로 표시하지 않음. |
| 2026-10-26 | Codex 사용자 MCP 등록·Codex app-server 클라이언트의 도구 검색·stdio MCP 호출 | `unity-mobile-game`에서 도구 110개 확인, 실제 프로젝트 경로·6000.3.27f1·Android 조회 성공. [연동 증거](evidence/PLAN-002/2026-10-26-mcp-verification.json). 새 작업/LLM 턴 없이 별도 클라이언트로 확인했다. |
| 2026-10-26 | 기존 Editor의 Test Framework 1.6.0으로 환경 검사 실행 | Edit Mode 1/1, Play Mode 1/1 통과. 실패·건너뜀·불확정 0. [통합 증거](evidence/PLAN-002/2026-10-26-tests-verification.json), [Edit Mode 결과](evidence/PLAN-002/2026-10-26-editmode-tests.json), [Play Mode 결과](evidence/PLAN-002/2026-10-26-playmode-tests-status.json), [실제 XML](evidence/PLAN-002/2026-10-26-playmode-tests.xml). |
| 2026-10-26 | 테스트 이후 Editor 예외 진단·공식 API로 도메인 재로드·일반 MCP 씬 조회 | `Assembly.GetName()` 실패가 메모리 내 PipelineEval 어셈블리 2개에서만 재현됨. [진단](evidence/PLAN-002/2026-10-26-quickinstall-assembly-diagnostic.json), [재로드 로그](evidence/PLAN-002/2026-10-26-domain-reload.log). 재로드 후 [씬 조회](evidence/PLAN-002/2026-10-26-unity-mcp-after-reload.json) 성공, [Editor 준비 상태](evidence/PLAN-002/2026-10-26-editor-final.json), [기존 예외 보존·새 오류 미관찰](evidence/PLAN-002/2026-10-26-console-final.json). |
| 2026-10-26 | 완료 기록·다음 PLAN 전환 후 문서와 증거 정합성 검사 | 줄 수·단일 active PLAN-003·목록 status·로컬 링크 통과. JSON 55개 파싱, 테스트 소스·메타 8개 해시, 실제 결과·MCP·최종 씬 상태 대조 통과. [문서 검사](evidence/PLAN-002/2026-10-26-final-doc-validation.log), [증거 대조](evidence/PLAN-002/2026-10-26-final-evidence-audit.json). |

최초 환경 조사는 설치·설정 변경 없이 수행했다. 이후 사용자가 지정 버전과 Android 도구 설치를 요청했으며, 설치와 게임 테스트의 완료 여부는 따로 판정한다.

## 연동·테스트와 완료 판정

- MCP 설정은 이 PC의 Codex 사용자 설정에 등록했다. 실행 경로와 프로젝트를 고정하며 [등록 결과](evidence/PLAN-002/2026-10-26-codex-mcp-config.json)에 대상만 기록했다. 다른 MCP 연결은 변경하지 않았다.
- Codex 클라이언트의 도구 검색과 별도 stdio MCP 클라이언트의 실제 호출을 구분해 검증했다. 재사용 절차와 [검증 스크립트](../../scripts/verify-unity-mcp.mjs)는 [Unity 도구 지침](../agents/unity-tools.md)에 연결했다.
- 최소 테스트는 승인된 Editor·Android·URP 2D 구성과 실제 씬·카메라·렌더러·프레임/시간 진행을 확인한다. 게임 규칙 구현이나 실제 Android 실행을 검사한 것은 아니다.
- 최초 테스트 컴파일의 `Renderer2DData` 참조 누락은 Edit Mode asmdef에 URP의 별도 2D Runtime 어셈블리 참조를 추가하여 해결했다. Unity 생성 `.meta`와 소스 해시를 증거에 남겼다.
- Edit Mode는 실제 JSON 결과를 보관했다. 기본 XML을 다음 Play Mode 실행이 덮어썼으므로 XML 확보만을 위한 재실행은 하지 않았다.
- QuickInstaller 예외는 테스트 결과 이후 발생했고 컴파일·테스트·일반 CLI/MCP 연결을 차단하지 않았다. 한글 프로젝트 경로 문제로 단정할 근거는 없으며 보안 정책·패키지 코드·프로젝트 위치를 변경하지 않았다. 재발 시 지원 명령을 우선 사용하고 새 후속 작업에서 도구 호환성을 다룬다.
- 이 PLAN의 환경·연동·최소 실행 경로 완료 기준을 실제 충족했다. 선행 기반 3묶음과 게임 프로토타입 완료를 의미하지 않는다.

## 관련 결정과 상태 전환

- 별도 기술 결정 없음. 위임된 주요 기술 선택이 생기면 [기술 결정 MAP](../decisions/README.md)에 연결한다.
- 2026-10-26: 문서 체계 정비 다음 작업으로 proposed 등록.
- 2026-10-26: PLAN-001 완료에 따라 proposed → active. 현재 작업 대상으로 지정했으며 환경 구성 실행은 미착수.
- 2026-10-26: 사용자 요청으로 설치 상태 조사 착수·완료. 후속 환경 구성이 남아 active 유지.
- 2026-10-26: 사용자 요청에 따라 지정 에디터와 Android 도구 설치 착수. active 유지.
- 2026-10-26: 지정 에디터와 Android 도구 설치·실행 점검 완료. 프로젝트 구성과 연동·최소 검증이 남아 active 유지.
- 2026-10-26: Universal 2D Android 프로젝트 생성·CLI 연결·Editor 실행 확인 완료. MCP 클라이언트 연결과 최소 자동 테스트가 남아 active 유지.
- 2026-10-26: 사용자가 남은 작업 진행을 허용하여 MCP 연결과 최소 테스트를 완료했다. 도구 호환 주의점을 보존하고 active → completed. 승인된 순서의 PLAN-003을 다음 작업 대상으로 함께 활성화했으며 해당 구현은 미착수.

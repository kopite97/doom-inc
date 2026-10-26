# 플레이 환경

- 문서 ID: GD-PLATFORM
- 분류: 제공 범위 > 플랫폼 > 플레이 환경
- 상태: Unity·Android 프로토타입·연동 방향·실행·저장 규칙 확정 / 환경·연동·최소 테스트 확인, 전체 게임 사이클 구현 전
- 검색어: 모바일, Android, 안드로이드, Apple, iOS, App Store, ONE store, 원스토어, Universal 2D, URP, 렌더링, 가로, 세로, PC, 실제 기기, 터치, 성능, 오프라인, 1인, 자동 저장, 저장 슬롯, 이어하기, 다시 시작, 종료 중 진행, Unity, ScriptableObject, CLI, MCP, Pipeline, 에이전트, 하네스
- 근거: S1-B / D-006, S2 / D-059·D-068·D-069·D-070·D-078·D-079·D-081·D-087·D-088·D-089·D-090 — [출처](../../00-management/04-evidence/sources.md), [결정 기록](../../00-management/02-decisions/record.md)
- 갱신일: 2026-10-26
- 탐색: [기획 MAP](../../README.md), [구현 계획](../../05-development/02-implementation/roadmap.md)

## 개발 환경 (D-081·D-087·D-088·D-089)

| 항목 | 확정 기준 |
| --- | --- |
| 엔진·에디터 | Unity 6.3 LTS `6000.3.27f1` |
| 프로젝트 템플릿 | Universal 2D(URP) |
| 표현 방향 | 기존 2D 도트·탑다운(D-006) |
| SO | Unity의 ScriptableObject |
| 프로토타입 대상 | 모바일 Android |
| 에이전트 연동 | 공식 Unity CLI·MCP |
| 반복 검증 | 단계별 하네스와 PC 플레이 검증, 완료 전 실제 Android 기기 검증 |

Unity 버전은 2026-10-26 기준 최신 LTS 계열의 정식 패치로 선택했다. [공식 릴리스 노트](https://unity.com/releases/editor/whats-new/6000.3.27f1)의 출시일은 2026-10-20이며, [공식 아카이브](https://unity.com/releases/editor/archive)와 [LTS 지원 안내](https://unity.com/releases/unity-6/support)를 선정 근거로 확인했다. 6.3 LTS 지원 기간은 2027년 12월까지로 안내되어 있다. 선정 당시 확인한 정보이며 실제 설치·호환성 검증과 구분한다.

Apple(iOS/App Store)과 ONE store 배포도 후속 계획에 포함한다. ONE store는 Android의 배포 채널이다. 후속 배포의 완료는 프로토타입 완료 조건이 아니며, 일정·순서·스토어별 절차는 아직 정하지 않았다.

## 공식 CLI·MCP와 하네스 (D-079·D-088)

- 공식 CLI·MCP를 에이전트와 Unity 개발 환경의 연결 경로로 활용한다.
- 플레이 화면과 하네스는 같은 명령 처리·규칙 검사·상태 조회를 사용한다. 공통 게임 규칙과 상태의 책임은 Thing·컴포넌트·담당 시스템에 둔다.
- 선행 기반부터 기능 추가 단계마다 하네스와 저장·복원 검증을 함께 확장한다. 단계와 통과 기준은 [구현 계획](../../05-development/02-implementation/roadmap.md)을 따른다.
- 실제 모바일 기기의 터치·성능 검증도 수행한다. 개발 도구의 연동과 인터넷 없이 혼자 플레이하는 게임 규칙은 구분한다.

환경 구성 시 [공식 MCP 안내](https://docs.unity.com/en-us/unity-cli/replace-mcp-server-unity-cli)와 [Pipeline 요구사항](https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package)을 참고한다. 조사 당시 공식 연결 경로는 Unity CLI와 Pipeline이다.

설치·프로젝트 생성·CLI/Pipeline·MCP 연결과 Editor 실행·최소 자동 테스트 결과는 [PLAN-002](../../../plans/PLAN-002-unity-environment.md)에 기록했다. 알려진 도구 호환 주의점과 Android 빌드·실기기 등 미검증 범위도 해당 PLAN에서 확인한다. 별도 에이전트 플러그인이나 유료 Assistant 도입은 포함하지 않는다.

## 실행과 기기 검증 (D-068·D-070·D-089·D-090)

- 개발 중에는 PC에서 게임 사이클을 반복 검증한다.
- 프로토타입은 **인터넷 연결 없이 혼자 플레이**한다.
- 완료 전 실제 Android 기기 한 종류에서 가로·세로 모두의 표시·터치 조작·성능을 확인한다. 진행을 막는 오류나 심각한 성능 문제가 없어야 한다.
- 가로 화면에서 기본 게임 사이클을 먼저 완성한 뒤 같은 기능과 화면 요소를 재배치해 세로 대응을 추가한다. 화면 범위는 [화면과 조작](../../03-player-experience/03-interface/controls.md), 구현 순서는 [구현 계획](../../05-development/02-implementation/roadmap.md)을 따른다.

최종 판정은 [프로토타입 완료 기준](../02-release/scope.md)의 네 가지 기준을 사용한다. 기본 프로젝트의 Editor 실행·CLI 연결 확인과 게임 시스템·하네스 구현, Android 빌드·실기기 검증을 구분한다.

## 게임 진행 저장과 이어하기 (D-069·D-078)

- 기기 내 저장 슬롯 1개에 게임 진행을 자동 저장한다.
- 다시 실행하면 마지막 저장 상태에서 이어한다.
- 게임이 꺼진 동안에는 진행하지 않는다. 생산·침공 성과도 추가로 진행하거나 누적하지 않는다.
- ‘다시 시작’을 선택하면 저장을 [초기 상태](../02-release/scope.md)로 바꾼다.

Thing의 고유 식별자와 개별 상태·연결 관계를 함께 저장·복원한다. 배정·운반·보관처·예약 관계와 생산 회차·외부 활동 상태가 기존 규칙에 맞게 이어져야 한다. 상태 소유와 복원 원칙은 [Thing과 정의 데이터](../../05-development/01-architecture/thing-model.md), 시작·이어하기·다시 시작의 전환 책임은 [GameFlow](../../05-development/01-architecture/game-flow.md)에서 관리한다.

## 개발에서 정할 항목 (D-059)

패키지 버전·구성, 최소 Android 버전·목표 API·검증 기종·해상도·배포 형식, 세부 렌더링 설정, 화면 비율·방향 전환 처리, 명령·API·테스트 도구·검증 구성, 저장 주기·파일 형식·복원 절차, 검증 반복 횟수·성능 수치는 개발 준비와 구현 과정에서 정한다.

위 항목은 사전 기획 완료를 막지 않는다. 환경 구성 결과와 검증 증거는 해당 [작업 PLAN](../../../plans/README.md)에, 주요 기술 선택은 [기술 결정](../../../decisions/README.md)에 남긴다. 전체 단계의 기준은 [구현 계획](../../05-development/02-implementation/roadmap.md)을 따른다.

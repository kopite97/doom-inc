---
id: PLAN-003
status: completed
title: SO 정의와 Thing·컴포넌트 기반
updated: 2026-10-26
---

# PLAN-003 SO 정의와 Thing·컴포넌트 기반

## 목표

같은 SO 정의를 공유하는 Thing들이 독립된 실행 상태와 소유 컴포넌트를 가지며, 필요한 기능을 조회할 수 있게 한다.
승인된 로드맵 0단계의 첫 묶음인 **정의·Thing 기반**에 한정한다.

## 범위

- 포함: 공통 SO 기반과 자원·시설·생명체 정의 분류, 대표 정의에 필요한 최소 데이터.
- 포함: 일반 C# Thing의 정의 참조·개별 ID·최초 생성 순서·존재 상태·소속 관계.
- 포함: 정의 설정에 따른 컴포넌트 생성·소유 연결·조회와 개체별 상태 독립.
- 포함: 자원 묶음의 단일 소속을 확인할 최소 실행 상태와 실제 구현 객체를 사용하는 하네스 검증.
- 제외: Map 배치·공통 화면 명령, GameFlow·파일 저장/복원, 건설·생산·운반·예약·Job·침공 구현.
- 근거: D-072·D-073·D-075·D-078·D-080·D-082~D-086. ID와 관계의 저장 요구는 고려하되 이번 작업에서 저장 완료로 판정하지 않는다.

## 기준 문서와 선행 조건

- 선행: [PLAN-002](PLAN-002-unity-environment.md)의 Unity 환경·연동·최소 테스트 완료.
- 기획: [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md), [Thing과 정의 데이터](../game-design/05-development/01-architecture/thing-model.md), [최소 콘텐츠](../game-design/04-delivery/02-release/scope.md).
- 지침: [작업 절차](../agents/workflow.md), [아키텍처](../agents/architecture.md), [Unity 에셋](../agents/unity-assets.md), [하네스](../agents/harness.md), [검증](../agents/verification.md), [Unity 도구](../agents/unity-tools.md).

## 작업 목록

- [x] 승인된 정의/실행 상태 경계를 기준으로 최소 자료형·컴포넌트·생성/조회 구조 구체화
- [x] 대표 SO 정의와 일반 C# Thing·컴포넌트 소유 관계 구현
- [x] 개별 ID·최초 생성 순서·존재 상태·단일 소속의 기준 구현
- [x] 상태 독립·정의 불변·컴포넌트 조회·소속 일관성을 실제 객체로 검증
- [x] Unity 컴파일과 해당 테스트 실행·증거 보관
- [x] 문서 검사 및 로드맵 첫 묶음 상태 갱신

모든 콘텐츠 SO 에셋이나 후속 단계의 모든 필드를 먼저 완성하지 않는다. 이번 검증에 필요한 대표 정의부터 구성한다.

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 같은 정의를 쓰는 두 개체의 상태가 독립적 | 한 개체를 변경해도 다른 개체와 SO 정의가 바뀌지 않음 | Edit Mode 테스트 결과·대상 정의 |
| 필요한 기능을 개체에서 조회할 수 있음 | 컴포넌트 구성·조회와 소유 Thing 연결 확인 | 실제 객체를 사용하는 테스트 |
| 개별 ID·정의 ID·생성 순서가 구분됨 | 개체 고유성, 같은 정의 공유, 관계 변경 후 식별·순서 유지 확인 | 식별·관계 테스트 결과 |
| 실물 자원의 소속 원본이 하나임 | 보관처 변경에서 중복 소속이나 중복 실물 참조가 생기지 않음 | 정상·중복 소속 방지 테스트 |
| Unity에서 임포트·실행 가능한 기반임 | 컴파일, 필요한 통합 검사, 문서·PLAN 검사 | 실행 수·결과·로그·문서 검사 |

파일 저장/복원과 Map 밖 개체 복원은 이후 기반 묶음에서 검증한다. 첫 묶음 완료만으로 선행 기반 전체 완료나 1단계 착수를 선언하지 않는다.

## 현재 상황과 다음 행동

- 마지막 확인: 최소 기반 코드와 대표 SO 3개를 구현하고 Unity 컴파일·Edit Mode 테스트 13개를 통과했다. 새 관찰 구간의 오류·경고는 0건이며 Editor는 `ready`·`stopped` 상태다.
- 다음 행동: 승인 순서의 [PLAN-004 공간·공통 조작 기반](PLAN-004-map-and-commands.md)을 다음 작업 대상으로 선정했다. 해당 구현·검증은 미착수다.
- 환경 참고: PLAN-002에 기록된 Editor/Pipeline 주의점을 확인하고 지원 명령을 우선 사용한다.

## 적용한 최소 구현

| 구분 | 코드·에셋 | 이번 범위 |
| --- | --- | --- |
| SO 정의 | [ThingDefinition](../../UnityProject/Assets/Game/Foundation/Definitions/ThingDefinition.cs)과 자원·시설·생명체 하위 SO | 정의 ID·이름·현재 필요한 컴포넌트 종류·유효성 검사 |
| 개별 상태 | [Thing](../../UnityProject/Assets/Game/Foundation/Things/Thing.cs), [ThingComponent](../../UnityProject/Assets/Game/Foundation/Things/ThingComponent.cs) | 읽기 전용 ID·생성 순서·정의·존재·소속 조회와 개별 컴포넌트 소유 |
| 관계 관리 | [ThingRegistry](../../UnityProject/Assets/Game/Foundation/Things/ThingRegistry.cs) | 생성·식별 조회·단일 자원 소속 변경·수량 소비·논리적 제거 |
| 대표 정의 | [유기물](../../UnityProject/Assets/Game/Definitions/OrganicMatter.asset), [저장소](../../UnityProject/Assets/Game/Definitions/Storage.asset), [시작 보조형](../../UnityProject/Assets/Game/Definitions/PrototypeSupport.asset) | 기존 콘텐츠에서 분류별 대표 1개. 전체 콘텐츠·생산 설정 완료가 아님 |
| 검증 | [Thing 검사](../../UnityProject/Assets/Tests/FoundationEditMode/ThingFoundationTests.cs), [소속 검사](../../UnityProject/Assets/Tests/FoundationEditMode/ResourceOwnershipTests.cs), [정의 검사](../../UnityProject/Assets/Tests/FoundationEditMode/DefinitionFoundationTests.cs) | 실제 공개 API와 Unity SO 로딩 사용 |

정의 설정에 따라 매번 새 C# 컴포넌트를 만들며 자원 소속은 `HolderId` 한 곳에서 관리한다. 구현 선택·대안·후속 영향은 [TECH-001](../decisions/TECH-001-thing-foundation.md)을 참조한다.
Map·화면·저장·생산·운반 규칙은 추가하지 않았다. 용량을 검사하지 않는 기반 보관 기능을 게임 입고 규칙 구현으로 해석하지 않는다.

## 재실행 방법

열린 UnityProject의 지정 Editor에서 실행한다. 메뉴는 누락된 대표 정의만 생성하며 기존 값이 다르면 덮어쓰지 않는다.

```powershell
$projectPath = Join-Path (Get-Location) 'UnityProject'
unity command editor invoke_menu --path 'Tools/Mobile Game/Create Foundation Definitions' --project-path $projectPath --json
unity command test run --mode editor --filter MobileGame.Foundation.EditMode.Tests --filter_type assembly --project-path $projectPath --json
```

코드 변경 뒤에는 [Unity 도구 절차](../agents/unity-tools.md)에 따라 컴파일 완료를 먼저 확인한다. 테스트 JSON의 Summary와 개별 결과를 보관하며 XML은 콘솔의 저장 위치에서 바로 복사한다.

## 검증 결과

| 날짜 | 대상·환경·방법 | 실제 결과·증거 |
| --- | --- | --- |
| 2026-10-26 | Unity 6000.3.27f1·Android 대상의 기존 Editor, 공식 CLI/Pipeline 재컴파일 | 성공, 컴파일 오류 없음. [런타임 컴파일](evidence/PLAN-003/2026-10-26-compile-result.json), [테스트 컴파일](evidence/PLAN-003/2026-10-26-test-compile-result.json). |
| 2026-10-26 | Editor 메뉴와 AssetDatabase로 대표 SO 3개 생성 | 메뉴 실행·Unity 생성 에셋·메타 확인. [생성 응답](evidence/PLAN-003/2026-10-26-definition-assets-create.json). 실제 에셋 로딩과 생성/이동 후 SO 불변은 테스트에서 검증. |
| 2026-10-26 | Test Framework 1.6.0, `MobileGame.Foundation.EditMode.Tests` 어셈블리 | 총 13개·통과 13개·실패/건너뜀/불확정 0개. [실제 JSON](evidence/PLAN-003/2026-10-26-foundation-tests.json), [NUnit XML](evidence/PLAN-003/2026-10-26-foundation-tests.xml), [통합 검증](evidence/PLAN-003/2026-10-26-foundation-verification.json). |
| 2026-10-26 | 변경 에셋·소스·메타 및 기존 프로젝트 설정 해시 검사 | 신규 소스/에셋 22개와 폴더 7개의 Unity 메타 확인. 기존 버전·패키지·씬·URP 설정 6개 해시 유지. [검사 결과](evidence/PLAN-003/2026-10-26-source-and-meta-audit.json). |
| 2026-10-26 | 기획 요약·기술 결정·PLAN 전환 후 문서 검사 | 줄 수 제한·단일 active PLAN-004·목록 status·로컬 참조 통과. [검사 로그](evidence/PLAN-003/2026-10-26-doc-validation.log). |

이번 테스트는 상태 독립·정의 불변·컴포넌트 소유·ID/순서 유지·단일 소속·중복 이동·잘못된 수량/소속·타 registry·소진/제거를 확인했다. 수량 예시 5 등은 테스트 입력이며 게임 수치의 신규 확정이 아니다.
이전 PLAN의 QuickInstaller 예외는 Pipeline 기록에 보존됐으며 이번 새 구간에서 재현되지 않았다. 근본 해결로 기록하지 않는다.
이번 PLAN에서 새 Play Mode 검사·Android 빌드·실기기·Map·파일 저장/복원은 실행하지 않았다. 순수 C# 기반과 실제 SO 로딩은 Edit Mode에서 검증했다.

## 관련 결정과 상태 전환

- 기술 결정: [TECH-001 최소 SO 정의와 Thing 소유 관계](../decisions/TECH-001-thing-foundation.md).
- 2026-10-26: 승인된 로드맵의 다음 작업 단위로 proposed 작성. 구현 미착수.
- 2026-10-26: PLAN-002 실제 완료에 따라 proposed → active. 현재 작업 대상으로 선정했으며 구현·검증을 시작한 것은 아니다.
- 2026-10-26: 사용자 요청으로 정의·Thing 기반 구현 착수. 검증 전이므로 active 유지.
- 2026-10-26: 첫 기반 묶음의 코드·대표 에셋·13개 테스트·문서 검증 완료로 active → completed. 다음 PLAN-004를 함께 활성화했으며 해당 구현은 시작하지 않았다.

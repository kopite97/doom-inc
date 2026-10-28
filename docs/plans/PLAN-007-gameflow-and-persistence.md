---
id: PLAN-007
type: development
status: completed
title: GameFlow와 저장·복원 기반
updated: 2026-10-26
---

# PLAN-007 GameFlow와 저장·복원 기반

## 목표

신규 시작·이어하기·다시 시작을 공통 GameFlow로 조정하고, 현재 구현된 Thing·자원 소속·Map 상태를 기기 내 한 슬롯에 저장·복원한다.
로드맵 0단계의 세 번째 묶음이다. 기반 검증을 통과한 뒤 1단계 초기 건설·수집·저장 작업을 별도 PLAN으로 진행한다.

## 범위

- 선행 정리: [프로젝트 구조](../agents/project-structure.md)·[스크립트 구조](../agents/script-structure.md)에 따라 기존 코드·SO 등 에셋을 역할·주제별로 이관하고 참조·기존 검증을 확인한다. 새 기능 구현과 구분해 기록한다.
- 포함: 한 판의 준비·플레이·멸망 연출·결과 전환과 진행 허용 여부를 관리하는 최소 GameFlow, 신규 세션 준비·복원·교체·초기화.
- 포함: 현재 상태의 저장 데이터, 정의 ID를 SO에 연결하는 조회, 기존 ID·최초 생성 순서·소속·배치의 복원 경로.
- 포함: 기기 내 논리적 1슬롯 파일 저장, 자동 저장, 마지막 저장 이어하기, 다시 시작 시 초기 상태로 저장 교체, 종료 중 진행 없음.
- 포함: 기존 배치 편집 상태와 연구소 일시정지의 연결, UI와 하네스가 함께 사용하는 전환·저장·조회 경로와 최소 PC 검증 화면.
- 포함: 잘못된 저장·파일 입출력 실패를 감지하고 부분 복원이나 기존 정상 상태의 손상을 막는 최소 처리.
- 제외: 실제 건설·수집·운반·가공·배양, Job·예약·경로 탐색, 저장 용량·허용 자원 등 아직 없는 기능과 그 상태.
- 제외: 실제 침공형 방출·외부 활동·성과·멸망 게이지·전과 보고·멸망 연출 에셋·최종 결과 UI, 전체 게임 사이클.
- 제외: 다중 슬롯·클라우드 저장·계정·오프라인 보상·범용 마이그레이션 프레임워크, 모바일 최종 UI·Android 빌드·실기기 검증.

2026-10-26 사용자가 이 PLAN 실행을 명시적으로 승인했다. 기존 Assets 이관·회귀 검증을 먼저 수행하고 GameFlow·저장 기반 구현을 이어간다.
클래스명·저장 형식·주기·복원 순서·씬 구성은 D-059의 개발 위임에 따라 구체화한다. 이번 작업으로 새 게임 규칙이나 초기 비용을 확정하지 않는다.

## 기준 문서와 선행 조건

- 선행: [PLAN-003](PLAN-003-thing-foundation.md)의 정의·Thing 기반, [PLAN-004](PLAN-004-map-and-commands.md)의 Map·공통 조작 기반 완료.
- 단계: [로드맵](../game-design/05-development/02-implementation/roadmap.md)의 D-080·D-086, 세 번째 기반 묶음 완료 기준.
- 흐름: [GameFlow](../game-design/05-development/01-architecture/game-flow.md)의 D-085·D-064, [시설 편집 일시정지](../game-design/02-systems/03-space/placement.md#placement-pause)의 D-033.
- 저장: [플레이 환경](../game-design/04-delivery/01-platform/environment.md)의 D-069, [Thing 모델](../game-design/05-development/01-architecture/thing-model.md)의 D-075~D-079·D-082~D-084.
- 실제 시작 상태와 후속 경계: [최초 제공 범위](../game-design/04-delivery/02-release/scope.md)의 D-071. 프로토타입 1개체·선배치 시설 없음·획득시설과 저장소 각 1개를 건설할 유기물이다.
- 기존 기술 결정: [TECH-001](../decisions/TECH-001-thing-foundation.md)의 단일 자원 소속·식별, [TECH-003](../decisions/TECH-003-map-and-placement.md)의 Map 단일 공간 원본·편집 상태.
- 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](AGETNS.md), [아키텍처](../agents/architecture.md), [하네스](../agents/harness.md), [검증](../agents/verification.md), [Unity 에셋](../agents/unity-assets.md), [Unity 도구](../agents/unity-tools.md).
- 실행 환경은 기존 Unity 6000.3.27f1·Universal 2D(URP)·Android 대상을 유지한다. 검증은 우선 PC Editor에서 수행한다.
- 추가 지침: 파일 배치는 [프로젝트 구조](../agents/project-structure.md)와 [스크립트 구조](../agents/script-structure.md)를 따른다. 아래 현재 소스 링크는 이관 때 실제 목적지로 갱신한다.
- 아트·임시 에셋이 필요한 경우 [아트 지침](../agents/art-assets.md)을 따른다. 정식 아트는 사용자 제공, 모든 Sprite는 PPU 32, 임시 에셋은 영문 `Temp_` 표식과 교체 위치를 기록한다. 아트 제작을 이 PLAN의 새 목표로 추가하지 않는다.

## 현재 구현과 저장 대상

착수 기준은 `0162869`의 PLAN-004 구현을 포함한 `aeec4cc`다. 아래 기존 상태에 GameFlow·파일 저장·복원·공통 진행 제어를 추가했다. 실제 생산·외부 활동은 후속 범위다.

| 현재 원본 | 이번 기반에서 보존·복원할 내용 |
| --- | --- |
| [ThingRegistry](../../UnityProject/Assets/Scripts/Core/Things/ThingRegistry.cs)·[Thing](../../UnityProject/Assets/Scripts/Core/Things/Thing.cs) | 존재하는 Thing의 ID·정의 ID·최초 생성 순서, 이후 생성에 사용할 순서의 최댓값. 제거된 Thing은 되살리지 않음 |
| [자원 수량](../../UnityProject/Assets/Scripts/Core/Resource/ResourceStackComponent.cs)·보관 관계 | 양의 수량과 자원 Thing의 HolderId. 보관처별 재고는 이 관계로 조회하며 중복 원본을 저장하지 않음 |
| [GridMap](../../UnityProject/Assets/Scripts/Core/Spatial/GridMap.cs) | 영역 크기, 배치된 Thing ID·위치·90도 회전. 점유·상호작용 위치·통행 정보는 기존 규칙으로 재구성 |
| Map에 없는 Thing | 존재와 컴포넌트·자원 소속 유지, 강제 좌표 부여 없음. 배치 해제한 생명체로 검증하며 외부 활동 구현으로 간주하지 않음 |
| GameFlow | 재개할 진행 단계와 필요한 최소 상태. 이어하기가 연출·결과 단계를 무조건 플레이로 바꾸지 않음 |

SO 자체·Unity InstanceID·화면 객체를 저장 원본으로 쓰지 않는다. 정의 ID로 동일 종류의 SO를 찾아 각 Thing에 독립된 실행 컴포넌트를 재구성한다.
배치 후보·선택 표시 같은 임시 조작 상태와 확정된 게임 상태의 저장 경계를 구현 시 정한다. 편집 중 자동 저장이 미확정 위치를 적용하거나 복원 후 일시정지가 풀리지 않는 상태를 만들지 않아야 한다.

## 구현 경계와 순서

먼저 기존 파일을 새 Assets 구조로 이관한다. `.meta`·GUID·어셈블리 정체성·게임 동작을 보존하고 생성 도구·테스트의 경로 문자열과 현행 문서 링크를 갱신한다. Unity 고정 경로는 유지하며 과거 검증 증거의 경로·해시는 수정하지 않는다. 컴파일·기존 회귀 검증을 통과한 뒤 아래 구현을 시작한다.

1. 세션·GameFlow·저장 기능의 책임과 저장할 현재 상태를 구체화한다. GameFlow는 전환을 조정하고 재고·공간 등 원본 상태는 기존 담당 객체에 둔다.
2. 기존 식별자·생성 순서로 Thing을 재구성하고 자원 관계·Map을 복원하는 최소 경로를 만든다. 복원 검증이 끝나기 전 부분 상태를 현재 세션에 노출하지 않는다.
3. 한 슬롯의 실제 파일 저장·읽기·자동 저장과 오류 처리를 연결한다. 저장이 없는 최초 실행은 시작 설정으로 새 세션을 준비하고, 이어하기는 마지막 저장에서 복원한다. 다시 시작은 새 세션 준비와 저장 교체를 함께 처리한다.
4. 준비 완료 후 플레이 진입, 목표 도달 통지 후 진행 정지·연출 진입, 연출 완료 후 결과 전환과 다시 시작을 공통 명령으로 연결한다. 이 단계의 목표 도달·연출 완료 입력은 검증용 신호다.
5. 배치 편집 중 연구소 진행 정지를 연결한다. 한 판 종료에 따른 전체 진행 정지와 구분하고, 확정·취소가 종료된 판을 재개하지 않게 한다. 화면 표시는 진행 정지 중에도 갱신한다.
6. 최소 PC 화면과 하네스를 같은 경로에 연결하고 아래 검증을 수행한다. 주요 기술 선택은 docs/decisions에, 실제 방법·결과는 이 PLAN에 기록한다.

신규 시작 구성은 주입·교체 가능한 경계로 둔다. 이 기반의 검증에는 명시적인 준비 데이터를 사용하며, [기존 MapFoundationScenario](../../UnityProject/Assets/Scripts/Presentation/Diagnostics/Map/MapFoundationScenario.cs)의 시설 2개·보조형 2개·유기물 5개를 실제 초기 상태로 채택하지 않는다.
D-071의 건설 비용·초기 지급량·전달 방식과 실제 시작 흐름은 1단계에서 연결한다. 이번 완료는 세션 초기화·저장 초기화를 검증한 것이며 실제 첫 건설까지 구현했다는 뜻이 아니다.
현재 없는 생산·외부 활동 계산 대신 공통 진행 제어에 전달되는 갱신을 관찰해 정지·재개·종료 중 진행 없음을 확인한다. 후속 기능의 시간 계산을 테스트 전용으로 복제하지 않는다.

## 작업 목록

- [x] 기존 Assets를 역할·주제별 구조로 이관하고 GUID·참조·코드 경로·문서 링크·기존 회귀 검증 확인
- [x] 세션·GameFlow·저장 책임, 정의 조회와 저장 대상·임시 상태 경계 구체화
- [x] ID·생성 순서·컴포넌트·단일 자원 소속·Map의 복원과 데이터 유효성 검사 구현
- [x] 실제 1슬롯 파일 입출력·자동 저장·오류 처리·다시 시작 시 저장 교체 구현
- [x] 단계 전환·진행 제어·배치 편집 정지와 UI/하네스 공통 경로 연결
- [x] 최소 PC 화면·실제 파일 왕복·세션 재생성·정상/예외 검증 수행
- [x] 기존 기반 회귀 확인, 결과·증거·기술 결정·로드맵 갱신 후 완료 판정

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 디렉터리 이관과 기존 동작 유지 | 프로젝트 C#이 Scripts 아래에 있고 각 역할/에셋 종류 아래 주제별로 분류됨. GUID·어셈블리·씬/SO 참조·경로 문자열·문서 링크 확인, 기존 테스트 통과 | 이동 목록·참조 검사·기존 테스트 결과 |
| 저장 후 관계와 상태 일치 | Edit Mode에서 실제 SO·Thing·Map을 저장 데이터/파일로 왕복하고 새 registry·Map으로 복원. ID·정의·생성 순서·수량·보관처·위치·회전·공간 조회 비교 | 테스트 결과와 비교 상태 |
| 단일 소속·개체 독립성 | 같은 SO의 시설 둘과 생명체, 보관처가 다른 자원, Map 밖 생명체를 복원. 한 객체 변경이 다른 객체/SO를 바꾸지 않고 자원이 한 보관처에만 존재함 | 관계·수량·독립성 단언 |
| 생명주기와 생성 순서 유지 | 제거/전량 소비한 Thing은 복원되지 않음. 마지막 생성 Thing 제거 후 저장해도 복원 후 새 생성 순서가 과거 최댓값을 넘음 | 제거·재생성 경계 테스트 |
| 유효한 기존 배치 보존 | 회전·생명체 중첩·통로 차단·Map 미배치 상태와 생명체 위에 시설을 놓은 유효 상태를 복원. 복원 순서가 기존 허용 상태를 거부하지 않음 | Map 조회·기존 공간 규칙 비교 |
| 잘못된 저장·쓰기 실패 처리 | 누락된 정의/보관처, 중복 ID·잘못된 수량·공간·순서, 손상 파일·지원하지 않는 저장 형식, 쓰기 실패를 대표 사례로 검사. 부분 세션이나 정상 저장 덮어쓰기가 없어야 함 | 실패 원인·전후 상태/파일 비교 |
| GameFlow 전환과 진행 제어 | 저장 없음은 신규 준비, 유효 저장은 복원으로 진입하며 준비 완료 뒤에만 재개. 검증 신호로 플레이→연출→결과 전환. 편집 중 연구소 정지, 확정·취소 후 허용 시 재개, 종료 후 재개 금지. 저장된 단계도 보존 | 공통 명령과 갱신 관찰 결과 |
| 자동 저장과 마지막 상태 이어하기 | 선택한 자동 저장 조건을 실제로 발생시켜 파일을 쓰고 실행 세션을 종료·재생성한 뒤 마지막 저장 복원. 수동 저장 함수 호출만으로 자동 저장 성공을 판정하지 않음 | Play Mode/PC 실행 로그·파일·복원 상태 |
| 종료 중 진행 없음 | 저장 후 벽시계상 시간이 경과한 조건에서도 복원 상태가 추가 진행하지 않음. 재개 후 명시적 게임 갱신만 반영 | 제어 가능한 시간 입력과 상태 비교 |
| 다시 시작의 완전한 초기화 | 자원·배치·Map 밖 개체·편집/선택이 있는 세션에서 다시 시작 후 이전 ID·관계·조작 참조가 현재 세션에 남지 않음. 다시 읽은 저장도 지정한 시작 구성과 일치 | 반복 초기화·파일 재읽기 결과 |
| 화면과 하네스 일치·기존 회귀 없음 | 실제 씬의 최소 조작과 하네스가 같은 세션 명령을 호출하고 복원된 상태를 표시. PLAN-003/004 검사를 포함해 영향받은 Edit Mode·Play Mode 검사 통과 | XML/JSON·실행 수·화면/관찰 기록 |

테스트는 별도 저장 경로를 주입해 플레이 저장을 덮어쓰지 않는다. 실제 파일 읽기와 새 객체 그래프 복원을 확인하며 같은 객체를 보관했다 다시 반환하는 방식으로 대체하지 않는다.
파일 형식·자동 저장 조건·임시 파일 교체 방식·지원 범위·편집 중 저장 처리는 구현 때 선택하고 근거와 실패 시 동작을 기록한다. 오류 파일을 자동 초기화하는 정책을 새 게임 시작과 혼동하지 않는다.
PC에서 검증한 파일 저장·Unity 생명주기 연결을 Android 강제 종료·실기기 검증 완료로 표현하지 않는다. 실제 생산·침공의 저장과 진행 정지는 해당 후속 단계에서 검증을 확장한다.
검증 증거는 구현 실행 시 `docs/plans/evidence/PLAN-007/`에 보관하고 실제 실행 명령·환경·소스 식별 정보·실패와 재검증·미검증 범위를 기록한다.

## 현재 상황과 다음 행동

- 2026-10-26: 확정 기획과 현재 코드를 읽고 범위·완료 기준·검증 방법을 작성했다. PLAN-003/004는 completed이며 PLAN-007은 proposed다.
- 2026-10-26: 사용자 요청의 전체 디렉터리 지침을 추가하고 이관을 선행 정리 항목으로 반영했다. 현재는 문서만 갱신했으며 Assets 파일은 이동하지 않았다.
- 2026-10-26: Aseprite 사용자 제작·Sprite 임포트 기본값·사용자 시각 검수·영문 Temp 표식을 지침에 반영했다. 실제 아트 임포트·에셋 생성·개명은 수행하지 않았다.
- 2026-10-26: 사용자 실행 승인으로 유일한 active PLAN으로 전환했다. 현재는 Assets 이관 준비 단계이며, 이관 후 기존 회귀 검증을 통과한 뒤 기반 기능을 구현한다.
- 2026-10-26: Assets 이관과 기존 회귀 검증 완료. 기존 메타/GUID 62개·어셈블리 7개 보존, Edit Mode 29/29·Play Mode 2/2 통과.
- 2026-10-26: 세션 전환·저장·복원·실패 보존·실제 Update 연결을 구현했다. 테스트 타입 이름 충돌과 손상 저장의 예외 처리 누락을 수정해 재컴파일·Edit Mode 72/72, Play Mode 4/4를 통과했다. 검토에서 발견한 전환 후 알림 예외와 파일 실패의 경계도 분리하고 회귀 검사했다.
- 2026-10-26: 최종 에셋 감사·독립 코드 검토·문서 정합성 확인 완료. 선행 기반 세 묶음의 마지막 완료 기준을 충족했다.
- 다음 행동: 1단계 초기 건설·수집·저장 PLAN의 별도 작성·승인. 새 PLAN을 만들거나 활성화하지 않았으며 실제 게임 사이클 구현은 시작하지 않았다.

## 구현 위치와 확인 범위

- [GameFlowManager](../../UnityProject/Assets/Scripts/GameFlow/Session/GameFlowManager.cs): 신규 시작·이어하기·재시작·준비/플레이/연출/결과 단계·공통 진행 및 배치 명령. [Edit Mode 검사](../../UnityProject/Assets/Scripts/Tests/EditMode/Foundation/GameFlow/GameFlowManagerTests.cs).
- [SessionSnapshotCodec](../../UnityProject/Assets/Scripts/Persistence/Serialization/SessionSnapshotCodec.cs)·[파일 저장](../../UnityProject/Assets/Scripts/Persistence/Storage/JsonFileSessionStore.cs): 버전 1 JSON, 별도 후보 그래프 검증, 실제 1슬롯 교체. [저장 검사](../../UnityProject/Assets/Scripts/Tests/EditMode/Foundation/Persistence/SessionPersistenceTests.cs).
- [진단 씬](../../UnityProject/Assets/Scenes/Diagnostics/Session/Temp_SessionFoundation.unity)·[화면 연결](../../UnityProject/Assets/Scripts/Presentation/Diagnostics/Session/SessionFoundationView.cs): 실제 Update·앱 중단·종료와 같은 명령 연결. [Play Mode 검사](../../UnityProject/Assets/Scripts/Tests/PlayMode/Session/SessionFoundationViewTests.cs)는 별도 임시 저장 경로에서 실행했다.
- 기본 자동 저장은 실행 중 5초 간격과 단계 전환·앱 중단·종료 시점이다. 새 게임/재시작은 파일 교체 성공 후 현재 세션을 바꾸고, 잘못된 저장을 자동으로 새 게임으로 덮어쓰지 않는다. 상세 책임·오류 처리는 [TECH-004](../decisions/TECH-004-session-and-persistence.md).
- 진단 씬·오브젝트는 임의(Temp) 검증용이다. 정식 게임 화면과 연결할 때 교체하며, 목적·구성과 실행법은 [증거 README](evidence/PLAN-007/README.md)에 기록했다. 새 Sprite를 사용하거나 요청하지 않았으므로 Require 폴더는 만들지 않았다.
- PC에서 실제 파일·객체 재생성·화면 렌더링을 확인했다. 마우스 이벤트 주입·모바일 UI·Android 파일 교체/강제 종료/빌드/실기기 성능은 검증하지 않았다. 실제 건설·생산·침공과 해당 진행도 저장도 후속 단계다.

## 검증 결과

| 날짜 | 대상·환경·실행 방법 | 실제 결과 | 판정·증거 |
| --- | --- | --- | --- |
| 2026-10-26 | 후보·구조/아트 지침·MAP·로드맵 / `node scripts/validate-agent-docs.mjs` | PLAN 7개와 목록 일치, 지침 줄 수·로컬 링크 920개 검사에서 오류 없음. active=0 조건만 오류 | 종료 코드 1. 승인 대기 상태를 유지하며 전체 검사 통과로 기록하지 않음 |
| 2026-10-26 | Assets 이관·Unity 재컴파일·기존 전체 회귀 | 컴파일 오류 0, Edit Mode 29/29, Play Mode 2/2, 건너뜀 0 | [이관 기록](evidence/PLAN-007/README.md), [컴파일](evidence/PLAN-007/migration-recompile-status.json), [Edit](evidence/PLAN-007/migration-editmode.json), [Play](evidence/PLAN-007/migration-playmode-status.json) |
| 2026-10-26 | 첫 통합 컴파일·Edit Mode | SessionState 이름 충돌 1건 수정 후 재컴파일 성공. Edit 72건 중 71 통과·손상 저장 처리 1 실패, 예외 처리 수정 | [최초 컴파일 실패](evidence/PLAN-007/session-recompile-status.json), [수정 후 컴파일](evidence/PLAN-007/session-recompile-retry-status.json), [최초 Edit 결과](evidence/PLAN-007/session-editmode-first.json). 아래 최종 실행에서 재검증 완료 |
| 2026-10-26 | Unity 6000.3.27f1·Windows Editor·Android 대상 / 전체 Edit Mode | 72/72 통과, 실패·건너뜀·판정 불가 0 | [최종 컴파일](evidence/PLAN-007/session-final-recompile-status.json), [전체 Edit JSON](evidence/PLAN-007/session-editmode-final.json) |
| 2026-10-26 | 같은 Editor·전체 Play Mode / 실제 진단 씬·Update·파일·공통 명령 | 4/4 통과, 실패·건너뜀·판정 불가 0. 자동 저장, 새 그래프 복원, 단계 유지, 오프라인 추가 진행 없음, 재시작 파일 교체와 선택 초기화 확인 | [씬 생성](evidence/PLAN-007/session-scene-creation.json), [전체 Play JSON](evidence/PLAN-007/session-playmode-first-status.json), [XML](evidence/PLAN-007/session-playmode-final.xml), [화면](evidence/PLAN-007/session-foundation-playmode.png) |
| 2026-10-26 | 종료 후 Editor·콘솔·화면 확인 | Play stopped, 컴파일 완료, 현재 콘솔 오류·경고 0. 화면의 격자·단계·저장 상태·조작 버튼 표시 확인 | [Editor](evidence/PLAN-007/session-final-editor-status.json), [콘솔](evidence/PLAN-007/session-final-console-status.json). 누적 버퍼 오류 2개는 이전 QuickInstaller 예외와 수정한 타입 이름 충돌이며 [이력](evidence/PLAN-007/session-final-console-history.json)을 보존함 |
| 2026-10-26 | 최종 정적 검사·독립 코드 검토 | 기존 메타/GUID 62·asmdef 7 보존. C# 39개 모두 Scripts, 메타 116개 누락·중복 0, asmref 4·씬/SO 참조 정상. 추가 코드 결함 없음 | [에셋 감사](evidence/PLAN-007/session-asset-final-audit.json), [최종 소스 SHA-256](evidence/PLAN-007/session-source-manifest.json). `git diff --check` 통과 |
| 2026-10-26 | `node scripts/validate-agent-docs.mjs` / 완료 전·후 | active PLAN-007 하나일 때 전체 통과. 완료 전환 후 PLAN 7개·목록·문서 줄 수·링크 오류 없이 active=0 조건만 오류 | [완료 전](evidence/PLAN-007/session-docs-active.txt), [완료 후](evidence/PLAN-007/session-docs-completed.txt). 완료 후 종료 코드 1은 다음 PLAN 승인 대기이며 전체 통과로 기록하지 않음 |

## 관련 결정과 상태 전환

- 기존 결정: TECH-001·TECH-003. 이번 구조·저장·오류·시간 제어는 [TECH-004](../decisions/TECH-004-session-and-persistence.md)에 기록했다.
- 2026-10-26: 사용자 요청 “PLAN-007을 proposed 상태로 작성해줘”에 따라 최초 생성 → proposed. 실행·활성화 승인은 없음.
- 2026-10-26: 사용자 요청 “PLAN-007의 실행을 승인할게. 먼저 기존 Assets를 새 디렉터리 구조로 정리하고, GameFlow와 저장·복원 기반 구현을 진행해줘”에 따라 proposed → active.
- 2026-10-26: Assets 이관, GameFlow·저장·복원 기반, 필수 Edit 72/72·Play 4/4와 최종 정적 검증 충족으로 active → completed. 실제 게임 기능·Android 검증은 명시한 후속 범위를 유지한다.
- 완료 후에도 다음 PLAN을 자동 활성화하지 않는다. 1단계의 후보 작성·실행은 별도 작업으로 관리한다.

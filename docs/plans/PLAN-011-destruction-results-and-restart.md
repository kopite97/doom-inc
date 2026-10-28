---
id: PLAN-011
type: development
status: completed
title: 멸망 연출·결과 화면·한 판 종료와 재시작 완결
updated: 2026-10-28
---

# PLAN-011 멸망 연출·결과 화면·한 판 종료와 재시작 완결

## 목표

PLAN-010의 실제 활동 성과로 목표에 도달한 뒤, 연구소와 외부 활동을 정지한 상태에서 간단한 세계 멸망 연출을 보여주고 결과 화면으로 전환한다.
결과 화면의 ‘다시 시작’이 초기 상태와 저장 슬롯을 함께 교체하고, 새 판의 생산·침공·종료를 다시 진행할 수 있게 한다.
PC 가로 화면에서 초기 건설 → 생산·배양 → 방출·성과/보고 → 목표 → 멸망 연출 → 결과 → 재시작까지 연결하여 로드맵 3단계를 마무리한다. 전체 프로토타입의 최종 완료와 4단계 모바일 검증은 구분한다.

## 범위

- 포함: 실제 목표 도달에 연결된 간단한 멸망 연출, 연출 완료 후 `Destruction → Results` 전환, 결과 화면과 다시 시작 조작.
- 포함: 종료 후 정지 유지, 결과 정보 조회, 연출/결과 중 입력과 편집 미리보기 처리, 단계에 맞는 화면 표시.
- 포함: 연출 시간과 게임 진행 시간의 분리, 앱 정지/재개, 화면 재연결·세션 교체·중복/지연 완료 신호 처리.
- 포함: 목표/결과 자동 저장, 종료 단계 이어하기, 기존 v4·v2/v3 종료 이관 호환, 저장·복원·재시작 실패 보존과 재시도.
- 포함: PC 가로 실제 씬의 전체 한 판과 재시작 후 반복 검증, 공통 명령/하네스·Edit/Play 회귀·화면·소스 증거.
- 제외: 여러 세계·개체 승계·환생·영구 보너스·새 보상/업적/해금·전투 지도·귀환·수명·손실·새 콘텐츠 종류.
- 제외: 세로 UI·Android 빌드/실기기·최종 성능/밸런스·대표 병목 개선의 종합 검증·전체 UI 개편·정식 아트 제작. 이들은 로드맵 4단계 또는 별도 후속 범위다.
- 기존 2자원·4시설·3생명체 모델, 생산/방출/성과 규칙과 초기 구성을 유지한다. 종료 연출을 위해 Thing을 삭제하거나 세계 성과를 다시 지급하지 않는다.

2026-10-28 사용자가 “멸망 연출·결과 화면·한 판 종료/재시작 완결을 위한 PLAN-011을 작성해줘.”라고 요청했다. 이번은 후보 작성이며 PLAN-010의 실행 승인을 이 PLAN에 적용하지 않는다. 최초 상태는 `proposed`다.
연출 표현·길이·문구·결과 항목·정확한 타입/API는 D-059의 개발 위임이다. 필수 사전 기획의 미해결 핵심 결정은 없다.

## 기준 문서와 선행 조건

- 선행: [PLAN-010](PLAN-010-invasion-activity-and-reports.md) completed. [실행 증거](evidence/PLAN-010/README.md)의 Edit 201/201·Play 8/8·오류/경고 0은 선행 기반 결과이며 이번 PLAN의 통과 결과로 재사용하지 않는다. 기존 미커밋 코드·에셋·문서·증거를 보존한다.
- 단계: [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md)의 3단계·D-080·D-086·D-090. Unity 6000.3.27f1·URP·Android 대상의 기존 Editor/CLI 경로와 PC 가로 검증을 유지한다.
- 종료 규칙: [목표와 반복 흐름](../game-design/01-direction/02-experience/core-loop.md#run-end)의 D-063·D-064·D-066. 실제 성과 목표 → 양쪽 정지 → 멸망 연출 → 결과 → 다시 시작 순서다.
- 전환/화면: [GameFlow](../game-design/05-development/01-architecture/game-flow.md)의 D-085, [화면과 조작](../game-design/03-player-experience/03-interface/controls.md)의 D-067·D-079·D-090, [시설 편집 정지](../game-design/02-systems/03-space/placement.md#placement-pause)의 D-033.
- 저장/초기화: [플레이 환경](../game-design/04-delivery/01-platform/environment.md)의 D-069, [최초 제공 범위](../game-design/04-delivery/02-release/scope.md)의 D-065·D-070·D-071, [Thing 상태 소유](../game-design/05-development/01-architecture/thing-model.md)의 D-077~079.
- 위임과 기술: [D-059 개발 위임](../game-design/00-management/03-questions/open-questions.md), [TECH-004](../decisions/TECH-004-session-and-persistence.md), [TECH-007](../decisions/TECH-007-invasion-time-and-save-boundaries.md).
- 실행 전 지침: [작업 절차](../agents/workflow.md)·[PLAN 관리](AGETNS.md)·[아키텍처](../agents/architecture.md)·[명명](../agents/naming.md)·[프로젝트 구조](../agents/project-structure.md)·[스크립트 구조](../agents/script-structure.md).
- 검증/에셋 지침: [하네스](../agents/harness.md)·[검증](../agents/verification.md)·[Unity 도구](../agents/unity-tools.md)·[Unity 에셋](../agents/unity-assets.md)·[아트](../agents/art-assets.md).

## 착수 시 기반과 확장 지점

| 확인한 현재 기반 | 이번에 연결/검증할 경계 |
| --- | --- |
| [GameFlowManager](../../UnityProject/Assets/Scripts/GameFlow/Session/GameFlowManager.cs)의 `ReachGoal` | 마지막 활동 확정 후 Destruction 자동 저장과 게임 진행 정지는 유지하고 실제 연출 진입을 연결 |
| 같은 Manager의 `FinishDestruction` | 현재 Destruction이면 Results로 바꾸고 즉시 저장함. 실제 연출 완료·세션 식별·중복 방지와 연결 |
| 같은 Manager의 `Restart`·`Reload`·`Generation` | 재시작은 새 객체 그래프 저장 성공 후 공개, 복원은 검증 후 공개. 결과 UI와 연출의 이전 세션 참조를 함께 교체 |
| [LaboratoryView](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.cs)·[GUI](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs) | 현재 목표 정지 문구와 일반 Save/Continue/Restart만 있음. 실제 연출·별도 결과 표시와 단계별 입력을 추가 |
| [InvasionState](../../UnityProject/Assets/Scripts/Gameplay/Invasion/InvasionState.cs)·[InvasionManager](../../UnityProject/Assets/Scripts/Gameplay/Invasion/InvasionManager.cs) | 실제 성과·목표·활동 수·보고·외부 개체를 결과 조회에 사용. 새로운 성과 원본을 화면에 만들지 않음 |
| [SessionSnapshotCodec](../../UnityProject/Assets/Scripts/Persistence/Serialization/SessionSnapshotCodec.cs)·[침공 Codec](../../UnityProject/Assets/Scripts/Persistence/Serialization/InvasionSnapshotCodec.cs) | v4의 단계/목표 검사·v2/v3 종료 이관·v1 진단 분리 유지, 연출/결과 복원과 실패 경로 확장 |
| [GameFlow 테스트](../../UnityProject/Assets/Scripts/Tests/EditMode/Foundation/GameFlow/GameFlowManagerTests.cs)·[침공 Play 테스트](../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.Invasion.cs) | 진단 완료 신호와 목표 정지 검증을 실제 연출→결과→재시작 경로로 확장 |

위 표는 작성 시 코드 점검이다. 실제 멸망 연출·결과 화면 구현/검증 완료를 뜻하지 않는다.

## 구현 순서와 주요 경계

### 1. 전환 책임과 연출 시간

GameFlow가 단계 전환을 소유하고 Presentation은 연출·입력·결과 표시를 맡는다. 연출 완료를 공통 명령으로 전달하며 UI와 하네스가 같은 허용 조건을 검사한다. 정확한 분할은 실행 시 기술 결정으로 남긴다.
연출이 진행되는 동안 연구소/외부 활동을 재개하지 않는다. 게임 전체 시간을 다시 켜거나 `CanAdvanceExternal`을 연출 허용 조건으로 사용하지 않는다. 기존 종료 후 저장 재시도는 게임 진행과 구분해 유지한다.
연출 시간은 프레임/명시적 시간 입력으로 검증할 수 있게 한다. 기간·단계는 D-059에서 정하되 실제 화면에 시작·진행·완료가 드러나야 한다. OnGUI 호출 횟수·보고 열람·게임 시뮬레이션 가속 배율이 연출 완료를 결정하지 않게 한다.
큰 게임 delta로 목표를 넘은 프레임의 전체 시간이나 버린 나머지를 새 연출에 다시 적용해 즉시 건너뛰지 않는다. 연출 진입 프레임·다음 프레임과 큰 연출 delta의 처리 방식을 명시하고 검사한다.
앱 정지/비활성 중 연출 시간 누적과 종료 신호를 멈추고, 재개 첫 프레임에 정지 기간을 보충하지 않는다. 실제 연출은 플레이 진행 정지와 독립적으로 화면이 활성인 동안 진행한다.

### 2. 멸망 연출·입력·완료 신호

실제 Destruction 진입을 감지해 간단한 세계 멸망 표현을 시작한다. 기존 Temp 색/문자/패널을 이용할 수 있으며 새 전투 지도나 정식 아트를 선행 조건으로 만들지 않는다.
연출/결과 중 배경 건설·배정·배양 선택·방출 입력이 통과하지 않도록 화면과 명령 경계를 함께 확인한다. 목표 도달 당시 편집 중이었다면 미리보기를 확정하지 않고 안전하게 정리하며, 정리가 생산/예약/배치 확정 상태를 바꾸거나 진행을 재개시키지 않아야 한다.
완료 신호는 해당 세션/연출과 결합한다. `Generation` 등 세션 식별과 현재 단계를 검사하여 중복 완료, 이전 View/코루틴/콜백의 지연 신호, 재시작·Reload 뒤의 신호가 현재 판을 Results로 넘기지 못하게 한다.
정상 연출 완료 뒤 Results로 한 번 전환한다. 같은 세션 View의 비활성/재활성·재연결 처리와 이벤트 해제도 설계하고 검증한다. 정확한 메서드/자료형은 위임 사항이다.

### 3. 결과 표시와 다시 시작

실제 결과 화면에서 한 판의 종료를 확인하고 ‘다시 시작’을 선택할 수 있게 한다. 최소 정보는 기존 상태에서 신뢰할 수 있는 누적 성과/목표와 활동 요약을 사용하며, 항목과 문구는 구현 시 기록한다.
결과는 종료된 세션의 확정 상태를 읽는다. 보관 보고의 합으로 성과를 재계산하거나 보고 열람으로 보상/성과를 지급하지 않는다. 목표 초과 성과가 있으면 실제 수치를 보존한다. 현재 기록하지 않는 통계를 가상의 숫자로 표시하지 않는다.
결과 UI의 재시작은 기존 새 세션 생성·저장 교체 경로를 사용한다. 정상 저장 후 Playing과 초기 화면으로 전환하고, 연출 진행·결과 참조·선택·보고 펼침·편집·이전 이벤트/콜백을 정리한다.
결과 버튼의 연속 입력과 오래된 결과 화면의 재시작 요청이 새 판을 다시 초기화하지 않도록 현재 단계/세션을 검사한다. 실패한 재시작은 결과 화면·현재 세션·정상 파일을 유지하고 오류와 재시도 수단을 제공한다.
기존 범용 Restart/Continue와 진단 경로의 유지/노출 방식을 명시한다. 결과 조작 추가를 이유로 실제 플레이 슬롯과 진단 슬롯의 구분을 없애지 않는다.

### 4. 저장·복원·오류 보존

PLAN-010의 ‘마지막 활동 확정 → ReachGoal 자동 저장’ 순서를 보존한다. 연출 완료 시 `FinishDestruction`의 Results 자동 저장도 추가 수동 Save 없이 검증한다. 두 경계에서 상태와 저장 결과를 구분한다.
기본 방향은 연출 진행을 일시적인 표현 상태로 두고, Destruction 저장을 열면 연출을 처음부터 재생하며 Results 저장은 결과 화면으로 바로 여는 것이다. 표현 재생은 성과/활동 재실행이 아니다. 실행 시 이 정책을 기술 결정과 테스트로 확정한다.
새 영속 상태가 필요 없다면 저장 버전을 불필요하게 올리지 않는다. 다른 선택으로 필드가 추가되면 버전·기본값·단계 관계·기존 파일 이관을 함께 정의한다. v4 원본과 v2/v3에서 이관한 종료 상태를 검사한다.
`legacyEndVersion`으로 보존된 구버전 종료 기록은 목표 성과가 0이어도 정상 종료 단계다. 해당 단계에 맞는 연출/결과를 열고, 기록되지 않은 성과/통계는 없음을 표현한다. Playing으로 돌리거나 목표량만큼 성과를 채우지 않는다.
목표/Results 저장 실패는 연구소/외부 정지를 해제하지 않는다. 현재 화면은 실제 메모리 단계와 저장 오류를 표시하고 재시도할 수 있어야 한다. 정상 파일에는 마지막 성공 단계가 남으며 재실행은 그 단계에서 시작한다.
재시작 파일 교체 실패 때 새 판을 먼저 공개하지 않는다. 복원 실패도 기존 세션/화면을 바꾸거나 자동 새 게임으로 대체하지 않는다. 읽기 실패·쓰기 실패·상태 전환 성공을 UI에서 혼동하지 않는다.

## 작업 목록

- [x] 실행 승인 후 유일한 active 전환·선행 기록/작업 트리 확인, 연출/전환/결과 조회·복원 정책 설계
- [x] 간단한 멸망 연출과 시간 입력·앱 정지·목표 진입 연결
- [x] 완료 신호의 세션/단계 검사·중복/지연 신호 방지·편집/배경 입력 처리
- [x] 결과 정보·다시 시작 UI와 공통 명령 연결, 실패 표시·재시도·새 판 재연결
- [x] Destruction/Results 복원·자동 전환 저장·v4/구버전 호환·실패 보존 검증
- [x] 실제 신규 시작→전체 한 판→결과→재시작→다음 판 종료 반복 및 PC 화면 검증
- [x] 영향받은 Edit/Play 회귀·GUID/참조 확인, 원본 실패/재검증·기술 결정·PLAN·로드맵 기록
- [x] Sprite 요청 유무와 3단계 완료/4단계 잔여 범위 정리

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 실제 한 판 연결 | 실제 초기 상태의 수집·유료 건설·가공·배양·방출로 목표 도달 후 연출→결과→재시작 | 공통 명령·상태 추적·실제 화면, 강제 목표/무료 지급 없이 완료 |
| 연출 중 정지 | 목표 직전/직후, 건설·이동/회전 편집 중 목표, 연출/결과 시간 진행 | 연구소/외부 진행·성과·보고·재고·예약 불변, 편집 확정 부작용 없음 |
| 연출 시간 | 정상 프레임, 큰 게임 delta로 목표 진입, 완료 직전/정확한 경계/초과 연출 delta, 앱 정지/재개 | 실제 연출 시작/중간/완료 관찰, 남은 게임 시간 재사용·오프라인 보충 없음 |
| 완료 신호 자격 | Playing/Results의 완료 신호, 동일 신호 반복, Reload/Restart·View 교체 후 오래된 신호 | 올바른 Destruction 한 번만 Results 전환, 새 세션 오염 없음 |
| 결과 정보 | 목표 초과 수치, 보고 생략/보관 한도·반복 열람, 이관된 통계 없는 종료 기록 | 확정 상태와 같은 값, 성과 재계산/가짜 통계/이전 판 혼입 없음 |
| 두 자동 저장 경계 | ReachGoal 및 실제 연출 완료 뒤 추가 Save 없이 파일을 새 객체로 복원 | 마지막 활동/보고/성과 보존, Destruction/Results의 올바른 화면과 정지 유지 |
| 종료 단계 이어하기 | 연출 직전/중간 저장·앱 종료·재시작, Results 저장, v4 및 v2/v3 종료 이관 후 재저장 | Destruction 재생 정책·Results 직행·단계/목표 검사·무진행·진단 슬롯 분리 |
| 저장/복원 실패 | 목표 저장·Results 저장·재시도·Reload·재시작 파일 교체 실패 주입 | 정상 파일/현재 세션 보존, 종료 후 진행 재개 없음, 오류와 재시도 표시 |
| 결과 재시작 | 결과 버튼 1회/연속/지연 요청, 실패 후 재시도, 성공 직후 저장을 새 객체로 복원 | 새 ID/관계, 프로토타입 1·유기물 4·시설/침공형 0·성과/보고/시간 초기화, 옛 콜백 무효 |
| 반복 완결 | 첫 결과에서 다시 시작하여 두 번째 판도 실제 생산→목표→연출→결과까지 진행 | 두 번째 판 독립 상태·종료 화면·파일, 이전 판의 선택/편집/연출/구독 잔류 없음 |
| 화면·공통 경로 | 실제 씬의 연출·결과·다시 시작·실패 표시, 같은 명령/시간 입력의 하네스 비교 | PC 가로 캡처·프레임/OnGUI·상태 비교·콘솔·SO/씬 참조 |
| 회귀 | 건설/생산/배양/물류/배정·방출/보고·큰 delta·저장 v4/구버전·기존 진단 Edit/Play | 실제 실행 수·통과/실패/건너뜀·원본 JSON/XML·소스 식별·GUID 보존 |

전체 흐름 검증에서 `ReachGoal`/`FinishDestruction` 직접 호출로 실제 성과나 연출을 건너뛰지 않는다. 반복 검증은 3단계 연결의 재현 확인이며 4단계의 종합 밸런스/성능 완료를 대신하지 않는다.
희귀 경계의 격리 fixture와 실제 시작 검증을 구분한다. 하네스는 실제 갱신/전환 로직을 사용하고 시간 가속·대기에 상한을 둔다. 렌더링 확인은 공통 로직 테스트로 대신하지 않는다.
실행 후 명령·환경·실패 원본·최종 결과·화면·소스 식별을 `docs/plans/evidence/PLAN-011/`에 기록한다. 테스트 저장 슬롯은 실제 `Gameplay/session.json`과 분리한다.
완료 판정은 이 범위의 구현·필수 검증을 충족한 뒤 한다. 그때 로드맵 3단계 완료를 갱신하며, D-070 전체 완료·Android 실기기·세로 대응 완료로 표현하지 않는다.

## 아트와 완료 판정

`development`이며 기존 Temp 표현으로 기능을 검증할 수 있다. 정식 게임 아트는 사용자가 제공하고, 미수령/시각 확인 대기와 기능 완료를 구분한다.
새 임시 에셋은 영어 `Temp_`와 교체 위치를 기록한다. Sprite가 필요하면 PPU 32 등 아트 지침을 적용하고 `UnityProject/Assets/Sprites/Require/PLAN-011/README.md`에 요청을 작성해 연결한다.
실행 결과 새 Sprite 요청은 없다. 기존 Temp 도형·문구를 사용했으며 Require 폴더나 정식 아트 파일을 추가하지 않았다.

## 현재 상황과 다음 행동

- 2026-10-28: PLAN-001~010 completed·active=0을 확인했다. PLAN-010의 결과와 실제 GameFlow/View/저장 경계를 읽고 이번 후보 범위를 정리했다.
- 2026-10-28: 사용자 실행 승인으로 유일한 active로 전환했다. 로드맵의 PLAN-010 completed 표기를 정리하고 PLAN-011 목록과 README 상태를 동기화했다. PLAN 관리 지침에 상태 전환 시 로드맵·README 동시 반영을 명시했다.
- 구현: [TECH-008](../decisions/TECH-008-run-ending-presentation.md)에 4초의 독립 연출 시계·프레임 상한·화면 연결/요청 식별·종료 화면·기존 v4 복원 정책을 기록했다. 새 Sprite 파일 없이 기존 Temp 도형 표현을 확장하며 `LaboratoryView.Ending.cs`의 세계 도형이 교체 위치다.
- 완료: Edit 212/212·최종 전체 Play 10/10·콘솔 오류/경고 0. 실제 두 판의 128/120 성과·연출·결과·재시작과 파일 교체 실패를 검증했다. 작은 가로 창의 오류/재시도 버튼 잘림도 보완 후 캡처와 전체 Play로 재검증했다.
- [실행 증거](evidence/PLAN-011/README.md)에 원본 실패·재검증·최종 화면·소스 SHA-256 364개를 기록했다. 기존 meta 198개 변경/누락 0, 신규 C# meta 누락 0. 새 Sprite 요청 없음.
- 로드맵 3단계 완료. 다음 작업은 별도 PLAN 작성/승인이 필요한 4단계 세로 대응·병목/반복 종합 검증·Android 빌드/실기기다. 후속 PLAN을 자동 생성하거나 활성화하지 않는다.
- 사전 기획상 막힌 핵심 결정 없음. 구현 상세는 D-059 범위에서 결정하며 모바일 최종 검증과 범위를 혼합하지 않는다.

## 검증 결과

| 날짜 | 대상·환경·실행 방법 | 기대 결과·실제 결과 | 판정·증거 |
| --- | --- | --- | --- |
| 2026-10-28 | 작성 전 `node scripts/validate-agent-docs.mjs` | PLAN 10개·목록·지침 줄 수·로컬 링크 1319개 정상, active=0 조건만 오류 | 종료 코드 1. 선행 완료 후 후보 작성 단계이며 기능 작업 미실행 |
| 2026-10-28 | 후보·목록·기획 MAP·로드맵 작성 후 문서 검사와 `git diff --check` | PLAN 11개·목록 일치·지침 줄 수·로컬 링크 1355개·공백 검사 정상, active=0 조건만 오류 | 종료 코드 1은 실행 승인 대기. PLAN-011 proposed·기존 10개 completed 유지. Unity 기능 검증 미실행 |
| 2026-10-28 | 실행 승인·상태/로드맵/README·관리 지침 반영 후 문서 검사 | 유일한 active=PLAN-011, PLAN 11개·로컬 링크 1359개·목록/줄 수 정상 | 종료 코드 0 |
| 2026-10-28 | 기존 Editor·첫 구현 컴파일·전체 Edit | 컴파일 성공, 212/212·실패/건너뜀 0 | `evidence/PLAN-011/2026-10-28-edit-1-status.json`·동명 XML, 화면 연결 추가 보강 후 재검증 중 |
| 2026-10-28 | 화면 연결 식별 보강 후 전체 Edit | 212/212·실패/건너뜀 0 | [JSON](evidence/PLAN-011/2026-10-28-edit-2-status.json)·[XML](evidence/PLAN-011/2026-10-28-edit-2.xml) |
| 2026-10-28 | 전체 Play 첫 실행→활성 상태 준비→화면 보완 재실행 | 6/10(4 실패) → 10/10 → 최종 10/10, 건너뜀 0 | [원인·모든 원본·최종 화면](evidence/PLAN-011/README.md) |
| 2026-10-28 | 최종 콘솔·Editor·meta·소스 확인 | 오류/경고 0·Play 종료·198 meta 유지·소스 364개 식별 | [콘솔](evidence/PLAN-011/2026-10-28-final-console.json)·[meta](evidence/PLAN-011/2026-10-28-meta-audit.json)·[소스](evidence/PLAN-011/2026-10-28-source-manifest.json) |
| 2026-10-28 | completed 전환·로드맵/README 동기화·문서/공백 검사 | PLAN 11개 completed·목록/링크/줄 수 정상·공백 오류 없음. active=0만 종료 코드 1 | [최종 문서 검사](evidence/PLAN-011/2026-10-28-docs-completed.txt). 후속 실행 승인이 없어 새 active를 만들지 않음 |

문서 검사와 기능 검증을 구분한다. PLAN-010의 201/8 결과를 새 연출·결과 화면의 통과로 기록하지 않는다.

## 관련 결정과 상태 전환

- 2026-10-28: 사용자 PLAN-011 작성 요청에 따라 최초 생성 → `proposed`. 실행·활성화 승인 없음.
- 2026-10-28: 사용자 “PLAN-011 실행을 승인할게”에 따라 `proposed → active`. 이번 승인 범위에서 구현·검증을 시작한다.
- 2026-10-28: 구현·필수 검증·실제 화면/두 판 반복을 충족하여 `active → completed`. PLAN 목록·기획 MAP·로드맵의 현재 작업과 작업 PLAN 연결을 함께 갱신한다.
- TECH-004·TECH-007의 저장/단계 계약을 유지한다. 연출의 일시 상태·완료 신호 자격·결과 조회·복원 정책은 실행 시 기술 결정으로 남긴다.
- 이번은 로드맵 3단계의 잔여 종료 연결이다. 후속 4단계 PLAN은 별도 작성·승인을 거치며 이번 실행만으로 생성/활성화하지 않는다.

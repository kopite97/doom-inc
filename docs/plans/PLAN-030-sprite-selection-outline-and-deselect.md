---
id: PLAN-030
type: development
status: completed
title: Sprite 외곽선 선택 강조와 빈 땅 클릭 선택 해제
updated: 2026-11-15
---

# PLAN-030 Sprite 외곽선 선택 강조와 빈 땅 클릭 선택 해제

## 목표

시설·생명체를 선택하면 All In 1 Sprite Shader로 현재 Sprite의 투명도 경계를 강조한다. 일반 지도 조작 중 빈 땅을 클릭·탭하면 선택 상태와 강조가 함께 해제된다.

## 범위

- 포함: 기존 노란 선택 사각형을 본체 Sprite 외곽선으로 교체, 선택 변경·해제·대상 제거의 표시 동기화, 빈 땅 클릭·탭의 선택 해제와 선택 UI 정리.
- 포함: 프로젝트 소유 선택 재질·설정 참조, 기존 SpriteRenderer 연결, 에셋 복원·Git 제외 경계 확인, 영향받는 입력·표현의 일반 Editor 검증.
- 제외: 배치 가능·불가를 표시하는 초록·빨강 사각형 교체, 클릭 판정의 알파 픽셀 단위 변경, 운반물까지 합친 통합 실루엣, 새 Sprite 제작, 다른 Shader 효과·펄스·Bloom·조명 도입.
- 제외: 게임 저장 형식·시뮬레이션 규칙 변경, 렌더 파이프라인 일괄 변경, Android/Player 빌드·실기기·고부하·성능 계측.
- 작업 요청 근거: 2026-11-15 사용자가 All In 1 Sprite Shader로 선택 강조를 바꾸고 땅 클릭 시 선택을 해제하는 PLAN 작성을 요청했다. 후보 작성 뒤 사용자가 같은 날 PLAN을 승인하고 작업 시작을 요청했다.

## 기준 문서와 선행 조건

- 제품 규칙: [화면과 조작](../game-design/03-player-experience/03-interface/controls.md#selection-feedback), [시설 배치](../game-design/02-systems/03-space/placement.md), [Thing과 정의 데이터](../game-design/05-development/01-architecture/thing-model.md).
- 로드맵: [구현 로드맵](../game-design/05-development/02-implementation/roadmap.md)의 프로토타입 이후 인게임 경험 개선 후보다. 아트 5-2의 잔여 작업이나 완료 판정을 대신하지 않는다.
- 선행: [PLAN-027](PLAN-027-laboratory-tilemap-and-sprite-rendering.md)의 SpriteRenderer 표현, [PLAN-028](PLAN-028-creature-movement-interpolation.md)의 이동·입력 동기화, [PLAN-029](PLAN-029-laboratory-processing-cultivation-storage-art.md)의 시설 아트 적용 완료.
- 필수 지침: [작업 절차](../agents/workflow.md), [PLAN 관리](AGETNS.md), [프로젝트 구조](../agents/project-structure.md), [명명](../agents/naming.md), [스크립트 구조](../agents/script-structure.md), [객체 설계](../agents/object-design.md), [추상화](../agents/abstraction.md), [아키텍처](../agents/architecture.md).
- 표현·검증 지침: [연출 MAP](../agents/presentation-effects.md), [All In 1 Sprite Shader](../agents/external-assets/all-in-1-sprite-shader.md), [Unity 에셋](../agents/unity-assets.md), [아트](../agents/art-assets.md), [하네스](../agents/harness.md), [Unity 도구](../agents/unity-tools.md), [검증](../agents/verification.md), [저장소](../agents/repository.md).
- 4.71을 Import했고 URP 자동 구성은 Cancel했다. 실제 재질 렌더링·빌드 호환성을 확인한 것으로 간주하지 않는다.
- 신규 Sprite 요청은 없다. 기존 아트·임시 Sprite를 사용하며 Require 폴더를 생성하지 않는다.

## 착수 시 확인한 구현

| 위치 | 현재 동작과 변경 지점 |
| --- | --- |
| [LaboratoryMapView.Art](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.Art.cs) | 본체 프레임을 그린 뒤 SelectedId 비교로 노란 DrawBorder를 호출한다. 같은 DrawBorder는 배치 미리보기에도 사용하므로 선택 표시만 분리한다. SortingGroup 활성 조건도 Border 상태에 의존하므로 함께 검토한다. |
| [LaboratoryWorldMap](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryWorldMap.cs) | 바닥·Sprite 생성에서 공유 월드 재질을 사용한다. 원본 재질 수정으로 선택을 표시하면 다른 대상까지 영향을 받는다. |
| [LaboratoryMapView](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.cs) | Tap은 UI·지도 유효성, 상태 마커, 배치 모드를 먼저 처리한다. 선택 후보가 0개면 그대로 반환해 기존 선택이 남는다. |
| [LaboratoryView](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.cs)·[Navigation](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Navigation.cs) | SelectedId와 선택 패널·후보 목록·입력 무효화·선택 대상 지도 이동을 관리한다. 해제도 이 소유 경계에서 일관되게 처리해야 한다. |

## 구현 설계

### 선택 표현

- 선택 상태의 원본은 기존 SelectedId로 유지한다. 화면 전용 강조 상태를 별도 게임 상태나 저장 데이터로 추가하지 않는다.
- 현재 Unlit 표현에 맞춰 기본 AllIn1SpriteShader 변형을 우선 검증한다. OUTBASE_ON·OUTBASEPIXELPERF_ON을 저장 재질에 설정하고 본체와 외곽선을 함께 표시한다.
- 초기 시각안은 밝은 청록 계열·원본 기준 1픽셀·Glow 1이다. 색·두께·4/8방향은 현재 바닥과 아트에서 식별성을 확인해 조정하는 개발 위임값이며 확정 색상 규칙이 아니다.
- 선택된 본체에 준비된 외곽선 재질을 할당하고 해제 시 기존 재질로 복원하는 방식을 우선한다. 선택 전용 재질을 불변으로 공유할 수 있으면 개체마다 재질을 복제하지 않는다. 개별 값이 필요하면 적용 가능한 MPB 또는 수명 관리되는 인스턴스를 검토하고 최종 방식을 기록한다.
- 바닥·다른 대상·운반물·상호작용 칸·상태 마커의 재질과 표시를 보존한다. 선택 사각형 전용 생성·갱신은 정리하되 배치 미리보기 사각형과 정렬을 유지한다.
- 현재 Sprite 프레임·flip·이동·가림 순서를 그대로 이용한다. 단색 Outline만을 위해 SetAtlasUvs나 자동 시간 컴포넌트를 추가하지 않는다.
- 프레임 가장자리 여백·클리핑·이웃 프레임 오염을 확인한다. Full Rect·PPU·Point·Clamp 등 기존 임포트 기준과 원본 아트를 유지하며 문제가 있으면 원인과 최소 조정안을 기록한다.
- 임시 사각형 Sprite는 투명도 경계 자체가 사각형이므로 외곽선도 사각형일 수 있다. 별도 노란 선택 사각형을 남기는 것으로 대체하지 않는다.

### 빈 땅 선택 해제

- 일반 지도 모드에서 UI에 가려지지 않은 지도 내부의 유효한 땅 클릭·탭이고, 상태 마커·시설·생명체의 기존 선택 판정에 걸리지 않으면 선택을 해제한다.
- 강조만 숨기지 않고 SelectedId를 비우며 선택 상세·요약·선택 후보 UI를 정리한다. 진행 중인 선택 대상 자동 지도 이동과 오래된 선택 입력도 취소한다.
- 선택과 무관한 메뉴를 임의로 닫는 동작은 추가하지 않는다. UI 버튼·패널 클릭, 지도 밖 클릭, 드래그·핀치·휠 확대는 빈 땅 탭으로 처리하지 않는다.
- 시설 배치·이동·회전 모드에서는 기존 위치 미리보기·확정·취소 흐름이 우선한다. 땅 클릭이 편집 대상 선택을 해제하지 않게 한다.
- 다른 대상 선택 시 이전 강조를 제거하고 새 대상만 강조한다. 동일 대상 클릭과 겹친 대상 후보 선택의 기존 규칙은 유지한다.
- 대상 제거·외부 활동으로 지도에서 사라짐·화면 종료·세션 교체 시 존재하지 않는 본체의 강조나 재질 참조가 남지 않게 한다. 외부 활동 대상의 기존 목록·상세 기능은 보존한다.

### 책임 분리와 자산 관리

- 근거/공통 책임: 시설과 생명체 모두 기존 ThingArt의 본체를 가지므로 선택 재질 적용·복원은 같은 경로에서 처리한다. 선택 해제는 View의 선택 상태·UI 정리를 한곳에서 담당한다.
- 달라지는 부분: Sprite 프레임·외형과 배치 미리보기는 기존 기능에 남긴다. 범용 효과 관리자·인터페이스·싱글톤은 만들지 않고 작은 메서드나 필요한 구체 객체로 시작한다.
- 줄어들 작업/검증: 대상 종류마다 외곽선 로직을 복제하지 않고 동일 선택 전환 검증으로 값 독립성과 복원을 확인한다.
- 프로젝트 소유 재질은 Assets/Materials/Laboratory, 연결 코드는 기존 Scripts/Presentation/Gameplay/Laboratory 책임 안에 둔다. 기존 설정/SO 연결점을 조사해 재질 참조를 직렬화하고 파일명 기반 런타임 검색은 피한다.
- 외부 원본은 수정하지 않는다. AllIn1Assets와 공통 의존성의 정확한 원본·meta 제외 경계 및 새 체크아웃 복원 안내를 저장소 지침과 대조해 정리한다. 게임용 재질·설정·연동 코드와 meta는 추적한다.

## 작업 목록

- [x] 사용자 요청·기획·현재 코드·에셋 사용 지침을 확인하고 proposed PLAN과 목록을 작성한다.
- [x] 승인 후 유일한 active PLAN으로 전환하고 코드·재질·원본 에셋 경계를 재확인한다.
- [x] 프로젝트 소유 외곽선 재질·설정 참조를 만들고 시설·생명체 본체의 선택 표시를 교체한다.
- [x] 빈 땅 입력에서 선택 해제·선택 UI 정리·지도 자동 이동 중단을 연결한다.
- [x] 선택 변경·편집·후보 선택·지도 제외·대상 제거·화면 재사용의 표시 수명을 정리한다.
- [x] 필요한 일반 Editor 기능 테스트와 시각 확인을 수행하고 증거·한계를 기록한다.
- [x] 완료 기준 충족 후 PLAN·목록·로드맵의 상태를 갱신한다.

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 | 필요한 증거 |
| --- | --- | --- |
| 시설·생명체의 본체에만 외곽선이 표시되고 기존 노란 선택 사각형은 없음 | 현재 시설 4종·애니메이션 프로토타입·임시 생명체를 선택 전환하고 실제 Editor 화면 확인 | 적용 재질·키워드 확인, 대표 화면과 관찰 기록 |
| 빈 땅 클릭·탭으로 선택 상태·강조·선택 UI가 해제됨 | 대상 선택 → 빈 땅 → 재선택, 겹친 후보 UI → 빈 땅, 선택 자동 지도 이동 중 해제 | SelectedId·UI·표시 결과의 Play Mode 기능 테스트 및 관찰 기록 |
| 입력 우선권과 기존 동작 보존 | UI 클릭·드래그·핀치·휠·상태 마커·겹친 대상·배치/이동/회전 조작, 가로·세로 화면 | 기존 Input·InputOperations·ReadingContext 테스트 중 영향 범위와 필요한 추가 사례 |
| 프레임·이동·줌에서 외곽선이 본체를 따름 | 방향·프레임 전환, 이동, 줌 범위, 겹침·가림, 정지·배속을 실제 렌더링으로 확인 | Art·World 관련 회귀 결과, 잘림·시트 오염·정렬의 시각 관찰 |
| 공유 재질 오염과 수명 잔류 없음 | 선택 A → B → 해제, 대상 지도 제외·삭제, 화면 재진입·세션 교체 | 다른 본체·바닥·운반물 표시 보존, 생성 재질의 소유·정리 확인 |
| 문서·에셋 참조 일관성 | 문서 검사, diff 검사, Unity 임포트·컴파일·참조 확인 | 실행 결과·증거 경로·Console 오류 여부 |

- 테스트는 관찰 가능한 선택·UI·표시 결과를 검증하고 구현 메서드 분할 자체를 정답으로 고정하지 않는다. 기존 Laboratory 테스트를 활용해 필요한 범위만 추가·실행한다.
- development PLAN이다. 정식 Sprite 추가 제작은 없고 최종 기술 검증에 실제 화면 확인을 포함한다. 색·두께 조정 결과와 미확인 범위를 기록한다.
- 빌드·실기기·고부하 테스트는 요청되지 않아 필수 검증에서 제외한다. Editor 통과를 Android 셰이더 변형·실기기 표시·성능 통과로 표현하지 않는다.

## 현재 상황과 다음 행동

- 현재 상태: 구현·일반 Editor 검증·실제 렌더링 확인을 완료해 completed로 전환했다. active PLAN은 0개다.
- 다음 행동: 이 PLAN의 필수 잔여 작업은 없다. 별도 후속 요청 전에는 다음 PLAN을 활성화하지 않는다.
- 한계: Player/Android 빌드·실기기·고부하·성능 계측은 범위 밖으로 실행하지 않았다. Editor 통과를 해당 환경 통과로 확대하지 않는다.
- 신규 Sprite 요청은 없다. 임시 본체의 투명 여백은 기존 단색 임시 표현을 코드로 생성한 것이며 Require 폴더를 만들지 않았다.

## 최종 구현 선택

- `LaboratorySelection.mat`에 기본 Unlit 셰이더와 `OUTBASE_ON`, `OUTBASEPIXELPERF_ON`, `OUTBASE8DIR_ON`을 저장했다. 밝은 청록 RGBA(0.25, 0.95, 1, 1), 원본 텍스처 기준 1픽셀, Alpha·Glow 1이다. 별도 펄스·Bloom은 없다.
- `LaboratoryUiAssets.SelectionMaterial`을 AppConfiguration과 연구소 씬에 직렬화했다. ThingArt 본체만 불변 공유 재질로 교체하고 해제 시 생성 당시 재질로 복원한다. 개체별 재질 인스턴스·MPB·Atlas UV 컴포넌트는 필요하지 않았다.
- 선택 사각형은 생성하지 않는다. 배치 미리보기만 `PlacementBorder_`를 유지하며 SortingGroup 조건도 이에 맞췄다. 운반물·바닥·상태 마커·상호작용 칸은 기존 재질을 사용한다.
- 기존 whiteTexture 임시 본체는 투명 경계가 없어 외곽선이 나오지 않았다. 32×32 불투명 영역과 1픽셀 투명 여백을 가진 34×34 Point/Clamp 임시 텍스처를 화면당 하나 생성한다. 불투명 본체의 월드 크기·기존 클릭 범위를 유지하고 화면 종료 시 Sprite와 텍스처를 해제한다.
- 빈 땅 Tap에서 View의 ClearSelection을 호출한다. 선택 ID·후보·선택 패널을 정리하고 입력 무효화로 자동 지도 이동을 중단한다. 편집 모드와 UI 우선권을 보존하며 선택과 무관한 메뉴는 닫지 않는다. Back·대상 삭제도 공통 선택 정리를 사용한다.
- 외부 소스는 수정하지 않았다. AllIn1Assets 원본과 meta를 Git에서 제외하고 4.71 복원 방법을 저장소 지침에 기록했다. 공급사 RenderPipelineChecker가 추가한 Android `ALLIN1_URP` define은 유지했다. 렌더 파이프라인 자동 구성은 실행하지 않았다.
- 자세한 실행 결과·화면·제약은 [검증 증거](evidence/PLAN-030/README.md)를 따른다.

## 검증 결과

| 날짜 | 대상·환경·실행 방법 | 기대 결과·실제 결과 | 판정·증거 |
| --- | --- | --- | --- |
| 2026-11-15 | 현재 선택·입력 소스의 정적 조회 | 노란 DrawBorder 호출과 후보 0개 시 반환 경로 확인 | 조사 완료. 위 현재 구현의 소스 링크 |
| 2026-11-15 | `node scripts/validate-agent-docs.mjs`, `git diff --check` | 문서 검사 PASS: PLAN 30개·active 0개·목록 30개·로컬 링크 3452개, diff 공백 오류 없음 | 통과. 문서 검증만 수행 |
| 2026-11-15 | Unity 실행·재질 적용·기능 테스트 — 후보 작성 시점 | 당시 실행하지 않음 | 후보 작성 이력; 아래 구현 후 결과와 구분 |
| 2026-11-15 | Unity 6000.3.27f1·URP 17.3·All In 1 Sprite Shader 4.71, 일반 Play Mode 선택/입력/이동/시설/월드/바닥 검증 | 서로 다른 테스트 27/27 통과. 선택 신규 3개를 Domain Reload ON에서도 재실행해 3/3 통과 | [결과·필터·원본 로그](evidence/PLAN-030/README.md) |
| 2026-11-15 | 실제 Game View 캡처 12장·셰이더 상태·현재 Console | 시설 4종·생명체 2종의 청록 외곽선, 가로/세로 해제, 줌 1/8 확인. 현재 오류·경고 0, 셰이더 메시지 없음 | [화면·환경](evidence/PLAN-030/README.md) |
| 2026-11-15 | 완료 문서·에셋·환경 복원 확인 | PLAN/목록/로드맵 completed, active 0. Editor 정지·Domain Reload OFF·runInBackground false 복원 | 문서 검사와 diff 검사, [환경 로그](evidence/PLAN-030/final-environment.json) |

## 관련 결정과 상태 전환

- 기존 [TECH-018](../decisions/TECH-018-creature-movement-interpolation.md)의 이동·입력 동기화를 보존한다. 이번 선택 재질·해제 처리의 세부 선택은 이 PLAN에 기록한다.
- 2026-11-15: 신규 → proposed. 사용자 PLAN 작성 요청에 따라 생성했으며 활성화하지 않았다.

- 2026-11-15: proposed → active. 사용자: “PLAN을 승인할게 작업 시작해줘.” 빌드·고부하 제외 범위를 유지한다.
- 2026-11-15: active → completed. 범위 안의 구현·일반 Editor 검증·화면 확인을 완료했다. 다음 PLAN은 활성화하지 않는다.

# 구현 순서와 선행 기반 작업

- 문서 ID: GD-IMPLEMENTATION-ROADMAP
- 분류: 개발 구조 > 구현 계획 > 구현 순서
- 상태: 선행 기반 세 묶음·1~4단계 및 D-070 완료 / 아트 5-1 완료 / PLAN-001~029 completed / PLAN-029 art·completed / active 0개
- 검색어: 개발 순서, 구현 순서, 로드맵, 기반 시스템, 선행 기반, 완료 기준, 데이터 구조, Thing, 컴포넌트, SO, Unity, GameFlow, 하네스, 저장, 복원, CLI, MCP, 에이전트, 개발 결정, 검증 기록
- 근거: S2 / D-059·D-071·D-072~D-090 — [출처](../../00-management/04-evidence/sources.md), [결정 기록](../../00-management/02-decisions/record.md)
- 갱신일: 2026-11-12
- 탐색: [기획 MAP](../../README.md), [Thing과 정의 데이터](../01-architecture/thing-model.md), [GameFlow와 시스템 책임](../01-architecture/game-flow.md), [플레이 환경](../../04-delivery/01-platform/environment.md)

## 현재 작업

현재 active PLAN은 0개다. 최근 완료 [PLAN-029 가공시설·배양시설·저장소 정식 아트 제작·적용](../../../plans/PLAN-029-laboratory-processing-cultivation-storage-art.md)은 `art` / `completed`다. 세 시설 r001·총 48프레임과 생산/입출고 동작·미리보기/목록 연결의 전체 적용, 일반 Editor 검증, 2026-11-12 사용자 게임 화면 승인을 마쳤다. 빌드·고부하·실기기는 제외했다. 바닥 외곽 등 5-2의 다른 후보와 전체 5-5는 별도 후속 범위다.

[PLAN-025 최신 버전 Android 통합 검증과 프로토타입 완료 판정](../../../plans/PLAN-025-android-integration-and-prototype-completion.md)은 completed다. Editor 회귀와 승인된 Android 통합 검증·환경 복원을 마쳐 4단계·D-070의 네 기준을 충족했다. SM-G977N 한 기종·현재 임시 표현의 판정이며 세부 측정 한계와 경미한 표시 결함은 [완료 근거](../../../plans/evidence/PLAN-025/README.md#d-070-완료-판정)를 따른다. PLAN-026~029도 완료했으며 현재 active는 0개다.

2026-11-07 사용자 요청으로 [정식 아트 제작·적용 로드맵](art-roadmap.md)을 후속 **5단계**로 추가했다. 샘플·연구소·생명체/자원·UI·동작·타이틀/종료·통합 검수 순서를 제안하며 2026-11-10 첫 5-1 샘플 묶음의 전체 적용·Editor 검증·사용자 시각 확인을 완료했다. PLAN-025의 표시/GC 개선 후보는 잠시 미뤄 두고, 완료된 프로토타입 판정과 아트 적용 완료를 구분한다.

5-1의 [PLAN-026 스타일·크기 샘플과 연구소 적용](../../../plans/PLAN-026-art-style-scale-and-sample-integration.md)은 `art` / `completed`다. 바닥 4개 포함 5개 분류·8개 샘플, PNG 5개·106 Sprite의 적용을 마쳤다. 획득시설 36프레임·보조형 4방향 64프레임·머리 위 운반·Build 교체를 전체 준비 후 검증하고 사용자가 실제 게임 화면을 승인했다. [완료 증거](../../../plans/evidence/PLAN-026/integration-2026-11-10/README.md)를 기준으로 5-1 완료를 판정하며 모바일 빌드/실기기는 이번에 실행하지 않았다.

후속 표현 기반 완료 [PLAN-027 연구소 바닥 Tilemap·시설과 개체 SpriteRenderer 전환](../../../plans/PLAN-027-laboratory-tilemap-and-sprite-rendering.md)은 `development` / `completed`다. 2026-11-10 사용자 승인 후 바닥 Tilemap·시설/개체 SpriteRenderer·UI uGUI 전환과 Editor 검증을 완료했다. 원본 아트·게임 상태를 보존했다.

[PLAN-028 연구소 생명체 이동 보간과 표시 동기화](../../../plans/PLAN-028-creature-movement-interpolation.md)은 `development` / `completed`다. 이동 보간·표시/입력 구현과 기존 Edit 410/410·Laboratory OFF/ON 각각 46/46·앱 씬 29/29 증거를 확인했다. 빌드·고부하 제외 재개 요청에 따라 현재 소스와 검증 당시 상태의 동일성을 대조하고 추가 계측 없이 기능 완료를 판정했다. 전체 GC/RenderLoop 증가·active CPU 전후 미측정 한계는 유지한다.

[PLAN-024 침공형 방출 성공 피드백과 화면 밖 시설 선택 시 지도 이동](../../../plans/PLAN-024-release-feedback-and-map-reveal.md)은 completed다. 두 연출의 연결·입력 우선권과 연구소 Editor 회귀를 검증했다.

[PLAN-023 로딩 진행률·Feel 연출과 전체 게임 UI 물마루 TMP 전환](../../../plans/PLAN-023-loading-progress-feel-and-mulmaru-tmp.md)은 completed다. 실제 씬 진행률과 기본 1초 로딩, Feel 웨이브, 물마루 Dynamic TMP를 Editor에서 검증했다.

[PLAN-022 약탈형 배양·침공과 연구소 자원 수신](../../../plans/PLAN-022-raider-and-resource-receiving.md)은 2단계 배양·물류와 3단계 침공을 연결하는 확장 작업을 완료했다.

[PLAN-020 Editor 런타임 운영 병목·반복 플레이·성능 검증](../../../plans/PLAN-020-editor-bottlenecks-repeated-play-and-performance.md)은 completed다. 운영 병목·3판 반복·장시간 실행과 고부하 지연을 Editor에서 검증했다. 작업자 탐색 최적화 및 CSV 회귀 수정은 PLAN-021에서 완료했다. 최신 Android 검증과 4단계·D-070 최종 판정은 PLAN-025에서 완료했다.

환경 준비 결과와 도구 호환 주의점은 [PLAN-002](../../../plans/PLAN-002-unity-environment.md)에 있다. 정의·Thing 최소 기반은 [PLAN-003](../../../plans/PLAN-003-thing-foundation.md), 공간·공통 조작·최소 화면은 [PLAN-004](../../../plans/PLAN-004-map-and-commands.md), 한 판·저장·복원 기반은 [PLAN-007](../../../plans/PLAN-007-gameflow-and-persistence.md)에서 구현·검증했다. 세 기반 묶음과 1~3단계를 완료했다. 4단계의 세로 UI·모바일 입력과 당시 APK·직접 기기 검증은 PLAN-013·014, Editor의 운영 병목·반복·성능 검증과 개선은 PLAN-020·021에서 완료했다. 최신 Android 기준의 종합 판정은 PLAN-025에서 완료했다.

1단계의 초기 건설·수집·운반·입고 흐름과 중간 저장·복원·재시작은 [PLAN-008](../../../plans/PLAN-008-initial-construction-and-logistics.md)에서 구현·검증했다. PC 최소 플레이 화면과 하네스 검사를 통과했다. 후속 단계는 별도 proposed PLAN을 작성하고 사용자 실행 승인을 받은 뒤 진행한다.

2단계의 가공·배양·보급형 운용은 [PLAN-009](../../../plans/PLAN-009-processing-cultivation-and-workers.md)에서 구현·검증했다. Manager 책임·명명 정비와 기존 회귀 검증을 먼저 수행한 뒤 생산·배정·두 자원 물류·저장 복원·PC 가로 화면을 연결했다. 완료 결과는 해당 PLAN의 검증 기록을 따른다.

3단계의 첫 작업 [PLAN-010](../../../plans/PLAN-010-invasion-activity-and-reports.md)은 completed다. 방출·외부 활동·성과 누적·전과 보고와 저장 복원, 목표 도달 시 진행 정지·GameFlow 전환을 구현하고 Edit 201/201·Play 8/8과 PC 화면을 검증했다. 검토에서 보완한 자동 목표 저장 확정 순서·단계/목표 교차 검증과 구버전 종료 이관도 포함한다. 잔여 종료 연결은 PLAN-011에서 이어 완료했다.

개별 작업 상태·다음 행동·검증 증거는 [PLAN MAP](../../../plans/README.md)에서 찾는다. 작업 대상은 유일한 active PLAN이며, 이 문서는 전체 단계·기획상 완료 기준과 요약 상태를 관리한다.

3단계의 잔여 종료 연결 [PLAN-011](../../../plans/PLAN-011-destruction-results-and-restart.md)은 completed다. 간단한 멸망 연출·결과 화면·다시 시작, 종료 단계 저장/복원·실패 보존과 PC 가로의 실제 두 판을 Edit 212/212·Play 10/10·오류/경고 0으로 검증하여 3단계를 완료했다. [실행 증거](../../../plans/evidence/PLAN-011/README.md)를 따른다. 4단계의 세로·모바일 직접 검증은 아래 PLAN-013에서 완료했고, 종합 검증은 이후 승인된 PLAN-025에서 완료했다.

데이터 기반 정비 [PLAN-012](../../../plans/PLAN-012-csv-so-data-and-game-data-store.md)는 completed다. CSV와 주제별 Registry SO를 결합한 Data를 초기 로딩하여 영속 GameDataStore에서 공유하고, 취득부 교체와 저장 호환성 검사를 구현했다. Edit 236/236·Domain Reload 양쪽 Play 각 13/13·Android 스크립트 컴파일과 CSV 참조 검사를 통과했다. 이후 PLAN-013의 완료 결과는 아래와 같다. 실제 Google Sheets 연결은 후속 범위이며, 4단계 종합 판정은 이후 PLAN-025에서 완료했다.

4단계 첫 작업 [PLAN-013](../../../plans/PLAN-013-responsive-ui-and-mobile-input.md)은 completed다. 가로·세로 UI·모바일 입력·방향 전환 상태 보존을 구현하고 Edit 259/259·Play 23/23·렌더 수정 후 종료 대상 1/1 및 PC 화면 14장을 확인했다. Android ARM64 APK 빌드와 2026-10-29 SM-G977N 실기기의 오프라인 초기 진입·양방향 운영·실제 핀치/회전·앱 복귀·한 판 종료/재시작도 확인했다. [실행 증거](../../../plans/evidence/PLAN-013/README.md)를 따른다. 후속 UI·UX는 PLAN-014, Editor 병목·반복·성능은 PLAN-020·021에서 검증했다. PLAN-013 당시에는 4단계 전체를 완료로 판정하지 않았으며 최신 Android·D-070 전체 판정은 이후 PLAN-025에서 완료했다.

UI·UX 정비 [PLAN-014](../../../plans/PLAN-014-mobile-ui-ux-and-facility-status.md)는 completed다. 가로·세로 탐색·작은 아이콘 조작·시설 상태 표시 ON/OFF·표식 재탭과 운영 유스케이스를 임시 에셋으로 구현하고 PC 검사·새 APK·실기기 한 판 종료/재시작 및 원상 복원을 확인했다. JSON 전환 결과는 아래 PLAN-015에 기록했다. 정식 아트는 해당 요청의 지정대로 별도 art PLAN에서 교체한다. Editor 병목·장시간 검증은 PLAN-020·021에서 완료했으며 최신 Android·D-070 전체 판정은 PLAN-025에서 완료했다.

데이터 기반 후속 정비 [PLAN-015](../../../plans/PLAN-015-topic-json-definition-data.md)는 completed다. CSV 원본을 전용 디렉터리의 주제별 JSON으로 전환하고 취득부 추상화·SO 결합·GameDataStore·기존 저장 호환성을 유지하는 작업이다. 사용자 지시에 따라 Editor 런타임 테스트를 포함하고 빌드 테스트는 제외한다. 두 검토 보완을 반영하고 구현·Edit 294/294·Domain Reload 양쪽 Play 각각 36/36을 확인했다. 빌드 테스트는 실행하지 않았다.

최근 작업 [PLAN-016](../../../plans/PLAN-016-bootstrap-title-and-scene-flow.md)은 completed다. Bootstrap·Title·Laboratory, 공용 LoadingScreen·단일 씬 전환 관리자·Editor 직접 진입과 저장/설정 복구를 구현하고 Editor 회귀를 검증했다. 해당 완료 결과에는 새 빌드·실기기 검증과 종합 성능 최종 판정을 포함하지 않는다.

최근 구조 정비 [PLAN-017](../../../plans/PLAN-017-production-storage-status-and-shared-ui.md)은 completed다. 생산 공통 판정·파일 처리·명시적 상태 계약·공용 UI와 두 검토 보완을 구현하고 Edit 302/302, ON Play 51/51, OFF 회귀와 진단 로그가 간섭한 1개 대상의 재검증, 실제 Editor 화면·이전 저장·반복 종료를 확인했다. [상세 증거](../../../plans/evidence/PLAN-017/README.md)를 따른다. 빌드 테스트는 실행하지 않았다. 4단계 종합 성능 최종 판정은 해당 PLAN의 범위 밖이었으며 이후 PLAN-025에서 완료했다.

텍스트 정비 [PLAN-018](../../../plans/PLAN-018-localization-and-csv-text-management.md)은 completed다. Unity Localization·주제별 CSV 9개/247개 키·TextManager·앱 언어 설정과 한국어·영어 전환, 추가 언어 등록·번역 재검토 추적을 구현했다. Edit 330/330·Play 57/57·직접 씬/반복 실행·두 언어 가로/세로 화면 검증을 완료했다. [실행 증거](../../../plans/evidence/PLAN-018/README.md)를 따른다. 후속 공용 Popup 정비 PLAN-019도 completed이며 아래 연결을 따른다. 후속 Editor 검증은 PLAN-020에서 완료했다. PLAN-018에서는 Player·Addressables 콘텐츠 빌드·실기기를 실행하지 않았다. 최신 Android·4단계 종합 판정은 이후 PLAN-025에서 완료했다.

확정한 Unity 버전·Universal 2D(URP)·Android 대상과 연동 방향은 [플레이 환경](../../04-delivery/01-platform/environment.md)을 따른다. 구현할 작업의 필수 참조를 아래에서 읽고, 승인 규칙 안에서 필요한 세부 설계를 구체화한다.

## 구현 순서와 작업별 참조 (D-080·D-086)

**데이터 구조 설계와 기반 시스템 작업을 먼저 진행하고, 세 묶음의 완료 기준을 통과한 뒤 기존 1~4단계를 이어간다.** 0은 선행 단계를 뜻하며 1~4의 상대 순서를 유지한다.

| 순서 | 작업 범위 | 필수 참조 | 완료 확인 |
| --- | --- | --- | --- |
| 0 | 정의·Thing / 공간·공통 조작 / 한 판·저장 기반 | [Thing 구조](../01-architecture/thing-model.md), [GameFlow](../01-architecture/game-flow.md), [플레이 환경](../../04-delivery/01-platform/environment.md) | 아래 선행 기반 3묶음의 완료 기준 |
| 1 | 초기 건설·수집·저장: 프로토타입 1개체와 초기 유기물로 획득시설·저장소를 건설하고 수집·운반·입고 연결 | [시작 상태와 진입 흐름](../../04-delivery/02-release/scope.md), [건설·배치](../../02-systems/03-space/placement.md), [획득 생산](../../02-systems/01-production/facilities.md), [저장·운반](../../02-systems/02-logistics/storage-and-transport.md), [작업 선택](../../02-systems/05-creatures/support-models.md) | 해당 시작 흐름과 규칙을 실행하고 상태·저장·하네스로 확인 |
| 2 | 가공·배양·보급형 운용: 배양액 생산, 생명체 배양, 배정, 20%·100% 속도 전환 연결 | [생산 회차·배양](../../02-systems/01-production/facilities.md), [배정·작업 전환](../../02-systems/05-creatures/support-models.md), [공급·예약](../../02-systems/02-logistics/storage-and-transport.md) | 생산·배양·작업 전환 규칙과 추가 상태의 저장·복원 확인 |
| 3 | 침공부터 한 판 종료: 방출·지속 활동·전과 보고·멸망 게이지·연출·결과·다시 시작 연결 | [반복 흐름과 침공](../../01-direction/02-experience/core-loop.md), [보고의 변화](../../03-player-experience/01-progression/flow.md), [GameFlow](../01-architecture/game-flow.md), [저장](../../04-delivery/01-platform/environment.md) | 가로 화면에서 한 판의 기본 흐름을 끝까지 연결하고 외부 활동·다시 시작 확인 |
| 4 | 반복 플레이와 모바일 최종 검증: 세로 대응, 병목 개선, 전체 사이클 반복, 실제 Android 기기 확인 | [최종 완료 기준](../../04-delivery/02-release/scope.md), [병목](../../02-systems/04-optimization/bottlenecks.md), [화면·가로세로](../../03-player-experience/03-interface/controls.md), [실행·기기 검증](../../04-delivery/01-platform/environment.md) | [PLAN-025 완료](../../../plans/PLAN-025-android-integration-and-prototype-completion.md): D-070 네 기준·양방향 표시/터치·성능 확인 |
| 5 | 프로토타입 이후 정식 아트 제작·적용: 작은 샘플부터 연구소·개체·UI·연출을 단계적으로 확장 | [아트 로드맵](art-roadmap.md), [기존 콘셉트](../../01-direction/01-identity/concept.md), [아트 지침](../../../agents/art-assets.md) | 5-1은 [PLAN-026](../../../plans/PLAN-026-art-style-scale-and-sample-integration.md) art·completed. 전체 샘플 적용·Editor 검증·사용자 게임 화면 승인 완료. 5-2 이후와 승인된 기기 검증은 별도 |

모든 단계에서 필요한 최소 화면과 상태 확인, 저장·복원 및 하네스 검증을 함께 진행한다. UI 조작은 [화면과 조작](../../03-player-experience/03-interface/controls.md), 공통 책임은 [Thing 구조](../01-architecture/thing-model.md)를 참조한다. 저장·하네스·상태 표시를 4단계에서 처음 구현하는 것으로 해석하지 않는다.

5단계는 완료된 1~4단계 뒤의 표현 개선 작업이며 D-070에 새 필수 조건을 소급해서 추가하지 않는다. 상세 제작 순서와 검수 기준은 아트 로드맵에서 관리한다.

## 선행 기반의 범위와 완료 기준 (D-086)

| 묶음 | 최소 구현 범위 | 완료 확인 | 현재 상태 |
| --- | --- | --- | --- |
| 정의·Thing 기반 | SO 정의 확인, Thing 생성, 컴포넌트 연결·조회, ID와 소속 관리 | 같은 정의를 사용하는 두 개체가 독립된 상태를 갖고 필요한 기능을 조회할 수 있다. | [완료·검증](../../../plans/PLAN-003-thing-foundation.md) |
| 공간·공통 조작 기반 | Thing과 Map의 기본 배치 정보 연결, 공통 명령·규칙 검사·상태 조회 | 배치 변경이 Map에 일관되게 반영되고, 화면과 하네스가 같은 처리 경로를 사용한다. | [완료·검증](../../../plans/PLAN-004-map-and-commands.md) |
| 한 판·저장 기반 | 신규 시작·초기화·복원 흐름, 현재 구현된 상태와 관계의 저장·복원 | 자원 소속과 Map 밖 개체를 복원하고, 다시 시작하면 이전 개체·관계가 남지 않는다. | [완료·검증](../../../plans/PLAN-007-gameflow-and-persistence.md) |

기반 단계에서는 같은 SO를 공유하는 시설 두 개의 실행 상태 독립성, 복원 후 자원이 정확히 한 보관처에 속하는지 등을 확인한다. 현재 구현한 기반 상태와 관계가 검증 대상이며, 후속 생산·운반 기능 전체를 먼저 완성할 필요는 없다.

실제 건설·수집·운반·생산·배양·침공은 1~4단계에서 구현한다. 해당 기능의 진행도·예약·Job 등도 그 단계에서 저장·복원과 하네스 검증에 추가한다.

세 묶음의 검증 결과를 해당 PLAN에 남기고 증거 링크와 함께 위 요약 상태를 갱신한다. **모든 기반 완료 기준을 실제로 통과한 뒤 1단계로 넘어간다.** 최종 프로토타입 완료 판정 D-070과 구분한다.

## 가로 기본 사이클 후 세로 대응 (D-090)

프로토타입은 가로·세로 모두 지원한다. 1~3단계에서 가로 화면의 초기 건설부터 한 판 종료·다시 시작까지 기본 게임 사이클을 먼저 완성하고 세로 대응을 추가한다. 최종 모바일 검증을 마치기 전에 두 방향을 모두 포함한다.

두 방향은 같은 기능·화면 요소를 재배치하여 사용한다. 정식 표현 범위는 [화면과 조작](../../03-player-experience/03-interface/controls.md)을 따른다. 가로에서만 동작하는 상태는 최종 프로토타입 완료가 아니다.

## 설계·연동·검증 적용 원칙 (D-059·D-080·D-088)

- 컴포넌트·Thing·SO·GameFlow와 담당 시스템의 세부 설계를 첫 개발 작업으로 구체화한다. 정확한 Manager 목록·클래스명·씬·조회 API·필드·갱신 순서를 미리 확정한 것은 아니다.
- 기존 정의와 상태 분리, Job과 생산 회차 책임, 자원 소유와 예약, 공통 Map, 저장·복원, UI·하네스 공통 경로를 유지한다.
- 선행 기반을 정식 출시용 전체 프레임워크나 후속 콘텐츠까지 먼저 구현하는 범위로 확대하지 않는다.
- 공식 CLI·MCP를 연결하고 하네스는 선행 기반부터 사용한다. 같은 명령 처리·규칙 검사·상태 조회로 실제 게임 상태를 검증하며 기능 추가에 따라 범위를 확장한다.
- 구체적인 명령·API·테스트 도구·검증 구성·반복 횟수·성능 수치는 개발에서 정한다. 실제 실행 결과로만 연결 성공과 검증 통과를 기록한다.

## 개발 기록의 위치

| 내용 | 원본 위치 |
| --- | --- |
| 작업 단위·상태·진행·다음 행동 | [docs/plans](../../../plans/README.md)의 해당 PLAN |
| 실행한 방법·환경·결과·증거·남은 문제 | 해당 PLAN의 검증 결과 |
| D-059 안의 주요 기술 선택·이유·대안·적용 위치 | [docs/decisions](../../../decisions/README.md) |
| 작은 구현 선택·일회성 조정 | 해당 PLAN |
| 승인된 게임 규칙·콘텐츠·완료 기준의 변경 | 기존 [기획 결정 기록](../../00-management/02-decisions/record.md)과 주제 문서 |

이 문서에 개별 PLAN 상태·기술 결정·검증 로그를 다시 작성하지 않는다. 단계 요약은 증거 PLAN을 연결하여 갱신한다.
문서 정비나 PLAN 활성화는 게임 구현·검증 완료를 의미하지 않는다.

## 작업 PLAN 연결

- [PLAN-001 지침 체계 정비](../../../plans/PLAN-001-agent-guidance.md)
- [PLAN-002 Unity 개발 환경 준비](../../../plans/PLAN-002-unity-environment.md)
- [PLAN-003 SO 정의와 Thing·컴포넌트 기반](../../../plans/PLAN-003-thing-foundation.md)
- [PLAN-004 공간·공통 조작 기반](../../../plans/PLAN-004-map-and-commands.md)
- [PLAN-007 GameFlow와 저장·복원 기반](../../../plans/PLAN-007-gameflow-and-persistence.md)
- [PLAN-008 초기 건설·수집·저장](../../../plans/PLAN-008-initial-construction-and-logistics.md)
- [PLAN-009 가공·배양·보급형 운용과 시스템 책임·명명 정비](../../../plans/PLAN-009-processing-cultivation-and-workers.md): 2단계 개발 완료·검증. 3단계 방출/외부 활동과 4단계 모바일 최종 검증은 후속 범위.
- [PLAN-010 침공형 방출·외부 활동·성과 누적·전과 보고](../../../plans/PLAN-010-invasion-activity-and-reports.md): completed. 3단계 방출·외부 활동·성과/보고·목표 정지 구현·검증 완료.
- [PLAN-011 멸망 연출·결과 화면·한 판 종료와 재시작 완결](../../../plans/PLAN-011-destruction-results-and-restart.md): completed. 후보 등록·사용자 실행 승인 후 3단계 잔여 종료 연결과 실제 두 판을 구현·검증 완료.
- [PLAN-012 CSV·SO 분리와 GameDataStore 초기 로딩](../../../plans/PLAN-012-csv-so-data-and-game-data-store.md): completed. 교체 가능한 CSV 취득부·Registry 결합 Data·영속 싱글턴·저장 호환성과 기존 게임 회귀 검증 완료.
- [PLAN-013 가로·세로 UI와 모바일 입력 대응](../../../plans/PLAN-013-responsive-ui-and-mobile-input.md): completed. 화면·입력 구현, PC 검사·Android APK 빌드와 2026-10-29 실기기 오프라인 초기 진입·양방향 조작·회전·앱 복귀·종료/재시작 검증 완료.
- [PLAN-014 모바일 UI·UX와 시설 상태 표시 정비](../../../plans/PLAN-014-mobile-ui-ux-and-facility-status.md): completed. 가로·세로 탐색·아이콘·시설 상태 표시 ON/OFF·운영 유스케이스와 사용자 후속 배치 보완을 구현하고 PC·APK·실기기 한 판/재시작·원상 복원을 검증했다. 정식 아트 교체는 별도 art PLAN.
- [PLAN-015 주제별 JSON 정의 데이터와 초기 로딩 전환](../../../plans/PLAN-015-topic-json-definition-data.md): completed. 주제별 JSON·DTO와 교체 가능한 취득부로 전환하며 Editor 런타임 테스트를 포함하고 빌드 테스트는 제외한다.

- [PLAN-016 Bootstrap·Title·Laboratory 씬 흐름과 Editor 직접 진입](../../../plans/PLAN-016-bootstrap-title-and-scene-flow.md): completed. 공통 초기화·싱글톤 씬 전환·공용 LoadingScreen·메뉴와 세션 진입/복귀·Editor 직접 실행을 구현하고 Editor 검증을 마쳤다. 사용자 지시에 따라 빌드 테스트는 실행하지 않았다.

- [PLAN-017 생산 판정·파일 저장·상태 계약·공용 UI 추상화 정비](../../../plans/PLAN-017-production-storage-status-and-shared-ui.md): completed. 생산/저장/상태/공용 UI 정비와 Editor 런타임 회귀를 완료했다. 빌드 테스트는 실행하지 않았다.

- [PLAN-018 Unity Localization·CSV 텍스트 관리와 한국어·영어 지원](../../../plans/PLAN-018-localization-and-csv-text-management.md): completed. 텍스트 연결·언어 확장·번역 유지보수와 Editor 런타임 검증 완료. 빌드 테스트는 실행하지 않았다.

- [PLAN-019 공용 Popup 시스템과 기존 확인·설정 화면 통합](../../../plans/PLAN-019-shared-popup-system.md): completed. PopupBase·SO Registry·공유 Manager/Host와 Confirm·Notice·Settings, 기존 확인/설정 이관·입력/수명·Localization을 구현했다. 앱 복귀 확인 의도 보존과 멸망 경계의 플레이 팝업 정리를 포함한다. Edit 343/343·Play 65/65·실제 직접 진입/반복 Play·두 언어/방향 화면을 확인했고 빌드·실기기는 제외했다. [실행 증거](../../../plans/evidence/PLAN-019/README.md)를 따른다.

- [PLAN-020 Editor 런타임 운영 병목·반복 플레이·성능 검증](../../../plans/PLAN-020-editor-bottlenecks-repeated-play-and-performance.md): completed. Editor 운영·반복·성능 검증·진단 완료. 제품 수정·최적화 구현·빌드·실기기 검증은 제외했다.

- [PLAN-021 작업자 경로 탐색 최적화와 번역 CSV 반복 가져오기 안정화](../../../plans/PLAN-021-worker-pathfinding-and-localization-import.md)는 completed다. Editor에서 작업자 탐색·배회·입력 반응과 번역 가져오기 회귀를 검증했다.

- [PLAN-022 약탈형 배양·침공과 연구소 자원 수신](../../../plans/PLAN-022-raider-and-resource-receiving.md)은 completed다.

- [PLAN-023 로딩 진행률·Feel 연출과 전체 게임 UI 물마루 TMP 전환](../../../plans/PLAN-023-loading-progress-feel-and-mulmaru-tmp.md)은 completed다.
- [PLAN-024 침공형 방출 성공 피드백과 화면 밖 시설 선택 시 지도 이동](../../../plans/PLAN-024-release-feedback-and-map-reveal.md)은 completed다.
- [PLAN-025 최신 버전 Android 통합 검증과 프로토타입 완료 판정](../../../plans/PLAN-025-android-integration-and-prototype-completion.md)은 completed다. 승인된 최신 Editor·Android 통합 검증과 환경 복원을 마쳐 4단계·D-070 완료로 판정했다.
- [PLAN-026 아트 5-1 스타일·크기 샘플과 연구소 적용](../../../plans/PLAN-026-art-style-scale-and-sample-integration.md)은 art·completed다. 5종 106 Sprite의 전체 적용 후 기술 검증·사용자 게임 화면 승인·환경 복원을 마쳤다.
- [PLAN-027 연구소 바닥 Tilemap·시설과 개체 SpriteRenderer 전환](../../../plans/PLAN-027-laboratory-tilemap-and-sprite-rendering.md)은 `development` / `completed`다. 2026-11-10 사용자 승인 후 바닥 Tilemap·시설/개체 SpriteRenderer·UI uGUI 전환과 Editor 검증을 완료했다. 원본 아트·게임 상태를 보존했다.

- [PLAN-028 연구소 생명체 이동 보간과 표시 동기화](../../../plans/PLAN-028-creature-movement-interpolation.md)는 development·completed다. 빌드·고부하 제외 조건으로 기능 완료를 판정했으며 기존 비용 증가와 미측정 한계는 실행 증거를 따른다.

- [PLAN-029 세 시설 정식 아트](../../../plans/PLAN-029-laboratory-processing-cultivation-storage-art.md)는 art·completed다. 5-2의 가공·배양·저장 시설 3종 r001 제작·수령·Art 승인·Unity 전체 적용·일반 기술 검증과 2026-11-12 사용자 게임 화면 승인을 완료했다. 5-2 전체 완료는 별도 판정한다.

이후 단계의 개별 PLAN은 착수할 작업 단위가 구체화되면 proposed로 작성한다.

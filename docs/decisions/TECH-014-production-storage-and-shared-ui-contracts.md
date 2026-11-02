# TECH-014 생산·저장·상태·공용 UI의 최소 공통 계약

- 날짜: 2026-11-02
- 상태: 채택
- 작업: [PLAN-017](../plans/PLAN-017-production-storage-status-and-shared-ui.md)
- 기준: [추상화](../agents/abstraction.md), [객체 설계](../agents/object-design.md)

## 배경과 선택

반복되는 규칙과 실제 공유 사용처를 기준으로 네 영역을 정비한다. 게임 규칙·정의 데이터·저장 스키마는 변경하지 않는다.
공통 기반 Manager·범용 Store·DI·상태 머신 대신 함수·결과 값·기존 객체 조합을 사용한다.

| 영역 | 선택 | 유지하는 책임 |
| --- | --- | --- |
| 생산 | ProductionManager.Evaluate가 ProductionReadiness를 반환하고 실행·대기 설명·시설 상태 조회가 공유 | 진행·재료 소비는 생산 담당, 작업자·예약 관찰과 표시 변환은 FacilityStatusQuery |
| 파일 교체 | AtomicTextFile.Write가 임시 파일·Flush·교체·정리를 수행 | 세션/설정의 직렬화·기본값·실패 후 정책은 각각의 Store |
| 저장 조회 | SaveFileProbe가 Missing·Readable·Unreadable과 오류를 반환 | 스냅샷 유효성은 Codec, 사용자 확인과 씬 흐름은 Title/SceneFlowManager |
| 상태 | WorkerWaitReason·LoadingScreenKind로 의미를 전달 | 사용자 문구는 Presentation, 작업자 대기는 복원 후 재계산하는 일시적 상태 |
| 공용 UI | UiCanvas·UiWidgets·UiList·UiButton·UiText를 Presentation/UI/Widgets로 이동 | AppUiLifetime이 공용 입력, 각 화면이 Canvas·목록·스타일·입력 맥락을 소유 |

## 중요한 계약

- 생산 판정은 관찰만 한다. 생산 시작·소비·진행·예약·캐시를 추가하지 않는다. 완료품·배양 대상·접근·입력 조건의 우선순위를 유지한다.
- 입력이 과다한 비정상 회차는 실행을 허용하지 않으며 상태 조회에서 정상 생산처럼 표시하지 않는다. 정상 입력 불변 조건과 기존 저장 검증을 완화하지 않는다.
- 파일 교체에 실패해도 기존 파일을 먼저 삭제하지 않는다. 정리는 원래 예외를 가리지 않으며 세션·설정 파일은 각 경로와 정책을 유지한다.
- 파일 조회가 성공해도 JSON·저장 호환성이 유효하다는 뜻은 아니다. 없음만 새 게임 확인을 생략하며 읽기 실패는 오류로 남긴다. 이후 실제 읽기/쓰기도 실패할 수 있다.
- WorkerWaitReason은 저장 필드가 아니다. 복원 시 None으로 초기화하고 기존 작업/경로 판단에서 갱신한다. 표시 문구를 로직 입력으로 역변환하지 않는다.
- LoadingScreen은 Show마다 앱 입력 세대를 무효화한다. 화면·문구·종류가 같아도 새 요청의 버튼을 이전 누름으로 실행하지 못한다. Hide와 프레임별 Render는 새 요청과 구분한다.
- UiText가 기본 크기 × 현재 배율을 적용한다. 화면과 목록은 폰트·배율을 전달하며 폰트가 없으면 기본 폰트를 사용한다. Initialize는 현재 스타일을 유지한 채 기본 크기만 바꾼다.
- UiList는 최소 터치 크기·스타일·입력 세대를 전달받으며 LaboratoryView를 탐색하지 않는다. 동적으로 추가한 행도 현재 스타일을 받는다.
- 공용 UI 이동 시 기존 스크립트 GUID를 유지하고 namespace·어셈블리는 유지한다. 연구소 맵·시설 표식·아이콘·게임 규칙을 공용 계층으로 옮기지 않는다.
- 실제 Play 종료에서 씬 화면이 영속 관리자보다 먼저 파괴되는 순서를 확인했다. SceneFlowManager 종료 정리는 Unity 객체의 `!= null` 검사로 이미 파괴된 LaboratoryView 호출을 건너뛰며, C# null 조건부 호출로 대체하지 않는다.

## 대안과 범위

- 생산 상태 조회가 실행 로직을 복제하는 대안은 규칙 수정 누락 위험 때문에 제외한다. UI의 전체 상태 모델을 실행 로직에 주입하지 않는다.
- 두 Store를 상속으로 합치는 대안은 저장 실패 후 서로 다른 동작까지 결합하므로 제외한다.
- 공용 UI 전체를 화면 기반 클래스 또는 싱글톤으로 만드는 대안은 현재 소유 관계에 필요하지 않다.
- 데이터 취득 IGameDataSource, 저장 ISessionStore, 기존 게임 Manager와 Thing 컴포넌트 조합은 유지한다.
- [TECH-010](TECH-010-responsive-ui-and-mobile-input.md)의 화면/입력 규칙과 [TECH-013](TECH-013-app-scenes-and-loading-screen.md)의 씬 수명 규칙을 유지하며 공용 UI의 이름·의존성과 상태 계약을 보완한다. 과거 실행 증거는 당시 이름을 유지한다.

## 검증

실제 실행·증거·한계는 [PLAN-017](../plans/PLAN-017-production-storage-status-and-shared-ui.md)에 기록한다.
생산/저장 Edit Mode와 공용 UI·상태·씬 Play Mode를 검증하며 빌드는 사용자 지시로 제외한다.

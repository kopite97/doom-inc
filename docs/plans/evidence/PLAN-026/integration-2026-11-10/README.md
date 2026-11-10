# PLAN-026 승인 아트 5종 통합 적용

- 날짜: 2026-11-10. Unity 6000.3.27f1, Windows Editor, Android 선택 상태. CLI 1.0.0-beta.17 / Pipeline 0.9.1-exp.1.
- [시작 기준](baseline.json), [기존 Git 변경](git-status-before.txt), [수령 원본 메타데이터](PrototypeSupport-r002-frame-metadata.json).
- [임포트/이동 결과](import-result.json), [참조 연결](bind-result.json), [전체 리소스 준비 확인](readiness.json), [원본/GUID 보존](source-integrity.json).
- 5 PNG·106 Sprite 전체 적용·참조 연결을 확인한 시각은 readiness의 UTC 기록을 따른다. 그 전에는 게임 실행·Edit/Play 테스트·화면 캡처를 수행하지 않았다.
- 표시는 uGUI 유지. 64px/칸, 시설 128px/2칸, 보조형 접지 (32,54), r002 프레임별 머리점, 유기물 받침 (18,28), Build 22 UI 단위. 새로운 게임 규칙·저장 필드·패키지·씬 구조 전환 없음.
- 기술 검증: 아래 최종 결과 통과. 전체 준비 확인 후에만 실행했다.
- 사용자 최종 게임 화면 시각 확인: **2026-11-10 승인 완료**. 실제 가로/세로 화면·운반·수집 동작을 제시한 별도 검수 질문에 사용자가 “현재 게임 화면 승인”으로 답했다. 기존 Art 원본 승인을 대신 사용하지 않았다.
- Android/콘텐츠/Player/IL2CPP 빌드·기기 검증은 이번 범위에서 실행하지 않는다.

## 적용 결과와 후속 제작 기준

| 리소스 | 실제 적용 | 참조 |
| --- | --- | --- |
| LaboratoryFloor r003 | 64×64 정적 4셀, 70/10/10/10 좌표 고정 혼합 | 기존 LaboratoryUiAssets.floorTiles 유지 |
| Gathering r003 | 128×128 × 36, Idle 12×200ms / Working 12×100ms / Complete 12×100ms 1회 | LaboratoryArt SO의 36개 프레임·시설 정의 |
| PrototypeSupport r002 | 64×64 × 64, 방향당 Idle 4×250ms / Move 6×100ms / Work 6×120ms | LaboratoryArt SO의 64개 프레임·64개 머리 좌표·개체 정의 |
| OrganicMatter r001 | 32×32 정적 1개 | ResourceDefinition.Icon → HUD 수량 / 실제 운반 자원 |
| Build r001 | 64×64 정적 1개, 화면 22×22 UI 단위 | LaboratoryUiAssets.icons[6], 씬/AppConfiguration/생성 도구 |

- 시설은 2칸, 보조형은 1칸의 64px 표시 비율을 채택했다. 시설 r003은 기본 방향 시트를 현재 논리 회전으로 재사용한다. 다른 시설의 시점·입구·그림자에 이를 무조건 적용하지 않고 후속 제작에서 확인한다.
- 보조형은 중심 피벗을 유지하고 접지 (32,54)를 칸 중심에 맞춘다. 자원은 r002 머리점과 자기 받침 (18,28)을 맞추며 몸체 변형·회전·반전은 물려받지 않는다. Down 두 점눈·측면 한 눈·Up 무안면과 4방향 태그를 유지한다.
- 정규 경로의 원본 PNG는 변경하지 않았다. 공통 PPU32·Point·None·Clamp, Multiple Full Rect/Single 설정과 플랫폼 override 없음은 [준비 확인](readiness.json)·[에셋 검사](edit-art-rerun.json)를 따른다.
- 참조는 작은 불변 LaboratoryArt SO로 묶었다. 각 Thing의 재생 시간/방향은 화면이 소유하며 게임 저장에 넣지 않는다. 후속 PLAN-027에서 같은 참조와 표시 계약을 이관할 수 있고, 이번에는 uGUI를 유지했다.
- 수집 완료는 실제 생성 뒤 알림만 읽는다. 이동은 실제 경로 캐시, 복원 첫 표시는 같은 경로 탐색기의 읽기 전용 투영을 사용한다. 프레임 재생은 생산·이동·소유를 변경하지 않는다.
- 새 아트가 상태 표식에 가리지 않도록 보조형·운반물 영역을 표식 배치의 회피 영역에 포함했다. 원본 Tint는 흰색, 선택/미리보기 경계는 별도 그래픽이다. 미제공 종류는 기존 표시를 유지한다.

## 검증 결과

| 검사 | 최종 결과 | 증거 |
| --- | --- | --- |
| Editor 재컴파일 | 오류 없음 | [환경](environment-final.json), [Console](console-final-status.json) |
| Foundation Edit 회귀 | 첫 363개 중 362 통과. 생성 도구의 불필요한 재임포트 수정 후 관련 6개 모두 통과, 미해결 실패 없음 | [첫 회귀](edit-status.json), [수정 후 6/6](edit-art-rerun.json) |
| 연구소 Play 전체 / Domain Reload ON | **38/38** | [결과](play-domain-on-status.json) |
| 최종 아트 Play / Domain Reload OFF | **5/5** | [결과](play-domain-off-status.json), [Unity XML](play-domain-off-results.xml) |
| 원본·GUID·환경·파일 | PNG 해시/GUID 5개·바닥 meta 불변, 보존 파일 33개 불변, 예상 밖 변경 0 | [최종 무결성](final-integrity.json), [최종 참조](final-readiness.json) |
| 최종 Console | errors 0 / warnings 0 | [실제 Console 상태](console-final-status.json) |
| 사용자 시각 확인 | 2026-11-10 “현재 게임 화면 승인” | 본문의 별도 사용자 확인 기록 |

검증 범위는 실제 건설/수집/운반/전체·부분 입고/잔량 대기, 12태그의 모든 프레임, 시설 36프레임, 4방향 회전, 경로 모서리의 진행률 유지, 개체별 독립 재생, 동작 전환 첫 프레임, 일시정지, 저장/복원/새 판/씬 재진입, 누락 아트 대체, 지도/상태/겹친 후보 선택·드래그·핀치·팝업·한영/큰 글씨다. 그림 갱신의 저장·게임 난수 불변도 확인했다.

수정·재검증 이력:

- 생성 도구가 설정이 맞는 Build를 다시 임포트해 참조 객체를 교체하던 것을 수정했다. 이미 맞는 설정은 재사용한다. 초기 호출에서 직렬화만 정리된 나머지 15개 임시 아이콘의 플랫폼 설정도 Editor API로 원래 값으로 복원했고 최종 diff가 없다.
- 첫 운반 검사의 고정 UI 좌표 기대값을 원본 픽셀 비율로 고쳤다. 배치 도구가 열리면 viewport 크기는 바뀔 수 있지만 프레임/앵커 시간은 고정된다. 미제공 Storage까지 정식 표시 객체로 세던 테스트도 제공 범위에 맞게 바로잡았다.
- 프레임 캡처가 EndOfFrame에서 돌아온 직후 다음 Advance를 같은 화면 프레임에 두 번 누적하던 테스트를 고쳐 각 실제 표시 갱신 뒤 샘플링했다. 순간 경로 재예약과 실제 부분 입고 완료를 구분하고, 완료 시설 fixture의 초기 건설 재료도 정상 건설처럼 소비했다.
- 실제 화면에서 상태 표식이 보조형 얼굴을 가린 문제는 제품 표시 코드를 보완했다. 이후 전체 연구소 38개 회귀와 최종 아트 5개가 통과했다. 이전 실패 파일은 이력으로 남긴다.

## 실제 게임 화면

검증 슬롯의 같은 상태로 1280×720 / 720×1280, zoom 1·3·8을 캡처했다. 실제 휴대폰 입력·성능 검증이 아니다. 선택된 시설은 (6,4), 보조형 실제 수집 작업이며 모두 같은 Sprite 리비전이다.

| 조건 | 가로 | 세로 |
| --- | --- | --- |
| 전체 보기 | [1280×720 / 1](screens/mixed-1280x720-zoom1.png) | [720×1280 / 1](screens/mixed-720x1280-zoom1.png) |
| 중간 확대 | [1280×720 / 3](screens/mixed-1280x720-zoom3.png) | [720×1280 / 3](screens/mixed-720x1280-zoom3.png) |
| 최대 확대 | [1280×720 / 8](screens/mixed-1280x720-zoom8.png) | [720×1280 / 8](screens/mixed-720x1280-zoom8.png) |

- [기본 바닥만](screens/base-only-1280x720-zoom1.png), [혼합 바닥](screens/mixed-1280x720-zoom1.png). 16×10 분포는 기본116·질감9·긁힘14·얼룩21로 기존과 같다.
- 작업 방향: [Up](screens/work-rotation-0-Up.png), [Right](screens/work-rotation-1-Right.png), [Down](screens/work-rotation-2-Down.png), [Left](screens/work-rotation-3-Left.png). 시설 각 회전과 실제 접근 방향을 사용했다.
- [실제 자원 집기](screens/actual-cargo.png), [일부 입고 뒤 남은 3개 운반 대기](screens/partial-admission-waiting-cargo.png).
- 실제 게임 캡처 재생: [Working](screens/animation/working.gif), [Complete](screens/animation/complete.gif). 검수용 영상이며 새 게임 리소스가 아니다. Working은 120ms 간격 12샘플, Complete는 100ms 간격 12샘플을 기록했다. 원본 Sprite 재생 시간은 위 명세대로다.

## 종료 상태와 한계

- Play 정지, 원래 Laboratory 씬·clean 상태, Android 선택, Domain Reload OFF 설정·runInBackground false로 복원했다. [환경 기록](environment-final.json).
- Balance JSON·ProjectSettings와 기존 Analytics 파일은 해시 불변이다. 원래 persistent 경로에는 사용자 session/preferences 파일이 없었고 새로 만들지 않았다. 테스트는 별도 임시 슬롯을 사용하고 정리했다. Unity TestRunner가 덮어쓰는 TestResults.xml만 정상적으로 변경되어 증거로 복사했다.
- 원래 사용자 변경을 보존하고 커밋·푸시는 하지 않았다. Unity가 생성한 GUID/meta와 LFS 적용 대상 PNG를 함께 유지한다. 테스트 임시 씬은 종료 후 남지 않는다.
- PLAN-026/5-1을 완료하고 active는 0개로 둔다. PLAN-027은 proposed이며 별도 활성화 승인 전까지 실행하지 않는다. Android 아트 가독성·실기기 성능과 아트 5-2 이후는 별도 후속 범위다.

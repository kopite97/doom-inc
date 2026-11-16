---
id: PLAN-033
type: art
status: completed
title: 보급형 보조형 외형·4방향 동작 제작·적용
updated: 2026-11-16
---

# PLAN-033 보급형 보조형 외형·4방향 동작 제작·적용

[사용 양식](_template-art-animation.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

보급형을 프로토타입과 실루엣으로 구별하고 건설·수집·운반·배정 생산의 실제 행동을 읽게 한다. `creature.support` 1종의 본체·목록 대표 프레임·방향별 동작과 최소 표시 연결을 포함한다.
프로토타입 그림·시간·머리 좌표를 보존하고 생산·배정·이동 속도·점유·저장 규칙은 유지한다. 고급형·특화형·진화·도구별 별도 동작은 제외한다.

## 기준 문서와 현재 확정 사항

- [보조형 규칙](../game-design/02-systems/05-creatures/support-models.md), [외형 기준](../game-design/05-development/02-implementation/art-roadmap.md#interaction-friendly-creatures), [정의](../../UnityProject/Assets/JSON/Balance/Creatures.json).
- 선행 완료: PLAN-026 PrototypeSupport r002·64프레임, PLAN-028 이동 보간, PLAN-030 선택 강조. [기존 승인/요청](../../UnityProject/Assets/Sprites/Require/PLAN-026/README.md)을 참조한다.
- [LaboratoryArtAssets](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryArtAssets.cs)와 [지도 표시](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.Art.cs)를 정의별 생명체 세트로 확장해 보급형 정식 시트를 연결했다. 기존 프로토타입 데이터는 보존했다.
- [캐릭터](../../../Art/docs/agetns/creation/characters/basics.md)·[타이밍](../../../Art/docs/agetns/animation/timing.md)·[정렬/방향](../../../Art/docs/agetns/animation/alignment-and-directions.md)을 적용한다.

## 디자인 의도와 원하는 모습

아래 외형은 r002로 채택했으며 귀여운 보조형·두 단색 점눈·4방향·머리 위 운반은 기존 확정 원칙이다.
프로토타입보다 옆으로 안정된 둥근 젤리 몸체를 사용한다. r001의 어깨 돌출 제안은 사용자 피드백으로 제거했으며, r002의 연속된 몸통 외곽과 보라색 외형을 2026-11-16 승인받았다. 검정 단색 점눈은 Down 두 개·측면 한 개·Up 없음이다. 손·도구보다 몸체의 눌림으로 작업을 표현한다.
탑다운 노출 면·표시 높이는 프로토타입과 맞춘다. 중간색 몸체·아랫면 음영·작은 상부 반사로 재질을 구별한다. 무늬는 한 덩어리 이하로 제안하며 정확한 팔레트·선 굵기는 승인본과 비교해 확정한다. 수직 천장광·짧은 접촉 음영을 사용하고 색만 다른 복제, 운반물을 가리는 큰 돌기·복잡한 장비를 피한다.

| 참고/버전 | 가져올 요소 | 그대로 가져오지 않을 요소 |
| --- | --- | --- |
| PrototypeSupport r002 승인본 | 점눈·얼굴 방향·접지·단순 변형·운반 합성 | 같은 실루엣을 색만 변경한 복제 |
| 기존 시설 승인본 | 같은 공간의 높이·대비·광원 | 세밀한 기계 부품 |

### 동작별 움직임 설명

| 동작/방향 | 준비 → 주요 움직임 → 마무리 | 움직일/고정할 부분·변형 | 리듬·연결 |
| --- | --- | --- | --- |
| Idle / 4방향 | 낮은 자세 → 몸통 부풀음 → 기본 자세 | 접점 고정·눈 간격/체적 유지 | 작고 느린 호흡, 반복 이음새 유지 |
| Move / 4방향 | 진행 쪽 눌림 → 몸체 늘어남 → 탄성 회복 | 실제 이동은 기존 보간, 그림은 제자리 변형; 임의 점프 없음 | 1칸 이동 속도와 비교, 정지 시 접점 안정 |
| Work / 4방향 | 작업면으로 기울임 → 몸통 누름 → 되돌림 | 바닥 고정·눈/몸체 비율 유지; 도구별 동작 없음 | Move보다 묵직한 반복, 중단 시 Idle |

## 산출물과 애니메이션 명세

| 에셋 | 산출물 제안 | 방향·동작·수량 | 제작 전 확정 |
| --- | --- | --- | --- |
| Support | Aseprite 1 → PNG 시트 1, 목록은 Down Idle 첫 셀 재사용 | 4 × (Idle 4 + Move 6 + Work 6) = 64프레임·12태그 | 64×64 셀·8×8 시트 제안, 불투명 영역·피벗·프레임별 머리 좌표 |

채택한 r002는 Down/Left/Right/Up 순서, 방향별 Idle→Move→Work, 64×64 셀·8×8 시트·64프레임이다. 아래 시간도 실제 원본·재생 연결에 적용했다. 좌우 방향 셀을 각각 사용한다.

| 동작 | 시간/길이 제안 | 반복·실제 시작/종료 |
| --- | --- | --- |
| Idle | 250ms × 4 = 1000ms | 반복, 이동/실제 작업 없음; 배정만으로 Work 금지 |
| Move | 100ms × 6 = 600ms | 반복, 실제 Worker 이동/유휴 배회 구간 |
| Work | 120ms × 6 = 720ms | 반복, 접근 완료·대기 사유 없는 실제 건설/수집/배정 생산; 중단/완료 시 현재 상태 |

운반은 Move/Idle와 별도 자원 Sprite·머리 좌표를 합성한다. 방향 변경은 위상 연속성을 유지한다. 정지/편집/홈 복귀 시 멈추고 복원 시 실제 상태에서 시작한다. 출력 좌표는 좌상단 원점·픽셀 단위 여부를 명세에 적는다.

## 사용처별 적용 계획

| 사용처 | 연결·최종 경로 제안 |
| --- | --- |
| 본체·선택·가려짐 | `Assets/Sprites/Creature/Support/Support.png` → 정의별 세트 → 기존 Renderer |
| 목록·배양 대상 | [Support SO](../../UnityProject/Assets/SO/Creature/Support.asset)의 Icon과 실제 행; 없는 아이콘 슬롯은 최소 추가 |
| 운반 | 기존 CargoSupport·프레임별 머리 좌표, 실제 자원 소유 때만 표시 |
| 생성 도구 | 정의/씬 재생성 후 정식 세트·Icon 보존 |

두 종류가 실제 공유하는 방향·프레임·시간·받침점만 작은 데이터 세트로 확장한다. 개체별 재생 상태는 화면이 소유하며 SO에 저장하지 않는다. 프로토타입 데이터/하위 Sprite ID를 보존하고 범용 애니메이션 프레임워크나 게임 상태 추가는 하지 않는다.

## 결과물 저장 위치

아래 r001 표는 최초 지정 경로 기록이다. 현재 적용은 r002이며 실제 파일·GUID·해시는 [산출물 목록](evidence/PLAN-033/asset-manifest.csv), 승인·검수는 [검수 색인](evidence/PLAN-033/README.md)을 기준으로 한다. 기준 루트와 표의 경로를 결합하면 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-033/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `Support` | `assets/characters/Support/source/Support.aseprite` | `assets/characters/Support/revisions/r001/source/Support.aseprite` | `assets/characters/Support/revisions/r001/export/Support.png` | `assets/characters/Support/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 발행한 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-033/README.md`에 둔다. 요청 ID를 유지하고 r002를 수령·최종 적용했다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `Support` | `PLAN-033-SPR-001` | `Assets/Sprites/Require/PLAN-033/PLAN-033-SPR-001/r001/Support.png` | `Assets/Sprites/Creature/Support/Support.png` |

수령 PNG는 GUID/참조를 보존해 최종 경로로 옮기거나 교체한다. Require는 장기 보관/런타임 참조 위치가 아니며 과거 파일은 Art 리비전에 보존한다. 기존 완료 에셋을 재사용할 때는 원본/요청을 복제하지 않는다.

### 검수 자료

이 PLAN에서 제출받아 비교하는 원본 미리보기와 Unity 적용 검수는 E 아래에서 구분한다. Art 자체 제작 PLAN의 기존 검수 증거가 있으면 그 실제 경로를 연결하고 같은 자료를 불필요하게 복사하지 않는다.

| 자료 | E 기준 저장 경로 |
| --- | --- |
| 전체 검수 색인·최신 적용/승인 대응 | `README.md` |
| 에셋별 실제 파일·리비전·GUID/해시·사용처 목록 | `asset-manifest.csv` |
| 기술 검증·원본/게임 화면 사용자 응답·날짜 | `validation.md` |
| 작업 전 상태와 원본/참조·저장/설정·Editor 복원 | `environment.md` |
| 첫 실행의 조건·언어·화면/배율·대상 리비전 | `runs/run-001/README.md` |
| 첫 실행의 가로/세로 대표 게임 화면 | `runs/run-001/screenshots/landscape.png`, `runs/run-001/screenshots/portrait.png` |
| 필요한 경우 실제 재생 검수 영상 | `runs/run-001/videos/playback.mp4` |
| 실행한 검증 결과·로그 | `runs/run-001/tests/editmode.xml`, `runs/run-001/tests/playmode.xml`, `runs/run-001/logs/editor.log` |

실행하지 않는 검증의 파일은 만들지 않고 validation.md에 해당 없음/미실행을 기록한다. 다음 실행은 run-002부터 증가하여 이전 증거를 덮어쓰지 않는다. 장면/언어별 추가 캡처는 해당 실행 README에 파일명과 조건을 등록한다.

| 제출받은 원본 비교 대상 | E 기준 리비전별 시각 비교 | 비교 조건·원본 승인 기록 |
| --- | --- | --- |
| Support | `source-reviews/Support/r001/design.png` | `source-reviews/Support/r001/README.md` |

원본 실제 시간 재생 미리보기가 필요하면 각 위 폴더의 `motion.gif`에 보관한다. 이미지 나열과 실제 재생 검수를 구분하며 게임 화면 검수 영상은 위 playback.mp4에 둔다.

## 작업 책임과 문서 경계

- 2026-11-16 사용자 요청으로 PLAN 실행 승인. 제작 규격 제안과 요청서를 준비하며 제작·수령·적용은 별도로 기록한다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 요청 [PLAN-033-SPR-001](../../UnityProject/Assets/Sprites/Require/PLAN-033/README.md)의 r002 원본 승인·수령·적용을 완료했다. Art 제작 기록은 [Art PLAN-009](../../../Art/docs/plans/PLAN-009-support-creature.md)이다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| Support 본체·4방향 Idle/Move/Work·목록 대표 프레임 | r002 제작 완료 | r002 | r002 승인 | r002 적용 | Edit 8·Play 7 통과 | 2026-11-16 승인 |

- 전체 준비 판정: 2026-11-16 충족. 필수 시트 1장·64프레임·본체·머리 좌표·목록/배양 대상 아이콘을 연결했다. [검수 색인](evidence/PLAN-033/README.md)의 임포트·참조 확인 이후 기술 테스트를 진행한다.
- 준비 중에는 파일·임포트·참조 확인만 수행한다. Edit/Play·수동 게임 실행·검수 캡처는 전체 준비 후 진행한다. 원본 승인과 게임 화면 승인을 구분한다.
- 임포트는 [아트 지침](../agents/art-assets.md)의 PPU 32·Point·원본 해상도 유지 기준을 따른다. 좌표·프레임 시간은 PNG와 별도로 기록한다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [x] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 에셋별 원본·리비전·수령·최종 적용·검수 자료의 예정 경로 지정
- [x] 수량·사용처·규격 확정, 상세 요청/Art 제작 기록 연결
- [x] 전체 필수 리소스 제작·수령·원본 승인·리비전 대조
- [x] 정규 경로·씬/SO·생성 도구의 실제 참조 전체 연결
- [x] 전체 준비 판정 후 기술 검증, 수정 리비전·채택 규격·환경 정리 기록
- [x] 사용자 게임 화면 확인·로드맵 완료 범위 기록

## 완료 기준과 검증 방법

| 기준 | 실행할 검증·기대 결과 |
| --- | --- |
| 디자인 일치 | 원하는 실루엣·구성·재질·명암·금지 요소를 실제 최소 표시 크기에서 대조, 사용자 확인 |
| 제작/참조 | 원본·출력·알파·셀/시간·기준점·임포트/플랫폼 override·누락 Sprite와 전체 사용처 확인 |
| 행동 일치 | 건설·수집·운반·배정 생산·재료 대기에서 실제 행동과 동작 대조 |
| 합성·선택 | 4방향 접지/받침·유기물 운반·외곽선·밀집 선택 확인; 배양액 PLAN을 기다릴 필요 없음 |
| 기존 보존 | 프로토타입 64프레임·머리 좌표·이동 보간·정지/이어하기·새 판 회귀 |
| 보존/재생성 | 기존 승인본 해시/GUID·하위 Sprite 참조 유지, 씬/정의 생성 도구 후 정식 연결 보존 |
| 완료 판정 | 전체 필수 대상 적용·기술 검증·사용자 게임 화면 승인과 환경 복원 완료 |

- 기본 검증은 일반 Editor 범위다. Player 컴파일·빌드·실기기·고부하 계측은 현재 명시적 요청이 없어 기본 실행/필수 완료 조건에서 제외한다. 요청 시 범위와 승인 근거를 기록한다.
- 연결 코드 변경 전 [추상화](../agents/abstraction.md)·[객체 설계](../agents/object-design.md)·[Unity 에셋](../agents/unity-assets.md)·[Localization](../agents/localization.md)의 해당 경계를 읽고 최소 변경만 수행한다.
- 원본/리비전·GUID·저장/설정·Editor 실행 상태를 작업 전 기록하고 종료 시 복원한다. 원본을 자동 리사이즈/재색칠해 규격에 맞추지 않는다.

## 검증 결과와 사용자 확인

- 2026-11-16: 문서·현행 코드/정의의 정적 조사로 후보 작성. 신규 리소스 제작·수령·게임 적용·게임 테스트는 미실행.
- 현재 승인: r002 원본 외형·게임 화면 승인 완료. 2026-11-16 사용자 「수정된 모습으로 진행하자. r002를 작업 원본에 반영하고 Unity에 적용해줘」를 원본 승인·적용 요청으로 기록했다.
- 문서 검사: 2026-11-16 문서 검사 통과(PLAN 42개·active 0개·목록 42개·로컬 링크 3,888개). 변경 형식 검사 통과. 기존 Unity/Art PLAN 40개가 작업 전 해시와 같음을 확인했다.
- 경로 보완 검사(2026-11-16): 문서 검사 통과(42 PLAN·active 0·로컬 링크 3,904개), 25개 제작 후보의 원본/제출/수령/최종 적용/원본 비교 경로 누락·중복 없음, 기존 완료 PLAN 40개 해시 불변 확인. 예정 결과물은 생성하지 않았다.
- 위 후보/경로 보완 시점에는 문서만 변경했다. 이후 실행 결과: r002 원본 동기화·최종 시트 연결 완료, Edit Mode 8개·Play Mode 7개 통과. 최초 생산 테스트의 UI 조작 실패를 수정 후 재검증했다. [검증 결과](evidence/PLAN-033/validation.md)에 실행별 성공/실패를 보존했다.
- 환경: Play 정지·씬 clean·Run In Background false, 동적 폰트 캐시 Git diff 0. Unity는 검수를 위해 열어 두었다. [환경 기록](evidence/PLAN-033/environment.md).

## 현재 상황과 다음 행동

- status: completed. r002 원본 외형 사용자 승인·작업 원본 동기화·Unity 수령 및 전체 연결 완료. 원본과 적용 PNG 해시가 일치하며 프로토타입 에셋은 보존했다.
- 다음 행동: 없음. 2026-11-16 사용자 「좋아. 게임 화면도 확인했어. PLAN-033과 Art PLAN-009를 완료 처리하고 다음 아트 플랜 방금과 같은 플로우로 실행해줘」로 r002 게임 화면 최종 확인·완료를 승인했다. 5-3 보급형 및 5-5 보급형 동작 범위 완료. 다음 대상은 권장 순서의 PLAN-035다.
- 로드맵: 5-3 생명체 / 5-5 보조형 동작. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/원본 승인 리비전은 r002, 64×64·8×8·64프레임·PPU32·중앙 피벗이다. 최신 기준 화면은 run-002의 1280×720·720×1280이다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

- 2026-11-16: proposed → active. 사용자 「PLAN-033 보급형 생명체 아트 작업을 진행해줘. 아트 작업은 해당 소스폴더의 지침을 따라」 요청에 따라 실행 승인.

- 2026-11-16: 사용자 명시적 제작 위임에 따라 Art PLAN-009의 보급형 r001 초안을 제작했다. 64프레임 시트·원본 대조 결과는 Art 제출 명세에 기록했으며 시각 승인·Unity 수령은 대기다.
- 2026-11-16: r002 외형 승인 후 작업 원본 동기화·Unity 적용·기술 검증 완료. 사용자 게임 화면 검수 대기로 active 유지.

- 2026-11-16: active → completed. 사용자 r002 게임 화면 확인·완료 요청. 적용·기술 검증·최종 시각 확인 충족.

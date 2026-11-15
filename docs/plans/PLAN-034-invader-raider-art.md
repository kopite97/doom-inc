---
id: PLAN-034
type: art
status: proposed
title: 일반 침공형·약탈형 외형과 대기 동작 제작·적용
updated: 2026-11-16
---

# PLAN-034 일반 침공형·약탈형 외형과 대기 동작 제작·적용

[사용 양식](_template-art-animation.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

`creature.invader`와 `creature.raider`의 외형·최소 대기 동작·목록 아이콘을 적용한다. 방출 효과는 PLAN-039이며 외부 활동은 기존 텍스트 보고로 유지한다.
현재 침공형에 없는 보조형 이동/작업·전투·공격/피격/사망·귀환·약탈품을 들고 돌아오는 동작은 추가하지 않는다.

## 기준 문서와 현재 확정 사항

- [침공 규칙](../game-design/01-direction/02-experience/core-loop.md), [정의](../../UnityProject/Assets/JSON/Balance/Creatures.json), [캐릭터 지침](../../../Art/docs/agetns/creation/characters/basics.md), [타이밍](../../../Art/docs/agetns/animation/timing.md).
- 선행: [PLAN-033](PLAN-033-support-creature-art.md)의 정의별 아트 연결. 기존 시설/프로토타입 승인본을 비교 기준으로 사용한다.
- [CreatureMotion](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/CreatureMotion.cs)은 Worker 없는 개체에 이동을 만들지 않는다. 침공형 대체 도형을 교체하며 보조형의 젤리·점눈을 의무로 확대하지 않는다.

## 디자인 의도와 원하는 모습

일반형은 위가 넓고 바닥이 안정된 짧은 쐐기형 몸체와 큰 턱/앞판으로 전진감을 제안한다. 약탈형은 작은 앞몸과 뒤쪽의 둥근 주머니 같은 돌출부로 수집 역할을 제안한다. 돌출부는 역할의 상징이며 실제 자원 소유·귀환 상태를 뜻하지 않는다.
일반형은 단단한 큰 외피 면, 약탈형은 부드러운 둥근 낭으로 재질을 구분한다. 주조색은 배경 대비, 강조색은 얼굴/앞면 식별에 한정한다. 정확한 눈 형태·색·외곽 굵기는 두 종류를 나란히 비교해 확정한다.
기존 승인본에서 시점·캐릭터 상대 크기·픽셀 밀도·수직 상부광을 가져오고 보조형 얼굴은 복제하지 않는다. 작은 화면에서 큰 외곽 두세 덩어리를 남기며 과한 가시·치아·무기·미세 점과 색만 다른 변형은 피한다.

### 동작별 움직임 설명

| 동작 | 준비 → 주요 움직임 → 마무리 | 고정/변형·리듬 |
| --- | --- | --- |
| Invader Idle | 몸을 낮춤 → 앞판/몸통이 작게 들림 → 안정 자세 | 접지·턱 길이·체적 유지, 무거운 호흡; 돌진처럼 보이지 않음 |
| Raider Idle | 뒤 낭 눌림 → 낭이 부풀고 앞몸이 미세하게 따라감 → 기본 자세 | 실제 이동 없이 탄성 반응, 자원 생성처럼 보이지 않음; 가벼운 리듬 |

## 산출물과 재생 명세

| 에셋 | 방향/동작 제안 | 수량·파일 | 규격 제안 |
| --- | --- | --- | --- |
| Invader | 대기 표시용 Down Idle | 4프레임·원본 1·PNG 1 | 64×64 셀·4×1 시트 |
| Raider | 대기 표시용 Down Idle | 4프레임·원본 1·PNG 1 | 64×64 셀·4×1 시트 |

합계 2종·8프레임·2 PNG 제안, 목록은 각 첫 셀 재사용. 접지/피벗·실제 그림 영역·여백은 제작 전에 확정한다. 이동하는 개체의 4방향 기준과 구분하며 실제 사용처에 방향 전환이 필요하면 제작 전 명세에 반영한다. 미사용 Move/Work는 만들지 않는다.

| 태그 | 시간 제안 | 시작·종료/정지 |
| --- | --- | --- |
| Invader_Idle_Down | 4 × 250ms = 1000ms 반복 | 연구소 배치·방출 전 대기, 외부 활동 전환 시 지도 제거 |
| Raider_Idle_Down | 4 × 200ms = 800ms 반복 | 동일; 정지/홈 복귀에서 위상 정지, 복원 때 과거 방출 재생 금지 |

## 사용처별 적용 계획

- 최종 경로 제안: `Assets/Sprites/Creature/Invader/Invader.png`·`Assets/Sprites/Creature/Raider/Raider.png`.
- [Invader SO](../../UnityProject/Assets/SO/Creature/Invader.asset)·[Raider SO](../../UnityProject/Assets/SO/Creature/Raider.asset)의 Icon과 배양/생명체/침공 목록, 지도 본체·외곽선·밀집 후보 선택에 연결한다.
- Worker 없는 개체는 보조형 작업 판정에 넣지 않는다. 방출 후 지도 본체는 즉시 제거하고 외부 활동 정보/목록 아이콘만 유지한다.
- PLAN-039의 효과는 개체를 논리 지도에 남겨두지 않고 표시 스냅샷을 이용한다. 생성 도구·새 판/복원에서 정의별 아트 연결을 유지한다.

## 결과물 저장 위치

아래는 이번 PLAN에서 사용할 **예정 경로**다. 위치 지정은 제작·수령 완료를 뜻하지 않으며 파일/빈 폴더는 지금 만들지 않는다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-034/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `Invader` | `assets/characters/Invader/source/Invader.aseprite` | `assets/characters/Invader/revisions/r001/source/Invader.aseprite` | `assets/characters/Invader/revisions/r001/export/Invader.png` | `assets/characters/Invader/revisions/r001/README.md` |
| `Raider` | `assets/characters/Raider/source/Raider.aseprite` | `assets/characters/Raider/revisions/r001/source/Raider.aseprite` | `assets/characters/Raider/revisions/r001/export/Raider.png` | `assets/characters/Raider/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-034/README.md`에 둔다. 아래 ID는 이 PLAN의 예정 요청 ID로 배정했으며 상세 요청서는 아직 미발행이다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `Invader` | `PLAN-034-SPR-001` | `Assets/Sprites/Require/PLAN-034/PLAN-034-SPR-001/r001/Invader.png` | `Assets/Sprites/Creature/Invader/Invader.png` |
| `Raider` | `PLAN-034-SPR-002` | `Assets/Sprites/Require/PLAN-034/PLAN-034-SPR-002/r001/Raider.png` | `Assets/Sprites/Creature/Raider/Raider.png` |

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
| Invader | `source-reviews/Invader/r001/design.png` | `source-reviews/Invader/r001/README.md` |
| Raider | `source-reviews/Raider/r001/design.png` | `source-reviews/Raider/r001/README.md` |

원본 실제 시간 재생 미리보기가 필요하면 각 위 폴더의 `motion.gif`에 보관한다. 이미지 나열과 실제 재생 검수를 구분하며 게임 화면 검수 영상은 위 playback.mp4에 둔다.

## 작업 책임과 문서 경계

- 현재 범위는 2026-11-16 사용자가 요청한 남은 아트 PLAN 작성이다. 제작·Unity 적용·게임 테스트는 미승인/미실행이다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 신규 요청은 미발행이다. 제작 규격 확정 시 `UnityProject/Assets/Sprites/Require/PLAN-034/README.md`에 요청별 명세·최종 경로·상태를 기록한다. 위 표의 예정 요청 ID·파일 위치를 사용하며 경로 지정은 실제 수령을 뜻하지 않는다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| Invader 본체·Idle·목록 대표 프레임 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| Raider 본체·Idle·목록 대표 프레임 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |

- 전체 준비 판정: 미충족. 전체 필수 리소스의 제작·수령·적용·참조 연결을 정적으로 확인한 날짜와 근거를 기록한 뒤 테스트를 시작한다.
- 준비 중에는 파일·임포트·참조 확인만 수행한다. Edit/Play·수동 게임 실행·검수 캡처는 전체 준비 후 진행한다. 원본 승인과 게임 화면 승인을 구분한다.
- 임포트는 [아트 지침](../agents/art-assets.md)의 PPU 32·Point·원본 해상도 유지 기준을 따른다. 좌표·프레임 시간은 PNG와 별도로 기록한다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [ ] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 에셋별 원본·리비전·수령·최종 적용·검수 자료의 예정 경로 지정
- [ ] 수량·사용처·규격 확정, 상세 요청/Art 제작 기록 연결
- [ ] 전체 필수 리소스 제작·수령·원본 승인·리비전 대조
- [ ] 정규 경로·씬/SO·생성 도구의 실제 참조 전체 연결
- [ ] 전체 준비 판정 후 기술 검증·사용자 게임 화면 확인
- [ ] 수정 리비전 반영·환경 복원·채택 규격·로드맵 완료 범위 기록

## 완료 기준과 검증 방법

| 기준 | 실행할 검증·기대 결과 |
| --- | --- |
| 디자인 일치 | 원하는 실루엣·구성·재질·명암·금지 요소를 실제 최소 표시 크기에서 대조, 사용자 확인 |
| 제작/참조 | 원본·출력·알파·셀/시간·기준점·임포트/플랫폼 override·누락 Sprite와 전체 사용처 확인 |
| 역할 식별 | 일반형·약탈형·보조형을 같은 배경/작은 표시에서 실루엣으로 구분 |
| 방출·목록 | 배양 출현·밀집 선택·전원 방출·외부 목록·이어하기에서 두 종류 일치 |
| 수명 | Worker 없음·정지/복귀·새 판에서 예외/과거 사건/남은 Renderer 없음 |
| 보존/재생성 | 기존 승인본 해시/GUID·하위 Sprite 참조 유지, 씬/정의 생성 도구 후 정식 연결 보존 |
| 완료 판정 | 전체 필수 대상 적용·기술 검증·사용자 게임 화면 승인과 환경 복원 완료 |

- 기본 검증은 일반 Editor 범위다. Player 컴파일·빌드·실기기·고부하 계측은 현재 명시적 요청이 없어 기본 실행/필수 완료 조건에서 제외한다. 요청 시 범위와 승인 근거를 기록한다.
- 연결 코드 변경 전 [추상화](../agents/abstraction.md)·[객체 설계](../agents/object-design.md)·[Unity 에셋](../agents/unity-assets.md)·[Localization](../agents/localization.md)의 해당 경계를 읽고 최소 변경만 수행한다.
- 원본/리비전·GUID·저장/설정·Editor 실행 상태를 작업 전 기록하고 종료 시 복원한다. 원본을 자동 리사이즈/재색칠해 규격에 맞추지 않는다.

## 검증 결과와 사용자 확인

- 2026-11-16: 문서·현행 코드/정의의 정적 조사로 후보 작성. 신규 리소스 제작·수령·게임 적용·게임 테스트는 미실행.
- 원본 승인·게임 화면 승인: 모두 대기. 실행 시 대상 리비전·화면/배율·사용자 응답/날짜·수정/재확인을 각각 기록한다.
- 문서 검사: 2026-11-16 문서 검사 통과(PLAN 42개·active 0개·목록 42개·로컬 링크 3,888개). 변경 형식 검사 통과. 기존 Unity/Art PLAN 40개가 작업 전 해시와 같음을 확인했다.
- 경로 보완 검사(2026-11-16): 문서 검사 통과(42 PLAN·active 0·로컬 링크 3,904개), 25개 제작 후보의 원본/제출/수령/최종 적용/원본 비교 경로 누락·중복 없음, 기존 완료 PLAN 40개 해시 불변 확인. 예정 결과물은 생성하지 않았다.
- 환경 복원: 문서만 작성하여 게임·Editor·원본 아트·저장/설정을 변경하지 않았다.

## 현재 상황과 다음 행동

- status: proposed. active PLAN은 0개이며 이 문서는 실행 승인이 아니다.
- 다음 행동: PLAN-033 표시 계약을 확인하고 두 외형과 정면 대기 세트의 제작 규격을 확정한다.
- 로드맵: 5-3 침공형 / 5-5 최소 대기 동작. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/승인 리비전·채택 규격·기준 화면은 실행 후 기록한다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

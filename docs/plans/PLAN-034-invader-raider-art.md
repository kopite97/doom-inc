---
id: PLAN-034
type: art
status: completed
title: 일반 침공형·약탈형 외형과 4방향 대기·이동 제작·적용
updated: 2026-11-17
---

# PLAN-034 일반 침공형·약탈형 외형과 4방향 대기·이동 제작·적용

## 최신 통합 상태 — 2026-11-17

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

## 제작 우선·마지막 일괄 적용

2026-11-16 사용자 「모든 아트리소스를 먼저 작업한 다음 한번에 Unity에 적용」「검증은 마지막 Art플랜에서 한번에」 요청을 반영한다. [PLAN-042 실행 절차](PLAN-042-art-integration-review.md#batch-art-workflow)를 이 묶음의 공통 운영 기준으로 사용한다.

- 이 PLAN 단계는 Art 작업 원본·리비전별 PNG/시트·명세·미리보기 제작과 제출 목록 작성까지다. 파일 저장/열기·출력 누락 같은 제작 작업 확인 및 문서 검사는 가능하지만, 정식 기술 검증·통합 시각 검수의 통과로 기록하지 않는다.
- 파일은 Art의 지정 리비전에 보관한다. Unity Assets로 PNG를 복사하지 않고, 임포트·슬라이싱·SO/씬/프리팹/재질·연결 코드·생성 도구 변경·Edit/Play·게임 캡처는 PLAN-042로 유보한다. 기존 화면/참조는 일괄 적용 전까지 유지한다.
- 아래 디자인·파일 수·요청 ID·Art 원본/리비전·Unity 최종 경로·적용 요구·검증 항목은 유지한다. 사용처별 적용과 모든 정식 검증은 PLAN-042가 담당하며, 이 PLAN의 체크는 해당 통합 증거 링크로 충족한다.
- 제작이 끝나면 `on hold`(일괄 적용·검증 대기)로 전환한다. 제작 완료를 Unity 아트 PLAN의 `completed`로 간주하지 않는다. 제작 묶음의 모든 에셋에 대해 아래 사용자 확인을 마친 뒤, 승인된 순차 실행 범위에서 다음 제작 PLAN으로 넘어간다. active는 최대 1개다.
- **제작 후 사용자 확인 필수:** 에셋별 정면/시트 미리보기와 애니메이션 GIF, 리비전·변경 내용을 제시하고 사용자 응답을 기다린다. 수정 요청은 새 리비전에 반영해 다시 보여준다. 해당 묶음의 외형·동작 확인 전 다음 Unity 제작 PLAN을 시작하지 않는다. 승인된 리비전·응답·날짜를 Art PLAN과 요청 README에 기록한다. 이 원본 확인은 PLAN-042의 기술 검사·게임 화면 검증과 별도이며, 기존 승인 외형은 통합 단계에서 변경된 경우에만 재확인한다.
- 앞 PLAN의 리소스를 참고할 때는 Art 제출본·디자인/규격을 사용한다. 앞 PLAN의 Unity 적용·게임 검증·completed 상태를 후속 제작의 선행 조건으로 요구하지 않는다.
- 2026-11-16 후속 사용자 요청으로 제작을 실행한다. Art PLAN-011 Invader → PLAN-012 Raider 순서이며 Unity 적용·정식 검증 유보는 유지한다.

## 목표와 범위

`creature.invader`와 `creature.raider`의 외형·4방향 대기·이동 동작·목록 아이콘을 적용한다. 방출 효과는 PLAN-039이며 외부 활동은 기존 텍스트 보고로 유지한다.
F-001 사용자 요청으로 네 방향 Idle/Move 프레임을 제작한다. Worker·이동 AI·작업·전투·공격/피격/사망·귀환·약탈품 운반 규칙은 변경하지 않는다.

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

2026-11-16 F-001 사용자 「침공형과 약탈형도 상하좌우 움직이는 프레임이 필요해」를 반영한 r002 명세다. r001은 보존한다.

| 에셋 | 방향/동작 | 수량·파일 | 규격 |
| --- | --- | --- | --- |
| Invader | Down/Left/Right/Up 각 Idle4+Move6 | 40프레임·원본1·PNG1 | 64×64셀·10열4행·640×256 |
| Raider | Down/Left/Right/Up 각 Idle4+Move6 | 40프레임·원본1·PNG1 | 동일 |

합계80프레임·2 PNG. 행 Down/Left/Right/Up, 각 행 Idle4셀 다음 Move6셀. 목록은 첫 셀. 접지 (32,54)px·하단 원점 피벗 (0.5,0.15625). 정면 회전 대신 방향별 측면/뒷면 제작. Move는 제자리 보행이며 위치 변화는 Unity 담당이다.

| 태그 | 시간 | 의도 |
| --- | --- | --- |
| Invader_Idle_방향 | 250ms×4 = 1000ms | 접지 고정 호흡 |
| Invader_Move_방향 | 120ms×6 = 720ms | 짧은 발 교대·무거운 외피 1px 반응 |
| Raider_Idle_방향 | 200ms×4 = 800ms | 뒤 낭 호흡 |
| Raider_Move_방향 | 100ms×6 = 600ms | 빠른 발 교대·낭의 지연 탄성 |

실제 방향/이동 상태 연결·속도 동기화는 PLAN-042에서 확인하고 아트만으로 이동 AI를 추가하지 않는다.

## PLAN-042에서 실행할 사용처별 적용 계획

- 최종 경로 제안: `Assets/Sprites/Creature/Invader/Invader.png`·`Assets/Sprites/Creature/Raider/Raider.png`.
- [Invader SO](../../UnityProject/Assets/SO/Creature/Invader.asset)·[Raider SO](../../UnityProject/Assets/SO/Creature/Raider.asset)의 Icon과 배양/생명체/침공 목록, 지도 본체·외곽선·밀집 후보 선택에 연결한다.
- Worker 없는 개체는 보조형 작업 판정에 넣지 않는다. 방출 후 지도 본체는 즉시 제거하고 외부 활동 정보/목록 아이콘만 유지한다.
- PLAN-039의 효과는 개체를 논리 지도에 남겨두지 않고 표시 스냅샷을 이용한다. 생성 도구·새 판/복원에서 정의별 아트 연결을 유지한다.

## 결과물 저장 위치

아래 Art 원본·r002 제출본은 제작했고 Unity 수령·최종 적용은 **예정 경로**다. 실제 자료는 [인계 색인](evidence/PLAN-034/README.md)에 연결했다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-034/`
- 최초 제출 r001을 보존하고 현재 제출은 r002다. 이후 수정 제출은 r003부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r002 원본 보존본 | r002 PNG 제출본 | r002 제출 명세 |
| --- | --- | --- | --- | --- |
| `Invader` | `assets/characters/Invader/source/Invader.aseprite` | `assets/characters/Invader/revisions/r002/source/Invader.aseprite` | `assets/characters/Invader/revisions/r002/export/Invader.png` | `assets/characters/Invader/revisions/r002/README.md` |
| `Raider` | `assets/characters/Raider/source/Raider.aseprite` | `assets/characters/Raider/revisions/r002/source/Raider.aseprite` | `assets/characters/Raider/revisions/r002/export/Raider.png` | `assets/characters/Raider/revisions/r002/README.md` |

### Unity 수령과 최종 적용

아래 경로는 PLAN-042에서 일괄 수령·적용할 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-034/README.md`에 둔다. 아래 ID로 상세 요청서를 발행했으며 PNG 수령은 PLAN-042까지 유보한다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r002 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `Invader` | `PLAN-034-SPR-001` | `Assets/Sprites/Require/PLAN-034/PLAN-034-SPR-001/r002/Invader.png` | `Assets/Sprites/Creature/Invader/Invader.png` |
| `Raider` | `PLAN-034-SPR-002` | `Assets/Sprites/Require/PLAN-034/PLAN-034-SPR-002/r002/Raider.png` | `Assets/Sprites/Creature/Raider/Raider.png` |

수령 PNG는 GUID/참조를 보존해 최종 경로로 옮기거나 교체한다. Require는 장기 보관/런타임 참조 위치가 아니며 과거 파일은 Art 리비전에 보존한다. 기존 완료 에셋을 재사용할 때는 원본/요청을 복제하지 않는다.

### 검수 자료

원본 미리보기·리비전 인계 목록은 이 PLAN/Art 지정 위치에 보관한다. 정식 검증·게임 캡처·환경 기록은 아래 `../PLAN-042/` 경로에 한 번만 저장하고 이 PLAN에서 링크한다. 이전에 지정한 개별 게임 실행 폴더는 생성하지 않는다.

이 PLAN에서 제출받아 비교하는 원본 미리보기와 Unity 적용 검수는 E 아래에서 구분한다. Art 자체 제작 PLAN의 기존 검수 증거가 있으면 그 실제 경로를 연결하고 같은 자료를 불필요하게 복사하지 않는다.

| 자료 | E 기준 저장 경로 |
| --- | --- |
| 전체 검수 색인·최신 적용/승인 대응 | `README.md` |
| 에셋별 실제 파일·리비전·GUID/해시·사용처 목록 | `asset-manifest.csv` |
| 기술 검증·원본/게임 화면 사용자 응답·날짜 | `../PLAN-042/validation.md` |
| 작업 전 상태와 원본/참조·저장/설정·Editor 복원 | `../PLAN-042/environment.md` |
| 첫 실행의 조건·언어·화면/배율·대상 리비전 | `../PLAN-042/runs/run-001/README.md` |
| 첫 실행의 가로/세로 대표 게임 화면 | `../PLAN-042/runs/run-001/screenshots/PLAN-034-landscape.png`, `../PLAN-042/runs/run-001/screenshots/PLAN-034-portrait.png` |
| 필요한 경우 실제 재생 검수 영상 | `../PLAN-042/runs/run-001/videos/game-flow.mp4` |
| 실행한 검증 결과·로그 | `../PLAN-042/runs/run-001/tests/editmode.xml`, `../PLAN-042/runs/run-001/tests/playmode.xml`, `../PLAN-042/runs/run-001/logs/editor.log` |

실행하지 않는 검증의 파일은 만들지 않고 validation.md에 해당 없음/미실행을 기록한다. 다음 실행은 run-002부터 증가하여 이전 증거를 덮어쓰지 않는다. 장면/언어별 추가 캡처는 해당 실행 README에 파일명과 조건을 등록한다.

| 제출받은 원본 비교 대상 | E 기준 리비전별 시각 비교 | 비교 조건·원본 승인 기록 |
| --- | --- | --- |
| Invader | `source-reviews/Invader/r002/design.png` | `source-reviews/Invader/r002/README.md` |
| Raider | `source-reviews/Raider/r002/design.png` | `source-reviews/Raider/r002/README.md` |

원본 실제 시간 재생 미리보기가 필요하면 각 위 폴더의 `motion.gif`에 보관한다. 이미지 나열과 실제 재생 검수를 구분하며 게임 화면 검수 영상은 위 playback.mp4에 둔다.

## 작업 책임과 문서 경계

- 사용자 후속 실행 요청으로 Art 원본·리비전·명세·미리보기를 제작한다. Unity 적용·정식 검증은 PLAN-042까지 유보한다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 2026-11-16 사용자 「Art 소스폴더 지침에 따라 PLAN을 작성하고 제작해줘」에 따라 이 두 에셋의 Aseprite 제작을 에이전트에 위임했다. 최종 시각 승인은 별도다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- [요청 README](../../UnityProject/Assets/Sprites/Require/PLAN-034/README.md)에 SPR-001/002의 명세·Art r002·최종 경로를 발행했다. PNG 미수령·미적용이다.

## 리소스 준비와 진행 요약

승인본 수령·Unity 적용·일반 Editor 기술 검증·사용자 실제 게임 화면 승인 완료(2026-11-17). 리비전/수령/최종 경로는 [최종 대응표](evidence/PLAN-042/asset-manifest.csv), 개별 완료 기준은 [49개 기준 대응](evidence/PLAN-042/plan-verification-matrix.csv)에 기록했다. 기존 승인본과 저장/설정을 보존했다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [x] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 에셋별 원본·리비전·수령·최종 적용·검수 자료의 예정 경로 지정
- [x] 수량·사용처·규격 확정, 상세 요청/Art 제작 기록 연결
- [x] 전체 필수 Art 원본·리비전 PNG·제출 명세·미리보기 제작
- [x] Art 파일 경로·요청 ID·리비전 인계 목록 작성, 제작 종료 후 on hold 전환
- [x] 사용자에게 원본 미리보기·재생 GIF 제시 → 피드백 반영 → 해당 묶음의 모든 에셋 리비전 확인 기록(다음 제작 전 필수)
- [x] PLAN-042: 원본/출력 기술 검증 후 Unity 일괄 수령·정규 경로·씬/SO·생성 도구 연결
- [x] PLAN-042: 전체 적용 후 일괄 기술 검증·원본/게임 화면 사용자 확인
- [x] PLAN-042: 필요한 수정본·재검증·환경 복원·완료 범위 증거 연결 후 completed 판정

## PLAN-042에서 확인할 완료 기준과 검증 방법

다음 항목의 검증 실행 주체는 PLAN-042다. 원본·출력·알파·시간·좌표 등 리소스 기술 검증도 마지막 단계에서 모아 수행하며, 제작 중 저장/미리보기 확인 및 제작 직후 사용자 원본 확인과 구분한다.

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
- 원본 승인: 2026-11-16 Invader/Raider r002 확인 완료. 게임 화면 승인: PLAN-042 적용 후 확인 예정.
- 문서 검사: 2026-11-16 문서 검사 통과(PLAN 42개·active 0개·목록 42개·로컬 링크 3,888개). 변경 형식 검사 통과. 기존 Unity/Art PLAN 40개가 작업 전 해시와 같음을 확인했다.
- 경로 보완 검사(2026-11-16): 문서 검사 통과(42 PLAN·active 0·로컬 링크 3,904개), 25개 제작 후보의 원본/제출/수령/최종 적용/원본 비교 경로 누락·중복 없음, 기존 완료 PLAN 40개 해시 불변 확인. 예정 결과물은 생성하지 않았다.
- 이번 제작의 환경: 새 Art 원본·리비전·미리보기와 요청/인계 문서만 생성했다. 기존 게임·Editor·완료 원본·저장/설정은 변경하지 않았다.

- 제작 후 문서 확인: Unity PLAN43·active0·로컬 링크4087 및 공백 검사 통과. Art PLAN12·active0이며 기존 완료 PLAN의 링크 오류9건 유지. [인계 기록](evidence/PLAN-034/README.md). 아트 정식 검증은 미실행이다.

## 현재 상황과 다음 행동

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

- 2026-11-16: 사용자 요청에 따라 제작 선행, Unity 적용·정식 검증은 PLAN-042로 통합. 미제작/proposed 유지. 제작 후 on hold 및 최종 증거에 의한 completed 전환을 구분했다.

- 2026-11-16: proposed → active. 사용자 PLAN-034 및 Art PLAN 작성·제작 명시 요청. Art PLAN-011/012 순차 제작, Unity 적용·정식 검증 유보.

- 2026-11-16: active → on hold. Art PLAN-011 Invader r001·PLAN-012 Raider r001 제작 및 인계 준비. 정식 기술·사용자 시각 검증과 Unity 적용은 PLAN-042에 유보. 신규 원본2·PNG시트2·총8프레임, 원본 승인 없음.

- 2026-11-16: 사용자 「작업이 끝나고 내가 확인하는 과정도 필요해」 반영. 각 제작 묶음 직후 미리보기/재생 제시와 원본 확인을 필수로 두며, 응답 전 다음 제작으로 넘어가지 않는다. Unity 적용·정식 통합 검증은 PLAN-042에 유지한다.

- 2026-11-16: on hold → active. 사용자 F-001 네 방향 이동 프레임 요청, Art PLAN-011/012 r002 각40프레임으로 확장. 제작 후 사용자 확인·마지막 Unity 적용/검증 유지.

- 2026-11-16: active → on hold. F-001 반영: Invader/Raider r002 각40프레임·8태그·640×256 PNG. 방향별 원본·대기/이동 GIF 제시 후 사용자 응답 대기. r001 및 제작 전 작업 원본 보존. Unity 적용·정식 검증 미실행.

- 2026-11-16 사용자 「좋아. r002로 진행하자. 확인 완료로 기록하고 다음 PLAN-036 아트 작업을 진행해줘」. Invader/Raider r002 원본 외형·동작 승인 완료. Unity 수령·적용·정식 검증·게임 화면 승인은 남아 on hold를 유지한다.

## 최종 완료 — 2026-11-17

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

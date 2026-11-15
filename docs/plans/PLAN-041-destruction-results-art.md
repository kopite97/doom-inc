---
id: PLAN-041
type: art
status: proposed
title: 세계 멸망 표현·결과 화면 아트 제작·적용
updated: 2026-11-16
---

# PLAN-041 세계 멸망 표현·결과 화면 아트 제작·적용

[사용 양식](_template-art-ui.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

현재 한 세계의 멸망을 작은 상징 이미지로 보여주고 결과 수치·다시 시작을 명확하게 읽게 한다. 기존 종료 진행·입력 차단·결과 전환·재시작/저장 실패 처리를 보존한다.
새 세계·전투 장면·시네마틱·음악 제작은 제외한다. 공용 버튼/스크롤은 PLAN-038을 재사용한다.

## 기준 문서와 현재 확정 사항

- [종료 규칙](../game-design/01-direction/02-experience/core-loop.md#run-end), [Ending 화면](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Ending.cs), [현재 세계 도형](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryUiWorld.cs).
- 현재 Ending.Progress에 따라 그림/문구가 바뀌고 Results는 1을 표시한다. 현재 세계는 코드 도형이며 교체할 Temp PNG가 있다고 가정하지 않는다.
- 선행: [PLAN-038](PLAN-038-shared-ui-skin-art.md). [PLAN-040](PLAN-040-title-loading-art.md)과 톤을 맞추되 기능상 완료 의존은 없다.

## 디자인 의도와 원하는 모습

실제 지리·세계 설정을 추가하지 않는 작은 구체를 레트로 관측 화면처럼 제안한다. 정상 구체의 큰 밝은 면이 갈라지고 작은 균열이 생긴 뒤 빛이 꺼진 실루엣으로 끝난다. 과한 폭발/백색 점멸 대신 조용해지는 대비로 마무리한다.
결과는 얇은 관측 보고서 프레임을 사용하고 수치·다시 시작이 장식보다 먼저 읽히게 한다. 공용 UI의 색·선 무게를 참고하고 구체에 연구소 바닥 접지/천장광을 강제하지 않는다. 게임명·번역 문구·점수는 그림에 넣지 않는다.

### 동작별 움직임 설명

| 단계 | 준비 → 주요 변화 → 마무리 | 고정/변형·리듬 |
| --- | --- | --- |
| WorldDestruction | 정상 구체 → 균열·밝은 면 감소 → 어두운 구체/잔광 | 중심·크기 고정, 큰 면 변화; 반복/역재생 없음 |
| Results | 마지막 상태 → 기존 결과 텍스트/버튼 → 정지 | 그림 재점멸 없음, 수치 읽기 우선 |

## UI 산출물과 재생 명세

| 대상 | 제안 수량·규격 | 표시 |
| --- | --- | --- |
| WorldDestruction | 원본1→시트1, 128×128셀×8, 4×2 | 기존 세계 영역, 비율 유지 |
| ResultFrame | 원본1→PNG1, 32×32·9-slice | 공용 선 무게·본문 여백 |

총 PNG2·세계8셀+프레임1셀 제안. 공용 PanelFrame으로 충분하면 제작 전 ResultFrame 재사용으로 확정한다. 캔버스·border·여백은 실제 표시 Rect로 확정한다.
세계는 독립 FPS가 아니라 기존 Ending.Progress 0~1에 8프레임을 순차 대응한다. 현재 연출 길이를 확인해 각 구간과 ms 환산값을 요청서에 적는다. Results는 마지막 프레임 고정이다. 아트를 이유로 종료 시간을 임의로 늘리지 않는다.

| 상태 | 연결·입력 |
| --- | --- |
| Destruction | 기존 진행 원본으로 프레임 선택, 시뮬레이션 정지 중 연출만 진행 |
| Results | 마지막 프레임·기존 수치·다시 시작 |
| 저장 실패/재시도 | 오류/버튼 영역 유지, 그림 입력 없음 |

## 사용처별 적용 계획

`Assets/Sprites/UI/Ending/WorldDestruction.png`와 필요한 `ResultFrame.png`를 EndingScreen에 연결하고 기존 코드 도형을 대체한다. 공용 Frame/Scroll은 재사용한다.
방향 전환 시 같은 Progress/결과를 유지하고 재진입·새 판에 이전 프레임/콜백을 정리한다. 결과 집계·세이브·초기화는 아트 콜백에서 수행하지 않는다.

## 결과물 저장 위치

아래는 이번 PLAN에서 사용할 **예정 경로**다. 위치 지정은 제작·수령 완료를 뜻하지 않으며 파일/빈 폴더는 지금 만들지 않는다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-041/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `WorldDestruction` | `assets/ui/WorldDestruction/source/WorldDestruction.aseprite` | `assets/ui/WorldDestruction/revisions/r001/source/WorldDestruction.aseprite` | `assets/ui/WorldDestruction/revisions/r001/export/WorldDestruction.png` | `assets/ui/WorldDestruction/revisions/r001/README.md` |
| `ResultFrame` | `assets/ui/ResultFrame/source/ResultFrame.aseprite` | `assets/ui/ResultFrame/revisions/r001/source/ResultFrame.aseprite` | `assets/ui/ResultFrame/revisions/r001/export/ResultFrame.png` | `assets/ui/ResultFrame/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-041/README.md`에 둔다. 아래 ID는 이 PLAN의 예정 요청 ID로 배정했으며 상세 요청서는 아직 미발행이다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `WorldDestruction` | `PLAN-041-SPR-001` | `Assets/Sprites/Require/PLAN-041/PLAN-041-SPR-001/r001/WorldDestruction.png` | `Assets/Sprites/UI/Ending/WorldDestruction.png` |
| `ResultFrame` | `PLAN-041-SPR-002` | `Assets/Sprites/Require/PLAN-041/PLAN-041-SPR-002/r001/ResultFrame.png` | `Assets/Sprites/UI/Ending/ResultFrame.png` |

수령 PNG는 GUID/참조를 보존해 최종 경로로 옮기거나 교체한다. Require는 장기 보관/런타임 참조 위치가 아니며 과거 파일은 Art 리비전에 보존한다. 기존 완료 에셋을 재사용할 때는 원본/요청을 복제하지 않는다.

ResultFrame의 기존 도형/리소스 재사용을 채택하면 해당 신규 파일은 만들지 않는다. 표에 재사용 원본의 실제 경로와 미생성 사유를 기록하며, 위 경로는 신규 제작을 선택했을 때의 목적지다.

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
| WorldDestruction | `source-reviews/WorldDestruction/r001/design.png` | `source-reviews/WorldDestruction/r001/README.md` |
| ResultFrame | `source-reviews/ResultFrame/r001/design.png` | `source-reviews/ResultFrame/r001/README.md` |

원본 실제 시간 재생 미리보기가 필요하면 각 위 폴더의 `motion.gif`에 보관한다. 이미지 나열과 실제 재생 검수를 구분하며 게임 화면 검수 영상은 위 playback.mp4에 둔다.

## 작업 책임과 문서 경계

- 현재 범위는 2026-11-16 사용자가 요청한 남은 아트 PLAN 작성이다. 제작·Unity 적용·게임 테스트는 미승인/미실행이다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 신규 요청은 미발행이다. 제작 규격 확정 시 `UnityProject/Assets/Sprites/Require/PLAN-041/README.md`에 요청별 명세·최종 경로·상태를 기록한다. 위 표의 예정 요청 ID·파일 위치를 사용하며 경로 지정은 실제 수령을 뜻하지 않는다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| WorldDestruction 세계 표현 세트 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| ResultFrame 결과 프레임 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |

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
| 종료 | 게이지 완료→진행 정지→세계 변화→결과→새 판이 기존 규칙대로 1회 |
| 시간 | 연출 중 방향 전환/홈 복귀·결과에서 Progress와 마지막 프레임 일치 |
| UI | 가로/세로·ko/en·큰 글씨·긴 수치·저장 실패/재시도에서 버튼/본문 가림 없음 |
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
- 다음 행동: 현재 Ending.Progress 시간과 표시 크기를 확인해 8프레임 단계·시간 대응을 확정한다.
- 로드맵: 5-6 종료 화면. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/승인 리비전·채택 규격·기준 화면은 실행 후 기록한다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

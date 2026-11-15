---
id: PLAN-040
type: art
status: proposed
title: 가로·세로 타이틀과 로딩 화면 아트 제작·적용
updated: 2026-11-16
---

# PLAN-040 가로·세로 타이틀과 로딩 화면 아트 제작·적용

[사용 양식](_template-art-ui.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

정식 연구소와 같은 세계로 느껴지는 가로/세로 타이틀과 최소 로딩 장식을 적용한다. 시작·이어하기·설정·오류/재시도·실제 로딩 진행은 유지한다.
정식 게임명은 [콘셉트](../game-design/01-direction/01-identity/concept.md)에서 미확정이다. 이름이 들어가는 로고/워드마크는 필수 산출물에서 제외하고 기존 TMP 제목을 유지한다. 이름 확정 후 필요하면 별도 범위를 정한다.

## 기준 문서와 현재 확정 사항

- [TitleView](../../UnityProject/Assets/Scripts/Presentation/Title/TitleView.cs)·[LoadingScreen](../../UnityProject/Assets/Scripts/Presentation/Loading/LoadingScreen.cs)·[씬 생성](../../UnityProject/Assets/Scripts/Editor/Scenes/AppSceneAssets.cs).
- 현재 Temp_NightShiftLandscape/Portrait는 정식 스타일 승인본이 아니다.
- 선행: [PLAN-038](PLAN-038-shared-ui-skin-art.md)의 공용 버튼/패널. 기존 승인 아트는 모티프로 참고하며 원본 픽셀을 임의 수정하지 않는다.

## 디자인 의도와 원하는 모습

지하 연구소 한 구석을 낮은 조도로 보여주고 작은 배양조·작업등·익숙한 시설 실루엣으로 운영 게임임을 전한다. 복잡한 전경보다 메뉴가 들어갈 빈 영역을 확보한다.
가로는 장면/메뉴 좌우 구성, 세로는 상단 장면/하단 메뉴를 제안한다. 가로 그림을 강제 잘라 세로에 맞추지 않는다. 어두운 회청색 바탕과 장치/생체의 작은 강조색으로 시선을 제한하고 텍스트보다 배경 대비를 낮춘다.
기존 월드에서 천장광·재질·귀엽고 기괴한 분위기를 가져오고 정확한 타일 배열을 확대 복제하지 않는다. 로딩은 작은 배양 용기/연구소 표식 하나를 제안한다. 가짜 진행률·문구는 그림에 넣지 않는다.

## UI 산출물과 표시 명세

| 대상 | 제안 출력 | 안전 영역 |
| --- | --- | --- |
| TitleLandscape | 원본1→PNG1·640×360 | 메뉴/제목 주변 낮은 대비, 가장자리 크롭 가능 |
| TitlePortrait | 원본1→PNG1·360×640 | 상단 장면·하단 메뉴, 노치/긴 화면 핵심 보존 |
| LoadingDecoration | 원본1→PNG1·64×64 투명 | 실제 진행/문구와 떨어진 작은 영역 |

총 원본/PNG 각 3개·정적이다. 제작 전에 실제 Rect·크롭 앵커·핵심 보존 영역을 확정한다. 비율 유지 Fill/여백 조합을 비교하고 비균등 확대는 하지 않는다. 공용 버튼/프레임은 별도 그림으로 중복 제작하지 않는다.

| 상태 | 표시·입력 |
| --- | --- |
| 새 게임/이어하기 가능·불가 | 같은 배경, 기존 저장 상태와 공용 버튼 |
| 로딩 | 정적 장식+실제 진행률·기존 Feel 웨이브 |
| 오류/재시도 | 기존 문구·버튼 영역, 장식은 비조작 |

## 사용처별 적용 계획

`Assets/Sprites/UI/Title/TitleLandscape.png`·`TitlePortrait.png`, `Assets/Sprites/UI/Loading/LoadingDecoration.png`를 실제 직렬화 참조·AppSceneAssets에 연결한다. TitlePortrait도 같은 Title 디렉터리다.
그림은 Raycast 비활성, 제목/문구는 Localization·물마루 TMP 유지. 실제 진행·최소 로딩 표시 시간·씬 구조를 바꾸지 않는다. 기존 로딩 효과를 복제하거나 새 패키지를 넣지 않는다.

## 결과물 저장 위치

아래는 이번 PLAN에서 사용할 **예정 경로**다. 위치 지정은 제작·수령 완료를 뜻하지 않으며 파일/빈 폴더는 지금 만들지 않는다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-040/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `TitleLandscape` | `assets/ui/TitleLandscape/source/TitleLandscape.aseprite` | `assets/ui/TitleLandscape/revisions/r001/source/TitleLandscape.aseprite` | `assets/ui/TitleLandscape/revisions/r001/export/TitleLandscape.png` | `assets/ui/TitleLandscape/revisions/r001/README.md` |
| `TitlePortrait` | `assets/ui/TitlePortrait/source/TitlePortrait.aseprite` | `assets/ui/TitlePortrait/revisions/r001/source/TitlePortrait.aseprite` | `assets/ui/TitlePortrait/revisions/r001/export/TitlePortrait.png` | `assets/ui/TitlePortrait/revisions/r001/README.md` |
| `LoadingDecoration` | `assets/ui/LoadingDecoration/source/LoadingDecoration.aseprite` | `assets/ui/LoadingDecoration/revisions/r001/source/LoadingDecoration.aseprite` | `assets/ui/LoadingDecoration/revisions/r001/export/LoadingDecoration.png` | `assets/ui/LoadingDecoration/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-040/README.md`에 둔다. 아래 ID는 이 PLAN의 예정 요청 ID로 배정했으며 상세 요청서는 아직 미발행이다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `TitleLandscape` | `PLAN-040-SPR-001` | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-001/r001/TitleLandscape.png` | `Assets/Sprites/UI/Title/TitleLandscape.png` |
| `TitlePortrait` | `PLAN-040-SPR-002` | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-002/r001/TitlePortrait.png` | `Assets/Sprites/UI/Title/TitlePortrait.png` |
| `LoadingDecoration` | `PLAN-040-SPR-003` | `Assets/Sprites/Require/PLAN-040/PLAN-040-SPR-003/r001/LoadingDecoration.png` | `Assets/Sprites/UI/Loading/LoadingDecoration.png` |

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
| TitleLandscape | `source-reviews/TitleLandscape/r001/design.png` | `source-reviews/TitleLandscape/r001/README.md` |
| TitlePortrait | `source-reviews/TitlePortrait/r001/design.png` | `source-reviews/TitlePortrait/r001/README.md` |
| LoadingDecoration | `source-reviews/LoadingDecoration/r001/design.png` | `source-reviews/LoadingDecoration/r001/README.md` |

## 작업 책임과 문서 경계

- 현재 범위는 2026-11-16 사용자가 요청한 남은 아트 PLAN 작성이다. 제작·Unity 적용·게임 테스트는 미승인/미실행이다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 신규 요청은 미발행이다. 제작 규격 확정 시 `UnityProject/Assets/Sprites/Require/PLAN-040/README.md`에 요청별 명세·최종 경로·상태를 기록한다. 위 표의 예정 요청 ID·파일 위치를 사용하며 경로 지정은 실제 수령을 뜻하지 않는다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| TitleLandscape 배경 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| TitlePortrait 배경 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| LoadingDecoration 장식 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |

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
| 크롭 | 가로/세로·긴 화면·안전 영역에서 핵심 장면과 메뉴/제목 대비 유지 |
| UI | ko/en·큰 글씨·이어하기 유무·오류/재시도·진행률/기존 웨이브 유지 |
| 씬 | Title→Loading→Laboratory·홈 복귀·직접 진입에서 중복 장식/참조 유실 없음 |
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
- 다음 행동: 공용 UI를 받아 가로/세로 구성·안전 영역을 확정한다. 게임명 미정은 배경 제작의 차단 조건이 아니다.
- 로드맵: 5-6 진입 화면. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/승인 리비전·채택 규격·기준 화면은 실행 후 기록한다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

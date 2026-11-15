---
id: PLAN-037
type: art
status: proposed
title: 연구소 조작·메뉴 아이콘 6종 정식 아트 교체
updated: 2026-11-16
---

# PLAN-037 연구소 조작·메뉴 아이콘 6종 정식 아트 교체

[사용 양식](_template-art-ui.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

현재 지도 조작/메뉴 아이콘 6종을 Build r001과 어울리는 정식 이미지로 교체한다. [기존 요청](../../UnityProject/Assets/Sprites/Require/PLAN-014/README.md)의 ID·파일명을 그대로 사용한다.
Build는 완료본을 재사용한다. 상태 ON/OFF 2종과 시설 상태 아이콘 7종은 PLAN-032에서 해당 사용처가 제거되어 필수 제작 대상에서 제외한다. 완료 PLAN-014 본문은 수정하지 않고 후속 수령/적용 기록만 요청 README에서 관리한다.

## 기준 문서와 현재 확정 사항

- [화면/조작](../game-design/03-player-experience/03-interface/controls.md), PLAN-026 Build 승인본, PLAN-032 상태 단순화.
- [UI 연결](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs), [생성 도구](../../UnityProject/Assets/Scripts/Editor/Presentation/LaboratoryUiAssetBuilder.cs).
- 기존 요청: 64×64 RGBA·Single·중심 피벗·각 독립 PNG·애니메이션 없음. 현재 22×22 UI 논리 단위와 Canvas/글씨 배율을 기준으로 가독성을 확인한다.

## 디자인 의도와 원하는 모습

Build r001의 둥근 외곽·큰 색면·절제한 금속 느낌과 무게를 따른다. 내부 장식보다 실루엣/공통 여백을 우선한다. 외곽·밝은 면·작은 강조색으로 구성하고 기능을 색만으로 구분하지 않는다.

| 대상·요청 끝자리 | 원하는 모습 | 피할 표현 |
| --- | --- | --- |
| ZoomOut / 001 | 짧은 손잡이 돋보기와 굵은 빼기 | 삭제 버튼 같은 형태, 테두리에 붙은 기호 |
| ZoomIn / 002 | 같은 돋보기와 굵은 더하기 | 다른 크기/손잡이 방향 |
| FitMap / 003 | 작은 지도와 떨어진 네 모서리 | 촬영·확대 아이콘과 같은 실루엣 |
| Creatures / 007 | 앞뒤 크기가 다른 두 생명체 덩어리 | 특정 종의 즉시 배양 명령처럼 보임 |
| Invasion / 008 | 프레임 밖을 향하는 화살표 | 메뉴 열기가 즉시 방출인 듯한 폭발 강조 |
| More / 009 | 같은 크기의 점 세 개·충분한 간격 | 작은 톱니·문자 |

평면 UI 명암을 사용하며 월드 광원을 강제하지 않는다. 정확한 색/선 굵기는 Build와 나란히 비교해 확정한다. 건물의 작은 창·부품은 공통 아이콘 장식으로 복제하지 않는다.

## UI 산출물과 표시 명세

6종 × 정적 1장 = 독립 원본 6·PNG 6개. 기존 64×64 요청을 유지하고 Build의 약 48×47px 그림 영역을 여백 비교 기준으로 제안한다. 개별 요청을 한 시트로 합치지 않는다.

| 상태 | 외형·연결 | 입력 |
| --- | --- | --- |
| 기본 | Sprite 1장·기존 배경/문구 | 기존 버튼 영역 |
| 눌림·선택 | 기존 Tint/전환 | 새 상태 없음 |
| 비활성 | 기존 Tint·interactable | 별도 PNG 없음 |

비율 유지·아이콘 Raycast 비활성, 텍스트는 TMP/Localization을 유지한다. 그림 때문에 버튼/터치 영역을 줄이지 않는다.

## 사용처별 적용 계획

아래 경로 표의 정식 PNG 6개 → `LaboratoryUiAssets.icons[1,2,3,7,8,9]`, Laboratory 씬/AppConfiguration 참조, 생성 도구 정식 파일 우선 연결을 함께 적용한다.
요청 README에 이 PLAN을 적용 담당으로 연결하고 중복 ID를 발행하지 않는다. 참조 없는 해당 6종 Temp만 정리 대상으로 검토하며 미사용 상태 아이콘 전체 삭제는 포함하지 않는다.

## 결과물 저장 위치

아래는 이번 PLAN에서 사용할 **예정 경로**다. 위치 지정은 제작·수령 완료를 뜻하지 않으며 파일/빈 폴더는 지금 만들지 않는다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-037/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `ZoomOut` | `assets/ui/ZoomOut/source/ZoomOut.aseprite` | `assets/ui/ZoomOut/revisions/r001/source/ZoomOut.aseprite` | `assets/ui/ZoomOut/revisions/r001/export/ZoomOut.png` | `assets/ui/ZoomOut/revisions/r001/README.md` |
| `ZoomIn` | `assets/ui/ZoomIn/source/ZoomIn.aseprite` | `assets/ui/ZoomIn/revisions/r001/source/ZoomIn.aseprite` | `assets/ui/ZoomIn/revisions/r001/export/ZoomIn.png` | `assets/ui/ZoomIn/revisions/r001/README.md` |
| `FitMap` | `assets/ui/FitMap/source/FitMap.aseprite` | `assets/ui/FitMap/revisions/r001/source/FitMap.aseprite` | `assets/ui/FitMap/revisions/r001/export/FitMap.png` | `assets/ui/FitMap/revisions/r001/README.md` |
| `Creatures` | `assets/ui/Creatures/source/Creatures.aseprite` | `assets/ui/Creatures/revisions/r001/source/Creatures.aseprite` | `assets/ui/Creatures/revisions/r001/export/Creatures.png` | `assets/ui/Creatures/revisions/r001/README.md` |
| `Invasion` | `assets/ui/Invasion/source/Invasion.aseprite` | `assets/ui/Invasion/revisions/r001/source/Invasion.aseprite` | `assets/ui/Invasion/revisions/r001/export/Invasion.png` | `assets/ui/Invasion/revisions/r001/README.md` |
| `More` | `assets/ui/More/source/More.aseprite` | `assets/ui/More/revisions/r001/source/More.aseprite` | `assets/ui/More/revisions/r001/export/More.png` | `assets/ui/More/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-014/README.md`에 둔다. 아래 ID는 기존 요청을 재사용한다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `ZoomOut` | `PLAN-014-SPR-001` | `Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-001/r001/ZoomOut.png` | `Assets/Sprites/UI/Laboratory/ZoomOut.png` |
| `ZoomIn` | `PLAN-014-SPR-002` | `Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-002/r001/ZoomIn.png` | `Assets/Sprites/UI/Laboratory/ZoomIn.png` |
| `FitMap` | `PLAN-014-SPR-003` | `Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-003/r001/FitMap.png` | `Assets/Sprites/UI/Laboratory/FitMap.png` |
| `Creatures` | `PLAN-014-SPR-007` | `Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-007/r001/Creatures.png` | `Assets/Sprites/UI/Laboratory/Creatures.png` |
| `Invasion` | `PLAN-014-SPR-008` | `Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-008/r001/Invasion.png` | `Assets/Sprites/UI/Laboratory/Invasion.png` |
| `More` | `PLAN-014-SPR-009` | `Assets/Sprites/Require/PLAN-014/PLAN-014-SPR-009/r001/More.png` | `Assets/Sprites/UI/Laboratory/More.png` |

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
| ZoomOut | `source-reviews/ZoomOut/r001/design.png` | `source-reviews/ZoomOut/r001/README.md` |
| ZoomIn | `source-reviews/ZoomIn/r001/design.png` | `source-reviews/ZoomIn/r001/README.md` |
| FitMap | `source-reviews/FitMap/r001/design.png` | `source-reviews/FitMap/r001/README.md` |
| Creatures | `source-reviews/Creatures/r001/design.png` | `source-reviews/Creatures/r001/README.md` |
| Invasion | `source-reviews/Invasion/r001/design.png` | `source-reviews/Invasion/r001/README.md` |
| More | `source-reviews/More/r001/design.png` | `source-reviews/More/r001/README.md` |

## 작업 책임과 문서 경계

- 현재 범위는 2026-11-16 사용자가 요청한 남은 아트 PLAN 작성이다. 제작·Unity 적용·게임 테스트는 미승인/미실행이다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 상세 요청은 기존 PLAN-014 README의 해당 6개 ID를 재사용한다. 중복 요청 폴더는 만들지 않는다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| ZoomOut / PLAN-014-SPR-001 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| ZoomIn / PLAN-014-SPR-002 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| FitMap / PLAN-014-SPR-003 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| Creatures / PLAN-014-SPR-007 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| Invasion / PLAN-014-SPR-008 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| More / PLAN-014-SPR-009 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |

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
| 의미 | 6종+Build를 최소 크기로 나란히 비교, 오인/시각적 무게 차이 없음 |
| 모바일 | 가로/세로·ko/en·큰 글씨에서 지도/하단 메뉴/더보기 정렬 |
| 입력·참조 | 줌/전체 보기·각 패널 열기·방출 명령과 메뉴 구분·재생성 후 정식 참조 |
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
- 다음 행동: 기존 요청 6종의 제작 묶음과 Build 비교 외형을 확정한다. 새 요청 ID는 만들지 않는다.
- 로드맵: 5-4 메뉴 아이콘. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/승인 리비전·채택 규격·기준 화면은 실행 후 기록한다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

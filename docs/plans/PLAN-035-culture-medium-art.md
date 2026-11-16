---
id: PLAN-035
type: art
status: completed
title: 배양액 자원·운반·UI 아트 제작·적용
updated: 2026-11-16
---

# PLAN-035 배양액 자원·운반·UI 아트 제작·적용

[사용 양식](_template-art-static.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

배양액을 유기물과 형태로 구별하고 가공→운반→배양 흐름을 읽게 한다. `resource.culture-medium` 1종을 정의·운반·HUD/관련 목록에 연결한다.
유기물은 완료본을 재사용한다. 자원 종류·수량·예약·입출고·비용 변경, 수량별 이미지와 액체 애니메이션은 제외한다.

## 기준 문서와 현재 확정 사항

- [저장/운반](../game-design/02-systems/02-logistics/storage-and-transport.md), [자원 정의](../../UnityProject/Assets/JSON/Balance/Resources.json), [유기물/운반 승인본](../../UnityProject/Assets/Sprites/Require/PLAN-026/README.md).
- [CultureMedium SO](../../UnityProject/Assets/SO/Resource/CultureMedium.asset), [지도 합성](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.Art.cs), [HUD](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs).
- 기존 OrganicMatter 전용 HUD를 두 자원 Icon/수치 행으로 확장했다. 실제 자원·재료·저장 허용 행도 연결했다.
- PLAN-033과 독립 실행 가능하며 기존 프로토타입 운반으로 검증한다.

## 디자인 의도와 원하는 모습

짧고 넓은 투명 용기에 수평 액면이 보이는 정제 배양액을 제안한다. 불규칙한 유기물과 달리 뚜껑·외곽·큰 액체 면이 남게 한다. 눈금·라벨 문자·작은 기포는 생략한다.
중성의 어두운 용기 외곽, 차가운 색 계열 액체, 작은 상부 반사점으로 재질을 구분하되 정확한 색은 미확정이다. 월드의 상부광을 따르고 바닥 그림자는 PNG에 굽지 않아 운반/UI에 공용 사용한다.
유기물 r001의 표시 크기·여백·선 무게를 참고하되 덩어리 윤곽/색은 복제하지 않는다. 숫자·문구·상태 표식은 그림과 분리한다.

## 정적 산출물과 공간 명세

| 대상 | 수량 | 규격 제안 | 기준점 |
| --- | --- | --- | --- |
| CultureMedium | 원본 1 → 정적 PNG 1, 1셀·회전/반전 없음 | 32×32 RGBA, 불투명 영역 약 24×24 이내·사방 4px 여백 | 중심 피벗, 용기 하단 중앙의 운반 받침 좌표 별도 기록 |

기존 유기물과 같은 32px는 시작안이다. 논리 점유를 추가하지 않고 실물 소유가 있을 때만 운반물을 표시한다. 몸체 눌림/반전을 상속하지 않고 프레임별 받침 위치만 따라간다. 바닥·타일 규칙은 해당 없다.

## 사용처별 적용 계획

| 사용처 | 변경/확인 |
| --- | --- |
| 정의 Icon | `Assets/Sprites/Resource/CultureMedium/CultureMedium.png` → 기존 SO |
| 실제 운반 | cargo Sprite·CargoSupport, 자원 교체·부분 입고·빈손 전환 |
| HUD | 두 자원 수치에 대응하는 작은 Icon 연결, 좁은 화면/큰 글씨 간격 유지 |
| 자원 목록/레시피 행 | 실제 표시 중인 자원 행에 최소 Icon 연결, 새 패널/기능 없음 |
| 생성 도구 | 정의/씬 재생성 후 정식 Icon 유지, 유기물 참조 보존 |

배양액 전용 렌더러 대신 기존 ResourceDefinition 그림 조회를 사용한다. HUD 숫자·번역은 기존 TMP/Localization을 유지한다.

## 결과물 저장 위치

아래는 최초 지정 경로다. 현재 r001 원본·PNG·Unity 적용 파일을 생성했으며 [검수 색인](evidence/PLAN-035/README.md)과 [요청 기록](../../UnityProject/Assets/Sprites/Require/PLAN-035/README.md)이 실제 상태의 기준이다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-035/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `CultureMedium` | `assets/resources/CultureMedium/source/CultureMedium.aseprite` | `assets/resources/CultureMedium/revisions/r001/source/CultureMedium.aseprite` | `assets/resources/CultureMedium/revisions/r001/export/CultureMedium.png` | `assets/resources/CultureMedium/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-035/README.md`에 둔다. 아래 ID는 이 PLAN의 예정 요청 ID로 배정했으며 상세 요청서는 아직 미발행이다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `CultureMedium` | `PLAN-035-SPR-001` | `Assets/Sprites/Require/PLAN-035/PLAN-035-SPR-001/r001/CultureMedium.png` | `Assets/Sprites/Resource/CultureMedium/CultureMedium.png` |

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
| CultureMedium | `source-reviews/CultureMedium/r001/design.png` | `source-reviews/CultureMedium/r001/README.md` |

## 작업 책임과 문서 경계

- 2026-11-16 사용자 「다음 아트 플랜 방금과 같은 플로우로 실행해줘」 요청. 권장 순서 PLAN-033 → PLAN-035에 따라 제작·연결·검증을 실행한다. Aseprite 제작 위임을 포함한 기존 흐름을 적용하며 시각 승인은 별도다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- [PLAN-035-SPR-001 요청](../../UnityProject/Assets/Sprites/Require/PLAN-035/README.md)을 발행하고 [Art PLAN-010](../../../Art/docs/plans/PLAN-010-culture-medium.md) r001을 수령·적용했다. 2026-11-16 사용자 확인으로 원본·게임 화면 시각 승인을 완료했다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| CultureMedium 정적 공용 이미지 | r001 제작 완료 | r001 | 2026-11-16 승인 | r001 | Edit10·Play7 통과 | 2026-11-16 승인 |

- 전체 준비 판정: 2026-11-16 충족. PNG 1장·Icon·CargoSupport·HUD/자원 행 연결 및 임포트/참조 정적 확인을 [증거](evidence/PLAN-035/README.md)에 기록한 후 테스트를 시작했다.
- 준비 중에는 파일·임포트·참조 확인만 수행한다. Edit/Play·수동 게임 실행·검수 캡처는 전체 준비 후 진행한다. 원본 승인과 게임 화면 승인을 구분한다.
- 임포트는 [아트 지침](../agents/art-assets.md)의 PPU 32·Point·원본 해상도 유지 기준을 따른다. 좌표·프레임 시간은 PNG와 별도로 기록한다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [x] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 에셋별 원본·리비전·수령·최종 적용·검수 자료의 예정 경로 지정
- [x] 수량·사용처·규격 확정, 상세 요청/Art 제작 기록 연결
- [x] 전체 필수 리소스 제작·수령·리비전 대조
- [x] 원본 사용자 시각 승인
- [x] 정규 경로·씬/SO·생성 도구의 실제 참조 전체 연결
- [x] 전체 준비 판정 후 기술 검증
- [x] 사용자 게임 화면 확인
- [x] r001 적용·환경 복원·채택 규격 기록
- [x] 사용자 승인 후 로드맵 완료 범위 기록

## 완료 기준과 검증 방법

| 기준 | 실행할 검증·기대 결과 |
| --- | --- |
| 디자인 일치 | 원하는 실루엣·구성·재질·명암·금지 요소를 실제 최소 표시 크기에서 대조, 사용자 확인 |
| 제작/참조 | 원본·출력·알파·셀/시간·기준점·임포트/플랫폼 override·누락 Sprite와 전체 사용처 확인 |
| 물류 | 실제 배양액 운반·빈손·부분 입고·재료 대기에서 실물과 표시 일치 |
| UI | ko/en·보통/큰 글씨·가로/세로의 HUD/자원 행에 겹침 없음 |
| 형태·합성 | 유기물과 나란히·4방향 머리 위·시설 앞에서 식별/접점/가려짐 확인 |
| 보존/재생성 | 기존 승인본 해시/GUID·하위 Sprite 참조 유지, 씬/정의 생성 도구 후 정식 연결 보존 |
| 완료 판정 | 전체 필수 대상 적용·기술 검증·사용자 게임 화면 승인과 환경 복원 완료 |

- 기본 검증은 일반 Editor 범위다. Player 컴파일·빌드·실기기·고부하 계측은 현재 명시적 요청이 없어 기본 실행/필수 완료 조건에서 제외한다. 요청 시 범위와 승인 근거를 기록한다.
- 연결 코드 변경 전 [추상화](../agents/abstraction.md)·[객체 설계](../agents/object-design.md)·[Unity 에셋](../agents/unity-assets.md)·[Localization](../agents/localization.md)의 해당 경계를 읽고 최소 변경만 수행한다.
- 원본/리비전·GUID·저장/설정·Editor 실행 상태를 작업 전 기록하고 종료 시 복원한다. 원본을 자동 리사이즈/재색칠해 규격에 맞추지 않는다.

## 검증 결과와 사용자 확인

- 2026-11-16: 문서·현행 코드/정의의 정적 조사로 후보 작성. 신규 리소스 제작·수령·게임 적용·게임 테스트는 미실행.
- 원본 승인·게임 화면 승인: 2026-11-16 사용자 배양액 확인 및 두 PLAN 완료 요청으로 완료. 실행 시 대상 리비전·화면/배율·사용자 응답/날짜·수정/재확인을 각각 기록한다.
- 문서 검사: 2026-11-16 문서 검사 통과(PLAN 42개·active 0개·목록 42개·로컬 링크 3,888개). 변경 형식 검사 통과. 기존 Unity/Art PLAN 40개가 작업 전 해시와 같음을 확인했다.
- 경로 보완 검사(2026-11-16): 문서 검사 통과(42 PLAN·active 0·로컬 링크 3,904개), 25개 제작 후보의 원본/제출/수령/최종 적용/원본 비교 경로 누락·중복 없음, 기존 완료 PLAN 40개 해시 불변 확인. 예정 결과물은 생성하지 않았다.
- 환경 복원: 문서만 작성하여 게임·Editor·원본 아트·저장/설정을 변경하지 않았다.

## 현재 상황과 다음 행동

- status: completed. PLAN-033 및 Art PLAN-009 완료 후 실행했으며, [Art PLAN-010](../../../Art/docs/plans/PLAN-010-culture-medium.md)의 제작과 Unity 적용·검증·사용자 시각 확인을 완료했다.
- 다음 행동: 없음. 사용자 「좋아. 배양액도 확인했어. PLAN-035와 Art PLAN-010을 완료 처리」로 r001 최종 시각 확인을 기록했다. 5-3 배양액 자원 범위 완료; 다른 아트 범위는 별도다.
- 로드맵: 5-3 자원. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 실제 적용/원본 승인/게임 화면 승인 모두 r001 완료. 채택 규격32×32·Single·PPU32·중앙 피벗·받침(16,28), 최신 화면run-004. 기술 검사 상세는 [증거](evidence/PLAN-035/validation.md).

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

- 2026-11-16: proposed → active. 사용자 다음 아트 PLAN 동일 흐름 실행 요청. 권장 순서의 배양액 제작·적용, 기존 active 완료 후 전환.

- 2026-11-16: r001 원본·PNG 제작 및 Unity 전체 연결. Edit10개·Play7개 통과, 캡처 조정 후 UI통합1개 재통과. 최종Console오류/경고0·원래 환경 복원, 기존 Sprite/정의 해시 보존. 원본/게임 시각 승인 대기로 active 유지.

- 2026-11-16: active → completed. 사용자 r001 확인·완료 요청. 기존 Edit10·Play7 및 화면 증거로 완료하며 재실행하지 않았다. 후속 아트 제작·일괄 적용 방식 변경은 남은 PLAN에만 적용한다.

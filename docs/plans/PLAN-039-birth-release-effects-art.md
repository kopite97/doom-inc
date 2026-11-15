---
id: PLAN-039
type: art
status: proposed
title: 생명체 탄생·침공형 방출 사건 연출 아트 적용
updated: 2026-11-16
---

# PLAN-039 생명체 탄생·침공형 방출 사건 연출 아트 적용

[사용 양식](_template-art-animation.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

배양 성공과 침공형 방출 성공을 짧고 작은 반응으로 구별한다. 기존 배양시설 Complete와 방출 메뉴 Feel 피드백을 재사용하고 부족한 지도 사건 표시만 보완한다.
시설/생명체 기본 동작·전투·귀환·음원 제작은 제외한다. 실제 생성/방출 시점·시뮬레이션 규칙을 연출에 맞춰 바꾸지 않는다.

## 기준 문서와 현재 확정 사항

- [배양](../game-design/02-systems/01-production/facilities.md), [방출](../game-design/01-direction/02-experience/core-loop.md#release).
- 선행: [PLAN-033](PLAN-033-support-creature-art.md)·[PLAN-034](PLAN-034-invader-raider-art.md), 기존 PLAN-029 Cultivation Complete와 PLAN-024 성공 피드백.
- [실제 명령](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.cs)·[방출 UI 연결](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryView.Gui.cs).
- [Feel](../agents/external-assets/feel.md) 사건 묶음, [DOTween](../agents/external-assets/dotween-pro.md) 수치 전환, [All In 1 Sprite Shader](../agents/external-assets/all-in-1-sprite-shader.md) 재질 효과를 필요한 곳에 사용한다. 공급사 원본·렌더러 전역 설정·패키지는 바꾸지 않는다.

## 디자인 의도와 원하는 모습

탄생은 새 개체 발밑의 작은 둥근 파동이 퍼졌다 사라지는 부드러운 반응을 제안한다. 시설 완료등을 또 크게 점멸시키거나 화면을 가리지 않는다.
방출은 개체가 있던 자리의 짧은 선/입자가 바깥 방향으로 흘러 소멸하는 반응이다. 실제 출구·이동 경로를 신설하지 않는다. 둥근 확산과 방향성 잔상으로 두 사건을 구분한다.
기존 아트의 절제한 강조색·큰 픽셀 덩어리를 가져오고 과한 Bloom·백색 화면 점멸·연속 카메라 흔들림을 피한다. 선택선/진행 바보다 오래 남지 않는 낮은 밀도로 제안한다. 효과색·반경·두께는 본체와 함께 비교해 확정한다.

### 동작별 움직임 설명

| 효과 | 준비 → 주요 움직임 → 마무리 | 고정/변형·리듬 |
| --- | --- | --- |
| BirthPulse | 작은 둥근 빛 → 짧은 확산 → 가장자리부터 소멸 | 위치 고정·반경 변화, 준비20%/확산40%/소멸40% 제안 |
| ReleaseTrail | 짧은 잔상 → 바깥으로 작은 파편 → 빠른 소멸 | 효과만 이동, 논리 개체/이동 보간에 Tween 없음; 1회 |

## 산출물과 재생 명세

| 효과 | 우선 제작안 | 재생 제안 |
| --- | --- | --- |
| BirthPulse | 정적 32×32 파동 마스크 PNG 1 | Scale/Alpha 450ms 1회, 투명해진 후 정리 |
| ReleaseTrail | 정적 32×32 잔상 마스크 PNG 1 | Position/Alpha 350ms 1회, 종료 후 정리 |

총 원본 2·PNG 2·각 1셀 제안이다. 프레임 시트 대신 재질/Transform 애니메이션을 우선 사용한다. 기존 리소스로 가능하면 제작 전 재사용 근거를 명시한다. 시트가 필요하면 프레임/시간/출력을 먼저 확정하고 미정 채로 게임 테스트하지 않는다.

| 사건/상태 | 시작·종료·중복 정책 |
| --- | --- |
| 탄생 | 실제 개체 생성·지도 배치 성공 이후 1회, 실패/완료 대기/복원 제외 |
| 방출 | 성공한 명령에 포함된 개체의 위치 스냅샷으로 1회, 논리 제거 지연 금지 |
| 연속 사건 | 사건 중복 방지·동시 상한 8개 제안, 초과 시 효과만 병합/생략 |
| 정지/수명 | 연구소 시간 정지/홈 복귀에 멈춤, 복귀 위상 유지; 멸망/씬 종료/새 판에 정리 |

## 사용처별 적용 계획

`Assets/Sprites/VFX/Birth/`·`Assets/Sprites/VFX/Release/`와 프로젝트 소유 재질/프리팹으로 연결한다. 성공 사건 관찰과 화면 소유 표시만 추가하며 게임 결과를 연출 완료 콜백에서 발생시키지 않는다.
본체 선택 재질을 덮어쓰지 않는 별도 효과 Renderer를 우선한다. 재질의 가변 값·Tween·Feel 수명은 대상별 소유/복원하고 같은 속성을 중복 제어하지 않는다.

## 결과물 저장 위치

아래는 이번 PLAN에서 사용할 **예정 경로**다. 위치 지정은 제작·수령 완료를 뜻하지 않으며 파일/빈 폴더는 지금 만들지 않는다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-039/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `BirthPulse` | `assets/vfx/BirthPulse/source/BirthPulse.aseprite` | `assets/vfx/BirthPulse/revisions/r001/source/BirthPulse.aseprite` | `assets/vfx/BirthPulse/revisions/r001/export/BirthPulse.png` | `assets/vfx/BirthPulse/revisions/r001/README.md` |
| `ReleaseTrail` | `assets/vfx/ReleaseTrail/source/ReleaseTrail.aseprite` | `assets/vfx/ReleaseTrail/revisions/r001/source/ReleaseTrail.aseprite` | `assets/vfx/ReleaseTrail/revisions/r001/export/ReleaseTrail.png` | `assets/vfx/ReleaseTrail/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-039/README.md`에 둔다. 아래 ID는 이 PLAN의 예정 요청 ID로 배정했으며 상세 요청서는 아직 미발행이다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `BirthPulse` | `PLAN-039-SPR-001` | `Assets/Sprites/Require/PLAN-039/PLAN-039-SPR-001/r001/BirthPulse.png` | `Assets/Sprites/VFX/Birth/BirthPulse.png` |
| `ReleaseTrail` | `PLAN-039-SPR-002` | `Assets/Sprites/Require/PLAN-039/PLAN-039-SPR-002/r001/ReleaseTrail.png` | `Assets/Sprites/VFX/Release/ReleaseTrail.png` |

수령 PNG는 GUID/참조를 보존해 최종 경로로 옮기거나 교체한다. Require는 장기 보관/런타임 참조 위치가 아니며 과거 파일은 Art 리비전에 보존한다. 기존 완료 에셋을 재사용할 때는 원본/요청을 복제하지 않는다.

BirthPulse·ReleaseTrail의 기존 도형/리소스 재사용을 채택하면 해당 신규 파일은 만들지 않는다. 표에 재사용 원본의 실제 경로와 미생성 사유를 기록하며, 위 경로는 신규 제작을 선택했을 때의 목적지다.

프로젝트 소유 재질/프리팹을 파일로 만드는 경우 Unity 루트 기준 목적지는 다음과 같다. PNG와 다른 에셋 종류이므로 Sprites에 섞지 않는다.

| 효과 | 재질 | 프리팹 |
| --- | --- | --- |
| BirthPulse | `Assets/Materials/VFX/Birth/BirthPulse.mat` | `Assets/Prefabs/VFX/Birth/BirthPulse.prefab` |
| ReleaseTrail | `Assets/Materials/VFX/Release/ReleaseTrail.mat` | `Assets/Prefabs/VFX/Release/ReleaseTrail.prefab` |

코드 구성/기존 에셋 재사용을 채택하면 실제 참조 경로와 미생성 사유를 기록한다.

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
| BirthPulse | `source-reviews/BirthPulse/r001/design.png` | `source-reviews/BirthPulse/r001/README.md` |
| ReleaseTrail | `source-reviews/ReleaseTrail/r001/design.png` | `source-reviews/ReleaseTrail/r001/README.md` |

## 작업 책임과 문서 경계

- 현재 범위는 2026-11-16 사용자가 요청한 남은 아트 PLAN 작성이다. 제작·Unity 적용·게임 테스트는 미승인/미실행이다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 신규 요청은 미발행이다. 제작 규격 확정 시 `UnityProject/Assets/Sprites/Require/PLAN-039/README.md`에 요청별 명세·최종 경로·상태를 기록한다. 위 표의 예정 요청 ID·파일 위치를 사용하며 경로 지정은 실제 수령을 뜻하지 않는다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| BirthPulse 탄생 효과 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |
| ReleaseTrail 방출 효과 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |

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
| 사건 | 배양 성공/배치 대기·방출 성공/실패·복원에서 성공 건만 1회 표시 |
| 수명 | 일반 기능 수준의 짧은 연속 사건·상한·정지/복귀·새 판 후 효과/콜백 잔류 없음 |
| 디자인 | 두 효과 형태 차이·시설 Complete와 중복 강조·선택 재질/화면 가림 없음 |
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
- 다음 행동: 성공 사건과 기존 효과를 대조해 크기·시간·리소스 재사용 여부를 확정한다.
- 로드맵: 5-5 탄생·방출 사건. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/승인 리비전·채택 규격·기준 화면은 실행 후 기록한다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

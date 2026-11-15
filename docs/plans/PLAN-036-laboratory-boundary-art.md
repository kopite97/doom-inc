---
id: PLAN-036
type: art
status: proposed
title: 연구소 바닥 외곽·공간 경계 아트 제작·적용
updated: 2026-11-16
---

# PLAN-036 연구소 바닥 외곽·공간 경계 아트 제작·적용

[사용 양식](_template-art-static.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

완료된 바닥 4변형·시설 4종을 재사용하면서 연구소의 끝과 지도 밖을 마감한다. 낮은 외곽 직선·모서리와 필요한 바깥 바탕을 포함한다.
새 벽 충돌·방·문·지도 확장·파손·시설별 건설 단계는 제외한다. PLAN-030 선택 외곽선·PLAN-031 미리보기/충돌 안내·PLAN-032 작은 진행 바는 재제작하지 않는다. 기존 상호작용 위치 표식의 정렬/가독성 확인은 포함한다.

## 기준 문서와 현재 확정 사항

- [시설 배치](../game-design/02-systems/03-space/placement.md), [바닥 r003 규격](../../UnityProject/Assets/Sprites/Require/PLAN-026/README.md), [배경 지침](../../../Art/docs/agetns/creation/backgrounds/basics.md).
- 선행 완료: PLAN-026 바닥, PLAN-027 Tilemap, PLAN-029 시설, PLAN-030~032 표시 개선.
- [LaboratoryWorldMap](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryWorldMap.cs)·[지도](../../UnityProject/Assets/Scripts/Presentation/Gameplay/Laboratory/LaboratoryMapView.cs)의 실제 경계와 전체 보기 기준을 사용한다.

## 디자인 의도와 원하는 모습

높은 벽 대신 얇은 금속 바닥 마감과 어두운 바깥 바탕으로 지하 연구실의 끝을 표현한다. 안쪽 이음선은 낮은 대비, 밖을 향한 면은 한 단계 어둡게 제안한다. 문/통로처럼 보이는 끊김이나 조작 버튼 같은 장식은 넣지 않는다.
기존 바닥 r003의 재질·픽셀 밀도·낮은 무늬 대비를 가져오고 중앙 무늬를 테두리에 복제하지 않는다. 작은 상부 하이라이트와 접촉 음영만 사용한다. 정확한 선 두께·색은 샘플과 비교해 확정한다. 바닥보다 시설·생명체·선택/충돌 표시가 먼저 읽혀야 한다.

## 정적 산출물과 공간 명세

| 대상 | 셀 수 제안 | 출력 | 규격 제안 |
| --- | --- | --- | --- |
| LaboratoryBoundary | 상/우/하/좌 직선 4 + 바깥 모서리 4 = 8 | 원본 1 → PNG 시트 1 | 64×64 셀·4×2 배열 |

- 순서 제안: Top/Right/Bottom/Left/TopLeft/TopRight/BottomRight/BottomLeft. 투명 영역·두께·피벗·접합선은 제작 전에 확정한다.
- 현재 직사각형 지도만 대상으로 하며 오목 모서리·복잡한 지형 RuleTile은 요구하지 않는다.
- 테두리는 기존 셀 바깥 장식으로 배치하고 실제 점유/통행 영역을 줄이지 않는다. 바깥 바탕은 기존 단색을 우선 재사용한다.
- 방향별 원화로 명암을 보존하고 고정 셀 좌표에 배치한다. 카메라·갱신·복원으로 무늬가 바뀌거나 게임 난수/저장이 변경되지 않는다.

## 사용처별 적용 계획

`Assets/Sprites/World/LaboratoryBoundary/LaboratoryBoundary.png`를 기존 월드 표시의 경계 레이어에 연결한다. 정렬과 바닥 접합을 명시하고 Collider/Raycast는 추가하지 않는다.
네 모서리·긴 변·끝 칸 시설·회전 입구·배치 불가 칸을 대조한다. 전체 보기 여백은 필요한 표시 범위만 조정하고 논리 지도 크기·접근 판정은 유지한다.

## 결과물 저장 위치

아래는 이번 PLAN에서 사용할 **예정 경로**다. 위치 지정은 제작·수령 완료를 뜻하지 않으며 파일/빈 폴더는 지금 만들지 않는다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

- Art 루트: `C:/Dev/Unity/Art/`
- Unity 루트: `C:/Dev/Unity/Doom_Inc/UnityProject/`
- 검수 루트 E: `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-036/`
- 최초 제출은 에셋별 `r001`, 수정 제출은 `r002`부터 증가한다. 승인본을 덮어쓰지 않고 실제 리비전을 요청/검수 기록에 명시한다.

### Art 원본과 리비전별 제출본

아래 경로는 Art 루트 기준이다. 제출 명세에는 태그/시간·좌표·출력 파일 목록·해시·Unity 요청 대응을 기록한다.

| asset_id | 작업용 원본 | r001 원본 보존본 | r001 PNG 제출본 | r001 제출 명세 |
| --- | --- | --- | --- | --- |
| `LaboratoryBoundary` | `assets/backgrounds/LaboratoryBoundary/source/LaboratoryBoundary.aseprite` | `assets/backgrounds/LaboratoryBoundary/revisions/r001/source/LaboratoryBoundary.aseprite` | `assets/backgrounds/LaboratoryBoundary/revisions/r001/export/LaboratoryBoundary.png` | `assets/backgrounds/LaboratoryBoundary/revisions/r001/README.md` |

### Unity 수령과 최종 적용

아래 경로는 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-036/README.md`에 둔다. 아래 ID는 이 PLAN의 예정 요청 ID로 배정했으며 상세 요청서는 아직 미발행이다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `LaboratoryBoundary` | `PLAN-036-SPR-001` | `Assets/Sprites/Require/PLAN-036/PLAN-036-SPR-001/r001/LaboratoryBoundary.png` | `Assets/Sprites/World/LaboratoryBoundary/LaboratoryBoundary.png` |

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
| LaboratoryBoundary | `source-reviews/LaboratoryBoundary/r001/design.png` | `source-reviews/LaboratoryBoundary/r001/README.md` |

## 작업 책임과 문서 경계

- 현재 범위는 2026-11-16 사용자가 요청한 남은 아트 PLAN 작성이다. 제작·Unity 적용·게임 테스트는 미승인/미실행이다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 신규 요청은 미발행이다. 제작 규격 확정 시 `UnityProject/Assets/Sprites/Require/PLAN-036/README.md`에 요청별 명세·최종 경로·상태를 기록한다. 위 표의 예정 요청 ID·파일 위치를 사용하며 경로 지정은 실제 수령을 뜻하지 않는다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| LaboratoryBoundary 직선·모서리 세트 | 제안/미착수 | 미수령 | 미확인 | 미적용 | 미실행 | 미확인 |

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
| 연결 | 직선 반복·네 모서리·기존 4변형 바닥에서 틈/겹친 선/강한 반복 무늬 없음 |
| 공간·입력 | 끝 칸 시설 4방향·접근 위치·충돌 칸·빈 땅 선택 해제·드래그 보존 |
| 표시 안정 | 기본/최대 줌/전체 보기·가로/세로·이어하기에서 낮은 대비와 동일 외곽 |
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
- 다음 행동: 외곽 8셀과 바깥 장식 배치를 검토한다. 완료 시 기존 표시 개선과 함께 5-2 잔여 범위를 대조해 단계 완료를 판정한다.
- 로드맵: 5-2 남은 공간 표현. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/승인 리비전·채택 규격·기준 화면은 실행 후 기록한다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

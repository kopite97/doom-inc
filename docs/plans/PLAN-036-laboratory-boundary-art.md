---
id: PLAN-036
type: art
status: completed
title: 연구소 바닥 외곽·공간 경계 아트 제작·적용
updated: 2026-11-17
---

# PLAN-036 연구소 바닥 외곽·공간 경계 아트 제작·적용

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
- 사용자 후속 요청으로 [Art PLAN-013](../../../Art/docs/plans/PLAN-013-laboratory-boundary.md)의 원본 제작을 진행한다. Unity 적용·정식 검증 유보는 유지한다.

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

- r001 순서: Top/Right/Bottom/Left/TopLeft/TopRight/BottomRight/BottomLeft. 셀 중심 피벗(0.5,0.5), 기존 바닥 외부10px 띠/10×10 모서리, 나머지 투명. 외부 인접 셀 배치·좌표/접합은 [Art 제출 명세](../../../Art/assets/backgrounds/LaboratoryBoundary/revisions/r001/README.md)에 기록했다.
- 현재 직사각형 지도만 대상으로 하며 오목 모서리·복잡한 지형 RuleTile은 요구하지 않는다.
- 테두리는 기존 셀 바깥 장식으로 배치하고 실제 점유/통행 영역을 줄이지 않는다. 바깥 바탕은 기존 단색을 우선 재사용한다.
- 방향별 원화로 명암을 보존하고 고정 셀 좌표에 배치한다. 카메라·갱신·복원으로 무늬가 바뀌거나 게임 난수/저장이 변경되지 않는다.

## PLAN-042에서 실행할 사용처별 적용 계획

`Assets/Sprites/World/LaboratoryBoundary/LaboratoryBoundary.png`를 기존 월드 표시의 경계 레이어에 연결한다. 정렬과 바닥 접합을 명시하고 Collider/Raycast는 추가하지 않는다.
네 모서리·긴 변·끝 칸 시설·회전 입구·배치 불가 칸을 대조한다. 전체 보기 여백은 필요한 표시 범위만 조정하고 논리 지도 크기·접근 판정은 유지한다.

## 결과물 저장 위치

아래 Art 원본·r001·미리보기는 생성했고 Unity 수령/최종 적용은 **예정 경로**다. 실제 파일은 [인계 색인](evidence/PLAN-036/README.md)에서 연결한다. 기준 루트와 표의 경로를 결합하면 실제 절대 경로가 된다.

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

아래 경로는 PLAN-042에서 일괄 수령·적용할 Unity 루트 기준이다. 요청 명세/진행 기록은 `Assets/Sprites/Require/PLAN-036/README.md`에 둔다. 아래 ID의 상세 요청서를 발행했으며 PNG 수령은 PLAN-042까지 유보한다. 재사용으로 제작하지 않는 항목은 해당 ID를 다른 에셋에 재배정하지 않는다.

| asset_id | 요청 ID | r001 수령 PNG | 최종 적용 PNG |
| --- | --- | --- | --- |
| `LaboratoryBoundary` | `PLAN-036-SPR-001` | `Assets/Sprites/Require/PLAN-036/PLAN-036-SPR-001/r001/LaboratoryBoundary.png` | `Assets/Sprites/World/LaboratoryBoundary/LaboratoryBoundary.png` |

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
| 첫 실행의 가로/세로 대표 게임 화면 | `../PLAN-042/runs/run-001/screenshots/PLAN-036-landscape.png`, `../PLAN-042/runs/run-001/screenshots/PLAN-036-portrait.png` |
| 필요한 경우 실제 재생 검수 영상 | `../PLAN-042/runs/run-001/videos/game-flow.mp4` |
| 실행한 검증 결과·로그 | `../PLAN-042/runs/run-001/tests/editmode.xml`, `../PLAN-042/runs/run-001/tests/playmode.xml`, `../PLAN-042/runs/run-001/logs/editor.log` |

실행하지 않는 검증의 파일은 만들지 않고 validation.md에 해당 없음/미실행을 기록한다. 다음 실행은 run-002부터 증가하여 이전 증거를 덮어쓰지 않는다. 장면/언어별 추가 캡처는 해당 실행 README에 파일명과 조건을 등록한다.

| 제출받은 원본 비교 대상 | E 기준 리비전별 시각 비교 | 비교 조건·원본 승인 기록 |
| --- | --- | --- |
| LaboratoryBoundary | `source-reviews/LaboratoryBoundary/r001/design.png` | `source-reviews/LaboratoryBoundary/r001/README.md` |

## 작업 책임과 문서 경계

- 2026-11-16 사용자가 PLAN-034 r002를 확인하고 PLAN-036 아트 제작을 요청했다. 이번 범위는 Art 원본·리비전·명세·제작 미리보기다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 이전 아트와 같은 흐름의 PLAN-036 제작 요청을 이 에셋의 Aseprite 제작 위임으로 기록한다. 최종 외형 승인은 사용자에게 받는다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- [요청 README](../../UnityProject/Assets/Sprites/Require/PLAN-036/README.md)에 SPR-001 및 Art PLAN-013 r001 원본/출력·최종 경로를 기록했다. PNG 미수령·미적용이다.

## 리소스 준비와 진행 요약

승인본 수령·Unity 적용·일반 Editor 기술 검증·사용자 실제 게임 화면 승인 완료(2026-11-17). 리비전/수령/최종 경로는 [최종 대응표](evidence/PLAN-042/asset-manifest.csv), 개별 완료 기준은 [49개 기준 대응](evidence/PLAN-042/plan-verification-matrix.csv)에 기록했다. 기존 승인본과 저장/설정을 보존했다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [x] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 에셋별 원본·리비전·수령·최종 적용·검수 자료의 예정 경로 지정
- [x] 수량·사용처·규격 확정, 상세 요청/Art 제작 기록 연결
- [x] 전체 필수 Art 원본·리비전 PNG·제출 명세·미리보기 제작
- [x] Art 파일 경로·요청 ID·리비전 인계 목록 작성, 제작 종료 후 on hold 전환
- [x] 사용자에게 바닥/시설 합성·모서리 미리보기 제시(정적 에셋으로 GIF 해당 없음) → 피드백 반영 → 해당 묶음의 모든 에셋 리비전 확인 기록(다음 제작 전 필수)
- [x] PLAN-042: 원본/출력 기술 검증 후 Unity 일괄 수령·정규 경로·씬/SO·생성 도구 연결
- [x] PLAN-042: 전체 적용 후 일괄 기술 검증·원본/게임 화면 사용자 확인
- [x] PLAN-042: 필요한 수정본·재검증·환경 복원·완료 범위 증거 연결 후 completed 판정

## PLAN-042에서 확인할 완료 기준과 검증 방법

다음 항목의 검증 실행 주체는 PLAN-042다. 원본·출력·알파·시간·좌표 등 리소스 기술 검증도 마지막 단계에서 모아 수행하며, 제작 중 저장/미리보기 확인 및 제작 직후 사용자 원본 확인과 구분한다.

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
- 원본 승인: r001 사용자 확인 완료(2026-11-16). 게임 화면 승인·정식 검증은 PLAN-042에 남긴다.
- 문서 검사: 2026-11-16 문서 검사 통과(PLAN 42개·active 0개·목록 42개·로컬 링크 3,888개). 변경 형식 검사 통과. 기존 Unity/Art PLAN 40개가 작업 전 해시와 같음을 확인했다.
- 경로 보완 검사(2026-11-16): 문서 검사 통과(42 PLAN·active 0·로컬 링크 3,904개), 25개 제작 후보의 원본/제출/수령/최종 적용/원본 비교 경로 누락·중복 없음, 기존 완료 PLAN 40개 해시 불변 확인. 예정 결과물은 생성하지 않았다.
- 제작 환경: 새 Art 원본/리비전/미리보기와 문서만 작성했다. 기존 승인 원본·Unity 코드/씬/SO/설정·게임 저장은 변경하지 않았다.

## 현재 상황과 다음 행동

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 원본·제출본·수령·최종 적용·검수 경로 지정. 파일 생성/제작/적용 없음 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

- 2026-11-16: 사용자 요청에 따라 제작 선행, Unity 적용·정식 검증은 PLAN-042로 통합. 미제작/proposed 유지. 제작 후 on hold 및 최종 증거에 의한 completed 전환을 구분했다.

- 2026-11-16: 사용자 「작업이 끝나고 내가 확인하는 과정도 필요해」 반영. 각 제작 묶음 직후 미리보기/재생 제시와 원본 확인을 필수로 두며, 응답 전 다음 제작으로 넘어가지 않는다. Unity 적용·정식 통합 검증은 PLAN-042에 유지한다.

- 2026-11-16: proposed → active. 사용자 PLAN-034 r002 확인 완료 및 PLAN-036 제작 요청. Art PLAN-013 원본8셀·미리보기 제작, 사용자 확인 전 다음 제작 금지. Unity 적용·정식 검증은 PLAN-042.

- 2026-11-16: active → on hold. LaboratoryBoundary r001 정적8셀·원본1·PNG1·명세·제작 미리보기 준비. 바닥 밖10px 마감, 기존 아트 읽기 합성만 수행. 사용자 확인 대기·Unity 미적용·정식 검증 미실행.

- 2026-11-16 사용자 「좋아. r001로 진행하자. 확인 완료로 기록하고 다음 PLAN-037 아트 작업을 진행해줘」. r001 외형 승인 완료, Unity 미적용·정식 검증 미실행으로 on hold 유지.

## 최종 완료 — 2026-11-17

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

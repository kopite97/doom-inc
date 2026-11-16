---
id: PLAN-042
type: art
status: completed
title: 정식 아트 일괄 적용·전체 통합 검증·잔여 임시 표현 정리
updated: 2026-11-17
---

# PLAN-042 정식 아트 일괄 적용·전체 통합 검증·잔여 임시 표현 정리

[사용 양식](_template-art-ui.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

PLAN-034·036~041에서 먼저 제작한 모든 아트 리소스를 Unity에 한 묶음으로 수령·적용하고, 원본/출력 기술 검사부터 실제 게임 통합 검증까지 마지막에 모아 수행한다. 이 PLAN이 해당 리소스의 임포트·슬라이싱·SO/씬/프리팹/재질·표시 코드·생성 도구 연결을 담당한다. 기존 개별 PLAN의 적용 요구·완료 기준을 삭제하지 않고 통합 실행하며, 잔여 임시 참조·스타일 불일치·합성 문제를 정리한다.
완료 PLAN 본문·과거 증거는 수정하지 않는다. 발견한 결함/수정 리비전은 이 PLAN과 해당 요청 README에 남기며 원본 변경은 사용자 제작/위임 범위에서 별도 리비전으로 받는다.

<a id="batch-art-workflow"></a>

## 제작 우선·일괄 적용 운영

이 절차는 2026-11-16 사용자 지정으로 남은 PLAN-034·036~042에 적용한다. 개별 PLAN의 기존 「제작 후 즉시 수령·연결·검증」 순서보다 우선하며, 이미 완료된 PLAN의 적용/승인을 되돌리지 않는다.

1. **제작 순서:** PLAN-034 → PLAN-036 → PLAN-037 → PLAN-038 → PLAN-039 → PLAN-040 → PLAN-041. Art 소스폴더의 에셋별 PLAN으로 원본·리비전 PNG·명세·미리보기를 만든다. Art active도 최대 1개이며 독립 에셋은 순차 작업한다.
2. **제작 중 경계:** 파일 저장·열기·출력 누락 확인·미리보기 조정·문서 검사까지만 한다. 원본/출력의 정식 기술 검증과 게임 화면 최종 시각 검수는 여기서 모아 수행한다. **각 제작 묶음이 끝나면 에셋별 미리보기·애니메이션 GIF·리비전과 변경점을 사용자에게 제시하고 응답을 기다린다. 수정은 새 리비전으로 보존해 다시 확인받으며, 해당 묶음의 원본 외형·동작 확인 전 다음 제작 PLAN을 시작하지 않는다.** 원본 승인 리비전·사용자 응답·날짜는 Art PLAN과 요청 README에 기록한다. Unity 복사/임포트·연결 코드/에셋 변경·게임 테스트는 하지 않는다.
3. **인계와 상태:** 모든 필수 원본·제출본·명세·경로/리비전 목록이 만들어진 제작 PLAN은 `on hold`로 둔다(사유: 사용자 지정 일괄 적용·검증 대기). 새 상태값을 만들거나 제작만으로 `completed`를 기록하지 않는다. Art PLAN도 필수 기술/시각 확인이 남으면 같은 의미로 대기를 기록한다. 제작 묶음의 사용자 확인을 마친 뒤 순차 실행 요청 범위 안에서 다음 PLAN만 active로 전환한다.
4. **PLAN-042 시작 조건:** 7개 제작 PLAN의 인계 준비와 각 묶음의 사용자 원본 확인이 모두 충족돼야 한다. 선택 산출물을 재사용/제외했으면 근거와 실제 참조를 포함한다. 앞 PLAN의 completed·Unity 적용·게임 검증을 조건으로 두지 않아 순환 의존을 피한다. 이번 문서 수정만으로 제작이나 PLAN-042를 활성화하지 않는다.
5. **일괄 수령·적용:** 전체 제출 리비전을 목록으로 고정하고 파일·알파·셀/태그/시간·좌표·원본/출력 대응을 모아 검증한다. 원래 요청 경로로 수령한 뒤 최종 Sprite 경로에 GUID를 보존해 배치하고, 아래 표의 모든 표시 코드·연결·생성 도구 작업을 수행한다. 「한번에」는 하나의 통합 적용 단계이며 파일을 동시에 쓰거나 거대한 한 번의 코드 수정으로 처리한다는 뜻은 아니다.
6. **게임 검증 시작 조건:** 모든 필수 대상의 임포트·참조·표시 연결이 완료된 뒤에만 Edit/Play·수동 게임 실행·캡처를 시작한다. 준비 중 필요한 Editor 재컴파일/임포트와 정적 참조 확인은 연결 작업이다. 일부 묶음이 연결됐다는 이유로 부분 게임 테스트를 먼저 실행하지 않는다.
7. **일괄 검증·완료:** 각 제작 PLAN의 검증 표와 아래 통합 시나리오를 합쳐 중복을 줄이고, 하나의 테스트 결과가 충족한 PLAN/항목을 명시한다. 실패가 있으면 필요한 수정·영향 범위 재검증을 수행한다. 「한번에 검증」은 검증 단계 통합이며 실패 후 재검증 생략을 뜻하지 않는다. 환경 복원·승인 원본과 적용 리비전 대응·게임 화면 최종 확인 후 각 on hold PLAN에 결과를 연결해 completed로 정리하고 마지막에 PLAN-042를 완료한다. 상태 기록을 위해 여러 PLAN을 active로 만들지 않는다.

기존 원본·제출 리비전·요청 ID·최종 경로는 각 제작 PLAN의 표를 유지한다. 특히 PLAN-037은 PLAN-014의 기존 6개 요청 ID를 재사용한다. 검증 결과·게임 화면은 이 PLAN에 집중하고 원래 PLAN/요청에는 결과 링크만 남긴다.

## 제작 PLAN별 일괄 적용·검증 책임

| 제작 출처 | PLAN-042에서 수행할 적용 | 놓치지 않을 검증 |
| --- | --- | --- |
| [PLAN-034](PLAN-034-invader-raider-art.md) | Invader/Raider r002 4방향 Idle/Move 분할·정의별 방향/재생 연결·SO Icon·배양/생명체/침공 목록·생성 도구 | 비Worker 대기·실제 이동 상태 연결 범위와 미연결 명시(이동 AI 임의 추가 금지), 방출 후 지도 제거, 밀집/선택, 정지·복원, 종류별 식별 |
| [PLAN-036](PLAN-036-laboratory-boundary-art.md) | 외곽 8셀·경계 레이어·정렬·필요 표시 여백 | 직선/모서리·바닥4변형 연결, 끝 칸 시설4방향/입구, 드래그·충돌·빈 땅, 줌·복원 |
| [PLAN-037](PLAN-037-laboratory-menu-icons-art.md) | 기존 6개 메뉴/카메라 아이콘·UI 배열·AppConfiguration/씬·생성 도구·무참조 Temp 정리 | 실제 작은 크기·상태·터치 영역·가로/세로·언어/큰 글씨·기존 Build 보존 |
| [PLAN-038](PLAN-038-shared-ui-skin-art.md) | 공용6개 PNG·기존 Fill 재사용·9-slice·위젯/PopupHost·화면별 예외 | HUD/목록/팝업의 입력·스크롤·안전 영역·언어/글씨 크기·상태별 대비 |
| [PLAN-039](PLAN-039-birth-release-effects-art.md) | Birth/Release Sprite·프로젝트 재질/프리팹·성공 사건 연결·연출 수명 | 성공1회/실패0회, 배치 대기/복원, 상한·정지·복귀·새 판·콜백 잔류·중복 강조 |
| [PLAN-040](PLAN-040-title-loading-art.md) | 가로/세로 타이틀·로딩 장식·직렬화 참조/AppSceneAssets | 크롭/안전 영역·큰 글씨·이어하기 유무·오류/재시도·실제 진행·씬 왕복 |
| [PLAN-041](PLAN-041-destruction-results-art.md) | 세계 변화 단계·Ending.Progress 표시·결과 프레임·공용 UI 재사용 | 종료→결과→새 판1회, 중간 방향 전환/홈 복귀, 긴 수치·저장 실패/재시도 |

각 행은 범위 요약이다. 해당 PLAN의 「PLAN-042에서 실행할 사용처별 적용 계획」과 「PLAN-042에서 확인할 완료 기준과 검증 방법」의 모든 항목을 최종 대응표에 포함한다. 검증을 이관하면서 대상·수량·게임 규칙·완료 기준을 줄이지 않는다.

## 기준 문서와 선행 조건

- [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md), [검증](../agents/verification.md), [아트 요청](../agents/art-assets.md).
- 제작 선행 필수: PLAN-034·036~041의 전체 필수 Art 원본·리비전 PNG/시트·제출 명세·미리보기·요청 대응 목록. 각 제작 PLAN은 사용자 원본 확인과 인계 준비가 끝난 on hold여도 진입 가능하다. 이들의 Unity 적용·검증·completed를 선행 조건으로 요구하지 않는다.
- 이미 완료한 PLAN-033·035와 PLAN-026/029·PLAN-030~032 결과는 보존·회귀 대상으로 사용한다. 이번 일괄 적용을 위해 기존 완료를 취소하거나 다시 제작하지 않는다.
- [기존 승인본](../../UnityProject/Assets/Sprites/Require/PLAN-026/README.md)·[시설 승인본](../../UnityProject/Assets/Sprites/Require/PLAN-029/README.md)·[UI 요청](../../UnityProject/Assets/Sprites/Require/PLAN-014/README.md)을 리비전 원본으로 사용한다.
- UI 양식에 정적/애니메이션의 공간·재생 검수를 합친다. 신규 제작물 기본 0개이며 실제 결함이 있을 때만 수정본을 추가한다.

## 디자인 의도와 원하는 모습

연구소에서는 바닥/외곽보다 시설·생명체·실제 상태가, UI에서는 장식보다 수치·선택·행동이 먼저 읽혀야 한다. 진입·운영·멸망·결과의 픽셀 밀도·재질·강조가 일관되게 이어져야 한다.
승인 외형을 통일성 때문에 일괄 재색칠하지 않는다. 의도된 화면 차이는 각 디자인 설명/참고 버전과 대조하고 실제 문제가 있는 사례만 수정 대상으로 제시한다. 기준은 기존 승인본·PLAN-034/036~041의 Art 제출본과 이 PLAN에서 처음 확보할 최종 화면이며 임시 이미지/예시 색/미사용 상태 아이콘은 기준에서 제외한다.

## 산출물과 적용 목록

| 대상 | 결과 |
| --- | --- |
| 에셋 대응표 | Art PLAN/asset_id·원본/출력 리비전·Unity 요청 ID·최종 경로·사용처·승인 |
| 실제 화면 | 타이틀/로딩/연구소/팝업/멸망/결과의 가로·세로와 사용 리비전 |
| 잔여 임시 표현 | 실제 참조된 Temp·대체 도형·누락 Icon의 유지 사유/교체 결과 |
| 수정본 | 문제·영향·리비전·기술/시각 재확인 |
| 환경/한계 | 저장/설정·Editor 복원, Android/고부하 미검증 또는 승인 후 실제 결과 |

각 제작 PLAN의 리소스 표를 모아 원본/출력 기술 검증·일괄 수령/적용·전체 참조 확인 일시를 남긴다. 수령 폴더에 PNG가 없다는 이유로 유실 판정하지 않고 정규 경로/Art 보관본을 대조한다. 완료 Art PLAN의 오래된 링크 정리는 제외한다.

## 통합 검수 시나리오

| 흐름 | 기대 결과 |
| --- | --- |
| 타이틀→로딩→연구소 | 크롭·글자 대비·실제 진행·씬 수명 유지 |
| 건설→가공→배양→운반 | 시설4종/생명체4종/자원2종의 실루엣·접지·방향·행동 일치 |
| 선택/빈 땅/배치 | 외곽선·드래그·충돌 안내·상호작용 위치·작은 바 유지 |
| 목록/상세/팝업 | 아이콘·프레임·언어·큰 글씨·입력 차단 유지 |
| 방출→멸망→결과→새 판 | 사건과 효과 일치, 프레임/재질/효과/콜백 잔류 없음 |
| 이어하기·홈 복귀 | 실물/진행/표현 일치, 과거 탄생/방출 재생 없음 |

## 검증 범위와 완료 판정

- 일반 Editor 기능·입력·수명과 사용자 실제 화면 검수가 기본이다. 폐기된 ON/OFF·표식 재탭 테스트를 복원하지 않는다.
- 가로/세로·ko/en·보통/큰 글씨·기본/최대 줌/전체 보기에서 대표 조합을 고르고 실제 해상도/배율·시나리오를 기록한다.
- 새 Player/Android 빌드·실기기 APK·고부하/반복 계측은 현재 요청이 없어 기본 필수에서 제외한다. 요청 시 기기·부하·시간·승인 근거·완료 조건을 구체화한다.
- Editor 완료를 Android 표시·셰이더 변형·성능 통과로 쓰지 않는다. 로드맵 5-7은 Editor 통합 완료와 플랫폼 검증 대기를 분리하고 전체 플랫폼 완료는 실제 결과가 있을 때만 기록한다.
- PLAN-025 성능 결과를 새 아트 증거로 복사하지 않는다. 알려진 큰 글씨/GC 문제와 새 아트 회귀를 구분하며 범위 밖 최적화는 별도 후속으로 남긴다.

## 결과물 저장 위치

이 PLAN은 별도 그림 원본을 중복 제작하지 않으며, 제작 PLAN의 원본/PNG를 한 묶음으로 수령·적용한다. 아래 선행 PLAN의 「결과물 저장 위치」에 지정한 Art 원본·리비전·Unity 수령/최종 적용 경로를 그대로 사용한다. PLAN-042 명의의 아트 복제본·Require 폴더는 만들지 않는다.

| 대상 | 원본·리비전·수령·최종 적용 경로의 원본 |
| --- | --- |
| PLAN-033 결과물 | [경로 표](PLAN-033-support-creature-art.md#결과물-저장-위치) |
| PLAN-034 결과물 | [경로 표](PLAN-034-invader-raider-art.md#결과물-저장-위치) |
| PLAN-035 결과물 | [경로 표](PLAN-035-culture-medium-art.md#결과물-저장-위치) |
| PLAN-036 결과물 | [경로 표](PLAN-036-laboratory-boundary-art.md#결과물-저장-위치) |
| PLAN-037 결과물 | [경로 표](PLAN-037-laboratory-menu-icons-art.md#결과물-저장-위치) |
| PLAN-038 결과물 | [경로 표](PLAN-038-shared-ui-skin-art.md#결과물-저장-위치) |
| PLAN-039 결과물 | [경로 표](PLAN-039-birth-release-effects-art.md#결과물-저장-위치) |
| PLAN-040 결과물 | [경로 표](PLAN-040-title-loading-art.md#결과물-저장-위치) |
| PLAN-041 결과물 | [경로 표](PLAN-041-destruction-results-art.md#결과물-저장-위치) |

기존 PLAN-026/029와 Build의 실제 경로는 각 완료 요청 README의 최종 적용 기록을 참조한다. 수정이 필요하면 원래 asset_id·요청 ID의 다음 리비전에 제출하고 최종 PNG를 같은 경로에 교체한다. 이 PLAN의 manifest에는 실제 적용 리비전과 원본 증거 링크를 기록한다.

검수 루트 E는 `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-042/`다. 실제 결과는 아래 색인과 실행별 기록에 보존했다.

| 결과물 | E 기준 저장 경로 |
| --- | --- |
| 전체 통합 검수 색인 | `README.md` |
| 전체 에셋의 Art/Unity 경로·요청·리비전·승인 대응 | `asset-manifest.csv` |
| 7개 제작 PLAN의 제작/인계 준비·필수/재사용/제외 대응 | `production-handoff.md` |
| 적용 순서·임포트/참조·생성 도구 작업과 전체 연결 판정 | `integration.md` |
| 제작 PLAN별 완료 기준 ↔ 실행 테스트/시각 증거/미해결 항목 | `plan-verification-matrix.csv` |
| 실제 잔여 Temp·대체 도형·유지/정리 판정 | `temporary-art-audit.md` |
| 수정 필요 항목·수정 리비전·재검수 결과 | `issues.md` |
| 기술 검증과 전체 사용자 시각 승인 | `validation.md` |
| 원본/참조·저장/설정·Editor 보존/복원 | `environment.md` |
| 첫 통합 실행의 조건/대상 리비전/시나리오 | `runs/run-001/README.md` |
| 실행한 검증 결과/로그 | `runs/run-001/tests/editmode-status.json`, `runs/run-001/tests/playmode.xml`, `runs/run-005/tests/playmode.xml`, `runs/run-005/logs/editor.log` |
| 실제 재생 증거 | 프레임 진행/정지/재진입 테스트 XML과 최종 화면. 별도 영상은 미제작 |

가로/세로 대표 화면은 각 파일로 구분한다. 각 제작 PLAN 전용 증거가 필요하면 같은 실행 폴더의 `screenshots/PLAN-034-landscape.png`·`PLAN-034-portrait.png` 형식으로 저장한다(036~041도 같은 형식). 공통 화면으로 충분하면 대응표에서 재사용하고 불필요한 중복 캡처를 만들지 않는다.

| 화면 | E 기준 가로 | E 기준 세로 |
| --- | --- | --- |
| title | `runs/run-005/screenshots/title-landscape.png` | `runs/run-005/screenshots/title-portrait.png` |
| loading | `runs/run-005/screenshots/loading-landscape.png` | `runs/run-005/screenshots/loading-portrait.png` |
| laboratory | `runs/run-005/screenshots/laboratory-landscape.png` | `runs/run-005/screenshots/laboratory-portrait.png` |
| popup | `runs/run-005/screenshots/popup-landscape.png` | `runs/run-005/screenshots/popup-portrait.png` |
| destruction | `runs/run-005/screenshots/destruction-landscape.png` | `runs/run-005/screenshots/destruction-portrait.png` |
| results | `runs/run-005/screenshots/results-landscape.png` | `runs/run-005/screenshots/results-portrait.png` |

재검수는 run-002부터 증가하고, 다른 언어/배율 캡처는 해당 실행 README에 이름과 조건을 기록한다. 미실행 테스트의 결과 파일은 만들지 않는다. 원본 시각 증거는 선행 PLAN의 source-reviews 또는 실제 Art 검수 경로를 연결한다. 빌드/실기기·고부하가 별도로 승인되면 실행 전 그 산출물 경로를 이 표에 추가한다.

## 작업 책임과 문서 경계

- 2026-11-16 사용자 「모든 아트리소스를 먼저 작업한 다음 한번에 Unity에 적용」「검증은 마지막 Art플랜에서 한번에」 요청을 반영했다. 당시 문서만 수정했고, 2026-11-17 사용자 실행 승인으로 active 전환했다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 신규 제작 요청은 없다. 수정본이 필요하면 선행 PLAN의 원래 요청 ID에서 리비전으로 추적한다.

## 리소스 준비와 진행 요약

승인본 수령·Unity 적용·일반 Editor 기술 검증·사용자 실제 게임 화면 승인 완료(2026-11-17). 리비전/수령/최종 경로는 [최종 대응표](evidence/PLAN-042/asset-manifest.csv), 개별 완료 기준은 [49개 기준 대응](evidence/PLAN-042/plan-verification-matrix.csv)에 기록했다. 기존 승인본과 저장/설정을 보존했다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [x] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 재사용 아트의 경로 원본·통합 검수 자료 위치 지정
- [x] 전체 리비전/참조 목록 확정, 선행 요청/Art 검수 기록 연결
- [x] 전체 원본/출력 기술 검증·리비전 대조·미제작/재사용/선택 제외 사유 확정
- [x] 원래 요청 ID별 Unity 일괄 수령·임포트·슬라이싱·최종 경로 이동
- [x] 각 제작 PLAN의 모든 사용처·코드·재질·씬/SO·생성 도구를 일괄 연결하고 전체 참조 준비 판정
- [x] 전체 적용 후 개별 PLAN별 검증 항목과 공통 회귀를 합쳐 실행·결과 매핑
- [x] 원본·실제 게임 화면 사용자 시각 확인 및 결함 수정/영향 범위 재검증
- [x] 환경 복원·최종 리비전·해결된 기술 결함과 PLAN별 근거 기록
- [x] on hold 제작 PLAN의 사용자 최종 게임 화면 승인 충족
- [x] 해당 제작 PLAN·요청·Art 제작 기록의 완료/남은 상태 및 로드맵 동기화 후 PLAN-042 completed 판정

## 완료 기준과 검증 방법

| 기준 | 실행할 검증·기대 결과 |
| --- | --- |
| 디자인 일치 | 원하는 실루엣·구성·재질·명암·금지 요소를 실제 최소 표시 크기에서 대조, 사용자 확인 |
| 제작/참조 | 원본·출력·알파·셀/시간·기준점·임포트/플랫폼 override·누락 Sprite와 전체 사용처 확인 |
| 전체 목록 | 최종 리비전과 모든 사용처 일치, 미해결 필수 아트/검수 0건 |
| 전체 흐름 | 정상 한 판·이어하기·방향 전환·새 판의 일반 기능/화면 증거 |
| 사용자 확인 | 전체 디자인/동작·간결성 승인과 미검증 플랫폼 범위 분리 기록 |
| 보존/재생성 | 기존 승인본 해시/GUID·하위 Sprite 참조 유지, 씬/정의 생성 도구 후 정식 연결 보존 |
| 완료 판정 | 전체 필수 대상 적용·기술 검증·사용자 게임 화면 승인과 환경 복원 완료 |

- 기본 검증은 일반 Editor 범위다. Player 컴파일·빌드·실기기·고부하 계측은 현재 명시적 요청이 없어 기본 실행/필수 완료 조건에서 제외한다. 요청 시 범위와 승인 근거를 기록한다.
- 연결 코드 변경 전 [추상화](../agents/abstraction.md)·[객체 설계](../agents/object-design.md)·[Unity 에셋](../agents/unity-assets.md)·[Localization](../agents/localization.md)의 해당 경계를 읽고 최소 변경만 수행한다.
- 원본/리비전·GUID·저장/설정·Editor 실행 상태를 작업 전 기록하고 종료 시 복원한다. 원본을 자동 리사이즈/재색칠해 규격에 맞추지 않는다.

## 검증 결과와 사용자 확인

- 2026-11-17: 승인 원본21개/출력 픽셀 불일치0, 수령 해시21개 일치. 7개 제작 PLAN의 49개 기준을 [대응표](evidence/PLAN-042/plan-verification-matrix.csv)에 연결했다.
- Edit Mode464개 통과, Play Mode 고유100개 항목 최종 통과. 초기 실패·후속 수정·재검증은 [검증 결과](evidence/PLAN-042/validation.md)와 각 run에 보존했다. 첫 Edit Mode는 실제 JSON만 확보했으며 XML을 소급 생성하지 않았다.
- 타이틀 큰 글씨·스크롤 위치 수정 후 관련6개 테스트 최종 통과. 가로/세로 실제 게임 화면16장을 run-005에 보존했다.
- WorldDestruction r002 원본 승인 기록 완료. **실제 Unity 적용 화면 사용자 확인 완료(2026-11-17)**. 사용자 응답은 완료 승인 기록에 보존했다.
- 저장 파일·ProjectSettings·Editor Play/씬/백그라운드 설정 복원. [환경](evidence/PLAN-042/environment.md). 기존 승인 아트·작업 전 미커밋 변경 유지.
- Android/Player/콘텐츠 빌드·실기기·고부하 검증은 미실행. 일반 Editor 결과와 플랫폼 검증을 구분한다.

## 현재 상황과 다음 행동

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 재사용 원본 경로·통합 검수 파일 위치 지정. 검수는 미실행 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

- 2026-11-16: 사용자 요청으로 마지막 일괄 적용·검증 담당으로 확장. 선행 조건을 제작/인계 준비로 수정하고 7개 PLAN의 적용·정식 검증·완료 증거를 통합했다. 이번에는 문서만 수정, proposed 유지.

- 2026-11-16: 사용자 후속 지시로 제작 직후 원본 확인 단계를 필수화했다. 원본 확인 전 다음 제작 금지, 승인 리비전/응답 기록, 일괄 적용 후 게임 화면 확인과 구분한다. 기존 승인 원본을 변경하지 않았다면 동일 외형 승인을 반복 요청하지 않는다.

- PLAN-040 r002 인계: 타이틀2종 Ambient·로딩 Bubbles 각16프레임×150ms·2.4초, 4×4 시트. Multiple 분할·최장변2560 보존(Max Size4096이상)·Sprite 배열/시간 연결·숨김/씬이탈 해제·재진입 초기화·가로세로 위상·반복경계 확인을 마지막 적용/검증에 포함한다. 실제 로딩 진행/문구/Feel 웨이브는 유지. 2026-11-16 사용자 r002 원본 확인 완료, Unity 적용/정식 검증은 별도 대기.

- PLAN-041 최신 인계(2026-11-17): WorldDestruction r002 생체 군락 확산/점령, Colonization32셀·8×4·1024×512,125ms×32=4초. min(31,floor(clamp01(Progress)*32)), Results32 고정. r001 균열/소멸 계약 대체, 파일명/요청 ID 유지. PanelFrame r001 재사용. 기존 캡션/Localization 의미와 점령 표현의 정합성·슬라이싱/시간·Results/새 판·가로세로/오류를 마지막 적용/검증에서 확인한다. 이 인계 당시 원본 확인 대기였으나, 같은 날 후속 사용자 승인으로 확인 완료·PLAN-042 active 전환.

- 2026-11-17: proposed → active. 사용자 PLAN-041 r002 원본 확인 및 전체 아트 일괄 적용/검증 요청. 전체 제출 리비전을 고정하고 기술 검사·모든 연결을 완료한 뒤 일반 Editor 통합 검증을 수행한다. Player/콘텐츠 빌드·실기기·고부하는 기본 범위에서 제외한다.

## 2026-11-17 전체 연결 준비

원본 확인 전부 완료 및 21개 PNG 기술 대조·수령·표시 연결 완료. [통합 기록](evidence/PLAN-042/integration.md)·[정적 참조](evidence/PLAN-042/static-bindings.json). 일반 Editor 통합 검증 완료(최종 Edit464/Play100 통과), 게임 화면 사용자 최종 확인 완료.

- 2026-11-17: 일괄 적용·일반 Editor 검증·환경 복원 완료. 실제 적용 화면 최종 사용자 확인을 남겨 active 유지. [결과](evidence/PLAN-042/README.md).

## 최종 완료 — 2026-11-17

completed. 2026-11-17 사용자: “좋아. 적용된 게임 화면도 확인했어. PLAN-042와 관련 Unity·Art PLAN을 모두 완료 처리해줘. 그리고 main브랜치에 푸쉬해줘”. 승인 리비전의 Unity 적용·일반 Editor 기술 검증·최종 게임 화면 확인·환경 복원을 모두 마쳤다. 추가 제작/수정 요청은 없으며 다음 PLAN을 활성화하지 않는다.

[완료 승인 기록](evidence/PLAN-042/completion.md) · [통합 증거](evidence/PLAN-042/README.md) · [검증 대응](evidence/PLAN-042/plan-verification-matrix.csv). Android/실기기·고부하 검증은 이번 완료 범위 밖이며 미실행이다.

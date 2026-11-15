---
id: PLAN-042
type: art
status: proposed
title: 정식 아트 전체 통합 검수·잔여 임시 표현 정리
updated: 2026-11-16
---

# PLAN-042 정식 아트 전체 통합 검수·잔여 임시 표현 정리

[사용 양식](_template-art-ui.md) · [PLAN 관리](AGETNS.md#art-template-records) · [전체 준비 후 테스트](../agents/verification.md#art-plan-test-readiness) · [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md)
## 목표와 범위

각 묶음의 정식 아트를 하나의 게임 흐름으로 비교하고 잔여 임시 참조·스타일 불일치·합성 문제를 정리한다. 새 대규모 제작이 아닌 전체 아트 적용 완결을 판정하는 `type: art` 작업이다.
완료 PLAN 본문·과거 증거는 수정하지 않는다. 발견한 결함/수정 리비전은 이 PLAN과 해당 요청 README에 남기며 원본 변경은 사용자 제작/위임 범위에서 별도 리비전으로 받는다.

## 기준 문서와 선행 조건

- [아트 로드맵](../game-design/05-development/02-implementation/art-roadmap.md), [검증](../agents/verification.md), [아트 요청](../agents/art-assets.md).
- 선행 필수: PLAN-033~041의 전체 필수 리소스 적용·기술 검증·사용자 시각 승인. PLAN-026/029·PLAN-030~032 결과도 보존한다.
- [기존 승인본](../../UnityProject/Assets/Sprites/Require/PLAN-026/README.md)·[시설 승인본](../../UnityProject/Assets/Sprites/Require/PLAN-029/README.md)·[UI 요청](../../UnityProject/Assets/Sprites/Require/PLAN-014/README.md)을 리비전 원본으로 사용한다.
- UI 양식에 정적/애니메이션의 공간·재생 검수를 합친다. 신규 제작물 기본 0개이며 실제 결함이 있을 때만 수정본을 추가한다.

## 디자인 의도와 원하는 모습

연구소에서는 바닥/외곽보다 시설·생명체·실제 상태가, UI에서는 장식보다 수치·선택·행동이 먼저 읽혀야 한다. 진입·운영·멸망·결과의 픽셀 밀도·재질·강조가 일관되게 이어져야 한다.
승인 외형을 통일성 때문에 일괄 재색칠하지 않는다. 의도된 화면 차이는 각 디자인 설명/참고 버전과 대조하고 실제 문제가 있는 사례만 수정 대상으로 제시한다. 기준은 기존 승인본과 PLAN-033~041 최종 화면이며 임시 이미지/예시 색/미사용 상태 아이콘은 기준에서 제외한다.

## 산출물과 적용 목록

| 대상 | 결과 |
| --- | --- |
| 에셋 대응표 | Art PLAN/asset_id·원본/출력 리비전·Unity 요청 ID·최종 경로·사용처·승인 |
| 실제 화면 | 타이틀/로딩/연구소/팝업/멸망/결과의 가로·세로와 사용 리비전 |
| 잔여 임시 표현 | 실제 참조된 Temp·대체 도형·누락 Icon의 유지 사유/교체 결과 |
| 수정본 | 문제·영향·리비전·기술/시각 재확인 |
| 환경/한계 | 저장/설정·Editor 복원, Android/고부하 미검증 또는 승인 후 실제 결과 |

각 선행 PLAN의 리소스 표를 모아 파일/참조 확인 일시를 남긴다. 수령 폴더에 PNG가 없다는 이유로 유실 판정하지 않고 정규 경로/Art 보관본을 대조한다. 완료 Art PLAN의 오래된 링크 정리는 제외한다.

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

이 PLAN은 신규 원본/PNG를 제작·수령하는 묶음이 아니다. 아래 선행 PLAN의 「결과물 저장 위치」에 지정한 Art 원본·리비전·Unity 수령/최종 적용 경로를 그대로 사용한다. PLAN-042 명의의 아트 복제본·Require 폴더는 만들지 않는다.

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

검수 루트 E는 `C:/Dev/Unity/Doom_Inc/docs/plans/evidence/PLAN-042/`다. 다음은 예정 경로이며 지금 파일을 생성하지 않는다.

| 결과물 | E 기준 저장 경로 |
| --- | --- |
| 전체 통합 검수 색인 | `README.md` |
| 전체 에셋의 Art/Unity 경로·요청·리비전·승인 대응 | `asset-manifest.csv` |
| 실제 잔여 Temp·대체 도형·유지/정리 판정 | `temporary-art-audit.md` |
| 수정 필요 항목·수정 리비전·재검수 결과 | `issues.md` |
| 기술 검증과 전체 사용자 시각 승인 | `validation.md` |
| 원본/참조·저장/설정·Editor 보존/복원 | `environment.md` |
| 첫 통합 실행의 조건/대상 리비전/시나리오 | `runs/run-001/README.md` |
| 실행한 검증 결과/로그 | `runs/run-001/tests/editmode.xml`, `runs/run-001/tests/playmode.xml`, `runs/run-001/logs/editor.log` |
| 필요한 실제 재생/전체 흐름 영상 | `runs/run-001/videos/game-flow.mp4` |

가로/세로 대표 화면은 각 파일로 구분한다.

| 화면 | E 기준 가로 | E 기준 세로 |
| --- | --- | --- |
| title | `runs/run-001/screenshots/title-landscape.png` | `runs/run-001/screenshots/title-portrait.png` |
| loading | `runs/run-001/screenshots/loading-landscape.png` | `runs/run-001/screenshots/loading-portrait.png` |
| laboratory | `runs/run-001/screenshots/laboratory-landscape.png` | `runs/run-001/screenshots/laboratory-portrait.png` |
| popup | `runs/run-001/screenshots/popup-landscape.png` | `runs/run-001/screenshots/popup-portrait.png` |
| destruction | `runs/run-001/screenshots/destruction-landscape.png` | `runs/run-001/screenshots/destruction-portrait.png` |
| results | `runs/run-001/screenshots/results-landscape.png` | `runs/run-001/screenshots/results-portrait.png` |

재검수는 run-002부터 증가하고, 다른 언어/배율 캡처는 해당 실행 README에 이름과 조건을 기록한다. 미실행 테스트의 결과 파일은 만들지 않는다. 원본 시각 증거는 선행 PLAN의 source-reviews 또는 실제 Art 검수 경로를 연결한다. 빌드/실기기·고부하가 별도로 승인되면 실행 전 그 산출물 경로를 이 표에 추가한다.

## 작업 책임과 문서 경계

- 현재 범위는 2026-11-16 사용자가 요청한 남은 아트 PLAN 작성이다. 제작·Unity 적용·게임 테스트는 미승인/미실행이다.
- 제작 작업공간은 `C:/Dev/Unity/Art`다. 정식 아트는 사용자가 Aseprite로 제작하고 에이전트는 명세·수령·연결·검증을 담당한다. 그림 제작을 별도로 위임받으면 그 근거를 기록한다.
- [Art 디자인 MAP](../../../Art/docs/agetns/design/README.md)·[광원](../../../Art/docs/agetns/design/lighting.md)·[팔레트](../../../Art/docs/agetns/design/palette.md)·[가독성](../../../Art/docs/agetns/design/readability.md)·[출력/전달](../../../Art/docs/agetns/production/export-and-handoff.md)을 적용한다. 공통 미정값은 개별 승인값을 취소하지 않는다.
- Art 제작 PLAN은 에셋별로 관리하며 Unity PLAN 번호와 독립이다. 기존 asset_id/리비전은 재사용한다. 이번에는 Art PLAN을 중복 생성하거나 완료 문서를 수정하지 않는다.
- 신규 제작 요청은 없다. 수정본이 필요하면 선행 PLAN의 원래 요청 ID에서 리비전으로 추적한다.

## 리소스 준비와 진행 요약

| 필수 대상 | 명세/제작 | 최신 수령 | 원본 승인 | 실제 적용 | 기술 검증 | 게임 화면 승인 |
| --- | --- | --- | --- | --- | --- | --- |
| 선행 PLAN-033~041의 전체 필수 대상 | 선행 완료 대기 | 리비전 미집계 | 선행 확인 전 | 목록 미확정 | 통합 미실행 | 전체 미확인 |

- 전체 준비 판정: 미충족. 전체 필수 리소스의 제작·수령·적용·참조 연결을 정적으로 확인한 날짜와 근거를 기록한 뒤 테스트를 시작한다.
- 준비 중에는 파일·임포트·참조 확인만 수행한다. Edit/Play·수동 게임 실행·검수 캡처는 전체 준비 후 진행한다. 원본 승인과 게임 화면 승인을 구분한다.
- 임포트는 [아트 지침](../agents/art-assets.md)의 PPU 32·Point·원본 해상도 유지 기준을 따른다. 좌표·프레임 시간은 PNG와 별도로 기록한다.

## 작업 목록

- [x] 디자인 의도·범위·현행 연결·선행 관계와 검수 기준을 제안으로 작성
- [ ] 실행 승인/선행 조건 확인, 해당 PLAN만 active로 전환
- [x] 재사용 아트의 경로 원본·통합 검수 자료 위치 지정
- [ ] 전체 리비전/참조 목록 확정, 선행 요청/Art 검수 기록 연결
- [ ] 전체 필수 리소스 제작·수령·원본 승인·리비전 대조
- [ ] 정규 경로·씬/SO·생성 도구의 실제 참조 전체 연결
- [ ] 전체 준비 판정 후 기술 검증·사용자 게임 화면 확인
- [ ] 수정 리비전 반영·환경 복원·채택 규격·로드맵 완료 범위 기록

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

- 2026-11-16: 문서·현행 코드/정의의 정적 조사로 후보 작성. 신규 리소스 제작·수령·게임 적용·게임 테스트는 미실행.
- 원본 승인·게임 화면 승인: 모두 대기. 실행 시 대상 리비전·화면/배율·사용자 응답/날짜·수정/재확인을 각각 기록한다.
- 문서 검사: 2026-11-16 문서 검사 통과(PLAN 42개·active 0개·목록 42개·로컬 링크 3,888개). 변경 형식 검사 통과. 기존 Unity/Art PLAN 40개가 작업 전 해시와 같음을 확인했다.
- 경로 보완 검사(2026-11-16): 문서 검사 통과(42 PLAN·active 0·로컬 링크 3,904개), 25개 제작 후보의 원본/제출/수령/최종 적용/원본 비교 경로 누락·중복 없음, 기존 완료 PLAN 40개 해시 불변 확인. 예정 결과물은 생성하지 않았다.
- 환경 복원: 문서만 작성하여 게임·Editor·원본 아트·저장/설정을 변경하지 않았다.

## 현재 상황과 다음 행동

- status: proposed. active PLAN은 0개이며 이 문서는 실행 승인이 아니다.
- 다음 행동: PLAN-033~041 완료 후 최종 리비전 목록을 고정하고 일반 Editor 통합 검수를 진행한다. 지금은 검수하지 않는다.
- 로드맵: 5-7 전체 통합 검수. 이 PLAN 완료가 다른 묶음/전체 아트의 완료를 대신하지 않는다.
- 최종 적용/승인 리비전·채택 규격·기준 화면은 실행 후 기록한다.

## 변경 이력과 상태 전환

| 날짜 | 변경 | 근거·영향 |
| --- | --- | --- |
| 2026-11-16 | 결과물 경로 보완 | 사용자 요청에 따라 재사용 원본 경로·통합 검수 파일 위치 지정. 검수는 미실행 |
| 2026-11-16 | 신규 proposed | 사용자: 남은 ART 관련 PLAN을 비슷한 카테고리별로 모두 작성. 제작/적용 미승인 |

# PLAN-014 Sprite 요청

- 요청 출처: [PLAN-014 모바일 UI·UX와 시설 상태 표시 정비](../../../../../docs/plans/PLAN-014-mobile-ui-ux-and-facility-status.md).
- 기준: [아트 요청·임포트 지침](../../../../../docs/agents/art-assets.md), [임의 에셋 규칙](../../../../../docs/agents/project-structure.md#temporary-assets).
- 정식 아트는 사용자가 직접 제작한다. **이번 요청의 정식 교체는 사용자가 지정한 별도 `type: art` PLAN의 승인·실행 범위에서 진행한다.** 파일 수령만으로 PLAN-014에서 교체하지 않는다.
- Build(PLAN-014-SPR-006)는 r001 수령·Art 승인·Unity 적용·게임 화면 사용자 승인 완료이며 나머지 15종은 `요청`이다. Art 승인을 Unity 적용·게임 화면 검수 완료로 처리하지 않는다.

## 공통 제작·교체 명세

- 구분: 아래 16종 모두 임의(Temp) Sprite 교체. 임시 이미지는 기능 식별을 위한 단색 도형이며 최종 색감·스타일 기준이 아니다.
- 이미지 크기: 항목별 64×64 PNG, 투명 배경, Sprite Mode Single. 정식 아트의 제작 크기를 변경하려면 별도 art PLAN에서 실제 UI 표시·터치 영역과 함께 확인한다.
- 프레임: 1장, 애니메이션 없음, 중심 피벗 `(0.5, 0.5)`, Full Rect. 여러 아이콘을 한 파일에 합치지 않는다.
- PPU와 임포트 공통값은 위 아트 지침을 따른다. Sprite 크기와 버튼의 실제 터치 영역은 별개다.
- 사용처: `LaboratoryView`의 직렬화된 `LaboratoryUiAssets.icons` 참조. 아래 `LaboratoryIcon` 값은 표현 참조 키이며 파일명·모양·픽셀 크기로 게임 명령을 판정하지 않는다.
- 임시 경로: `UnityProject/Assets/Sprites/UI/Laboratory/Temp_<키>.png`.
- 최종 경로: `UnityProject/Assets/Sprites/UI/Laboratory/<요청 파일명>`.
- 생성 도구: `UnityProject/Assets/Scripts/Editor/Presentation/LaboratoryUiAssetBuilder.cs`. `CreateMissing()`는 누락된 임시 PNG만 만들며 기존 제공본을 덮어쓰지 않는다.
- 참조 교체: 최종 Sprite를 해당 `LaboratoryIcon` 슬롯에 연결한다. 씬 재생성 도구가 최종 연결을 다시 임시 참조로 바꾸지 않도록 생성 도구의 바인딩도 함께 정비한다.

## 항목별 요청

| 요청 ID | 키·교체할 Temp_ 파일 | 요청 파일명 | 용도·표시 위치와 필요한 모습 | 사용처·교체 참조 | 상태 |
| --- | --- | --- | --- | --- | --- |
| PLAN-014-SPR-001 | `ZoomOut` / `Temp_ZoomOut.png` | `ZoomOut.png` | 맵 조작 버튼. 돋보기 안의 빼기 표식으로 축소를 구분 | `icons[1]` | 요청 |
| PLAN-014-SPR-002 | `ZoomIn` / `Temp_ZoomIn.png` | `ZoomIn.png` | 맵 조작 버튼. 돋보기 안의 더하기 표식으로 확대를 구분 | `icons[2]` | 요청 |
| PLAN-014-SPR-003 | `FitMap` / `Temp_FitMap.png` | `FitMap.png` | 맵 조작 버튼. 작은 맵과 네 모서리 프레임으로 전체 보기 의미 전달 | `icons[3]` | 요청 |
| PLAN-014-SPR-004 | `ShowStatus` / `Temp_ShowStatus.png` | `ShowStatus.png` | 상태 표시 ON 토글. 열린 눈 형태, 상태 문구와 함께 사용 | `icons[4]` | 요청 |
| PLAN-014-SPR-005 | `HideStatus` / `Temp_HideStatus.png` | `HideStatus.png` | 상태 표시 OFF 토글. 눈에 취소선으로 ON과 구분 | `icons[5]` | 요청 |
| PLAN-014-SPR-006 | `Build` / `Temp_Build.png` | `Build.png` | 하단 건설 메뉴. 건물 외형으로 건설 목록 진입 의미 전달 | `icons[6]` | r001 적용·게임 화면 승인 완료 |
| PLAN-014-SPR-007 | `Creatures` / `Temp_Creatures.png` | `Creatures.png` | 하단 생명체 메뉴. 여러 개체 목록임을 나타내는 실루엣 | `icons[7]` | 요청 |
| PLAN-014-SPR-008 | `Invasion` / `Temp_Invasion.png` | `Invasion.png` | 하단 침공·보고 메뉴. 외부로 향하는 화살표, 열기 자체는 방출 명령이 아님 | `icons[8]` | 요청 |
| PLAN-014-SPR-009 | `More` / `Temp_More.png` | `More.png` | 게임 메뉴/설정 진입. 점 세 개 또는 구분 가능한 메뉴 형태 | `icons[9]` | 요청 |
| PLAN-014-SPR-010 | `Worker` / `Temp_Worker.png` | `Worker.png` | 시설 상태 표식. 실제 개체 작업 중이며 단순 배정 상태와 구분 | `icons[10]` | 요청 |
| PLAN-014-SPR-011 | `Automatic` / `Temp_Automatic.png` | `Automatic.png` | 시설 상태 표식. 자동 생산 순환 표식. 회차 진행률과 자동 속도는 문구로 구분 | `icons[11]` | 요청 |
| PLAN-014-SPR-012 | `Material` / `Temp_Material.png` | `Material.png` | 시설 재료 대기 표식. 재료 상자, 필요량/도착량은 상세 문구로 제공 | `icons[12]` | 요청 |
| PLAN-014-SPR-013 | `Delivery` / `Temp_Delivery.png` | `Delivery.png` | 완료품 실제 입고 대기 표식. 보관함으로 들어가는 화살표 | `icons[13]` | 요청 |
| PLAN-014-SPR-014 | `Blocked` / `Temp_Blocked.png` | `Blocked.png` | 작업 위치 접근 불가 표식. 금지/차단 형태로 색 이외 단서 제공 | `icons[14]` | 요청 |
| PLAN-014-SPR-015 | `SelectTarget` / `Temp_SelectTarget.png` | `SelectTarget.png` | 배양 대상 미선택 표식. 대상 선택 필요 의미의 조준/선택 프레임 | `icons[15]` | 요청 |
| PLAN-014-SPR-016 | `Construction` / `Temp_Construction.png` | `Construction.png` | 건설 중 시설 표식. 망치 등 시공 도구로 생산 진행과 구분 | `icons[16]` | 요청 |

## PLAN-014-SPR-006 — 건설 메뉴 아이콘 제작 연결

- 요청·파일명·사용처는 위 Build 항목 그대로 유지한다. r001 제작·수령·Art 승인·Unity 적용을 완료했고 2026-11-10 최종 게임 화면 사용자 승인까지 완료했다. 나머지 15종의 상태는 변경하지 않는다.
- 적용 담당은 완료된 [Unity PLAN-026](../../../../../docs/plans/PLAN-026-art-style-scale-and-sample-integration.md)의 5-1 건설 메뉴 샘플이다. 출처인 완료된 Unity PLAN-014를 다시 활성화하지 않는다.
- 제작 계획: [Art PLAN-005](../../../../../../Art/docs/plans/PLAN-005-build-icon.md), `asset_id: Build`, completed·r001 승인·approved_revision 1. 2026-11-07 사용자 요청으로 제작·검사·전달했고 2026-11-10 사용자가 r001 승인과 PLAN-005 완료를 명시했다.
- 제작 명세: `Build.aseprite` 1개·64×64 RGB/sRGB·정적 1프레임, `Build.png` 64×64 RGBA 투명 1개. Single·중심 피벗·Full Rect·공통 PPU 32 유지. 애니메이션·방향별 변형 없음.
- r001 디자인: 지붕·본체·중앙 입구·두 창이 읽히는 연구소 건물 실루엣, 회청색 몸체·청록색 유리. 불투명 8색·외곽 주로 2px, bounds (8,8,48,47)·최소 8px 여백. 건설 목록 진입용이며 시공 도구 중심 Construction 아이콘·건설 확정 표식과 구별한다.
- 현재 코드의 아이콘 영역은 22×22 UI 논리 단위, 흰색 틴트·비율 유지·입력 비차단이다. 64px 원본과 작은 표시에서의 형태를 비교한다. 실제 화면 픽셀 수는 Canvas 배율에 따르며 버튼 터치 영역과 구분한다.
- [r001 수령 PNG](../../UI/Laboratory/Build.png), [Art 제출 명세·비교·검증](../../../../../../Art/assets/ui/Build/revisions/r001/README.md). 최종 배치: `Assets/Sprites/UI/Laboratory/Build.png`. 새 PLAN-026 요청 ID는 발행하지 않는다.
- 파일 검사: 작업/제출 원본과 PNG 각 4,096픽셀 차이 0, 크기·알파 0/255·여백·단일 프레임·Guide 제외 통과. Art/Unity PNG SHA-256 `EB2E9FC97F3918B95D62B9E7389CEAE016235F1A4D0DCAC68C7A330B4A50639D` 일치.
- Art의 64/32/22/16px·배경 대비 검토와 r001 사용자 시각 승인을 마쳤다. 2026-11-10 정식 Build를 씬/AppConfiguration의 `icons[6]`에 연결하고 생성 도구도 정식 참조를 유지하도록 수정했다. 이후 사용자 미사용 Sprite 정리 요청으로 참조가 없는 `Temp_Build.png`와 메타데이터를 제거했다. ko/en·큰 글씨·가로/세로·버튼 상태는 아래 통합 검증 기록을 따른다.

## 수령·적용·검증 기록

- 2026-10-29: 요청 명세와 임시 이미지 생성 코드를 준비했다. 정식 파일은 미수령이며 적용·사용자 확인은 미실행이다.
- 2026-10-29: Unity 메뉴로 임시 PNG 16종을 생성·임포트하고 연구소 씬의 직렬화된 참조를 연결했다. PPU 32·Point·Clamp·Single·밉맵 OFF·비압축을 [임포트 확인](../../../../../docs/plans/evidence/PLAN-014/icon-imports.json)에서 확인했다. 가로·세로/밀집 ON·OFF와 한글 글리프·실제 아이콘 참조는 최신 Play 34/34와 캡처로 확인했다. SM-G977N에서 작은 아이콘 조작·표식 재탭·게임 흐름을 확인했고 사용자도 수정 배치와 조작, 보고·결과 화면이 정상이라고 답했다. 이는 임시 표현의 기능·UX 확인이며 정식 아트 검수는 아니다. 상세 결과는 [실행 기록](../../../../../docs/plans/evidence/PLAN-014/README.md)에서 관리한다.
- 한국어 글꼴은 별도 공개 라이선스 기능 에셋이며 [출처·라이선스·정적 글리프 확인](../../../Fonts/NanumGothic/README.md)에 기록했다. 이 Sprite 제작 요청에 포함하지 않는다.
- 2026-11-07: 사용자 위임으로 Art PLAN-005에서 Build r001을 제작·검증해 PLAN-014-SPR-006 경로에 수령했다. 최신 수령 r001·실제 적용 없음·사용자 시각 승인 대기. Unity 적용 담당은 PLAN-026이며 출처 PLAN-014의 completed는 유지한다.
- 2026-11-10: 사용자가 Build r001 승인·Art PLAN-005 완료를 명시했다. 최신 수령·Art 승인 r001, 실제 적용 없음. 제출 파일 해시를 다시 대조했고 Unity 적용·게임 화면 검수는 다른 대화에서 진행한다.
- 남은 정식 작업: 나머지 15종 수령·별도 승인 범위에서 규격/참조/임포트 검증·교체·사용자 확인. Build는 완료했다. Art 승인과 게임 적용 검수를 구분한다.

## 2026-11-10 Unity 적용 상태

- 사용자 요청에 따라 최신 Art PLAN 승인본을 사용했다. Art 승인과 Unity 게임 화면 사용자 시각 확인은 별도다.
- 바닥 r003의 기존 적용을 보존하고 Gathering r003·PrototypeSupport r002·OrganicMatter r001·Build r001의 정규 경로 이동·임포트·참조 연결을 완료했다. 이전 수령 경로의 PNG는 GUID를 보존해 이동했으며 위 파일 링크는 정규 경로를 가리킨다. 이전 리비전과 Aseprite 원본은 유지한다.
- 전체 5 PNG·106 Sprite, PNG 해시/GUID 5개·바닥 Sprite ID 보존을 정적으로 확인했다. 이 시점까지 게임 테스트는 실행하지 않았으며 이제 전체 적용 후 검증을 진행한다.
- [통합 적용 증거](../../../../../docs/plans/evidence/PLAN-026/integration-2026-11-10/README.md)에 실제 경로·임포트·참조·검증 결과와 사용자 게임 화면 확인 상태를 기록한다. 이전 대기 기록은 해당 날짜의 이력이며 최신 적용 상태는 이 절을 따른다.

## 2026-11-10 Build 적용·게임 화면 승인 완료

- Build r001을 정규 경로에 적용하고 씬/AppConfiguration icons[6]와 생성 도구의 정식 참조 유지를 검증했다. ko/en·큰 글씨·가로/세로·버튼/입력 회귀를 통과했다.
- 사용자가 별도 Unity 게임 화면 검수에 “현재 게임 화면 승인”으로 답했다. 최신 수령·적용·검수 r001, 요청 상태 완료다. 나머지 15종은 요청 상태를 유지한다.
- [PLAN-026 통합 완료 증거](../../../../../docs/plans/evidence/PLAN-026/integration-2026-11-10/README.md). 출처 PLAN-014의 과거 완료 상태는 변경하지 않는다.

## 2026-11-10 미사용 Sprite 정리

- 사용자 요청으로 교체 완료된 Temp_Build와 이전 리비전 수령 PNG 11장, 해당 메타데이터·빈 폴더를 제거했다. 이전 리비전의 Art 보관본과 Aseprite 원본, 요청·승인 이력은 유지한다.
- 삭제 대상 참조 0, 보관본 11장 해시 일치, 남은 22 PNG의 해시/GUID 불변과 정식 아트 Edit Mode 6/6 통과를 확인했다. [삭제 목록·검증 증거](../../../../../docs/plans/evidence/PLAN-026/cleanup-2026-11-10/README.md)를 참조한다.
- 요청 완료 상태와 PLAN 상태는 변경하지 않는다. 새 Sprite 요청은 없다.

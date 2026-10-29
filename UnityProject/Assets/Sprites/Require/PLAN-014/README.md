# PLAN-014 Sprite 요청

- 요청 출처: [PLAN-014 모바일 UI·UX와 시설 상태 표시 정비](../../../../../docs/plans/PLAN-014-mobile-ui-ux-and-facility-status.md).
- 기준: [아트 요청·임포트 지침](../../../../../docs/agents/art-assets.md), [임의 에셋 규칙](../../../../../docs/agents/project-structure.md#temporary-assets).
- 정식 아트는 사용자가 직접 제작한다. **이번 요청의 정식 교체는 사용자가 지정한 별도 `type: art` PLAN의 승인·실행 범위에서 진행한다.** 파일 수령만으로 PLAN-014에서 교체하지 않는다.
- 요청 상태는 아래 모든 항목이 `요청`이다. 아직 정식 제공본을 수령·적용하거나 사용자 시각 확인을 받지 않았다.

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
| PLAN-014-SPR-006 | `Build` / `Temp_Build.png` | `Build.png` | 하단 건설 메뉴. 건물 외형으로 건설 목록 진입 의미 전달 | `icons[6]` | 요청 |
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

## 수령·적용·검증 기록

- 2026-10-29: 요청 명세와 임시 이미지 생성 코드를 준비했다. 정식 파일은 미수령이며 적용·사용자 확인은 미실행이다.
- 2026-10-29: Unity 메뉴로 임시 PNG 16종을 생성·임포트하고 연구소 씬의 직렬화된 참조를 연결했다. PPU 32·Point·Clamp·Single·밉맵 OFF·비압축을 [임포트 확인](../../../../../docs/plans/evidence/PLAN-014/icon-imports.json)에서 확인했다. 가로·세로/밀집 ON·OFF와 한글 글리프·실제 아이콘 참조는 최신 Play 34/34와 캡처로 확인했다. SM-G977N에서 작은 아이콘 조작·표식 재탭·게임 흐름을 확인했고 사용자도 수정 배치와 조작, 보고·결과 화면이 정상이라고 답했다. 이는 임시 표현의 기능·UX 확인이며 정식 아트 검수는 아니다. 상세 결과는 [실행 기록](../../../../../docs/plans/evidence/PLAN-014/README.md)에서 관리한다.
- 한국어 글꼴은 별도 공개 라이선스 기능 에셋이며 [출처·라이선스·정적 글리프 확인](../../../Fonts/NanumGothic/README.md)에 기록했다. 이 Sprite 제작 요청에 포함하지 않는다.
- 남은 정식 작업: 16종 수령 → 별도 art PLAN에서 규격/참조/임포트 검증·교체 → 사용자 시각 확인. 기능 검증과 정식 시각 검수 결과를 혼합하지 않는다.

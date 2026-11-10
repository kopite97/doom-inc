# TECH-017 연구소 월드 표시와 uGUI 투영

- 날짜: 2026-11-10
- 상태: 채택·구현·Editor 검증 완료
- 실행 계획: [PLAN-027](../plans/PLAN-027-laboratory-tilemap-and-sprite-rendering.md)

## 결정

기존 URP 17.3 Renderer2D를 유지한다. LaboratoryMapView가 일반 C# 객체 LaboratoryWorldMap을 소유하고, 이 객체가 연구소 수명의 월드 루트·직교 카메라·Grid/Tilemap·공용 임시 단색 Sprite·4개 런타임 Tile을 생성하고 해제한다. Thing의 표시 객체는 Guid로 관리한다. 여러 Sprite를 함께 그릴 때만 SortingGroup을 활성화하며 몸체만 보이는 경우에는 직접 배치한다. 게임 모델·저장에는 표시 필드를 추가하지 않는다.

- 논리 칸 좌하단 `(x,y)`는 월드 `(2x,2y)`, 칸 중심은 `(2x+1,2y+1)`이다. Grid cell size `(2,2,1)`, Tile anchor `(0.5,0.5,0)`와 기존 중앙 피벗·PPU 32를 사용한다.
- 카메라의 화면 사각형은 최종 MapViewport 영역이다. 정수 픽셀 경계 안쪽으로 출력 영역을 맞추고 그 차이만큼 카메라 중심/크기를 보정한다. 기존 중심·CellSize·연속 zoom 1~8로 orthographicSize를 계산한다. 바닥·시설·입력·상태 UI는 같은 변환을 사용한다.
- 전용 LaboratoryWorld 레이어를 월드 카메라만 렌더한다. 지도 표시 중에는 기존 배경 카메라를 비활성화하고 월드 카메라만 렌더한다. 지도 바깥은 Canvas의 4개 불투명 배경 사각형으로 채워 이전 viewport 잔상을 막는다. 지도 비활성·크기 0·화면 종료 시 기존 카메라 상태를 복원하며 기존 AudioListener를 유지한다. 추가 AudioListener·RenderTexture·Collider는 만들지 않는다.
- 불투명 PlayScreen/MapViewport 배경은 카메라를 가리지 않도록 조정한다. MapViewport의 raycast와 RectMask2D는 유지한다. UI는 Overlay로 월드 위에 표시한다.
- 바닥은 sortingOrder -30000, 시설/개체는 발 위치의 y 내림차순·CreationOrder 오름차순으로 4씩 간격을 둔 안정적 순서, 미리보기는 30000이다. 몸체·CarryAnchor는 형제이며 Cargo는 Anchor 아래에서 따로 회전/크기를 유지한다.
- 정식 아트가 없는 종류는 기존 색/시설 역할·생성 순서 의미를 단색 Sprite와 uGUI 주석으로 보존한다. 칸별 Image/TMP 배열은 제거한다. 자원 정의 Icon이 없으면 단색 대체를 표시한다.
- 단색 Sprite와 Tile은 씬 소유의 작은 런타임 객체다. 원본 PNG 복제·시설별 Material 복제·정적 가변 캐시는 만들지 않는다. Unlit 공용 Material은 에셋 참조로 연결해 플레이어 셰이더 참조를 보존한다.
- LateUpdate에서 UI 레이아웃 이후 월드 카메라/표시를 갱신하고 상태 표식을 투영한다. 바닥은 크기/참조가 변할 때만 갱신한다. 개체 목록/제거/겹침 수 버퍼는 재사용하고 칸마다 전체 배치를 검색하지 않는다.

## 대안과 제한

Pixel Perfect Camera는 유보한다. 현재 연속 줌·가변 지도 영역을 보존하며 정수 배율 고정이나 새 레터박스를 도입하지 않는다. Point 필터를 유지하지만 비정수 화면 확대와 팬에서 원본 픽셀의 화면 폭이 균등하다고 보장하지 않는다. 이동 보간은 별도 PLAN-028 범위다.

RenderTexture 표시와 UI 전체 개편은 이 전환에 필요하지 않다. 기존 Renderer2D의 Base Camera viewport 기능을 사용한다. 최초 두 카메라 안에서 RenderLoop 중앙값이 약 0.58→0.71ms로 늘어난 것을 확인해, 배경을 uGUI로 채우는 단일 활성 카메라 방식으로 수정했다. [Unity의 다중 카메라 viewport 설명](https://docs.unity3d.com/6000.3/Documentation/Manual/urp/rendering-to-the-same-render-target.html), [Pixel Perfect Camera 소개](https://docs.unity3d.com/Packages/com.unity.2d.pixel-perfect@5.0/manual/index.html)를 참고했다.

## 검증

실제 결과·예외·환경 복원은 [PLAN-027](../plans/PLAN-027-laboratory-tilemap-and-sprite-rendering.md)에 기록한다. Edit 363/363, 최종 연구소 Play 42/42와 앱 씬 Play 29/29를 통과했다. 성능 반복 범위·배치 증가와 기기 미검증은 완료 증거에 명시한다.

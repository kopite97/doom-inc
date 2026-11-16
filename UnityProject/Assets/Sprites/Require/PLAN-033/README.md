# PLAN-033 Sprite 요청

출처·적용: [Unity PLAN-033](../../../../../docs/plans/PLAN-033-support-creature-art.md) · [아트 지침](../../../../../docs/agents/art-assets.md) · [Art 제작 PLAN-009](../../../../../../Art/docs/plans/PLAN-009-support-creature.md).

## PLAN-033-SPR-001 — 보급형 생명체

- 구분: 임시 표현 교체. asset_id `Support`, 요청 파일 `Support.png`, 최초 리비전 r001.
- 용도: `creature.support`의 지도 본체와 선택 강조, 건설·수집·운반·배정 생산, 목록 대표 프레임. 기존 프로토타입 본체/애니메이션은 보존한다.
- 확정 디자인: 귀엽고 부드러운 젤리형 보조형, 검정 또는 흰색 한 가지 색의 점눈 두 개, 위에서 아래로 비추는 천장광, Down/Left/Right/Up, 머리 위 별도 자원 합성. 정면 두 눈, 측면 먼 눈 생략 가능, 뒷면 눈 없음. 몸체를 색만 바꾼 프로토타입 복제로 만들지 않는다.
- 디자인 제안: 프로토타입보다 안정적으로 넓고 낮은 몸체, 위쪽 밝은 면·아래쪽 음영, 큰 장비 없이 눌림으로 작업. 작은 어깨 돌출 제안은 사용자 F-001에 따라 제거하고 몸통 외곽이 이어지도록 수정했다. 정확한 색·실루엣·그림 영역은 제작과 사용자 시각 확인으로 정한다.
- r002 적용 규격: 64×64px 캔버스, 네 방향 각각 Idle 4·Move 6·Work 6, 총 64프레임/12태그, 순서는 Down/Left/Right/Up 및 방향별 Idle→Move→Work. 8×8셀 512×512 PNG 1장, 좌상단부터 행 우선, 간격/외곽 여백 0, 자동 자르기·회전·병합 없음. 태그는 `Idle_Down`, Sprite 이름은 `Support_Idle_Down_01` 형식이다.
- 적용 시간: Idle 250ms×4, Move 100ms×6, Work 120ms×6, 모두 반복. 원본과 일치한다. 중앙 피벗·접지 (32,54), 머리 좌표는 Art r002 프레임 메타데이터와 대응한다. 원본 외형 승인은 2026-11-16이며 실제 게임 화면·동작의 사용자 최종 검수는 별도다.
- 운반물은 몸체에 그리지 않고 별도 유기물 Sprite 사용. 각 방향·프레임의 바닥 접점과 머리 받침 좌표를 좌상단 원점, 오른쪽 +X/아래 +Y, 픽셀 단위로 제출한다. 기준점과 실제 그림 범위·여백·피벗도 기록한다.
- Art 작업 원본 예정: `C:/Dev/Unity/Art/assets/characters/Support/source/Support.aseprite`. r001 원본·출력·명세 예정: `C:/Dev/Unity/Art/assets/characters/Support/revisions/r001/source/Support.aseprite`, `export/Support.png`, `README.md` (같은 r001 폴더 기준).
- Unity 수령 예정: `UnityProject/Assets/Sprites/Require/PLAN-033/PLAN-033-SPR-001/r001/Support.png`. 최종 적용 예정: `UnityProject/Assets/Sprites/Creature/Support/Support.png`. Sprite 설정은 공통 아트 지침을 따른다.
- 원본 비교 예정: `docs/plans/evidence/PLAN-033/source-reviews/Support/r001/design.png`와 같은 폴더의 `README.md`. 실제 게임·기술 검증 예정: `docs/plans/evidence/PLAN-033/runs/run-001/` 및 색인 `docs/plans/evidence/PLAN-033/README.md`.
- 상태: r002 수령·적용 완료. 2026-11-16 사용자 「수정된 모습으로 진행하자. r002를 작업 원본에 반영하고 Unity에 적용해줘」를 원본 외형 승인과 적용 요청으로 기록한다. 최신 수령/실제 적용/원본 승인 리비전은 모두 r002다. 게임 화면 승인은 2026-11-16 사용자 확인 및 두 PLAN 완료 요청으로 완료됐다.
- 실제 수령 경로: `Assets/Sprites/Require/PLAN-033/PLAN-033-SPR-001/r002/Support.png`. Unity AssetDatabase로 [최종 Support.png](../../Creature/Support/Support.png)에 이동했다. GUID `248c380c94ad9be4e89bae91d11d5224`, SHA-256 `6a5cc62a095cadb869a1226c7d89acc227c605d0b4470724e46d5f3da53340a9`.
- PPU 32·Point·FullRect·Clamp·무압축·MipMap/ReadWrite Off·sRGB/알파 사용·MaxSize 512·플랫폼 override 없음. LaboratoryArt의 정의별 64프레임/머리 좌표와 Support SO Icon을 연결했고 목록·배양 대상에 대표 프레임을 표시한다. 생성 도구는 연결을 보존한다.
- 전체 리소스 준비: 2026-11-16 충족. [검수 기록](../../../../../docs/plans/evidence/PLAN-033/README.md)에 참조 확인과 테스트 결과·게임 화면을 기록한다. Art 작업 원본·r002 제출본의 해시는 일치한다. 원본 보존본은 Art에 있다.
- 전체 필수 시트 수령·적용·참조 연결 전에는 art PLAN 게임 테스트를 시작하지 않는다. 원본 사용자 확인과 게임 화면 사용자 확인은 별도로 기록한다.

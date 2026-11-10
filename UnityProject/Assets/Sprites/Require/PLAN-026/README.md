# PLAN-026 Sprite 요청

출처·적용 담당: [PLAN-026](../../../../../docs/plans/PLAN-026-art-style-scale-and-sample-integration.md).
공통 PPU·임포트값·수령 절차: [아트 지침](../../../../../docs/agents/art-assets.md).
발행 요청은 바닥 세트·기초 획득시설·프로토타입 보조형 애니메이션·유기물이다. 2026-11-10 바닥 r003·시설 r003·프로토타입 r002·유기물 r001과 기존 Build r001의 Unity 적용·참조 연결을 완료했다. 명세·Art 승인·게임 적용·최종 게임 화면 검수를 구분한다.
[전체 리소스 작업 완료 후 테스트 원칙](../../../../../docs/agents/verification.md#art-plan-test-readiness)을 충족한 뒤 통합 기술 검증과 최종 게임 화면 사용자 승인을 완료했다. 아래 바닥 부분 검증은 이전 이력으로 보존하며, 최신 검증 상태는 위 PLAN과 마지막 적용 기록을 따른다.

## PLAN-026-SPR-001 — 연구소 바닥 세트

- 구분: 신규, 바닥 기본과 변형 3장. 애니메이션·방향 변형 없음.
- 사용자 요청: 2026-11-07 참고 이미지와 활성 Unity PLAN을 바탕으로 에이전트의 바닥 제작을 명시적으로 위임하고, Art PLAN부터 작성하도록 지시했다.
- Art 제작: [PLAN-001](../../../../../../Art/docs/plans/PLAN-001-laboratory-floor.md), `asset_id: LaboratoryFloor`, 최신 r003.
- 기준 이미지: [사용자 제공 참고](../../../../../../Art/references/laboratory-floor/reference-2026-11-07.png).
- 용도·표시 위치: 연구소 지도 한 칸을 채우는 저대비 청회색 바닥. 기존 칸별 Image 표시의 샘플 아트.
- 규격: `LaboratoryFloorSet.png` 한 장, 256×64·4열×1행·각 셀 64×64, 전체 불투명, RGB/RGBA·sRGB. 정적 변형 4개이며 PNG 100%, 잘라내기·리사이즈·회전 없음.
- Unity 슬라이싱: Multiple, 셀 64×64, Offset (0,0), Padding (0,0), 아래 네 Rect로 적용 완료. PPU 32·중심 피벗·Full Rect·Point·Clamp·압축/Mipmap/ReadWrite 없음. 플랫폼별 축소 override 없음.
- 그림: 기본은 최소한의 질감, 변형은 내부의 작은 표면 질감·긁힘·얼룩. 사방 가장자리 색을 맞추고 진한 사각 외곽선·방향성 그림자·UI 표식은 포함하지 않는다.
- 혼합 가중치: 기본 70%, 나머지 각 10%. 칸 좌표의 고정 해시로 선택하며 게임 난수·저장 필드는 추가하지 않는다. 현재 16×10 지도는 기본 116·질감 9·긁힘 14·얼룩 21칸이다.
- 피벗: 중심 `(0.5, 0.5)` 시작안. 실제 표시 크기·정렬은 Unity 적용 때 확인한다. 공통 PPU는 위 아트 지침 참조.
- 교체할 Temp_ 에셋: 없음. 기존 색상/문자 기반 표현에 신규 연결할 대상.
- 사용처·참조: `LaboratoryMapView`의 바닥 레이어, `AppConfiguration.uiAssets`와 연구소 씬의 `LaboratoryView.uiAssets`. 생성 도구도 같은 네 Sprite 참조를 복원한다.
- 최종 배치: [LaboratoryFloorSet.png](../../World/Laboratory/LaboratoryFloorSet.png). 원본 PNG의 픽셀·색과 에셋 GUID `01104bcccd5217245a27f0ce9bb47fb4`를 유지해 AssetDatabase로 이동했다.
- 최신 수령: r003 시트 한 장을 위 정규 경로로 이관했다. r001/r002 개별 PNG 수령 이력과 Art의 r003 제출 원본은 보존한다.
- 원본: Art `assets/backgrounds/LaboratoryFloor/source/LaboratoryFloorSet.aseprite` 한 파일. 프레임 1/Base, 2/Texture, 3/Scuffed, 4/Stained의 정적 변형이며 애니메이션이 아니다. 제출 보존은 `revisions/r003/source/`, Unity에는 PNG 시트만 전달한다.
- 상태: **Unity 적용·바닥 기술 검증 완료 / 최종 게임 화면 사용자 승인 완료(2026-11-10)**.
- 최신 수령·실제 적용 리비전: r003 (2026-11-07).
- 사용자 확인: 2026-11-07 한 파일 구성을 확인하고 같은 Base의 변형을 단일 시트로 전달해 Unity에서 나누도록 지정했다. 최종 게임 화면 검수와 구분한다.

| 셀·Sprite 이름 | Rect x,y,w,h | 모습 | 수령·기술 검사 |
| --- | --- | --- | --- |
| LaboratoryFloor | 0,0,64,64 | 기본 | r003 시트 수령·셀 검사 통과 |
| LaboratoryFloorVariant | 64,0,64,64 | 질감 | r003 시트 수령·셀 검사 통과 |
| LaboratoryFloorScuffed | 128,0,64,64 | 긁힘 | r003 시트 수령·셀 검사 통과 |
| LaboratoryFloorStained | 192,0,64,64 | 얼룩 | r003 시트 수령·셀 검사 통과 |

한 행이므로 위 y=0은 Aseprite 좌상단 원점과 Unity Sprite Rect의 좌하단 원점에서 모두 같다. 기존 파일 기본 이름을 하위 Sprite 이름으로 유지한다.

## PLAN-026-SPR-002 — 기초 획득시설

- 구분: 기존 facility.gathering 기본 방향 외형 1종과 Idle·Working·Complete 각각 12프레임(총 36프레임).
- 제작 승인: 2026-11-07 사용자가 Art PLAN-002 활성화와 참고 이미지 기반 제작, 이후 상태 애니메이션·디스플레이 아이콘·Working/Complete 점멸을 요청했다.
- r003 정정: 합계 12프레임은 에이전트의 해석 오류였다. 사용자가 상태마다 12프레임을 지정하고 필요시 복수 스프라이트를 허용했다. 같은 본체를 공유하므로 원본 하나·시트 하나를 유지한다.
- Art 제작: [PLAN-002](../../../../../../Art/docs/plans/PLAN-002-gathering-facility.md), asset_id Gathering, 최신 r003.
- 기준: [사용자 참고 이미지](../../../../../../Art/references/gathering/reference-2026-11-07.png).
- 파일: Gathering.png, **768×768 RGBA 투명 시트**, 6열×6행, 셀 128×128. 좌상단 행 우선, 첫 2행 Idle·다음 2행 Working·마지막 2행 Complete. 간격·시트 여백 0, 트리밍·회전·리사이즈·병합 없음.
- 태그·시간: Idle 1~12 각 200ms/2.4초 반복, Working 13~24 각 100ms/1.2초 반복, Complete 25~36 각 100ms/1.2초 한 번 후 Idle.
- 아이콘: Idle 민트 원형 고정. Working 호박색 순환 화살표 300ms ON/300ms OFF. Complete 체크 ON 200ms/OFF 200ms/ON 200ms/OFF 200ms/ON 400ms.
- 모습: 중앙 수집조·왼쪽 아래 패널·오른쪽 아래 펌프. Idle은 미세한 수면 반사·부유, Working은 물결·기포·연결 링 가동, Complete는 확산 물결 후 정착. 본체·그림자·수직 천장 광원은 고정.
- 재생 계약: 실제 수집 중 Working, 실제 유기물 생성 사건에 Complete 1회. 애니메이션 루프 횟수로 자원을 생성하지 않는다. 입고 대기·작업 중단은 Idle, 연구소 일시정지 중 시간·프레임 유지.
- Unity 적용 설정: Multiple·공통 PPU 32·중심 피벗 (0.5,0.5) 시작안. Sprite 이름은 각 상태 Gathering_Idle_01~12, Gathering_Working_01~12, Gathering_Complete_01~12. 전체 Rect·시간은 아래 제출 명세 참조.
- 용도·위치: 연구소 지도·배치 미리보기. 2×2 점유·상호작용 (0,-1) 유지. 기존 색상/문자 표시 교체 대상이며 해당 Temp_ 에셋은 없다.
- 참조: LaboratoryArt SO의 Gathering 정의·36프레임 → LaboratoryMapView 시설 표시. 최종 경로 Assets/Sprites/Facility/Gathering/Gathering.png. 적용·4방향 회전 검증·사용자 시각 확인 완료.
- 최신 수령: [r003 Gathering.png](../../Facility/Gathering/Gathering.png), 2026-11-07. **제작·수령·Art 승인·Unity 적용 완료 / 최종 게임 화면 사용자 승인 완료(2026-11-10)**. Art PLAN-002의 최신 completed·승인 r003 기록을 따른다.
- 최신 수령·실제 적용 r003. 2026-11-10 정규 배치·36셀 슬라이싱·임포트·LaboratoryArt SO/씬/공용 참조와 실제 상태별 재생을 연결했다.
- 원본: .aseprite 하나, 36프레임·3태그·10레이어. 고정 레이어 5개는 연결 셀. [r003 제출 명세·Rect·시간·GIF·검사](../../../../../../Art/assets/buildings/Gathering/revisions/r003/README.md).
- 파일 확인: 36셀·589,824픽셀의 원본/PNG 차이 0. 각 상태 고유 이미지 12개. 고정 구조·태그·시간·점멸·여백 확인. Art/Unity PNG SHA-256 F58CDA52D4245B639E9A40BB3245D08CFEF15AEBF6E90E4D56B141D57E30C7D7 일치.
- 이전 제출: [r001 PNG](../../../../../../Art/assets/buildings/Gathering/revisions/r001/export/Gathering.png), [r001 명세](../../../../../../Art/assets/buildings/Gathering/revisions/r001/README.md), [r002 PNG](../../../../../../Art/assets/buildings/Gathering/revisions/r002/export/Gathering.png), [r002 명세](../../../../../../Art/assets/buildings/Gathering/revisions/r002/README.md). r001은 정적 1장, r002는 합계 12프레임/512×384였으며 둘 다 Unity 미적용이었다. 제출·검사 이력과 파일은 Art에 보존하고, 2026-11-10 사용자 요청으로 Unity의 미사용 수령 복사본을 제거했다.
- 전체 리소스 준비 후 실제 상태 연결·4방향 회전·36프레임 재생을 검증하고 사용자의 최종 게임 화면 승인을 받았다.

<a id="plan-026-spr-003--프로토타입-보조형-애니메이션"></a>

## PLAN-026-SPR-003 — 프로토타입 보조형 애니메이션

- 구분: 신규. 기존 시작 개체 `PrototypeSupport` 1종의 Idle·Move·Work 각각 상·하·좌·우 4방향, 총 12태그·64프레임과 머리 위 자원 받침 좌표.
- 요청 근거: 2026-11-07 사용자가 보조형 애니메이션 추가 후 상하좌우 4방향과 각 애니메이션의 프레임 수 명시를 요청했다. 이번 제작 기준은 **각 방향 Idle 4·Move 6·Work 6**, 총 64프레임이다. 종전 단일 방향 16프레임안을 대체하며 기존 제작·제출 기록은 당시 명세로 보존한다.
- Art 제작: [PLAN-003](../../../../../../Art/docs/plans/PLAN-003-prototype-support.md)은 `completed`, asset_id `PrototypeSupport`, 최신 승인 r002·approved_revision 2다. 2026-11-07 사용자가 r002 확인·승인과 PLAN-003 완료를 명시했다. 검은 두 점눈·4방향·64프레임을 원본 하나·PNG 하나로 제작·파일/재생 확인·수령·사용자 시각 승인까지 마쳤고 기존 r001은 보존했다. 2026-11-10 Unity 적용·게임 테스트·별도 게임 화면 사용자 승인도 완료했다.
- 참고·외형: 최신 [두 점눈·4방향 사용자 참고](../../../../../../Art/references/prototype-support/reference-2026-11-07-two-eyes-four-directions.png)를 기준으로 한다. FRONT/LEFT/RIGHT/BACK은 Down/Left/Right/Up이며 검은 점눈을 이 캐릭터의 시작값으로 사용한다. [이전 참고](../../../../../../Art/references/prototype-support/reference-2026-11-07.png)와 과거 외눈 수정 기록은 보존한다. 64×64 몸체와 머리 위 별도 자원 계약은 유지한다.
- 용도: 연구소 지도에서 프로토타입의 대기·실제 이동·건설/수집 작업을 표시한다. 보급형·침공형 동작·새 게임 기능은 포함하지 않는다.
- 외형: [상호작용이 많은 생명체 기준](../../../../../docs/game-design/05-development/02-implementation/art-roadmap.md#interaction-friendly-creatures)의 팔다리 없는 단순한 슬라임·젤리형. 둥글고 부드러운 몸체와 귀엽고 친근한 인상을 우선한다.
- 얼굴: r002는 **검은 단색 점눈**이며 각 눈은 3×3 범위의 5픽셀이다. 흰자·홍채·동공·눈 안 반사점 없이 Down 정면 두 눈, Left/Right 측면 한 눈, Up 뒷면 눈 없음으로 제작했다. 기존 외눈·동공 초안의 현재 명세를 대체하며 Art r002의 사용자 시각 승인은 완료했다. Unity 게임 화면 검수는 적용 후 별도로 진행한다.
- 프레임 크기: **64×64 RGBA 투명**, 본체는 중앙 약 48×48과 사방 약 8픽셀 여백을 시작안으로 한다. 움직임이 최대인 프레임도 셀 안에 포함하고 몸체·얼굴·접지의 의도하지 않은 이동을 방지한다.
- 원본: `PrototypeSupport.aseprite` 1개, **64프레임·12태그**. 별도 자원·이펙트·선택 원·이름·상태 아이콘을 몸체에 굽지 않는다.
- 출력: `PrototypeSupport.png` **512×512, 8열×8행, 셀 64×64** 1개. 좌상단부터 행 우선, 간격/시트 여백 0, 100% 크기, 트리밍·패킹 회전·프레임 병합 없이 내보낸다. 임포트는 Multiple이며 공통 PPU·필터·압축은 상단 아트 지침을 따른다.

| Aseprite 태그 | 프레임 번호(1부터) | 프레임 수 | 프레임별 시간·반복 |
| --- | --- | --- | --- |
| `Idle_Down` | 1~4 | 4 | 각 250ms, 총 1000ms, 정방향 반복 |
| `Move_Down` | 5~10 | 6 | 각 100ms, 총 600ms, 정방향 반복 |
| `Work_Down` | 11~16 | 6 | 각 120ms, 총 720ms, 정방향 반복 |
| `Idle_Left` | 17~20 | 4 | 각 250ms, 총 1000ms, 정방향 반복 |
| `Move_Left` | 21~26 | 6 | 각 100ms, 총 600ms, 정방향 반복 |
| `Work_Left` | 27~32 | 6 | 각 120ms, 총 720ms, 정방향 반복 |
| `Idle_Right` | 33~36 | 4 | 각 250ms, 총 1000ms, 정방향 반복 |
| `Move_Right` | 37~42 | 6 | 각 100ms, 총 600ms, 정방향 반복 |
| `Work_Right` | 43~48 | 6 | 각 120ms, 총 720ms, 정방향 반복 |
| `Idle_Up` | 49~52 | 4 | 각 250ms, 총 1000ms, 정방향 반복 |
| `Move_Up` | 53~58 | 6 | 각 100ms, 총 600ms, 정방향 반복 |
| `Work_Up` | 59~64 | 6 | 각 120ms, 총 720ms, 정방향 반복 |

- 동작: Idle은 작은 호흡·눌림→복원, Move는 이동 리듬의 눌림·늘어남, Work는 실제 건설/수집에 사용하는 더 분명한 제자리 눌림·팽창이다. 작업별 전용 몸체·도구 프레임은 추가하지 않는다.
- Sprite 이름: `PrototypeSupport_<동작>_<방향>_<두 자리 번호>`. 예: `PrototypeSupport_Idle_Down_01`~`04`, `PrototypeSupport_Move_Left_01`~`06`. 번호는 각 태그 안에서 다시 01부터 시작한다.
- 배열: Down→Left→Right→Up 순서로 각각 16셀·2행이다. 각 방향 내부는 Idle 4→Move 6→Work 6이다. Unity Rect는 전체 프레임 번호 `n`에 대해 `x=64*((n-1)%8)`, `y=64*(7-floor((n-1)/8))`, `w=h=64`인 좌하단 원점이다. 제출 명세에 실제 이름·Rect·시간을 기록한다.
- 방향별 외형: Down은 정면, Up은 뒷면, Left/Right는 해당 측면이다. Up에 정면 얼굴을 그대로 붙이거나 원본을 90도 회전해 방향을 대신하지 않는다. 좌우도 각각 태그와 출력 셀을 제공한다. 대칭 부분의 제작 재사용은 가능하나 눈·무늬·광원·실루엣·받침점을 검수하며 런타임 자동 반전을 전제하지 않는다.
- 방향·피벗: +Y=Up, -Y=Down, -X=Left, +X=Right인 지도 논리 축 기준이다. 화면 가로/세로 회전으로 방향 정의를 바꾸지 않는다. 모든 방향과 프레임은 64×64·중심 피벗 `(0.5,0.5)`·같은 바닥 접지 기준을 사용한다. 대각선 시트는 제외한다.
- 바닥·머리 기준점: 제출 명세에 64×64 프레임 좌상단 원점의 픽셀 좌표로 공통 접지점과 **각 방향의 기본 머리 받침점**을 적는다. 프레임별 변화는 기본점 대비 `(dx,dy)`로 기록하며 같은 좌표는 공통값으로 묶을 수 있다. 기준점은 PNG에 그리지 않는다.
- 자원 합성: `Carry` 전용 몸체 시트는 없다. 이동은 해당 방향의 Move+머리 위 자원, 대기는 마지막 방향의 Idle+자원으로 표현한다. 자원은 같은 그림을 네 방향에서 사용하며 받침 위치만 따라간다. 몸체 방향·눌림에 자원 크기·회전·반전을 전파하지 않는다.
- 자원 그리기 순서: 자기 몸체 위에 자원을 표시하되 다른 개체/시설과의 순서는 몸체의 지도 위치를 따른다. Up에서도 머리에 올려진 위치·자원 식별성을 유지하고 상태 UI와 겹치지 않게 한다.
- 집기·전달: `PickUp`/`Drop` 프레임을 만들지 않는다. 실제 집기·부분/전량 전달·소비 결과로 자원 표시를 유지/해제하며 예약·초기 지급 재고만으로 표시하지 않는다. 몸체 애니메이션은 자원이나 작업 진행을 발생시키지 않는다.
- 상태·방향 선택: 실제 경로 구간의 진행 방향으로 Move, 상호작용 칸에서 인접한 시설 점유 칸을 향하는 방향으로 Work, 대기 시 마지막 유효 방향의 Idle을 선택한다. 배정·예약·접근 불가만으로 Work를 표시하지 않는다. 목표 시설의 네 방향 회전에도 적용한다.
- 전환: 동작이 같고 방향만 바뀌면 반복 진행률을 유지하며 대응 방향 프레임을 선택한다. 동작이 바뀔 때만 첫 프레임으로 전환한다. 반복 경계와 같은 진행률에서의 방향 변경이 형태·접지·머리 위치를 불필요하게 튀게 하지 않는지 확인한다.
- 일시정지·복원: 연구소 진행 정지 시 현재 동작·방향·프레임/시간을 유지한다. 새 판·이어하기는 실제 이동 구간/작업 대상으로 방향을 재구성하고 해당 동작의 첫 프레임에서 시작한다. 방향 근거가 없는 최초 대기는 Down이다. 표시용 방향·프레임은 게임 저장에 추가하지 않는다.
- 별도 전달 정보: PNG에 프레임 시간이나 받침 좌표가 들어 있다고 가정하지 않는다. 태그·프레임 범위·시간·방향·피벗·접지·받침 좌표를 제출 명세에 함께 적는다. [Art 애니메이션 지침](../../../../../../Art/docs/agetns/animation/README.md)을 따른다.
- 교체 대상: 기존 프로토타입의 색상/문자 표시. 교체할 전용 Temp PNG는 없음. 기존 선택·상태 UI는 유지한다.
- 참조: `Assets/SO/Creature/PrototypeSupport.asset`와 연구소 표현의 동작×방향별 프레임·시간·받침 좌표 구성. `Icon`에는 필요 시 `Idle_Down_01`을 연결하되 Icon 하나만으로 전체 동작이 연결된 것으로 처리하지 않는다.
- 최종 배치: `Assets/Sprites/Creature/PrototypeSupport/PrototypeSupport.png`. 최신 수령은 [r002 PrototypeSupport.png](../../Creature/PrototypeSupport/PrototypeSupport.png), 2026-11-07이며 2026-11-10 적용했다.
- 제출 명세: [r002 원본·64개 Rect/시간/머리 좌표·12개 태그·GIF·파일 검사](../../../../../../Art/assets/characters/PrototypeSupport/revisions/r002/README.md). 기본 머리 받침은 네 방향 각각 대조한 (32,24), 공통 접지 (32,54), 좌상단 픽셀 좌표다. 프레임별 dx=-1~1, dy=-3~3이며 r001의 (32,22)를 그대로 사용하지 않는다.
- 상태: **r002 제작·수령·Art 승인·Unity 적용 완료 / 최종 게임 화면 사용자 승인 완료(2026-11-10)**. 최신 수령·승인·실제 적용 r002. 64프레임 슬라이싱·LaboratoryArt SO·씬/공용 참조·상태/방향/운반과 64개 머리 좌표를 연결했다.
- 파일 확인: 64셀·262,144픽셀의 원본/PNG 차이 0, 12태그·시간·눈 개수/단색·접지·받침·여백·그림자 고정·가이드 제외 확인. Art/Unity PNG SHA-256 `15CB6825C1335023B6397E515DF8CCF9BB1DC6D19D65AD8734CCA1C2AA08EAD3` 일치. 실제 Aseprite 재생과 Art 합성 비교는 Unity 게임 검수와 구분한다.
- 이전 이력: [r001 PNG](../../../../../../Art/assets/characters/PrototypeSupport/revisions/r001/export/PrototypeSupport.png)와 [당시 Art 명세](../../../../../../Art/assets/characters/PrototypeSupport/revisions/r001/README.md)는 외눈·16프레임·256×256 초안이다. Art/Unity PNG SHA-256 `444A16B4FE1DDF8E8458B09F1CF4666A4C252E7E9FC40CF8BDD85B7D41F1CDB6` 일치 기록과 Art 보관본을 유지한다. r001은 Unity 미적용이었으며 2026-11-10 사용자 요청으로 Unity의 수령 복사본만 제거했다.
- 검증: 전체 리소스 준비 후 12태그·64셀·경로 모서리·작업 방향·받침·부분 입고 잔량·일시정지/복원·독립 재생을 확인했다. 최종 게임 화면은 2026-11-10 사용자가 승인했다. 상세 증거는 마지막 통합 기록을 따른다.

## PLAN-026-SPR-004 — 유기물 정적 이미지

- 구분·요청 근거: 신규. 2026-11-07 사용자가 활성 Unity PLAN의 유기물 제작을 요청했다. PLAN 1-4와 사용자 의도에 따라 애니메이션 없음·정적 1장·방향 구분 없음이다.
- Art 제작: [PLAN-004](../../../../../../Art/docs/plans/PLAN-004-organic-matter.md), `asset_id: OrganicMatter`, completed·r001 제작/파일 검증/전달/사용자 승인 완료·approved_revision 1. 2026-11-07 제출 후 사용자가 현재 PLAN 완료를 요청했다.
- 용도: 유기물 수량 옆 아이콘과 실제 운반 중 보조형 머리 위 표시가 같은 그림을 재사용한다.
- 명세: 32×32 RGBA 투명, 중앙 약 24×24·사방 4px 이상 여백. 부드럽고 불규칙한 세포 군집 하나, 용기·액체와 형태로 구별. r001 Art 외형·색은 사용자 승인 완료이며 Unity 게임 화면 검수는 별도다.
- 디자인 시작안: 획득시설 수집조의 황록색 생체 재료와 연결되는 올리브색 군집. 윗면을 밝게 하는 수직 천장 광원, 1px 어두운 외곽, 큰 색면. 눈·입·팔다리·문자·수량·상태 프레임·바닥 그림자는 넣지 않는다.
- 원본·출력: Art에 `OrganicMatter.aseprite` 1개·1프레임, Unity용 `OrganicMatter.png` 32×32 1개. 트리밍·회전·스케일·애니메이션·방향별 변형 없음.
- Unity 설정 계약: Single·Rect (0,0,32,32)·중심 피벗 (0.5,0.5)·Full Rect·PPU 32·Point·압축/밉맵 없음. 실제 임포트·참조 연결은 별도 대화에서 수행한다.
- 운반: 자원 하단 받침은 좌상단 원점 **(18,28)px**, Unity 중심 피벗 기준 (2,-12)px. 캐릭터 머리 (hx,hy)에 맞춘 자원 캔버스 좌상단은 (hx-18,hy-28). 프레임별 위치만 따르며 자원 크기·회전·반전은 유지한다. Art 합성 비교와 실제 게임 검수는 구분한다.
- [r001 수령 PNG](../../Resource/OrganicMatter/OrganicMatter.png), [Art 제출 명세·비교·검증](../../../../../../Art/assets/resources/OrganicMatter/revisions/r001/README.md). 최종 배치: `Assets/Sprites/Resource/OrganicMatter/OrganicMatter.png`.
- 상태: **r001 제작·수령·Art 승인·Unity 적용 완료 / 최종 게임 화면 사용자 승인 완료(2026-11-10)**. 최신 수령·승인·실제 적용 r001. ResourceDefinition.Icon·HUD 수량 아이콘·실제 운반 표시가 같은 Sprite를 사용하며 제출 원본·PNG 해시는 유지했다.
- 수령 검사: 32×32·1프레임·6색+투명·알파 0/255, 불투명 bounds (5,5,23,23), 원본/PNG 1,024픽셀 차이 0. 캐릭터 64프레임 머리 받침 대입 시 눈/몸체 겹침 0. Art/Unity PNG SHA-256 `43D5769FAB670FDB1A3411F3B415EE72681C2680FC481F96481F4A39F96C3243` 일치.
- 검증: 32×32·알파·여백·단일 프레임·원본/PNG 일치·가이드 제외·수량 옆 아이콘 및 네 방향 운반 Art 비교. Unity 적용·게임 테스트는 전체 리소스 준비 조건을 따른다.

## 기존 건설 메뉴 아이콘 요청 연결

- Build는 [PLAN-014-SPR-006](../PLAN-014/README.md#plan-014-spr-006--건설-메뉴-아이콘-제작-연결)을 재사용한다. 2026-11-10 사용자 승인에 따라 [Art PLAN-005](../../../../../../Art/docs/plans/PLAN-005-build-icon.md)는 completed·r001 제작/파일 검사/수령/시각 승인 완료·approved_revision 1이다.
- 64×64 투명 정적 PNG 1장·애니메이션 없음. [r001 적용본](../../UI/Laboratory/Build.png)은 기존 PLAN-014 수령 경로에서 정규 경로로 이동했다. icons[6]·생성 도구 연결을 완료했으며 최종 게임 화면 승인까지 완료했다.

## 검증·전달 이력

2026-11-07: r001 PNG 4장 수령. 64×64·불투명·1프레임, Aseprite 원본/제출본/PNG 전 픽셀 일치, 수평 16개·수직 16개 경계 조합 불일치 0을 확인했다.
Art와 수령본의 SHA-256도 4개 모두 일치한다. [제출 명세·비교 이미지·검사 증거](../../../../../../Art/assets/backgrounds/LaboratoryFloor/revisions/r001/README.md)를 참조한다.
Unity 임포트 설정·참조 연결·씬 표시·사용자 시각 검수는 별도이며, 파일 수령을 적용 완료로 기록하지 않는다.

2026-11-07 r002: 사용자 요청에 따라 원본을 한 파일·4프레임·4태그로 통합했다. r001 대비 각 편집 레이어와 PNG 픽셀 차이는 0이며 이름·64×64·중심 피벗 시작안은 동일하다.
[r002 제출 명세·검증·전달 체크섬](../../../../../../Art/assets/backgrounds/LaboratoryFloor/revisions/r002/README.md)에 통합 원본과 4개 출력의 대응을 기록했다. 사용자 시각 확인·Unity 적용은 계속 대기한다.

2026-11-07 r003: 사용자 지정에 따라 PNG 시트 한 장으로 전환했다. 4셀의 원본 대비 픽셀 차이 0, 16384픽셀 불투명, Art/Unity 수령본 SHA-256 일치.
[r003 명세·검증·전달 체크섬](../../../../../../Art/assets/backgrounds/LaboratoryFloor/revisions/r003/README.md)에 원본·시트·슬라이싱 계약을 기록했다. 실제 Unity 슬라이싱·임포트 설정·게임 적용은 대기한다.

2026-11-07 r003 적용: 사용자가 64×64 슬라이싱과 4종 혼합 연구소 적용을 명시적으로 요청했다. 위 정식 경로·임포트·두 참조를 연결하고, 기존 선택/건설/상태 표시 아래에 틈 없는 바닥 레이어를 추가했다. 네 타일을 임의 회전·반전·재색칠하지 않았다.
[적용 증거·가로/세로 3배율·기본/혼합 비교·검증 결과](../../../../../docs/plans/evidence/PLAN-026/floor-r003/README.md)를 참조한다. Editor 기술 검사와 사용자 시각 승인은 구분하며, 사용자 최종 확인과 나머지 4개 분류는 대기다. 빌드·기기 검증은 실행하지 않았다.

## 2026-11-10 Unity 적용 상태

- 사용자 요청에 따라 최신 Art PLAN 승인본을 사용했다. Art 승인과 Unity 게임 화면 사용자 시각 확인은 별도다.
- 바닥 r003의 기존 적용을 보존하고 Gathering r003·PrototypeSupport r002·OrganicMatter r001·Build r001의 정규 경로 이동·임포트·참조 연결을 완료했다. 이전 수령 경로의 PNG는 GUID를 보존해 이동했으며 위 파일 링크는 정규 경로를 가리킨다. 이전 리비전과 Aseprite 원본은 유지한다.
- 전체 5 PNG·106 Sprite, PNG 해시/GUID 5개·바닥 Sprite ID 보존을 정적으로 확인했다. 이 시점까지 게임 테스트는 실행하지 않았으며 이제 전체 적용 후 검증을 진행한다.
- [통합 적용 증거](../../../../../docs/plans/evidence/PLAN-026/integration-2026-11-10/README.md)에 실제 경로·임포트·참조·검증 결과와 사용자 게임 화면 확인 상태를 기록한다. 이전 대기 기록은 해당 날짜의 이력이며 최신 적용 상태는 이 절을 따른다.

## 2026-11-10 통합 검증·사용자 확인 완료

- 최신 적용·검수: 바닥 r003(4변형), Gathering r003(3상태 36프레임), PrototypeSupport r002(4방향 12태그 64프레임), OrganicMatter r001, Build r001. 모두 적용·기술 검증·별도 게임 화면 사용자 승인 완료다.
- 사용자가 실제 가로/세로·운반·수집 동작 자료를 확인하고 “현재 게임 화면 승인”으로 답했다. 원본 Art 승인과 구분한 Unity 시각 검수 기록이다.
- Play Domain Reload ON 38/38, 최종 OFF 아트 5/5, Edit 생성 도구 보완 후 관련 6/6 통과. Console 오류/경고 0. 원본 PNG/GUID와 바닥 meta, 게임 JSON·환경을 보존했다.
- [완료 증거와 후속 제작 채택 기준](../../../../../docs/plans/evidence/PLAN-026/integration-2026-11-10/README.md). PLAN-026 completed·5-1 완료이며 후속 PLAN-027의 월드 표시 이관도 별도 승인·Editor 검증 후 completed다. 원본 리비전·Sprite 식별자와 기존 아트 승인 상태는 유지한다. Android 빌드/실기기는 이번 범위에서 실행하지 않았다.

## 2026-11-10 미사용 Sprite 정리

- 사용자 요청으로 교체 완료된 Temp_Build와 이전 리비전 수령 PNG 11장, 해당 메타데이터·빈 폴더를 제거했다. 이전 리비전의 Art 보관본과 Aseprite 원본, 요청·승인 이력은 유지한다.
- 삭제 대상 참조 0, 보관본 11장 해시 일치, 남은 22 PNG의 해시/GUID 불변과 정식 아트 Edit Mode 6/6 통과를 확인했다. [삭제 목록·검증 증거](../../../../../docs/plans/evidence/PLAN-026/cleanup-2026-11-10/README.md)를 참조한다.
- 요청 완료 상태와 PLAN 상태는 변경하지 않는다. 새 Sprite 요청은 없다.

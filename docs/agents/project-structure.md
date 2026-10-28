# 프로젝트 디렉터리 구조

적용: 폴더·파일·에셋의 생성, 배치, 이동, 이름 변경. Scripts뿐 아니라 프로젝트 전체에 적용한다.
탐색: [루트 MAP](../../AGENTS.md), [스크립트 구조](script-structure.md), [Unity 에셋](unity-assets.md), [작업 절차](workflow.md).

## 공통 분류 규칙

- 각 디렉터리에 직접 들어가는 파일은 해당 디렉터리가 맡은 동일한 주제와 역할에 속해야 한다.
- 같은 부모 아래의 형제 디렉터리는 같은 분류 기준으로 나눈다. 역할·기능·개별 에셋을 같은 층위에 섞지 않는다.
- 상위 분류는 하위 주제를 묶는 곳이다. 상위 폴더에 여러 하위 주제의 파일을 나열하지 않는다.
- 세부 주제가 생기면 하위 폴더로 분리한다. 같은 주제의 파일은 한 위치에서 관리하고 편의를 위한 복사본을 두지 않는다.
- `Common`, `Shared`, `Utils`, `Managers`, `Misc`를 서로 무관한 파일의 보관함으로 쓰지 않는다. 실제 공통 책임이 있으면 그 책임을 이름으로 드러낸다.
- 분류 안내 README, 폴더·파일의 `.meta`, 어셈블리 경계를 정하는 `.asmdef`·`.asmref`는 해당 범위의 관리 파일로 함께 둘 수 있다.
- 사용자가 지정한 이름·구조를 우선한다. 새 기능 하위 폴더는 승인된 작업 범위에서 에이전트가 필요에 따라 추가할 수 있다.
- 새 분류는 기존 책임과 겹치지 않게 이름을 붙이고 작업 PLAN에 이유·경로를 남긴다. 독립 Sprite 작업은 요청 README에 기록한다. 상위 분류나 공통 배치 원칙이 바뀌면 이 지침과 MAP도 갱신한다.
- 폴더 예시는 배치 기준이다. 실제 파일이 필요할 때 생성하며 예시만 보고 빈 폴더·새 콘텐츠·기능을 추가하지 않는다.
- 폴더·파일·에셋의 영문 표기와 대소문자는 [명명 규칙](naming.md)을 따른다.

예: `Presentation/` 아래는 `UI`, `VFX`, `Input`처럼 표현 기능으로 나눈다.
`UI/` 아래가 `FacilityPanel`, `CreaturePanel`로 세분화되면 각 패널의 파일을 해당 폴더에 둔다.
한 부모 아래에 `Creature`, `Scripts`, `PrototypeSupport`를 함께 두는 식으로 주제·파일 역할·개별 종류를 섞지 않는다.

## 저장소 경계

| 위치 | 책임 |
| --- | --- |
| `UnityProject/` | Unity에서 여는 프로젝트. 게임 에셋과 프로젝트 설정 |
| `docs/game-design/` | 승인된 기획. 기존 숫자 분류와 README MAP 유지 |
| `docs/agents/` | 작업 지침. 한 문서에 한 주제를 두고 150줄 이내 유지 |
| `docs/plans/` | 작업별 PLAN·상태·검증 결과. README는 PLAN 목록과 status만 표시 |
| `docs/decisions/` | 기술 선택·이유·대체 관계 |
| `scripts/` | 저장소 문서 검사·개발 도구 자동화. Unity C# 코드와 구분 |

기존 문서의 파일명·소문자·숫자 분류를 Unity 폴더 규칙에 맞추기 위해 바꾸지 않는다.
Unity의 `Packages/`, `ProjectSettings/`, 생성 캐시의 위치와 관리 정책은 [저장소 지침](repository.md)을 따른다.

## Assets 분류

아래 경로는 모두 `UnityProject/Assets/` 기준이다. 에셋 종류를 먼저 나누고 그 안에서 기능·주제로 세분화한다.

| 상위 경로 | 담을 파일 | 하위 분류 예시 |
| --- | --- | --- |
| `Scripts/` | 프로젝트 C#·관련 어셈블리 설정. Editor·테스트 코드 포함 | 역할 → 기능. [상세 기준](script-structure.md) |
| `CSV/` | 밸런스·정의 값의 CSV 원본 | `Balance/` |
| `SO/` | 정의 ID·Unity 에셋 바인딩·주제별 Registry SO 인스턴스 | `Resource/`, `Facility/`, `Creature/`, `Session/` |
| `Scenes/` | 실제 플레이·검증 장면 | `Gameplay/`, `Diagnostics/` → 담당 기능 |
| `Prefabs/` | 재사용할 GameObject 구성 | `Creature/`, `Facility/`, `UI/`, `VFX/` |
| `Sprites/` | 스프라이트 이미지·시트·아틀라스 | `Creature/`, `Facility/`, `UI/`, `World/`, `VFX/` |
| `Textures/` | 스프라이트로 쓰지 않는 텍스처가 필요할 때 | `World/`, `VFX/` |
| `Audio/` | 음원과 오디오 믹서 | `Clips/`, `Mixers/`; Clips 아래 `Music/`, `SFX/`; SFX 아래 `UI/`, `Creature/`, `Facility/` |
| `Animations/` | Animation Clip·Animator Controller | `Creature/`, `Facility/`, `UI/` → 종류별 주제. 필요하면 `Clips/`, `Controllers/`로 분리 |
| `Materials/` | 머티리얼 | `Creature/`, `Facility/`, `UI/`, `VFX/` |
| `Shaders/` | 셰이더·Shader Graph·관련 소스 | 표현 기능별 하위 폴더 |
| `Fonts/` | 폰트 파일·폰트 에셋 | 글꼴별 하위 폴더 |
| `Settings/` | URP·렌더러·입력·템플릿 등 도구/프로젝트 설정 에셋 | `Rendering/`, `Input/`, `SceneTemplates/` |

### Sprite 요청·수령 대기 경로

- `Sprites/Require/PLAN-NNN/README.md`는 [아트 요청 절차](art-assets.md#sprite-requests)의 전용 경로다. 사용자가 제작한 파일도 해당 PLAN 폴더에 넣는다.
- Require는 일반 기능별 분류의 예외다. 형제 폴더는 요청 출처 PLAN 번호로 통일하고, 각 폴더에는 그 PLAN의 Sprite 요청 README와 수령 파일·관련 원본만 둔다.
- PLAN 번호는 출처 식별용이다. 요청 폴더와 처리 기록은 출처 PLAN이 완료된 뒤에도 유지하며, 독립 작업 여부와 별도 art PLAN의 경계는 아트 요청 절차를 따른다.
- 요청이 있을 때만 만든다. 다른 종류의 임시 에셋이나 무관한 자료를 모으는 폴더로 쓰지 않는다.
- 적용할 Sprite는 정규 기능·주제별 경로로 이동하고 README에 최종 위치를 기록한다. Require에 런타임용 복제본을 남기거나 이 경로를 영구 참조하지 않는다. README는 요청·수령·적용 기록으로 유지한다.

## 종류와 주제를 함께 지키기

- 같은 생명체라도 코드·정의·이미지·음원을 한 폴더에 섞지 않는다. 각 종류의 경로 아래에서 같은 주제명을 재사용한다.
- 예: `SO/Creature/PrototypeSupport/`, `Sprites/Creature/PrototypeSupport/`, `Prefabs/Creature/PrototypeSupport/`.
- `Presentation/VFX/`에는 효과 표현 코드, `Prefabs/VFX/`에는 효과 프리팹, `Sprites/VFX/`에는 효과 이미지만 둔다.
- SO 클래스의 `.cs`는 `Scripts/Data/`에, 그 인스턴스 `.asset`은 `SO/`에 둔다. `.asset` 확장자만 보고 URP·렌더러·폰트 등을 SO로 옮기지 않는다.
- 셰이더 소스는 `Shaders/`로 분류한다. 모든 C#을 Scripts에 둔다는 규칙을 모든 텍스트 파일에 확대하지 않는다.
- 게임 저장 파일·테스트 임시 결과·빌드 산출물을 Assets의 정의 에셋으로 넣지 않는다. 실행 저장 경로·검증 증거·생성 파일 관리는 각 담당 지침을 따른다.
- 아트 제공·Sprite PPU와 임포트 기본값·사용자 시각 검수는 [아트 지침](art-assets.md)을 따른다.

<a id="temporary-assets"></a>

## 임의 에셋 표식과 교체

- 파일·에셋·씬 오브젝트·하위 Sprite/Clip의 임시 표식은 [명명 규칙](naming.md)의 `Temp_` 기준을 따른다.
- 임시 에셋도 종류·기능별 원래 분류에 둔다. 여러 주제를 전역 Temp 폴더에 모으지 않는다.
- 경로·목적·사용처·교체할 정식 대상·연결 위치를 기록한다. Sprite 요청 상세는 Require의 README에 두고 PLAN에서 연결한다. 다른 임시 에셋은 PLAN에 기록한다.
- 화면·프리팹·SO의 에셋 참조로 교체할 수 있게 연결한다. 임시 파일명·픽셀 크기를 게임 규칙의 식별자나 동작 조건으로 사용하지 않는다.
- 정식 제공본 연결 시 필요한 참조를 갱신하고 잔여 임시 참조를 확인한다. 파일명에서 Temp만 지워 정식 아트로 간주하지 않는다.
- 검증용 에셋과 개발 위임 초기값을 구분한다. 일반 코드·확정 정의 전체를 임시 에셋으로 일괄 개명하지 않는다. 실제 이관 시 임시 대체물을 식별해 적용한다.

## Unity 고정 경로와 외부 구성

- 게임 자원 주제명은 `Resource`로 쓴다. `Resources`는 Unity 런타임 로딩용 특수 폴더이므로 일반 분류명으로 사용하지 않는다.
- `Editor`, `Resources`, `StreamingAssets`, `Plugins` 등 특수 경로는 실제 기능·도구 요구가 있을 때 사용하고 이유를 PLAN에 기록한다.
- 패키지·외부 도구가 요구하는 원본 경로와 Unity 생성 캐시는 임의 재배치하지 않는다. 프로젝트가 직접 작성하는 C#은 Editor·테스트까지 Scripts 아래에 둔다.
- 필요하지 않은 Resources 로딩·Addressables·외부 플러그인을 폴더 구조만을 이유로 도입하지 않는다.
- Unity 특수 경로의 의미는 [Unity 6.3 공식 문서](https://docs.unity3d.com/6000.3/Documentation/Manual/SpecialFolders.html)를 기준으로 확인한다.

## 현재 구조에서 이관

기존 `Assets/Game/`, `Assets/Tests/`의 코드는 `Assets/Scripts/`, 정의 SO는 `Assets/SO/`로 이관한다.
실제 이관 범위·이동 목록·참조 검사·회귀 검증 결과는 [PLAN-007](../plans/PLAN-007-gameflow-and-persistence.md)에서 확인한다.

1. 실제 파일·참조·고정 경로를 조사하고 주제별 목적지를 정한다. 분류만 바꾸며 게임 동작·클래스명·공개 API 변경을 섞지 않는다.
2. [Unity 에셋 지침](unity-assets.md)에 따라 `.meta`·GUID를 보존한다. 폴더를 새로 만들 때의 메타데이터도 Unity에서 생성한다.
3. 경로를 쓰는 생성 도구·테스트·씬 설정·현행 문서 링크를 함께 갱신한다. 과거 실행 증거의 경로·해시는 당시 사실로 보존한다.
4. 기존 어셈블리와 씬·SO 참조를 확인하고 컴파일·영향받은 기존 테스트를 통과한 뒤 새 기능 구현을 이어간다.
5. 새 파일 배치, 이전 위치의 잔여 코드, 중복 원본, 누락 참조를 확인하고 PLAN에 결과를 기록한다.

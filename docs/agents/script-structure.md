# C# 스크립트 구조

적용: 프로젝트 C#·어셈블리 설정의 생성·배치·이동.
공통 분류·폴더 추가·이관 규칙은 [프로젝트 구조](project-structure.md), 상태 소유와 의존 경계는 [아키텍처](architecture.md)를 따른다.

## 기본 경로

프로젝트에서 직접 작성·관리하는 모든 C#은 `UnityProject/Assets/Scripts/` 아래에 둔다.
일반 C# 객체·MonoBehaviour·SO 클래스·Editor 도구·테스트·프로젝트 생성 C# 모두 포함한다.
런타임 코드, Editor 코드, 테스트 코드는 아래 역할별 경로와 어셈블리 설정으로 분리한다.
외부 패키지 원본·생성 캐시·저장소 자동화 `scripts/`의 경계는 프로젝트 구조 지침을 따른다.

## 역할 → 기능 → 필요한 세부 주제

| 역할 폴더 | 책임 | 기능 하위 폴더 예시 |
| --- | --- | --- |
| `Data/` | CSV 계약·파서·읽기 전용 Data·SO 바인딩·Registry. 개별 실행 상태를 두지 않음 | `Definitions/` 아래 `Resource/`, `Facility/`, `Creature/` |
| `Core/` | 이 게임의 공통 실행 기반. Thing·컴포넌트·식별·단일 소속·Map | `Things/`, `Resource/`, `Spatial/` |
| `Gameplay/` | 건설·생산·운반·배양·생명체 행동 등 기능별 게임 규칙 | `Construction/`, `Production/`, `Logistics/`, `Creature/`, 필요 시 `Player/` |
| `GameFlow/` | 한 판의 준비·진행 단계·전환·정지·다시 시작 조정 | `Session/`, `Transitions/`, `Progression/` |
| `Persistence/` | 저장 데이터·직렬화·파일 입출력·상태 복원. 규칙의 원본 상태는 소유하지 않음 | `Snapshots/`, `Serialization/`, `Storage/`, `Restoration/` |
| `Presentation/` | 화면·입력·UI·시각 효과·Unity 실행 연결. 공통 명령 전달과 상태 표시 | `UI/`, `VFX/`, `Input/`, `Bootstrap/`, `Diagnostics/` |
| `Editor/` | Editor 전용 정의/씬 생성 도구·Inspector·검사 | `Definitions/`, `Scenes/`, `Validation/` |
| `Tests/` | 자동 테스트와 테스트 전용 준비 코드 | `EditMode/`, `PlayMode/` 아래 검증 기능별 폴더 |

기능 폴더는 필요한 시점에 추가한다. 위 예시는 고정된 전체 클래스·폴더 목록이나 기능 구현 승인이 아니다.
`Player`는 플레이어 관련 게임 규칙이 실제 필요할 때 사용한다. 입력 처리만 있다는 이유로 플레이어 개체나 전용 게임 시스템을 만들지 않는다.
역할 최상위에 여러 기능의 `.cs`를 직접 모으지 않는다. 한 기능이 커지면 같은 기능 안에서 하위 주제를 분리한다.

## 책임 경계

- 타입·파일·폴더 이름과 Manager 기준은 [명명 규칙](naming.md)을 따른다.
- 같은 기능 이름은 여러 역할에서 사용할 수 있다. `Gameplay/Creature`는 행동 규칙, `Presentation/Creature`는 생명체 표시를 담당한다.
- `Data` 정의가 Thing·Map·화면 같은 실행 객체를 참조하지 않게 한다. 여러 정의가 공유하는 타입은 `Data/Definitions/` 범위에 둘 수 있다.
- `Core`는 범용 프레임워크를 뜻하지 않는다. 정의를 참조하는 공통 게임 기반이며 Gameplay·GameFlow·Presentation에 의존하지 않게 한다.
- 현재 Thing과 Map이 연결되어 있으므로 둘 모두 Core에 둔다. 이름만 보고 Map을 Gameplay로 분리해 역방향 의존을 만들지 않는다.
- GameFlow는 저장 시점과 세션 전환을 조정하고 Persistence는 저장·복원을 담당한다. 서로의 구현을 순환 참조하지 않도록 값·계약의 경계를 정한다.
- Presentation은 게임 규칙을 복제하지 않는다. 테스트도 실제 공통 명령·조회 경로를 사용한다.
- 테스트에서만 쓰는 준비 코드는 Tests에 둔다. 실제 검증 화면에서 쓰는 구성은 `Presentation/Diagnostics/`에 둘 수 있으며 정식 초기 상태와 구분한다.
- `Manager`, `Controller`, `Component`처럼 클래스 형태만으로 서로 다른 게임 기능을 같은 폴더에 모으지 않는다.

## 어셈블리와 Unity

- 디렉터리 분류와 어셈블리·namespace는 별개다. 폴더마다 `.asmdef`를 만들거나 이관만을 이유로 클래스·namespace·어셈블리 이름을 일괄 변경하지 않는다.
- 현재 Foundation·Presentation·Editor·테스트의 어셈블리 정체성과 참조를 보존하는 것을 우선한다. 필요하면 `.asmref`로 떨어진 폴더의 소속을 연결한다.
- Editor는 Editor 전용 어셈블리, 테스트는 테스트 어셈블리 설정을 유지한다. 런타임 코드가 UnityEditor·테스트 코드를 참조하지 않게 한다.
- 상위 `.asmdef` 아래에서는 Editor 폴더 이름만으로 기존 Editor 분리가 유지된다고 가정하지 않는다. 실제 소속·플랫폼 설정을 확인한다.
- 어셈블리 경계 변경이 필요하면 이유·의존 관계·검증을 PLAN과 기술 결정에 기록한다. 컴파일 경계 보존 절차는 [Unity 6.3 공식 문서](https://docs.unity3d.com/6000.3/Documentation/Manual/assembly-definitions-intro.html)를 참고한다.

## 기존 코드의 이관 기준

| 이관 전 범위 | 목적지 기준 |
| --- | --- |
| `Assets/Game/Foundation/Definitions/` | `Scripts/Data/Definitions/`와 정의별 주제 폴더 |
| `Assets/Game/Foundation/Things/` | Thing·Registry·기반 컴포넌트는 `Scripts/Core/Things/`, 자원 수량·보관 컴포넌트는 `Scripts/Core/Resource/` |
| `Assets/Game/Foundation/Spatial/` | `Scripts/Core/Spatial/` |
| `Assets/Game/Presentation/` | `Scripts/Presentation/Diagnostics/` 등 실제 기능별 분류 |
| `Assets/Game/Editor/` | `Scripts/Editor/Definitions/`, `Scripts/Editor/Scenes/` |
| `Assets/Tests/` | `Scripts/Tests/EditMode/`, `Scripts/Tests/PlayMode/` 아래 기존 검증 기능별 분류 |

목적지는 Assets 기준이다. 실제 이동 시 어셈블리 소속·내부 접근·테스트 탐색·직렬화 참조·경로 문자열을 함께 점검한다.
새 코드도 기존 위치에 추가하지 않으며, 실제 파일 이동과 검증은 승인된 PLAN 안에서 수행한다.

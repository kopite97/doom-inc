# 명명 규칙

적용: 프로젝트에서 직접 작성하는 코드·Unity 폴더·에셋의 생성과 이름 변경.
배치는 [프로젝트 구조](project-structure.md)·[스크립트 구조](script-structure.md), 상태 소유는 [아키텍처](architecture.md)를 따른다.

## 기본

- 사용자가 지정한 이름을 우선한다. 이름은 담당 기능과 역할을 드러내고 같은 개념에는 같은 용어를 쓴다.
- 타입명·C# 파일명·Unity 기능 폴더명은 영문 `PascalCase`를 사용한다. `UI`, `VFX`, `SO` 약어와 `GameFlow`, `Gameplay`, `Audio` 표기를 유지한다.
- 주요 타입과 파일명을 맞춘다. partial 파일은 `타입명.세부주제.cs`로 쓴다. 예: `LogisticsManager.Validation.cs`.
- 개발용 에셋의 파일명·에셋명·하위 Sprite/Clip·씬 오브젝트 이름도 영어로 쓴다. 문서와 게임 표시 문구에는 적용하지 않는다.
- 임시 대체 에셋의 파일·에셋·씬 오브젝트·하위 Sprite/Clip 이름에는 `Temp_`를 붙인다. 문서에는 `임의(Temp)`로 표시하며 한글 접두사·Placeholder를 섞지 않는다. 예: `Temp_Storage.png`.
- 일반 코드·확정 정의 전체에 Temp_를 붙이지 않는다. 임시 에셋의 배치·교체는 [교체 절차](project-structure.md#temporary-assets)를 따른다.
- 기존 문서의 소문자·숫자 분류, Unity 고정 경로·외부 패키지 이름은 이 규칙으로 일괄 변경하지 않는다.

## 역할별 이름

| 역할 | 이름 기준·예 |
| --- | --- |
| 시스템 규칙·여러 대상 조율 | 기능 + `Manager`: `ProductionManager`, `LogisticsManager` |
| 종류 정의 | `Definition`: `FacilityDefinition` |
| 개별 기능·상태 | `Component`, `State`: `WorkerComponent`, `SessionState` |
| 명령·결과·행동 | `Commands`, `Result`, `Job` |
| 생성·검사·저장 | `Factory`, `Validator`, `Snapshot`, `Codec`, `Store` |
| 화면·테스트 | `View`, `Tests` |

Thing·GridMap·Registry처럼 책임이 명확한 기반 이름은 유지한다. 모든 클래스에 접미사를 강제로 붙이지 않는다.

## Manager

- 한 Manager는 한 영역의 규칙 또는 조율을 맡는다. 이름만 바꾸지 않고 실제 책임도 확인한다.
- 같은 조율 역할에 `System`·`Controller`·`Simulation`을 혼용하지 않고 `Manager`로 통일한다. 다른 역할의 클래스까지 Manager로 바꾸지 않는다.
- 개별 상태의 원본은 담당 컴포넌트에 유지한다. Manager가 같은 상태를 중복 소유하거나 다른 영역의 규칙까지 관리하지 않는다.
- Manager는 일반 C# 객체로 두고 의존성을 명시적으로 연결한다. 이름만으로 싱글턴·전역 접근·MonoBehaviour를 도입하지 않는다.
- 기능별 폴더에 두며 서로 다른 기능을 `Managers/`에 모으지 않는다.

## 기존 이름 변경

- 승인된 작업 범위에서 변경 전/후 이름·경로·책임을 기록하고 호출부·partial·테스트·Editor 도구·현행 문서 참조를 함께 갱신한다.
- [Unity 에셋 지침](unity-assets.md)에 따라 `.meta`·GUID·직렬화 참조를 보존한다. 명명 변경만으로 정의 ID·저장 계약·게임 규칙을 바꾸지 않는다.
- 과거 실행 로그·결과·해시는 당시 증거로 남긴다. 컴파일·참조 로드·저장 복원·영향받은 기존 동작의 검증은 [검증 지침](verification.md)을 따른다.

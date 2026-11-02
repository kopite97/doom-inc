# TECH-012 주제별 JSON·타입 DTO와 호환 해시

- 날짜: 2026-11-02
- 근거: 사용자의 [PLAN-015](../plans/PLAN-015-topic-json-definition-data.md) 실행 승인. Editor 런타임 검증은 포함하고 빌드 검증은 제외한다.
- 대체: [TECH-009](TECH-009-csv-data-catalog.md)의 CSV 원본·파서·행 DTO·취득 계약을 대체한다. Registry ID 결합·불변 Data·영속 GameDataStore·진행 저장 보호 원칙은 유지한다.
- 상태: 구현 적용, 검증 결과·완료 판정은 PLAN에서 관리한다.

## 결정

1. `Assets/JSON/Balance/`의 5개 주제 문서를 편집 원본으로 삼는다. 각 문서는 schemaVersion/entries를 가지며 Bootstrap에는 주제 키와 TextAsset을 명시적으로 등록한다. 실행 중 폴더 검색이나 파일명에서 정의 ID를 추론하지 않는다.
2. `IGameDataSource`는 취소 가능한 비동기 원문 묶음 `GameDataBundle`을 반환한다. 로컬 TextAsset 접근은 `LocalJsonDataSource`에 한정한다. 원격 구현 없이도 가짜 지연/실패 취득부와 같은 Composer를 사용할 수 있다.
3. `JsonDefinitionParser`가 주제별 타입 DTO를 검사한다. Data 생성자는 DTO의 값을 복사하고 SO의 에셋을 연결한다. CsvRow·CSV 문자열 변환을 중간 계층으로 남기지 않는다. DTO는 입력 전용이며 카탈로그에 공개하지 않는다.
4. 기존 간접 의존성이던 `com.unity.nuget.newtonsoft-json@3.2.2`를 PackageManager Client API로 직접 등록했다. 이미 로컬에 있던 동일 버전이며 다른 파서나 패키지 업그레이드를 추가하지 않았다. JSON DOM·중복 속성·오류 위치 검사를 사용하고, 비표준 확장(주석/작은따옴표/후행 쉼표/잘못된 숫자)을 작은 어휘 검사로 거부한다. 기본값을 조용히 채우는 역직렬화만으로 검증하지 않는다.
5. 원문 숫자 토큰을 DOM 순서와 대응시켜 정확한 정규화 값을 유지한다. int/long 범위는 이 값에서 확인하고, Runtime double/float 변환 전 호환 해시에 반영한다. long 값이 double 정밀도를 넘거나 작은 차이가 float에서 사라져도 다른 입력을 같은 해시로 만들지 않는다.
6. 역할별 생략 필드는 기존 CSV의 0/빈 문자열로 복원한다. 주제/정의 ID/필드 Ordinal 정렬·길이 접두 토큰을 유지하여 현행 배포 데이터의 해시가 LegacyDataBaseline과 일치한다. 의미가 같은 숫자 표기와 서식·표시 이름 변경은 호환되며, 지원하지 않는 과거 임의 CSV 표기의 저장은 보수적으로 거부한다.
7. JSON 전체·Registry 전체를 검사한 뒤에만 카탈로그를 원자적으로 공개한다. 새 판·씬 전환은 준비된 Store를 재사용한다. 실행 상태·진행 저장·UI 설정의 소유와 스키마는 변경하지 않는다.
8. 씬 의존성 검사를 Player 스크립트 컴파일에서 분리했다. `Validate JSON Scene References`는 AssetDatabase로 저장된 씬 3개의 JSON 포함/CSV 잔여를 검사한다. Play 검사는 저장 슬롯만 격리하고 JSON/Registry를 주입하지 않은 상태에서 세 씬을 새로 로드한다.

## 대안과 제한

JsonUtility의 기본값 기반 복원만으로는 필수 누락·중복 속성·정확한 long 수치 판정을 충족하지 못한다. 독자적인 전체 JSON DOM 파서를 만드는 대신 기존 Newtonsoft 의존성과 제한된 입력 검사를 사용한다. 직렬화 에셋 참조를 유지하여 런타임 Editor API나 Resources/StreamingAssets 로딩을 도입하지 않는다.

빌드/IL2CPP·실기기 호환성은 이번 실행에서 판정하지 않는다. 빌드 도구의 JSON 경로 정비는 실제 빌드 통과와 구분한다. 전체 필드·기본값·편집 계약은 [JSON 안내](../../UnityProject/Assets/JSON/Balance/README.md), 실제 결과는 PLAN의 증거를 따른다.

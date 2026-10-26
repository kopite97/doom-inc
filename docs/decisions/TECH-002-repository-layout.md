# TECH-002 저장소 루트와 Unity LFS 범위

- 결정일: 2026-10-26
- 근거: 사용자가 현재 구조 유지, 최상위 Git 초기화, 지정 원격 연결과 LFS 설정을 요청했다.
- 관련 작업: [PLAN-005](../plans/PLAN-005-repository-setup.md).
- 적용: [루트 ignore](../../.gitignore), [Unity ignore](../../UnityProject/.gitignore), [attributes](../../.gitattributes), [저장소 지침](../agents/repository.md).

## 결정과 이유

- 루트 AGENTS.md, 기획·지침·PLAN·기술 결정, 스크립트와 Unity 소스를 하나의 Git 저장소로 관리한다.
- Unity 프로젝트 경로는 `UnityProject/`로 유지한다. 문서 이동이나 별도 중첩 저장소는 필요하지 않다.
- 공유 ignore와 Unity 전용 ignore를 분리하되, 둘 다 루트 저장소에 적용한다.
- 생성 캐시·IDE 파일·빌드 결과·서명 키는 제외하고 Assets·메타데이터·패키지 잠금·프로젝트 설정·검증 증거는 보존한다.
- LFS는 Unity 소스 바이너리 형식에 한정한다. 직렬화 텍스트와 작은 문서 증거 이미지는 일반 Git에 둔다.
- LFS 필터와 훅은 이 로컬 저장소에 설치한다. 공유할 패턴은 `.gitattributes`에 저장한다.

## 영향과 검증

기획과 구현을 같은 커밋에서 추적할 수 있고 기존 상대 링크와 Unity 경로를 유지한다.
다른 PC에서도 Git LFS 준비가 필요하다. 추후 에셋 형식이 늘면 최초 커밋 전에 해당 패턴을 검토한다.
저장소 경계, 원격 조회, ignore·attributes와 실제 추적 후보 검증은 PLAN-005에 기록한다.
게임 규칙이나 Unity 실행 설정은 변경하지 않는다.

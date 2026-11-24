# 에이전트 작업 MAP

## 시작

1. [작업 절차](docs/agents/workflow.md)와 [PLAN 관리 지침](docs/plans/AGETNS.md)을 반드시 읽는다.
2. `node scripts/validate-agent-docs.mjs`로 상태와 `active` PLAN이 최대 1개인지 확인한다. 0개는 정상 대기 상태다.
3. PLAN 실행은 승인된 `active` PLAN 범위만 진행한다. 2개 이상이면 먼저 상태를 정리하고, 0개이면 PLAN 실행을 시작하지 않는다. [독립 Sprite 작업](docs/agents/art-assets.md#sprite-requests)은 별도 절차를 따른다.
4. 아래에서 해당 작업의 지침을 읽고 [기획 MAP](docs/game-design/README.md)으로 기준 규칙을 찾는다.

2026-11-17부터 기획 방향을 재수립한다. [이전 기획](docs/game-design-deprecated/README.md)은 deprecated이며 새 작업의 기준으로 자동 적용하지 않는다.

## 작업별 지침

| 작업 | 반드시 읽을 문서 |
| --- | --- |
| 모든 작업·재개·PLAN 전환 | [작업 절차](docs/agents/workflow.md) |
| 기획 조회·작성·변경·MAP 관리 | [기획 지침](docs/agents/game-design.md) |
| 게임 개발 사전준비·분야별 산출물 준비 | [사전준비 지침 MAP](docs/agents/pre-production/README.md)의 해당 분야 |
| 새 기획의 기술 설계·기존 시스템 재사용 | [구현·재사용 지침](docs/agents/implementation.md) |
| PLAN 조회·작성·상태 관리 | [PLAN 관리 지침](docs/plans/AGETNS.md) |
| 폴더·파일·에셋 생성·배치·이동·이름 변경 | [프로젝트 구조](docs/agents/project-structure.md) |
| 코드·폴더·에셋 명명과 기존 이름 정비 | [명명 규칙](docs/agents/naming.md) |
| C#·어셈블리 생성·배치·이동 | [스크립트 구조](docs/agents/script-structure.md) |
| 객체 형태·Unity 의존성·소유와 수명·싱글톤·static 판단 | [객체 설계](docs/agents/object-design.md) |
| 초기 기능 설계·구현·기존 코드 변경의 추상화·공통화·재사용 판단 | [추상화 기준](docs/agents/abstraction.md) |
| Thing·SO·Job·Map·저장·GameFlow 설계 및 변경 | [아키텍처](docs/agents/architecture.md) |
| 밸런스 편집창 사용·JSON 항목/주제·DTO 수치 필드 추가 | [밸런스 편집과 데이터 확장](docs/agents/balance-editor.md) |
| 씬별 책임·초기화·씬 전환·Editor 직접 실행 | [씬 흐름](docs/agents/scene-flow.md) |
| 제품 텍스트·번역 키·CSV·언어 설정·Localization | [텍스트와 Localization](docs/agents/localization.md) |
| 외부 연출 에셋 선택·조합·지침 탐색 | [외부 연출 에셋 MAP](docs/agents/presentation-effects.md) |
| UI·오브젝트·효과 수치의 시간에 따른 전환 | [DOTween Pro](docs/agents/external-assets/dotween-pro.md) |
| 게임 사건의 효과음·진동·점멸·흔들림 | [Feel](docs/agents/external-assets/feel.md) |
| 글자·단어 단위 등장·퇴장·움직임 | [Text Animator](docs/agents/external-assets/text-animator.md) |
| Sprite 외곽선·색상·Shine·Dissolve 등 재질 효과 | [All In 1 Sprite Shader](docs/agents/external-assets/all-in-1-sprite-shader.md) |
| 씬·프리팹·SO·스크립트 에셋 변경 | [Unity 에셋](docs/agents/unity-assets.md) |
| 아트 요청·수령·임시 에셋·Sprite 설정·아트 검수 | [아트 지침](docs/agents/art-assets.md) |
| 명령·상태 조회·시뮬레이션·검증 시나리오 | [하네스](docs/agents/harness.md) |
| 검증 선택·빌드/고부하 테스트 승인·완료 판정 | [검증](docs/agents/verification.md) |
| Unity 환경·CLI·MCP·테스트·빌드 실행 | [Unity 도구](docs/agents/unity-tools.md) |
| Git·LFS·추적 파일·원격 저장소 설정 | [저장소 관리](docs/agents/repository.md) |

여러 작업에 해당하면 관련 지침을 함께 읽는다. 링크된 지침이 자동 로드된다고 가정하지 않는다.

## 문서의 기준 위치

| 찾는 정보 | 위치 |
| --- | --- |
| 승인된 게임 규칙 | [기획 MAP](docs/game-design/README.md)의 주제 문서 |
| 전체 단계·단계별 확인 기준 | [개발 로드맵](docs/game-design/development/roadmap.md). 개별 게임 규칙은 기획 MAP의 주제 문서 |
| 현재 작업·진행·다음 행동·검증 결과 | [PLAN MAP](docs/plans/README.md)의 유일한 active PLAN |
| 독립 Sprite 요청·수령·적용·검수 상태 | [아트 요청 절차](docs/agents/art-assets.md#sprite-requests)의 PLAN 번호별 요청 README |
| 기술 선택·이유·대체 관계 | [기술 결정 MAP](docs/decisions/README.md) |
| 앞으로 정할 기획과 후속 작업 요약 | [TODO](docs/todo/todo.md) |

루트는 MAP으로 유지한다. 줄 수·분리·갱신 규칙은 [작업 절차](docs/agents/workflow.md)를 따른다.

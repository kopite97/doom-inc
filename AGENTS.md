# 에이전트 작업 MAP

## 시작

1. [작업 절차](docs/agents/workflow.md)와 [PLAN 관리 지침](docs/plans/AGETNS.md)을 반드시 읽는다.
2. `node scripts/validate-agent-docs.mjs`로 상태와 유일한 `active` PLAN을 확인한다.
3. 해당 PLAN을 읽고 그 범위만 작업한다. `active`가 정확히 1개가 아니면 먼저 상태를 정리한다.
4. 아래에서 해당 작업의 지침을 읽고 [기획 MAP](docs/game-design/README.md)으로 기준 규칙을 찾는다.

## 작업별 지침

| 작업 | 반드시 읽을 문서 |
| --- | --- |
| 모든 작업·재개·PLAN 전환 | [작업 절차](docs/agents/workflow.md) |
| PLAN 조회·작성·상태 관리 | [PLAN 관리 지침](docs/plans/AGETNS.md) |
| Thing·SO·Job·Map·저장·GameFlow 설계 및 변경 | [아키텍처](docs/agents/architecture.md) |
| 씬·프리팹·SO·스크립트 에셋 변경 | [Unity 에셋](docs/agents/unity-assets.md) |
| 명령·상태 조회·시뮬레이션·검증 시나리오 | [하네스](docs/agents/harness.md) |
| 변경 검증·완료 판정 | [검증](docs/agents/verification.md) |
| Unity 환경·CLI·MCP·테스트·빌드 실행 | [Unity 도구](docs/agents/unity-tools.md) |
| Git·LFS·추적 파일·원격 저장소 설정 | [저장소 관리](docs/agents/repository.md) |

여러 작업에 해당하면 관련 지침을 함께 읽는다. 링크된 지침이 자동 로드된다고 가정하지 않는다.

## 문서의 기준 위치

| 찾는 정보 | 위치 |
| --- | --- |
| 승인된 게임 규칙 | [기획 MAP](docs/game-design/README.md)의 주제 문서 |
| 전체 단계·기획상 완료 기준 | [구현 로드맵](docs/game-design/05-development/02-implementation/roadmap.md) |
| 현재 작업·진행·다음 행동·검증 결과 | [PLAN MAP](docs/plans/README.md)의 유일한 active PLAN |
| 기술 선택·이유·대체 관계 | [기술 결정 MAP](docs/decisions/README.md) |

루트는 MAP으로 유지한다. 줄 수·분리·갱신 규칙은 [작업 절차](docs/agents/workflow.md)를 따른다.

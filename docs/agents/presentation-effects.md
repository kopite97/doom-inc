# 외부 연출 에셋 MAP

적용: UI·오브젝트 전환, 게임 사건 피드백, 글자 연출, Sprite 셰이더 효과.
탐색: [루트 MAP](../../AGENTS.md), [작업 절차](workflow.md), [검증](verification.md).

## 에셋 선택과 필수 지침

작업에 해당하는 에셋 문서를 반드시 읽는다. 여러 에셋을 조합하면 각 문서를 함께 읽는다.
아래는 선택 기준이며, 설정·API·수명·에셋별 확인 항목은 연결된 문서 한 곳에서 관리한다.

| 에셋 지침 | 선택 기준 |
| --- | --- |
| [DOTween Pro](external-assets/dotween-pro.md) | UI·오브젝트의 위치·크기·투명도나 효과 수치를 목표 상태로 전환 |
| [Feel](external-assets/feel.md) | 게임 사건에 반응하는 효과음·진동·점멸·흔들림 등 피드백 묶음 |
| [Text Animator](external-assets/text-animator.md) | 글자·단어 단위 등장·퇴장·움직임 |
| [All In 1 Sprite Shader](external-assets/all-in-1-sprite-shader.md) | Sprite 투명도 경계의 외곽선, 색상·Shine·Dissolve·Hologram 등 재질 효과 |

## 관련 지침

| 찾는 기준 | 담당 문서 |
| --- | --- |
| 외부 원본 복원·Git 추적 | [저장소 관리](repository.md) |
| 프로젝트 소유 코드·재질 배치 | [프로젝트 구조](project-structure.md) |
| 제품 문구·번역 | [Localization](localization.md) |
| Sprite 원본·임포트·시각 검수 | [아트 지침](art-assets.md) |
| 실행 범위·완료 판정 | [검증](verification.md) |

## MAP 유지

- 에셋별 지침은 `docs/agents/external-assets/` 아래에서 에셋마다 독립 문서로 관리한다. 이 문서는 현재 위치에서 탐색과 선택 기준만 유지한다.
- 에셋을 추가·교체하면 이 표와 루트 MAP의 읽기 조건을 갱신한다. 사용법·설정·주의사항은 해당 에셋 지침에서 수정한다.

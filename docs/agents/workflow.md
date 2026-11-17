# 작업 절차

적용: 모든 작업의 시작·재개·종료, PLAN 상태 전환, 문서 변경.
탐색: [루트 MAP](../../AGENTS.md), [PLAN MAP](../plans/README.md), [기술 결정](../decisions/README.md).

## 시작과 재개

1. 사용자 요청과 기존 변경을 확인한다. 이전 에이전트의 작업을 덮어쓰지 않는다.
2. 저장소 루트에서 `node scripts/validate-agent-docs.mjs`를 실행한다.
3. PLAN 작업이면 출력된 유일한 active PLAN의 타입·목표·범위·현재 상황·다음 행동·검증 결과를 읽는다.
4. PLAN 작업이면 [새 기획 MAP](../game-design/README.md)과 해당 PLAN의 기준 기획을 확인한다. 새 로드맵이 작성된 경우 해당 단계도 확인하며, deprecated 로드맵은 자동 적용하지 않는다.
5. [루트 MAP](../../AGENTS.md)에서 작업에 해당하는 지침을 추가로 읽은 뒤 작업한다.
6. 관련 Sprite 요청이 있으면 [아트 요청 절차](art-assets.md#sprite-requests)에 따라 Require의 README와 수령 파일을 확인한다. 독립 아트 작업과 별도 art PLAN 작업의 경계를 따른다.

검사기를 실행할 수 없으면 PLAN 메타데이터를 직접 확인하고 실행 불가 이유를 기록한다.
검사기 실행 불가를 통과로 기록하지 않는다. 상태가 불명확한 채로 구현하지 않는다.

## active 최대 1개 규칙

- PLAN으로 관리하는 작업에 적용한다. [독립 Sprite 작업](art-assets.md#sprite-requests)은 새 PLAN 생성·활성화 없이 진행하고 요청 README에 기록한다.
- active가 0개인 정상 대기 상태에서도 사용자 요청에 따른 문서·작업 관리·독립 Sprite 작업은 진행할 수 있다. 검사 오류가 있으면 확인·수정하고 전체 검사 통과로 기록하지 않는다.
- PLAN 상태는 `proposed`, `active`, `completed`, `rejected`, `on hold`만 사용한다.
- 모든 새 PLAN은 `proposed`로 시작하며, 사용자의 명시적 승인 없이 `active`로 전환하지 않는다. 상세 기준은 [PLAN 관리 지침](../plans/AGETNS.md)을 따른다.
- 실제 PLAN 전체에서 `status: active`는 최대 1개만 허용한다. 0개는 정상 대기 상태이며 2개 이상은 오류다. 대소문자·공백을 임의로 바꾸지 않는다.
- active PLAN만 실행한다. 다른 PLAN의 구현·테스트·설치 작업을 병행하지 않는다.
- 다른 개선점은 후보 PLAN이나 현재 PLAN의 후속 항목에 기록한다. 범위에 조용히 추가하지 않는다.
- 후보 작성과 상태 전환은 작업 관리다. 후보를 작성했다는 이유로 구현에 착수하지 않는다.
- active는 현재 작업 대상을 뜻한다. 새 작업 권한이나 이미 실행했다는 증거가 아니다.
- 서브에이전트를 쓰면 같은 active PLAN 안의 담당 범위를 나눈다. 별도 PLAN을 활성화하지 않는다.
- active가 0개이면 PLAN 실행을 시작하지 않고 승인된 대상이 생길 때까지 대기한다. 2개 이상이면 PLAN 실행을 멈추고 마지막 검증 결과·사용자 요청을 근거로 상태를 복구한다.
- 대상이 불명확하면 사용자에게 작업 대상을 확인한다. 검사 통과만을 위해 임의 PLAN을 활성화하지 않는다.

## 상태 전환

| 전환 | 조건과 기록 |
| --- | --- |
| proposed → active | 사용자가 해당 PLAN 실행·활성화를 명시적으로 승인했고 선행 조건을 충족함. 승인 근거를 기록함 |
| active → completed | [타입별 완료 규칙](../plans/AGETNS.md)에 따라 해당 PLAN의 작업·완료 기준·필수 검증을 충족하고 증거를 남김 |
| active → on hold | 진행을 보류하며 중단 지점·이유·재개 조건을 기록함 |
| on hold → active | 재개 조건을 충족하고 사용자의 해당 PLAN 재개에 대한 명시적 승인 근거가 있음 |
| proposed / active / on hold → rejected | 채택하지 않거나 취소한 근거와 남은 변경을 기록함 |

- 완료·보류·취소 후 다음 PLAN 활성화는 사용자 승인이 있을 때만 함께 처리하고 직후 검사한다. 승인이 없으면 다음 PLAN을 활성화하지 않는다.
- 완료·보류·취소 후 active가 0개인 대기 상태를 유지할 수 있다. 다음 PLAN을 활성화할 때도 두 개가 동시에 active가 되지 않게 한다.
- 다음 대상이 정해지지 않았다면 전환에 필요한 선택을 먼저 확인한다. 완료하지 않은 일을 completed로 만들지 않는다.
- completed·rejected PLAN의 후속 기능 작업은 새 PLAN으로 만든다. 독립 Sprite 작업은 아트 요청 절차를 따른다. 과거 증거·전환 이유는 보존한다.
- 막힌 점을 해결하는 동안 같은 범위의 독립 작업이 가능하면 active를 유지하고 막힌 내용을 본문에 기록한다.

## 기록의 역할

| 위치 | 기록할 내용 |
| --- | --- |
| docs/game-design | 새 게임의 개요·경험·규칙·상호작용과 별도 개발 순서. 확정·제안·미정 구분 |
| docs/game-design-deprecated | 이전 기획의 원문과 승인 이력 보관. 새 작업의 규칙으로 자동 적용하지 않음 |
| docs/agents | 작업 방법·필수 참조·구현과 재사용 지침·검증 절차 |
| docs/plans | 타입별 PLAN의 진행·다음 행동·막힌 점·실행한 검증과 증거 |
| UnityProject/Assets/Sprites/Require/PLAN-NNN/README.md | Sprite 요청 명세·수령·적용·사용자 확인·검증 기록 |
| docs/decisions | 영향이 큰 기술 선택·이유·대안·적용 위치·대체 관계 |

## 기획 기록

[기획 지침](game-design.md)에 따라 확정·제안·미정·개발 위임을 구분하고 기록한다.
기획 README는 간단한 개요와 MAP만 유지하며, 작업 순서는 [개발 로드맵](../game-design/development/roadmap.md), 실제 진행은 PLAN에 기록한다.

## 진행 갱신과 종료

1. PLAN 작업은 의미 있는 작업·검증 후 PLAN의 체크리스트·현재 상황·다음 행동을 갱신한다. 독립 Sprite 작업은 요청 README를 갱신한다.
2. [검증 지침](verification.md)에 따라 실제 결과와 증거를 남긴다. 로그 전문은 PLAN에 반복하지 않는다.
3. 작은 구현 선택은 PLAN에, 독립 Sprite 적용 선택은 요청 README에, 중요한 기술 선택은 [기술 결정](../decisions/README.md)에 기록한다.
4. 새 기획의 로드맵이 있고 전체 단계의 완료 여부가 달라지면 요약과 증거 PLAN 링크를 반영한다. deprecated 기획과 완료 PLAN의 원문은 유지한다.
5. 신규·교체 Sprite가 필요한지 확인하고, 있으면 [아트 요청 절차](art-assets.md#sprite-requests)에 따라 `UnityProject/Assets/Sprites/Require/PLAN-NNN/README.md`를 작성·갱신해 PLAN에서 연결한다. 요청이 없으면 폴더를 만들지 않는다.
6. 상태 전환·문서 변경 후 검사기를 다시 실행한다. 실행 범위·결과·남은 일과 Sprite 요청 유무·요청 README 위치를 사용자에게 보고한다.

## 문서 유지

- 루트 AGENTS.md는 빈 줄·표·코드 블록 포함 100줄 이내, docs/agents의 각 Markdown은 150줄 이내다.
- 다른 주제가 들어가거나 제한에 가까워지면 주제별 문서로 분리하고 루트 MAP에 읽기 조건과 링크를 추가한다.
- 긴 문장을 한 줄로 합쳐 제한을 우회하지 않는다. 한 규칙은 한 곳에 쓰고 다른 곳에서는 링크한다.
- 설치 경로·명령·패키지 구성은 확인된 사실만 기록한다. 미구현 경로나 예시 명령을 실행 완료로 표현하지 않는다.
- PLAN·기술 결정 ID는 고유하게 유지한다. 타입·상태의 원본은 PLAN 상단 필드이며 README의 type·status 요약도 함께 갱신하고 검사한다.

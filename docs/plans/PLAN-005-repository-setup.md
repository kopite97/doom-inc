---
id: PLAN-005
type: development
status: completed
title: 루트 Git 저장소와 LFS 설정
updated: 2026-10-26
---

# PLAN-005 루트 Git 저장소와 LFS 설정

## 목표와 범위

기존 디렉터리 구조를 유지하고 루트 AGENTS.md가 있는 폴더에서 기획·지침·Unity 소스를 함께 버전 관리한다.

- 사용자 요청: 최상위 Git 초기화, 생성 파일 제외, `https://github.com/kopite97/doom-inc.git` 연결, Git LFS 사전 설정.
- 포함: 로컬 저장소·origin·ignore·attributes·저장소 작업 지침과 검증.
- 제외: 게임 기능 구현, 기존 파일 이동, 최초 커밋·원격 push, 전역 Git 설정 변경.
- 선행: [PLAN-002](PLAN-002-unity-environment.md) 환경과 [PLAN-003](PLAN-003-thing-foundation.md) 소스를 보존한다.
- 지침: [작업 절차](../agents/workflow.md), [에셋](../agents/unity-assets.md), [검증](../agents/verification.md).

## 작업 목록

- [x] 루트 저장소 초기화와 origin 연결
- [x] 생성 파일 제외 및 Unity 소스·메타데이터 포함 확인
- [x] 로컬 Git LFS 설치와 바이너리 에셋 속성 설정
- [x] 설정·원격 조회·파일 분류 검증 및 증거 기록
- [x] 완료 후 PLAN-004를 유일한 active로 복원

## 완료 기준과 검증 방법

| 기준 | 실행할 검증 |
| --- | --- |
| 저장소 경계 | 루트와 UnityProject의 `git rev-parse --show-toplevel`이 동일 |
| 원격 주소 | `git remote get-url origin`; `git ls-remote origin` 읽기 조회 |
| 파일 범위 | `git ls-files --others --exclude-standard`와 `git check-ignore`로 소스 포함·캐시 제외 |
| LFS | 버전·로컬 필터·pre-push 훅·`git check-attr`의 바이너리/텍스트 분류 |
| 문서 일관성 | 에이전트 문서 검사기 통과와 정확히 하나의 active PLAN |

## 현재 상황과 다음 행동

- 완료: 루트에 `main` 브랜치의 저장소를 초기화하고 지정 주소를 origin으로 설정했다. 원격 읽기 조회에 성공했고 당시 공개된 ref는 없었다.
- 완료: 로컬 LFS 필터와 pre-push 훅, 공용 ignore·Unity ignore·바이너리 attributes, [저장소 지침](../agents/repository.md)을 적용했다.
- 현재 Assets에는 LFS 대상 바이너리가 없다. 향후 해당 형식이 추가되면 규칙이 적용된다.
- 미실행: 최초 커밋·push·LFS 서버 업로드/다운로드. 사용자 파일은 스테이징하지 않았다.
- 다음: [PLAN-004](PLAN-004-map-and-commands.md)를 유일한 active로 복원했다. 공간 기능 구현은 여전히 미착수다.

## 검증 결과

| 날짜 | 검증 | 실제 결과·증거 |
| --- | --- | --- |
| 2026-10-26 | 저장소·원격·ignore·attributes·소스와 메타데이터 | 67개 확인 항목 통과. [검증 JSON](evidence/PLAN-005/2026-10-26-repository-verification.json) |
| 2026-10-26 | LFS 로컬 clean/smudge | 기존 증거 PNG 8,024바이트를 stdin으로 변환·복원하여 SHA-256과 원본 바이트 일치. 검증용 에셋 파일은 만들지 않고 새 임시 LFS 객체는 제거함 |
| 2026-10-26 | 독립 읽기 전용 검토 | 소스·증거·플러그인 포함, 생성 파일 제외, 혼합 대소문자 확장자 LFS 적용 확인. 수정 필요 사항 없음 |
| 2026-10-26 | 문서 검사 | 링크·줄 수·PLAN 상태 일치 통과. [검사 로그](evidence/PLAN-005/2026-10-26-doc-validation.log) |

검증 JSON을 쓰기 전 추적 후보는 238개, 총 5,270,084바이트였다. 이후 이 PLAN의 증거가 추가된다.
최대 파일은 약 3.94 MB의 텍스트 씬 템플릿이며, 기존 캐시와 생성 IDE 파일은 후보에서 제외됐다.
검증 증거 `.log` 9개를 보존하고 자산 파일·폴더의 `.meta` 누락이 없음을 확인했다.
LFS 원격 전송과 Unity 실행 검증은 이번 저장소 설정에서 수행하지 않았다.

주요 확인 명령:

```text
git rev-parse --show-toplevel
git -C UnityProject rev-parse --show-toplevel
git remote get-url origin
git ls-remote --symref origin
git ls-files --others --exclude-standard
git status --short --ignored
git check-ignore -v -- UnityProject/Library/test.bin
git check-attr filter diff merge text -- UnityProject/Assets/example.png
git lfs track
git lfs ls-files
node scripts/validate-agent-docs.mjs
```

## 상태 전환

- 2026-10-26: 사용자 저장소 설정 요청에 따라 active 생성. PLAN-004를 함께 on hold로 전환.
- 2026-10-26: 설정과 검증 완료로 active → completed. PLAN-004를 on hold → active로 복원.
- 기술 결정: [TECH-002](../decisions/TECH-002-repository-layout.md).

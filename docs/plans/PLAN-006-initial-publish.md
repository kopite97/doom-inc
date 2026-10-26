---
id: PLAN-006
type: development
status: completed
title: 기획·Unity 소스 최초 커밋과 GitHub 업로드
updated: 2026-10-26
---

# PLAN-006 기획·Unity 소스 최초 커밋과 GitHub 업로드

## 목표와 범위

사용자 요청에 따라 현재 기획·지침·검증 증거와 Unity 소스를 최초 커밋하고 지정한 GitHub 저장소의 main에 업로드한다.

- 포함: 실제 스테이징 목록·제외 파일·인증 정보 혼입 확인, 최초 커밋, origin/main push와 원격 일치 확인.
- 제외: 게임 기능 변경, Unity 재실행·재빌드, 기존 원격 이력 덮어쓰기.
- 기준: [PLAN-005](PLAN-005-repository-setup.md), [저장소 지침](../agents/repository.md), [작업 절차](../agents/workflow.md), [검증](../agents/verification.md).
- 대상: `https://github.com/kopite97/doom-inc.git`, `main` 브랜치.

## 작업 목록과 완료 기준

- [x] 실제 커밋 대상에 기획·Unity 소스·메타데이터가 포함되고 생성 파일이 제외되는지 확인
- [x] 설정·증거에서 인증 정보 패턴과 대용량 파일을 점검
- [x] 문서 검사 후 최초 커밋 생성
- [x] 강제 push 없이 main 업로드 및 원격 SHA 일치 확인
- [x] 완료 기록 작성 및 PLAN-004를 유일한 active로 복원

## 현재 상황과 다음 행동

- 완료: 최초 커밋 `b8edfd990cd45ddcb9742ea7e202d3d26f1629d3`에 243개 파일을 포함해 origin/main에 업로드했다.
- 원격 main과 로컬 SHA 일치를 확인했고 main은 origin/main을 추적한다. 최초 업로드 직후 작업 트리는 깨끗했다.
- 실제 스테이징된 241개 파일(검증 기록 추가 전)에 생성 파일·인증 정보 패턴·소스 메타데이터 누락이 없었다.
- 이번 업로드 결과와 상태 전환 기록은 별도 일반 커밋으로 반영한다. 최초 커밋을 재작성하거나 강제 push하지 않는다.
- 다음 개발 대상은 [PLAN-004](PLAN-004-map-and-commands.md)이며 공간 기능 구현은 미착수다.

## 검증 결과

- [스테이징 검사](evidence/PLAN-006/2026-10-26-staged-audit.json): 인덱스의 실제 blob을 검사했다. 예상 밖 경로·캐시/빌드·메타데이터 누락·고확률 비밀 값 패턴 및 민감 JSON 값 발견 0건.
- 검사 시점의 총 blob 크기 5,282,484바이트. 최대 파일은 약 3.94 MB의 텍스트 씬 템플릿이며 검증 증거 로그 10개를 보존했다.
- 현재 LFS 대상 바이너리 에셋이 없어 LFS 추적 목록은 비어 있다. `.gitattributes`와 필터·훅 설정은 유지한다.
- [문서 검사](evidence/PLAN-006/2026-10-26-doc-validation.log): 링크·줄 수·상태 검증 결과를 보관한다.
- 이번 작업에서 Unity 테스트·컴파일·빌드는 재실행하지 않는다. 비밀 값 패턴 검색은 전수 보안 보증이 아니다.
- [최초 업로드 증거](evidence/PLAN-006/2026-10-26-initial-push.json): push 성공, 원격 SHA 일치, upstream 설정과 업로드 직후 작업 트리 상태 확인.

## 상태 전환

- 2026-10-26: 사용자의 최초 커밋·GitHub 업로드 요청으로 active 생성. PLAN-004를 on hold로 전환.
- 2026-10-26: 최초 커밋·업로드와 원격 일치 검증 후 active → completed. PLAN-004를 on hold → active로 복원.

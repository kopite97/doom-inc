# 저장소 관리 지침

Git 루트는 AGENTS.md가 있는 최상위 폴더이고 Unity에서 여는 프로젝트는 `UnityProject/`다.
[작업 절차](workflow.md)에 따라 유일한 active PLAN과 사용자 요청 범위에서 작업한다. [독립 Sprite 작업](art-assets.md#sprite-requests)의 저장소 확인·사용자가 요청한 커밋도 해당 독립 범위를 따른다.
설정 근거는 [TECH-002](../decisions/TECH-002-repository-layout.md), 최초 검증은 [PLAN-005](../plans/PLAN-005-repository-setup.md)를 따른다.

## 경계와 추적 대상

- `docs/`, `scripts/`, Unity 소스를 하나의 저장소에서 관리한다. 중첩 저장소를 만들지 않는다.
- Unity `Assets/`와 파일·폴더의 `.meta`, `Packages/`, `ProjectSettings/`, `.vsconfig`를 함께 관리한다.
- 캐시·빌드·로컬 설정은 제외한다. [루트 ignore](../../.gitignore)와 [Unity ignore](../../UnityProject/.gitignore)는 함께 적용된다.
- `docs/plans/evidence/`의 검토된 로그·결과는 추적한다. 모든 `.log`를 무조건 제외하지 않는다.
- ignore 변경 후 소스·메타데이터·증거가 실수로 제외되지 않는지 확인한다.
- ignore는 비밀 값의 내용을 검사하지 않는다. 커밋 전 설정·로그에 인증 정보가 없는지 변경 내용을 검토한다.

## LFS와 텍스트

- LFS 대상은 [attributes](../../.gitattributes)에 정한 `UnityProject/Assets/`의 이미지·음원·영상·모델·폰트 등 바이너리다.
- C#·문서·씬·프리팹·SO·`.meta` 등 텍스트는 일반 Git으로 관리한다. 폴더 전체나 `*.asset` 전체를 LFS로 돌리지 않는다.
- LFS 대상 추가·변경은 파일을 처음 커밋하기 전에 적용한다. `.gitattributes`도 함께 커밋한다.
- 새 개발 환경은 Git LFS를 설치하고 복제한 저장소에서 `git lfs install --local`을 실행한다. 필요한 경우 `git lfs pull`로 원본을 받는다.
- 포인터만 있는 에셋을 Unity 원본으로 취급하지 않는다. 바이너리 원본 다운로드 여부를 확인한다.
- 현재 없는 파일 형식은 필요할 때 패턴을 추가한다. 플러그인 `.dll`·`.so`를 전역 ignore하지 않는다.
- 전역 Git 설정·LFS 파일 잠금·이력 재작성은 이번 기본 설정에 포함하지 않는다.

## 확인 명령

저장소 루트에서 실행한다.

```text
git rev-parse --show-toplevel
git remote -v
git status --short --ignored
git ls-files --others --exclude-standard
git check-ignore -v -- <확인할 파일>
git check-attr filter diff merge text -- <확인할 파일>
git lfs track
git lfs ls-files
```

`git lfs ls-files`는 커밋·스테이징된 파일 기준이다. 패턴만 준비했거나 해당 에셋이 없으면 비어 있는 것이 정상이다.
커밋 전에 `git diff --cached --stat`과 `git diff --cached`로 실제 포함 파일을 확인한다.
원격 주소는 `origin = https://github.com/kopite97/doom-inc.git`이다. 초기 설정과 실제 커밋·push 결과를 구분해 보고한다.

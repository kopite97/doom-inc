# 환경과 보존

2026-11-16 · Unity 6000.3.27f1 / Android 대상 / Unity CLI 1.0.0.

- 시작 시 Unity 미실행, Aseprite 실행 중이었다. Unity를 열어 실제 임포트·연결·검증을 수행했다. 끝에는 사용자 확인을 위해 Editor를 열어 두었으며 Play Mode는 정지했다. 시작의 미실행 상태와 동일하다고 주장하지 않는다.
- 마지막 씬은 Temp_Laboratory, dirty false, Run In Background false다. [최종 Editor](runs/run-003/editor-final.json), [환경·동적 폰트 정리](runs/run-003/environment-final.json).
- 테스트는 기존 격리 저장 절차를 이용했다. 제품 저장 파일에 새 진행을 저장하지 않았다. ProjectSettings·씬·프로토타입 PNG/meta/SO의 Git 변경 없음.
- 테스트로 추가된 MulmaruDynamic 글리프 209개를 TMP ClearFontAssetData(true)로 정리했다. 글리프/문자 0, 해당 에셋 Git diff 0이다.
- Art 작업 원본을 r002 보존본으로 교체하기 전 디스크 파일을 `C:/Dev/Unity/Art/docs/plans/reviews/PLAN-009/r002/Support-before-sync.aseprite`에 보존했다. 열린 Aseprite 탭의 메모리 내용은 변경하지 않았다. 같은 파일의 이전 탭은 새 디스크 원본을 다시 열어 확인해야 한다.
- 수령 PNG는 AssetDatabase.MoveAsset으로 최종 경로로 이동했고 GUID를 유지했다. Sprite Editor 데이터 제공자로 64셀을 구성했다. 기존 프로토타입의 직렬화 필드·하위 Sprite ID는 보존했다.

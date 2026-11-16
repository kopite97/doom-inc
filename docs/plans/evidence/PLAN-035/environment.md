# 환경과 기존 에셋 보존

2026-11-16 · [시작](environment-before.json) · [종료](environment-final.json).

- Unity6000.3.27f1 Android 대상 Editor를 기존 실행 인스턴스에서 사용했다. 시작/종료 모두 Temp_Laboratory clean, Play 정지, Domain Reload OFF, Run In Background false다. Player 빌드·실기기·고부하는 실행하지 않았다.
- 테스트는 기존 PointerScene의 격리 저장·설정 및 정리 절차를 사용했다. 새 판을 제품 저장 슬롯에 덮어쓰지 않았다.
- 테스트의 동적 폰트 캐시는 TMP ClearFontAssetData(true)로 정리했다. 글리프0, 폰트 에셋 Git diff0. ProjectSettings·씬도 Git diff0이다.
- [기존 에셋/설정 해시](assets-before.json)와 [변경 목록](assets-changed.json)을 대조했다. 예상된 LaboratoryArt의 배양액 받침, CultureMedium SO Icon, Localization 세 테이블만 변경됐다. 기존 승인 Sprite/meta·프로토타입·보급형 정의는 작업 시작 값과 일치한다.
- [최종 Console](console-final.json)의 현재 groundTruth는 compilationFailed false, Console 오류/경고0이다. 누적 세션 로그에는 과거 및 준비 중 테스트 코드 namespace 오류가 남아 있으며 현재 실패와 구분한다.
- Art는 새 CultureMedium 원본만 제작했다. 기존 승인 원본·열린 Aseprite 메모리 탭을 수정하지 않았다. 작업/보존 원본과 PNG 체크섬은 [실제 파일 목록](asset-manifest.csv)을 따른다.

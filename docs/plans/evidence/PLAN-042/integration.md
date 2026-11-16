# PLAN-042 일괄 연결

2026-11-17. WorldDestruction r002 사용자 확인 완료. PLAN-042 유일 active.

- 원본/PNG: 21개 Aseprite 보존 원본의 모든 출력 픽셀 일치(불일치 0), 프레임/태그/시간은 source-validation.json. 수령 PNG 해시 21개 일치.
- 원래 Require 요청 경로에서 Import한 뒤 AssetDatabase.MoveAsset으로 최종 경로 이동. Sprite Data Provider capability 검사, 이름별 Sprite ID 보존. PPU32/Point/Clamp/FullRect/무압축/밉맵 OFF, 큰 타이틀 4096 상한.
- 침공형·약탈형: 정의별 40프레임/4방향/시간, SO Icon 및 목록 참조. 비Worker null 경계 수정. 이동 AI는 추가하지 않음.
- 외곽: 논리 지도 바깥 8변형, 바닥 위/본체 아래, 점유/Collider/Raycast 없음, 전체 보기 여백 반영.
- 메뉴6개/공용 프레임6개: AppConfiguration/연구소 씬, 화면 소유 UiSkinContext. DestructionFill은 기존 Image RGB(.85,.3,.2) 재사용. ScrollThumb은 기존 ScrollRect를 방해하지 않는 비조작 위치 표시. UiSkinAssets는 Data/Presentation에 배치.
- 탄생/방출: 성공 후 사건·실제 위치, 화면/세션 소유 8슬롯, DOTween 핸들만 관리, 시뮬레이션 시간 샘플, 복원 시 과거 사건 재생 없음. 기존 프로젝트 LaboratoryWorld 재질을 공유하므로 별도 재질/프리팹 중복 없음.
- 타이틀: 16프레임/150ms/Contain/가로·세로, 글자는 TMP. 로딩: 16프레임/150ms/진행 표시 위 장식, 부족한 공간은 숨김. 씬/프리팹/생성 도구 참조 갱신.
- 점령: Ending.Progress를 32프레임에 매핑, 결과에서 마지막 프레임 고정. 결과 프레임은 PanelFrame 재사용. 한국어/영어7문구 실제 검토·CSV Import·테이블 일치 확인.

## 전체 준비 판정

2026-11-17 00:51 KST: 모든 필수 대상 수령·임포트·표시 연결 완료. 컴파일 오류 없음, 정적 참조 결과 static-bindings.json. 이 시점 이후 통합 Edit/Play/실제 화면 검증 시작. 게임 화면 사용자 승인은 아직 미확인.

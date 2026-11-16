# 통합 검증 결과

2026-11-17. Editor 기술 검증 완료, 사용자 실제 게임 화면 최종 확인 완료.

- 원본21개: Aseprite 보존 원본과 PNG의 모든 픽셀 일치(불일치0). 프레임·태그·시간·알파·셀 배열·불투명 경계 기록. 효과2개 흰색 마스크 RGB 일치. [원본 검사](source-validation.json), [경계](source-bounds.json).
- 수령21개 PNG 해시 승인본과 일치. 22행 대응표(Fill 재사용 포함), 하위 Sprite/피벗/PPU32/Point/FullRect·플랫폼 override·타이틀2560px 유지. [해시](received-hashes.json), [manifest](asset-manifest.csv).
- Edit Mode 464/464 통과. 최종 문구 변경 후 Localization13/13 재검증.
- Play Mode 고유100개 항목 최종 통과. 최초99개 중3실패를 숨기지 않고 run-001 XML에 보존. 후속 run-002~005에서 실패/변경 영향만 재검증했다. [최종 집계](final-test-summary.json).
- 마지막 run-005:6/6 통과. 탄생 성공/방출 성공·실패·8슬롯 상한·시간정지·복원/새 판, 경계 레이어, NonWorker Idle, 타이틀/로딩 반복, 가로/세로 종료·결과·저장 오류 복구·대체 아트·UI프레임을 확인.
- 기존 배치 드래그·충돌/이유·빈 땅 해제·외곽선·작은 상태바·시설4방향/생산·운반·보급형/배양액·ko/en·큰 글씨·입력·씬 왕복은 run-001 일반 회귀에 포함.
- 에이전트가 실제 GameView 캡처를 확인해 타이틀 큰 글씨와 스크롤 표시 위치를 보정. 사용자 디자인/동작 승인을 대신하지 않는다.

## 범위와 남은 확인

침공형·약탈형 4방향 Idle/Move40프레임은 분할·재생 계약에 연결했다. 현재 비Worker 이동 AI가 없으므로 실제 게임에서는 Down Idle이 재생된다. 새 이동 AI/작업/운반 동작은 추가하지 않았다.

일반 Editor 검증이며 Android/Player/Addressables 콘텐츠 빌드·실기기 성능·고부하 계측은 미실행. 기존 승인 없는 성능 계측1건도 Explicit/MovementPerformance로 분리하여 자동 실행에서 제외했다.

새로 남은 필수 기술 실패는0건. 2026-11-17 사용자의 적용 화면 확인으로 Unity034·036~041/Art011~031과 통합042를 completed로 전환했다. [승인](completion.md).

## 문서·변경 형식 검사

Unity 문서 검사 통과:43 PLAN·active1(PLAN-042)·로컬 링크4245개. Art 문서는31 PLAN·active0이며 기존 완료 PLAN의 수령 경로 링크 오류9건만 유지한다(사용자 완료 문서 수정 제외 지시). 신규 링크 오류0건. Git diff --check 통과. 밸런스 JSON/ProjectSettings diff 없음.

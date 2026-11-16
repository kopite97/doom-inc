# 확인된 문제와 처리

| 항목 | 원인·처리 | 증거 |
| --- | --- | --- |
| 비Worker 표시 예외 가능 | Worker를 가정한 표시 경계를 null-safe하게 수정, 정의별 프레임/시간 사용 | BatchArtNonWorkers… 최종 통과 |
| 첫 Play3실패 | 종료 문구 하드코딩/Support를 미제작으로 가정한 fallback fixture/화면 복원 직후 focus 조건 | run-001 원본 보존,002~003 해결 |
| 탄생 대기 중 게임 종료 | 테스트 선행 방출 개체가 배양 전 목표 달성. 독립 새 판으로 분리 | run-003 1/1 |
| 타이틀 큰 글씨 과대 | 옛 wordmark 대체 TMP 크기가 배경 저대비 영역을 넘음. 제목 영역/최대 크기 보정 | run-002 이후 양방향 캡처 |
| ScrollThumb 본문 침범 | 최초 배치 전 폭에서 위치가 계산됨. OnRectTransformDimensionsChange 갱신 및 우측 정렬 검증 | run-005 6/6·최종 화면 |
| 설정 디스크 잔류 | 테스트 runInBackground 메모리 복원과 디스크 불일치. 작업 전 해시와 HEAD 원문 일치를 확인하고 유일한 테스트 변경만 원복 | preservation-check.json |

2026-11-17 사용자 게임 화면 최종 확인 완료. 신규 미해결 기술 결함 없음. 과거 PLAN-025 성능 한계는 이번 검증으로 해결/통과했다고 간주하지 않는다. Art 완료 문서의 기존 링크9건은 사용자 지시에 따라 그대로 둔다.

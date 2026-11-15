# 다섯 보완사항 검증

| 항목 | 실제 확인 |
| --- | --- |
| 편집 제외 값과의 교차 오류 | CrossFieldErrorIdentifiesReadOnlyReportThreshold와 실제 창에서 targetScore=10 / detailedUntilScore=60 저장 거부·두 값/원인 표시·원본 불변. 기존 InvasionSettings 검증 메시지만 보완했으며 조건은 동일 |
| 공통 등록과 실제 실행 연결 | RuntimeBindingMismatchIsDetectedWithoutChangingConfiguration의 파일 교체·누락·주제 중복 3종 차단. 실제 Configuration은 그대로 두고 복제본만 검사. 정상 카탈로그/다음 Play 적용 통과. 새 주제의 게임 로직 구현까지 자동화한다고 주장하지 않음 |
| DTO 변경과 초안 | 필드 서명·대상 이름 변경, 편집 속성이 없는 필드로의 이동, 적용 조건 변경에 충돌. 표시용 Unit/Group 변경은 서명 불변. 실제 재컴파일 후 초안 유지 및 서명 충돌 차단도 확인 |
| 저장 끝까지 복구 | 두 번째 파일 쓰기 전·재임포트 전·최종 검증 전 실패, 실제 최종 validation 콜백 실패, 재임포트/복구 실패를 주입. 원본 전체 복원 또는 pending journal 보존·추가 저장 차단. 새 저장소 인스턴스에서 복구. 외부 수정과 삭제 모두 덮어쓰기/재생성 금지 |
| 개발자 진단 | 공통 등록 형식의 테스트 주제/DTO에서 편집 가능·속성 없음·조건 불일치·미지원 타입·JSON 값 없음·잘못된 조건 참조를 자동 판별. long 9007199254740995를 그대로 패치. 제품 JSON이나 창에 테스트 필드 목록을 추가하지 않음 |

원문 토큰 검사에서 BOM·CRLF·문자열 escape·다른 ID의 같은 필드·고정밀 미수정 소수·생략 필드를 보존했다. 잘못된 숫자·무변경 저장·외부 수정 충돌·초기 운반량 교차 규칙도 실제 파서/Composer로 검증했다. 상세 테스트명과 결과는 [최종 XML](editmode-completion.xml)을 따른다.

저장소의 선택 필드 치환·전체 카탈로그 검증·원자적 파일 교체·실패 journal 복구 경로를 자체 검토했다. 새 런타임 singleton/UI 의존, 수동 편집 필드 목록, 전체 DTO/DOM 재직렬화, SO 밸런스 복제는 없다. 그룹 메타데이터는 확장용 표시 정보이며 현재 모든 수치는 기본 그룹으로 표시한다.

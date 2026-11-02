# TECH-015 Unity Localization과 CSV 텍스트 원본

- 상태: 채택·구현 및 Editor 검증 완료
- 근거·결과: 사용자 승인 [PLAN-018](../plans/PLAN-018-localization-and-csv-text-management.md).
- 관련: [앱 수명](TECH-013-app-scenes-and-loading-screen.md), [JSON](TECH-012-topic-json-catalog.md), [파일 교체](TECH-014-production-storage-and-shared-ui-contracts.md).

## 선택과 이유

1. Unity Localization 1.5.13의 Locale·String Table·Smart Strings를 사용한다. 자체 번역 사전·복수형 엔진을 중복 구현하지 않는다.
2. 주제별 CSV를 편집 원본으로 두고 공개 패키지 CSV API 전에 ID·인수·번역 검토 계약을 검사한다. 밸런스 JSON은 기존 경계를 유지한다.
3. TextManager·UnityLocalizationService는 일반 C# 객체다. SceneFlowManager의 AppLocalization이 앱 수명으로 소유하며 격리 진입은 GameBootstrap이 같은 계약을 소유한다. 별도 싱글톤·서비스 로케이터는 불필요하다.
4. 패키지 SelectedLocale·테이블이 원본이다. 서비스는 준비된 테이블 참조만 보관한다. 새 언어의 필수 테이블 준비 후 마지막 요청만 적용한다.
   Localization 1.5.13은 null 결과의 개별 ReleaseTable로 실패 캐시를 지우지 못하므로, 재시도는 직렬화한 로드 사이에서 데이터베이스 캐시를 초기화한다. 성공한 테이블 핸들을 명시적으로 Acquire해 기존 표시를 보존하고 소유자 종료 시 자기 참조만 Release한다.
5. 문구는 테이블·키·불변 인수로 표현하며 중첩 이름까지 같은 언어로 해석한다. 누락 시 전체 메시지를 한국어로 대체한다.
6. 기존 UI는 준비된 테이블을 포맷하고 UiList 행을 재사용한다. TextManager.Observe는 별도 표시 수명에서 사용하며 동일 Text의 이중 연결을 금지한다.
7. 언어 변경은 입력 세대를 갱신해 이전 언어에서 시작한 클릭을 무효화한다. 언어 저장은 앱 범위 Preferences이며 AtomicTextFile을 재사용한다.
8. 초기 오류는 Bootstrap CSV에서 생성한 작은 JSON으로 표시해 패키지 초기화와 순환하지 않는다.
9. 정의 JSON의 displayName을 displayNameKey로 바꾸되 저장 호환성 해시에서 계속 제외한다. 보고는 사건·수치·시각으로 유지하고 Gameplay의 완성 영문 Text를 제거한다.
10. 원문·문맥·인수·Smart·번역이 바뀌면 재검토한다. Import는 승인하지 않으며 명시적인 개별 검토 명령을 사용한다.
11. 이름 변경·삭제는 참조 검사와 별도 적용 메뉴를 사용한다. 현재 검사 대상은 코드 리터럴/조건 키·Laboratory 헬퍼·JSON 이름 키다. 새 동적/에셋 바인딩은 검사 확장과 함께 도입한다.

기존 legacy uGUI·폰트를 유지한다. TMP 전환·원격 공급·런타임 CSV 파싱은 제외한다.
Editor는 Addressables 에셋 직접 사용 모드로 확인한다. 사용자 요청에 따라 Player/콘텐츠 빌드·실기기는 제외하며 배포 콘텐츠 검증으로 확대 해석하지 않는다.
작업 방법은 [텍스트 지침](../agents/localization.md), 도구 설명은 [CSV README](../../UnityProject/Assets/CSV/Localization/README.md)에 둔다.

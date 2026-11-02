# Localization CSV 사용 설명

원칙은 [텍스트 지침](../../../../docs/agents/localization.md), 진행·결과는 [PLAN-018](../../../../docs/plans/PLAN-018-localization-and-csv-text-management.md)을 따른다. 런타임은 CSV를 직접 읽지 않는다.

## 파일과 열

Bootstrap은 초기 준비·오류 복구, Common/Title/Loading은 공통 설정·타이틀·확인 화면, Laboratory/Status는 연구소·시설 상태, Definitions는 정의 표시 이름, Reports/Results는 보고·결과를 담당한다.

| 열 | 규칙 |
| --- | --- |
| Key | 주제 안에서 고유. 이름 변경 시 사용처도 정비 |
| Id | 안정적인 양의 15자리 이하 정수. 재생성·재사용·지수 표기 금지 |
| Shared Comments | 번역 문맥. 변경 시 재검토 |
| Smart | true/false. 테이블 IsSmart에 적용 |
| Arguments | count:number;resource:text 형식. text/number/bool |
| ko, en | 필수 한국어·영어. 공백 불가 |
| Reviewed:en | 원문·문맥·인수·Smart·해당 번역의 검토 스탬프. 직접 계산하지 않음 |

예: 한국어 `보유 {resource} {count:N0}개`, 영어 `Stock: {count:N0} {resource}`, Smart=true, Arguments=`resource:text;count:number`.
복수형 예: `{count:N0} {count:plural:item|items}`. 모든 번역의 변수 집합은 인수 계약과 일치해야 한다.

## 수정과 검토

1. UTF-8 CSV를 편집한다. 쉼표·따옴표·개행은 표준 CSV 인용을 사용한다.
2. `Tools/Mobile Game/Localization/Preview CSV Changes`에서 변경을 확인한다.
3. `Review Translations` 창에 주제·Key·대상 Locale을 입력하고 Inspect로 최신 원문·문맥·번역을 확인한다.
4. 실제 검토 후 Confirm한다. Inspect 이후 CSV가 바뀌면 다시 확인해야 한다.
5. `Validate CSV` → `Import Reviewed CSV` → `Validate Imported Tables`를 실행한다.
6. Editor Play에서 영향 화면·언어 전환을 확인한다. 빌드는 필요하지 않다.

검토 기록 시 CSV writer가 Key Ordinal 정렬·CRLF로 기록하고 ID는 유지한다.
`Normalize CSV Source`는 같은 정렬·인용 규칙을 명시적으로 적용하며 검토 스탬프를 바꾸지 않는다. `Inspect Key References`는 누락을 검사하고 사용되지 않는 키를 나열한다.
한국어만 바뀌어도 영어 재검토가 필요하다. 번역 수정이 불필요한 경우에도 직접 확인한다. Import는 검토를 자동 승인하지 않는다.
자동화용 개별 확인 API는 `LocalizationCsvTools.ConfirmReview(topic, key, language)`다.

## 추가·이름 변경·삭제·확장

- 추가: 새 키·사용하지 않은 ID·문맥·번역·인수 계약을 입력하고 검토한다.
- 이름 변경: ID를 유지하고 코드/JSON 참조를 함께 수정한다. Preview의 RENAME을 확인하고 `Apply Reviewed Key Changes`를 실행한다.
- 삭제: 참조 제거 후 Preview의 REMOVE를 확인하고 같은 명시적 메뉴로 적용한다. 사용 중인 키 삭제는 차단된다.
- 주제 삭제: 참조와 String Table Collection을 Editor에서 정리한 뒤 CSV를 제거한다. 고아 Collection은 검사 실패다.
- 테이블 직접 수정은 불일치 검사로 탐지한다. CSV로 다시 가져와 복구한다. 적용 중 실패는 기존 테이블·등록·내장 오류 문구로 되돌린다.
- 언어 추가: Locale 등록 → 모든 CSV에 언어 열·Reviewed:코드 추가/번역/검토 → 각 Collection의 해당 Locale 테이블 추가 → 가져오기/검사 → 폰트·문법·가로/세로·큰 글자·저장·fallback 검증.
- UI는 등록 Locale 목록을 사용한다. ko/en은 필수이며 세 번째 제품 언어의 번역은 별도 범위다.

## 실행 명령

저장소 루트에서 열린 Editor 연결을 사용한다.

```powershell
unity command editor invoke_menu --path 'Tools/Mobile Game/Localization/Validate CSV' --project-path UnityProject --caller plugin --skill localization --json
unity command editor invoke_menu --path 'Tools/Mobile Game/Localization/Preview CSV Changes' --project-path UnityProject --caller plugin --skill localization --json
unity command editor invoke_menu --path 'Tools/Mobile Game/Localization/Import Reviewed CSV' --project-path UnityProject --caller plugin --skill localization --json
unity command editor invoke_menu --path 'Tools/Mobile Game/Localization/Validate Imported Tables' --project-path UnityProject --caller plugin --skill localization --json
```

메뉴 호출 성공과 검사 통과는 다르다. 콘솔 오류와 완료 로그를 확인한다.
생성 결과는 `Assets/SO/Localization/`, `Assets/Settings/Localization/`, 패키지 등록은 `Assets/AddressableAssetsData/`에 있다.
Editor는 Addressables의 Use Asset Database 모드를 사용하며 이 명령들은 콘텐츠/Player 빌드를 실행하지 않는다.

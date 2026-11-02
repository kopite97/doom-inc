# PLAN-018 구현·검증 기록

완료 판정은 [PLAN-018](../../PLAN-018-localization-and-csv-text-management.md)의 원본 상태를 따른다. 구현·Editor 검증 완료이며 빌드·실기기 판정은 포함하지 않는다.

## 환경과 원본

- Unity 6000.3.27f1, Unity CLI 1.0.0-beta.15, Pipeline 0.9.1-exp.1.
- Localization 1.5.13, 실제 lock의 Addressables 2.10.3. 의존성 선언의 최소 버전과 실제 lock 버전을 구분한다.
- 기존 Editor·Android 대상에서 Editor 컴파일/Edit/Play만 실행한다. Player 컴파일·APK·IL2CPP·Addressables 콘텐츠 빌드는 실행하지 않는다.
- 패키지 최초 설치는 인증 실패였으나 사용자 Hub 로그인 후 UPM 설치가 완료됐다.
- CSV 9개 주제·247개 키, ko/en. 공통 미사용 키 2개는 Preview의 REMOVE 확인 후 명시적 키 변경 메뉴로 정리했다.
- [최종 테이블 일치·참조 검사](csv-validation-final.json): 247개 키 유효·CSV/테이블 일치·누락 참조 없음·미사용 키 0. 초기 공급자 직렬화 문제는 아래 테스트 격리 수정과 구분해 기록한다.

## 이관/제외 목록

| 대상 | 처리 |
| --- | --- |
| Title·표시/언어 설정 | Common/Title. 등록 Locale 목록에서 선택 버튼 생성 |
| 공용 LoadingScreen·확인·오류·재시도 | Loading/Bootstrap. 열린 메시지는 키·인수 유지 |
| 패키지 준비 전 BootstrapLoadingView | Bootstrap CSV에서 생성한 독립 내장 JSON |
| 연구소 HUD·메뉴·배치·작업/자원/시설 상세·안내·알림 | Laboratory. 문장별 Smart 인수와 기존 행 식별자 유지 |
| 시설 상태 표식·작업/자재 사유 | Status와 Laboratory. 상태 조회 책임은 그대로 유지 |
| 시설·생명체·자원 이름 | JSON displayNameKey → Definitions. 정의 ID·밸런스·호환성 해시 보존 |
| 전과 보고 | Reports. Gameplay의 영문 Text 제거, 사건·수치·시각은 구조화 상태 유지 |
| 멸망 연출·결과·새 판·오류 | Results. 재번역이 단계·시계·콜백을 재시작하지 않음 |
| 씬·프리팹·생성 도구 | 제품 문구는 런타임 생성 경로에 모여 있음. 제품 씬/공용 UI 프리팹에 별도 m_Text 문구 없음 확인 |
| 명령/정의 ID·상태 코드·GameObject/행 이름·맵의 임시 기호·숫자 | 프로그램/임시 표현 식별자로 유지 |
| 하네스·진단 씬·Editor UI·예외 원문·로그·법적 폰트 라이선스 | 제품 번역 제외. 오류 종류를 제품 번역과 구분하고 진단 값 보존 |

## 확인한 결과

- [메시지 기본 Edit](text-core-edit.json): 초기 9/9 통과. 이후 전체 Edit에 fallback·설정·CSV 검사를 추가했다.
- [CSV Edit](csv-edit-rerun.json): 11/11 통과. BOM·인용/개행·인수/ID·검토·세 번째 언어 열·참조 삭제·적용 실패 복구·반복 반영.
- [패키지 Play 기본](localization-play-integration.json): 한국어/영어·중첩 인수 1/1 통과.
- [연구소 언어 Play](laboratory-language-play.json): 미리보기·세션·알림·해시 유지, 오래된 클릭 차단, 언어 파일 저장·재진입 1/1 통과.
- [공급/실패 Play](provider-play-rerun.json): 실패 캐시 재시도, 마지막 요청 우선, 늦은 응답 차단, 테스트용 fr 등록/조회/fallback·정리 1/1 통과.
- [가로/세로 큰 글자 초기 검사](localization-layout-initial.json): 1/1 통과 후 이미지 검수에서 Close의 단어 중간 줄바꿈을 발견해 폭을 늘리고 긴 알림 높이를 확보했다. 아래 최종 전체 Play와 화면 증거에서 수정을 확인했다.
- [전체 Edit 첫 검사](edit-regression-initial.json) 308/313 → [두 번째](edit-regression-second.json) 327/329. 초기 계약 변경과 손상 설정 예외 누락을 수정한 과정이며, 아래 최종 결과와 구분한다.
- [전체 Edit 최종](edit-regression-final.json): 330/330 통과. 키 추가·이름 변경·삭제·ID 유지·테이블 수동 변조 탐지와 원본 복구까지 포함한다.
- [전체 Play 최종](play-regression-final.json): 57/57 통과·실패/건너뜀 0. 기존 게임 흐름과 생산/배양·전과 보고·멸망/결과·재시작 중 언어 변경, 열린 확인창의 중첩 이름/수량/콜백 유지, 늦은 응답/실패/추가 Locale를 포함한다.
- 중간 전체 Play는 [첫 검사](play-regression-initial.json), [두 번째](play-regression-second.json), [세 번째](play-regression-third.json), [네 번째](play-regression-fourth.json), [다섯 번째](play-regression-fifth.json)로 보존한다. 오류 표시 계약·비동기 준비 대기와 아래 검증 환경 문제를 해결한 후 최종 전체 검사를 통과했다.

## 최종 화면

Editor 125% 큰 글자에서 [한국어 가로](plan018-ko-1280x720.png)·[한국어 세로](plan018-ko-720x1280.png)·[영어 가로](plan018-en-1280x720.png)·[영어 세로](plan018-en-720x1280.png)를 확인했다. 화면/렌더 타깃 치수를 함께 검사했다. Close의 단어 중간 줄바꿈을 해소하고 긴 알림 높이를 확보했다. 상세 패널 끝의 잘림은 스크롤 클리핑이며 내용은 스크롤로 접근한다. 모바일 밀도·실기기 폰트/성능은 이번 검증에 포함하지 않는다.

## 실제 Editor 재진입

동일 실행의 두 시점에서 frame/time 증가를 확인했다. 모든 행에서 초기화 완료·오류 없음, SceneFlowManager/GameDataStore/EventSystem 각각 1개였다.

| 진입·설정 | frame / time 변화 | 증거 |
| --- | --- | --- |
| Bootstrap 전체 흐름, Domain/Scene Reload ON | 6753→13715 / 16.57→33.51 | [첫 시점](direct-bootstrap-on-1.json), [두 번째](direct-bootstrap-on-2.json) |
| Title 직접 진입, Domain Reload OFF | 2991→18539 / 7.68→45.75 | [첫 시점](direct-title-off-1.json), [두 번째](direct-title-off-2.json) |
| Laboratory 새 판, Domain Reload OFF | 3088→5477 / 10.42→18.51 | [첫 시점](direct-lab-off-1.json), [두 번째](direct-lab-off-2.json) |
| Laboratory 새 판, Domain/Scene Reload OFF | 2914→7944 / 9.01→25.58 | [첫 시점](direct-no-reloads-new-1.json), [두 번째](direct-no-reloads-new-2.json) |
| Laboratory 이어하기 재진입, Domain/Scene Reload OFF | 4345→9417 / 14.21→30.91 | [첫 시점](direct-no-reloads-continue-1.json), [두 번째](direct-no-reloads-continue-2.json) |

Title의 실제 언어 버튼으로 영어를 선택한 [호출](direct-title-language-action.json)과 [결과](direct-title-language-result.json)를 확인했다. 후속 직접 실행에서도 en을 유지했다. 제품 저장과 분리된 SlotB를 사용했고 검증 전 저장/언어/표시 설정 파일 3개와 Editor 선택을 백업해 [복원](direct-restored.json)했다. Game View 재진입 검증은 기기 터치 검증이 아니다.

## 검증 도구에서 발견한 문제

- 여러 Game 창이 서로 다른 해상도로 렌더링하면 Screen 값과 입력 좌표/캡처가 교대로 바뀐다. 크기 fixture가 기존 모든 Game 창을 같은 검증 해상도로 맞추고 각각의 원래 선택을 복원하도록 보완했다.
- 캡처는 창의 잘린 백버퍼 대신 Game View 렌더 타깃을 읽고 실제 치수를 검사한다. 초기 캡처를 최종 화면 증거로 사용하지 않는다.
- 테스트용 공급자는 LocalizationSettings 복제본에서 실행하도록 격리했다. 원본의 남은 테스트 참조를 Editor API로 제거하고 제품 Locale이 ko/en뿐임을 [확인](fixture-cleanup.json)했다. 테스트 후에도 잔류 여부를 검사한다.

최종 전체 검사 후 설정/SO/CSV에서 테스트 공급자·유지보수 fixture 키의 잔류가 없었다. [Editor 컴파일](compile-final.json)과 [문서 검사](docs-validation-final.txt)를 통과했고, [Editor 종료 상태](editor-final.json)는 Play 정지·백그라운드 실행 원복이다. 콘솔의 이전 실패 기록과 최종 실행을 구분하며, 마지막 전체 Play 실행 이후 새 오류 로그는 없었다.

## 재현

저장소 루트에서 열린 Editor를 사용한다. 실행 중 테스트가 있으면 완료 후 다음 실행을 시작한다.

```powershell
unity command script recompile --project-path UnityProject --caller plugin --skill unity-cli --json
unity command script recompile_status --project-path UnityProject --caller plugin --skill unity-cli --json
unity command test run --mode editor --project-path UnityProject --caller plugin --skill unity-cli --json
unity command test run --mode playmode --async_tests true --project-path UnityProject --caller plugin --skill unity-cli --json
unity command test status --project-path UnityProject --caller plugin --skill unity-cli --json
```

CSV 도구·검토·확장 절차는 [사용 설명](../../../../UnityProject/Assets/CSV/Localization/README.md)에 있다. 실제 실행 결과와 오류/재검사 구간을 구분한다.

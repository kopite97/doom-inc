# PLAN-015 실행 증거

Unity 6000.3.27f1·Pipeline 0.9.1-exp.1·Unity CLI 1.0.0-beta.15에서 Editor만 사용했다. Newtonsoft JSON은 기존 3.2.2를 직접 의존성으로 등록했으며 버전을 올리지 않았다. **Player 컴파일·APK/PC 빌드·IL2CPP·기기 설치/테스트는 실행하지 않았다.**

## 데이터와 초기화

- [동결 CSV 목록/크기/해시](baseline-csv/manifest.json)는 변환 전 5개 파일을 보존한다. 제품에서는 CSV 에셋과 CsvRow/CSV 파서·어댑터를 제거했다. 과거 증거와 비교 fixture는 제품 로딩에 사용하지 않는다.
- 제품 원본은 [JSON 파일 안내](../../../../UnityProject/Assets/JSON/Balance/README.md)의 주제별 파일 5개다. 원문 → 타입 DTO → Registry ID 결합 → 불변 Data → Runtime 경계를 적용했다.
- JSON으로 읽은 9개 정의의 해시는 기존 `2578869b88b49ba4855785cf587d8cf2f0cfd0dd8ba1fbfc2cd3fb8d2ca7f195`와 같다. 테스트는 전체 DTO 필드를 동결 CSV와 대조하고, 현재 저장/해시 없는 구버전 저장의 복원 및 다른 값의 거부를 확인한다.
- 수치 원문 정규화와 Runtime 실수 변환을 분리했다. `1`/`1.0`/지수 표기와 역할별 생략/명시적 기본값을 비교한다. `9007199254740993.0`의 Int64 값과 float 반올림으로 사라지는 수치 차이도 검사한다.
- [씬 이관](scene-migration.json)으로 플레이/Map 진단/Session 진단 씬의 직렬화 JSON 참조를 설정했다. [최종 의존성 검사](scene-dependencies-final.json)는 Player 컴파일과 분리된 AssetDatabase 검사다. [CSV 에셋 제거](retire-csv.json) 전에 세 씬의 JSON 포함/CSV 미참조를 확인했다.

## 실행 이력

| 실행 | 실제 결과 | 해석 |
| --- | --- | --- |
| [Edit 1](editmode-01.json) | 292개 중 291 통과·1 실패 | 생략 기본값 테스트가 필수 interactionX까지 제거한 fixture 오류. 생략 가능한 필드만 제거하도록 수정했다. |
| [Edit 2](editmode-02.json) | 292/292 통과 | 타입·값·해시·저장 등가성과 기존 회귀. |
| [Edit 3](editmode-03.json) | 294/294 통과 | 마지막 입력 검토에서 미사용 시설 실수에도 Runtime float 범위/언더플로 검사를 추가한 최종 Edit 실행. |
| [Play ON 1](playmode-on-01.json) | 36개 중 35 통과·1 실패 | 새 씬 검사에서 정상 진단 Log의 Expect 등록 누락. 게임 규칙 오류가 아니며 이 실행만으로 새 씬 초기화 완료를 판정하지 않았다. |
| [저장된 씬 재검사](saved-scenes-result.json) | 1/1 통과, 1.37초 | JSON/Registry 주입 없이 실제 저장된 세 씬을 새로 로드했다. 사용자 저장 대신 테스트 슬롯만 격리했다. |
| [Play ON 2](playmode-on-02.json) | 36/36 통과, 62.90초 | CSV 에셋 제거 후 전체 런타임 회귀. 최종 실수 범위 보완 전 이력이다. |
| [최종 Play ON](playmode-on-final.json) | 36/36 통과, 64.65초 | 최종 입력 검증 코드를 포함한 Domain Reload ON 전체 회귀. |
| [최종 Play OFF](playmode-off.json) | 36/36 통과, 62.48초 | Domain Reload OFF 전체 회귀. Store 재사용/재초기화와 두 판 흐름을 같은 검사로 확인했다. |

`*-start.json`의 running·0개는 시작 응답이며 통과 결과가 아니다. 실제 완료 응답의 실행 수와 개별 결과를 판정한다. 이전 PLAN의 테스트 수치를 이번 결과로 사용하지 않는다.

실행 명령의 공통 접미사는 `--project-path UnityProject --caller plugin --skill unity-cli --json`이다. Edit는 `unity command test run --mode editor`, Play는 `--mode playmode --async_tests true`를 사용하고 `unity command test status`의 완료 응답을 보존했다. Domain Reload 설정은 기존 Editor 검증 메뉴로 바꾸며 종료 시 원래 설정을 복원한다.

## 도구 진단과 제한

- CLI 업데이트 후 최초 컴파일에서 테스트 어셈블리의 Newtonsoft 참조 누락을 수정했다. PackageManager Client API로 기존 패키지를 직접 등록하고, 테스트 어셈블리에 필요한 DLL 참조를 명시했다.
- 코드 평가 중 Editor 캐시 재컴파일 요청이 응답을 지연시켜 이번 작업에서 연 Editor를 재시작했다. 이후 전용 recompile 명령으로 컴파일 완료를 확인했다. 이 도구 진단을 런타임 기능 통과로 간주하지 않는다.
- [테스트 전 콘솔](editor-before-tests.json)은 Editor 초기화의 ScriptableSingleton 오류 1건과 데이터/씬 검사 로그를 보존한다. 테스트 구간은 이 기록을 남긴 뒤 콘솔을 비워 구분했다. 최종 콘솔 상태는 완료 기록을 따른다.
- 빌드 도구의 경로/진단 문구도 JSON으로 갱신했지만 빌드 성공·IL2CPP·Android 실행을 확인한 것은 아니다. 원격 취득/API·정식 아트·성능 최적화는 범위 밖이다.

## 종료 확인

[원래 Play 설정 복원](play-options-restored.json) 후 [최종 콘솔](console-final.json)의 Read Play Options 로그는 원래 값인 옵션 활성·DisableDomainReload를 확인한다. [Editor 최종 상태](editor-final.json)는 ready·Play stopped·runInBackground false이며 [Editor 컴파일](compile-final.json)은 실패/오류 없음이다. 콘솔 오류·경고는 0이다.

[최종 정적 대조](static-final.json)는 JSON 5종·정의 9개/설정 2개, 누락 meta 0, 제품 CSV/옛 파서 없음, 동결 CSV 5개의 원본 바이트/해시 일치와 SO·게임 규칙·진행 저장 코드 변경 없음을 확인한다. 마지막 Editor 도구 정리에서는 향후 Player 컴파일 로그가 과거 PLAN-012 증거를 덮어쓰지 않도록 시간별 로컬 Logs 경로로 바꿨고 Editor 컴파일만 확인했다. 런타임 코드는 최종 ON/OFF 검사 이후 바꾸지 않았다.

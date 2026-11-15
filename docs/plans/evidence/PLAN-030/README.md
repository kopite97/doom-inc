# PLAN-030 검증 증거

2026-11-15, [PLAN-030](../../PLAN-030-sprite-selection-outline-and-deselect.md)의 선택 외곽선·빈 땅 선택 해제 구현 검증이다.

## 환경과 실행 범위

- Unity 6000.3.27f1 / URP 17.3 / All In 1 Sprite Shader 4.71 / Windows Editor / Android 활성 타깃.
- Unity CLI 1.0.0-beta.17, 실행 중 Editor의 Pipeline 0.9.1-exp.1 서버를 사용했다. CLI 업데이트·알림 설정은 변경하지 않았다.
- 기존 Domain Reload OFF에서 아래 일반 Play Mode 테스트 27개를 실행했다. 선택 전용 3개는 Domain Reload ON에서도 재실행했다. 빌드·실기기·고부하·성능 계측은 실행하지 않았다.
- `Device`는 테스트 필터 이름이다. Editor Input System의 마우스·터치 재현이며 실제 모바일 기기 실행이 아니다.

## 결과

| 실행 필터·범위 | 결과 | 원본 로그 |
| --- | --- | --- |
| SelectionOutline / Domain Reload OFF | 3/3 | [결과](selection-domain-off-tests.json) |
| Device / 마우스·터치·UI·지도 조작 | 15/15 | [결과](device-tests.json) |
| MovementPresentation / 이동·표시 수명 | 3/3 | [결과](movement-tests.json) |
| FacilityAnimation / 시설 프레임·정지·배속 | 3/3 | [결과](facilities-tests.json) |
| WorldRendering / 월드 표시·정렬 | 2/2 | [결과](world-tests.json) |
| FloorArtKeeps / 바닥·배치 미리보기 | 1/1 | [결과](floor-tests.json) |
| SelectionOutline / Domain Reload ON 재실행 | 3/3 | [결과](selection-domain-on-tests.json) |

- 신규 선택 테스트는 시설 4종·생명체 프로토타입·임시 생명체의 선택 재질 전환/복원, 바닥·운반물 공유 재질 보존, 프레임 변경, 줌 1/8, 화면 재진입 시 임시 텍스처 해제를 확인한다.
- 1280×720·720×1280에서 마우스 빈 땅 클릭·터치 탭으로 선택과 패널을 해제한다. 펼친 상세·후보 목록·선택과 무관한 메뉴 보존, 시설 이동 미리보기 우선권, 선택 자동 지도 이동 중단, 세션 재시작도 확인한다.
- [컴파일](compile.json)은 완료·실패 없음이다. 초기 신규 테스트 소스의 컴파일 오류 4건을 수정한 후 최종 테스트를 진행했다. [최종 Console](final-console.json)의 누적 telemetry 4건은 이 초기 오류다. [수정 이후 오류 조회](console-after-fix.json)는 entries 0, 현재 Console 오류·경고 0, compilationFailed false다.
- [최종 환경](final-environment.json): 셰이더 지원 true, 예상 키워드 3개, ShaderUtil 메시지 없음, 셰이더 컴파일 종료. [Editor 상태](final-editor.json)는 stopped/ready다. Domain Reload OFF(options enabled=true, options=1), runInBackground=false를 복원했으며 EditorSettings 파일도 작업 전과 같다.
- `*-start.json`은 비동기 실행 접수 응답이므로 완료 결과로 사용하지 않는다. `selection-tests.json`은 초기 선택 테스트 통과 이력이며, 최종 판정은 위 OFF/ON 결과다.

## 화면 확인

최종 선택 테스트가 실제 Game View를 캡처한 PNG다. 코드가 만든 개념도나 합성 이미지가 아니다.

| 확인 대상 | 화면과 관찰 |
| --- | --- |
| 자원 획득·배양 시설 | [획득](screens/body-facility.gathering.png), [배양](screens/body-facility.cultivation.png): 투명 경계를 따라 청록 선, 기존 노란 선택 사각형 없음 |
| 가공·저장 시설 | [가공](screens/body-facility.processing.png), [저장](screens/body-facility.storage.png): 본체 외곽선, 주변 시설의 표현 유지 |
| 생명체 | [프로토타입](screens/body-creature.support.prototype.png), [임시 본체](screens/body-creature.support.png): 현재 Sprite 외곽선 적용. 임시 사각형은 외곽선도 사각형 |
| 가로 화면 | [선택](screens/selected-1280.png), [해제](screens/cleared-1280.png): 빈 땅 클릭 후 외곽선·선택 패널 제거 |
| 세로 화면 | [선택](screens/selected-720.png), [해제](screens/cleared-720.png): 터치/마우스 검증과 동일하게 선택 표시 정리 |
| 줌 양끝 | [줌 1](screens/animated-zoom-1.png), [줌 8](screens/animated-zoom-8.png): 프레임 외곽선 유지, 캡처에서 시트 이웃 프레임 유입·잘림 관찰되지 않음 |

- 두께는 원본 텍스처 1픽셀 기준이므로 확대하면 화면에서도 굵어진다. 현재 생명체 Sprite에 포함된 그림자의 알파 경계도 외곽선에 포함된다. 운반물을 합친 통합 외곽선은 구현 범위가 아니다.
- 최초 캡처는 Editor 비동기 셰이더 컴파일 중 청록 임시 표시를 잡았다. 캡처 전에 `ShaderUtil.anythingCompiling` 종료를 기다리도록 수정하고 OFF/ON 테스트와 화면을 다시 생성했다. Unity의 [비동기 셰이더 컴파일 문서](https://docs.unity.com/en-us/engine/6000.3/manual/materials-and-shaders/shaders/shader-troubleshooting/shader-reduce-stalling/asynchronous-shader-compilation)를 참고했다.
- 초기 세로 캡처에서 겹친 후보용 임시 생명체가 선택 대상을 가렸으므로 후보 해제 확인 후 다른 위치로 옮겼다. 위 최종 화면은 변경된 테스트 배치의 결과다.

## 재현과 추적

- 테스트 소스: [선택 전용 테스트](../../../../UnityProject/Assets/Scripts/Tests/PlayMode/Laboratory/LaboratoryViewTests.Selection.cs). 기존 Movement/Facilities/Floor 테스트의 선택 재질·배치 테두리 기대값도 갱신했다.
- 실행은 `unity command`의 `test run`으로 PlayMode와 위 filter/category를 지정하고 `test status`가 completed일 때 결과를 보관했다. 실제 인자는 각 `*-start.json`에 있다. 공통 인자는 `--caller plugin --skill unity-cli --project-path C:/Dev/Unity/Doom_Inc/UnityProject --json`이다.
- 캡처 생성 위치는 `UnityProject/Temp/PLAN-030/screens`이고 이 디렉터리의 screens에 최종 12장을 복사했다. 기존 시설 회귀 캡처도 Temp 증거 경로를 사용하도록 변경해 PLAN-029의 승인된 과거 화면을 덮어쓰지 않는다.
- 프로젝트 소유 선택 재질과 AppConfiguration/연구소 씬의 직렬화 참조를 확인했다. 외부 AllIn1Assets 원본·meta는 Git 제외이며 복원은 [저장소 지침](../../../agents/repository.md)을 따른다. 공급사 파이프라인 검사기가 설정한 `ALLIN1_URP` define을 유지했다.
- 문서 검사 `node scripts/validate-agent-docs.mjs`는 PLAN 30개·active 0개·목록 30개·로컬 링크 3486개 PASS, `git diff --check`는 공백 오류 없음이다. Editor 결과는 Player 셰이더 변형 포함 여부나 실기기 성능 증거가 아니다.
- MulmaruDynamic.asset은 작업 전부터 변경되어 있었으며 Editor 검증 중 동적 글리프 캐시가 추가로 갱신됐다. 기존 변경과 섞여 있어 원복하지 않았다. 폰트 디자인·설정 변경을 의도한 작업은 아니며 커밋 시 별도 검토 대상으로 남긴다.

# PLAN-016 Editor 검증 기록

환경: Windows, Unity 6000.3.27f1, Unity CLI 1.0.0-beta.15 / Pipeline 0.9.1-exp.1, 기존 열린 Editor. Android 대상 설정은 유지했다. 사용자 지시에 따라 Player 컴파일·빌드·APK·기기 설치는 실행하지 않았다.

## 자동 검증

- [Domain Reload ON 전체 Play](play-domain-on-result.json): 46/46 통과. 신규 앱 흐름 10개와 기존 게임 회귀 36개.
- [최종 Domain Reload ON 전체 Play](play-final-result.json): 47/47 통과. 씬 도착 중 관리자 종료 검증을 추가한 최종 코드의 앱 흐름 11개와 기존 회귀 36개.
- [Domain Reload OFF 전체 Play](play-domain-off-result.json): 46/46 통과. 이후 추가한 씬 도착 시 관리자 종료 경계를 포함한 [최종 앱 흐름 OFF](app-final-off-result.json) 11/11 통과.
- [초기 Edit 결과](edit-first.json): 293/294. SampleScene의 제품 포함을 요구하던 기존 단언을 Bootstrap·Title·Laboratory 목록으로 이관하고 [대상 재검증](edit-environment-fixed.json) 1/1 통과.
- [최종 Edit](edit-final.json): 294/294 통과. 최종 코드와 제품 씬 참조를 검증했다.
- [첫 전체 Play](play-all-first-result.json): 42/43. 기존 물리 입력 재시작 테스트 1건 실패 후 [단독 재검증](restart-focus-result.json) 통과. 최종 ON 전체에서는 동일 테스트를 포함한 46개가 통과했다. 단독 통과만으로 회귀 완료로 판단하지 않았다.
- [확장 시나리오 첫 결과](app-expanded-result.json): 8/10. 종료 연출 테스트가 기존 프레임별 최대 시간 규칙을 따르도록 수정했고, 화면 캡처는 Host backbuffer 대신 GameView의 실제 RenderTexture로 바꿨다. 최종 ON 전체에서 재검증했다.
- [초기 앱 흐름 실패](app-flow-first-result.json)와 [재컴파일 후 5/5](app-flow-second-result.json)는 수정 이력을 보존한다. 초기 오류·실패 기록은 최종 성공 로그와 혼합하지 않는다.
- [Edit 입력 경계 실패](edit-input-boundary-failure.json): 291/294. Play 종료 뒤 남은 입력 잠금이 Edit Mode의 직접 UI 테스트를 막던 문제를 수정해, 앱 입력 잠금은 Play 중에만 적용한다.
- [화면 준비 대기 실패](app-title-readiness-failure.json): 10/11. 회전 후 버튼이 실제 조작 가능해진 시점까지 기다린 다음 한 번 Submit하도록 테스트 준비를 수정했다. 고정 프레임 대기를 충분한 준비 완료로 간주하지 않는다. 위 최종 앱 흐름 OFF에서 재검증했다.

## 검증한 앱 계약

Bootstrap → Title 체류 중 세션/저장 생성 없음, 명시적 새 판·이어하기, 저장 성공 후 메뉴 복귀, 카탈로그 재사용, 활성 EventSystem과 SceneFlowManager 각 하나를 확인했다. 저장 손상·쓰기 실패·초기 데이터 실패·이전 씬 제거 뒤 대상 화면 누락의 오류 UI와 재시도, 최신 표시 설정의 쓰기 실패 후 씬 왕복·새 판·저장 재시도를 확인했다.

전환 중 pause는 대상 세션 시작을 막는다. 이전 화면의 눌린 버튼 해제는 새 확인창을 실행하지 않는다. 중복 관리자는 기존 소유자를 교체하지 않는다. 관리자 종료 후 완료된 공유 데이터 작업은 예전 씬 진입을 실행하지 않는다. 공유 데이터 자체는 GameDataStore 수명에서 완료·재사용하며, 저장소 파괴 시 취소와 늦은 결과 차단은 기존 GameDataStore 회귀로 검증한다.

이미 시작한 씬 로드는 취소할 수 없으므로 후속 관리자는 해당 작업 완료를 기다린다. 씬 도착 콜백에서 관리자를 파괴해도 대상 GameBootstrap은 취소된 진입을 자동으로 새 판으로 바꾸지 않는다. 오류 표시 후 명시적인 새 초기화로 복구하는 경로를 추가 검증했다.

운영·종료 직전 시나리오는 공통 LaboratoryCommands·GameFlow 시간 진행으로 준비하며 정상 Continue로 복원한다. 저장된 Destruction·Results 단계로 복원되는지 확인했다. 준비용 시간 진행은 실제 모바일 사용자 입력으로 집계하지 않는다.

## 화면

[screens](screens)에는 1280×720, 720×1280에서 큰 글자를 적용한 타이틀·설정·덮어쓰기 확인·로딩·오류 화면을 보관한다. GameView의 렌더 타깃을 캡처하여 OS 창 크기에 따른 왜곡을 피했다. 화면 밖 버튼·글자 잘림을 확인했으며 오류 버튼은 실제 ScrollRect viewport 안에 있는지도 단언했다. Safe Area의 계산·기존 게임 레이아웃은 기존 Edit/Play 회귀를 함께 사용한다.

## 실제 Editor 메뉴 실행

- [Laboratory 새 판 관찰 1](direct-lab-new-1.json)·[관찰 2](direct-lab-new-2.json): frame 3252→6570, time 10.9086→21.7363. Ready, Playing, generation 1, SceneFlowManager·GameDataStore·EventSystem 각 1개. 단순 Play 진입/화면 캡처만으로 실행 중이라고 판단하지 않았다.
- [이어하기](direct-lab-continue.json), [저장 개체 ID 비교](direct-continue-save.json): 동일 개체 2개의 ID를 유지했다. Editor SlotB만 사용했다.
- [Title 직접 진입](direct-title.json), [Full Bootstrap Flow](direct-full-bootstrap.json): Title Ready, 게임 세션 없음. 기존 시작 씬이 Bootstrap이어도 Current Scene 도구는 현재 씬을 실행하며 [종료 후 원래 시작 씬](direct-start-restored.json)으로 복원한다.
- Domain Reload와 Scene Reload를 모두 끈 [새 판](direct-no-reloads-1.json)·[이어하기 재진입](direct-no-reloads-2.json): 각각 generation 1, 영속 세 객체 각 1개, 정상 Playing.
- [기존 상태](direct-original.json)와 [복원](direct-restored.json): 열린 Laboratory 씬·미수정 상태·background false·기본 슬롯·시작 씬 없음·원래 Play 옵션으로 복원했다. 제품 씬 목록은 [정적 검사](product-scenes-validation.json), 다섯 씬의 JSON 참조는 [별도 검사](json-references-validation.json)로 확인했다. 해당 메뉴는 빌드를 호출하지 않는다.
- 임시 SlotB는 준비 전 없었으나 생성한 `EditorSessions/SlotB/session.json`의 정리 명령이 자동 승인 검토에서 정책상 차단되어 남겨 두었다. 구체적 차단 사유는 제공되지 않았다. 현재 선택은 원래 Default 슬롯이며 제품 저장에는 접근하지 않았다.

최종 검사 후 [Play 옵션 복원](play-options-final-restored.json)을 수행하고 원래 `DisableDomainReload` 설정이 메모리·파일에서 일치하는지 확인했다. 최종 에셋 참조 검사 이후 [Console](final-console.json)의 새 오류·경고는 0이다. 문서 검사와 `git diff --check`도 통과했으며 신규 에셋의 `.meta` 누락은 없다.

## 한계

새 Android 패키지의 시작 씬·실제 터치·회전 센서·플랫폼 패키징은 이번 결과로 검증한 것이 아니다. 정식 아트는 교체하지 않았으며 신규 Sprite 요청도 없다.

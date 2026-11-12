# PLAN-029 Unity 적용·기술 검증

2026-11-12, Windows / Unity 6000.3.27f1 / URP Direct3D12 / Android 대상 Editor. 빌드·고부하·실기기 테스트는 실행하지 않았다. Art 승인과 별도로 2026-11-12 사용자가 Unity 게임 화면과 동작을 그대로 승인하고 PLAN-029 완료·main 푸시를 요청했다. 기술 검증·최종 사용자 시각 확인·환경 복원을 모두 충족해 PLAN-029 completed, active 0개다.

## 적용과 준비

- [수령 검증](received-art.json): Art PLAN-006/007/008의 승인 r001, 원본/보존본/출력/Unity PNG 12파일 해시 일치, 총 48프레임·9태그·프레임 시간 확인.
- [설치 기록](install-result.json), [전체 테스트 준비](test-readiness.json): 세 PNG를 정규 Facility 폴더로 GUID 유지 이동, Multiple 4×4·Full Rect·중앙 피벗·PPU32·Point·무압축, 16프레임/시설·정의 아이콘·본체/미리보기·생성 도구 참조 완료. **2026-11-12T13:55:38Z 전체 준비 확인 후** 처음 게임 테스트를 실행했다.
- [TECH-019](../../../decisions/TECH-019-facility-art-and-committed-events.md): 실제 생산 성공과 소유 이전 관찰, 화면 소유 반응·최신 대기 한 개, 실제 진행률/시뮬레이션 시계 사용. 생산/재고/저장 규칙은 바꾸지 않는다.

## 최종 결과

| 실행 | 통과/실행 | 증거 |
| --- | --- | --- |
| 전체 Edit Mode | 418/418 | [JSON](edit-result.json), [XML](edit-result.xml) |
| 시설 생산/이전·회전/표시, Domain Reload OFF | 2/2 | [JSON](facility-off-result.json), [XML](facility-off-result.xml) |
| 같은 시설 수명/표시, Domain Reload ON | 2/2 | [JSON](facility-on-result.json), [XML](facility-on-result.xml) |
| 실제 건설·이동/취소·줌·영어 큰 글씨 | 1/1 | [JSON](construction-result.json), [XML](construction-result.xml) |
| 기존 ApprovedArt 회귀 | 5/5 | [JSON](existing-art-result.json), [XML](existing-art-result.xml) |
| 기존 MovementPresentation 회귀 | 3/3 | [JSON](movement-result.json), [XML](movement-result.xml) |

일반 Play 검사는 총 13회 통과(고유 시나리오 11개), 실패/건너뜀 0이다. 대규모 시설/생명체와 성능 카운터를 사용하는 테스트는 선택하지 않았다. 전체 Laboratory 테스트를 무필터 실행하지 않았다.

- 실제 가공/배양 자동 생산 20%에서 Working, 산출/출현당 Complete 한 번, 산출물 대기 중 반복 억제, 정지/재개, 서로 다른 저장소의 실제 부분 입출고·동일 시점 후속 반응, 생성/복원 시 반응 억제와 해제 후 구독 정리를 확인했다.
- 네 방향의 크기/회전, 저장소 비정사각형 보존, 같은 그림의 미리보기와 탭 선택, 건설 미완료→완료, 무효 배치 거부, 이동 확정/취소를 확인했다. 화면 게시만 반복해도 저장 상태가 변하지 않는다.
- 가로/세로, 줌1/1.5/3/8, 상태 OFF, 한국어 보통/영어 큰 글씨 화면을 확인했다. 확대 시 화면 밖 부분이 viewport에서 잘리는 카메라 동작은 원본 Sprite 변형과 구분한다. 실제 색감·외형/동작은 2026-11-12 사용자가 게임 화면 확인 후 그대로 승인했다.
- 최초 Play 실행은 1/2였다([JSON](facility-initial-failed.json), [XML](facility-initial-failed.xml)). 캡처의 EndOfFrame 직후 다음 LateUpdate 전에 이전 Sprite를 읽던 검사에 갱신 대기를 추가했다. 실제 성공 사건·방향·시간 기대값은 유지했고 최종 실행은 2/2 통과했다.
- 신규 검사 작성 중 누락된 Editor 네임스페이스의 컴파일 오류 2건을 수정했다. 이전 README 임포트 중 파일 시각 차이 경고 1건과 함께 [Console 이력](console-history.json)에 보존했다. [최종 컴파일](compile-final.json)은 완료·오류 0, [최종 Console](console-final.json)은 컴파일 실패 false·오류/경고 0이다.

## 게임 화면

- [자동 생산 Working GIF](screens/automatic-working.gif): 실제 GameView 8장을 원래 자동 생산 시간 간격(625ms)으로 조합. 실제 가동이 20%이므로 가공 Working 한 주기가 5초다. 배양은 같은 실제 진행률에서 자신의 150ms 프레임 시간을 사용한다.
- 기본 외형 [0도](screens/rotation-0-landscape.png), [90도](screens/rotation-1-landscape.png), [180도](screens/rotation-2-landscape.png), [270도](screens/rotation-3-landscape.png).
- [가공 완료](screens/processing-complete.png), [배양 완료](screens/cultivation-complete.png), [실제 입출고](screens/actual-production-and-transfer.png), [건설 중](screens/construction-sites.png).
- [선택](screens/selected-landscape.png), [세로](screens/all-rotations-portrait.png), [영어 큰 글씨](screens/english-large-portrait.png), [상태 OFF](screens/status-off-zoom-1.png).

GIF는 `working-00.png`~`working-07.png`를 Pillow로 duration625ms/loop0으로 조합한 검수 자료다. 게임에 쓰는 원본 PNG는 수정하지 않았다. 2026-11-12 사용자 게임 화면 승인 후 PLAN-029 완료 판정을 기록했다.

## 재현과 복원

열린 Editor의 Unity CLI Pipeline에서 `--project-path C:/Dev/Unity/Doom_Inc/UnityProject --caller plugin --skill unity-cli --json`을 붙여 실행했다.

1. `script recompile` → `script recompile_status` 완료/오류 0 확인.
2. `test run --mode editor --async_tests true`.
3. `test run --mode playmode --filter FacilityAnimation --filter_type category --async_tests true`(OFF/ON 각각).
4. 이름 필터 `FacilityArtKeepsConstructionMoveAndReadableOverlays`, `ApprovedArt`, 카테고리 `MovementPresentation`을 각각 실행.
5. 매 실행 `test status` 완료·실행 수·실패/건너뜀과 XML을 다음 실행 전 보존.

[시작 환경](environment-before.json), [종료 환경](environment-after.json), [복원 처리](restoration.json), [Editor 설정 파일 확인](editor-settings-restored.json), [무결성](integrity-after.json)을 남겼다. Play 정지·Laboratory 씬 clean·Domain Reload OFF·background false·vSync1·fps -1·Android·URP/D3D12로 복원했다. 테스트가 채운 동적 폰트는 TMP API로 빈 문자/1×1 atlas로 복원했다.

EditorSettings API 복원 후 메모리는 OFF였으나 직렬화 파일의 ON 값이 남아, 기존 구조를 확인해 `m_EnterPlayModeOptions` 한 값만 시작값1로 되돌리고 Unity Refresh/메모리·파일을 대조했다. 기존 ProjectSettings·씬·폰트 diff는 없다. 수령 PNG가 이동한 신규 요청 경로를 제외한 기존 파일 259개 중 의도한 SO 4개만 변경됐다. Art 원본/출력과 최종 PNG 해시는 모두 수령 시와 같다. 실제 게임 슬롯 대신 임시 테스트 슬롯만 사용했다.

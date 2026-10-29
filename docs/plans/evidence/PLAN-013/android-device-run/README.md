# PLAN-013 Android 실기기 검증 — 2026-10-29

대상: [PLAN-013](../../../PLAN-013-responsive-ui-and-mobile-input.md). 2026-10-29 사용자가 유예했던 실기기 검증의 재개를 승인하여 실행했다. PLAN은 재개 전부터 유일한 `active`였으며, 이 기록은 존재하지 않았던 `on hold → active` 전환을 의미하지 않는다.

이 실행에서 **오프라인 새 플레이 → 건설·생산·운용 → 방출·보고 → 멸망 연출·결과 → 새 판 재시작**의 실기기 핵심 흐름을 확인했다. 가로·세로 조작과 사용자 실제 제스처 확인 후 18:38 KST에 원래 저장·기기 설정을 복원했다. 기존 PC 검사·APK 빌드와 이 실행 결과를 합쳐 PLAN-013을 completed로 전환했다.

[검증 요약·사용자 응답·선별 원본 30개의 해시](summary.json), [ADB 입력 시각·좌표](actions.jsonl), [최종 로그 발췌](final-log-excerpt.txt)를 함께 보존한다. 이번 재개는 커밋 `6e46eb2e24ff3bcc6d29bdbc18ea802aca74b79c`의 코드·APK를 그대로 사용한 검증과 문서 정리다.

선별 원본의 크기·해시를 유지하도록 저장 스냅샷(`*-save.json`, CRLF)과 `actions.jsonl`은 `.gitattributes`에서 줄바꿈 변환을 제외했다. 스테이징된 Git 내용도 30개 모두 기록된 크기·SHA-256과 대조한다.

## 환경과 증거 범위

| 항목 | 확인 내용 |
| --- | --- |
| 기기 | Samsung Galaxy S10 계열, 모델 SM-G977N, Android 12 / API 31, ARM64 |
| 화면 | 기기 물리 해상도 1440×3040, 설정된 출력 해상도 세로 1080×2280 / 가로 2280×1080 |
| 앱 | `com.DefaultCompany.urp_2d`, 버전 1.0 / code 1, `UnityPlayerGameActivity` |
| APK | 2026-10-28 빌드, Development / IL2CPP / ARM64. 설치 파일 해시는 [앞선 읽기 전용 관찰](../android-20261029-observation.json)에서 기존 산출물과 일치 확인 |
| APK SHA-256 | `e1678afd444741b7e43590707f5b5224324c950023bbca59610d5e60c2f2fe64` |
| 입력 방법 | ADB `input tap`·단일 포인터 `input swipe`·Home/앱 복귀, 사용자 실제 기기 회전·두 손가락 제스처 |
| 상태 확인 | 화면 캡처와 해당 실행의 게임 저장 스냅샷을 함께 확인. 저장 내용 편집으로 결과를 만들지 않음 |
| 원본 증거 | 저장소 루트 기준 `Temp/PLAN-013-device-run-20261029/`. 아래 짧은 파일명은 이 폴더의 원본명을 사용하며, 선별한 공유 증거는 이 README와 같은 폴더에 보존 |

[actions.jsonl](actions.jsonl)은 UTC 시각이며 이 문서의 시각은 KST(UTC+9)다. 원본 폴더는 로컬 검증 자료다. 공유본에는 필요한 화면·검증 스냅샷·로그 발췌만 선별하고 사용자의 원본 저장 파일, 복원 후 사용자 저장 파일, 기기 일련번호와 민감 식별자는 포함하지 않는다.

## 저장 보호와 오프라인 시작

1. 기존 플레이 저장을 별도로 백업·격리하고 새 플레이 상태에서 검증을 시작했다. [actions.jsonl](actions.jsonl)의 18:16:57 백업 기록과 `original.json`의 원래 설정을 보존했다.
2. `original-save.json`의 실제 SHA-256을 다시 계산하여 백업 기록과 일치함을 확인했다: `81ca5bbb9c5f4ba07f76b6587bd986349153498a55067a855c96b6fec8052ee9`.
3. 시작 전 설정은 `wifi=1`, `mobile=1`, `airplane=0`, `autoRotation=0`, `rotation=0`이었다. 18:16:58에 `wifi=0`, `mobile=0`, `airplane=0`을 기록한 뒤 앱을 cold launch했다.
4. [fresh-ready-portrait.png](fresh-ready-portrait.png)에서 연구소·조작 UI·초기 생명체를 확인했다. [fresh-save.json](fresh-save.json)은 기존 저장의 저장소 없이 보조형 프로토타입 1개·유기물 4개인 새 상태이며, 이후 실제 건설·생산 명령을 수행했다.
5. 초기 저장과 후속 스냅샷의 DataHash는 `2578869b88b49ba4855785cf587d8cf2f0cfd0dd8ba1fbfc2cd3fb8d2ca7f195`로 일치했다. 오프라인 조건에서 로컬 데이터 초기화 후 플레이 진입을 확인한 증거다. `GameDataStore Ready` 상태 자체를 별도 로그로 직접 수집한 것은 아니며, 앱의 모든 네트워크 호출 유무를 판정한 것도 아니다.

기존 저장은 보조형 프로토타입·유기물 2개·저장소 1개였다. 새 검증 상태와 분리했고, 검증 완료 후 아래 복원 기록의 해시 비교와 화면 확인으로 원상 복원을 확인했다. 사용자 원본 저장의 내용은 로컬에서만 보관한다.

## 실행하여 확인한 범위

| 시나리오 | 관찰 결과 | 원본 증거 |
| --- | --- | --- |
| 세로 건설과 건설 미리보기 90° 회전·취소 | 채집 시설과 저장소 건설 UI를 사용했다. 저장소 `(6,3) / 90° OK` 미리보기와 `Laboratory paused`를 확인한 뒤 Cancel, 다시 0° 건설을 확정했다. 후속 저장에서 생성·시공 진행 및 완료 확인 | [storage-rotate.png](storage-rotate.png), [actions.jsonl](actions.jsonl), `gather-storage-save.json`, [now-save.json](now-save.json) |
| 세로 시설 이동·회전 확정 | 첫 저장소의 확정 위치 `(6,3), rotation=0`에서 `(6,6), rotation=1`로 변경됐다. 미리보기와 확정된 저장 상태를 구분했다 | [move-rotated.png](move-rotated.png), `gather-storage-summary.json`, `now-summary.json` |
| 세로 기본 생산 흐름 | 채집·저장·가공·배양 시설 4종의 시공 완료를 [now-save.json](now-save.json)에서 확인. 후속 가공 산출 배양액 1개와 배양 시설까지의 운반을 확인 | [now-save.json](now-save.json), `production-progress-save.json`, [waiting-human-save.json](waiting-human-save.json) |
| 생산 대상 선택 | 첫 Support 탭 뒤에는 목표가 반영되지 않았으나 재탭 후 화면 `Next: Support` 및 `selectedTarget=1`을 확인. 첫 입력 미반영의 원인은 확정하지 않음 | `support-target.png`, [now-save.json](now-save.json), `production-progress-save.json` |
| 보급형 배양 시작 | `after-human-gestures-save.json`에서 `selectedTarget=1`, `activeTarget=1`, `inProgress=true` 확인. 해당 시점은 배양 시작이며 새 보급형 생성 완료는 아님 | `after-human-gestures-save.json` |
| 가로 센서 회전·배치 | 사용자가 기기를 가로로 돌렸고 큰 검은 여백이 없다고 확인. 캡처에서 2280×1080의 좌측 맵·우측 패널 배치를 확인 | 사용자 응답, [physical-landscape.png](physical-landscape.png) |
| 가로 건설 확정·완료 | 세 번째 저장소 미리보기·확정 후 공사 표시를 확인. 후속 저장에서 `(6,1), rotation=0`의 저장소가 완료 상태 | [landscape-storage-draft.png](landscape-storage-draft.png), [landscape-zoom-pan.png](landscape-zoom-pan.png), [waiting-human-save.json](waiting-human-save.json) |
| 선택·패널 스크롤·우선순위 변경 | 시설·생명체를 선택하고 패널을 스크롤했다. 프리셋 Haul 값 변경과 후속 저장의 적용값을 확인. [waiting-human-save.json](waiting-human-save.json)의 프리셋과 작업자 값은 모두 `[1,0,1,4]` | `priorities-scroll.png`, `preset-haul-first.png`, `production-progress-save.json`, [waiting-human-save.json](waiting-human-save.json) |
| 저장소 허용 토글 | 두 자원 모두 `blocked`인 화면과 `allowsOrganic=false`, `allowsCulture=false`를 확인한 뒤 허용 상태로 되돌렸다 | [storage-blocked-confirmed.png](storage-blocked-confirmed.png), [storage-blocked-save.json](storage-blocked-save.json), [move-before-rotation-save.json](move-before-rotation-save.json) |
| 버튼 줌·맵 팬 | 두 방향에서 확대 버튼과 단일 포인터 맵 이동을 수행하고 표시 변화를 확인 | `zoom-pan.png`, [landscape-zoom-pan.png](landscape-zoom-pan.png), [actions.jsonl](actions.jsonl) |
| 가로 실제 핀치와 포인터 수 전환 | 사용자가 핀치·두 손가락에서 한 손가락 전환·맵 이동·패널 스크롤이 모두 정상이고 오작동이 없다고 확인. 이 판정의 근거는 사용자 실조작 응답이며 ADB 단일 포인터 입력으로 대체하지 않음 | 사용자 응답, 조작 뒤 `after-human-gestures.png` |
| 일반 Home 후 복귀 | 18:24:56.205 Home, 18:24:56.564 앱 복귀 뒤 연구소 화면을 확인. 약 0.359초의 짧은 전환이므로 장시간 백그라운드 정지·복귀 또는 조작 도중 잔류 터치 취소를 입증하지 않음 | [actions.jsonl](actions.jsonl), `resumed.png` |

18:30 이전의 [waiting-human-save.json](waiting-human-save.json)에는 채집 1·가공 1·배양 1·저장소 3개가 모두 시공 완료였고, 보조형 프로토타입만 1개, 세계 점수는 0이었다. 배양액 1개가 배양 시설에 도달했으며 이후 `after-human-gestures-save.json`에서 Support 배양이 시작됐다. 생성·배정은 아래 후속 증거로 별도 확인했다.

## 18:30~18:35 추가 확인

| 시나리오 | 관찰 결과 | 원본 증거 |
| --- | --- | --- |
| 이동 미리보기 중 가로 → 세로 회전 | `(6,6) / 90° OK`와 `Laboratory paused`가 세로 화면에도 유지됐다. 사용자가 상태 유지를 확인했고 전환 뒤 Cancel을 실행했다. 전후 저장의 `dataHash`, `phase`, `things`, `placements`, `laboratory`는 완전히 같고 phase는 1이었다. `invasion.elapsed`만 경과 시간에 따라 증가 | 사용자 응답, [move-preview-before-physical.png](move-preview-before-physical.png), [move-preview-portrait.png](move-preview-portrait.png), [move-before-rotation-save.json](move-before-rotation-save.json), [move-after-rotation-save.json](move-after-rotation-save.json), [actions.jsonl](actions.jsonl) |
| 세로 실제 핀치·포인터 수 전환 | 사용자가 세로에서 핀치·두 손가락에서 한 손가락 전환·맵 이동·패널 스크롤이 정상이고 의도치 않은 조작이 없다고 확인 | 사용자 응답, `portrait-human-return.png` |
| 누른 상태에서 Home 후 복귀 | 사용자가 화면을 누른 채 Home으로 나가 약 10초 뒤 복귀했고 상태 유지·정상 조작·의도치 않은 조작 없음을 확인. 이는 잔류 입력의 사용자 관찰이며, 정지 시간의 정량 비교는 별도 미확인 | 사용자 응답, `portrait-human-return.png`, `portrait-manual-logcat.txt` |
| 보급형 2개 생성·배정·해제 | 첫 신규 보급형 생성과 배양 시설 배정 후 생산 작업을 저장에서 확인. Unassign 뒤 화면 `Assigned: None`을 확인하고 다시 배정했다. 두 번째 보급형도 생성되어 가공 시설에 배정됨 | `support-created-save.json`, `support-assigned-save.json`, [support-unassigned.png](support-unassigned.png), `support-working-save.json`, [workers-two-save.json](workers-two-save.json) |
| 개별 우선순위 | 신규 보급형 2개 각각 `usesPreset=false`, `[1,0,2,1]`을 확인. 배양·가공 시설에 각각 배정되어 생산 작업을 수행하는 저장 상태와 개별 우선순위 화면 일치 | [support-individual-priority.png](support-individual-priority.png), [second-support-assigned-priority.png](second-support-assigned-priority.png), [workers-two-save.json](workers-two-save.json) |
| 침공형 생성 | [invasion-ready-save.json](invasion-ready-save.json)에 `creature.invader` 1개와 `isExternal=false`, 세계 점수 0을 확인. 이 대기 상태에서 아래 방출·외부 활동 검증으로 진행 | [invasion-ready-save.json](invasion-ready-save.json), `invasion-ready.png` |

## 회전 증거에서 제외한 시도

- `fresh-ready-landscape.png`는 파일명과 달리 실제 캡처가 1080×2280 세로다. 가로 전환 통과 증거가 아니다.
- Android 설정으로 강제한 `landscape-forced.png`·`landscape-settled.png`에는 좌우 큰 검은 여백이 생겼다. 강제 설정 시도와 실제 센서 회전을 구분하고, 이 화면을 정상 가로 표시의 증거로 사용하지 않는다.
- 정상 가로 배치 판정은 이후 사용자가 직접 돌린 기기와 [physical-landscape.png](physical-landscape.png)를 근거로 한다. 강제 설정의 여백이 생긴 정확한 원인과 일반 센서 회전의 결함 여부를 같은 것으로 단정하지 않는다.
- `four-facilities-save.json`·`second-storage-save.json` 등 이름만으로 시설 수나 시공 완료를 판정하지 않았다. `storage-disallowed.png`도 실제 표시가 둘 다 `allowed`여서 토글 성공 증거에서 제외하고, 후속 [storage-blocked-confirmed.png](storage-blocked-confirmed.png)와 저장 값을 사용했다.
- `ending-phase-3.png`는 OS 방향 전환 애니메이션 도중 캡처다. 안정된 결과 화면 판정에는 회전이 끝난 [results-portrait-settled.png](results-portrait-settled.png)를 사용했다.

## 방출·한 판 종료·재시작

| 시나리오 | 관찰 결과 | 원본 증거 |
| --- | --- | --- |
| 방출·외부 활동·보고 표시 | 18:35:20 방출 버튼과 보고 버튼을 사용했다. [report-activity.png](report-activity.png)에 `World 36 / 120`, `Outside 2`, 상세 보고와 +2·+10 점수 기록이 표시됐다. 사용자가 회전 뒤에도 보고가 유지된다고 확인 | [actions.jsonl](actions.jsonl), `invasion-released-reports.png`, [report-activity.png](report-activity.png), 사용자 응답 |
| 목표 도달·멸망 연출 | 18:36:41의 저장은 `phase=2`, 세계 점수 128, 완료 활동 32, 경과 시간 약 1156.6초였다. 가로 화면에서 행성 그림과 `Final sequence in progress...` 연출을 확인 | [ending-phase-2-save.json](ending-phase-2-save.json), [ending-phase-2.png](ending-phase-2.png) |
| 연출 중 회전·결과 | 18:36:45 저장은 `phase=3`으로 전환됐다. 사용자가 연출 중 회전 후 결과 화면이 정상이라고 확인. 안정된 세로 결과 화면에 128 / 120·활동 32·외부 침공형 2·1156.6초·새 판 시작 버튼이 표시됐다. 결과 대기 저장도 같은 점수·활동·경과 시간을 유지 | 사용자 응답, [ending-phase-3-save.json](ending-phase-3-save.json), `results-stable-save.json`, [results-portrait-settled.png](results-portrait-settled.png) |
| 새 판 재시작 | 18:37:30 결과 화면의 `START A NEW RUN`을 눌렀다. 저장은 `phase=1`, 점수·활동·경과 시간 0, 보조형 프로토타입 1개·유기물 4개로 초기화됐다. DataHash는 그대로이고 이전 판의 Thing ID는 하나도 남지 않았다. 연구소 화면 재표시 확인 | [actions.jsonl](actions.jsonl), [restarted-save.json](restarted-save.json), [restarted.png](restarted.png) |

## 로그와 검증 한계

최종 `final-logcat.txt`는 검증 게임 프로세스의 18:17:00.144~18:37:31.896 로그 1,255줄이다. 이 범위에서 Unity Error/Fatal, `Exception`, `FATAL EXCEPTION`, `ANR in` 표식은 각각 0건이었다. 원본 복원 후 재실행은 다른 프로세스이므로 이 최종 검증 로그의 수집 범위와 구분한다.

Warning 5건(hidden API 2, Gralloc3 2, Gralloc4 1), Error 32건(libprocessgroup `set_timerslack_ns` 권한 17, SwappyDisplayManager `libgame.so` 13, USNET appName 1, BufferQueueProducer 1)이 있었다. 이 실행에서 플레이를 막는 오류는 관찰되지 않았지만, native 메시지들의 성능·장기 실행 영향은 계측하지 않았다. “오류 0”이나 “무해”로 판정하지 않으며 공유용 최종 발췌에 원문을 남긴다.

다음 한계는 기능 확인 결과와 별도로 유지한다.

- 사용자 약 10초 Home 복귀·상태 유지·잔류 입력 없음은 확인했지만, 백그라운드 시간 정지의 전후 수치를 정량 비교하지 않았다.
- 지속 FPS·발열·메모리·배터리 성능은 계측하지 않았다. 성능 검증은 후속 범위다.
- 단일 Android 기기 확인이므로 다른 기종·다른 노치·가로 반대 방향까지 일반화하지 않는다.
- 중첩 개체 선택·개별 우선순위 검사와 양방향 개별 배정·오류 주입·이전 이벤트 검사는 기존 PC 통과 증거를 사용한다. 이 실행에서 실기기의 중첩 상태나 실패 상태를 별도로 재현했다고 기록하지 않는다.

## 원래 상태 복원 기록

| 대상 | 복원 기준 | 확인 결과 |
| --- | --- | --- |
| 사용자 게임 저장 | 원본 SHA-256 `81ca5bbb9c5f4ba07f76b6587bd986349153498a55067a855c96b6fec8052ee9` | 18:38:02 [actions.jsonl](actions.jsonl)의 `restored` 기록에서 복원 직후 해시가 원본과 완전히 일치 |
| Wi-Fi / 모바일 데이터 / 비행기 모드 | `1 / 1 / 0` | 복원 후 직접 조회와 `restored` 기록 일치 |
| 자동 회전 / 사용자 회전값 | `0 / 0` | 복원 후 직접 조회와 `restored` 기록 일치. 고정 회전 모드도 `default` 확인 |
| 원본 플레이 재진입 | 보조형 프로토타입 1개·유기물 2개·저장소 1개 | 18:38:10 재실행, 18:38:49 [original-restored-ready.png](original-restored-ready.png)에서 원래 상태 재표시 확인 |
| 검증 상태 보관 | 사용자 원본과 검증 상태 분리 | 결과·재시작 저장과 최종 검증 로그를 분리 보존. 사용자 원본 저장·복원 후 저장은 공유본에서 제외 |

복원 직후 바이트 일치는 재실행 전에 확인했다. 이후 원본 플레이를 다시 실행하면 자동 저장으로 해시가 바뀔 수 있으므로, 복원 직후 해시와 재실행 후 화면·상태 확인을 구분했다.

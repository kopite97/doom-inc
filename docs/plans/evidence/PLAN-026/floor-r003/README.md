# PLAN-026 바닥 r003 적용 증거

2026-11-07. 사용자의 “r003 바닥 시트를 64×64로 슬라이싱하고, 4가지 타일을 섞어서 연구소 바닥에 적용” 요청에 따른 바닥 부분 실행이다.
[PLAN](../../../PLAN-026-art-style-scale-and-sample-integration.md)과 [요청 README](../../../../../UnityProject/Assets/Sprites/Require/PLAN-026/README.md)가 범위·수령·사용자 확인 상태의 원본이다.

## 적용과 환경

- Unity 6000.3.27f1, Windows Editor, 기존 uGUI/URP와 패키지 유지. CLI는 공식 self-update로 1.0.0-beta.13에서 확인된 최신 1.0.0-beta.17로 갱신했다. Editor/pipeline 패키지는 변경하지 않았다.
- 최종 파일: [LaboratoryFloorSet.png](../../../../../UnityProject/Assets/Sprites/World/Laboratory/LaboratoryFloorSet.png). Multiple, 256×64 시트에서 `(0,0)`, `(64,0)`, `(128,0)`, `(192,0)`의 64×64 Rect. 순서는 기본·질감·긁힘·얼룩이다.
- PPU 32, 피벗 `(0.5,0.5)`, Full Rect, Point, Clamp, sRGB, alpha transparency, NPOT None, 압축/Mipmap/ReadWrite 없음, Android/Standalone/iPhone/WebGL override 없음.
- Sprite Editor data provider의 생성·이름·Rect·피벗·Border 편집 capability를 확인하고 공식 API로 슬라이싱했다. 기존 기본 Sprite ID와 texture GUID `01104bcccd5217245a27f0ce9bb47fb4`를 보존해 AssetDatabase로 Require에서 정식 폴더로 이동했다.
- Art r003 제출 PNG와 최종 PNG의 SHA-256은 모두 `4C09D407BC3E5EA4ECC26B3672C3F798AB02A97FBCF1558F57C9859E7202B615`다. 원본 픽셀·색·크기·Aseprite 원본은 변경하지 않았다.
- `LaboratoryUiAssets`의 네 Sprite를 연구소 씬과 AppConfiguration에 연결했다. `LaboratoryUiAssetBuilder.CreateMissing()` 재실행에서도 같은 네 이름/참조가 복원됐다.
- 지도 인스턴스가 바닥 Image 레이어를 소유한다. 기존 표시의 아래에 놓고 raycast를 끄며, 원본 색을 그대로 표시하고 바닥 사이의 2px 간격을 없앴다. 기존 점유·선택·건설·상호작용 색상/문자는 그 위에 남는다. 새 전역 상태·이벤트·게임 저장 필드는 없다.
- 좌표 정수 해시의 70/10/10/10 가중치를 사용한다. [실제 분포](distribution.txt)는 16×10 지도에서 기본 116, 질감 9, 긁힘 14, 얼룩 21이다. 작은 지도에서 정확한 비율을 강제하지 않는다. 재색칠·회전·반전은 없다.

## 검증

| 대상 | 실행·결과 | 증거 |
| --- | --- | --- |
| 임포트·공용 참조 | Edit 1/1 통과 | [edit-floor.json](edit-floor.json) |
| 기존 제스처 로직 | Edit 13/13 통과 | [edit-gestures.json](edit-gestures.json) |
| 기존 UI 레이아웃 | Edit 12/12 통과 | [edit-layout.json](edit-layout.json) |
| 바닥 실제 씬 | Domain Reload OFF·ON 각각 Play 1/1 통과 | [OFF](play-floor-off.json), [ON](play-floor-on.json) |
| 실제 입력 장치 이벤트 | 드래그·두 손가락 확대·남은 손가락 해제·다음 탭 Play 1/1 통과 | [play-device-gesture.json](play-device-gesture.json) |
| Balance 원본 | 5개 JSON 전후 SHA-256 모두 동일 | [balance-hashes.json](balance-hashes.json) |
| 최종 Editor | 컴파일 실패 없음, Console error 0 / warning 0 | [final-console.json](final-console.json) |

실제 바닥 검사는 네 종류·단일 texture·백색 tint·비차단 Image·160칸·연속 배치와 동일 좌표 Sprite를 확인한다. 가로/세로 및 zoom 1/3/8 전환 후 전체 세션 스냅샷 동일, Draw 전후 Unity Random.state 동일, 선택·건설 미리보기 입력, 기본 타일만 제공/전부 누락 시 대체 표현, 복원 시 저장 파일 동일, 씬 재진입·새 판 후 동일 무늬와 레이어 한 개를 확인했다.

실행 명령은 `unity command test run --mode editor|playmode --filter <위 테스트명 또는 클래스명> --filter_type testName --async_tests true --project-path C:/Dev/Unity/Doom_Inc/UnityProject --caller plugin --skill unity-cli --format json`이며 `test status`의 실제 실행 수·결과를 확인했다. 재컴파일은 `script recompile`과 `script recompile_status`를 사용했다.

초기 여러 클래스의 `|` 필터는 이 CLI에서 지원되지 않아 0개가 발견됐고 통과로 세지 않았다. 클래스별 재실행으로 위 26개를 확인했다. [최초 Play 실패](play-initial-failure.json)는 비활성 부모에서 얻은 자식 컴포넌트 배열이 비어 있어야 한다는 테스트 가정이었다. 실제 렌더링 활성 상태를 직접 확인하도록 바꾸고, 누락 시 기존 칸 색상·입력도 함께 검사해 통과했다. 제품 코드를 우회하거나 실패 검사를 제거하지 않았다. 이후 ON 실행에는 기본/혼합을 같은 무선택 상태에서 캡처하는 보완과 실제 분포 기록을 추가했다.

## 실제 화면

모두 Editor Game View의 격리된 새 게임, 16×10 지도·프로토타입 1개체·유기물 4·시뮬레이션 속도 0이다. 시설/개체/자원 신규 그림은 아직 없다. 실제 휴대폰 검증 결과가 아니다.

| 조건 | 전체 보기 1× | 중간 확대 3× | 최대 확대 8× |
| --- | --- | --- | --- |
| 가로 1280×720 | [혼합](mixed-1280x720-zoom1.png) | [혼합](mixed-1280x720-zoom3.png) | [혼합](mixed-1280x720-zoom8.png) |
| 세로 720×1280 | [혼합](mixed-720x1280-zoom1.png) | [혼합](mixed-720x1280-zoom3.png) | [혼합](mixed-720x1280-zoom8.png) |

[기본만 반복한 비교 화면](base-only-1280x720-zoom1.png)은 가로 전체 보기 혼합 화면과 같은 상태·카메라다. [건설 미리보기](construction-preview.png)는 `(3,3)`의 획득시설 2×2칸·접근 칸·선택 개체를 기존 색/문자로 표시한다.

에이전트가 8개 화면을 직접 확인했다. 바닥 사이 빈틈·진한 격자선 없이 무늬가 섞이고 확대 시 픽셀은 선명하며 viewport 밖은 잘린다. 혼합 화면에는 옅은 긁힘·얼룩이 추가되고 기본만 반복한 화면은 더 균질하다. 최종 대비·반복감·스타일의 사용자 승인은 아직 없다. 같은 무늬가 이웃하는 것을 금지하는 규칙은 추가하지 않았다.

## 복원과 남은 범위

- 사용자 진행 저장을 사용하지 않고 테스트용 임시 슬롯을 생성·삭제했다. 테스트의 입력 장치·Game View 크기·백그라운드 설정을 finally/Dispose로 복원했다.
- Editor는 원래 Bootstrap 씬·dirty false·Play 종료·runInBackground false·enterPlayModeOptionsEnabled true·DisableDomainReload·playModeStartScene null로 복원했다.
- 생성 도구 검증으로 생긴 기존 Temp 아이콘의 불필요한 플랫폼 설정 재직렬화와 TMP 동적 glyph 캐시는 작업 전 clean 상태를 확인한 파일만 복원했다. 연구소 씬의 AudioListener는 저장 순서만 달라지고 객체/필드 값은 그대로다.
- 바닥 부분 기술 검증 완료다. PLAN-026과 5-1은 미완료이며 유일한 active를 유지한다. 바닥 사용자 시각 확인, 나머지 시설·프로토타입·유기물·Build의 제공·적용·전체 조합 검수가 남는다.
- Android APK/콘텐츠·Player/IL2CPP 빌드·기기 설치·기기 성능 검증은 실행하지 않았다. 과거 PLAN-025 결과를 새 아트 결과로 사용하지 않는다.

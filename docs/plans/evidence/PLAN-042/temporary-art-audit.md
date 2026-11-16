# 임시 표현 참조 점검

[직렬화 참조 조사](temporary-reference-scan.json): SO/씬/프리팹/Settings 의존성을 AssetDatabase로 대조.

- 공식6아이콘 연결 후 무참조 Temp_ZoomOut/ZoomIn/FitMap/Creatures/Invasion/More 삭제.
- 공식 타이틀2종 연결 후 무참조 Temp_NightShiftLandscape/Portrait 삭제. 생성 도구는 정식 참조를 사용하며 옛 타이틀 분할 참조 코드를 제거.
- 사용하지 않는 LaboratoryUiWorld 절차적 그림 코드 제거. Ending.Progress는 수령 Sprite32프레임을 사용.
- Temp_Construction/Material/Delivery/Worker/Automatic/Blocked/SelectTarget/ShowStatus/HideStatus는 기존 icons 배열/IsComplete 계약의 직렬화 참조가 남아 보존했다. 현행 수동 상태바에 이 아이콘을 다시 표시하지 않는다. enum/저장 호환 정리는 별도 범위다.
- Temp_LaboratorySolid/Body는 미제공 정의를 위한 명시적 fallback. 승인된 실제 시설/생명체4종은 정식 Sprite를 사용한다. 누락 아트 기능 회귀를 위해 fallback 자체는 보존.
- 기능상 단색 영역(전체 배경, 입력 shield, Fill, 배치 충돌 셀, 작은 진행바)은 역할상 유지. 이름의 Temp만 제거해 정식 아트로 둔갑시키지 않았다.
- 기존 씬/프리팹 Temp 파일명은 GUID/경로 계약을 보존. 완성된 PLAN 문서·과거 증거는 수정하지 않음.

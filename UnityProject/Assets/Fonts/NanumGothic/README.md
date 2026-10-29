# Nanum Gothic 한국어 UI 폰트

- 용도: PLAN-014의 임의(Temp) uGUI 한국어 표시. 기존 `UnityEngine.UI.Text`를 유지하며 시스템 폰트 설치에 의존하지 않는다.
- 파일: [Temp_NanumGothic-Regular.ttf](Temp_NanumGothic-Regular.ttf). Google Fonts의 정적 Regular 원본과 동일한 바이트이며 파일명에만 임시 표식을 추가했다. 글리프·내부 글꼴 이름·폰트 데이터를 수정하지 않았다.
- 원본: [Google Fonts Nanum Gothic](https://github.com/google/fonts/tree/133ccbee9a8b408eb71f31a36ccb9116f5c695ad/ofl/nanumgothic). 고정한 저장소 커밋은 `133ccbee9a8b408eb71f31a36ccb9116f5c695ad`다.
- 원본 다운로드: [NanumGothic-Regular.ttf](https://raw.githubusercontent.com/google/fonts/133ccbee9a8b408eb71f31a36ccb9116f5c695ad/ofl/nanumgothic/NanumGothic-Regular.ttf).
- 라이선스: [OFL.txt](OFL.txt), SIL Open Font License 1.1. 원본 저작권·라이선스를 그대로 동봉했다. 개별 폰트 판매용이 아닌 게임에 포함하는 사용이며 정식 사용자 제작 아트를 대신하지 않는다.
- 원본 라이선스: [공식 OFL 고지](https://raw.githubusercontent.com/google/fonts/133ccbee9a8b408eb71f31a36ccb9116f5c695ad/ofl/nanumgothic/OFL.txt).
- 폰트 SHA-256: `76f45ef4a6bcff344c837c95a7dcc26e017e38b5846d5ae0cdcb5b86be2e2d31`.
- 라이선스 SHA-256: `eeacf16032901d0ed0456876ec77b8f0fda6b3fecec7d972f8543eb602e6c30f`.
- 2026-10-29 정적 확인: 파일 크기 2,054,744바이트. TTF `cmap`에서 현대 한글 완성형 11,172자와 주요 UI 문구의 글리프를 확인했다. 이 결과는 Unity/Android의 실제 렌더링 통과를 뜻하지 않는다.
- 연결: `LaboratoryUiAssetBuilder.CreateMissing()`에서 Dynamic·Include Font Data를 설정하고, Font와 OFL TextAsset을 `LaboratoryUiAssets`에 전달한다. 직렬화된 씬 참조로 플레이어 빌드에 포함하며 UI의 글꼴 라이선스 열람 경로로 고지를 제공한다.
- 2026-10-29 실행 확인: Unity 임포트·씬 참조·글리프를 포함한 전체 Play 34/34와 양방향 화면을 확인했다. SM-G977N Android APK에서도 보통/큰 글씨·선택 패널·보고·결과의 한국어 표시를 확인했다. 상세 범위와 증거는 [PLAN-014](../../../../docs/plans/PLAN-014-mobile-ui-ux-and-facility-status.md)를 따른다. 정식 UI 테마/폰트로 바꾸는 경우 해당 별도 art PLAN에서 라이선스와 남은 임시 참조를 함께 확인한다.

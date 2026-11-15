# All In 1 Sprite Shader 사용 지침

적용: Sprite 외곽선·색상·투명도·Shine·Dissolve·Hologram 등 재질 효과.
탐색: [외부 연출 에셋 MAP](../presentation-effects.md), [아트 지침](../art-assets.md), [Unity 에셋](../unity-assets.md), [검증](../verification.md).

## 확인 기준과 범위

- 2026-11-15 설치된 4.71의 소스·예제와 제작사 문서를 기준으로 작성했다. 설치 위치는 `UnityProject/Assets/Plugins/AllIn1Assets/`이며 `AllIn1SpriteShader`와 `AllIn1Shared`를 함께 사용한다.
- 사용자는 에셋 사용을 선택하고 URP 자동 구성 창에서 Cancel 후 Import를 마쳤다. 이는 효과의 게임 적용·Play 검증 완료를 뜻하지 않는다.
- 현재 선택 외곽선의 적용 방식과 Editor 검증은 [PLAN-030](../../plans/PLAN-030-sprite-selection-outline-and-deselect.md)에 기록한다. 불변 선택 재질을 공유하고 본체의 재질 참조만 교체·복원하며 원본 공유 재질의 값을 수정하지 않는다.
- 버전 변경 시 실제 메뉴·프로퍼티·키워드·셰이더 변형을 재확인한다. 아래 효과 예시는 사용 방법이며 새 기능 구현 승인이나 완료 증거가 아니다.

## 기본 사용 절차

1. 대상 Renderer와 조명·UI 마스크 필요 여부를 확인하고 아래 기준으로 셰이더를 고른다.
2. Project 창의 `Create > AllIn1Shader Materials > CreateDefaultMaterial`로 재질을 만들고 필요한 Shader를 지정한다.
3. 프로젝트 소유 재질은 [프로젝트 구조](../project-structure.md)의 `Materials/` 아래 기능별 위치에 저장한다. 외부 원본 폴더에 게임용 재질을 추가하지 않는다.
4. SpriteRenderer의 Material에 연결하고 Material Inspector에서 필요한 효과만 활성화한다.
5. 게임 상태에서 `Material.SetFloat`, `SetColor`, `SetTexture`로 값을 제어한다. 프로퍼티 이름은 Inspector 툴팁과 설치된 셰이더 소스에서 확인한다.
6. 대상별 값 분리·재질 정리·애니메이션 프레임 갱신을 연결한 뒤 실제 화면에서 확인한다.

`AllIn1Shader` 컴포넌트는 재질 생성·복사·저장·Atlas 설정을 돕는 편의 도구이며 런타임 필수 컴포넌트가 아니다.
프로젝트의 기존 커스텀 재질을 항상 자동 교체한다고 가정하지 말고 할당 결과를 확인한다. 프리팹·씬에 사용할 재질은 파일로 저장한다.
`Tools > AllIn1 > Sprite Shader Window`에서 저장 경로와 노멀맵·그라디언트·Atlas·노이즈·RGBA 텍스처 제작 도구를 확인할 수 있다. 필요 없는 생성 도구는 실행하지 않는다.

## 셰이더 선택

Shader 이름의 공통 접두사는 `AllIn1SpriteShader/`다.

| 변형 | 사용 기준 |
| --- | --- |
| `AllIn1SpriteShader` | 조명 영향을 받지 않는 기본 Sprite 효과. 현재 Unlit 월드의 우선 검토 대상 |
| `AllIn1Urp2dRenderer` | URP 2D 조명·노멀맵이 필요한 대상 |
| `AllIn1SpriteShaderUiMask` | 마스크가 필요한 UI Image |
| `AllIn1SpriteShaderScaledTime`·`AllIn1SpriteShaderUiMaskScaledTime` | `SetGlobalTime`이 제공하는 unscaled 시간으로 자동 효과를 움직일 때 |
| `AllIn1SpriteShaderLit`·`AllIn1SpriteShaderLitTransparent` | 3D 조명 표현이 필요한 대상 |
| `AllIn1SpriteShaderSRPBatch` | 해당 파이프라인과 효과에서 적합성을 확인한 경우. 이름만으로 성능 개선을 단정하지 않음 |

- URP 사용만을 이유로 2D 조명용 변형을 선택하지 않는다. 현재 Unlit 표현을 보존하고 필요한 조명만 별도 검토한다.
- 기본 외곽선 때문에 렌더러·Color Space·Bloom을 일괄 변경하지 않는다. 자동 구성의 변경 대상과 현재 Renderer 종류를 먼저 확인한다.

## 선택 외곽선

| Inspector 설정 | 제어 값·초기 검토 기준 |
| --- | --- |
| Outline | `OUTBASE_ON` 활성화 |
| Outline is Pixel Perfect? | `OUTBASEPIXELPERF_ON` 활성화, `_OutlinePixelWidth`는 1부터 비교 |
| Outline Color / Alpha | `_OutlineColor`, `_OutlineAlpha`; 선택 1·해제 0으로 표시 제어 가능 |
| Outline Glow | `_OutlineGlow`는 일반 단색 강조에서 1부터 비교 |
| Outline High Resolution? | `OUTBASE8DIR_ON`; 4방향과 8방향 외형을 비교해 결정 |
| Only render outline? | `ONLYOUTLINE_ON`; 본체와 함께 그리는 재질에서는 끄기 |

- 4.71의 위 옵션은 키워드로 동작한다. 오래된 예제의 `_OutlineIsPixel`·`_Outline8Directions`·`_OnlyOutline` Float만 바꿔 동작한다고 가정하지 않는다.
- 기본 Outline은 현재 프레임의 투명도 경계를 읽으며 내부 투명 영역의 경계에도 선이 생길 수 있다. 엄밀한 최외곽 실루엣만 필요하면 실제 아트로 확인한다.
- Pixel Perfect 두께는 원본 텍스처 픽셀 기준이다. 화면상 고정 픽셀 두께를 보장하지 않으므로 확대·축소 결과를 확인한다.
- Full Rect와 투명 여백을 확인한다. 두꺼운 외곽선은 사각형 경계에서 잘리거나 시트의 이웃 프레임을 읽을 수 있다. 원본 아트·PPU·필터 변경은 아트 지침을 따른다.
- 제공되는 Unified Outline 도구는 복사 Sprite를 만드는 방식이며 동적 프레임·운반물 변경의 지속 동기화를 자동 보장하지 않는다. 복합 대상은 본체·부속물 포함 범위를 정하고 연결한다.

## 시트·Atlas와 프레임 갱신

- 단색 기본 Outline만을 위해 Atlas UV 보정을 무조건 추가하지 않는다. Shine·패턴·UV 변형처럼 개별 Sprite의 0~1 좌표가 필요한 효과에서 검토한다.
- 제공 도구의 Sprite Atlas Auto Setup은 `SetAtlasUvs`와 `ATLAS_ON`을 설정한다. 프레임이 바뀌면 `_MinXUV`·`_MaxXUV`·`_MinYUV`·`_MaxYUV`도 갱신해야 한다.
- `SetAtlasUvs`는 기본적으로 sharedMaterial에 쓴다. 서로 다른 프레임의 Renderer가 같은 재질 값을 덮어쓰지 않도록 분리한다.
- `useMaterialInstanceIfPossible`은 Play 중 SpriteRenderer의 개별 재질 사용 옵션이다. UI의 재질 분리와 생성된 재질 정리를 대신하지 않는다.
- 기본 `Update Every Frame` 갱신은 LateUpdate다. 현재 게임도 LateUpdate에서 Sprite를 교체하므로 보정이 필요한 효과는 프레임 교체 직후 갱신하거나 순서를 명시한다.
- Atlas UV 정규화가 외곽선의 이웃 프레임 샘플링까지 차단하지는 않는다. Rect Size·Polar Coords 등 효과별 Atlas 제한도 확인한다.

## 재질·효과·시간의 소유

- 기존 공유 월드 재질을 변경해 선택 대상을 표현하지 않는다. 개별 재질 또는 적용 가능성을 확인한 MaterialPropertyBlock 등으로 대상별 값을 분리한다.
- `Renderer.material`의 인스턴스 생성과 sharedMaterial 수정 범위를 구분한다. 런타임 소유 재질은 캐시·재사용하고 소유 대상 종료 시 해제한다. 원본 재질 에셋을 Destroy하지 않는다.
- UI Image는 재질 공유·Mask용 `materialForRendering` 동작을 별도로 확인한다. SpriteRenderer의 분리 방법을 그대로 가정하지 않는다.
- 필요한 키워드 조합은 저장된 재질에 준비하고 런타임에는 강도·색 등 값을 제어하는 방식을 우선한다. 빌드에 없는 키워드 조합을 실행 중 켜면 셰이더 변형 누락이 생길 수 있다.
- 효과 강도를 0으로 만들어도 활성 키워드의 연산 비용이 제거되는 것은 아니다. 모든 개체에 모든 효과를 켜 두지 않는다.
- `_Alpha`는 전체 투명도, `_ShineLocation`은 Shine 위치, `_HitEffectBlend`는 Hit 강도, `_FadeAmount`는 Fade 진행에 사용한다. 형태가 있는 Dissolve에는 적절한 `_FadeTex`가 필요하다.
- [DOTween](dotween-pro.md)은 값의 시간 변화, [Feel](feel.md)은 사건 피드백, 이 에셋은 셰이더 표현을 맡도록 조합한다.
- 기본 셰이더 시간과 `SetGlobalTime`의 unscaled 시간은 현재 게임의 자체 시뮬레이션 정지·배속을 자동 반영하지 않는다. UI 강조와 생산 연출의 시간 정책을 나누고 필요하면 외부에서 진행값을 제어한다.

## 확인 항목과 근거

- 대상 간 값 독립성, 선택·해제·대상 삭제·재사용 시 원래 표현 복원을 확인한다.
- 애니메이션 프레임·방향 전환, 줌, Sprite 경계·투명 구멍, 정렬·가림에서 의도한 외형을 확인한다.
- 일시정지·배속·씬 종료 시 자동 효과와 재질 수명을 확인한다. Editor 결과를 빌드·실기기 검증으로 확대하지 않는다. 실행 범위는 [검증 지침](../verification.md)을 따른다.
- 로컬 근거: 설치 루트 아래 `AllIn1SpriteShader/Scripts/Editor/AllIn1SpriteShaderMaterialInspector.cs`, `Scripts/SetAtlasUvs.cs`, `Scripts/SetGlobalTime.cs`, `Scripts/All1CreateUnifiedOutline.cs`, `Shaders/AllIn1SpriteShader.shader`. 뒤 네 경로도 `AllIn1SpriteShader/` 기준이다.
- 제작사 문서: [기본 사용](https://seasidestudios.gitbook.io/seaside-studios/sprite-shader/first-steps-must-read), [코드 제어](https://seasidestudios.gitbook.io/seaside-studios/sprite-shader/scripting), [Sprite Atlas](https://seasidestudios.gitbook.io/seaside-studios/sprite-shader/sprite-atlases).

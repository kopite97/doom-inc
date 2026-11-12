# TECH-019 시설 아트와 성공 사건 관찰

- 적용: [PLAN-029](../plans/PLAN-029-laboratory-processing-cultivation-storage-art.md), 2026-11-12.
- 기준: [객체 설계](../agents/object-design.md), [아키텍처](../agents/architecture.md), [추상화](../agents/abstraction.md).

가공/배양 Complete와 저장소 Receive/Send를 정확한 성공 사건에 맞춘다. 재고 총량 비교는 같은 프레임의 입출고를 잃고, 생성·복원·병합을 실제 운반으로 오인할 수 있다.

- `ThingRegistry.ResourceTransferred(source, destination, amount)`는 검증 후 실제 전체/부분 소유 이전이 끝난 곳에서만 발행한다. 동일 소유자·생성·병합·소모는 제외한다.
- `LaboratoryManager.ProductionCompleted`는 가공 산출 또는 배양 개체 생성·배치 성공 후 발행한다. 산출물 보관 대기는 새 성공이 아니다.
- 기존 SO에 시설 정의별 16개 Sprite와 프레임 시간을 보관한다. 정의의 첫 Idle Sprite를 기존 목록 아이콘에도 재사용한다. 새 전역 저장소나 범용 이벤트 버스는 만들지 않는다.
- 화면의 시설별 `FacilityAnimation`이 현재 반응과 최신 대기 반응 최대 한 개를 소유한다. 경과 시간은 시뮬레이션 시계, Working 위상은 실제 생산 진행률로 계산한다. 자동 생산 20%와 일시정지를 그대로 반영한다.
- 화면은 세션 교체·해제 시 구독과 캐시를 버린다. 저장/복원은 애니메이션 반응을 저장하거나 재생하지 않는다. 게임 규칙·수량·시뮬레이션 처리 순서는 바꾸지 않는다.
- Sprite 임포트는 Unity Sprite Editor 데이터 공급자 API를 쓰고 이름별 Sprite ID와 수령 GUID를 유지한다. 생성 도구 재실행도 같은 바인딩을 보장한다.

검증은 실제 이전 방향·수량, 자동 생산 완료, 출력 대기 중 재생 억제, 정지/복원/해제, 네 방향과 입력을 대상으로 한다. 결과와 게임 화면 승인은 PLAN에서 관리한다. 범용 애니메이터·재고 이벤트 큐·실제 재고 변경을 애니메이션에서 호출하는 대안은 이 범위에 필요하지 않아 도입하지 않는다.

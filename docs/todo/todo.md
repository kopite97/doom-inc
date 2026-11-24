# 앞으로 할 작업

이 문서는 앞으로 해야 할 작업을 간단히 정리하는 목록이다. 상세 기획·확정 규칙·구현 및 검증 기록은 각 담당 문서에서 관리하며, 이 목록 자체는 기능 채택이나 개발 착수 승인을 뜻하지 않는다.
세부 미정 사항은 [미정 사항 색인](../game-design/management/open-questions.md)을 참고한다.
게임 비전부터 출시·운영까지 분야별 기존 자료와 준비할 결과물은 [게임 개발 사전준비 점검표](pre-production.md)에서 확인한다.

## 먼저 마무리할 주요 기획

- [ ] 각 특화 계통의 세부 연구와 선행 관계, 연구로 해금할 생명체·시설을 정하고 조수 전문성·분석 장비·활동 보너스 조건을 연결한다. [연구](../game-design/systems/research.md)
- [ ] 장기 성장: 행성 전체의 침공 완료에 이르는 초반·중반·후반의 목표와 성장 체감, 새로운 생명체·시설·연구·자원 공급의 연결, 반복 플레이를 정한다. [성장](../game-design/experience/growth.md)
- [ ] 위협과 외부 세력: 토벌군·운영 사건의 종류, 발생 원인·예고·강도 상승·대응 기회와 외부 세력의 반응을 정한다. [방어](../game-design/systems/defense.md) · [사건](../game-design/systems/events.md) · [외부 세력](../game-design/systems/external-world.md)
- [ ] 패배와 복구: 방어 실패·시설 손실·핵심 개체 부재의 영향, 재건 수단과 회복 불가능한 패배·게임 종료 조건을 정한다. [방어](../game-design/systems/defense.md) · [유지와 손실](../game-design/systems/maintenance-and-loss.md)
- [ ] 외부 침공: 추후 목표·출격 준비·전장·승패·손실·귀환·자원 획득과 본진 동시 운영을 상세 기획한다. [침공 구상](../game-design/systems/external-world.md#외부-침공-구상)

## 함께 보완할 기획

- [ ] 초기 근접·원거리 전투의 실제 개체 구성과 공격·탐지 상실·추격·대피 세부 규칙을 정한다. [전투](../game-design/systems/defense.md)
- [ ] 커맨더의 수·체계·지휘 범위, Low/Medium/High의 작업 배분 기준과 부재 시 운영을 정한다. [커맨더](../game-design/systems/commanders.md)
- [ ] 맵별 자연 자원과 채취 가능한 생명체, 바닥·벽에 고를 수 있는 실제 재료와 지정 재료가 필요한 생산시설을 정한다. 생명체·시설 해금, 첫 연구 보상·첫 습격 준비와 중반 이후 공급 수단을 연결한다. [생명체](../game-design/systems/creatures.md) · [자원](../game-design/systems/resources.md) · [초반 흐름](../game-design/experience/early-game.md)
- [ ] 맵의 크기·지형·자원 배치와 확장 이유를 구체화한다. 기후·계절은 추후 도입 여부부터 논의한다. [맵](../game-design/systems/maps.md)
- [ ] 시설·실내·장비 손상, 건설 취소·철거, 자원 부족과 긴급 작업 충돌에서 플레이어가 겪을 결과를 정한다. [실내](../game-design/systems/rooms.md) · [건설](../game-design/systems/construction.md) · [작업](../game-design/systems/work.md)
- [ ] PC 화면 배치·일괄 설정·명령 접근·상태와 실패 이유 표시, 일시정지·배속·저장 및 재개 경험을 정한다. [화면](../game-design/experience/interface.md) · [게임 방향](../game-design/direction/overview.md)
- [ ] Windows용 Steam 첫 출시의 콘텐츠 범위와 후반 확장 후보를 구분한다. 구체적인 밸런스 수치는 개발 중 조정하되 플레이 규칙의 미정과 구분한다. [게임 방향](../game-design/direction/overview.md) · [로드맵](../game-design/development/roadmap.md)

## 기획 마무리 후

- [ ] 기획을 재정리하고 개발 비용·재미 기여도를 검토하여 축소·제외할 범위를 결정한다.
- [ ] 기존 지침·구현을 검토한 뒤 여러 시스템이 공유하는 상태·처리 순서·중단과 예약·저장 경계를 결정한다.
- [ ] 정리된 범위에 맞춰 개발 로드맵을 갱신하고 단계별 PLAN을 준비한다. [작업 순서 기준](../agents/game-design.md#작업-초안과-최종-정리)

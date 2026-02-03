스크롤 게임 배경 넣는 위치 (Unity 2D)
========================================

[방법 1] 카메라 자식으로 넣기 (추천 - 화면 고정 배경)
  - Hierarchy에서 Main Camera를 선택 → 우클릭 → Create Empty → 이름 "Background"
  - Background에 Sprite Renderer 추가 후 배경 스프라이트 할당
  - 카메라가 움직여도 배경이 항상 화면을 가득 채움 (스크롤 시 카메라와 함께 이동)

[방법 2] 씬 루트에 배치 (월드 스크롤 배경)
  - Hierarchy 루트에 "Background" 오브젝트 생성 (지금 씬에 추가해 둔 것)
  - 배경 스프라이트 할당 후, 스크롤 시 이 오브젝트를 움직이거나
  - 타일링 스프라이트를 여러 개 나란히 배치해 무한 스크롤 구현

[정리]
  - 화면에 고정된 배경(패럴랙스용) → Main Camera 자식
  - 월드가 스크롤되는 배경(지형/타일) → 씬 루트 또는 전용 "Background" 부모 아래

Sprite Renderer에서:
  - Sorting Layer: Default (또는 "Background" 레이어 만들어서 사용)
  - Order in Layer: -10 등 낮은 값 → 다른 오브젝트보다 뒤에 그려짐

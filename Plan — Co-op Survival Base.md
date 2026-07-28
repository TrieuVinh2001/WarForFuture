SPEC KỸ THUẬT — Co-op Survival Base-Building (2D)

Định dạng backlog dành cho AI agent (Claude Code / tương tự) tự đọc và triển khai từng ticket. Nguyên tắc: mỗi ticket độc lập nhất có thể, có Input/Output/Acceptance Criteria rõ ràng, không mơ hồ.

0. Nguyên tắc làm việc cho AI Agent
Không tự mở rộng scope. Chỉ implement đúng những gì ticket yêu cầu. Nếu thấy thiếu, ghi vào mục "Câu hỏi mở" ở cuối file thay vì tự quyết định.
Làm theo thứ tự Phase. Không bắt đầu Phase N+1 khi Phase N chưa pass Acceptance Criteria.
Mỗi ticket = 1 commit. Message dạng [P<phase>-T<ticket>] <mô tả ngắn>.
Ưu tiên chạy được hơn đẹp. MVP trước, polish sau (Phase 6).
Server Authoritative tuyệt đối: client KHÔNG BAO GIỜ tự tính damage/vị trí cuối/loot — chỉ gửi input, server trả state.
Test thủ công tối thiểu sau mỗi ticket: build server headless + 2 client instance, quay lại 1 câu mô tả kết quả.
1. MVP — Scope thu gọn (bắt buộc phải xong trước khi nghĩ đến phần mở rộng)

Loại bỏ khỏi MVP (để dành Phase mở rộng — mục 7): Roguelite meta-map, Weather system, NPC merchant/dialogue, Procedural đa biome (chỉ 1 biome tại MVP), Skill tree, Pet, Equipment rarity.

MVP giữ lại:

1 map cố định (không procedural), 1 biome (Forest).
2–4 người chơi, không PvP.
Chu kỳ ngày/đêm đơn giản: Day (thu thập/craft/xây) → Night (3 wave quái) → Boss ở night thứ 5.
Resource: Wood, Stone, Fiber, Food (4 loại thôi).
Crafting: 6 công thức cố định (không cần blueprint-unlock system ở MVP).
Building: Wall, Door, Workbench, Campfire, Chest, Watch Tower (6 loại).
Combat: 1 vũ khí cận chiến (Sword), 1 vũ khí tầm xa (Bow), 1 loại quái thường, 1 Boss.
Meta progression: chỉ lưu Gold + unlock 2 building mới sau khi thắng — không cần hệ thống Talent/Skin/Character.
Không cloud save phức tạp — 1 bảng PlayerSave JSON là đủ cho MVP.

Định nghĩa "MVP xong": 2-4 client kết nối cùng 1 server, chơi trọn 1 trận (5 ngày + boss), sống hoặc chết, disconnect/reconnect không crash server.

2. Kiến trúc kỹ thuật
Unity Client (FishNet Client)
        │  RPC / SyncVar
Unity Headless Server (FishNet Server)
        │  HTTP REST (chỉ lúc vào/thoát trận)
ASP.NET Core API (Auth, PlayerSave, Leaderboard)
        │
PostgreSQL
Tick rate: Network tick 20/s (50ms). Physics chạy FixedUpdate riêng ở 50/s.
Client → Server: chỉ gửi Input packet (MoveInput, Jump, AttackInput, InteractInput, CraftRequest, BuildRequest).
Server → Client: SyncVar/ObserverRPC cho Position, HP, AnimState, InventoryDelta, WaveState, BossState.
Không sync: particle, sound, camera, tooltip, UI local, animation event (client tự chạy dựa trên AnimState enum nhận từ server).
Client-side prediction: bật cho Move/Jump để tránh cảm giác lag; Server reconcile mỗi tick, correction nếu lệch > ngưỡng (đề xuất 0.15 unit).
Object Pooling bắt buộc cho: Projectile, Enemy, DamageText, LootDrop, Effect — không Instantiate/Destroy trực tiếp trong gameplay loop.
Network Message Contract (đủ để AI code thẳng)
csharp
// Client -> Server
struct MoveInputMsg   { float horizontal; bool jumpPressed; uint clientTick; }
struct AttackInputMsg { byte weaponSlot; uint clientTick; }
struct InteractMsg    { int targetNetId; }
struct CraftRequestMsg{ int recipeId; }
struct BuildRequestMsg{ int buildingId; Vector2 gridPos; }

// Server -> Client (ObserverRPC hoặc SyncVar)
struct StateCorrectionMsg { Vector2 position; float velocity; uint ackTick; }
struct HpUpdateMsg        { int netId; int currentHp; int maxHp; }
struct InventoryDeltaMsg  { int itemId; int deltaAmount; }
struct WaveStateMsg       { int waveIndex; int enemiesRemaining; float timeToNextWave; }
struct BossStateMsg       { int bossNetId; int phase; int hp; }
Database Schema (MVP)
sql
Player       (id, username, password_hash, created_at)
PlayerSave   (player_id FK, gold int, unlocked_buildings jsonb, updated_at)
RoomSession  (id, server_address, status enum('waiting','in_progress','ended'), created_at)
RoomPlayer   (room_id FK, player_id FK, joined_at)

Mở rộng bảng Item/Inventory chi tiết ở Phase 5 khi có nhiều item hơn — MVP chỉ cần jsonb đơn giản để tránh over-engineering sớm.

3. Cấu trúc thư mục Unity
Assets/
  Art/ (Sprites, Animations, Tilemaps)
  Audio/
  Prefabs/ (Player, Enemies, Buildings, Projectiles)
  Scenes/ (Bootstrap, ForestMap, MainMenu)
  Scripts/
    Network/        (FishNet spawners, NetworkManager config)
    Gameplay/
      Player/
      Combat/
      Inventory/
      Buildings/
      Enemies/
      Boss/
      DayNightCycle/
    UI/
    Data/            (ScriptableObjects: ItemData, RecipeData, BuildingData)
    Save/
  Resources/
  Addressables/
4. Roadmap dạng Backlog (Ticket có Acceptance Criteria)
Phase 1 — Nền tảng mạng (ưu tiên #1, không có gì chạy được nếu thiếu phần này)
#	Ticket	Input	Output	Acceptance Criteria
1.1	Setup Unity project + FishNet package	—	Project trống build được	Build headless server thành công, không lỗi console
1.2	NetworkManager + Server/Client boot scene	1.1	Server headless khởi động, client connect được	2 client connect cùng lúc, log xác nhận trên server
1.3	Player spawn + di chuyển cơ bản (move, jump)	1.2	Player prefab với PlayerMovement.cs	Client thấy player di chuyển mượt, server thấy đúng vị trí (log)
1.4	Client-side prediction + reconciliation	1.3	StateCorrectionMsg hoạt động	Giả lập 150ms latency (Clumsy/Network Emulator), player vẫn di chuyển mượt, không giật
Phase 2 — Gameplay cốt lõi
#	Ticket	Acceptance Criteria
2.1	Resource node (cây, đá) + thu thập	Chặt cây → item vào inventory, node biến mất đúng thời gian respawn
2.2	Inventory system (server-authoritative)	Client gửi request, server validate + trả InventoryDeltaMsg, UI cập nhật
2.3	Crafting (6 công thức MVP)	Đủ nguyên liệu → craft thành công; thiếu → server từ chối, client nhận lỗi rõ ràng
2.4	Building placement (6 loại)	Đặt building hợp lệ trên grid, sync cho tất cả client, invalid position bị chặn
2.5	Day/Night cycle timer	Server điều khiển giờ, client hiển thị UI đồng bộ, chuyển pha đúng thời điểm
Phase 3 — Combat
#	Ticket	Acceptance Criteria
3.1	Enemy AI state machine (Idle/Patrol/Detect/Attack/Return)	Quái phản ứng đúng khi player vào tầm nhìn, quay lại patrol khi mất dấu
3.2	Melee + ranged combat, damage server-validated	Client không thể tự gây damage giả (test bằng cách sửa client damage value — server phải bỏ qua)
3.3	HP/death/respawn	Player chết → respawn đúng vị trí quy định, không crash
3.4	Wave spawner (3 wave/đêm)	Wave spawn đúng số lượng, đúng thời điểm, dọn hết wave mới cho qua night tiếp
3.5	Boss (1 con, có ít nhất 2 phase)	Boss chuyển phase đúng ngưỡng HP, chết → trigger reward
Phase 4 — Co-op hoàn chỉnh
#	Ticket	Acceptance Criteria
4.1	Sync building/loot giữa nhiều client	2 client cùng thấy building/loot người kia tạo ra real-time
4.2	Revive đồng đội	Player gục (không chết hẳn) → đồng đội tương tác → hồi sinh
4.3	Chat + ping cơ bản	Gửi tin nhắn/ping, tất cả client trong room nhận được
Phase 5 — Backend & Persistence
#	Ticket	Acceptance Criteria
5.1	ASP.NET Core API: Auth (register/login JWT)	Đăng ký/đăng nhập, token trả về hợp lệ
5.2	PlayerSave API (gold, unlocked buildings)	Sau trận, gọi API lưu đúng dữ liệu, load lại đúng khi login
5.3	Room/Matchmaking đơn giản (list room, join)	Client thấy danh sách room đang mở, join thành công
Phase 6 — Tối ưu & Đóng gói MVP
#	Ticket	Acceptance Criteria
6.1	Object Pooling cho Projectile/Enemy/Loot/Effect	Không còn Instantiate/Destroy trong runtime path (grep để confirm)
6.2	Addressables cho Art/Audio	Build size giảm, load scene không giật khung hình
6.3	Bandwidth profiling	Đo băng thông/player/giây, ghi số liệu vào README
6.4	Test end-to-end: 4 client, 1 trận đầy đủ	Chạy trọn từ join → 5 ngày → boss → thoát, không crash, không desync
5. Mở rộng sau MVP (backlog tham khảo — KHÔNG làm trước khi Phase 1-6 xong)

Giữ nguyên toàn bộ nội dung tầm nhìn gốc để dùng dần: procedural biome (Mountain/River/Cave/Temple/Snow/Volcano), weather (Rain/Snow/Storm/Fog/Blood Moon), thêm resource hiếm (Crystal, Ancient Relic, Dragon Heart), Tier 2/3 building (Tesla Tower, Auto Turret, Teleport), thêm vũ khí (Crossbow, Shotgun, Rifle, Magic Staff, Laser), NPC (Merchant, Blacksmith, Wizard...), sự kiện (Meteor, Invasion, Merchant Festival), thêm boss (Spider Queen, Ice Dragon, Necromancer, Demon King), meta progression đầy đủ (Character, Skin, Pet, Talent), anti-cheat nâng cao (rate-limit theo hành vi, thống kê bất thường).

Mỗi hệ thống ở đây nên được viết lại thành ticket theo đúng format Phase ở trên khi tới lượt triển khai — không thêm vào giữa chừng MVP.

6. Câu hỏi mở (cần người quyết định, AI không tự ý chọn)
Nền tảng phát hành mục tiêu: PC (Steam) và có cả mobile (ảnh hưởng input scheme, UI scale)
Art style: pixel art hay hand-drawn? (ảnh hưởng pipeline Addressables/Sprite Atlas) : hand-drawn
Có cần cross-platform save (mobile + PC cùng account) ở MVP không, hay để sau? : để sau
Server hosting: tự host hay dùng dịch vụ (PlayFab, Photon Fusion hosting, v.v.)? : tự host
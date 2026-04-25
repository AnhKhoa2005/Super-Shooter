# Super Shooter

Description: Super Shooter là game bắn xe tăng góc nhìn arena theo thời gian thực, nơi người chơi phải vừa di chuyển vừa ngắm bắn chính xác để hạ gục đối thủ. Trong mỗi trận, người chơi điều khiển xe tăng, xoay tháp súng theo hướng chuột, khai hỏa đúng thời điểm và tận dụng các vật phẩm vũ khí xuất hiện trên bản đồ để thay đổi hỏa lực. Bên cạnh tấn công, người chơi cần né đạn, giữ vị trí an toàn và chọn thời điểm giao tranh hợp lý để không bị hạ gục liên tục. Mục tiêu cuối cùng là tích lũy điểm hạ gục cao nhất trước khi thời gian trận đấu kết thúc.

Technique applied:

- OOP
- ScriptableObject-driven architecture
- Observer Pattern (EventManager)
- Singleton Pattern (Singleton và NetworkSingleton)
- Object Pooling
- State-based gameplay flow (Lobby -> Playing -> EndGame)
- Real-time multiplayer networking với Photon Fusion (Host/Client, Networked Properties, RPC)
- In-room realtime chat với Photon Chat
- Cloud backend integration với PlayFab (đăng nhập người chơi, leaderboard)
- Async workflow với UniTask

Software used:

- Unity 2022.3.62f2
- C#
- Visual Studio Code
- Git, GitHub
- Photon Fusion
- Photon Chat
- PlayFab
- UniTask
- Odin Inspector
- DOTween

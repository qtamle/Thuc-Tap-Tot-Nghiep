# Thuc-Tap-Tot-Nghiep
# Make Mobile Android Game Online With Unity Gaming Services

Install Unity 6 6000.0.23f1

Mô tả đề tài (Explain Project):
Tựa game được lấy cảm hứng từ thể loại rouge-lite sẽ có những chức năng sau:

Đánh qua ải (Progressive Level System): Người chơi phải vượt qua từng màn chơi một cách tuần tự, và khi thua sẽ phải quay lại từ màn đầu.
Chết là bắt đầu lại (Permadeath Mechanics): Tương tự Roguelike, khi thua, người chơi mất tiến trình hiện tại nhưng vẫn giữ được phần thưởng hoặc nâng cấp để bắt đầu lại mạnh mẽ hơn.

Hệ thống nâng cấp và phần thưởng: Sau mỗi màn thắng hoặc toàn bộ vòng chơi, người chơi nhận được tài nguyên, phần thưởng để mua vũ khí, nâng cấp và trở nên mạnh hơn. Điều này tạo sự lặp lại có ý nghĩa và khuyến khích thử thách tiếp theo.

Loop Gameplay (Chơi lại từ đầu): Khi hoàn thành trò chơi hoặc thua, người chơi sẽ bắt đầu lại từ màn đầu với độ khó hoặc sự thay đổi dựa trên nâng cấp trước đó, duy trì tính hấp dẫn lâu dài.

Công nghệ sử dụng:

Game sẽ được nhóm xây dựng bằng Unity và build ra trên hệ điều hành Android (IOS nếu có thể).

Nhóm sẽ thiết kế trên Aseprite và Photoshop để vẽ bối cảnh nhân vật.

Asset Store Unity tìm những Package có thể hỗ trợ quá trình làm game.

Âm nhạc và âm thanh: Reaper và FL Studio.

Công nghệ có thể phát triển thêm:

Sử dụng Netcode để có thể xây coop giữa hai người chơi trong 1 màn chơi.

Cloud Save Unity của Unity Gaming Services (UGS) để lưu dữ liệu người dùng và đồng thời xây dựng backend.

Authentication để xác thực người dùng trên Unity Cloud.

Lobby là một dịch vụ để tạo phòng cho 2 người chơi qua mạng của UGS.

Relay là một dịch vụ của UGS giúp triển khai tính multiplayer, P2P networking.

Mini game 3D trên mobile.

-Cách cài đặt (How to install)
B1: Clone Project về "https://github.com/qtamle/Thuc-Tap-Tot-Nghiep"
B2: Mở Project bằng Unity.
B3: Cần tạo tài khoản trên Unity Cloud "https://login.unity.com/en/sign-up" 
B4: Sau khi có tài khoản trên Unity Cloud thì Add new Project trên Cloud
B5: Sau khi Project mở thành công trên Unity thì cần thiếp lập dịch vụ của UGS (Unity gaming services): Menu > Edit > Project Settings > Services (Chọn Organization, sau đó thì link Project trên Unity với Cloud Ở Bước 4)
B6: Cài những Package cần có như Services, Authentication, Cloud Save, Netcode for GameObject

- Cách sử dụng (How to Run)
  B1: Ở Scene Login, tạo tài khoản
  B2: Ở Scene Shop_Online, người dùng sẽ dùng số tiền đang có để mua 1 vũ khí, vũ khí sau khi được mua sẽ có thể nâng cấp bằng tiền (tối đa 1 vũ khí có 4 cấp độ)
  B3: Sau khi mua xong người chơi nhấn vào biểu tượng Play thì sẽ có 2 lựa chọn cho người chơi đó là Singleplayer hoặc Multiplayer
  B4: Nếu chọn Singleplayer thì sẽ vào màn chơi 1, Nếu chọn Multiplayer thì sẽ hỏi rằng muốn tạo phòng (Host) hoặc tham gia phòng (Client)
  B5: Nếu chọn Host thì sẽ được đưa Scene Lobby, ở đây sẽ có Room Code dùng để cho Client có thể Join vào Lobby.
  B6: Sau khi cả hai người chơi cùng vào lobby thì sẽ lựa chọn vũ khí của mình, (Lưu ý: Vũ khí ở trong lobby sẽ được đổ dữ liệu riêng với từng người chơi, tức là vũ khí mà người chơi đã sỡ hữu ngoài Shop thì mới có thể lựa chọn được)
  B7: Cả hai người chơi cùng nhấn Ready thì sẽ vào Màn chơi 1
  B8: Khi hoàn thành 1 màn chơi thì cả hai sẽ cùng đến với Màn Supply, Đây là màn cung cấp đồ tiếp tế cho người chơi. Mỗi người chơi chỉ được chọn 1 supply, sau khi cả hai người chơi chọn xong supply của mình thì sẽ qua màn tiếp theo.
  B9: Có tổng cộng 5 màn chơi, Sau khi hoàn thành thì sẽ về Summary và tổng kết lại người chơi đã kiếm được bao nhiêu tiền, có thể dùng để nâng cấp vũ khí hoặc mua vũ khí mới.
  
Nhiệm vụ vai trò của các thành viên:
Lê Quốc Tâm: Đảm nhận vai trò lên ý tưởng thiết kế gameplay, bối cảnh của trò chơi, vẽ và xây dựng animation cho nhân vật, xây dựng UI game. 
(Có thể lập trình hệ thống mạng)

Lê Trọng Nam: Đảm nhận vai trò lên ý tưởng gameplay, thiết kế và xây dựng logic trò chơi, xây dựng hệ thống game, quản lý lưu dữ liệu game.

Phan Hoàng Phương: Đảm nhận vai trò thiết kế và tích hợp hệ thống âm thanh động thay đổi theo ngữ cảnh của trò chơi, sáng tác nhạc nền, hiệu ứng âm thanh 
(Có thể lập trình hệ thống mạng)

Đề tài đã thực hiện qua của thành viên nhóm:
Lập trình mạng: 
Xây dựng ứng dụng winform C# Chatting App sử dụng giao thức TCP/IP. (Lê Quốc Tâm, Phan Hoàng Phương)
Lập trình app mobile: 
Xây dựng ứng dụng bán hàng laptop sử dụng công nghệ Cloud lưu trữ Firebase. (Lê Trọng Nam)
Xây dựng ứng dụng Travel-Planning-Chat App sử dụng Flutter và database NoSQL Firebase. (Lê Quốc Tâm)
	Lập trình game:
Xây dựng game 2D chiến thuật lấy cảm hứng từ Plant VS. Zombie sử dụng Unity C# (Lê Quốc Tâm, Lê Trọng Nam)
Đồ án chuyên ngành:
Xây dựng game 2D Platform thể loại Adventure RPG lấy ý tưởng từ cổ tích Việt Nam là Thạch Sanh: Sử dụng Unity, Photoshop, Skeleton Animation, Lưu dữ liệu Local bằng Json. 
(Lê Quốc Tâm, Lê Trọng Nam)


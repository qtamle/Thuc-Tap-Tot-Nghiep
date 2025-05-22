
---

# Thực Tập Tốt Nghiệp 

![LogoKhoa](https://github.com/user-attachments/assets/a41f75e9-98de-4dc9-8738-844b3f6491ac)


## **Make Mobile Android Game Online With Unity Gaming Services**

### **1. Cài đặt (How to Install)**
1. Clone project từ GitHub: `https://github.com/qtamle/Thuc-Tap-Tot-Nghiep`.
2. Mở project bằng Unity.
3. Tạo tài khoản trên Unity Cloud: [Đăng ký tại đây](https://login.unity.com/en/sign-up).
4. Add project mới trên Unity Cloud.
5. Thiết lập các dịch vụ Unity Gaming Services (UGS):  
   **Menu > Edit > Project Settings > Services** (Chọn Organization, sau đó thiết lập).
6. Cài các Package cần thiết: 
   - Services
   - Authentication
   - Cloud Save
   - Netcode for GameObject

---

### **2. Hướng dẫn sử dụng (How to Run)**
1. **Scene Login:** Tạo tài khoản (Mật khẩu phải có ít nhất 8 ký tự, gồm 1 chữ hoa, số, ký tự đặc biệt).
2. **Scene Shop_Online:** 
   - Người chơi dùng tiền để mua và nâng cấp vũ khí (tối đa 4 cấp).
3. **Chọn chế độ chơi:**  
   - Singleplayer: Vào màn chơi 1.  
   - Multiplayer: Chọn "Host" để tạo phòng hoặc "Client" để tham gia phòng.  
   - Lưu ý: Host sẽ có mã phòng (Room Code) để Client tham gia.
4. **Chơi Multiplayer:**
   - Cả hai người chơi chọn vũ khí và nhấn "Ready".
   - Vào màn chơi 1 và tiếp tục qua các màn.
   - Sau mỗi màn, người chơi sẽ vào **Màn Supply** để nhận đồ tiếp tế.
5. **Hoàn thành:** Tổng cộng 5 màn chơi. Sau khi kết thúc sẽ hiển thị tóm tắt số tiền kiếm được, dùng để nâng cấp vũ khí.

---

### **3. Mô tả đề tài (Project Description)**
#### **Tính năng chính:**
- **Đánh qua ải (Progressive Level System):** Vượt qua từng màn chơi tuần tự, thua thì quay lại từ đầu.
- **Chết là bắt đầu lại (Permadeath Mechanics):** Mất tiến trình hiện tại nhưng giữ phần thưởng/nâng cấp.
- **Hệ thống nâng cấp:** Nhận tài nguyên sau mỗi màn để mua vũ khí, nâng cấp.
- **Loop Gameplay:** Sau khi hoàn thành hoặc thua, bắt đầu lại từ màn 1 với độ khó tăng.

#### **Công nghệ sử dụng:**
1. **Unity:** Xây dựng game trên Android (có thể phát triển trên iOS).
2. **Thiết kế đồ họa:** Aseprite, Photoshop.
3. **Âm thanh:** Reaper, FL Studio.
4. **Unity Gaming Services (UGS):**
   - Netcode: Chơi coop giữa hai người chơi.
   - Cloud Save: Lưu dữ liệu người dùng.
   - Authentication: Xác thực tài khoản.
   - Lobby: Tạo phòng chơi.
   - Relay: Multiplayer, P2P networking.

---

### **4. Nhiệm vụ các thành viên**
- **Lê Quốc Tâm:**  
  Thiết kế gameplay, bối cảnh, vẽ animation, xây dựng hệ thống game, xây dựng UI game ,quản lý lưu dữ liệu game.(có thể lập trình mạng).  
- **Lê Trọng Nam:**  
  Thiết kế gameplay, bối cảnh,Thiết kế logic trò chơi, xây dựng hệ thống game, quản lý lưu dữ liệu game.  
- **Phan Hoàng Phương:**  
  Thiết kế & tích hợp âm thanh, sáng tác nhạc nền, hiệu ứng âm thanh (có thể lập trình mạng).

---

### **5. Đề tài đã thực hiện trước đây**
- **Lập trình mạng:**  
  - App Chatting (C#, TCP/IP).  
- **Lập trình app mobile:**  
  - Ứng dụng bán hàng laptop (Firebase).  
  - Ứng dụng Travel-Planning-Chat App (Flutter, Firebase).  
- **Lập trình game:**  
  - Game 2D chiến thuật (Unity).  
  - Game 2D Platformer Adventure RPG (Unity, ý tưởng từ "Thạch Sanh").  

---

### **6. Hình ảnh minh họa**
#### **Gameplay:**
- **Shop:**  
  ![Shop](https://github.com/user-attachments/assets/374ff491-289f-4f1e-96d9-0a6b45c21e7f)
- **Supply Scene:**  
  ![Supply Scene](https://github.com/user-attachments/assets/fd91fc6a-febc-480d-b36e-46ac57db2b63)
- **Levels 1-5:**  
  ![Level 1](https://github.com/user-attachments/assets/0373c1b2-e640-4747-a1a5-28f6c90ddd85)  
  ![Level 2](https://github.com/user-attachments/assets/04deade8-c0e5-4b66-9db0-e3dcda4c2cff)  
  ![Level 3](https://github.com/user-attachments/assets/a2c0cca8-061f-49eb-9002-31a8712f16c3)  
  ![Level 4](https://github.com/user-attachments/assets/6017d9c1-ae97-4f10-b611-ee72567a6e0d)  
  ![Level 5](https://github.com/user-attachments/assets/87389289-bc3a-46cb-965d-17301070c7e4)

#### **Enemy Bosses:**
- **Boss 1-5:**  
  ![Boss 1](https://github.com/user-attachments/assets/5c6e455d-6cfe-4e67-8a8e-e3f22c6ac904)  
  ![Boss 2](https://github.com/user-attachments/assets/1855a854-7ed8-4dc1-804d-89584a5acfc7)  
  ![Boss 3](https://github.com/user-attachments/assets/c3ad4018-6198-4b5b-989f-57cd8f76ddbe)  
  ![Boss 4](https://github.com/user-attachments/assets/76cbc08e-db5c-4d38-9ddc-ffedf0e47cad)  
  ![Boss 5](https://github.com/user-attachments/assets/3c2a5cfa-5e49-48ee-909b-c4ccb374ffbe)

#### **Vũ khí:**
- **Dagger:**  
  ![Dagger](https://github.com/user-attachments/assets/41136ca8-8a52-4cf1-96c5-5f6c2a23292d)  
- **Gloves:**  
  ![Gloves](https://github.com/user-attachments/assets/c76d274c-d5ae-4dc0-ab3f-9e0f3b4e50f7)  
- **ChainSaw:**  
  ![ChainSaw](https://github.com/user-attachments/assets/7e1a02fa-ed93-43b6-9dc6-90430dce107d)  
- **Claws:**  
  ![Claws](https://github.com/user-attachments/assets/f20a36a4-71ad-4ad4-89d3-8e004585f1a5)  
- **EnergyOrb:**  
  ![EnergyOrb](https://github.com/user-attachments/assets/3e603d55-9e85-426d-b7d7-3b64da535d9d)  
- **Katana:**  
  ![Katana](https://github.com/user-attachments/assets/b4416b2b-5a1b-4ccb-b5a7-58b46861d50e)

---



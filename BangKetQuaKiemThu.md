# Kết Quả Kiểm Thử (Test Execution Result)

## 1. Trạng thái chạy kiểm thử sau cùng (Tất cả PASS)

Dưới đây là bảng kết quả chạy tự động của 26 Kịch bản Kiểm thử hiện tại trên hệ thống (SAU KHI ĐÃ VÁ LỖI).

| ID | Input | Expected Output | Actual Output |
| :--- | :--- | :--- | :--- |
| **BVT_1** | Token cũ: `0`, Thời gian chờ: `0s` | Token khả dụng: `0` | Token khả dụng: `0` |
| **BVT_2** | Token cũ: `0`, Thời gian chờ: `1s` | Token khả dụng: `1` | Token khả dụng: `1` |
| **BVT_3** | Token cũ: `8`, Thời gian chờ: `1s` | Token khả dụng: `9` | Token khả dụng: `9` |
| **BVT_4** | Token cũ: `8`, Thời gian chờ: `2s` | Token khả dụng: `10` | Token khả dụng: `10` |
| **BVT_5** | Token cũ: `8`, Thời gian chờ: `5s` | Token khả dụng: `10` (Chặn max) | Token khả dụng: `10` |
| **BVT_6** | Token cũ: `5`, Thời gian lùi: `-3600s` | Token khả dụng: `5` (Bảo lưu, không bị âm) | Token khả dụng: `5` |
| **R1** | Cấp: `Free`, Loại: `GET`, Chờ: `0.95s` | Trạng thái: `429`, Dư: `0.95 Token` | Trạng thái: `429`, Dư: `0.95 Token` |
| **R2** | Cấp: `Free`, Loại: `GET`, Chờ: `1.00s` | Trạng thái: `200`, Dư: `0 Token` | Trạng thái: `200`, Dư: `0 Token` |
| **R3** | Cấp: `Free`, Loại: `GET`, Chờ: `1.01s` | Trạng thái: `200`, Dư: `0.01 Token` | Trạng thái: `200`, Dư: `0.01 Token` |
| **R4** | Cấp: `Free`, Loại: `POST`, Chờ: `2.95s` | Trạng thái: `429`, Dư: `2.95 Token` | Trạng thái: `429`, Dư: `2.95 Token` |
| **R5** | Cấp: `Free`, Loại: `POST`, Chờ: `3.00s` | Trạng thái: `200`, Dư: `0 Token` | Trạng thái: `200`, Dư: `0 Token` |
| **R6** | Cấp: `Free`, Loại: `POST`, Chờ: `3.01s` | Trạng thái: `200`, Dư: `0.01 Token` | Trạng thái: `200`, Dư: `0.01 Token` |
| **R7** | Cấp: `Premium`, Loại: `GET`, Chờ: `0.95s` | Trạng thái: `429`, Dư: `0.95 Token` | Trạng thái: `429`, Dư: `0.95 Token` |
| **R8** | Cấp: `Premium`, Loại: `GET`, Chờ: `1.00s` | Trạng thái: `200`, Dư: `0 Token` | Trạng thái: `200`, Dư: `0 Token` |
| **R9** | Cấp: `Premium`, Loại: `GET`, Chờ: `1.01s` | Trạng thái: `200`, Dư: `0.01 Token` | Trạng thái: `200`, Dư: `0.01 Token` |
| **R10** | Cấp: `Premium`, Loại: `POST`, Chờ: `1.95s`| Trạng thái: `429`, Dư: `1.95 Token` | Trạng thái: `429`, Dư: `1.95 Token` |
| **R11** | Cấp: `Premium`, Loại: `POST`, Chờ: `2.00s`| Trạng thái: `200`, Dư: `0 Token` | Trạng thái: `200`, Dư: `0 Token` |
| **R12** | Cấp: `Premium`, Loại: `POST`, Chờ: `2.01s`| Trạng thái: `200`, Dư: `0.01 Token` | Trạng thái: `200`, Dư: `0.01 Token` |
| **INV_1** | Khởi tạo với Loại tài khoản: `"vip"` | Lỗi `ArgumentException`: "Tier không hợp lệ" | Lỗi `ArgumentException` |
| **INV_2** | Khởi tạo với Loại tài khoản: `""` | Lỗi `ArgumentException`: "Tier không hợp lệ" | Lỗi `ArgumentException` |
| **INV_3** | Khởi tạo với Loại tài khoản: `" "` | Lỗi `ArgumentException`: "Tier không hợp lệ" | Lỗi `ArgumentException`|
| **INV_4** | Gọi API với Phương thức: `"PUT"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException`|
| **INV_5** | Gọi API với Phương thức: `"DELETE"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException`|
| **INV_6** | Gọi API với Phương thức: `"PATCH"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException`|
| **INV_7** | Gọi API với Phương thức: `""` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException`|
| **INV_8** | Gọi API với Phương thức: `" "` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException`|

---

## 2. Báo cáo Tình trạng Lỗi và Khắc phục (Bug Tracking & Fixes)

Trong quá trình tiến hành chạy các kịch bản kiểm thử, hệ thống đã phát hiện và ghi nhận 3 vấn đề gây cản trở sự chính xác của ứng dụng (Trạng thái FAIL). Dưới đây là biên bản ghi nhận lỗi và quá trình vá mã nguồn.

### SỰ CỐ 1 (Lỗi Logic Mã nguồn): Sinh ra số âm vô hạn khi đồng hồ gián đoạn (Time Drift Bug)
* **Kịch bản thao tác:** BVT_6 / CallApi_TimeGoesBack_ShouldNotResultInNegativeTokens
* **Phân loại lỗi:** Lỗi logic hệ thống nghiêm trọng (Critical Bug).
* **Diễn giải:** Khi đồng hồ hệ điều hành bị giật lùi về quá khứ (do NTP sync hoặc mất điện), hàm tính toán khoảng thời gian đã bị âm. 
* **Kết quả mong đợi:** Token không bị thay đổi (Bảo lưu nguyên vẹn).
* **Kết quả Máy chủ trả về (ACTUAL OUTPUT - FAIL):** 
  ```text
  BUG NGHIÊM TRỌNG: Đồng hồ lùi 1 tiếng làm Token bị ÂM (-3594999 ms). Hệ thống sẽ bị khoá vĩnh viễn!
  ```
* **Mã nguồn ban đầu (Sai):**
  ```csharp
  // File: APIRateLimiter.cs
  // Lỗi hiển nhiên: Không chặn giá trị âm nếu phép trừ ra kết quả bé hơn 0
  int elapsedMs = (int)(DateTime.UtcNow - _lastRefillTime).TotalMilliseconds;
  ```
* **Cách khắc phục:** 
  Áp dụng hàm `Math.Max(0, ...)` để đánh chặn bắt buộc số mili-giây trôi qua nhỏ nhất phải là 0:
  ```csharp
  // VÁ LỖI File: APIRateLimiter.cs
  int elapsedMs = Math.Max(0, (int)(DateTime.UtcNow - _lastRefillTime).TotalMilliseconds);
  ```
* **Trạng thái cấu hình lại:** Chạy lại kịch bản tự động bằng xUnit ➔ **PASS**.

---

### SỰ CỐ 2 (Lỗi Kịch bản Test): Sai lệch do độ trễ bộ đếm giờ Windows (Timer Resolution)
* **Kịch bản thao tác:** R1, R4, R7, R10 (Kiểm thử ngay dưới biên hồi phục).
* **Phân loại lỗi:** Lỗi Kịch bản / Môi trường chạy Test. 
* **Diễn giải:** Kịch bản R10 mong đợi từ chối gọi API nếu chỉ mới chờ 1990ms (cần 2000ms). Tuy nhiên, lệnh `Thread.Sleep(1990)` của C# bị hệ điều hành Windows cộng thêm độ trễ tác vụ, khiến luồng thực sự ngủ tới hơn 2000ms. 
* **Kết quả mong đợi:** Mã trạng thái 429 (Từ chối).
* **Kết quả Máy chủ trả về (ACTUAL OUTPUT - FAIL):** 
  ```text
     Assert.Equal() Failure: Values differ
  Expected: 429
  Actual:   200  (Hệ thống tưởng đã đủ thời gian và Trừ tiền duyệt luôn)
  ```
* **Cách khắc phục kịch bản:** 
  Hạ thời gian "Dưới biên" xuống khoảng cách an toàn (từ `990` thành `950ms`). Đồng thời sửa đổi điều kiện thẩm định `Assert.Equal` tuyệt đối thành kỹ thuật "châm chước sai lệch tương đối" để bù trừ nhịp ngủ của CPU:
  ```csharp
  // VÁ LỖI File: UnitTest1.cs
  // Từ:
  Assert.Equal(waitToken, currentToken, precision: 1); 
  // Sửa thành (Dùng sai lệch dung sai):
  Assert.True(Math.Abs(currentToken - waitToken) <= 0.05); 
  ```
* **Trạng thái cấu hình lại:** Chạy lại kịch bản hoàn chỉnh ➔ **PASS**.

---

### SỰ CỐ 3 (Lỗi Bất đồng bộ Thông báo): Bắt ngoại lệ chuỗi chữ bị sai lệch
* **Kịch bản thao tác:** INV_1, INV_2, INV_3 (Nhập loại tài khoản sai)
* **Phân loại lỗi:** Lỗi kịch bản Test (Assert Assertion).
* **Diễn giải:** Các lập trình viên Hệ thống cố tình sinh ra thông báo lỗi bắt đầu là `"Tier không hợp lệ: '{tier}'"`. Nhưng bên trong script kiểm thử tự động, Kỹ sư kiểm thử lại đi vòi hỏi đoạn chữ: `"Loại tài khoản không hợp lệ"`.
* **Kết quả Máy chủ trả về (ACTUAL OUTPUT - FAIL):** 
  ```text
    Assert.Contains() Failure: Sub-string not found
    Tiếng kêu gào: "Tier không hợp lệ: 'vip'. Chọn 'free' hoặc 'premium'."
    Test đi rình:  "Loại tài khoản không hợp lệ"
  ```
* **Cách khắc phục:** 
  Điều chỉnh lại kịch bản file `UnitTest1.cs` cho đồng bộ với đặc tả API (API Specification).
  ```csharp
  // VÁ LỖI File: UnitTest1.cs
  // Từ:
  Assert.Contains("Loại tài khoản không hợp lệ", exception.Message);
  // Sửa thành:
  Assert.Contains("Tier không hợp lệ", exception.Message);
  ```
* **Trạng thái cấu hình lại:** Chạy lại luồng Test ➔ **PASS**.

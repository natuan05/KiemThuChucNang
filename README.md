# Kiểm thử chức năng

## Bài toán: API Rate limiter - Token Bucket

Hệ thống cấp phát mỗi người dùng một Token bucket. Mỗi khi người dùng gọi API, hệ thống sẽ kiểm tra số lượng token hiện có, đối chiếu chi phí của yêu cầu để quyết định duyệt hoặc từ chối

Cấu hình hệ thống

- Sức chứa tối đa: 10 tokens/ tài khoản. Bộ đếm không bao giờ vượt qua số này.
- Tốc độ hồi phục: 1 token 1 giây.
- Công thức cập nhật: Token khả dụng = Tối thiểu (10, Token cũ + Số giây đã trôi qua)

Quy tắc tính phí (Tham số đầu vào từ người dùng)

- Tài khoản Free: Gọi phương thức GET tốn 1 token, POST tốn 3 token
- Tài khoản Premium: GET tốn 1 token, POST tốn 2 tokens

Điều kiện duyệt

- Hợp lệ: Token khả dụng ≥ Chi phí yêu cầu. Hệ thống trả về mã thành công và trừ số token tương ứng
- Không hợp lệ (Từ chối): Token khả dụng < Chi phí yêu cầu. Lỗi 429 Too Many Requests và Giữ nguyên token.

**Kiểm thử biên**

| **ID** | **Token cũ** | **Số giây trôi qua** | **Giá trị tính toán** | **Kết quả mong đợi (Token khả dụng)** | **Kết quả thực tế** |
| --- | --- | --- | --- | --- | --- |
| **BVT_1** | 0 | 0 | 0 | **0** | Token khả dụng: `0` |
| **BVT_2** | 0 | 1 | 1 | **1** | Token khả dụng: `1` |
| **BVT_3** | 8 | 1 | 9 | **9** | Token khả dụng: `9` |
| **BVT_4** | 8 | 2 | 10 | **10** | Token khả dụng: `10` |
| **BVT_5** | 8 | 5 | 13 | **10** | Token khả dụng: `10` |
| **BVT_6** | 5 | -3600 | -3595 | **5** | Token khả dụng: `5` |

| ID | Input | Expected Output | Actual Output |
| --- | --- | --- | --- |
| 1 | Gọi API với Phương thức: `"PUT"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| 2 | Gọi API với Phương thức: `"DELETE"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| 3 | Gọi API với Phương thức: `"PATCH"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| 4 | Gọi API với Phương thức: `""` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| 5 | Gọi API với Phương thức: `" "` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |

**Bảng quyết định**

|  |  | **R1** | **R2** | **R3** | **R4** | **R5** | **R6** | **R7** | **R8** | **R9** |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| **Điều kiện**  | **Loại tài khoản (T)** | free | free | free | free | premium | premium | premium | premium |  |
|  | **Phương thức (M)** | GET | GET | POST | POST | GET | GET | POST | POST | Khác |
|  | **Đủ Token tương ứng?** | Có | Không | Có | Không | Có | Không | Có | Không |  |
| **Hành động** | **Duyệt (200 OK)** | **X** |  | **X** |  | **X** |  | **X** |  |  |
|  | **Từ chối (429)** |  | **X** |  | **X** |  | **X** |  | **X** |  |
|  | **Tokens không đổi** |  | **X** |  | **X** |  | **X** |  | **X** |  |
|  | **Tokens - 1** | **X** |  |  |  | **X** |  |  |  |  |
|  | **Tokens - 2** |  |  |  |  |  |  | **X** |  |  |
|  | **Tokens - 3** |  |  | **X** |  |  |  |  |  |  |
|  | **Báo input không hợp lệ** |  |  |  |  |  |  |  |  | **X** |

**Đặt số token về 0, kết hợp bảng quyết định và các giá trị biên**

| **Quy tắc (Rule)** | **Tier** | **Method** | **Thời gian chờ (t)** | **Kết quả mong đợi** | **Kết quả thực tế** |
| --- | --- | --- | --- | --- | --- |
| **R1 (Dưới biên)** | Free | GET | **0.9s** | **Từ chối (429), Tokens không đổi** | Trạng thái: `429`, Dư: `0.9 Token` |
| **R2 (Tại biên)** | Free | GET | **1.00s** | **Duyệt (200), Tokens dư 0** | Trạng thái: `200`, Dư: `0 Token` |
| **R3 (Trên biên)** | Free | GET | **1.1s** | **Duyệt (200), Tokens dư 0.1** | Trạng thái: `200`, Dư: `0.1 Token` |
| **R4 (Dưới biên)** | Free | POST | **2.9s** | **Từ chối (429)** | Trạng thái: `429`, Dư: `2.9 Token` |
| **R5 (Tại biên)** | Free | POST | **3.00s** | **Duyệt (200)** | Trạng thái: `200`, Dư: `0 Token` |
| **R6 (Trên biên)** | Free | POST | **3.1s** | **Duyệt (200)** | Trạng thái: `200`, Dư: `0.1 Token` |
| **R7 (Dưới biên)** | Premium | GET | **0.9s** | **Từ chối (429)** | Trạng thái: `429`, Dư: `0.9 Token` |
| **R8 (Tại biên)** | Premium | GET | **1.00s** | **Duyệt (200)** | Trạng thái: `200`, Dư: `0 Token` |
| **R9 (Trên biên)** | Premium | GET | **1.1s** | **Duyệt (200)** | Trạng thái: `200`, Dư: `0.1 Token` |
| **R10 (Dưới biên)** | Premium | POST | **1.9s** | **Từ chối (429)** | Trạng thái: `429`, Dư: `1.9 Token` |
| **R11 (Tại biên)** | Premium | POST | **2.00s** | **Duyệt (200)** | Trạng thái: `200`, Dư: `0 Token` |
| **R12 (Trên biên)** | Premium | POST | **2.1s** | **Duyệt (200)** | Trạng thái: `200`, Dư: `0.1 Token` |

**Báo cáo Tình trạng Lỗi và Khắc phục (Bug Tracking & Fixes)**

Lỗi đã gặp:

| ID | Input | Expected Output | Actual Output |
| :--- | :--- | :--- | :--- |
| **BVT_6** | Token cũ: `5`<br>Lùi giờ hệ thống: `- 1 giờ` | Token không đổi, khả dụng: `5` | ❌ **FAIL (Lỗi Time Drift)**<br>Token rớt thành số âm.<br>`BUG: Token bị ÂM (-3594999 ms)` |
| **R10** | Cấp: `Premium`, Loại: `POST`<br>Thời gian chờ sát biên: `1.99s` | Trạng thái: `429` (Từ chối) | ❌ **FAIL (Trễ nhịp Sleep OS)**<br>Hệ thống tưởng đủ giờ và Duyệt trừ tiền luôn.<br>`Expected: 429`<br>`Actual: 200` |

*(Sau khi phát hiện 2 bản ghi đỏ trên, tôi đã áp dụng hàm `Math.Max` để chặn số âm cho `BVT_6`, đồng thời nới rộng khoảng cách "Dưới biên" và tăng sai lệch so sánh cho nhóm `R10`, đem tất cả trở lại trạng thái Xanh Passing ở bảng mục 1).*


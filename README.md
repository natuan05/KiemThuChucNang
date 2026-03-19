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

| ID | Input | Expected Output | Actual Output |
| --- | --- | --- | --- |
| **BVT_1** | Token cũ: `0`, Trôi qua: `0s` | Token khả dụng: `0` | Token khả dụng: `0` |
| **BVT_2** | Token cũ: `0`, Trôi qua: `1s` | Token khả dụng: `1` | Token khả dụng: `1` |
| **BVT_3** | Token cũ: `8`, Trôi qua: `1s` | Token khả dụng: `9` | Token khả dụng: `9` |
| **BVT_4** | Token cũ: `8`, Trôi qua: `2s` | Token khả dụng: `10` | Token khả dụng: `10` |
| **BVT_5** | Token cũ: `8`, Trôi qua: `5s` | Token khả dụng: `10` | Token khả dụng: `10` |
| **BVT_6** | Token cũ: `5`, Trôi qua: `-3600s` | Token khả dụng: `5` | Token khả dụng: `5` |

| ID | Input | Expected Output | Actual Output |
| --- | --- | --- | --- |
| **INV_1** | Gọi API với Phương thức: `"PUT"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| **INV_2** | Gọi API với Phương thức: `"DELETE"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| **INV_3** | Dữ liệu biên - Chuỗi Rỗng (Length = 0): `""` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| **INV_4** | Dữ liệu biên - Có khoảng trắng (Length = 1): `" "` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| **INV_5** | Dữ liệu biên - Đối tượng NULL (Chưa khởi tạo) | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| **INV_6** | Đoán lỗi chính tả - Chuỗi tương tự: `"GETT"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |
| **INV_7** | Nhập ký tự đặc biệt - Cố ý phá ứng dụng: `"123!@#"` | Lỗi `ArgumentException`: "Phương thức không hợp lệ" | Lỗi `ArgumentException` |

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

| ID | Input | Expected Output | Actual Output |
| --- | --- | --- | --- |
| **R1** (Dưới biên) | Cấp: `Free`, Loại: `GET`, Chờ: `0.9s` | **Từ chối (429), Tokens không đổi** | Trạng thái: `429`, Dư: `0.9 Token` |
| **R2** (Tại biên) | Cấp: `Free`, Loại: `GET`, Chờ: `1.00s` | **Duyệt (200), Tokens dư 0** | Trạng thái: `200`, Dư: `0 Token` |
| **R3** (Trên biên) | Cấp: `Free`, Loại: `GET`, Chờ: `1.1s` | **Duyệt (200), Tokens dư 0.1** | Trạng thái: `200`, Dư: `0.1 Token` |
| **R4** (Dưới biên) | Cấp: `Free`, Loại: `POST`, Chờ: `2.9s` | **Từ chối (429)** | Trạng thái: `429`, Dư: `2.9 Token` |
| **R5** (Tại biên) | Cấp: `Free`, Loại: `POST`, Chờ: `3.00s` | **Duyệt (200)** | Trạng thái: `200`, Dư: `0 Token` |
| **R6** (Trên biên) | Cấp: `Free`, Loại: `POST`, Chờ: `3.1s` | **Duyệt (200)** | Trạng thái: `200`, Dư: `0.1 Token` |
| **R7** (Dưới biên) | Cấp: `Premium`, Loại: `GET`, Chờ: `0.9s` | **Từ chối (429)** | Trạng thái: `429`, Dư: `0.9 Token` |
| **R8** (Tại biên) | Cấp: `Premium`, Loại: `GET`, Chờ: `1.00s` | **Duyệt (200)** | Trạng thái: `200`, Dư: `0 Token` |
| **R9** (Trên biên) | Cấp: `Premium`, Loại: `GET`, Chờ: `1.1s` | **Duyệt (200)** | Trạng thái: `200`, Dư: `0.1 Token` |
| **R10** (Dưới biên) | Cấp: `Premium`, Loại: `POST`, Chờ: `1.9s` | **Từ chối (429)** | Trạng thái: `429`, Dư: `1.9 Token` |
| **R11** (Tại biên) | Cấp: `Premium`, Loại: `POST`, Chờ: `2.00s` | **Duyệt (200)** | Trạng thái: `200`, Dư: `0 Token` |
| **R12** (Trên biên) | Cấp: `Premium`, Loại: `POST`, Chờ: `2.1s` | **Duyệt (200)** | Trạng thái: `200`, Dư: `0.1 Token` |

**Báo cáo Tình trạng Lỗi và Khắc phục (Bug Tracking & Fixes)**

Lỗi đã gặp:

| ID | Input | Expected Output | Actual Output |
| :--- | :--- | :--- | :--- |
| **BVT_6** | Token cũ: `5`<br>Lùi giờ hệ thống: `- 1 giờ` | Token không đổi, khả dụng: `5` | ❌ **FAIL (Lỗi Time Drift)**<br>Token rớt thành số âm.<br>`BUG: Token bị ÂM (-3594999 ms)` |
| **R10** | Cấp: `Premium`, Loại: `POST`<br>Thời gian chờ sát biên: `1.99s` | Trạng thái: `429` (Từ chối) | ❌ **FAIL (Trễ nhịp Sleep OS)**<br>Hệ thống tưởng đủ giờ và Duyệt trừ tiền luôn.<br>`Expected: 429`<br>`Actual: 200` |
| **INV_5** | Nhập Phương thức bị Null/Rỗng: `null` | Lỗi `ArgumentException` (Thông báo hợp lệ báo văng lỗi) | ❌ **FAIL (Crash Hệ Thống)**<br>Hệ thống sập ném ra `NullReferenceException` do cố chạy lệnh `.ToUpper()` lên `null`. |

*(Sau khi phát hiện 3 bản ghi đỏ trên, thiết kế đã được khắc phục lại như sau: Bổ sung lệnh `string.IsNullOrWhiteSpace` chặn lỗi crash cho `INV_5`, áp dụng hàm `Math.Max` chặn số âm cho `BVT_6`, và nới rộng khoảng cách "Dưới biên" bù trừ độ trễ OS cho `R10`, giúp tất cả trở lại trạng thái Xanh (Passing) ở các bảng phía trên).*


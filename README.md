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

Input được xác định là:

- Loại tài khoản: "free" hoặc "premium"
- Phương thức: "GET" hoặc "POST"
- Số tokens hiện tại có trong bucket

**Kiểm thử biên**

| ID | Input | Expected Output | Actual Output |
| --- | --- | --- | --- |
| **BVT_1** | Token: `0`, Pass: `0s` | 0 Token | 0 Token |
| **BVT_2** | Token: `0`, Pass: `1s` | 1 Token | 1 Token |
| **BVT_3** | Token: `8`, Pass: `1s` | 9 Token | 9 Token |
| **BVT_4** | Token: `8`, Pass: `2s` | 10 Token | 10 Token |
| **BVT_5** | Token: `8`, Pass: `5s` | 10 Token | 10 Token |
| **BVT_6** | Token: `5`, Pass: `-3600s` | 5 Token | 5 Token |

| ID | Input | Expected Output | Actual Output |
| --- | --- | --- | --- |
| **INV_1** | Method: `"PUT"` | ArgumentException | ArgumentException |
| **INV_2** | Method: `"DELETE"` | ArgumentException | ArgumentException |
| **INV_3** | Method: `""` | ArgumentException | ArgumentException |
| **INV_4** | Method: `" "` | ArgumentException | ArgumentException |
| **INV_5** | Method: `null` | ArgumentException | ArgumentException |
| **INV_6** | Method: `"GETT"` | ArgumentException | ArgumentException |
| **INV_7** | Method: `"123!@#"` | ArgumentException | ArgumentException |
| **INV_8** | Tier: `"vip"` | ArgumentException | ArgumentException |
| **INV_9** | Tier: `""` | ArgumentException | ArgumentException |
| **INV_10**| Tier: `" "` | ArgumentException | ArgumentException |
| **INV_11**| Tier: `null` | ArgumentException | ArgumentException |
| **INV_12**| Tier: `"freee"` | ArgumentException | ArgumentException |
| **INV_13**| Tier: `"123!@#"` | ArgumentException | ArgumentException |

**Bảng quyết định**

| | Quy tắc / Rule | R1 | R2 | R3 | R4 | R5 | R6 | R7 | R8 | R9 | R10 |
| --- | --- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **Điều kiện** | **C1: Loại tài khoản là `Free`?** | T | T | T | T | F | F | F | F | F | \- |
| | **C2: Loại tài khoản là `Premium`?**| F | F | F | F | T | T | T | T | F | \- |
| | **C3: Request Method hợp lệ?** | T | T | T | T | T | T | T | T | \- | F |
| | **C4: Phương thức là `GET`?** | T | T | F | F | T | T | F | F | \- | \- |
| | **C5: Phương thức là `POST`?** | F | F | T | T | F | F | T | T | \- | \- |
| | **C6: Token dư ≥ Chi phí?** | T | F | T | F | T | F | T | F | \- | \- |
| **Hành động** | **A1: Hệ thống Duyệt (200 OK)** | X | | X | | X | | X | | | |
| | **A2: Gửi Từ chối (429 Too Many)** | | X | | X | | X | | X | | |
| | **A3: Số dư Token giữ nguyên** | | X | | X | | X | | X | | |
| | **A4: Trừ tài khoản 1 Token** | X | | | | X | | | | | |
| | **A5: Trừ tài khoản 2 Token** | | | | | | | X | | | |
| | **A6: Trừ tài khoản 3 Token** | | | X | | | | | | | |
| | **A7: Văng lỗi ArgumentException** | | | | | | | | | X | X |

**Đặt số token về 0, kết hợp bảng quyết định và các giá trị biên**

| ID | Input | Expected Output | Actual Output |
| --- | --- | --- | --- |
| **R1** | `Free`, `GET`, `0.9s` | 429, Không đổi | 429, Dư 0.9 Token |
| **R2** | `Free`, `GET`, `1.00s` | 200, Dư 0 Token | 200, Dư 0 Token |
| **R3** | `Free`, `GET`, `1.1s` | 200, Dư 0.1 Token | 200, Dư 0.1 Token |
| **R4** | `Free`, `POST`, `2.9s` | 429, Không đổi | 429, Dư 2.9 Token |
| **R5** | `Free`, `POST`, `3.00s` | 200, Dư 0 Token | 200, Dư 0 Token |
| **R6** | `Free`, `POST`, `3.1s` | 200, Dư 0.1 Token | 200, Dư 0.1 Token |
| **R7** | `Premium`, `GET`, `0.9s` | 429, Không đổi | 429, Dư 0.9 Token |
| **R8** | `Premium`, `GET`, `1.00s` | 200, Dư 0 Token | 200, Dư 0 Token |
| **R9** | `Premium`, `GET`, `1.1s` | 200, Dư 0.1 Token | 200, Dư 0.1 Token |
| **R10** | `Premium`, `POST`, `1.9s` | 429, Không đổi | 429, Dư 1.9 Token |
| **R11** | `Premium`, `POST`, `2.00s` | 200, Dư 0 Token | 200, Dư 0 Token |
| **R12** | `Premium`, `POST`, `2.1s` | 200, Dư 0.1 Token | 200, Dư 0.1 Token |

**Báo cáo Tình trạng Lỗi và Khắc phục (Bug Tracking & Fixes)**

Lỗi đã gặp:

| ID | Input | Expected Output | Actual Output |
| --- | --- | --- | --- |
| **BVT_6** | Token: `5`, Sleep: `-3600s` | 5 Token | ❌ FAIL: Token âm |
| **R10** | `Premium`, `POST`, `1.99s` | 429 | ❌ FAIL: Trả về 200 (OS trễ nhịp) |
| **INV_5** | Method: `null` | ArgumentException | ❌ FAIL: Văng NullReferenceException |
| **INV_11**| Tier: `null` | ArgumentException | ❌ FAIL: Văng NullReferenceException |

*(Sau khi phát hiện 4 bản ghi đỏ trên, thiết kế đã được khắc phục lại như sau: Bổ sung lệnh `string.IsNullOrWhiteSpace` chặn lỗi crash cho cả `INV_5` và `INV_11`, áp dụng hàm `Math.Max` chặn số âm cho `BVT_6`, và nới rộng khoảng cách "Dưới biên" bù trừ độ trễ OS cho `R10`, giúp tất cả trở lại trạng thái Xanh (Passing) ở các bảng phía trên).*


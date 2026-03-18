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

| **ID** | **Token cũ** | **Số giây trôi qua** | **Giá trị tính toán** | **Kết quả mong đợi (Token khả dụng)** | **Ý nghĩa kiểm thử** |
| --- | --- | --- | --- | --- | --- |
| **BVT_1** | 0 | 0 | 0 | **0** | Biên dưới cùng (Hết sạch token). |
| **BVT_2** | 0 | 1 | 1 | **1** | Ngay trên biên dưới. |
| **BVT_3** | 8 | 1 | 9 | **9** | Ngay dưới biên tối đa. |
| **BVT_4** | 8 | 2 | 10 | **10** | Đạt biên tối đa sức chứa. |
| **BVT_5** | 8 | 5 | 13 | **10** | Vượt biên tính toán, hệ thống phải chặn ở mức 10. |
| **BVT_6** | 5 | -3600 | -3595 | **5** | Kiểm tra lỗi ngược thời gian (Time Drift) để đảm bảo token không bị rớt xuống số âm. |

**Bảng quyết định**

| **Điều kiện / Quy tắc** | **R1** | **R2** | **R3** | **R4** | **R5** | **R6** | **R7** | **R8** | **R9** | **R10** |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| **Loại tài khoản (T)** | free | free | free | free | premium | premium | premium | premium | Khác |  |
| **Phương thức (M)** | GET | GET | POST | POST | GET | GET | POST | POST |  | Khác |
| **Đủ Token tương ứng?** | Có | Không | Có | Không | Có | Không | Có | Không |  |  |
| **Hành động: Duyệt (200 OK)** | **X** |  | **X** |  | **X** |  | **X** |  |  |  |
| **Hành động: Từ chối (429)** |  | **X** |  | **X** |  | **X** |  | **X** |  |  |
| **Tokens - 1** | **X** |  |  |  | **X** |  |  |  |  |  |
| **Tokens - 2** |  |  |  |  |  |  | **X** |  |  |  |
| **Tokens - 3** |  |  | **X** |  |  |  |  |  |  |  |
| **Tokens không đổi** |  | **X** |  | **X** |  | **X** |  | **X** |  |  |
| **Báo input không hợp lệ** |  |  |  |  |  |  |  |  | **X** | **X** |

**Đặt số token về 0, kết hợp bảng quyết định và các giá trị biên**

| **Quy tắc (Rule)** | **Tier** | **Method** | **Thời gian chờ (t)** | **Kết quả mong đợi** | **Kết quả thực tế** |
| --- | --- | --- | --- | --- | --- |
| **R1 (Dưới biên)** | Free | GET | **0.95s** | **Từ chối (429)** |  |
| **R2 (Tại biên)** | Free | GET | **1.00s** | **Duyệt (200)** |  |
| **R3 (Trên biên)** | Free | GET | **1.01s** | **Duyệt (200)** |  |
| **R4 (Dưới biên)** | Free | POST | **2.95s** | **Từ chối (429)** |  |
| **R5 (Tại biên)** | Free | POST | **3.00s** | **Duyệt (200)** |  |
| **R6 (Trên biên)** | Free | POST | **3.01s** | **Duyệt (200)** |  |
| **R7 (Dưới biên)** | Premium | GET | **0.95s** | **Từ chối (429)** |  |
| **R8 (Tại biên)** | Premium | GET | **1.00s** | **Duyệt (200)** |  |
| **R9 (Trên biên)** | Premium | GET | **1.01s** | **Duyệt (200)** |  |
| **R10 (Dưới biên)** | Premium | POST | **1.95s** | **Từ chối (429)** |  |
| **R11 (Tại biên)** | Premium | POST | **2.00s** | **Duyệt (200)** |  |
| **R12 (Trên biên)** | Premium | POST | **2.01s** | **Duyệt (200)** |  |
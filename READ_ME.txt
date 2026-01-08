--------------------------------------------------------Cách chạy được WebAPI------------------------------------------------------

Bước 1: Đổ dữ liệu vào Database
Mở SQL Server Management Studio -> Tạo CSDL có tên HSO -> Sử dụng file HSO.sql đã được chuẩn bị sẵn trong folder WebAPI project.

Bước 2: Cấu hình port
Mở PowerShell Run by Administrator -> gõ netsh http add urlacl url=http://localhost:55555/ user=Everyone. Có thể chọn port khác nếu port 55555 bị chiếm.
Nếu port bị chặn?
Mở Window Defender Firewall with Advanced Security -> click phải vào Inbound Rules -> khai báo port và để quyền truy cập mở.

Bước 3: Chỉ định port cho WebAPI
Mở WebAPI project bằng Visual Studio -> chọn Project ở trên cùng màn hình -> HSO_WebAPI Properties -> Web ở bên trái màn hình -> chỉ định port ở mục Project Url: http://localhost:55555/ (Nếu Visual Studio có hỏi Create Virtual Directory thì cứ YES mà bấm) -> Save -> Run ở dạng IIS Express.

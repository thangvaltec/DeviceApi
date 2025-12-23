# HƯỚNG DẪN THIẾT LẬP DATABASE (CHI TIẾT)

Tài liệu này hướng dẫn chính xác bạn cần chạy script nào vào database nào để hệ thống Smart Routing hoạt động.

---

## 1. Cơ sở dữ liệu TỔNG: `DBmanager`
Đây là database trung tâm dùng để quản lý danh sách khách hàng (Tenants) và định tuyến thiết bị toàn hệ thống.

**Script chạy trên `DBmanager`:**
```sql
-- A. Tạo bảng danh sách khách hàng
CREATE TABLE IF NOT EXISTS contract_client (
    "ContractClientCd" VARCHAR(50) PRIMARY KEY, -- Ví dụ: '777', '888', 'MasterDb'
    "ContractClientName" VARCHAR(200) NOT NULL,
    "DelFlg" BOOLEAN DEFAULT false,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- B. Tạo bảng định tuyến thiết bị (Master Routing Board)
CREATE TABLE IF NOT EXISTS device_routing (
    "SerialNo" VARCHAR(100) PRIMARY KEY,
    "ContractClientCd" VARCHAR(50) NOT NULL, -- Chỏ tới tenant đang sở hữu thiết bị
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- C. Đăng ký các Tenant khởi tạo
-- Quan trọng: 'MasterDb' là tên database chứa thiết bị mới
INSERT INTO contract_client ("ContractClientCd", "ContractClientName")
VALUES ('777', 'Khách hàng thử nghiệm 777'),
       ('MasterDb', 'Hồ chứa thiết bị mới (Discovery Pool)')
ON CONFLICT ("ContractClientCd") DO NOTHING;
```

---

## 2. Cơ sở dữ liệu PHỤ: `MasterDb`
Đây là nơi chứa dữ liệu của những thiết bị **mới tinh**, chưa được đăng ký vào bất kỳ khách hàng nào.

**Script chạy trên `MasterDb`:**
*(Sử dụng chung cấu trúc với Tenant DB bên dưới)*
```sql
-- Chạy script ở Mục 4 bên dưới vào database này.
```

---

## 3. Cơ sở dữ liệu KHÁCH HÀNG: `777` (hoặc các code khác)
Đây là nơi chứa dữ liệu riêng tư của từng khách hàng (Thiết bị, Log, User quản trị).

**Script chạy trên `777`:**
*(Sử dụng chung cấu trúc với Tenant DB bên dưới)*
```sql
-- Chạy script ở Mục 4 bên dưới vào database này.
```

---

## 4. CẤU TRÚC CHUNG CHO TENANT DB (Chạy cho cả `MasterDb` và `777`)
Hãy copy script này và chạy vào **mọi** database tenant mà bạn có.

```sql
-- 1. Bảng thiết bị (Bản sao cục bộ của Tenant)
CREATE TABLE IF NOT EXISTS devices (
    "Id" SERIAL PRIMARY KEY,
    "SerialNo" VARCHAR(100) UNIQUE NOT NULL,
    "DeviceName" VARCHAR(200),
    "AuthMode" INTEGER DEFAULT 0, -- 0:Face, 1:Vein, 2:Both
    "IsActive" BOOLEAN DEFAULT true,
    "DelFlg" BOOLEAN DEFAULT false,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Bảng lưu nhật ký xác thực (Android gửi về đây)
CREATE TABLE IF NOT EXISTS auth_logs (
    "Id" SERIAL PRIMARY KEY,
    "SerialNo" VARCHAR(100) NOT NULL,
    "UserId" VARCHAR(100),
    "UserName" VARCHAR(200),
    "AuthMode" INTEGER,
    "IsSuccess" BOOLEAN,
    "ErrorMessage" TEXT,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. Bảng người dùng quản trị (Để log in vào Web 777 hoặc 9999)
CREATE TABLE IF NOT EXISTS admin_users (
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(100) UNIQUE NOT NULL,
    "PasswordHash" TEXT NOT NULL,
    "Role" VARCHAR(50) DEFAULT 'admin',
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 4. Bảng nhật ký thao tác (Theo dõi ai đã sửa thiết bị)
CREATE TABLE IF NOT EXISTS device_logs (
    "Id" SERIAL PRIMARY KEY,
    "SerialNo" VARCHAR(100) NOT NULL,
    "Action" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 5. Tạo user quản trị mặc định (User: admin / Pass: valtec)
INSERT INTO admin_users ("Username", "PasswordHash", "Role")
VALUES ('admin', '8bc24564c4897f26139cdcc9656b823e2079f5fdd72e519c72462e0717208d98', 'super_admin')
ON CONFLICT ("Username") DO NOTHING;
```

---

## Tóm tắt Luồng Hoạt Động:
1. Android gọi API -> Server kiểm tra `DBmanager.device_routing`.
2. Nếu không thấy -> Trả về kết quả từ `MasterDb`.
3. Nếu thấy (ví dụ máy 123 thuộc về 777) -> Trả về kết quả từ `777`.
4. Khi bạn thêm máy vào Web `777` -> Server tự động cập nhật bảng `DBmanager.device_routing` để trỏ máy đó về `777`.

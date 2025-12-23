# HƯỚNG DẪN THIẾT LẬP DATABASE (PHỤC HỒI PASCALCASE)

Tôi đã khôi phục lại định dạng **PascalCase** (chữ Hoa đầu từ) cho toàn bộ hệ thống để khớp với lịch sử dự án của bạn, ngoại trừ bảng `admin_users`.

---

## 1. Cho Master DB: `DBmanager`
```sql
CREATE TABLE IF NOT EXISTS contract_client (
    "Id" SERIAL PRIMARY KEY,
    "ContractClientCd" VARCHAR(50) UNIQUE NOT NULL,
    "ContractClientName" VARCHAR(200),
    "DelFlg" BOOLEAN DEFAULT false,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS device_routing (
    "SerialNo" VARCHAR(100) PRIMARY KEY,
    "ContractClientCd" VARCHAR(50) NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

INSERT INTO contract_client ("ContractClientCd", "ContractClientName")
VALUES ('777', 'Khách hàng 777'),
       ('MasterDb', 'Hồ chứa thiết bị mới')
ON CONFLICT ("ContractClientCd") DO NOTHING;
```

---

## 2. Cho Tenant DB: `MasterDb` và `777`
Bạn hãy chạy script này cho từng Tenant DB.

**Chú ý**: Chỉ bảng `admin_users` dùng chữ thường. Các bảng khác dùng chữ Hoa trong ngoặc kép.

```sql
-- 1. Bảng devices
CREATE TABLE IF NOT EXISTS devices (
    "Id" SERIAL PRIMARY KEY,
    "SerialNo" VARCHAR(100) UNIQUE NOT NULL,
    "DeviceName" VARCHAR(200),
    "AuthMode" INTEGER DEFAULT 0,
    "IsActive" BOOLEAN DEFAULT true,
    "DelFlg" BOOLEAN DEFAULT false,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Bảng auth_logs
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

-- 3. Bảng admin_users (CHỮ THƯỜNG - KHÔNG NGOẶC KÉP)
CREATE TABLE IF NOT EXISTS admin_users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    role VARCHAR(50) DEFAULT 'admin',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 4. Bảng device_logs
CREATE TABLE IF NOT EXISTS device_logs (
    "Id" SERIAL PRIMARY KEY,
    "SerialNo" VARCHAR(100) NOT NULL,
    "Action" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 5. Tạo user admin (Pass: valtec)
INSERT INTO admin_users (username, password_hash, role)
VALUES ('admin', '39f8485ae66793496c7f4e437acfa60d3905653ea01ca155cf1b5d05446f3702', 'super_admin')
ON CONFLICT (username) DO UPDATE SET password_hash = EXCLUDED.password_hash;
```

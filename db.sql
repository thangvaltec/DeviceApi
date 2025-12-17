/* 契約ごとのテーブル作成スクリプト */

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" varchar(150) NOT NULL,
    "ProductVersion" varchar(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

BEGIN;

CREATE TABLE IF NOT EXISTS admin_users (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    role TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS device_logs (
    "Id" SERIAL PRIMARY KEY,
    "SerialNo" TEXT NOT NULL,
    "Action" TEXT NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS devices (
    "Id" SERIAL PRIMARY KEY,
    "SerialNo" TEXT NOT NULL,
    "DeviceName" TEXT NOT NULL,
    "AuthMode" INTEGER NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    "DelFlg" BOOLEAN NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL
);

-- ============================================================================
-- Table for Authentication History (AuthHistory Screen)
-- ============================================================================
CREATE TABLE IF NOT EXISTS auth_logs (
    "Id" SERIAL PRIMARY KEY,
    "SerialNo" TEXT NOT NULL,       -- Matches devices."SerialNo"
    "UserId" TEXT NOT NULL,         -- ID of the person attempting auth
    "UserName" TEXT,                -- Name of the person (Optional, from Face Face)
    "AuthMode" INTEGER NOT NULL,    -- 0:Face, 1:Vein, 2:Dual
    "IsSuccess" BOOLEAN NOT NULL,
    "ErrorMessage" TEXT,            -- Reason for failure
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP -- Matches device_logs."CreatedAt"
);

-- 明示的に ID = 1 を登録（PostgreSQLでは IDENTITY_INSERT 不要）
INSERT INTO admin_users (
    id, created_at, password_hash, role, username
) VALUES (
    1,
    TIMESTAMP '2025-12-05 00:00:00',
    '39f8485ae66793496c7f4e437acfa60d3905653ea01ca155cf1b5d05446f3702',
    'super_admin',
    'admin'
)
ON CONFLICT (id) DO NOTHING;

-- シーケンスを 1 以上に調整（SERIAL対策）
SELECT setval(
    pg_get_serial_sequence('admin_users', 'id'),
    (SELECT MAX(id) FROM admin_users)
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251208033307_InitClean', '8.0.10')
ON CONFLICT ("MigrationId") DO NOTHING;

COMMIT;

/* 共通テナント */

CREATE TABLE  IF NOT EXISTS contract_client (
    "Id" SERIAL PRIMARY KEY,
    "ContractClientCd" TEXT NOT NULL,
    "ContractClientName" TEXT NOT NULL,
    "DelFlg" BOOLEAN NOT NULL,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL
);

INSERT INTO public.contract_client(
	"ContractClientCd", "ContractClientName", "DelFlg", "CreatedAt", "UpdatedAt")
	VALUES ('1234', 'テスト用１', false, current_timestamp, current_timestamp);

COMMIT;
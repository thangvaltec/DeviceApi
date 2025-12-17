using DeviceApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// サービス登録 
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// フロントエンド（Vite開発サーバー）向けのCORS設定
// フロントエンド（Vite開発サーバー）向けのCORS設定
// 開発中は全オリジンを許可するため、allowedOriginsは一時的に無効化
/*
var allowedOrigins = new[]
{
    "http://localhost:5173",
    "http://10.200.2.29:5173"
};
*/

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendCors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// DbContextの登録（PostgreSQLを使用）
builder.Services.AddDbContext<ContractClientDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddSingleton<ContractClientDbContextFactory>();

var app = builder.Build();

// ===== HTTPパイプライン設定 =====
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 開発用の内部環境ではHTTPSを強制しない（必要なら下行を有効化）
// app.UseHttpsRedirection();

// ===== 静的ファイル配信（フロントエンドビルド成果物） =====
app.UseDefaultFiles();  // wwwroot 内の index.html を自動探索
app.UseStaticFiles();   // wwwroot から静的ファイルを返却
// ========================================================

app.UseCors("FrontendCors"); // フロントエンドからのAPI呼び出しを許可

app.UseAuthorization();

app.MapControllers();   // /api/... エンドポイントを有効化

app.Run();

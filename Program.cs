// ================== 依赖引用区域 ==================
// 引入应用程序的数据库上下文（封装了所有实体与 EF Core 配置）
using LibraryManagement.LM.Common.AppDbContext;
// 引入自定义的全局异常处理中间件扩展方法
using LibraryManagement.LM.Common.Exceptions;
// 引入仓储接口与实现（Repository 层）
using LibraryManagement.LM.Service.Repository;
using LibraryManagement.LM.Service.Repository.Impl;
// 引入业务服务接口与实现（Service 层）
using LibraryManagement.LM.Service.Services;
using LibraryManagement.LM.Service.Services.Impl;
// 引入用于绑定配置的选项类（例如 DashScope AI 配置）
// 引入 EF Core 提供的扩展方法（例如 UseSqlServer）
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using LibraryManagement.ai.Options;
using LibraryManagement.ai.services;
using LibraryManagement.ai.services.Impl;

// 创建 Web 应用程序构建器，负责收集配置、注册服务等
var builder = WebApplication.CreateBuilder(args);

// ================== Services 配置（依赖注入容器） ==================
// 配置跨域策略：允许前端 Vue 项目（http://localhost:6060）访问本 API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost:6060"
            )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

// ================== JWT 认证配置 ==================
var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException("JWT Key is not configured. Please set configuration value 'Jwt:Key'.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            RoleClaimType = "role"
        };
    });


// ================== JSON 全局配置（HttpResults + MVC 统一序列化规则） ==================
// 这里的 ConfigureHttpJsonOptions 主要影响 Minimal API / HttpResults 的序列化行为
builder.Services.ConfigureHttpJsonOptions(options =>
{
    // 统一使用 camelCase 属性命名，方便前端（JavaScript）直接消费
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    // 忽略对象图中的循环引用，避免序列化时抛出异常（例如实体之间互相引用）
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

// 这里的 AddControllers().AddJsonOptions 主要作用于传统 MVC Controller 的 JSON 返回
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 同样使用 camelCase，并忽略循环引用，保持与 HttpResults 一致
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 注册 SignalR，用于实时通信（例如通知推送、聊天等）
builder.Services.AddSignalR();

// 路由选项：强制生成的小写 URL，统一风格，避免大小写带来的路由差异
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

// 添加 API 终结点的探索器（Swagger 生成所需的元数据）
builder.Services.AddEndpointsApiExplorer();

// ================== Swagger 文档配置 ==================
// 使用 Swashbuckle 生成 OpenAPI/Swagger 文档与可视化界面
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        // 在 Swagger UI 中显示的标题与描述信息
        Title = "图书管理系统API",
        Version = "v1",
        Description = "管理员接口、图书管理、借阅、认证相关接口"
    });
});

// ================== 数据库上下文配置 ==================
// 注册 ApplicationDbContext，并指定使用 SQL Server 与配置文件中的连接字符串
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// ================== 仓储（Repository）依赖注入 ==================
// 作用域生命周期（Scoped）：每个 HTTP 请求创建一次实例
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();

// ================== 业务服务（Service）依赖注入 ==================
// Service 层封装了具体的业务逻辑，对外提供更高层次的接口
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOssService, OssService>();

// ================== AI 相关配置与服务 ==================
// 从配置文件中绑定 DashScope 相关配置（如 API Key、模型名称等）
builder.Services.Configure<DashScopeOptions>(builder.Configuration.GetSection("DashScope"));
// 注册可复用的 HttpClient，用于调用外部 HTTP 服务（例如通义千问 API）
builder.Services.AddHttpClient();
// 封装与通义千问（Qwen）交互的底层调用逻辑
builder.Services.AddScoped<QwenApiService>();
// 对外暴露一个通用的聊天 / 问答服务接口，供 Controller 调用
builder.Services.AddScoped<IChatService, ChatService>();

// ================== 构建应用程序实例 ==================
var app = builder.Build();

// ================== 中间件管线配置 ==================
// 仅在开发环境启用 Swagger，便于调试与接口测试
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用自定义的全局异常处理中间件，统一捕获并格式化异常响应
app.UseExceptionHandlerMiddleware();
// 强制将 HTTP 请求重定向为 HTTPS，提高安全性（生产环境视部署情况可调整）
app.UseHttpsRedirection();

// 启用路由匹配（应在大多数中间件之前/之后保持合理顺序）
app.UseRouting();
// 启用前面配置的 CORS 策略，允许前端跨域访问
app.UseCors("AllowAll");
// 启用认证中间件，必须在 UseAuthorization 之前
app.UseAuthentication();
// 启用授权中间件（如后续增加认证/授权机制，会在此处生效）
app.UseAuthorization();
// 映射所有基于控制器的路由到终结点管线
app.MapControllers();

// 启动 Web 应用程序，开始监听 HTTP 请求（整个程序入口的最后一步）
app.Run();
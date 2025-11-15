using LibraryManagement.AppDbContext;
using LibraryManagement.Repository;
using LibraryManagement.Repository.Impl;
using LibraryManagement.Services;
using LibraryManagement.Services.Impl;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",policy =>
    {
        policy.WithOrigins(
            "http://localhost:5173", //vite默认端口
            "https://localhost:5173"
            )
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials(); //允许凭证
    });
});

//配置JSON序列化
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy =
    System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// 添加服务
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // 让 JSON 字段变成 camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 启用小写 URL 路由 ← 关键！
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加数据库上下文
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    .LogTo(Console.WriteLine, LogLevel.Information) //输出SQL语句到控制台
    );

//添加仓储层
builder.Services.AddScoped<IBookRepository, BookRepository>();
//添加服务层
builder.Services.AddScoped<IBookService, BookService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

//app.UseAuthorization(); //TODO: 等待开发

app.MapControllers();

app.Run();

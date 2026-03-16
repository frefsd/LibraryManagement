using LibraryManagement.LM.Common.AppDbContext;
using LibraryManagement.LM.Common.Exceptions;
using LibraryManagement.LM.Service.Repository;
using LibraryManagement.LM.Service.Repository.Impl;
using LibraryManagement.LM.Service.Services;
using LibraryManagement.LM.Service.Services.Impl;
using LibraryManagement.Options;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

//=========Services 配置============
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
            "http://localhost:6060"
            )
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});


// JSON 全局配置（HttpResults + MVC）
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddSignalR();

builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "图书管理系统API",
        Version = "v1",
        Description = "管理员接口、图书管理、借阅、认证相关接口"
    });
});
// ====================================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

//仓储
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPublisherRepository, PublisherRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBorrowRepository, BorrowRepository>();

//服务
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IBorrowService, BorrowService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IOssService, OssService>();

// AI
builder.Services.Configure<DashScopeOptions>(builder.Configuration.GetSection("DashScope"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<QwenApiService>();
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandlerMiddleware();
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.UseRouting();
app.MapControllers();

app.Run();
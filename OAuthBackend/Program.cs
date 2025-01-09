using Pomelo.EntityFrameworkCore.MySql; // 別忘了要在頂端 using
using Microsoft.EntityFrameworkCore;
using OAuthBackend.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 添加 CORS 服務
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        corsBuilder => corsBuilder
            .WithOrigins("http://35.212.232.36:8080") // Vue 應用的地址
            .AllowAnyMethod()
            .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// 改用 MySQL
builder.Services.AddDbContext<OAuthDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        // 版本改成你實際的 MySQL 版本，以下示範 8.0.32
        new MySqlServerVersion(new Version(8, 0, 32)),
        b => b.MigrationsAssembly("OAuthBackend")
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 使用 CORS - 必須在其他 middleware 之前
app.UseCors("AllowVueApp");

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.Run();
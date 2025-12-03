using EStore.Entity.Models;
using EStore.Services.Interfaces;
using EStore.Services.Interfaces.GenericClient;
using EStore.Services.Repositories;
using EStore.Services.Repositories.GenericClient;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Controllers + JSON Options ----------
builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        // ensure enums return string values
        opt.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        opt.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ---------- EF Core ----------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Swagger ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ---------- HttpClient + HttpContext ----------
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

// ---------- CORS ----------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// ------------------------------------------------
// GenericUserClientRepository registration
// ------------------------------------------------
builder.Services.AddHttpClient<IGenericUserClientRepository, GenericUserClientRepository>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiConfiguration:ClientUrl"]);
});

// ---------- Application Services ----------
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryAttributeService, CategoryAttributeService>();
builder.Services.AddScoped<IProductService, ProductService>();



var app = builder.Build();

// ---------- Middleware ----------
app.UseSwagger();
app.UseSwaggerUI();

// Serve static files (uploads folder)
app.UseStaticFiles();

// CORS
app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

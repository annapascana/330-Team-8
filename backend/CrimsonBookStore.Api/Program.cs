using CrimsonBookStore.Api.Data;
using CrimsonBookStore.Api.Repositories;
using CrimsonBookStore.Api.Services;
using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Add MVC for serving frontend pages
builder.Services.AddControllersWithViews();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Session for cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// CORS - Configured for session cookies (allow both HTTP and HTTPS in development)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(origin => 
            origin == "http://localhost:5000" || 
            origin == "http://127.0.0.1:5000" ||
            origin == "https://localhost:5001" ||
            origin == "https://127.0.0.1:5001" ||
            origin.StartsWith("http://localhost:") ||
            origin.StartsWith("http://127.0.0.1:") ||
            origin.StartsWith("https://localhost:") ||
            origin.StartsWith("https://127.0.0.1:"))
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for session cookies
    });
});

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddSingleton<IDbConnectionFactory>(new DbConnectionFactory(connectionString));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<ISellSubmissionRepository, SellSubmissionRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ISellSubmissionService, SellSubmissionService>();
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// CORS must come after UseRouting but before UseEndpoints/MapControllers
app.UseCors("AllowAll");

// Disable HTTPS redirection in development to avoid CORS issues
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseSession();
app.UseAuthorization();

// Map API controllers FIRST (so /api routes take precedence)
app.MapControllers();

// Serve static files from frontend directory (CSS, JS, images)
var frontendPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "frontend");
if (Directory.Exists(frontendPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });
    
    // Default file
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(frontendPath),
        RequestPath = ""
    });
    
    // Fallback to index.html for non-API routes only
    app.MapFallback(async context =>
    {
        var path = context.Request.Path.Value?.TrimStart('/') ?? "";
        
        // Don't handle API routes
        if (path.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 404;
            return;
        }
        
        // Default to index.html
        var indexPath = Path.Combine(frontendPath, "index.html");
        if (File.Exists(indexPath))
        {
            context.Response.ContentType = "text/html";
            await context.Response.SendFileAsync(indexPath);
        }
        else
        {
            context.Response.StatusCode = 404;
        }
    });
}

// Configure URLs - use HTTP for development to avoid redirect issues
if (app.Environment.IsDevelopment())
{
    app.Urls.Clear();
    app.Urls.Add("http://localhost:5000");
}
else
{
    app.Urls.Add("http://localhost:5000");
    app.Urls.Add("https://localhost:5001");
}

app.Run();


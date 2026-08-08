using Microsoft.Data.Sqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Register SQLite connection from configuration
builder.Services.AddSingleton<SqliteConnection>(sp =>
{
    var connString = builder.Configuration.GetConnectionString("SalesDb");
    return new SqliteConnection(connString);
});

// Enable CORS to allow requests from React client
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseCors("AllowAll");  // Apply CORS policy
app.MapControllers();

app.Run();
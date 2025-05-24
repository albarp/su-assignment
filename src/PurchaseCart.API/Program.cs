using PurchaseCart.DataAccessSqlite;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SQLite services
var environment = builder.Environment.EnvironmentName.ToLower();
// TODO: move to configuration
var connectionString = $"Data Source=purchasecart.{environment}.db";

builder.Services.AddSingleton<DBSchemaInitializer>(sp => 
    new DBSchemaInitializer(connectionString, sp.GetRequiredService<ILogger<DBSchemaInitializer>>()));

builder.Services.AddSingleton<DBSeeder>(sp => 
    new DBSeeder(connectionString, sp.GetRequiredService<ILogger<DBSeeder>>()));

var app = builder.Build();

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DBSchemaInitializer>();
    initializer.Initialize();

    var seeder = scope.ServiceProvider.GetRequiredService<DBSeeder>();
    seeder.Seed();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

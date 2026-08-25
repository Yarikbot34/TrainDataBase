using API.BuilderFuctions;
using DB;
using DB.Repositories;
using FileWorker;
using Microsoft.EntityFrameworkCore;
using Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IRouteRepo, RouteRepo>();
builder.Services.AddScoped<ITrainRepo, TrainRepo>();
builder.Services.AddScoped<IStationRepo, StationRepo>();
builder.Services.AddScoped<ITransactionRepo, TransactionRepo>();
builder.Services.AddScoped<IMapSchemaRepo, MapSchemaRepo>();
builder.Services.AddScoped<IMapCellRepo, MapCellRepo>();

builder.Services.AddScoped<IFileWorker, FileWorkerService>();

builder.Services.AddScoped<ITrainService, TrainService>();
builder.Services.AddScoped<ISummaryDataService, SummaryDataService>();
builder.Services.AddScoped<IdbContentService, DbContentService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IMapService, MapService>();

builder.SetupJWT();

builder.Services.AddAuthentication();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{ options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();  
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>(); 
    
    dbContext.Database.Migrate();
}

app.UseCors("AllowAll");
app.UseSwagger();
app.MapHealthChecks("/health");
app.UseSwaggerUI();
app.MapControllers();
app.Run();
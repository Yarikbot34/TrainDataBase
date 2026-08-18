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

builder.Services.AddScoped<IFileWorker, FileWorker.FileWorker>();

builder.Services.AddScoped<ITrainService, TrainService>();
builder.Services.AddScoped<ISummaryDataService, SummaryDataService>();
builder.Services.AddScoped<IdbContentService, DbContentService>();
builder.Services.AddScoped<IRouteService, RouteService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IMapService, MapService>();

builder.Services.AddControllers();
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

app.UseCors("AllowAll");
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Run();
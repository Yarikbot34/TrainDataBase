using DB;
using TableReader;
using Microsoft.EntityFrameworkCore;
using Services;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped<IRawDataRepo, RawDataRepo>();
builder.Services.AddScoped<ITableReader, TrainExtractor>();
builder.Services.AddScoped<ITrainRepo, TrainRepo>();
builder.Services.AddScoped<ISummaryDataRepo, SummaryDataRepo>();
builder.Services.AddScoped<IdbContentRepo, DbContentRepo>();
builder.Services.AddScoped<IRouteRepo, RouteRepo>();

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
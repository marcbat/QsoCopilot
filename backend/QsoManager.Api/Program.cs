using QsoManager.Application;
using QsoManager.Infrastructure;
using QsoManager.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Swagger/OpenAPI avec Swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "QSO Manager API", 
        Version = "v1",
        Description = "API REST pour la gestion des QSO (Event Sourcing + CQRS)"
    });
});

// Add our layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Domain services
builder.Services.AddScoped<IQsoAggregateService, QsoAggregateService>();

// CORS policy for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "QSO Manager API v1");
        c.RoutePrefix = "swagger"; // Swagger sera accessible à /swagger
    });
    app.UseCors("Development");
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

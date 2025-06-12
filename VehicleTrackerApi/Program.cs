var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();          // Enables controller support
builder.Services.AddEndpointsApiExplorer(); // For Swagger/OpenAPI
builder.Services.AddSwaggerGen();           // Swagger UI generation

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();  // Map controller routes

app.Run();

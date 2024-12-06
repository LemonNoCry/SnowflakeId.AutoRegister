using Yitter.IdGenerator.AutoRegister.AspCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((context, configurationBuilder) =>
{
    App.Configuration = context.Configuration;
    configurationBuilder.AddJsonFile("appsettings.json", true, false);
    configurationBuilder.AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json",
        true, false);
});

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
App.Service = app.Services;
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Lifetime.ApplicationStopping.Register(IdGeneratorUtil.UnRegister);
app.Run();
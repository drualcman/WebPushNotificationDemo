using PushServerApi.Options;
using PushServerApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddOptions().Configure<VapidInfoOptions>(builder.Configuration.GetSection(VapidInfoOptions.SectionKey));
builder.Services.AddScoped<PushNotificationService>();
builder.Services.AddWebPushService();

builder.Services.AddCors(c => 
{
    c.AddDefaultPolicy(p => 
    {
        p.AllowAnyMethod();
        p.AllowAnyHeader();
        p.AllowAnyOrigin();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("subscribe", async (Entities.WebPushsubscrition subscription, PushNotificationService service) => 
{
    await service.Subscribe(subscription);
    return Results.Ok();
});

app.MapGet("request-example-notification", (PushNotificationService service) => 
{
    service.SendExampleNotification(); 
    return Results.Ok();
});

app.UseCors();


app.Run();


using Avtotest.Web.Options;
using Avtotest.Web.Repositories;
using Avtotest.Web.Services;

var builder = WebApplication.CreateBuilder(args);

//var configuration = builder.Configuration;
//var settings = configuration.GetSection("TicketSettings").Get<TicketSettings>();

builder.Services.Configure<TicketSettings>(
    builder.Configuration.GetSection("TicketSettings"));

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<UsersService>();
builder.Services.AddScoped<CookiesService>();
builder.Services.AddScoped<QuestionsRepository>();
builder.Services.AddScoped<TicketsRepository>();
builder.Services.AddScoped<UsersRepository>();

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using Mignow.Atc.Api.Bootstrap;

var builder = WebApplication.CreateBuilder(args);
builder.AddAtcApi();
var app = builder.Build();
await app.UseAtcApiAsync();
app.Run();

public partial class Program;

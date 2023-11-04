var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseMiddleware<RawRequestLoggingMiddleware>();
app.Run();


public class RawRequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RawRequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var injectedRequestStream = new MemoryStream();
        var originalRequestBody = context.Request.Body;

        await context.Request.Body.CopyToAsync(injectedRequestStream);
        injectedRequestStream.Seek(0, SeekOrigin.Begin);

        string requestBody = new StreamReader(injectedRequestStream).ReadToEnd();
        injectedRequestStream.Seek(0, SeekOrigin.Begin);
        context.Request.Body = injectedRequestStream;

        // Log the raw request body
        System.IO.File.WriteAllText($"sample-{context.Connection.Id}-{context.Request.Path.ToString().Replace("/", string.Empty)}", requestBody);

        await _next(context);
        context.Request.Body = originalRequestBody;
    }
}

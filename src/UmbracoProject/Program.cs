using Joonasw.AspNetCore.SecurityHeaders;
using System.Threading.Tasks;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//HSTS options, maxage = 2 years 
builder.Services.AddHsts(options =>
{
	options.Preload = true;
	options.IncludeSubDomains = true;
	options.MaxAge = TimeSpan.FromSeconds(63072000);
	options.ExcludedHosts.Add("example.com");
	options.ExcludedHosts.Add("www.example.com");
});

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

//security course addition for HSTS, excercise 4
if (!app.Environment.IsDevelopment())
{
	app.UseHsts();
}

app.UseHttpsRedirection();

/** clickjacking prevention
app.Use(async (context, next) =>
{
	context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
	context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
	context.Response.Headers.Add("Content-Security-Policy", "default-src 'self' packages.umbraco.org our.umbraco.org;script-src 'self' ajax.googleapis.com unpkg.com ajax.aspnetcdn.com cdnjs.cloudflare.com cdn.jsdelivr.net;style-src 'self' 'unsafe-inline' fonts.googleapis.com cdn.jsdelivr.net cdnjs.cloudflare.com cdn.linearicons.com;img-src 'self' via.placeholder.com data: *.googleapis.com;font-src 'self' cdnjs.cloudflare.com fonts.gstatic.com cdn.linearicons.com frame-ancestors 'self'"); 
    await next();
});
**/

//CSP
app.UseCsp(csp =>
{
	csp.ByDefaultAllow.FromSelf().From("packages.umbraco.org our.umbraco.org");
	csp.AllowScripts.FromSelf().From("ajax.googleapis.com unpkg.com ajax.aspnetcdn.com cdnjs.cloudflare.com cdn.jsdelivr.net");
	csp.AllowStyles.FromSelf().From("fonts.googleapis.com cdn.jsdelivr.net cdnjs.cloudflare.com cdn.linearicons.com").AllowUnsafeInline();
	csp.AllowImages.FromSelf().From("data: via.placeholder.com");
	csp.AllowFonts.FromSelf().From("data: cdnjs.cloudflare.com fonts.gstatic.com cdn.linearicons.com");

	csp.AllowFraming.FromSelf();

	csp.OnSendingHeader = context =>
	{
		context.ShouldNotSend = context.HttpContext.Request.Path.StartsWithSegments("/umbraco");
		return Task.CompletedTask;
	};
});

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();

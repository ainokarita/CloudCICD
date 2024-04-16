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

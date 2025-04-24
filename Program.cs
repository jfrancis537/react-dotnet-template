
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Net.Http.Headers;

namespace events;

public class Program
{
  public static void Main(string[] args)
  {
    var builder = WebApplication.CreateBuilder(args);
    ConfigureServices(builder.Services, builder.Configuration);

    var app = builder.Build();
    ConfigureApp(app);
  }

  public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
  {
    services.AddControllers();
    services.AddAuthentication(options =>
    {
      options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    }).AddCookie().AddOpenIdConnect(options =>
    {
      var oidcConfig = configuration.GetSection("Authentication");

      options.Authority = oidcConfig["Authority"];
      options.ClientId = oidcConfig["ClientId"];
      options.ClientSecret = oidcConfig["ClientSecret"];

      var scopes = oidcConfig["scopes"];

      if (scopes != null)
      {
        foreach (var scope in scopes.Split(","))
        {
          options.Scope.Add(scope);
        }
      }

      options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
      options.ResponseType = OpenIdConnectResponseType.Code;

      options.SaveTokens = true;
      options.GetClaimsFromUserInfoEndpoint = true;
      options.MapInboundClaims = false;
      options.RequireHttpsMetadata = true;

      options.TokenValidationParameters.NameClaimType = ClaimConstants.PreferredUserName;
      options.TokenValidationParameters.ValidAudience = oidcConfig["ClientId"];

      options.Events = new OpenIdConnectEvents
      {

        OnRedirectToIdentityProvider = context =>
        {
          var pathValue = context.Request.Path.Value;
          if (pathValue != null && pathValue.EndsWith("api/auth/login"))
          {
            context.Response.Redirect(context.ProtocolMessage.CreateAuthenticationRequestUrl());
          }
          else
          {
            context.HandleResponse();
            context.Response.StatusCode = 401;
          }
          return Task.CompletedTask;
        }
      };
    });

    services.AddSpaStaticFiles(configuration =>
    {
      configuration.RootPath = "client/dist";
    });
  }

  public static void ConfigureApp(WebApplication app)
  {
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
      app.UseDeveloperExceptionPage();
    }
    else
    {
      app.UseExceptionHandler("/Error");
      // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
      app.UseHsts();
    }
    app.UseStaticFiles();
    app.UseSpaStaticFiles();
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
      ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    app.UseAuthentication();
    app.UseRouting();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller}/{action=Index}/{id?}");
    });

    app.UseSpa(spa =>
      {
        spa.Options.SourcePath = "client";
        spa.Options.DefaultPageStaticFileOptions = new StaticFileOptions()
        {
          OnPrepareResponse = config =>
          {
            var ctx = config.Context;
            var headers = ctx.Response.GetTypedHeaders();
            headers.CacheControl = new CacheControlHeaderValue
            {
              NoCache = true,
              NoStore = true,
              MustRevalidate = true,
              MaxAge = TimeSpan.Zero
            };
          }
        };
        if (app.Environment.IsDevelopment())
        {
          spa.UseProxyToSpaDevelopmentServer("http://localhost:5173");
        }
      });

    app.Run();

  }
}

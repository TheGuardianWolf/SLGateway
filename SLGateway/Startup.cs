using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using SLGateway.Repositories;
using SLGateway.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using AspNetCore.Authentication.ApiKey;
using SLGateway.Data;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace SLGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var auth0Domain = Configuration.GetValue<string>("Auth0:Domain");
            var auth0ClientId = Configuration.GetValue<string>("Auth0:ClientId");
            var auth0ClientSecret = Configuration.GetValue<string>("Auth0:ClientSecret");

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });
            // Add authentication services
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddOpenIdConnect("Auth0", options =>
                {
                    // Set the authority to your Auth0 domain
                    options.Authority = $"https://{auth0Domain}";

                    // Configure the Auth0 Client ID and Client Secret
                    options.ClientId = auth0ClientId;
                    options.ClientSecret = auth0ClientSecret;

                    // Set response type to code
                    options.ResponseType = OpenIdConnectResponseType.Code;

                    // Configure the scope
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    // Set the callback path, so Auth0 will call back to http://localhost:3000/callback
                    // Also ensure that you have added the URL as an Allowed Callback URL in your Auth0 dashboard
                    options.CallbackPath = new PathString("/callback");

                    // Configure the Claims Issuer to be Auth0
                    options.ClaimsIssuer = "Auth0";

                    options.Events = new OpenIdConnectEvents
                    {
                        // handle the logout redirection
                        OnRedirectToIdentityProviderForSignOut = (context) =>
                        {
                            var logoutUri = $"https://{auth0Domain}/v2/logout?client_id={auth0ClientId}";

                            var postLogoutUri = context.Properties.RedirectUri;
                            if (!string.IsNullOrEmpty(postLogoutUri))
                            {
                                if (postLogoutUri.StartsWith("/"))
                                {
                                    // transform to absolute
                                    var request = context.Request;
                                    postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                                }
                                logoutUri += $"&returnTo={ Uri.EscapeDataString(HttpsUrl(postLogoutUri)) }";
                            }

                            context.Response.Redirect(logoutUri);
                            context.HandleResponse();

                            return Task.CompletedTask;
                        },
                        OnRedirectToIdentityProvider = context => {
                            context.ProtocolMessage.RedirectUri = HttpsUrl(context.ProtocolMessage.RedirectUri);

                            return Task.FromResult(0);
                        }
                    };

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name"
                    };
                })
                .AddApiKeyInAuthorizationHeader<ApiKeyService>(ApiKeyAuthenticationDefaults.BearerAuthenticationScheme, options =>
                {
                    options.KeyName = ApiKeyAuthenticationDefaults.BearerAuthenticationScheme;
                    options.IgnoreAuthenticationIfAllowAnonymous = true;
                    options.SuppressWWWAuthenticateHeader = true;
                });
            services.AddAuthorization(options =>
            {
                options.AddPolicy(ApiKeyAuthenticationPolicy.Object, policy => policy
                    .AddAuthenticationSchemes(ApiKeyAuthenticationDefaults.BearerAuthenticationScheme)
                    .RequireClaim(ApiKeyScopes.Object, bool.TrueString));
                options.AddPolicy(ApiKeyAuthenticationPolicy.Client, policy => policy
                    .AddAuthenticationSchemes(ApiKeyAuthenticationDefaults.BearerAuthenticationScheme)
                    .RequireClaim(ApiKeyScopes.Client, bool.TrueString));
            });

            services.AddHttpClient();
            services.AddSingleton<IObjectEventsRepository, ObjectEventsRepository>();
            services.AddSingleton<IObjectRegistrationRepository, ObjectRegistrationRepository>();
            services.AddSingleton<IApiKeyRepository, ApiKeyRepository>();
            services.AddScoped<IEventsService, EventsService>();
            services.AddScoped<IObjectRegistrationService, ObjectRegistrationService>();
            services.AddScoped<IApiKeyService, ApiKeyService>();
            services.AddHostedService<StorageManagerHostedService>();

            services.AddRazorPages(options =>
            {
                options.RootDirectory = "/View/Pages";
            });
            services.AddServerSideBlazor();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "SL Gateway", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (API Key)",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer", //The name of the previously defined security scheme.
                                Type = ReferenceType.SecurityScheme
                            }
                        },
                        new List<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SL Gateway v1"));

            app.UseSerilogRequestLogging();

            app.UseCookiePolicy();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }

        private string HttpsUrl(string url)
        {
            if (url.StartsWith("http://"))
            {
                return "https://" + url.Substring("http://".Length);
            }

            return url;
        }
    }
}

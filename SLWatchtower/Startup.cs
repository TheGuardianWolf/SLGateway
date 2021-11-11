using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using PreRenderComponent;
using Serilog;
using SLGateway.HostedServices;
using SLWatchtower.Data;
using SLWatchtower.Repositories;
using SLWatchtower.Services;

namespace SLWatchtower
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
#pragma warning disable CS0618 // Type or member is obsolete
            BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
#pragma warning restore CS0618 // Type or member is obsolete
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var auth0Domain = Configuration.GetValue<string>("Auth0:Domain");
            var auth0ClientId = Configuration.GetValue<string>("Auth0:ClientId");
            var auth0ClientSecret = Configuration.GetValue<string>("Auth0:ClientSecret");

            var connectionString = Configuration.GetConnectionString("Storage");
            var databaseName = Configuration.GetValue<string>("DatabaseName");

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.None;
                options.Secure = CookieSecurePolicy.SameAsRequest;
            });

            // Add authentication services
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = new PathString("/auth/login");
                    options.LogoutPath = new PathString("/auth/logout");
                    options.AccessDeniedPath = string.Empty;
                    options.Events.OnRedirectToAccessDenied = (context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return context.Response.CompleteAsync();
                    });
                })
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
                        OnRedirectToIdentityProvider = context =>
                        {
                            context.ProtocolMessage.RedirectUri = HttpsUrl(context.ProtocolMessage.RedirectUri);

                            return Task.FromResult(0);
                        }
                    };

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name"
                    };
                });

            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddScoped<IPreRenderFlag, PreRenderFlag>();
            services.AddSingleton<IMongoDataSource, MongoDataSource>(sc =>
            {
                return new MongoDataSource(connectionString, databaseName);
            });
            services.AddSingleton<IParcelRegistrationRepository, ParcelRegistrationRepository>();
            services.AddSingleton<IUserRegistrationRepository, UserRegistrationRepository>();
            services.AddSingleton<IGatewayService, GatewayService>();
            services.AddScoped<IAgentService, AgentService>();
            services.AddScoped<IParcelService, ParcelService>();
            services.AddScoped<IUserRegistrationService, UserRegistrationService>();
            services.AddHostedService<WarmupServicesOnStartupService>(sc => new WarmupServicesOnStartupService(services, sc));

            services.AddRazorPages(options =>
            {
                options.RootDirectory = "/View/Pages";
            });
            services.AddServerSideBlazor();
            services.AddControllers();
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

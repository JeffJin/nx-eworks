using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using adworks.data_services;
using adworks.data_services.Identity;
using adworks.media_web_api.Extensions;
using adworks.media_web_api.Middlewares;
using adworks.media_web_api.Services;
using adworks.message_bus;
using MessagePack;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Hosting;

namespace adworks.media_web_api
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            this._env = env;
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        private IServiceProvider _serviceProvider { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<CommonDbContext>()
                .AddDefaultTokenProviders();

            //SignalR
            services.AddSignalR()
                .AddMessagePackProtocol(options =>
                {
                    //TODO review the resolver and security settings
                    options.SerializerOptions = MessagePackSerializerOptions.Standard
                        .WithResolver(MessagePack.Resolvers.StandardResolver.Instance)
                        .WithSecurity(MessagePackSecurity.TrustedData);
                })
                .AddJsonProtocol(o =>
                {
                    o.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
            services.AddCors(o =>
            {
                o.AddPolicy("Everything", p =>
                {
                    p.SetIsOriginAllowed(IsOriginAllowed)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });
            //Not needed for JWT Token based Auth
            //services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");
            
            // services.Configure<ForwardedHeadersOptions>(options =>
            // {
            //     options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
            // });
            
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });
            
            // ===== Add Jwt Authentication ========
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear(); // => remove default claims
            services.
                AddAuthentication(options =>
                {
                    // Identity made Cookie authentication the default.
                    // However, we want JWT Bearer Auth to be the default.
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddFacebook(facebookOptions =>
                {
                    facebookOptions.AppId = Configuration["Authentication:Facebook:AppId"];
                    facebookOptions.AppSecret = Configuration["Authentication:Facebook:AppSecret"];
                    facebookOptions.SaveTokens = true;
                    facebookOptions.Events.OnTicketReceived = (context) =>
                    {
                        Console.WriteLine(context.HttpContext.User);
                        return Task.CompletedTask;
                    };
                    facebookOptions.Events.OnCreatingTicket = (context) =>
                    {
                        Console.WriteLine(context.Identity);
                        return Task.CompletedTask;
                    };
                })
                .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.RequireHttpsMetadata = false;
                    jwtBearerOptions.SaveToken = true;
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.Configuration["JwtSecretKey"])),
                        ValidateIssuer = true,
                        ValidIssuer = Configuration["JwtIssuer"],
                        ValidateAudience = true,
                        ValidAudience = Configuration["JwtAudience"],
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };
                    jwtBearerOptions.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/eventing")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = c =>
                        {
                            //clean up rabbit subscriptions
                            var rabbitListener = _serviceProvider.GetService<IRabbitListener>();
                            rabbitListener?.Deregister();
                            return Task.CompletedTask;
                        }
                    };
                });  
                    
            
            services.AddAuthorization(options => {
                options.AddPolicy("CanManageVideo", policyBuilder =>
                    policyBuilder.AddRequirements(new IsVideoOwnerRequirement()));
            });

            services.AddRazorPages();

            services.AddMvc(options =>
            {
                options.MaxModelValidationErrors = 100;
                options.EnableEndpointRouting = false;
            });
            
            // Add Autofac
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<WebApiModule>();
            containerBuilder.Populate(services);
            
            var container = containerBuilder.Build();
            _serviceProvider = new AutofacServiceProvider(container);
           
            return _serviceProvider;
        }

        private static bool IsOriginAllowed(string url)
        {
            if (url.Contains("localhost"))
            {
                return true;
            }
            return url.Contains("reefbook.ca") ||
                   url.Contains("reefbook.co") ||
                   url.Contains("coralporn.ca") ||
                   url.Contains("jeffjin.com") ||
                   url.Contains("jeffjin.net") ||
                   url.Contains("eworks.io") ||
                   url.Contains("eworkspace.ca");
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CommonDbContext dbContext, 
            UserManager<User> userManager, IMessageClient messageClient,
            IHostApplicationLifetime applicationLifetime, RoleManager<Role> roleManager)
        {
            if (env.EnvironmentName == "dev")
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error/500");
            }

            // exception handling middleware
            app.UseErrorHandlingMiddleware();
            
            //seed default user and other data
            dbContext.Database.EnsureCreated();
            DataContextFactory.SeedAuthData(userManager, roleManager).GetAwaiter().GetResult();
            DataContextFactory.SeedAppData(dbContext).GetAwaiter().GetResult();
            
            app.UseFileServer();
            app.UseRouting();
            
            app.UseCors("Everything");
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<EventHub>("/eventing", options =>
                {
                    options.Transports =
                        HttpTransportType.WebSockets |
                        HttpTransportType.LongPolling;
                });
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
            
            app.UseWebSockets();

            // rabbit mq 
            app.UseRabbitListener();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            
            applicationLifetime.ApplicationStopping.Register(OnShutdown, messageClient);
        }

        private void OnShutdown(object messageClient)  
        {
            try
            {
                ((IDisposable) messageClient).Dispose();
            }
            finally
            {
                //TODO Log errors
            }
        }
        
        private void ConfigureRoutes(IRouteBuilder routeBuilder)
        {
           
        }
    }
}
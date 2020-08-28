using AspNetCoreRateLimit;
using Exemplar_Mining.Authentication;
using Exemplar_Mining.Models;
using Exemplar_Mining.Services;
using Exemplar_Mining.Services.Interfaces;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

namespace Exemplar_Mining
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            Configuration.GetSection("Okta").Bind(OktaOAuthOptions._instance);
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {

            services.AddAuthorization();

            services.AddControllers();

            services.AddDbContext<DBContext>(opts => opts.UseNpgsql(Configuration.GetConnectionString("Postgres")));


            services.AddMvc().AddFluentValidation(opts => opts.RegisterValidatorsFromAssemblyContaining<Startup>());

            //Setting up default error response for invalid model type errors.
            services.Configure<ApiBehaviorOptions>(apiBehaviorOptions => apiBehaviorOptions.InvalidModelStateResponseFactory = actionContext =>
            {
                return new BadRequestObjectResult(new
                {
                    code = 400,
                    errors = actionContext.ModelState.Values.SelectMany(x => x.Errors)
                        .Select(x => x.ErrorMessage)
                });
            });

            //Adding config for Okta OAuth2 authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "Okta";
            }).AddCookie().AddOAuth("Okta", OktaOAuthOptions.GetAuthOptions());


            //Adding config for rate limiting functionality
            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimit"));

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddScoped<ISearcher, Searcher>();


        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }


            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseIpRateLimiting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}

﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using huypq.SwaMiddleware;

namespace SwaMiddlewareDemo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection()
                .ProtectKeysWithDpapi()
                .PersistKeysToFileSystem(new DirectoryInfo(@"c:\temp-keys"));
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwa("SwaMiddlewareDemo", new SwaOptions()
            {
                IsUseTokenAuthentication = true,
                TokenEnpoint = "user.token",
                AllowAnonymousActions = new System.Collections.Generic.List<string>(new string[]
                {
                    "user.register",
                    "test.getimage",
                    "test.getfile",
                    "test.get",
                    "test.getbytes",
                })
            });
        }
    }
}

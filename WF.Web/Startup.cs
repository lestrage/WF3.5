﻿using Autofac;
using Autofac.Configuration;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OptimaJet.Workflow.Core.Runtime;
using System;
using WF.Business.Workflow;
using WF.MsSql;
using WF.MsSql.Implementation;
using WF.Web.ServiceLocation;

namespace WF.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public readonly SampleContext _sampleContext;
        public readonly MICHRContext _michrContext;

        public IContainer Container { get; private set; }

        public WorkflowRuntime Runtime { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddAutoMapper();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the container builder.
            var builder = new ContainerBuilder();

            builder.Populate(services);

            var config = new ConfigurationBuilder();
            config.AddJsonFile("autofac.json");
            var module = new ConfigurationModule(config.Build());

            builder.RegisterInstance(Configuration);

            builder.RegisterModule(module);

            Container = builder.Build();

            // Create the IServiceProvider based on the container.
            return new AutofacServiceProvider(Container);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Document}/{action=WorkflowScheme}/{id?}");
            });

            Runtime = WorkflowInit.Create(new DataServiceProvider(Container));
        }
    }
}

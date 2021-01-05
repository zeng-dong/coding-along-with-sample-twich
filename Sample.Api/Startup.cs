using MassTransit;
using MassTransit.Definition;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Sample.Components.Consumers;
using Sample.Contracts;
using System;

namespace Sample.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
            services.AddMassTransit(cfg =>
            {
                cfg.AddBus(provider => Bus.Factory.CreateUsingRabbitMq());

                // this will publish, since on address
                //cfg.AddRequestClient<SubmitOrder>();

                // exchange bind to queue
                cfg.AddRequestClient<SubmitOrder>(
                  new Uri($"queue:{KebabCaseEndpointNameFormatter.Instance.Consumer<SumbitOrderConsumer>()}"));

                // no binding, send to exchange only, if no queue exists, message just stop there.
                //cfg.AddRequestClient<SubmitOrder>(
                //    //new Uri($"exchange:{KebabCaseEndpointNameFormatter.Instance.Consumer<SumbitOrderConsumer>()}"));
                //    new Uri($"exchange:submit-order"));
            });

            services.AddMassTransitHostedService();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sample.Api", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample.Api v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}

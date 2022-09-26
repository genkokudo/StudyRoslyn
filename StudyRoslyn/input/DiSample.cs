using Microsoft.Extensions.DependencyInjection;
using StudyRoslyn.input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace StudyRoslyn
{
    public partial class DiSample
    {
        public DiSample()
        {
            Ioc.Default.ConfigureServices(new ServiceCollection()
                .AddTransient<ITestService, TestService>()
                .AddTransient<ITestService, TestService>()
                .BuildServiceProvider());
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Services
            services.AddSingleton<ITestService, TestService>();

            // Views and ViewModels
            services.AddTransient<TestService>();
            services.AddTransient<ITestService, TestService>();
            services.AddTransient<ITestService>();
        }
    }

    /// <summary>
    /// ただのモック
    /// </summary>
    public class Ioc
    {
        public static Default DefaultObject { get; set; }
        public class Default
        {
            public static void ConfigureServices(ServiceProvider collection)
            {

            }

        }
    }
}

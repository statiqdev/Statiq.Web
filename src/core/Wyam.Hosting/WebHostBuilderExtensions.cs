using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wyam.Hosting
{
    //internal static class WebHostBuilderExtensions
    //{
    //    // Taken from Microsoft.AspNetCore.Hosting.WebHostBuilder.Build() due to breaking change in Microsoft.Extensions.DependencyInjection
    //    // See https://github.com/aspnet/EntityFrameworkCore/issues/8498
    //    // Can be removed once the AspNetCore packages are updated to 2.x
    //    public static IWebHost BuildWebHost(this IWebHostBuilder builder)
    //    {
    //        IServiceCollection serviceCollection = (IServiceCollection)InvokeMethod((WebHostBuilder)builder, "BuildCommonServices");
    //        IServiceProvider hostingServiceProvider = ServiceCollectionContainerBuilderExtensions.BuildServiceProvider(serviceCollection);
    //        InvokeMethod((WebHostBuilder)builder, "AddApplicationServices", new object[] { serviceCollection, hostingServiceProvider });
    //        WebHost webHost = new WebHost(
    //            serviceCollection,
    //            hostingServiceProvider,
    //            (WebHostOptions)GetFieldValue((WebHostBuilder)builder, "_options"),
    //            (IConfiguration)GetFieldValue((WebHostBuilder)builder, "_config"));

    //        if ((IServiceProvider)GetFieldValue(webHost, "_applicationServices") == null)
    //        {
    //            IServiceCollection applicationServiceCollection = (IServiceCollection)GetFieldValue(webHost, "_applicationServiceCollection");
    //            SetFieldValue(webHost, "_applicationServices", applicationServiceCollection.BuildServiceProvider());
    //        }

    //        SetFieldValue(webHost, "_application", BuildApplication(webHost));
    //        return webHost;
    //    }

    //    // From https://github.com/aspnet/Hosting/blob/rel/1.1.2/src/Microsoft.AspNetCore.Hosting/Internal/WebHost.cs#L135
    //    private static RequestDelegate BuildApplication(WebHost webHost)
    //    {
    //        try
    //        {
    //            InvokeMethod(webHost, "EnsureApplicationServices");
    //            InvokeMethod(webHost, "EnsureServer");
    //            InvokeMethod(webHost, "EnsureStartup");

    //            IServiceProvider applicationServices = (IServiceProvider)GetFieldValue(webHost, "_applicationServices");
    //            var builderFactory = applicationServices.GetRequiredService<IApplicationBuilderFactory>();
    //            IServer server = (IServer)GetPropertyValue(webHost, "Server");
    //            var builder = builderFactory.CreateBuilder(server.Features);
    //            builder.ApplicationServices = applicationServices;

    //            var startupFilters = applicationServices.GetService<IEnumerable<IStartupFilter>>();
    //            IStartup startup = (IStartup)GetFieldValue(webHost, "_startup");
    //            Action<IApplicationBuilder> configure = startup.Configure;
    //            foreach (var filter in startupFilters.Reverse())
    //            {
    //                configure = filter.Configure(configure);
    //            }

    //            configure(builder);

    //            return builder.Build();
    //        }
    //        catch (Exception ex) when (((WebHostOptions)GetFieldValue(webHost, "_options")).CaptureStartupErrors)
    //        {
    //            // EnsureApplicationServices may have failed due to a missing or throwing Startup class.
    //            if ((IServiceProvider)GetFieldValue(webHost, "_applicationServices") == null)
    //            {
    //                IServiceCollection applicationServiceCollection = (IServiceCollection)GetFieldValue(webHost, "_applicationServiceCollection");
    //                SetFieldValue(webHost, "_applicationServices", applicationServiceCollection.BuildServiceProvider());
    //            }

    //            InvokeMethod(webHost, "EnsureServer");

    //            return null;
    //        }
    //    }

    //    private static object InvokeMethod<TObject>(TObject obj, string methodName, object[] parameters = null) =>
    //        typeof(TObject).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(obj, parameters ?? Array.Empty<object>());

    //    private static object GetFieldValue<TObject>(TObject obj, string fieldName) =>
    //        typeof(TObject).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);

    //    private static void SetFieldValue<TObject>(TObject obj, string fieldName, object value) =>
    //        typeof(TObject).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(obj, value);

    //    private static object GetPropertyValue<TObject>(TObject obj, string propertyName) =>
    //        typeof(TObject).GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(obj);
    //}
}

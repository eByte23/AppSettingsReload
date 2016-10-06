using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace AppSettingsReload
{
    public static class ConfigureSettingExtension
    {
        public const string SETTINGS_FILE_FITER = "appsettings*.json";

        public static void ConfigureSettingScoped<TSettingModel>
            (this IServiceCollection services, IConfigurationRoot configRoot)
            where TSettingModel : class => ConfigureSettingScoped<TSettingModel>(services, configRoot, null, SETTINGS_FILE_FITER, false);

        public static void ConfigureSetting<TSettingModel>
            (this IServiceCollection services, IConfigurationRoot configRoot, string configSection)
            where TSettingModel : class => ConfigureSettingScoped<TSettingModel>(services, configRoot, null, SETTINGS_FILE_FITER, false);

        public static void ConfigureSettingScoped<TSettingModel>
            (this IServiceCollection services, IConfigurationRoot configRoot, string configSection, string settingsFileFilter)
            where TSettingModel : class => ConfigureSettingScoped<TSettingModel>(services, configRoot, configSection, settingsFileFilter, false);

        public static void ConfigureSettingScoped<TSettingModel>
            (this IServiceCollection services, IConfigurationRoot configRoot, string configSection, bool reload)
            where TSettingModel : class => ConfigureSettingScoped<TSettingModel>(services, configRoot, configSection, SETTINGS_FILE_FITER, reload);

        public static void ConfigureSettingScoped<TSettingModel>
            (this IServiceCollection services, IConfigurationRoot configRoot, bool reload)
            where TSettingModel : class => ConfigureSettingScoped<TSettingModel>(services, configRoot, null, SETTINGS_FILE_FITER, reload);

        public static void ConfigureSettingScoped<TSettingModel>
            (this IServiceCollection services, IConfigurationRoot configRoot, bool reload, string settingsFileFilter)
            where TSettingModel : class => ConfigureSettingScoped<TSettingModel>(services, configRoot, null, settingsFileFilter, reload);

        public static void ConfigureSettingScoped<TSettingModel>(
                    IServiceCollection services,
                    IConfigurationRoot configRoot,
                    string configSection,
                    string settingsFileFilter,
                    bool reload)
            where TSettingModel : class
        {
            FileSystemWatcher watcher = null;
            services.AddScoped(ctx =>
            {
                TSettingModel settingModel = Activator.CreateInstance<TSettingModel>();

                if (services == null) throw new ArgumentNullException(nameof(services));
                if (configRoot == null) throw new ArgumentNullException(nameof(configRoot));

                if (configSection == null)
                {
                    configRoot.Bind(settingModel);
                }
                else
                {
                    configRoot.GetSection(configSection).Bind(settingModel);
                }

                if (reload)
                {
                    if (watcher == null)
                    {
                        watcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), settingsFileFilter);

                        watcher.EnableRaisingEvents = true;
                        watcher.Changed += ((x, y) =>
                        {
                            configRoot.Reload();
                        });
                    }
                }

                return settingModel;
            });
        }

        public static void ConfigureSettingNew<TSettingModel>(this IServiceCollection services, IConfigurationRoot configRoot)
           where TSettingModel : class => ConfigureSettingNew<TSettingModel>(services, configRoot, null);

        public static void ConfigureSettingNew<TSettingModel>(this IServiceCollection services, IConfigurationRoot configRoot, string configSection)
            where TSettingModel : class
        {
            services.AddScoped(ctx =>
            {
                TSettingModel settingModel = Activator.CreateInstance<TSettingModel>();
                if (services == null) throw new ArgumentNullException(nameof(services));
                if (configRoot == null) throw new ArgumentNullException(nameof(configRoot));

                if (configSection == null)
                {
                    configRoot.Bind(settingModel);
                }
                else
                {
                    configRoot.GetSection(configSection).Bind(settingModel);
                }

                return settingModel;
            });
        }

        public static IApplicationBuilder UseAppSettingsReload(this IApplicationBuilder app, IConfigurationRoot configRoot)
            => UseAppSettingsReload(app, configRoot, null);

        public static IApplicationBuilder UseAppSettingsReload(this IApplicationBuilder app, IConfigurationRoot configRoot, string settingsFileFilter)
        {
            FileSystemWatcher watcher = null;

            app.Use(async (context, next) =>
            {
                if (watcher == null)
                {
                    watcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), settingsFileFilter ?? SETTINGS_FILE_FITER);

                    watcher.EnableRaisingEvents = true;
                    watcher.Changed += ((x, y) =>
                    {
                        configRoot.Reload();
                    });
                }

                await next.Invoke();
            });

            return app;
        }
    }
}
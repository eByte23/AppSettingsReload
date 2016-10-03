using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace AppSettingsReload
{
    public static class ConfigureSettingExtension
    {
        public static TSettingModel ConfigureSetting<TSettingModel>(this IServiceCollection services, IConfigurationRoot configRoot, string configSection, bool reload, string settingsFileFilter = "appsettings*.json")
            where TSettingModel : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configRoot == null) throw new ArgumentNullException(nameof(configRoot));

            var config = Activator.CreateInstance<TSettingModel>();
            configRoot.GetSection(configSection).Bind(config);

            services.AddSingleton(config);
            
            if (reload)
            {
                var watcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), settingsFileFilter);
                
                watcher.EnableRaisingEvents = true;
                watcher.Changed += ((x, y) =>
                {
                    var settingObj = services.Where(s => s.ServiceType == typeof(TSettingModel)).SingleOrDefault();
                    var setting = (TSettingModel)settingObj.ImplementationInstance;
                    configRoot.Reload();
                    configRoot.GetSection(configSection).Bind(setting);
                });
                
            }

            return config;
        }

        public static TSettingModel ConfigureSetting<TSettingModel>(this IServiceCollection services, IConfigurationRoot configRoot, string configSection, Func<TSettingModel> settingsProvider, bool reload, string settingsFileFilter = "appsettings*.json")
            where TSettingModel : class
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (configRoot == null) throw new ArgumentNullException(nameof(configRoot));
            if (settingsProvider == null) throw new ArgumentNullException(nameof(settingsProvider));

            var config = settingsProvider();
            configRoot.GetSection(configSection).Bind(config);

            services.AddSingleton(config);

            if (reload)
            {
                var watcher = new FileSystemWatcher(Directory.GetCurrentDirectory(), settingsFileFilter);

                watcher.EnableRaisingEvents = true;
                watcher.Changed += ((x, y) =>
                {
                    var settingObj = services.Where(s => s.ServiceType == typeof(TSettingModel)).SingleOrDefault();
                    var setting = (TSettingModel)settingObj.ImplementationInstance;
                    configRoot.Reload();
                    configRoot.GetSection(configSection).Bind(setting);
                });

            }

            return config;
        }
    }
}
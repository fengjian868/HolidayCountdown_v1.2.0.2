using System.Reflection;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Enums.SettingsWindow;
using ClassIsland.Core.Extensions.Registry;
using ClassIsland.Core.Services.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HolidayCountdown;

[PluginEntrance]
public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        services.AddComponent<Views.Components.HolidayCountdownComponent>();
        services.AddComponent<Views.Components.GreetingComponent>();
        services.AddComponent<Views.Components.SolarTermComponent>();
        services.AddComponent<Views.Components.LunarDateComponent>();
        services.AddComponent<Views.Components.CustomHolidayComponent>();
        services.AddComponent<Views.Components.VacationCountdownComponent>();
        services.AddComponent<Views.Components.WeatherGreetingComponent>();

        services.AddSettingsPage<Views.SettingsPages.HolidaySettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.GreetingSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.SolarTermSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.LunarSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.CustomHolidaySettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.VacationSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.WeatherSettingsPage>();
        services.AddSettingsPage<Views.SettingsPages.AboutSettingsPage>();

        // 尝试将设置页归类到子菜单下
        TryGroupSettingsPages(services);
    }

    private void TryGroupSettingsPages(IServiceCollection services)
    {
        try
        {
            var registeredInfos = SettingsWindowRegistryService.Registered
                .Where(info => info.Id.StartsWith("holidaycountdown") && info.Category == SettingsPageCategory.External)
                .ToList();

            var addGroupMethod = typeof(SettingsWindowRegistryExtensions).GetMethod(
                "AddSettingsPageGroup",
                BindingFlags.Public | BindingFlags.Static);

            if (addGroupMethod != null)
            {
                addGroupMethod.Invoke(null, new object[] { services, "holidaycountdown.settings", "\uE8F5", "节假日倒计时" });

                var groupIdProp = typeof(SettingsPageInfo).GetProperty("GroupId");
                if (groupIdProp != null)
                {
                    foreach (var info in registeredInfos)
                    {
                        groupIdProp.SetValue(info, "holidaycountdown.settings");
                    }
                }
            }
            else
            {
                // 降级：在名称前加前缀
                var nameField = typeof(SettingsPageInfo).GetField("_name", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?? typeof(SettingsPageInfo).GetField("Name", BindingFlags.NonPublic | BindingFlags.Instance);

                if (nameField != null)
                {
                    foreach (var info in registeredInfos)
                    {
                        var currentName = nameField.GetValue(info)?.ToString() ?? "";
                        if (!currentName.StartsWith("节假日倒计时"))
                            nameField.SetValue(info, "节假日倒计时·" + currentName);
                    }
                }
            }
        }
        catch
        {
            // 如果反射失败，不影响插件正常运行
        }
    }
}

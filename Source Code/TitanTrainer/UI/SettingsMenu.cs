using System;
using System.Collections.Generic;
using LemonUI.Menus;
using TitanTrainer.Core;
using TitanTrainer.Utils;
namespace TitanTrainer.UI
{
    public class SettingsMenu
    {
        private readonly ServiceContainer _services; private readonly Config _config; private readonly List<NativeMenu> _allMenus = new List<NativeMenu>();
        public NativeMenu Menu { get; } public NativeMenu[] AllMenus => _allMenus.ToArray();
        public SettingsMenu(ServiceContainer services)
        {
            _services=services??throw new ArgumentNullException(nameof(services)); _config=_services.Resolve<Config>()??throw new InvalidOperationException("Config service not found.");
            Menu=new NativeMenu("Titan Trainer","Settings") { Visible=false }; _allMenus.Add(Menu);
            var general=MenuNavigation.CreateChild(Menu,"General Settings","Trainer notifications and general behavior",_allMenus);
            var spawning=MenuNavigation.CreateChild(Menu,"Vehicle Spawn Defaults","Default options applied when spawning vehicles",_allMenus);
            MenuNavigation.AddBack(general,Menu,_allMenus); MenuNavigation.AddBack(spawning,Menu,_allMenus);
            BuildToggle(general,"Notifications",()=>_config.EnableNotifications,v=>_config.EnableNotifications=v,"Toggle trainer notifications");
            BuildToggle(spawning,"Spawn Inside Vehicle",()=>_config.SpawnDefaults.SpawnInside,v=>_config.SpawnDefaults.SpawnInside=v,"Spawn inside the vehicle when supported");
            BuildToggle(spawning,"Max Upgrade",()=>_config.SpawnDefaults.MaxUpgrade,v=>_config.SpawnDefaults.MaxUpgrade=v,"Apply maximum upgrades to spawned vehicles");
        }
        private void BuildToggle(NativeMenu menu,string label,Func<bool> get,Action<bool> set,string description)
        {
            var item=new NativeItem(label+" : "+(get()?"ON":"OFF"),description); item.Activated+=(s,e)=>{set(!get());_config.Save();_services.Resolve<NotificationService>()?.NotifyInfo(label+" "+(get()?"Enabled":"Disabled"));}; menu.Add(item);
        }
        public void Show(){MenuNavigation.Hide(_allMenus);Menu.Visible=true;} public void Hide()=>MenuNavigation.Hide(_allMenus);
    }
}

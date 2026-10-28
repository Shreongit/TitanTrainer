using System.Collections.Generic;
using System.Linq;
using LemonUI.Menus;
using TitanTrainer.Utils;
using TitanTrainer.World;

namespace TitanTrainer.UI
{
    public sealed class TimeMenu : BaseMenu
    {
        private readonly TimeManager _time; private readonly List<NativeMenu> _allMenus = new List<NativeMenu>();
        public NativeMenu[] AllMenus => _allMenus.ToArray();
        public TimeMenu(ServiceContainer services) : base("Titan Trainer", "Time")
        {
            _time = services.Resolve<TimeManager>(); _allMenus.Add(Menu);
            var presets = MenuNavigation.CreateChild(Menu, "Time Presets", "Choose a time of day", _allMenus);
            var adjust = MenuNavigation.CreateChild(Menu, "Adjust Clock", "Move the game clock forward or backward", _allMenus);
            var options = MenuNavigation.CreateChild(Menu, "Time Options", "Freeze or unfreeze the game clock", _allMenus);
            foreach (var child in new[] { presets, adjust, options }) MenuNavigation.AddBack(child, Menu, _allMenus);
            AddTime(presets, "Midnight (00:00)",0,0); AddTime(presets,"03:00",3,0); AddTime(presets,"Sunrise (06:00)",6,0); AddTime(presets,"09:00",9,0); AddTime(presets,"Noon (12:00)",12,0); AddTime(presets,"15:00",15,0); AddTime(presets,"Sunset (18:00)",18,0); AddTime(presets,"21:00",21,0);
            AddAction(adjust,"+1 Hour","Advance one hour",()=>_time.AddHour()); AddAction(adjust,"-1 Hour","Go back one hour",()=>_time.RemoveHour()); AddAction(adjust,"+30 Minutes","Advance thirty minutes",()=>_time.AddMinute()); AddAction(adjust,"-30 Minutes","Go back thirty minutes",()=>_time.RemoveMinute());
            var freeze = new NativeCheckboxItem("Freeze Time", false); freeze.CheckboxChanged += (s,e)=>_time.FreezeTime(freeze.Checked); options.Add(freeze);
        }
        private void AddTime(NativeMenu target,string name,int hour,int minute) => AddAction(target,name,"Set the game clock to this time",()=>_time.SetTime(hour,minute));
        private void AddAction(NativeMenu target,string name,string desc,System.Action action) { var item=new NativeItem(name,desc); item.Activated+=(s,e)=>action(); target.Add(item); }
        public override void Show(){MenuNavigation.Hide(_allMenus);Menu.Visible=true;} public override void Hide()=>MenuNavigation.Hide(_allMenus);
    }
}

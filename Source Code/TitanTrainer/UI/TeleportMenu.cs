using System.Collections.Generic;
using GTA.Math;
using LemonUI.Menus;
using TitanTrainer.Player;
using TitanTrainer.Utils;
namespace TitanTrainer.UI
{
    public sealed class TeleportMenu : BaseMenu
    {
        private readonly TeleportManager _teleport; private readonly NotificationService _notifications; private readonly System.Action _goBack; private readonly List<NativeMenu> _allMenus=new List<NativeMenu>();
        public NativeMenu[] AllMenus=>_allMenus.ToArray();
        public TeleportMenu(TeleportManager teleport,NotificationService notifications,System.Action goBack):base("Titan Trainer","Teleport & Locations")
        {
            _teleport=teleport;_notifications=notifications;_goBack=goBack;_allMenus.Add(Menu);
            var quick=MenuNavigation.CreateChild(Menu,"Quick Teleport","Waypoint, forward, safe lift, sky drop, and return",_allMenus);
            var city=MenuNavigation.CreateChild(Menu,"Los Santos Locations","Popular city destinations",_allMenus);
            var county=MenuNavigation.CreateChild(Menu,"Blaine County Locations","Rural, mountain, and military destinations",_allMenus);
            var saved=MenuNavigation.CreateChild(Menu,"Saved Locations","Manage and revisit your saved locations",_allMenus);
            foreach(var child in new[]{quick,city,county,saved})MenuNavigation.AddBack(child,Menu,_allMenus);
            AddAction(quick,"Save Current Location","Save your current position and heading",()=>{if(_teleport.SaveCurrentLocation())RebuildSaved(saved);});
            AddAction(quick,"Teleport to Waypoint","Teleport to the map waypoint",()=>_teleport.TeleportToWaypoint()); AddAction(quick,"Return to Previous Position","Return to the position before your last teleport",()=>_teleport.TeleportToLastPosition()); AddAction(quick,"Teleport Forward","Move 10 metres forward",()=>_teleport.TeleportForward(10f)); AddAction(quick,"Safe Lift","Move slightly upward",()=>_teleport.TeleportSafe()); AddAction(quick,"Sky Drop","Move high above the current position",()=>_teleport.TeleportSky());
            AddLocation(city,"Los Santos International Airport",new Vector3(-1034.6f,-2733.6f,20.2f)); AddLocation(city,"Legion Square",new Vector3(195.2f,-933.8f,30.7f)); AddLocation(city,"Maze Bank Tower",new Vector3(-75f,-818.9f,326.2f)); AddLocation(city,"Vespucci Beach",new Vector3(-1165f,-1520f,10.6f));
            AddLocation(county,"Sandy Shores Airfield",new Vector3(1737.5f,3288f,41.1f)); AddLocation(county,"Mount Chiliad Summit",new Vector3(501f,5604f,797.9f)); AddLocation(county,"Fort Zancudo Entrance",new Vector3(-2047f,3132f,32.8f)); AddLocation(county,"Paleto Bay",new Vector3(-95f,6457f,31.5f)); RebuildSaved(saved);
        }
        private void RebuildSaved(NativeMenu saved){saved.Clear(); if(_teleport.SavedLocations.Count==0)saved.Add(new NativeItem("No saved locations yet","Use Quick Teleport → Save Current Location first")); else foreach(var location in _teleport.SavedLocations){string name=location.Name;AddAction(saved,"Go: "+name,string.Format("X {0:0.0} | Y {1:0.0} | Z {2:0.0}",location.X,location.Y,location.Z),()=>_teleport.TeleportToSavedLocation(name));AddAction(saved,"Delete: "+name,"Remove this saved location",()=>{if(_teleport.DeleteLocation(name))RebuildSaved(saved);});} MenuNavigation.AddBack(saved, Menu, _allMenus);}
        private void AddAction(NativeMenu menu,string name,string description,System.Action action){var item=new NativeItem(name,description);item.Activated+=(s,e)=>action?.Invoke();menu.Add(item);}
        private void AddLocation(NativeMenu menu,string name,Vector3 position){AddAction(menu,name,"Teleport to this preset location",()=>{_teleport.TeleportTo(position);_notifications?.NotifyInfo("Location: "+name);});}
        public override void Show(){MenuNavigation.Hide(_allMenus);Menu.Visible=true;} public override void Hide()=>MenuNavigation.Hide(_allMenus);
    }
}

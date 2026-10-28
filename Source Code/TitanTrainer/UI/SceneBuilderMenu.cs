using System;
using System.Collections.Generic;
using System.Linq;
using LemonUI.Menus;
using TitanTrainer.World;
namespace TitanTrainer.UI
{
    /// <summary>Hierarchical menus for saving, loading, and deleting scene snapshots.</summary>
    public sealed class SceneBuilderMenu : BaseMenu
    {
        private readonly SceneBuilderService _scenes; private readonly Action _goBack; private readonly NativeMenu _saveMenu,_manageMenu,_cleanupMenu,_loadMenu,_deleteMenu; private readonly List<NativeMenu> _allMenus=new List<NativeMenu>();
        public NativeMenu[] AllMenus=>_allMenus.ToArray();
        public SceneBuilderMenu(SceneBuilderService scenes,Action goBack):base("Titan Trainer","Scene Builder")
        {
            _scenes=scenes;_goBack=goBack;_allMenus.Add(Menu);
            _saveMenu=MenuNavigation.CreateChild(Menu,"Save Scene","Capture the current trainer scene",_allMenus);
            _manageMenu=MenuNavigation.CreateChild(Menu,"Manage Saved Scenes","Load or delete a saved scene",_allMenus);
            _cleanupMenu=MenuNavigation.CreateChild(Menu,"Scene Cleanup","Remove entities loaded by Scene Builder",_allMenus);
            _loadMenu=MenuNavigation.CreateChild(_manageMenu,"Load Saved Scene","Choose a saved scene to spawn",_allMenus);
            _deleteMenu=MenuNavigation.CreateChild(_manageMenu,"Delete Saved Scene","Choose a saved scene to delete",_allMenus);
            foreach(var child in new[]{_saveMenu,_manageMenu,_cleanupMenu})MenuNavigation.AddBack(child,Menu,_allMenus);
            AddAction(_saveMenu,"Save Current Scene","Save trainer-spawned props and pedestrians plus the vehicle you are driving",()=>{_scenes.SaveCurrentScene();RefreshSceneMenus();});
            AddAction(_cleanupMenu,"Delete Entities Loaded by Scene Builder","Delete entities spawned by Scene Builder during this session",()=>_scenes.DeleteLoadedEntities());
            RefreshSceneMenus();
        }
        private void RefreshSceneMenus()
        {
            _loadMenu.Clear();_deleteMenu.Clear();var scenes=_scenes.Scenes.OrderByDescending(s=>s.CreatedUtc).ToList();
            if(scenes.Count==0){_loadMenu.Add(new NativeItem("No saved scenes","Save a scene first"));_deleteMenu.Add(new NativeItem("No saved scenes","There is nothing to delete"));}
            else foreach(var scene in scenes){string name=scene.Name;var load=new NativeItem(name,scene.Entities.Count+" saved entities. Select to load.");load.Activated+=(s,e)=>{Hide();_scenes.LoadScene(name);Show();};_loadMenu.Add(load);var delete=new NativeItem(name,"Delete this saved scene");delete.Activated+=(s,e)=>{_scenes.DeleteScene(name);RefreshSceneMenus();MenuNavigation.Hide(_allMenus);_deleteMenu.Visible=true;};_deleteMenu.Add(delete);}
            MenuNavigation.AddBack(_loadMenu,_manageMenu,_allMenus); MenuNavigation.AddBack(_deleteMenu,_manageMenu,_allMenus);
        }
        private void AddAction(NativeMenu target,string label,string description,Action action){var item=new NativeItem(label,description);item.Activated+=(s,e)=>action?.Invoke();target.Add(item);}
        public override void Show(){RefreshSceneMenus();MenuNavigation.Hide(_allMenus);Menu.Visible=true;}public override void Hide()=>MenuNavigation.Hide(_allMenus);
    }
}

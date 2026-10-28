using System;
using System.Linq;
using LemonUI.Menus;
using TitanTrainer.World;

namespace TitanTrainer.UI
{
    /// <summary>Menus for saving, loading, and deleting scene snapshots.</summary>
    public sealed class SceneBuilderMenu : BaseMenu
    {
        private readonly SceneBuilderService _scenes;
        private readonly Action _goBack;
        private readonly NativeMenu _loadMenu;
        private readonly NativeMenu _deleteMenu;
        public NativeMenu[] AllMenus { get; }

        public SceneBuilderMenu(SceneBuilderService scenes, Action goBack) : base("Titan Trainer", "Scene Builder")
        {
            _scenes = scenes;
            _goBack = goBack;
            _loadMenu = new NativeMenu("Titan Trainer", "Load Saved Scene");
            _deleteMenu = new NativeMenu("Titan Trainer", "Delete Saved Scene");
            _loadMenu.Visible = false;
            _deleteMenu.Visible = false;
            AllMenus = new[] { Menu, _loadMenu, _deleteMenu };
            BuildRoot();
            RefreshSceneMenus();
        }

        private void BuildRoot()
        {
            Menu.Clear();
            var save = new NativeItem("Save Current Scene", "Save Titan Trainer-spawned props and pedestrians, plus the vehicle you are currently driving");
            save.Activated += (s, e) => { _scenes.SaveCurrentScene(); RefreshSceneMenus(); };
            Menu.Add(save);
            var load = new NativeItem("Load Saved Scene", "Spawn the entities stored in a saved scene");
            load.Activated += (s, e) => { RefreshSceneMenus(); HideAll(); _loadMenu.Visible = true; };
            Menu.Add(load);
            var delete = new NativeItem("Delete Saved Scene", "Permanently remove a scene from the saved catalogue");
            delete.Activated += (s, e) => { RefreshSceneMenus(); HideAll(); _deleteMenu.Visible = true; };
            Menu.Add(delete);
            var cleanup = new NativeItem("Delete Entities Loaded by Scene Builder", "Delete only entities spawned by Scene Builder during this session");
            cleanup.Activated += (s, e) => _scenes.DeleteLoadedEntities();
            Menu.Add(cleanup);
            AddBack(Menu, "Back to Main Menu", _goBack);
        }

        private void RefreshSceneMenus()
        {
            _loadMenu.Clear();
            _deleteMenu.Clear();
            var scenes = _scenes.Scenes.OrderByDescending(s => s.CreatedUtc).ToList();
            if (scenes.Count == 0)
            {
                _loadMenu.Add(new NativeItem("No saved scenes", "Save a scene first"));
                _deleteMenu.Add(new NativeItem("No saved scenes", "There is nothing to delete"));
            }
            else
            {
                foreach (var scene in scenes)
                {
                    string name = scene.Name;
                    var load = new NativeItem(name, scene.Entities.Count + " saved entities. Select to load.");
                    load.Activated += (s, e) => { HideAll(); _scenes.LoadScene(name); Show(); };
                    _loadMenu.Add(load);
                    var delete = new NativeItem(name, "Delete this saved scene");
                    delete.Activated += (s, e) => { _scenes.DeleteScene(name); RefreshSceneMenus(); HideAll(); _deleteMenu.Visible = true; };
                    _deleteMenu.Add(delete);
                }
            }
            AddBack(_loadMenu, "Back to Scene Builder", Show);
            AddBack(_deleteMenu, "Back to Scene Builder", Show);
        }

        private void AddBack(NativeMenu menu, string label, Action action)
        {
            menu.Add(new NativeSeparatorItem("Navigation"));
            var back = new NativeItem(label, "Return to the previous menu");
            back.Activated += (s, e) => { HideAll(); action?.Invoke(); };
            menu.Add(back);
        }

        private void HideAll()
        {
            Menu.Visible = false;
            _loadMenu.Visible = false;
            _deleteMenu.Visible = false;
        }

        public override void Show() { RefreshSceneMenus(); HideAll(); Menu.Visible = true; }
        public override void Hide() => HideAll();
    }
}

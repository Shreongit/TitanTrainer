using System.Collections.Generic;
using System.Linq;
using LemonUI.Menus;

namespace TitanTrainer.UI
{
    /// <summary>Small shared helpers for consistent parent/child menu navigation.</summary>
    internal static class MenuNavigation
    {
        private static readonly Dictionary<NativeMenu, NativeMenu> Parents = new Dictionary<NativeMenu, NativeMenu>();
        public static NativeMenu CreateChild(NativeMenu parent, string title, string description, IList<NativeMenu> allMenus)
        {
            var child = new NativeMenu("Titan Trainer", title) { Visible = false };
            allMenus.Add(child);
            Parents[child] = parent;
            var open = new NativeItem(title, description);
            open.Activated += (s, e) => { Hide(allMenus); child.Visible = true; };
            parent.Add(open);
            return child;
        }

        public static void RegisterParent(NativeMenu child, NativeMenu parent) { Parents[child] = parent; }

        public static bool TryBack(IEnumerable<NativeMenu> menus)
        {
            var visible = menus.FirstOrDefault(m => m != null && m.Visible);
            if (visible == null || !Parents.TryGetValue(visible, out var parent)) return false;
            Hide(menus); parent.Visible = true; return true;
        }

        public static void AddBack(NativeMenu child, NativeMenu parent, IList<NativeMenu> allMenus, string label = "Back")
        {
            var back = new NativeItem(label, "Return to the previous menu");
            back.Activated += (s, e) => { Hide(allMenus); parent.Visible = true; };
            child.Add(back);
        }

        public static void Hide(IEnumerable<NativeMenu> menus)
        {
            foreach (var menu in menus.Where(m => m != null)) menu.Visible = false;
        }
    }
}

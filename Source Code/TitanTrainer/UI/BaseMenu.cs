using LemonUI.Menus;

namespace TitanTrainer.UI
{
    public abstract class BaseMenu
    {
        public NativeMenu Menu { get; protected set; }

        protected BaseMenu ParentMenu;

        protected BaseMenu(string title, string subtitle)
        {
            Menu = new NativeMenu(title, subtitle);
            Menu.Visible = false;
        }

        public virtual void Show()
        {
            Menu.Visible = true;
        }

        public virtual void Hide()
        {
            Menu.Visible = false;
        }

        public virtual void Toggle()
        {
            Menu.Visible = !Menu.Visible;
        }

        public void SetParent(BaseMenu parent)
        {
            ParentMenu = parent;

            Menu.Closing += (s, e) =>
            {
                Hide();

                ParentMenu?.Show();
            };
        }
    }
}
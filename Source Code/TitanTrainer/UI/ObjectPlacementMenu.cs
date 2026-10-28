using System;
using LemonUI.Menus;
using TitanTrainer.World;

namespace TitanTrainer.UI
{
    /// <summary>Tools for adjusting the last prop spawned by Titan Trainer.</summary>
    public sealed class ObjectPlacementMenu : BaseMenu
    {
        private readonly EntitySpawner _spawner;
        private readonly Action _goBack;

        public ObjectPlacementMenu(EntitySpawner spawner, Action goBack)
            : base("Titan Trainer", "Object Placement")
        {
            _spawner = spawner ?? throw new ArgumentNullException(nameof(spawner));
            _goBack = goBack;

            AddAction("Move Forward", "Move the last prop 0.5 metres forward", () => _spawner.MoveLastProp(0.5f, 0, 0));
            AddAction("Move Backward", "Move the last prop 0.5 metres backward", () => _spawner.MoveLastProp(-0.5f, 0, 0));
            AddAction("Move Left", "Move the last prop 0.5 metres left", () => _spawner.MoveLastProp(0, -0.5f, 0));
            AddAction("Move Right", "Move the last prop 0.5 metres right", () => _spawner.MoveLastProp(0, 0.5f, 0));
            AddAction("Move Up", "Raise the last prop 0.25 metres", () => _spawner.MoveLastProp(0, 0, 0.25f));
            AddAction("Move Down", "Lower the last prop 0.25 metres", () => _spawner.MoveLastProp(0, 0, -0.25f));
            Menu.Add(new NativeSeparatorItem("Rotation"));
            AddAction("Rotate Left 15°", "Rotate the last prop left by 15 degrees", () => _spawner.RotateLastProp(-15f));
            AddAction("Rotate Right 15°", "Rotate the last prop right by 15 degrees", () => _spawner.RotateLastProp(15f));
            AddAction("Rotate Left 90°", "Rotate the last prop left by 90 degrees", () => _spawner.RotateLastProp(-90f));
            AddAction("Rotate Right 90°", "Rotate the last prop right by 90 degrees", () => _spawner.RotateLastProp(90f));
            AddAction("Reset Rotation", "Reset the last prop rotation", _spawner.ResetLastPropRotation);
            Menu.Add(new NativeSeparatorItem("Object Actions"));
            AddAction("Duplicate Last Prop", "Create a copy of the last prop", _spawner.DuplicateLastProp);
            AddAction("Delete Last Prop", "Delete the last prop spawned by Titan Trainer", _spawner.DeleteLastProp);
            AddAction("Delete All Trainer Props", "Delete all props spawned by Titan Trainer in this session", _spawner.DeleteAllSpawnedProps);
            Menu.Add(new NativeSeparatorItem("Navigation"));
            AddAction("Back", "Return to Entity Spawner", () => { Hide(); _goBack?.Invoke(); });
        }

        private void AddAction(string label, string description, Action action)
        {
            var item = new NativeItem(label, description);
            item.Activated += (s, e) => action?.Invoke();
            Menu.Add(item);
        }
    }
}

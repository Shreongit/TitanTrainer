using System;
using System.Collections.Generic;
using LemonUI.Menus;
using TitanTrainer.World;
namespace TitanTrainer.UI
{
    /// <summary>Hierarchical tools for positioning and managing trainer-spawned props.</summary>
    public sealed class ObjectPlacementMenu : BaseMenu
    {
        private readonly EntitySpawner _spawner; private readonly Action _goBack; private readonly List<NativeMenu> _allMenus=new List<NativeMenu>();
        public NativeMenu[] AllMenus=>_allMenus.ToArray();
        public ObjectPlacementMenu(EntitySpawner spawner,Action goBack):base("Titan Trainer","Object Placement")
        {
            _spawner=spawner??throw new ArgumentNullException(nameof(spawner));_goBack=goBack;_allMenus.Add(Menu);
            var move=MenuNavigation.CreateChild(Menu,"Position Object","Move the last spawned prop along each axis",_allMenus);
            var rotate=MenuNavigation.CreateChild(Menu,"Rotate Object","Rotate the last spawned prop by common angles",_allMenus);
            var manage=MenuNavigation.CreateChild(Menu,"Object Management","Duplicate or delete trainer-spawned props",_allMenus);
            foreach(var child in new[]{move,rotate,manage})MenuNavigation.AddBack(child,Menu,_allMenus);
            AddAction(move,"Move Forward","Move 0.5 metres forward",()=>_spawner.MoveLastProp(0.5f,0,0));AddAction(move,"Move Backward","Move 0.5 metres backward",()=>_spawner.MoveLastProp(-0.5f,0,0));AddAction(move,"Move Left","Move 0.5 metres left",()=>_spawner.MoveLastProp(0,-0.5f,0));AddAction(move,"Move Right","Move 0.5 metres right",()=>_spawner.MoveLastProp(0,0.5f,0));AddAction(move,"Move Up","Raise 0.25 metres",()=>_spawner.MoveLastProp(0,0,0.25f));AddAction(move,"Move Down","Lower 0.25 metres",()=>_spawner.MoveLastProp(0,0,-0.25f));
            AddAction(rotate,"Rotate Left 15°","Rotate left by 15 degrees",()=>_spawner.RotateLastProp(-15f));AddAction(rotate,"Rotate Right 15°","Rotate right by 15 degrees",()=>_spawner.RotateLastProp(15f));AddAction(rotate,"Rotate Left 90°","Rotate left by 90 degrees",()=>_spawner.RotateLastProp(-90f));AddAction(rotate,"Rotate Right 90°","Rotate right by 90 degrees",()=>_spawner.RotateLastProp(90f));AddAction(rotate,"Reset Rotation","Reset the prop rotation",_spawner.ResetLastPropRotation);
            AddAction(manage,"Duplicate Last Prop","Create a copy of the last spawned prop",_spawner.DuplicateLastProp);AddAction(manage,"Delete Last Prop","Delete the last spawned prop",_spawner.DeleteLastProp);AddAction(manage,"Delete All Trainer Props","Delete all props spawned by Titan Trainer in this session",_spawner.DeleteAllSpawnedProps);
            var back=new NativeItem("Back to Entity Spawner","Return to Entity Spawner");back.Activated+=(s,e)=>{Hide();_goBack?.Invoke();};Menu.Add(back);
        }
        private void AddAction(NativeMenu target,string label,string description,Action action){var item=new NativeItem(label,description);item.Activated+=(s,e)=>action?.Invoke();target.Add(item);}
        public override void Show(){MenuNavigation.Hide(_allMenus);Menu.Visible=true;}public override void Hide()=>MenuNavigation.Hide(_allMenus);
    }
}

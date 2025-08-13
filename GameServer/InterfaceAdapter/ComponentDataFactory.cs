using GameServer.Domain.Object.Components;
using GameServer.Utility;

namespace GameServer.InterfaceAdapter
{
    public static class ComponentDataFactory
    {
        public static IGameComponentDTO ToData(GameComponent component)
        {
            return component switch
            {
                ActionHeld a_h => new ActionComponentData { Type = "ActionHeld", NeedAttribute = a_h.NeedAttribute },
                ActionSelf a_s => new ActionComponentData { Type = "ActionSelf", NeedAttribute = "" },
                CarryableComponent ca => new CarriableComponentData { Type = "Carriable" },
                ChoicableComponent ch => new ChoicableComponentData { Type = "Choicable", Choices = ch.Choices, SelectedChoice = ch.SelectedChoice },
                DoorComponent dr => new DoorComponentData { Type = "Door", IsOpen = dr.IsOpen, IsLock = dr.IsLock },
                InspectableComponent ins => new InspectableComponentData { Type = "Inspectable", DisplayName = ins.DisplayName, Description = ins.Description, IsActioned = ins.IsActioned }
            };
        } 
    }


}

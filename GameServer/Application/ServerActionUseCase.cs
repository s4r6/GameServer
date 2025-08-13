using GameServer.Application.DTO;
using GameServer.Application.Interface;
using GameServer.Domain;
using GameServer.Domain.Object.Components;
using GameServer.Utility;

namespace GameServer.Application
{


    public class ServerActionUseCase
    {
        ActionService service;
        IRoomRegistry roomRegistry;

        public ServerActionUseCase(IRoomRegistry roomRegistry, ActionService service)
        {
            this.service = service;
            this.roomRegistry = roomRegistry;
        }

        public ActionResultData Action(ActionInputData input)
        {
            if (string.IsNullOrEmpty(input.RoomId) || string.IsNullOrEmpty(input.ActionLabel))
            {
                Console.WriteLine("roomId,selectedChoiceのいずれかが不完全です.");
                return new ActionResultData
                {
                    result = ActionResultType.Unknown,
                    RoomId = input.RoomId
                };
            }

            var room = roomRegistry.Get(input.RoomId);
            if (room == null)
            {
                Console.WriteLine("Roomが存在しません");
                return new ActionResultData
                {
                    result = ActionResultType.Unknown,
                    RoomId = input.RoomId
                };
            }

            var stage = room!.GetStage();
            var targetEntity = stage.TryGetEntity(input.TargetId);
            var heldEntity = stage.TryGetEntity(input.HeldId);
            var availableActions = service.GetAvailableActions(targetEntity, heldEntity);
            var actionId = availableActions.Find(a => a.label == input.ActionLabel)?.id ?? throw new Exception("ActionLabelが存在しません");

            //var result = stage.ApplyAction(input.ObjectId, input.ActionLabel, input.Type, room.Id);
            if (!service.CanAction(targetEntity, heldEntity))
            {
                return new ActionResultData
                {
                    result = ActionResultType.Unknown,
                    RoomId = input.RoomId
                };
            }

            if(input.Type == TargetType.Self)
            {
                var result = service.ApplyAction(input.ActionLabel, targetEntity, stage);
                if(result == ActionResultType.Success)
                {
                    return new ActionResultData
                    {
                        result = ActionResultType.Success,
                        target = TargetType.Self,
                        actionId = actionId,
                        RoomId = input.RoomId,
                        RoomName = room.Name,
                        PlayerName = room.GetPlayer(input.PlayerId).Name,
                        entity = targetEntity,
                        currentRiskAmount = stage.CurrentRiskAmount,
                        currentActionPointAmount = stage.CurrentActionPoint,
                        histories = stage.histories
                    };
                }
                else if(result == ActionResultType.ShortageActionPoint)
                {
                    return new ActionResultData
                    {
                        result = ActionResultType.ShortageActionPoint,
                        RoomId = input.RoomId,
                    };
                }
            }
            else if(input.Type == TargetType.HeldItem)
            {
                var result = service.ApplyAction(input.ActionLabel, heldEntity, stage);
                if (result == ActionResultType.Success)
                {
                    return new ActionResultData
                    {
                        result = ActionResultType.Success,
                        target = TargetType.HeldItem,
                        actionId = actionId,
                        RoomId = input.RoomId,
                        RoomName = room.Name,
                        PlayerName = room.GetPlayer(input.PlayerId).Name,
                        entity = heldEntity,
                        currentRiskAmount = stage.CurrentRiskAmount,
                        currentActionPointAmount = stage.CurrentActionPoint,
                        histories = stage.histories
                    };
                }
                else if (result == ActionResultType.ShortageActionPoint)
                {
                    return new ActionResultData
                    {
                        result = ActionResultType.ShortageActionPoint,
                        RoomId = input.RoomId,
                    };
                }
            }

            return new ActionResultData
            {
                result = ActionResultType.Unknown,
                RoomId = input.RoomId
            };    
        }
    }
}

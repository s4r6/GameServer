using GameServer.Application.DTO;
using GameServer.Application.Interface;
using GameServer.Domain.Object;
using GameServer.Infrastracture.Repository;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Collections.Generic;
using System.Linq;

namespace GameServer.Application
{
    public class ServerInspectUseCase
    {
        IRoomRegistry roomRegistory;
        public ServerInspectUseCase(IRoomRegistry roomRepository)
        {
            this.roomRegistory = roomRepository;
        }

        public InspectResultData Inspect(string roomId, string objectId, string selectedChoice, string playerId)
        {
            if(roomId == null || objectId == null || selectedChoice == null)
            {
                Console.WriteLine("roomId,objectId,selectedChoiceのいずれかが不完全です.");
                return new InspectResultData
                {
                    Success = false,
                    RoomId = roomId == null ? string.Empty : roomId
                };
            }

            var room = roomRegistory.Get(roomId);
            if (room == null) 
            {
                Console.WriteLine("Roomが存在しません");
                return new InspectResultData
                {
                    Success = false,
                    RoomId = roomId
                };
            }
            var playerName = room.GetPlayer(playerId)?.Name;

            var stage = room!.GetStage();
            var inspectedObj = stage.Inspect(objectId, selectedChoice);
            return new InspectResultData
            {
                Success = true,
                RoomId = room.Id,
                RoomName = room.Name,
                PlayerName = playerName,
                Entity = inspectedObj
            };
        }

    }
}

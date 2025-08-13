using GameServer.Application;
using GameServer.Application.DTO;
using GameServer.Application.Interface;
using GameServer.Infrastracture;
using GameServer.Utility;

namespace GameServer.InterfaceAdapter
{
    public class VotePresenter
    {
        VoteUseCase usecase;

        IRoomRegistry registry;

        public VotePresenter(VoteUseCase usecase, IRoomRegistry registry)
        {
            this.usecase = usecase; 
            this.registry = registry;
        }

        public async Task HandleStartVoteRequest(string rawJson, string connectionId)
        {
            var packet = PacketSerializer.Deserialize<StartVoteRequest>(rawJson);

            var input = new StartVoteInputData
            {
                RoomId = packet.Payload.RoomId,
                PlayerId = packet.Payload.PlayerId,
            };
            var result = usecase.HandleStartVote(input);
            if (result == null || result.IsSuccess == false)
                return;

          
            if(result.Result == VoteResult.Passed)
            {
                var endnotifier = new PacketModel<VoteEndNotifier>()
                {
                    PacketId = PacketId.VoteEndNotifier,
                    Payload = new VoteEndNotifier
                    {
                        Result = result.Result,
                    }
                };

                var end_json = PacketSerializer.Serialize(endnotifier);
                await registry.BroadcastAsync(input.RoomId, end_json);
            }
            else
            {
                var notifierPacket = new PacketModel<VoteNotifier>
                {
                    PacketId = PacketId.VoteNotifier,
                    Payload = new VoteNotifier
                    {
                        Yes = result.Yes,
                        No = result.No,
                        Total = result.Total
                    }
                };

                var json = PacketSerializer.Serialize(notifierPacket);
                await registry.BroadcastAsync(input.RoomId, json);
            }
                
            
        }

        public async Task HandleCastVotePacket(string rawJson, string connectionId)
        {
            var packet = PacketSerializer.Deserialize<VoteChoiceRequest>(rawJson);

            var input = new CastVoteInput
            {
                RoomId = packet.Payload.RoomId,
                PlayerId = packet.Payload.PlayerId,
                Choice = packet.Payload.Choice
            };
            var result = usecase.HandleCastVote(input);
            if (result == null || result.IsSuccess == false)
                return;

            if(result.Result == VoteResult.Failed || result.Result == VoteResult.Passed)
            {
                var endPacket = new PacketModel<VoteEndNotifier>
                {
                    PacketId = PacketId.VoteEndNotifier,
                    Payload = new VoteEndNotifier
                    {
                        Result = result.Result,
                    }
                };

                var end_json = PacketSerializer.Serialize(endPacket);
                await registry.BroadcastAsync(input.RoomId, end_json);
            }
            else
            {
                var notifierPacket = new PacketModel<VoteNotifier>
                {
                    PacketId = PacketId.VoteNotifier,
                    Payload = new VoteNotifier
                    {
                        Yes = result.Yes,
                        No = result.No,
                        Total = result.Total
                    }
                };

                var json = PacketSerializer.Serialize(notifierPacket);
                await registry.BroadcastAsync(input.RoomId, json);
            }
                
        }
    }
}

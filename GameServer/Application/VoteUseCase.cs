using GameServer.Application.DTO;
using GameServer.Application.Interface;

namespace GameServer.Application
{
    public enum VoteChoice { Undefined, Yes, No }
    public enum VoteResult { Pending, Passed, Failed }
    /// <summary>
    /// 投票の途中経過をビュー層へ通知するための VO
    /// </summary>
    public readonly struct VoteProgress
    {
        public readonly int Yes;
        public readonly int No;
        public readonly int Total;
        public float RatioYes => Total == 0 ? 0f : (float)Yes / Total;
        public float RatioNo => Total == 0 ? 0f : (float)No / Total;
        public VoteProgress(int yes, int no, int total)
        {
            Yes = yes; No = no; Total = total;
        }
    }

    public class VoteUseCase
    {
        private readonly IRoomRegistry _rooms;

        public VoteUseCase(IRoomRegistry rooms)
        {
            _rooms = rooms;
        }

        // ───────────────────────────────────────────────────────
        //  投票開始
        // ───────────────────────────────────────────────────────
        public VoteOutputData  HandleStartVote(StartVoteInputData input)
        {
            var room = _rooms.Get(input.RoomId);
            if (room == null) {
                return new VoteOutputData
                {
                    IsSuccess = false,
                }; 
            }

            VoteProgress progress = default;
            bool ok = room.TryStartVote(
                input.PlayerId,
                duration: TimeSpan.FromSeconds(30),
                requiredRatio: 1f,
                out progress);

            if (!ok)
            {
                return new VoteOutputData
                {
                    IsSuccess = false,
                };
            }

            if(room.GetPlayers().Count() <= 1)
            {
                return new VoteOutputData
                {
                    IsSuccess = true,
                    Yes = progress.Yes,
                    No = progress.No,
                    Total = progress.Total,
                    Result = VoteResult.Passed,
                };
            }

            return new VoteOutputData
            {
                IsSuccess = true,
                Yes = progress.Yes,
                No = progress.No,
                Total = progress.Total,
                Result = VoteResult.Pending
            };
        }

        // ───────────────────────────────────────────────────────
        //  投票
        // ───────────────────────────────────────────────────────
        public VoteOutputData HandleCastVote(CastVoteInput input)
        {
            var room = _rooms.Get(input.RoomId);
            if (room == null)
            {
                return new VoteOutputData
                {
                    IsSuccess = false,
                };
            }
            if (!room.HasVote)
            {
                return new VoteOutputData
                {
                    IsSuccess = false,
                };
            }
            if (!room.TryCastVote(input.PlayerId, input.Choice, out var progress))
            {
                return new VoteOutputData
                {
                    IsSuccess = false,
                };
            }

            var result = room.EvaluateVote();
            if (result == null || result == VoteResult.Pending)
            {
                result = VoteResult.Pending;
            }

            if (result == VoteResult.Passed || result == VoteResult.Failed)
            {
                room.ClearVote();
                Console.WriteLine("VoteClear");
            }
                

            return new VoteOutputData
            {
                IsSuccess = true,
                Yes = progress.Yes,
                No = progress.No,
                Total = progress.Total,
                Result = result.Value
            };
        }
    }

    // -------------------------------------------------------------------------
    //  Packet DTOs (for brevity – replace with your real packet definitions)
    // -------------------------------------------------------------------------

    public struct VoteStartAckPacket
    {
        public bool Success;
        public string Message;
    }

}

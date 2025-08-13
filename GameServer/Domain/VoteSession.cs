using GameServer.Application;
using System;
using System.Collections.Concurrent;

namespace GameServer.Domain
{
    public class VoteSession
    {
        private readonly string _initiatorId;
        private readonly HashSet<string> _allPlayerIds;
        private readonly ConcurrentDictionary<string, VoteChoice> _votes = new();
        private readonly DateTime _deadlineUtc;
        private readonly float _requiredRatio;

        private volatile VoteResult _cachedResult = VoteResult.Pending;

        public bool IsActive => _cachedResult == VoteResult.Pending && DateTime.UtcNow <= _deadlineUtc;

        public VoteSession(
            string initiatorId,
            IEnumerable<string> allPlayerIds,
            TimeSpan duration,
            float requiredRatio,
            out VoteProgress progress)
        {
            _initiatorId = initiatorId;
            _allPlayerIds = new HashSet<string>(allPlayerIds);
            _deadlineUtc = DateTime.UtcNow.Add(duration);
            _requiredRatio = requiredRatio;

            progress = default;
            TryCastVote(initiatorId, VoteChoice.Yes, out progress);
        }

        /// <summary>
        /// 票を追加する。二重投票や期限切れの場合 false を返す。
        /// </summary>
        public bool TryCastVote(string playerId, VoteChoice choice, out VoteProgress progress)
        {
            progress = default;
            if (!IsActive) return false;
            if (!_allPlayerIds.Contains(playerId)) return false;
            if (choice == VoteChoice.Undefined) return false;

            // AddOrUpdate for idempotency & thread safety
            if (!_votes.TryAdd(playerId, choice)) return false; // already voted

            progress = GetProgressUnsafe();
            return true;
        }

        /// <summary>
        /// 現在の途中経過を取得。
        /// </summary>
        public VoteProgress GetProgress()
        {
            return GetProgressUnsafe();
        }

        /// <summary>
        /// 投票の成否を判定し、確定した場合に結果を返す。
        /// 複数スレッドから同時に呼ばれても安全。
        /// </summary>
        public VoteResult Evaluate()
        {
            if (_cachedResult != VoteResult.Pending) return _cachedResult; // already resolved

            var progress = GetProgressUnsafe();
            int requiredYes = (int)Math.Ceiling(progress.Total * _requiredRatio);

            if (progress.Yes >= requiredYes)
            {
                _cachedResult = VoteResult.Passed;
            }
            else if (progress.No > progress.Total - requiredYes)
            {
                _cachedResult = VoteResult.Failed;
            }
            else if (DateTime.UtcNow > _deadlineUtc)
            {
                _cachedResult = VoteResult.Failed; // timeout treat as fail
            }



            return _cachedResult;
        }

        // ──────────────────────── private ────────────────────────
        private VoteProgress GetProgressUnsafe()
        {
            int yes = 0, no = 0;
            foreach (var choice in _votes.Values)
            {
                if (choice == VoteChoice.Yes) yes++;
                else if (choice == VoteChoice.No) no++;
            }
            int total = _allPlayerIds.Count;
            return new VoteProgress(yes, no, total);
        }
    }
}

namespace GameServer.Application
{
    public interface IGameClock
    {
        /// <summary>ゲーム開始からの経過時間を返す。</summary>
        TimeSpan ElapsedSinceGameStart();
    }
}

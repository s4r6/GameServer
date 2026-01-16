using System.Net.WebSockets;
using System.Text;
using GameServer.Application.Interface;
using GameServer.Application;
using GameServer.Infrastracture;
using GameServer.InterfaceAdapter.Interface;
using GameServer.InterfaceAdapter;
using GameServer.Utility;
using GameServer.Infrastracture.Repository;
using GameServer.Domain;
using GameServer.Infrastracture.Factory;
using GameServer.InterfaceAdapter.Registory;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5001");

// DI登録
builder.Services.Configure<GameLogOptions>(builder.Configuration.GetSection("GameLog"));
builder.Services.AddSingleton<ILogRepository>(sp =>
{
    var options = sp.GetRequiredService<IOptions<GameLogOptions>>().Value;
    return new RoomLogRepository(options.Path);
});
builder.Services.AddSingleton<IEntityFactory, EntityFactory>();
builder.Services.AddSingleton<IObjectRepository, ObjectRepository>();
builder.Services.AddSingleton<StageFactory>();
builder.Services.AddSingleton<IRoomFactory, RoomFactory>();
builder.Services.AddSingleton<IConnectionRegistry, ConnectionRegistry>();
builder.Services.AddSingleton<IRoomRegistry, RoomRegistry>();
builder.Services.AddSingleton<LoggerUseCase>();
builder.Services.AddSingleton<IPlayerFactory, PlayerFactory>();
builder.Services.AddSingleton<RoomUseCase>();
builder.Services.AddSingleton<RoomPresenter>();
builder.Services.AddSingleton<SyncPositionPresenter>();
builder.Services.AddSingleton<ServerInspectUseCase>();
builder.Services.AddSingleton<InspectPresenter>();
builder.Services.AddSingleton<ActionService>();
builder.Services.AddSingleton<ServerActionUseCase>();
builder.Services.AddSingleton<ActionPresenter>();
builder.Services.AddSingleton<VotePresenter>();
builder.Services.AddSingleton<VoteUseCase>();

var app = builder.Build();

app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();
        var connection = new ASPSocketConnection(webSocket);

        var registry = context.RequestServices.GetRequiredService<IConnectionRegistry>();
        registry.Register(connectionId, connection);

        var roomPresenter = context.RequestServices.GetRequiredService<RoomPresenter>();
        var inspectPresenter = context.RequestServices.GetRequiredService<InspectPresenter>();
        var actionPresenter = context.RequestServices.GetRequiredService<ActionPresenter>();
        var syncPosPresenter = context.RequestServices.GetRequiredService<SyncPositionPresenter>();
        var votePresenter = context.RequestServices.GetRequiredService<VotePresenter>();

        try
        {
            await ReceiveLoopAsync(webSocket, connectionId, roomPresenter, inspectPresenter, actionPresenter, syncPosPresenter, votePresenter);
        }
        finally
        {
            await roomPresenter.HandlePlayerDisconnected(connectionId);
            registry.Unregister(connectionId);
        }
    }
    else
    {
        await next();
    }
});

app.Run();

async Task ReceiveLoopAsync(WebSocket socket, string connectionId, RoomPresenter roomPresenter, InspectPresenter inspectPresenter, ActionPresenter actionPresenter, SyncPositionPresenter syncPosPresenter, VotePresenter votePresenter)
{
    var buffer = new byte[4096];
    while (socket.State == WebSocketState.Open)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        try
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Bye", CancellationToken.None);
                return;
            }

            var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var packetId = PacketSerializer.ExtractPacketId(json);
            if (packetId != PacketId.PositionUpdate || packetId != PacketId.Ping)
                Console.WriteLine(json);

            switch (packetId)
            {
                case PacketId.CreateRoomRequest:
                    await roomPresenter.HandleCreateRequest(json, connectionId);
                    break;

                case PacketId.JoinRequest:
                    await roomPresenter.HandleJoinRequest(json, connectionId);
                    break;

                case PacketId.SearchRoomRequest:
                    await roomPresenter.HandleSearchRequest(connectionId);
                    break;

                case PacketId.InspectObjectRequest:
                    await inspectPresenter.HandleInspectRequest(json, connectionId);
                    break;

                case PacketId.ActionRequest:
                    await actionPresenter.HandleActionRequest(json, connectionId);
                    break;

                case PacketId.PositionUpdate:
                    await syncPosPresenter.HandlePositionUpdate(json, connectionId);
                    break;

                case PacketId.StartVoteRequest:
                    await votePresenter.HandleStartVoteRequest(json, connectionId);
                    break;

                case PacketId.VoteChoiceRequest:
                    await votePresenter.HandleCastVotePacket(json, connectionId);
                    break;

                case PacketId.Ping:
                    break;

                default:
                    Console.WriteLine($"[Warning] Unknown PacketId: {packetId}");
                    break;
            }
        }
        catch(OperationCanceledException)
        {
            //タイムアウト
            Console.WriteLine($"[Timeout] connection {connectionId}");
            break;
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"[WebSocket] connection {connectionId} closed unexpectedly: {ex.Message}");
            break; // 強制切断
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] connection {connectionId}: {ex}");
            break; // 想定外エラー
        }
    }
}




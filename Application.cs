namespace Application;

public static class Application
{
    public static async Task Main()
    {
        var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };
        #if DEBUG
            await Task.WhenAll(GameClient.RunAsync(Bot.NAME, cts.Token),
                                GameClient.RunAsync(Bot.NAME, cts.Token));
            #else
        
        await GameClient.RunAsync(Bot.NAME, cts.Token);
        #endif
    }
}

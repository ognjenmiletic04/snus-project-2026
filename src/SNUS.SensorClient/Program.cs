using SNUS.SensorClient.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "SNUS - Sensor Client Simulator";
        Console.WriteLine("=== SNUS SENSOR SIMULATOR LOKALNE MREŽE ===");

        var httpClient = new SensorHttpClient();
        var simulator = new SensorSimulator(httpClient);

        await simulator.StartSimulationAsync();

        while (true)
        {
            var keyInfo = Console.ReadKey(true);

            if (keyInfo.Key == ConsoleKey.B)
            {
                simulator.TriggerTemporaryBlock();
            }
            if (keyInfo.Key == ConsoleKey.X)
            {
                var attacker = new MaliciousSensorSimulator(httpClient);
                await attacker.AttackSystemAsync();
            }
        }
    }
}
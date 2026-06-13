using SNUS.SensorClient.Services;

class Program
{
    static async Task Main(string[] args)
    {
        Console.Title = "SNUS - Sensor Client Simulator";
        Console.WriteLine("=== SNUS SENSOR SIMULATOR LOKALNE MREŽE ===");
        Console.WriteLine("Pritisni [X] za Kriptografski napad (Loš potpis)");
        Console.WriteLine("Pritisni [Y] za DDoS napad (Preko 10 poruka u sekundi)");

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
                await attacker.AttackWithBadSignatureAsync(); 
            }
            if (keyInfo.Key == ConsoleKey.Y)
            {
                var attacker = new MaliciousSensorSimulator(httpClient);
                await attacker.AttackWithDDoSAsync(); 
            }

        }
    }
}
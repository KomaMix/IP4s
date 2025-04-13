using System.Net.NetworkInformation;
using System.Net;
using IP4s;

public class Program
{
    private static readonly int MaxDegreeOfParallelism = 50;

    public static async Task Main(string[] args)
    {
        // Задаём математический паттерн заранее (например, "==")
        string mathPattern = "==";
        IpPatternService patternService = new IpPatternService();

        // Получаем кандидатов по паттерну (одиночный вариант)
        List<string> candidateIps = patternService.GetIpAddressesByPattern(mathPattern);
        Console.WriteLine($"Candidates count: {candidateIps.Count}");

        List<IPAddress> responsiveIps = new List<IPAddress>();
        SemaphoreSlim semaphore = new SemaphoreSlim(MaxDegreeOfParallelism);
        List<Task> tasks = new List<Task>();

        // Асинхронно пингуем адреса-кандидаты
        foreach (string ipStr in candidateIps)
        {
            IPAddress ip = IPAddress.Parse(ipStr);
            await semaphore.WaitAsync();
            Task task = Task.Run(async () =>
            {
                try
                {
                    using (Ping ping = new Ping())
                    {
                        PingReply reply = await ping.SendPingAsync(ip, 1000);
                        if (reply.Status == IPStatus.Success)
                        {
                            lock (responsiveIps)
                            {
                                responsiveIps.Add(ip);
                            }
                        }
                    }
                }
                catch { }
                finally
                {
                    semaphore.Release();
                }
            });
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);

        Console.WriteLine("\n-- Single Address Results --");
        // Вывод одиночных адресов в требуемом формате
        foreach (IPAddress ip in responsiveIps)
        {
            string feature = GetHalfSumFeature(ip);
            Console.WriteLine($"{ip}, доступен, признак: {feature}");
        }

        // Реализация варианта с парами адресов
        // Группируем адреса по сумме всех октетов (это выбранный критерий совпадения)
        Dictionary<int, List<IPAddress>> groups = new Dictionary<int, List<IPAddress>>();
        foreach (IPAddress ip in responsiveIps)
        {
            int totalSum = SumOfOctets(ip);
            if (!groups.ContainsKey(totalSum))
                groups[totalSum] = new List<IPAddress>();
            groups[totalSum].Add(ip);
        }

        Console.WriteLine("\n-- Paired Address Results --");
        // Для групп, содержащих более одного адреса, формируем все возможные пары
        foreach (var kvp in groups)
        {
            List<IPAddress> group = kvp.Value;
            if (group.Count > 1)
            {
                for (int i = 0; i < group.Count; i++)
                {
                    for (int j = i + 1; j < group.Count; j++)
                    {
                        Console.WriteLine($"{group[i]}, доступен; {group[j]}, доступен; признак: сумма = {kvp.Key}");
                    }
                }
            }
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static string GetHalfSumFeature(IPAddress ip)
    {
        byte[] octets = ip.GetAddressBytes();
        if (octets.Length != 4)
            return "";
        int left = octets[0] + octets[1];
        int right = octets[2] + octets[3];
        return $"{left} vs {right}";
    }

    static int SumOfOctets(IPAddress ip)
    {
        int sum = 0;
        foreach (byte b in ip.GetAddressBytes())
            sum += b;
        return sum;
    }
}
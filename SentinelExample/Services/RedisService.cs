using StackExchange.Redis;

namespace SentinelExample.Services
{
    public class RedisService
    {

        static ConfigurationOptions sentinelOptions => new()
        {
            EndPoints = //sentinel sunucularımın endpointlerini tanımlıyorum.
            {
                { "localhost", 6383 },
                { "localhost", 6384 },
                { "localhost", 6385 },
            },
            CommandMap = CommandMap.Sentinel,
            AbortOnConnectFail = false
        };

        static ConfigurationOptions masterOptions => new()
        {
            AbortOnConnectFail = false
        };

        public static async Task<IDatabase> redisMasterDatabase()
        {
            ConnectionMultiplexer sentinelConnection = await ConnectionMultiplexer.SentinelConnectAsync(sentinelOptions);

            //hangi redis sunucusunun master olduğunu bilmediğimiz için sentinel sunucuları arasında dönerek master redis databaseini elde etmeye çalışıyoruz.
            System.Net.EndPoint masterEndpoint = null;

            foreach (System.Net.EndPoint endpoint in sentinelConnection.GetEndPoints())
            {
                IServer server = sentinelConnection.GetServer(endpoint);

                if (!server.IsConnected)
                    continue;
                //sentinelin izlediği master sunucunun adresini burada elde ediyoruz. Elde ederken kullandığımız servis adı ise sentineli ayağa kaldırırken(sentinel.conf) mastera verdiğmiz isim olan mymaster ile talep ediyoruz.
                masterEndpoint = await server.SentinelGetMasterAddressByNameAsync("mymaster");
                break;

            }

            //masterEndpoint değişkenindeki master sunucumun docker containerında olduğu için internal ipsini elde edebiliyorum. Eğer fiziksel sunucularda olsaydık aşağıdaki dönüşüme gerek kalmayacak doğrudan sunucunun ipsini elde etmiş olacaktık.
            //şimdi ise docker containerından dönen internal ip yi fiziksel bilgisayarımdaki docker container adresine dönüştürmem gerekiyor.
            var localMasterIP = masterEndpoint.ToString() switch
            {
                "172.18.0.2:6379" => "localhost:6379",
                "172.18.0.3:6379" => "localhost:6380",
                "172.18.0.4:6379" => "localhost:6381",
                "172.18.0.5:6379" => "localhost:6382",
            };

            //artık elimde master sunucunun fiziksel bilgisayarımdan erişilebilir bir connection bilgisi var ve gerisi tekil bir redis sunucusuna bağlanmak kadar kolay.
            ConnectionMultiplexer masterConnection = await ConnectionMultiplexer.ConnectAsync(localMasterIP);
            IDatabase database = masterConnection.GetDatabase();
            return database;
        }
    }
}

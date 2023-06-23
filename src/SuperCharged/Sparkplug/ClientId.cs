using System.Net;

internal class ClientId
{
	private static Random _rnd = new Random();
	public static string Build(string name)
	{
		string hostName = Dns.GetHostName();
		string randomBit = _rnd.Next().ToString("X");
		string built = $"{name}_{hostName}_{randomBit}";
		return built;
	}
}

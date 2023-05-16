using System.Globalization;
using System.Reflection;
using CsvHelper;
namespace Otympics.Data;

public class OtympicsData
{
    public static readonly string LocalDataDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "..", "..", "..", "..", "data");
    public static readonly string ServerDataDir = @"h:\root\home\grumbull-001\www\otympics-data\";

    public string RootDir { get; }
    public string UsersFile => Path.Combine(RootDir, "users.csv");
    public string EventsFile => Path.Combine(RootDir, "events.csv");

    private readonly object _lock = new object();

    public OtympicsData()
    {
        if (Directory.Exists(ServerDataDir))
        {
            RootDir = ServerDataDir;
        }
        else
        {
            RootDir = LocalDataDir;
        }

        if (!File.Exists(UsersFile))
        {
            Console.WriteLine($"Creating user file at: {UsersFile}");
            File.Create(UsersFile);
        }
        if (!File.Exists(EventsFile))
        {
            Console.WriteLine($"Creating event file at: {UsersFile}");
            File.Create(EventsFile);
        }

        using (var reader = new StreamReader(UsersFile))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<User>().ToList();
            foreach (var record in records)
            {
                Console.WriteLine($"{record.Name}");
                UserLookup[record.Name] = record;
            }
            Console.WriteLine(UserLookup.Count);
        }
    }

    public Dictionary<string, User> UserLookup { get; set; } = new();
    public Dictionary<string, Game> GameLookup { get; set; } = new();

    public List<User> GetUsers()
    {
        lock (_lock)
        {
            var users = new List<User>(UserLookup.Values);
            users.Sort();
            return users;
        }
    }

    public List<Game> GetGames()
    {
        lock (_lock)
        {
            return new List<Game>(GameLookup.Values);
        }
    }

    public void SimpleEventAdd(HashSet<string> gold, HashSet<string> silver, HashSet<string> bronze, HashSet<string> dnp)
    {
        lock (_lock)
        {
            foreach (var user in gold)
            {
                Console.WriteLine($"Adding gold for {user}");
                var userData = UserLookup[user];
                userData.Gold++;
            }
            foreach (var user in silver)
            {
                Console.WriteLine($"Adding silver for {user}");
                var userData = UserLookup[user];
                userData.Silver++;
            }
            foreach (var user in bronze)
            {
                Console.WriteLine($"Adding bronze for {user}");
                var userData = UserLookup[user];
                userData.Bronze++;
            }
            foreach (var user in dnp)
            {
                Console.WriteLine($"Adding dnp for {user}");
                var userData = UserLookup[user];
                userData.DNP++;
            }
        }
    }

    public void SerializeUserData()
    {
        lock (_lock)
        {
            using (var writer = new StreamWriter(UsersFile))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(UserLookup.Values.ToList());
            }
        }
    }
}


public class User : IComparable<User>
{
    public string Name { get; set; }
    public string Flag { get; set; }
    public int Gold { get; set; }
    public int Silver { get; set; }
    public int Bronze { get; set; }
    public int DNP { get; set; }

    [CsvHelper.Configuration.Attributes.Ignore]
    public int TotalMedals => Gold + Silver + Bronze;

    [CsvHelper.Configuration.Attributes.Ignore]
    public int TotalEvents => TotalMedals + DNP;

    public int CompareTo(User? other)
    {
        var medalCompare = other.TotalMedals.CompareTo(TotalMedals);
        var goldCompare = other.Gold.CompareTo(Gold);
        var silverCompare = other.Silver.CompareTo(Silver);
        var bronzeCompare = other.Bronze.CompareTo(Bronze);
        var eventCompare = TotalEvents.CompareTo(other.TotalEvents);

        if (medalCompare != 0) { return medalCompare; }
        if (goldCompare != 0) { return goldCompare; }
        if (silverCompare != 0) { return goldCompare; }
        if (bronzeCompare != 0) { return bronzeCompare; }
        if (eventCompare != 0) { return eventCompare; }
        return Name.CompareTo(other.Name);
    }
}

public class Game
{
    public string Name { get; set; }
    public HashSet<string> Users { get; set; }
    public string Gold { get; set; }
    public string Silver { get; set; }
    public string Bronze { get; set; }
}
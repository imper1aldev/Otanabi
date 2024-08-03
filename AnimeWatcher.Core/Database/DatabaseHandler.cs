#pragma warning disable IDE0044

using AnimeWatcher.Core.Helpers;
using AnimeWatcher.Core.Models;
using SQLite;

namespace AnimeWatcher.Core.Database;
public sealed class DatabaseHandler
{

    private static DatabaseHandler instance = null;
    private readonly ClassReflectionHelper _classReflectionHelper = new();
    public SQLiteAsyncConnection _db;

    private const string _defaultApplicationDataFolder = "AnimeWatcher/ApplicationData";
    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private string currDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

    private string dbName
    {
        get
        {
            return new ModeDetector().IsDebug ? $"animeDB-DEBUG" : "animeDB";
        }
    }


    public static DatabaseHandler GetInstance()
    {
        if (instance == null)
        {
            instance = new DatabaseHandler();
        }
        return instance;
    }
    public SQLiteAsyncConnection GetConnection()
    {
        return _db;
    }


    public DatabaseHandler()
    {
        var _applicationDataFolder = Path.Combine(_localApplicationData, _defaultApplicationDataFolder);

        if (!Directory.Exists(_applicationDataFolder))
        {
            Directory.CreateDirectory(_applicationDataFolder);
        }


        var dbPath = Path.Combine(_applicationDataFolder, dbName);

        // new ModeDetector().IsDebug?$"{dbName}-DEBUG":
        if (File.Exists(Path.Combine(currDir, dbName)))
        {
            if (!File.Exists(dbPath))
            {
                File.Move(Path.Combine(currDir, dbName), dbPath);
            }
        }



        _db = new SQLiteAsyncConnection(dbPath);






    }
    private void checkPreviusAndMove()
    {

    }

    public async Task InitDb()
    {

        await _db.CreateTablesAsync(CreateFlags.None,
            typeof(Anime),
            typeof(FavoriteList),
            typeof(AnimexFavorite),
            typeof(Chapter),
            typeof(History),
            typeof(Provider));

        await initData();

    }

    public async Task initData()
    {

        var provDLL = _classReflectionHelper.GetProviders();
        var provDB = await _db.Table<Provider>().ToArrayAsync();
        var onDBsize = provDB.Length;
        var onLocalSize = provDLL.Length;

        var providersToAdd = provDLL.Where(c1 => !provDB.Any(c2 => c1.Id == c2.Id));
        if (providersToAdd.Count() > 0)
        {
            await _db.InsertAllAsync(providersToAdd);
        }

        var runinit = await _db.Table<FavoriteList>().Where(f => f.Id == 1).ToListAsync();
        if (runinit.Count == 0)
        {
            var indatlistFav = new FavoriteList { Id = 1, Name = "Favorite List", Placement = 0 };
            await _db.InsertAsync(indatlistFav);
        }

    }
}

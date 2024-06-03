using System.Reflection;
using System.Xml.Linq;
using AnimeWatcher.Core.Helpers;
using AnimeWatcher.Core.Models;
using SQLite;

namespace AnimeWatcher.Core.Database;
public sealed class DatabaseHandler
{

    private static DatabaseHandler instance = null;

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

    private readonly ClassReflectionHelper _classReflectionHelper = new();
    public SQLiteAsyncConnection _db;
    public DatabaseHandler()
    {
        var currDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        var dbPath = Path.Combine(currDir, "animeDB");
        _db = new SQLiteAsyncConnection(dbPath);

    }
    public async Task InitDb()
    {
        //await _db.CreateTableAsync<Anime>();
        //await _db.CreateTableAsync<FavoriteList>();
        //await _db.CreateTableAsync<AnimexFavorite>();
        //await _db.CreateTableAsync<Chapter>();
        //await _db.CreateTableAsync<History>();
        //await _db.CreateTableAsync<Provider>();
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
            var indatlistFav = new FavoriteList{ Id = 1, Name = "Favorite List",Placement=0 }; 
            await _db.InsertAsync(indatlistFav);
        }

    }
}

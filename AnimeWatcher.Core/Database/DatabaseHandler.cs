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

        if (onDBsize != onLocalSize)
        {
            foreach (var provider in provDLL)
            {
                var pDB = await _db.Table<Provider>().Where(p => p.Id == provider.Id).FirstOrDefaultAsync();
                if (pDB == null)
                {
                    await _db.InsertAsync(provider);
                }
            }
        }


        var runinit = await _db.Table<FavoriteList>().Where(f => f.Id == 1).ToListAsync();

        if (runinit.Count == 0)
        {
            var indatlistFav = new List<FavoriteList>()
            {
                new() { Id = 1, Name = "Favorite List" },
                new() { Id = 2, Name = "Favorite List 2" },
                new() { Id = 3, Name = "Favorite List 3" },
                new() { Id = 4, Name = "Favorite List 4" },
                new() { Id = 5, Name = "Favorite List 5" }
            };

            var countFavOrder = 0;
            foreach (var fav in indatlistFav)
            {
                countFavOrder++;
                await _db.ExecuteAsync("insert into FavoriteList (Id,Name,Placement) values (?,?,?);", fav.Id, fav.Name, countFavOrder);
            }
            // await _db.InsertAllAsync(_classReflectionHelper.GetProviders());
        }
        
    }
}

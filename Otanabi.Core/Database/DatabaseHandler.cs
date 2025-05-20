#pragma warning disable IDE0044

using Otanabi.Core.Helpers;
using Otanabi.Core.Models;
using SQLite;

namespace Otanabi.Core.Database;

public sealed class DatabaseHandler
{
    private static DatabaseHandler instance = null;
    private readonly ClassReflectionHelper _classReflectionHelper = new();
    public SQLiteAsyncConnection _db;

    private const string _defaultApplicationDataFolder = "Otanabi/ApplicationData";
    private readonly string _localApplicationData = Environment.GetFolderPath(
        Environment.SpecialFolder.LocalApplicationData
    );

    private string currDir = Path.GetDirectoryName(
        System.Reflection.Assembly.GetExecutingAssembly().Location
    );

    private string DBName => new ModeDetector().IsDebug ? $"animeDB-DEBUG" : "animeDB";

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
        var _applicationDataFolder = Path.Combine(
            _localApplicationData,
            _defaultApplicationDataFolder
        );

        if (!Directory.Exists(_applicationDataFolder))
        {
            Directory.CreateDirectory(_applicationDataFolder);
        }

        var dbPath = Path.Combine(_applicationDataFolder, DBName);

        // new ModeDetector().IsDebug?$"{DBName}-DEBUG":
        if (File.Exists(Path.Combine(currDir, DBName)))
        {
            if (!File.Exists(dbPath))
            {
                File.Move(Path.Combine(currDir, DBName), dbPath);
            }
        }

        _db = new SQLiteAsyncConnection(dbPath);
    }

    private void checkPreviusAndMove()
    {
    }

    public async Task InitDb()
    {
        await _db.CreateTablesAsync(
            CreateFlags.None,
            typeof(Anime),
            typeof(FavoriteList),
            typeof(AnimexFavorite),
            typeof(Chapter),
            typeof(History),
            typeof(Provider)
        );

        await ApplyMigrations();
        await ProvidersSetUp();
        await InitData();
    }

    private async Task ApplyMigrations()
    {
        var tables = new Type[]
        {
            typeof(Anime),
            typeof(FavoriteList),
            typeof(AnimexFavorite),
            typeof(Chapter),
            typeof(History),
            typeof(Provider)
        };

        foreach (var table in tables)
        {
            try
            {
                await AutoMigrateTable(table);
            }
            catch (Exception)
            {
                continue;
            }
        }
    }

    public async Task InitData()
    {
        var favData = await _db.Table<FavoriteList>().Where(f => f.Id == 1).ToListAsync();
        if (favData.Count == 0)
        {
            var indatlistFav = new FavoriteList
            {
                Id = 1,
                Name = "Favorite List",
                Placement = 0
            };
            await _db.InsertAsync(indatlistFav);
        }
    }

    private async Task ProvidersSetUp()
    {
        //Add new providers
        var provDLL = _classReflectionHelper.GetProviders();
        var provDB = await _db.Table<Provider>().ToArrayAsync();

        var providersToAdd = provDLL.Where(c1 => !provDB.Any(c2 => c1.Id == c2.Id));
        if (providersToAdd.Count() > 0)
        {
            await _db.InsertAllAsync(providersToAdd);
        }

        //Fix current providers
        foreach (var prop in provDB)
        {
            var prDll = provDLL.FirstOrDefault(c1 => c1.Id == prop.Id);
            if (prDll != null && (prDll.Name != prop.Name || prDll.Persistent != prop.Persistent || prDll.Url != prop.Url))
            {
                prop.Name = prDll.Name;
                prop.Persistent = prDll.Persistent;
                prop.Url = prDll.Url;
                await _db.UpdateAsync(prop);
            }
        }
    }

    private async Task AutoMigrateTable(Type modelType)
    {
        var tableName = modelType.Name;
        var tableInfo = await _db.GetTableInfoAsync(tableName);
        var existingColumns = tableInfo.Select(c => c.Name).ToHashSet();

        var props = modelType.GetProperties().Where(p => p.CanWrite && !p.IsDefined(typeof(IgnoreAttribute), true));

        foreach (var prop in props)
        {
            if (!existingColumns.Contains(prop.Name))
            {
                var sqliteType = GetSQLiteType(prop.PropertyType);
                var defaultValue = GetDefaultValue(prop.PropertyType);

                var alterSql = $"ALTER TABLE {tableName} ADD COLUMN {prop.Name} {sqliteType} DEFAULT {defaultValue}";
                await _db.ExecuteAsync(alterSql);
            }
        }
    }

    private static string GetSQLiteType(Type type) => type switch
    {
        _ when type == typeof(int) || type == typeof(long) || type == typeof(bool) => "INTEGER",
        _ when type == typeof(double) || type == typeof(float) => "REAL",
        _ when type == typeof(string) || type == typeof(DateTime) => "TEXT",
        _ => "TEXT"
    };

    private static string GetDefaultValue(Type type) => type switch
    {
        _ when type == typeof(int) || type == typeof(long) || type == typeof(bool) => "0",
        _ when type == typeof(double) || type == typeof(float) => "0.0",
        _ when type == typeof(DateTime) => $"'{DateTime.MinValue:yyyy-MM-dd HH:mm:ss}'",
        _ => "''"
    };
}

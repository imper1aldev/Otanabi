using AnimeWatcher.Core.Models;
using SQLite;

namespace AnimeWatcher.Core.Database;
public class DatabaseHandler
{
    public SQLiteAsyncConnection _db;
    public DatabaseHandler()
    {
        _db = new SQLiteAsyncConnection("animeDb");
    }
    public async void InitDb()
    {
        await _db.CreateTableAsync<Anime>();
        await _db.CreateTableAsync<FavoriteList>();
        await _db.CreateTableAsync<Chapter>();
        await _db.CreateTableAsync<Favorite>();
        await _db.CreateTableAsync<History>();
        await _db.CreateTableAsync<Provider>();
    }
}

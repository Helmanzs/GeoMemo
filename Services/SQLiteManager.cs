using GeoMemo.Models;
using System.Text.Json;

namespace GeoMemo.Services
{
    public class SQLiteManager
    {
        private bool _initialized;
        private readonly SQLiteAsyncConnection _db;

        public SQLiteManager(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
        }

        public async Task InitAsync()
        {
            if (_initialized)
                return;

            await _db.CreateTableAsync<GeoVzpominka>();
            await _db.CreateTableAsync<Obec>();

            await SeedObceAsync();
            _initialized = true;
        }

        // CREATE
        public Task<int> AddGeoVzpominkaAsync(GeoVzpominka item)
        {
            return _db.InsertAsync(item);
        }

        // READ
        public Task<List<GeoVzpominka>> GetAllVzpominka()
        {
            return _db.Table<GeoVzpominka>()
                      .OrderByDescending(x => x.CreatedAt)
                      .ToListAsync();
        }

        // SAFE READ - 
        public async Task<List<GeoVzpominka>> GetAllVzpominkaSafe()
        {
            try
            {
                if (!_initialized)
                    await InitAsync();

                return await _db.Table<GeoVzpominka>()
                                .OrderByDescending(x => x.CreatedAt)
                                .ToListAsync();
            }
            catch (SQLite.SQLiteException ex) when (ex.Message.Contains("no such table"))
            {
                System.Diagnostics.Debug.WriteLine("Tabulka GeoVzpominky ještě není připravena, zkusím znovu později.");
                await Task.Delay(2000);
                return await GetAllVzpominkaSafe();
            }
        }

        // READ Obce
        public Task<List<Obec>> GetObceAsync()
        {
            return _db.Table<Obec>()
                      .OrderBy(o => o.Nazev)
                      .ToListAsync();
        }

        // DELETE
        public Task<int> DeleteVzpominka(GeoVzpominka item)
        {
            return _db.DeleteAsync(item);
        }

        // naplnění tabulky obcí ze souboru JSON
        private async Task SeedObceAsync()
        {
            var count = await _db.Table<Obec>().CountAsync();
            if (count > 0)
                return;

            using var stream = await FileSystem.OpenAppPackageFileAsync("obce.json");
            using var reader = new StreamReader(stream);

            var json = await reader.ReadToEndAsync();
            var obce = JsonSerializer.Deserialize<List<Obec>>(json);

            await _db.InsertAllAsync(obce);
        }
    }
}

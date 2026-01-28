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

            await _db.CreateTableAsync<Obec>();
            await _db.CreateTableAsync<GeoVzpominka>();

            await SeedObceAsync();
            _initialized = true;
        }

        // CREATE
        public Task<int> AddGeoVzpominkaAsync(GeoVzpominka item)
        {
            return _db.InsertAsync(item);
        }

        // READ
        public Task<List<GeoVzpominka>> GetAllAsync()
        {
            return _db.Table<GeoVzpominka>()
                      .OrderByDescending(x => x.CreatedAt)
                      .ToListAsync();
        }

        public Task<List<Obec>> GetObceAsync()
        {
            return _db.Table<Obec>()
                      .OrderBy(o => o.Nazev)
                      .ToListAsync();
        }

        public Task<GeoVzpominka> GetByIdAsync(int id)
        {
            return _db.Table<GeoVzpominka>()
                      .FirstOrDefaultAsync(x => x.Id == id);
        }

        // UPDATE
        public Task<int> UpdateAsync(GeoVzpominka item)
        {
            return _db.UpdateAsync(item);
        }

        // DELETE
        public Task<int> DeleteAsync(GeoVzpominka item)
        {
            return _db.DeleteAsync(item);
        }

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

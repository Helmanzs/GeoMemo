using GeoMemo.Models;
using GeoMemo.Services;
using System.Collections.ObjectModel;

namespace GeoMemo.ViewModels;

public class MainPageViewModel
{
    private readonly SQLiteManager _db;

    public ObservableCollection<Obec> Obce { get; } = new();

    public MainPageViewModel(SQLiteManager database)
    {
        _db = database;
    }

    public async Task LoadAsync()
    {
        await _db.InitAsync();
        var obce = await _db.GetObceAsync();

        Obce.Clear();
        foreach (var obec in obce)
            Obce.Add(obec);
    }

}

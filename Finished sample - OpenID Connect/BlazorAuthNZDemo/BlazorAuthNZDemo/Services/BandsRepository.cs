using BlazorAuthNZDemo.Client.Models;

namespace BlazorAuthNZDemo.Services;

public class BandsRepository
{
    public IEnumerable<Band> GetBands()
    {
        return [ new (1, "Nirvana (from local API)"),
            new (2, "Queens of the Stone Age (from local API)"),
            new (3, "Fred Again. (from local API)"),
            new (4, "Underworld (from local API)" )];
    }

}

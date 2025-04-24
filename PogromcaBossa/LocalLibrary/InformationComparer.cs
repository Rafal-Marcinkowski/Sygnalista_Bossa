using PogromcaBossa.MVVM.Models;
using System.Diagnostics.CodeAnalysis;

namespace PogromcaBossa.LocalLibrary;
public class InformationComparer : IEqualityComparer<Information>
{
    public bool Equals(Information? x, Information? y)
    {
        if (x == null || y == null)
            return false;

        return x.Time == y.Time &&
               x.Content == y.Content &&
               x.Title == y.Title;
    }

    public int GetHashCode([DisallowNull] Information obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

        int hash = 17;
        hash = hash * 23 + (obj.Time?.GetHashCode() ?? 0);
        hash = hash * 23 + (obj.Content?.GetHashCode() ?? 0);
        hash = hash * 23 + (obj.Title?.GetHashCode() ?? 0);

        return hash;
    }
}

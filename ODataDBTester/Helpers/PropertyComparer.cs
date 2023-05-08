namespace ODataDBTester.Helpers;

public class PropertyComparer<T> : IEqualityComparer<T>
{
    public bool Equals(T x, T y)
    {
        var type = typeof(T);

        foreach (var prop in type.GetProperties())
        {
            var propX = prop.GetValue(x);
            var propY = prop.GetValue(y);

            if (propX == null && propY == null)
            {
                continue;
            }
            else if (propX == null || propY == null)
            {
                return false;
            }
            else if (!propX.Equals(propY))
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(T obj)
    {
        return obj.GetHashCode();
    }
}

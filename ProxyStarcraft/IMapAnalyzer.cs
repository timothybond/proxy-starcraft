namespace ProxyStarcraft
{
    public interface IMapAnalyzer<T>
    {
        T GetInitial(Map map);

        T Get(T prior, Map map);
    }
}

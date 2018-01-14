namespace ProxyStarcraft
{
    public interface IGameClient
    {
        void AddMapAnalyzer<T>(IMapAnalyzer<T> analyzer);
    }
}

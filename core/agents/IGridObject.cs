public interface IGridObject
{
    public int Id { get; set; }

    void ToAbstract();
    void ToDetailed();
    void Save();
    void Load();
}
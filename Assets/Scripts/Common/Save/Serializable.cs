public interface ISerializable
{
    void OnLoad(Compound compound);
    Compound OnSave();
}

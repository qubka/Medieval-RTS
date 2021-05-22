public interface IManager<T> where T: ISelectable
{
    //void SetLayout(ILayout layout);
    //ILayout GetLayout();
    void AddSelected(T t, bool toggle = false);
    void DeselectAll();
    void DeselectAllExcept(T t);
    void SwapSelected(int newPos, int oldPos);
}

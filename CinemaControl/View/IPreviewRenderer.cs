namespace CinemaControl;

public interface IPreviewRenderer
{
    void Render(string filePath);
    void Hide();
}
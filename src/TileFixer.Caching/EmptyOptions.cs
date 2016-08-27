using System.Drawing;

namespace Tile.Caching
{
  public class EmptyOptions
  {
    public BorderOptions Border { get; set; }

    public static EmptyOptions Defaults()
    {
      return new EmptyOptions
      {
        Border = new BorderOptions
        {
          Color = Color.Black
        }
      };
    }
  }

  public class BorderOptions
  {
    public Color? Color { get; set; }
    public bool Hide { get; set; }
  }
}
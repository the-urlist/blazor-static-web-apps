namespace BlazorApp.Client.Utils;

public class Debouncer
{
  private Timer? timer;
  private readonly int delay;

  public Debouncer(int delay)
  {
    this.delay = delay;
  }

  public void Debounce(Action action)
  {
    timer?.Dispose();
    timer = new Timer(_ => action(), null, delay, Timeout.Infinite);
  }
}
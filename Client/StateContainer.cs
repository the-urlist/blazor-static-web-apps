using BlazorApp.Shared;

public class StateContainer
{
  private LinkBundle savedLinkBundle = new LinkBundle();

  public LinkBundle LinkBundle
  {
    get => savedLinkBundle ??= new LinkBundle();
    set
    {
      savedLinkBundle = value;
      NotifyStateChanged();
    }
  }

  private bool appIsBusy { get; set; }

  public bool AppIsBusy
  {
    get => appIsBusy;
    set
    {
      appIsBusy = value;
      NotifyStateChanged();
    }
  }

  private User? user;

  public User? User
  {
    get => user;
    set
    {
      user = value;
      NotifyStateChanged();
    }
  }

  public void DeleteLinkFromBundle(Link link)
  {
    LinkBundle.Links.Remove(link);
    NotifyStateChanged();
  }

  public void AddLinkToBundle(Link link)
  {
    LinkBundle.Links.Add(link);
    NotifyStateChanged();
  }

  public void UpdateLinkInBundle(Link link, Link updatedLink)
  {
    link.Title = updatedLink.Title;
    link.Description = updatedLink.Description;
    link.Image = updatedLink.Image;

    NotifyStateChanged();
  }

  public void ReorderLinks(int moveFromIndex, int moveToIndex)
  {
    var links = LinkBundle.Links;
    var itemToMove = links[moveFromIndex];
    links.RemoveAt(moveFromIndex);

    if (moveToIndex < links.Count)
    {
      links.Insert(moveToIndex, itemToMove);
    }
    else
    {
      links.Add(itemToMove);
    }

    NotifyStateChanged();
  }

  public async Task<HttpResponseMessage> GetLinkBundle(string vanityUrl)
  {
    var client = new HttpClient();
    var response = await client.GetAsync($"links/{vanityUrl}");
    return response;
  }

  public event Action? OnChange;

  private void NotifyStateChanged() => OnChange?.Invoke();
}
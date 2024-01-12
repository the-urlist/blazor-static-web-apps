using System.Text.Json;
using BlazorApp.Shared;
using Microsoft.JSInterop;


public class StateContainer
{
  private readonly IJSRuntime jsRuntime;

  private LinkBundle? linkBundle;

  public StateContainer(IJSRuntime jsRuntime)
  {
    this.jsRuntime = jsRuntime;
  }

  public LinkBundle LinkBundle
  {
    get => linkBundle ??= new LinkBundle();
    set
    {
      linkBundle = value;

      SaveLinkBundleToLocalStorage();
      NotifyStateChanged();
    }
  }

  public async Task LoadLinkBundleFromLocalStorage()
  {
    var json = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "linkBundle");
    if (!string.IsNullOrEmpty(json))
    {
      LinkBundle = JsonSerializer.Deserialize<LinkBundle>(json) ?? new LinkBundle();
    }
  }

  public void SaveLinkBundleToLocalStorage()
  {
    Console.WriteLine("Saving link bundle to local storage");
    var json = JsonSerializer.Serialize(LinkBundle);
    jsRuntime.InvokeVoidAsync("localStorage.setItem", "linkBundle", json);
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

    SaveLinkBundleToLocalStorage();
    NotifyStateChanged();
  }

  public void AddLinkToBundle(Link link)
  {
    LinkBundle.Links.Add(link);

    SaveLinkBundleToLocalStorage();
    NotifyStateChanged();
  }

  public void UpdateLinkInBundle(Link link, Link? updatedLink)
  {
    if (updatedLink == null)
    {
      return;
    }

    link.Title = updatedLink.Title;
    link.Description = updatedLink.Description;
    link.Image = updatedLink.Image;

    SaveLinkBundleToLocalStorage();
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

    SaveLinkBundleToLocalStorage();
    NotifyStateChanged();
  }

  public event Action? OnChange;

  private void NotifyStateChanged()
  {
    OnChange?.Invoke();
  }
}
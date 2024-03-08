using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BlazorApp.Shared;
using Microsoft.JSInterop;

public class StateContainer
{
	private readonly IJSRuntime jsRuntime;

	public StateContainer(IJSRuntime jsRuntime)
	{
		this.jsRuntime = jsRuntime;
	}

	private LinkBundle? linkBundle;
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

	private readonly List<Link> _linksPendingUpdate = [];

	public void AddLinkToUpdatePool(Link link)
	{
		_linksPendingUpdate.Add(link);
		NotifyStateChanged();
	}

	public void RemoveLinkFromUpdatePool(Link link)
	{
		_linksPendingUpdate.Remove(link);
		NotifyStateChanged();
	}

	public bool IsLinkInUpdatePool(Link link)
	{
		return _linksPendingUpdate.Contains(link);
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Shared assembly hasn't opted into trimming")]
	public async Task LoadLinkBundleFromLocalStorage()
	{
		var json = await jsRuntime.InvokeAsync<string>("localStorage.getItem", "linkBundle");
		if (!string.IsNullOrEmpty(json))
		{
			LinkBundle = JsonSerializer.Deserialize<LinkBundle>(json) ?? new LinkBundle();
		}
	}

	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "Shared assembly hasn't opted into trimming")]
	public void SaveLinkBundleToLocalStorage()
	{
		var json = JsonSerializer.Serialize(LinkBundle);
		jsRuntime.InvokeVoidAsync("localStorage.setItem", "linkBundle", json).AsTask();
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

		LinkBundleHasChanged();
	}

	public void AddLinkToBundle(Link link)
	{
		LinkBundle.Links.Add(link);
		LinkBundleHasChanged();
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

		LinkBundleHasChanged();
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

		LinkBundleHasChanged();
	}

	private void LinkBundleHasChanged()
	{
		SaveLinkBundleToLocalStorage();
		NotifyStateChanged();
	}

	public event Action? OnChange;

	private void NotifyStateChanged()
	{
		OnChange?.Invoke();
	}
}
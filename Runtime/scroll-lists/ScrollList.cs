using BeatThat.ItemManagers;
using UnityEngine;
using UnityEngine.UI;

namespace BeatThat.ScrollLists
{

    public class ScrollList<ItemType> : ScrollList<ItemType, ItemType> where ItemType : Component {}

	/// <summary>
	/// Simple scroll list that manages adding and cleaning up items. 
	/// Assumes all items are the same type/can be instantiated from a single prefab.
	/// </summary>
	public class ScrollList<ItemType, ListItemType> : ItemList<ItemType, ListItemType>
		where ListItemType : Component
		where ItemType : class
	{
		public ScrollRect m_scrollRect;

		override protected Transform FindContentParent()
		{
			var sc = (m_scrollRect != null)? m_scrollRect: GetComponentInChildren<ScrollRect> (true);
			return sc != null ? sc.content : null;
		}
	}

	public static class ScrollRectExt
	{
		public static RectTransform GetViewport(this ScrollRect scrollRect)
		{
			// Analysis disable ConvertConditionalTernaryToNullCoalescing
			return scrollRect.viewport != null? scrollRect.viewport: scrollRect.transform as RectTransform;
			// Analysis restore ConvertConditionalTernaryToNullCoalescing
		}
	}
}



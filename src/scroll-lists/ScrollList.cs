using System.Collections.Generic;
using BeatThat;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using BeatThat.UI;

namespace BeatThat
{
	public interface IHasItemAddedGoEvent 
	{
		UnityEvent<GameObject> itemAddedGO { get; }
	}

	public class ScrollList<ItemType> : ScrollList<ItemType, ItemType> where ItemType : Component {}

	/// <summary>
	/// Simple scroll list that manages adding and cleaning up items. 
	/// Assumes all items are the same type/can be instantiated from a single prefab.
	/// </summary>
	public class ScrollList<ItemType, ListItemType> : Controller, ItemManager<ItemType>, IHasItemAddedGoEvent
		where ListItemType : Component
		where ItemType : class
	{
		public ListItemType m_itemPrefab;
		public ScrollRect m_scrollRect;

		public bool m_setItemNames;
		public string m_itemNameFormat = "item-{0}";

		// Analysis disable ConvertToAutoProperty
		public bool setItemNames { get { return m_setItemNames; } set { m_setItemNames = value; } }
		public string itemNameFormat { get { return m_itemNameFormat; } set { m_itemNameFormat = value; } }
		// Analysis restore ConvertToAutoProperty

		public bool m_clearItemsOnUnbind = true;

		public UnityEvent<GameObject> itemAddedGO { get { return m_itemAddedGO?? (m_itemAddedGO = new GameObjectEvent()); } set { m_itemAddedGO = value; } }
		[SerializeField]private UnityEvent<GameObject> m_itemAddedGO;

		sealed override protected void UnbindController()
		{
			if(m_clearItemsOnUnbind) {
				ClearItems();
			}
			base.UnbindController();
			UnbindScrollList();
		}

		/// <summary>
		/// Override to add behaviour on unbind
		/// </summary>
		virtual protected void UnbindScrollList()
		{
		}

		public ListItemType lastItem 
		{
			get {
				return m_listItems.Count == 0 ? null : m_listItems [m_listItems.Count - 1].value;
			}
		}

		public bool GetLastItem<T>(out T item) where T : class
		{
			item = this.lastItem as T;
			return (item != null);
		}

		public ListItemType GetListItem(int ix) 
		{
			return m_listItems[ix].value;
		}

		public void GetAllRootItems(ICollection<ListItemType> results)
		{
			foreach(var li in m_listItems) {
				var v = li.value;
				if(v != null) {
					results.Add(v);
				}
			}
		}

		public ItemType Get(int ix) 
		{
			// fix later with less wasteful search
			using(var items = ListPool<ItemType>.Get()) {
				GetAll(items);
				return items[ix];
			}
		}

		public void GetAll(ICollection<ItemType> result)
		{
			foreach(var i in m_listItems) {
				var v = i.value;
				if(v == null) {
					continue;
				}
				GetItems(i.value, result);
			}
		}

		#region IHasRootItems implementation
		public int rootItemCount { get { return m_listItems.Count; } }

		virtual public int GetRootItems<T>(ICollection<T> results) where T : class
		{
			using(var items = ListPool<ListItemType>.Get()) {
				GetAllRootItems(items);
				return ExtractComponents<ListItemType, T>(items, results);
			}
		}
		#endregion


		#region IHasItems implementation
		public int count
		{ 
			get { 
				// TODO: optimize
				using(var items = ListPool<ItemType>.Get()) {
					GetItems(items);
					return items.Count;
				}
			}
		}

		virtual public int GetItems<T>(ICollection<T> results) where T : class
		{
			using(var items = ListPool<ItemType>.Get()) {
				GetAll(items);
				return ExtractComponents<ItemType, T>(items, results);
			}
		}
		#endregion

		private static int ExtractComponents<ItemT, ExtractT>(ICollection<ItemT> itemsIn, ICollection<ExtractT> itemsOut) 
			where ItemT : class 
			where ExtractT : class
		{
			int n = 0;
			foreach(var ti in itemsIn) {
				if(ti == null) {
					continue;
				}

				var item = ti as ExtractT;
				if(item != null) {
					itemsOut.Add(item);
					n++;
					continue;
				}

				var c = ti as Component;
				if(c != null && (item = c.GetComponent<ExtractT>()) != null) {
					itemsOut.Add(item);
					n++;
					continue;
				}

				Debug.LogWarning("Unable to convert item to requested type " + typeof(ExtractT).Name);
			}
			return n;
		}

		public int AddItems(ICollection<ItemType> items)
		{
			return GetItems(AddItem(), items);
		}

		private static int GetItems(ListItemType listItem, ICollection<ItemType> items)
		{
			var asItem = listItem as ItemType;
			if(asItem != null) {
				items.Add(asItem);
				return 1;
			}

			var hasItems = listItem as IHasItems;
			if(hasItems == null) {
				throw new InvalidCastException("the ListItemType of a ScrollList must either be the same as the ItemType or it must implement IHasItems ItemType="
					+ typeof(ItemType).Name + " ListItemType=" + typeof(ListItemType).Name);
			}

			return hasItems.GetItems(items);
		}

		/// <summary>
		/// Creates a new item and inserts it into the list at the given index
		/// </summary>
		/// <returns>The newly instantiated item</returns>
		/// <param name="index">Index.</param>
		/// <param name="prefab">Pass to override the default item prefab</param>
		public ListItemType InsertItemAt(int index, ListItemType prefab = null)
		{
			var item = Instantiate(prefab?? m_itemPrefab);

			if(index < m_listItems.Count) {
				// the actual transform sibling order of the items may be different from the list, e.g. if there are bookend items not in the list
				var siblingIndex = m_listItems[index].value.transform.GetSiblingIndex(); 
				item.transform.SetParent(this.scrollRect.content, false);
				item.transform.SetSiblingIndex(siblingIndex);
				m_listItems.Insert(index, new SafeRef<ListItemType>(item));
			}
			else {
				item.transform.SetParent(this.scrollRect.content, false);
				m_listItems.Add(new SafeRef<ListItemType>(item));
			}

			if(this.setItemNames) {
				item.name = string.Format(this.itemNameFormat, m_listItems.Count);
			}

			if(m_itemAddedGO != null) {
				m_itemAddedGO.Invoke(item.gameObject);
			}

			return item;
		}

		public void EnsureItemCount(int count, ListItemType prefab = null)
		{
			if(this.count == count) {
				return;
			}

			while(this.count < count) {
				AddItem(prefab);
			}

			while(this.count > count) {
				RemoveItemAt(this.count - 1);
			}
		}

		public ListItemType RemoveItemAt(int index)
		{
			var item = m_listItems[index];
			m_listItems.RemoveAt(index);
			return item.value;
		}

		/// <summary>
		/// Creates a new item and adds it to the list as the last item
		/// </summary>
		/// <returns>The newly instantiated item</returns>
		/// <param name="prefab">Pass to override the default item prefab</param>
		public ListItemType AddItem(ListItemType prefab = null)
		{
			return InsertItemAt(m_listItems.Count, prefab);
		}

		public void ClearItems()
		{
			for(int i = m_listItems.Count - 1; i >= 0; i--) {
				var item = m_listItems[i].value;
				m_listItems.RemoveAt(i);
				if(item == null) {
					continue;
				}
				Destroy(item.gameObject);
			}
		}

		public ScrollRect scrollRect { get { return m_scrollRect?? (m_scrollRect = GetComponent<ScrollRect>()); } }

		private List<SafeRef<ListItemType>> m_listItems = new List<SafeRef<ListItemType>>();
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

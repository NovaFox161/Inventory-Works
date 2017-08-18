using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

[AddComponentMenu("Inventory/ Item Outline")]
/// <summary>
/// Item outline. A specialized Utility class to handle the outlining of Items.
/// </summary>
public class ItemOutline : MonoBehaviour {
	static ItemOutline instance;
	static Transform thisTransStatic;

	public Shader outlineShader;

	readonly List<Guid> outlinedItems = new List<Guid>();
	readonly Hashtable items = new Hashtable();
	readonly Hashtable originalShaders = new Hashtable();


	#region Instnance handling
	ItemOutline() {} //Prevent initialization.

	/// <summary>
	/// Gets the manager.
	/// </summary>
	/// <returns>The ItemOutline instance.</returns>
	public static ItemOutline getManager() {
		if (instance == null) {
			instance = ItemOutline.thisTransStatic.gameObject.GetComponent<ItemOutline>();
		}
		return instance;
	}
	#endregion

	#region Unity methods
	void Awake() {
		thisTransStatic = gameObject.transform;
	}
	#endregion

	/// <summary>
	/// Outlines the item.
	/// </summary>
	/// <returns><c>true</c>, if item was outlined, <c>false</c> otherwise.</returns>
	/// <param name="item">Item to outline.</param>
	/// <param name="outlineColor">The color to outline the item with.</param>
	/// <param name="viewer">The InventoryViewer involved.</param>
	public bool outlineItem(Item item, Color outlineColor, InventoryViewer viewer) {
		if (!outlinedItems.Contains(item.getUniqueId()) && item.GetComponent<Renderer>() != null) {
			if (items.ContainsKey(viewer)) {
				stopOutlining(viewer);
			}
			Renderer rend = item.GetComponent<Renderer>();

			outlinedItems.Add(item.getUniqueId());
			items.Add(viewer, item.getUniqueId());
			originalShaders.Add(item.getUniqueId(), rend.material.shader);

			rend.material.shader = outlineShader;
			rend.material.SetColor("_OutlineColor", outlineColor);
			return true;
		}
		return false;
	}

	/// <summary>
	/// Stops outlining an item
	/// </summary>
	/// <returns><c>true</c>, if outlining was stoped, <c>false</c> otherwise.</returns>
	/// <param name="viewer">The InventoryViewer involved</param>
	public bool stopOutlining(InventoryViewer viewer) {
		if (items.ContainsKey(viewer)) {
			Guid itemId = (Guid)items[viewer];
			Item item = ItemManager.getItemManager().getItem(itemId);

			Renderer rend = item.GetComponent<Renderer>();
			rend.material.shader = (Shader)originalShaders[item.getUniqueId()];

			//Remove from all lists/tables
			outlinedItems.Remove(itemId);
			originalShaders.Remove(itemId);
			items.Remove(viewer);
			return true;
		}
		return false;
	}
}
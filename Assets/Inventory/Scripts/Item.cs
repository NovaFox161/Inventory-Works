using UnityEngine;
using System;
using System.Collections.Generic;

[AddComponentMenu("Inventory/Item")]
[ExecuteInEditMode]
/// <summary>
/// Item. This is to be added to a GameObject to denote that it is an item that can be added to an inventory.
/// </summary>
public class Item : MonoBehaviour {
	public string itemName;

	public itemMetaStruct itemMeta;

	[UniqueIdentifier] // Treat this special in the editor.
	public string uniqueId = Guid.NewGuid().ToString();


	#region Bools/Checkers
	/// <summary>
	/// Checks if the Item has Item Meta.
	/// </summary>
	/// <returns><c>true</c>, if the item has ItemMeta, <c>false</c> otherwise.</returns>
	public bool hasItemMeta() {
		return itemMeta.addMeta;
	}

	/// <summary>
	/// Checks if the two items are similar (Unique ID excluded)
	/// </summary>
	/// <returns><c>true</c>, if items are similar, <c>false</c> otherwise.</returns>
	/// <param name="compareTo">The item to compare to.</param>
	public bool isSimilar(Item compareTo) {
		if (compareTo.getName().Equals(itemName)) {
			if (compareTo.hasItemMeta() && hasItemMeta()) {
				if (compareTo.getItemMeta().matches(getItemMeta())) {
					return true;
				}
			} else if (!compareTo.hasItemMeta() && !hasItemMeta()) {
				return true;
			}
		}
		return false;
	} 
	#endregion

	#region Getters
	/// <summary>
	/// Gets the item's item name.
	/// </summary>
	/// <returns>The name of the item.</returns>
	public string getName() {
		return itemName;
	}

	/// <summary>
	/// Gets the Unique identifier of the item.
	/// **WARNING** Only use this if you know exactly what you are doing.
	/// </summary>
	/// <returns>The unique identifier.</returns>
	public Guid getUniqueId() {
		return new Guid(uniqueId);
	}

	/// <summary>
	/// Gets the Item Meta of the item, if it has item meta.
	/// </summary>
	/// <returns>The item meta.</returns>
	public ItemMeta getItemMeta() {
		ItemMeta meta = new ItemMeta();
		meta.setDisplayName(itemMeta.displayName);
		meta.setLore(itemMeta.lore);

		return meta;
	}
	#endregion

	#region Setters
	/// <summary>
	/// Sets the item's ItemMeta to the specified meta.
	/// </summary>
	/// <param name="meta">The ItemMeta to set the item with.</param>
	public void setItemMeta(ItemMeta meta) {
		itemMeta.addMeta = true;

		if (meta.hasDisplayName()) {
			itemMeta.displayName = meta.getDisplayName();
		}

		if (meta.hasLore()) {
			itemMeta.lore.Clear();
			foreach (string s in meta.getLore()) {
				itemMeta.lore.Add(s);
			}
		}
	}

	/// <summary>
	/// Clears the item's ItemMeta.
	/// **WARNING** Use with care!! This will clear the item meta and cannot be undone!!
	/// </summary>
	public void clearItemMeta() {
		itemMeta.addMeta = false;
		itemMeta.displayName = "";
		itemMeta.lore.Clear();
	}


	/// <summary>
	/// Resets the Unique identifier of the item.
	/// **WARNING** Only use this if you know exactly what you are doing. 
	/// This can and will break the inventory if used improperly.
	/// </summary>
	[ContextMenu("Reset UniqueId")]
	public void resetUniqueId() {
		uniqueId = Guid.NewGuid().ToString();
	}
	#endregion
}
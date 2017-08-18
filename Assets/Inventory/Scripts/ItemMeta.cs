using System;
using System.Collections.Generic;

[Serializable]
/// <summary>
/// Item meta. This is extra info an item or itemstack can have attached.
/// This contains methods and properties for holding custom data.
/// </summary>
public class ItemMeta {
	string displayName;
	readonly List<string> lore = new List<string>();

	/// <summary>
	/// Initializes a new instance of the <see cref="ItemMeta"/> class.
	/// </summary>
	public ItemMeta() {}

	#region Bools & Checkers
	/// <summary>
	/// Checks if the meta contains a cusom display name.
	/// </summary>
	/// <returns><c>true</c>, if the meta has a display name, <c>false</c> otherwise.</returns>
	public bool hasDisplayName() {
		return !String.IsNullOrEmpty(displayName);
	}

	/// <summary>
	/// Checks if the meta contains custom lore.
	/// </summary>
	/// <returns><c>true</c>, if the meta has lore, <c>false</c> otherwise.</returns>
	public bool hasLore() {
		return lore.Count > 0;
	}

	/// <summary>
	/// Compares the two ItemMeta objects and checks if they are the same.
	/// </summary>
	/// <returns><c>true</c> if the ItemMeta matches, <c>false</c> otherwise</returns>
	/// <param name="compare">The ItemMeta to compare to.</param>
	public bool matches(ItemMeta compare) {
		//Check display names
		if (compare.hasDisplayName() && !hasDisplayName()) {
			return false;
		} else if (!compare.hasDisplayName() && hasDisplayName()) {
			return false;
		} else if (compare.hasDisplayName() && hasDisplayName()) {
			if (!compare.displayName.Equals(displayName)) {
				return false;
			}
		}

		//Check lore
		if (compare.hasLore() && !hasLore()) {
			return false;
		} else if (!compare.hasLore() && hasLore()) {
			return false;
		} else if (compare.hasLore() && hasLore()) {
			if (compare.getLore().Count != getLore().Count) {
				return false;
			} else {
				for (int i = 0; i < getLore().Count; i++) {
					if (!compare.getLore()[i].Equals(lore[i])) {
						return false;
					}
				}
			}
		}
		return true;
	}
	#endregion

	#region Getters
	/// <summary>
	/// Gets the display name of the item if one exists.
	/// </summary>
	/// <returns>The display name of the item.</returns>
	public string getDisplayName() {
		return displayName;
	}

	/// <summary>
	/// Gets the custom lore of the item if any exists.
	/// </summary>
	/// <returns>The lore of the item.</returns>
	public List<string> getLore() {
		return lore;
	}
	#endregion

	#region Setters
	/// <summary>
	/// Sets the display name.
	/// </summary>
	/// <param name="_displayName">The new display name.</param>
	public void setDisplayName(string _displayName) {
		displayName = _displayName;
	}

	/// <summary>
	/// Sets the custom lore of the item.
	/// </summary>
	/// <param name="_lore">The new lore.</param>
	public void setLore(List<string> _lore) {
		lore.Clear();

		foreach (string loreText in _lore) {
			lore.Add(loreText);
		}
	}
	#endregion
}

[Serializable]
public class itemMetaStruct {
	public bool addMeta;

	public string displayName;
	public List<string> lore = new List<string>();
}
using UnityEngine.UI;
using System.Collections.Generic;
using System;

/// <summary>
/// The Inventory Object. This is a virtual backpack for a player.
/// </summary>
public class Inventory {
	InventoryType type;
	int size;
	int maxStackSize = 64;
	string name;
	bool inventoryOpen;
	Image inventoryBg;
	Text invNameText;

	InventoryViewer owner;
	InventoryViewer viewer;

	Hotbar hotbar;

	//Events
	/// <summary>
	/// Occurs when the inventory is opened.
	/// </summary>
	public event Action<Inventory> onInventoryOpenEvent;
	/// <summary>
	/// Occurs when the inventory is closed.
	/// </summary>
	public event Action<Inventory> onInventoryCloseEvent;
	/// <summary>
	/// Occurs when the inventory is updated.
	/// </summary>
	public event Action<Inventory> onInventoryUpdateEvent;

	readonly SortedDictionary<int, ItemStack> itemStacks = new SortedDictionary<int, ItemStack>();

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="Inventory"/> class.
	/// </summary>
	/// <param name="_name">The Inventory's name.</param>
	/// <param name="_size">The size of the inventory (must be a multiple of 8)</param>
	public Inventory(string _name, int _size) {
		type = InventoryType.Player;
		inventoryOpen = false;
		name = _name;
		size = _size;
		for (int i = 0; i < size; i++) {
			ItemStack newItemStack = new ItemStack(null, 0, true);
			newItemStack.setIndex(i);
			newItemStack.setInventory(this);
			newItemStack.setItemSlotType(ItemSlotType.Inventory);
			itemStacks.Add(i, newItemStack);
		}

		//Access manager to set positions of itemSlot images.
		InventoryUI.getUI().setInventoryDimensions(this);

		Close();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Inventory"/> class.
	/// </summary>
	/// <param name="_name">The Inventory's name.</param>
	/// <param name="_size">The size of the inventory (must be a multiple of 8)</param>
	/// <param name="_owner">The owner of the inventory</param>
	public Inventory(string _name, int _size, InventoryViewer _owner) {
		type = InventoryType.Player;
		inventoryOpen = false;
		name = _name;
		size = _size;
		owner = _owner;

		for (int i = 0; i < size; i++) {
			ItemStack newItemStack = new ItemStack(null, 0, true);
			newItemStack.setIndex(i);
			newItemStack.setInventory(this);
			newItemStack.setItemSlotType(ItemSlotType.Inventory);
			itemStacks.Add(i, newItemStack);
		}

		hotbar = new Hotbar(this);

		//Access manager to set positons of itemStot images.
		InventoryUI.getUI().setInventoryDimensions(this);

		Close();
		hotbar.displayHotbar();
	}
	#endregion

	#region Public inventory operations
	/// <summary>
	/// Opens the inventory and displays it on screen.
	/// </summary>
	public void Open() {
		UpdateInventory();

		if (hasHotbar()) {
			hotbar.hideHotbar();
		}
		inventoryOpen = true;
		inventoryBg.gameObject.SetActive(true);
		if (invNameText != null) {
			invNameText.text = getName();
		}
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack stack;
				itemStacks.TryGetValue(i, out stack);
				stack.getItemImage().gameObject.SetActive(true);
			}
		}
		//event on open
		if (onInventoryOpenEvent != null) {
			onInventoryOpenEvent(this);
		}
	}

	/// <summary>
	/// Opens the inventory and displays it to the specified viewer.
	/// </summary>
	/// <param name="_viewer">The viewer of the inventory.</param>
	public void Open(InventoryViewer _viewer) {
		viewer = _viewer;
		Open();
	}

	/// <summary>
	/// Closes the inventory and removes it from the screen.
	/// </summary>
	public void Close() {
		inventoryOpen = false;
		viewer = null;
		inventoryBg.gameObject.SetActive(false);
		if (invNameText != null) {
			invNameText.text = "";
		}
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack stack;
				itemStacks.TryGetValue(i, out stack);
				stack.getItemImage().gameObject.SetActive(false);
			}
		}

		if (hasHotbar()) {
			hotbar.displayHotbar();
		}
		//event on close
		if (onInventoryCloseEvent != null) {
			onInventoryCloseEvent(this);
		}
	}

	/// <summary>
	/// Updates the inventory to display the correct data. 
	/// If the inventory has a hotbar, this will also update that.
	/// </summary>
	public void UpdateInventory() {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack stack;
				itemStacks.TryGetValue(i, out stack);
				if (!stack.getName().Equals("Air")) {
					stack.setItemSprite(InventoryUI.getUI().getImageForItem(stack.getName()));
					stack.getItemCountText().text = stack.getAmount().ToString();
				} else {
					stack.setItemSprite(InventoryUI.getUI().getImageForItem("Air"));
					stack.getItemCountText().text = "";
				}
			}
		}

		if (hasHotbar()) {
			hotbar.UpdateHotbar();
		}
		if (onInventoryUpdateEvent != null) {
			onInventoryUpdateEvent(this);
		}
	}
	#endregion

	#region Bools & Checkers
	/// <summary>
	/// Checks if the inventory is open.
	/// </summary>
	/// <returns><c>true</c>, if the inventory is open, <c>false</c> otherwise.</returns>
	public bool isOpen() {
		return inventoryOpen;
	}

	/// <summary>
	/// Checks if the inventory has an owner.
	/// </summary>
	/// <returns><c>true</c>, if the inventory has an owner, <c>false</c> otherwise.</returns>
	public bool hasOwner() {
		return owner != null;
	}

	/// <summary>
	/// Checks if the inventory has a viewer.
	/// </summary>
	/// <returns><c>true</c>, the inventory has a viewer, <c>false</c> otherwise.</returns>
	public bool hasViewer() {
		return viewer != null;
	}

	/// <summary>
	/// Checks if the inventory has a hotbar.
	/// </summary>
	/// <returns><c>true</c>, of the inventory has a hotbar., <c>false</c> otherwise.</returns>
	public bool hasHotbar() {
		return hotbar != null;
	}

	/// <summary>
	/// Checks if the inventory contains the specified itemstack (Exact data, meta included).
	/// </summary>
	/// <param name="itemStack">the itemStack to check.</param>
	public bool contains (ItemStack itemStack) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.getName().Equals(itemStack.getName())) {
					if (compareTo.getAmount() == itemStack.getAmount()) {
						if (compareTo.hasItemMeta() && itemStack.hasItemMeta()) {
							if (compareTo.getItemMeta().matches(itemStack.getItemMeta())) {
								return true;
							}
						} else if (!compareTo.hasItemMeta() && !itemStack.hasItemMeta()) {
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Checks if the inventory contains the specified itemstack and amount (or greater) (meta included).
	/// </summary>
	/// <param name="itemStack">The ItemStack to check.</param>
	/// <param name="amount">The amount of this item required</param>
	public bool contains(ItemStack itemStack, int amount) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (itemStack.isSimilar(compareTo.getName())) {
					if (compareTo.getAmount() >= amount) {
						if (compareTo.hasItemMeta() && itemStack.hasItemMeta()) {
							if (compareTo.getItemMeta().matches(itemStack.getItemMeta())) {
								return true;
							}
						} else if (!compareTo.hasItemMeta() && !itemStack.hasItemMeta()) {
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Checks if the inventory contains the specified item (amount excluded, meta included).
	/// </summary>
	/// <param name="item">The Item to check for.</param>
	public bool contains(Item item) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.isSimilar(item.getName())) {
					if (compareTo.hasItemMeta() && item.hasItemMeta()) {
						if (compareTo.getItemMeta().matches(item.getItemMeta())) {
							return true;
						}
					} else if (!compareTo.hasItemMeta() && !item.hasItemMeta()) {
						return true;
					}
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Checks if the inventory contains the specified item and amount (Meta included).
	/// </summary>
	/// <param name="item">The Item to check for.</param>
	/// <param name="amount">The amount of this item required.</param>
	public bool contains(Item item, int amount) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.isSimilar(item.getName())) {
					if (compareTo.getAmount() >= amount) {
						if (compareTo.hasItemMeta() && item.hasItemMeta()) {
							if (compareTo.getItemMeta().matches(item.getItemMeta())) {
								return true;
							}
						} else if (!compareTo.hasItemMeta() && !item.hasItemMeta()) {
							return true;
						}
					}
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Checks if the invntory contains items with the specified name (Amount and meta excluded).
	/// </summary>
	/// <param name="itemName">The name of the item to check for.</param>
	public bool contains(String itemName) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (itemName == compareTo.getName()) {
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Checks if the inventory contains items with the specified name and amount (meta excluded).
	/// </summary>
	/// <param name="itemName">The name of the item to check for.</param>
	/// <param name="amount">The amount of the item required.</param>
	public bool contains(String itemName, int amount) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (itemName == compareTo.getName() && compareTo.getAmount() >= amount) {
					return true;
				}
			}
		}
		return false;
	}
	#endregion

	#region Getters
	/// <summary>
	/// Gets the index of the first item slot containing the specified itemstack (exact reference)
	/// </summary>
	/// <param name="item">The item to check for.</param>
	/// <returns>The index of the first item slot containing the item or <code>-1</code> if not present.</returns>
	public int first(ItemStack item) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (item == compareTo) {
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the index of the first item slot containing the specified item (amount excluded, meta included).
	/// </summary>
	/// <param name="item">The item to check for</param>
	/// <returns>The index of the first item slot containing the item or <code>-1</code> if not present.</returns>
	public int first(Item item) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.isSimilar(item.getName())) {
					if (compareTo.hasItemMeta() && item.hasItemMeta()) {
						if (compareTo.getItemMeta().matches(item.getItemMeta())) {
							return i;
						}
					} else if (!compareTo.hasItemMeta() && !item.hasItemMeta()) {
						return i;
					}
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the index of the first item slot containing the specified item (amount excluded).
	/// </summary>
	/// <param name="itemName">The name of the item to check for.</param>
	/// <returns>The index of the first item slot containing the item or <code>-1</code> if not present</returns>
	public int first(String itemName) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.isSimilar(itemName)) {
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the index of the first item slot that does not contain an item.
	/// </summary>
	/// <returns>The first empty slot containing no items or <code>-1</code> if not present.</returns>
	public int firstEmpty() {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo); 
				if (compareTo.getName().Equals("Air")) {
					return i;
				}
			}
		}
		return -1;
	}

	/// <summary>
	/// Gets the index of the first item slot that contains the specified item and can be added onto.
	/// Excludes Meta
	/// </summary>
	/// <returns>The index of the first slot that can be added onto, returns an empty slot if not found</returns>
	/// <param name="itemName">The name of the item to check for.</param>
	public int firstStackable(String itemName) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.getName().Equals(itemName)) {
					if (compareTo.getAmount() < maxStackSize) {
						return i;
					}
				}
			}
		}
		return firstEmpty();
	}

	/// <summary>
	/// Gets the index of the first item slot that contains the specified item and can be added onto.
	/// Includes Meta
	/// </summary>
	/// <returns>The index of the first slot that can be added onto, returns an empty slot if not found.</returns>
	/// <param name="item">The item to check for.</param>
	public int firstStackable(Item item) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.getName().Equals(item.getName())) {
					if (compareTo.getAmount() < maxStackSize) {
						if (compareTo.hasItemMeta() && item.hasItemMeta()) {
							if (compareTo.getItemMeta().matches(item.getItemMeta())) {
								return i;
							}
						} else if (!compareTo.hasItemMeta() && !item.hasItemMeta()) {
							return i;
						}
					}
				}
			}
		}
		return firstEmpty();
	}

	/// <summary>
	/// Gets the index of the first item slot that contains the specified item and can be added onto.
	/// Includes Meta.
	/// </summary>
	/// <returns>The index of the first slot that can be added onto, returns an empty slot if not found.</returns>
	/// <param name="item">The item to check for.</param>
	public int firstStackable(ItemStack item) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.getName().Equals(item.getName())) {
					if (compareTo.getAmount() < maxStackSize) {
						if (compareTo.hasItemMeta() && item.hasItemMeta()) {
							if (compareTo.getItemMeta().matches(item.getItemMeta())) {
								return i;
							}
						} else if (!compareTo.hasItemMeta() && !item.hasItemMeta()) {
							return i;
						}
					}
				}
			}
		}
		return firstEmpty();
	}

	/// <summary>
	/// Gets a list of all itemstacks within the inventory. "Air" included.
	/// </summary>
	/// <returns>A full ist of all itemstacks within the inventory.</returns>
	public List<ItemStack> getContents() {
		List<ItemStack> stacks = new List<ItemStack>();
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack stack;
				itemStacks.TryGetValue(i, out stack);
				stacks.Add(stack);
			}
		}
		return stacks;
	}

	/// <summary>
	/// Gets the itemstack from the specified itemstack. 
	/// </summary>
	/// <returns>The specified itemstack, <code>null</code> if out of bounds</returns>
	/// <param name="index">The index of the item slot to get.</param>
	public ItemStack getItem(int index) {
		if (itemStacks.ContainsKey(index)) {
			ItemStack item;
			itemStacks.TryGetValue(index, out item);
			return item;
		}
		return null;
	}

	/// <summary>
	/// Gets the max stack size of a single itemstack.
	/// </summary>
	/// <returns>The max stack size of a single itemstack.</returns>
	public int getMaxStackSize() {
		return maxStackSize;
	}

	/// <summary>
	/// Gets the name of the inventory.
	/// </summary>
	/// <returns>The name of the inventory.</returns>
	public string getName() {
		return name;
	}

	/// <summary>
	/// Gets the size of the inventory, a multiple of 8.
	/// </summary>
	/// <returns>The size of the inventory.</returns>
	public int getSize() {
		return size;
	}

	/// <summary>
	/// Gets the type of this inventory.
	/// </summary>
	/// <returns>The type of this inventory.</returns>
	public InventoryType getType() {
		return type;
	}

	/// <summary>
	/// Gets the owner of the inventory.
	/// </summary>
	/// <returns>The owner of the inventory, <code>null</code> if one does not exist.</returns>
	public InventoryViewer getOwner() {
		return owner;
	}

	/// <summary>
	/// Gets the current viewer of the inventory.
	/// </summary>
	/// <returns>The current viewer of the inventory, <code>null</code> if one does not exist.</returns>
	public InventoryViewer getViewer() {
		return viewer;
	}

	/// <summary>
	/// Gets the inventory's child hotbar.
	/// </summary>
	/// <returns>The inventory's hotbar, <code>null</code> if one does not exist.</returns>
	public Hotbar getHotbar() {
		return hotbar;
	}
	#endregion

	#region Setters
	/// <summary>
	/// Sets the specified item slot as the specified itemstack.
	/// **WARNING** This will set it regardless of it's current itemstack.
	/// </summary>
	/// <returns><c>true</c>, if item slot was successfully set, <c>false</c> otherwise.</returns>
	/// <param name="index">The index of the item slot.</param>
	/// <param name="item">The itemstack to set the item slot as.</param>
	public bool setItem(int index, ItemStack item) {
		if (index < size) {
			if (itemStacks.Count < size) {
				ItemStack orStack;
				itemStacks.TryGetValue(index, out orStack);
				itemStacks.Remove(index);
				orStack.setItemStack(item);
				itemStacks.Add(index, orStack);
				UpdateInventory();
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Adds the specified item to the inventory. 
	/// This will automatically chose the proper item slot.
	/// </summary>
	/// <returns><c>true</c>, if item was successfully added, <c>false</c> otherwise.</returns>
	/// <param name="item">The itemstack to add to the inventory.</param>
	public bool addItem(ItemStack item) {
		int index = firstStackable(item);
		if (index > -1) {
			ItemStack orStack;
			itemStacks.TryGetValue(index, out orStack);
			orStack.setItem(item.getName());
			orStack.setAmount(orStack.getAmount() + 1);
			orStack.setItemMeta(item.getItemMeta());
			itemStacks.Remove(index);
			itemStacks.Add(index, orStack);
			UpdateInventory();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Removes the specified itemstack from the inventory (Exact Reference).
	/// **WARNING** This removes ALL occurances of the itemstack.
	/// </summary>
	/// <param name="item">The itemstack to remove from the inventory.</param>
	public void remove(ItemStack item) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo == item) {
					resetStack(i);
					continue;
				}
			}
		}
		UpdateInventory();
	}

	/// <summary>
	/// Removes the specified item from the inventory.
	/// **WARNING** this removes ALL occurances of the item.
	/// </summary>
	/// <param name="item">The item to remove from the inventory.</param>
	public void remove(Item item) {
		for (int i = 0; i < size; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack compareTo;
				itemStacks.TryGetValue(i, out compareTo);
				if (compareTo.getName() != null && item.getName() == compareTo.getName()) {
					resetStack(i);
					continue;
				}
			}
		}
		UpdateInventory();
	}

	/// <summary>
	/// Clears the ENTIRE inventory.
	/// **WARNING** Only use when needed!!! This clears the ENTIRE inventory!!
	/// </summary>
	public void clear() {
		for (int i = 0; i < size; i++) {
			resetStack(i);
		}
	}

	/// <summary>
	/// Clears the specified item slot and resets it to "Air"
	/// </summary>
	/// <param name="index">The index of the item slot to reset.</param>
	public void clearSlot(int index) {
		resetStack(index);
	}

	/// <summary>
	/// Sets the max stack size of a single item slot.
	/// </summary>
	/// <param name="_maxStackSize">The new max stack size for a single item slot</param>
	public void setMaxStackSize(int _maxStackSize) {
		maxStackSize = _maxStackSize;
	}

	/// <summary>
	/// Sets the type this inventory is.
	/// **WARNING** Use with care, this can break the inventory if done improperly!
	/// </summary>
	/// <param name="_type">The new Type of inventory.</param>
	public void setType(InventoryType _type) {
		type = _type;
	}
	#endregion

	#region Private inventory operations
	/// <summary>
	/// Resets the specified item slot.
	/// </summary>
	/// <param name="index">The index of the slot to reset.</param>
	void resetStack(int index) {
		if (itemStacks.ContainsKey(index)) {
			ItemStack original;
			itemStacks.TryGetValue(index, out original);

			original.clear();

			itemStacks.Remove(index);
			itemStacks.Add(index, original);

			if (hasHotbar()) {
				hotbar.UpdateHotbar();
			}
		}
	}
	#endregion

	#region Public utility methods
	/// <summary>
	/// Sets the inventory's background image.
	/// **WARNING** Use with care as to not break the Inventory UI.
	/// </summary>
	/// <param name="bg">The inventory's new background image.</param>
	public void setInventoryBackground(Image bg) {
		inventoryBg = bg;
	}

	/// <summary>
	/// Sets the inventory's name text.
	/// **WARNING** Use with care as to not break the Inventory UI.
	/// </summary>
	/// <param name="_text">The inventory's new inv name Text.</param>
	public void setInventoryNameText(Text _text) {
		invNameText = _text;
	} 

	/// <summary>
	/// Gets the inventory's background image.
	/// </summary>
	/// <returns>The inventory's background.</returns>
	public Image getInventoryBackground() {
		return inventoryBg;
	}

	/// <summary>
	/// Gets the inventory's name text object.
	/// </summary>
	/// <returns>The inventory name text.</returns>
	public Text getInventoryNameText() {
		return invNameText;
	}
	#endregion
}
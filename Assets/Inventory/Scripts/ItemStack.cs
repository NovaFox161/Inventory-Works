using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// Itemstack. An Object that contains data about an imaginary item.
/// This is used for storing items in an inventory or for other purposes.
/// </summary>
public class ItemStack {
	//Utils
	int index;
	Inventory inventory;

	//ItemStack vars
	int amount;
	String itemName = "Air";
	ItemMeta meta;
	ItemSlotType slotType = ItemSlotType.Inventory;

	//Functional vars
	Image itemImage;
	Text countText;
	Vector3 posOr;
	readonly bool physical;

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="ItemStack"/> class.
	/// Amount defaults to one if item is not null.
	/// Defaults to <code>phsical = false</code>.
	/// </summary>
	/// <param name="_item">The item to set the itemstack as.</param>
	public ItemStack(Item _item) {
		physical = false;
		index = -1;

		itemName = _item != null ? _item.getName() : "Air";
		if (_item != null && _item.hasItemMeta()) {
			meta = _item.getItemMeta();
		}

		amount = _item == null ? 0 : 1;

		generateImages();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ItemStack"/> class.
	/// Amount defaults to one if item is not null.
	/// </summary>
	/// <param name="_item">The item to set the itemstack as.</param>
	/// <param name="_physical">If set to <c>true</c> the itemstack is physical, or has an image to go with it.</param>
	public ItemStack(Item _item, bool _physical) {
		physical = _physical;
		index = -1;

		itemName = _item != null ? _item.getName() : "Air";
		if (_item != null && _item.hasItemMeta()) {
			meta = _item.getItemMeta();
		}

		amount = _item == null ? 0 : 1;
		generateImages();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ItemStack"/> class.
	/// Defaults to <code>phsical = false</code>.
	/// </summary>
	/// <param name="_item">The item to set the itemstack as.</param>
	/// <param name="_amount">The amount of items in the itemstack.</param>
	public ItemStack(Item _item, int _amount) {
		physical = false;
		index = -1;

		itemName = _item != null ? _item.getName() : "Air";
		if (_item != null && _item.hasItemMeta()) {
			meta = _item.getItemMeta();
		}

		amount = _amount;

		generateImages();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ItemStack"/> class.
	/// </summary>
	/// <param name="_item">The item to set the itemstack as.</param>
	/// <param name="_amount">The amount of items in the itemstack.</param>
	/// <param name="_physical">If set to <c>true</c> the itemstack is physical or has an image to go with it.</param>
	public ItemStack(Item _item, int _amount, bool _physical) {
		physical = _physical;
		index = -1;

		itemName = _item != null ? _item.getName() : "Air";
		if (_item != null && _item.hasItemMeta()) {
			meta = _item.getItemMeta();
		}

		amount = _amount;

		generateImages();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ItemStack"/> class.
	/// Defaults to <code>phsical = false</code>.
	/// </summary>
	/// <param name="_item">The item to set the itemstack as.</param>
	/// <param name="_amount">The amount of items in the itemstack.</param>
	/// <param name="_meta">The item's ItemMeta.</param>
	public ItemStack(Item _item, int _amount, ItemMeta _meta) {
		physical = false;
		index = -1;
		itemName = _item != null ? _item.getName() : "Air";
		amount = _amount;
		meta = _meta;
		generateImages();
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ItemStack"/> class.
	/// </summary>
	/// <param name="_item">The item to set the itemstack to.</param>
	/// <param name="_amount">The amount of items in the itemstack.</param>
	/// <param name="_meta">The item's ItemMeta.</param>
	/// <param name="_physical">If set to <c>true</c> the itemstack is physical or has an image to go with it.</param>
	public ItemStack(Item _item, int _amount, ItemMeta _meta, bool _physical) {
		physical = _physical;
		index = -1;
		itemName = _item != null ? _item.getName() : "Air";
		amount = _amount;
		meta = _meta;
		generateImages();
	}
	#endregion

	#region Bools & Checkers
	/// <summary>
	/// Checks if the itemstacks are similar to each other (amount/meta/etc excluded)
	/// </summary>
	/// <returns><c>true</c>, if the two stacks are similar, <c>false</c> otherwise.</returns>
	/// <param name="compareName">The item name of the itemstack to compare.</param>
	public bool isSimilar(string compareName) {
		return compareName == itemName;
	}

	/// <summary>
	/// Cheks if the itemstacks are similar to each other (All itemstack data).
	/// </summary>
	/// <returns><c>true</c>, if the two stacks are similar, <c>false</c> otherwise.</returns>
	/// <param name="compare">The itemstack to compare to.</param>
	public bool isSimilar(ItemStack compare) {
		if (compare.getName().Equals(itemName)) {
			if (compare.getAmount() == getAmount()) {
				if (compare.hasItemMeta() && hasItemMeta()) {
					if (getItemMeta().matches(compare.getItemMeta())) {
						return true;
					}
				} else if (!compare.hasItemMeta() && !hasItemMeta()) {
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Checks if th4e itemstack has ItemMeta
	/// </summary>
	/// <returns><c>true</c>, if the itemstack has ItemMeta, <c>false</c> otherwise.</returns>
	public bool hasItemMeta() {
		return meta != null;
	}
	#endregion

	#region Getters
	/// <summary>
	/// Gets the name of the item in the itemstack.
	/// </summary>
	/// <returns>The name of the item.</returns>
	public String getName() {
		return itemName;
	}

	/// <summary>
	/// Gets the amount of items in the itemstack.
	/// </summary>
	/// <returns>The amount of items in the itemstack.</returns>
	public int getAmount() {
		return amount;
	}

	/// <summary>
	/// Gets the itemstack's ItemMeta.
	/// You should first check if it has ItemMeta.
	/// </summary>
	/// <returns>The itemstack's ItemMeta, <c>null</c> if it does not have any.</returns>
	public ItemMeta getItemMeta() {
		return meta;
	}

	/// <summary>
	/// Gets the itemstack's ItemSlot Type.
	/// </summary>
	/// <returns>The itemstack's ItemSlot Type</returns>
	public ItemSlotType getItemSlotType() {
		return slotType;
	}
	#endregion

	#region Setters
	/// <summary>
	/// Clears the itemstack and resets it to "Air"
	/// </summary>
	public void clear() {
		itemName = "Air";
		amount = 0;
		meta = null;

		generateImages();
	}

	/// <summary>
	/// Sets this itemstack to be a copy of another itemstack.
	/// If the itemstack is physical, it will update the image.
	/// </summary>
	/// <param name="stackB">The stack to clone/set as.</param>
	public void setAs(ItemStack stackB) {
		itemName = stackB.getName();
		amount = stackB.getAmount();
		meta = stackB.getItemMeta();

		if (physical) {
			itemImage.overrideSprite = !itemName.Equals("Air") ? InventoryUI.getUI().getImageForItem(itemName) : InventoryUI.getUI().getImageForItem("Air");
			countText.text = !itemName.Equals("Air") ? stackB.getAmount().ToString() : "";
		}
	}

	/// <summary>
	/// Sets this itemstack to be a copy of another itemstack.
	/// This will also change it's itemImage to the one provided (if physical).
	/// </summary>
	/// <param name="stackB">The stack to clone/set as.</param>
	/// <param name="image">The image to use to replace the old image.</param>
	public void setAs(ItemStack stackB, Image image) {
		itemName = stackB.getName();
		amount = stackB.getAmount();
		meta = stackB.getItemMeta();

		if (physical) {
			itemImage = image;

			itemImage.overrideSprite = !itemName.Equals("Air") ? InventoryUI.getUI().getImageForItem(itemName) : InventoryUI.getUI().getImageForItem("Air");
			countText.text = !itemName.Equals("Air") ? stackB.getAmount().ToString() : "";
		}
	}

	/// <summary>
	/// Sets the itemstack as the item. (Amount excluded)
	/// </summary>
	/// <param name="_item">The item to set the itemstack as.</param>
	public void setItem(Item _item) {
		itemName = _item != null ? _item.getName() : "Air";
		if (_item != null && _item.hasItemMeta()) {
			meta = _item.getItemMeta();
		}
		generateImages();
	}

	/// <summary>
	/// Sets the itemstack to the provided item name (Amount excluded).
	/// </summary>
	/// <param name="_itemName">The item name to set the itemstack as.</param>
	public void setItem(String _itemName) {
		itemName = _itemName;
		generateImages();
	}

	/// <summary>
	/// Sets the itemstack as the item and amount.
	/// </summary>
	/// <param name="_itemName">The item name to set the itemstack as.</param>
	/// <param name="_amount">The amount to set the itemstack as.</param>
	public void setItem(String _itemName, int _amount) {
		itemName = _itemName;
		amount = _amount;
		generateImages();
	}

	/// <summary>
	/// Sets the itemstack to be similar to another itemstack.
	/// </summary>
	/// <param name="stack">the itemstack to clone/be similar to.</param>
	public void setItemStack(ItemStack stack) {
		itemName = stack.getName();
		amount = stack.getAmount();
		meta = stack.getItemMeta();

		generateImages();
	}

	/// <summary>
	/// Sets the amount of this itemstack.
	/// </summary>
	/// <param name="_amount">The new amount of this itemstack.</param>
	public void setAmount(int _amount) {
		amount = _amount;
		if (_amount > 0) {
			if (physical) {
				countText.text = amount.ToString();
			}
		}
	}

	/// <summary>
	/// Sets the itemstack's ItemMeta.
	/// </summary>
	/// <param name="_meta">The new ItemMeta.</param>
	public void setItemMeta(ItemMeta _meta) {
		meta = _meta;
	}

	/// <summary>
	/// Sets the itemstack's ItemSlot Type.
	/// </summary>
	/// <param name="_slotType">The new ItemSlot type</param>
	public void setItemSlotType(ItemSlotType _slotType) {
		slotType = _slotType;
	}
	#endregion

	#region Public Technical functions
	/// <summary>
	/// Gets the Unity world space of this itemstack if it is physical.
	/// </summary>
	/// <returns>The Unity World space position of this itemstack.</returns>
	public Vector3 getPosition() {
		return posOr;
	}

	/// <summary>
	/// Sets the Unity World space of this itemstack if physical.
	/// </summary>
	/// <param name="pos">The new position of the itemstack.</param>
	public void setPosition(Vector3 pos) {
		posOr = pos;
		if (physical) {
			itemImage.rectTransform.position = pos;
		}
	}

	/// <summary>
	/// Gets the itemstack's physical image.
	/// </summary>
	/// <returns>The itemstack's physical image, <c>null</c> if not physical.</returns>
	public Image getItemImage() {
		return itemImage;
	}

	/// <summary>
	/// Gets the current sprite being displayed on the phyical itemstack.
	/// </summary>
	/// <returns>The currently displayed sprite, <c>null</c> if one does not exist.</returns>
	public Sprite getItemSprite() {
		return itemImage.overrideSprite;
	}

	/// <summary>
	/// Gets the Itemstack's physical count text.
	/// </summary>
	/// <returns>The itemstack's count text, <c>null</c> if not physical.</returns>
	public Text getItemCountText() {
		return countText;
	}

	/// <summary>
	/// Sets the itemstack's physical image if physical.
	/// </summary>
	/// <param name="_itemImage">The new itemstack image.</param>
	public void setItemImage(Image _itemImage) {
		if (physical) {
			itemImage = _itemImage;
		}
	}

	/// <summary>
	/// Sets the itemstack's displayed sprite if physical
	/// </summary>
	/// <param name="_itemSprite">The new sprite to display.</param>
	public void setItemSprite(Sprite _itemSprite) {
		if (physical) {
			itemImage.overrideSprite = _itemSprite;
		}
	}

	/// <summary>
	/// Sets the itemstack's physical count text if physical.
	/// </summary>
	/// <param name="_countText">The new count text.</param>
	public void setItemCountText(Text _countText) {
		if (physical) {
			countText = _countText;
		}
	}

	/// <summary>
	/// Sets the itemstack's index within an inventory.
	/// **WARNING** DO NOT USE if you do not know exactly what you are doing.
	/// </summary>
	/// <param name="_index">Index.</param>
	public void setIndex(int _index) {
		if (index == -1) {
			index = _index;
		}
	}

	/// <summary>
	/// Gets the itemstack's index within an inventory, if in an inventory.
	/// </summary>
	/// <returns>The index within an inventory, <c>-1</c> if not in an inventory.</returns>
	public int getIndex() {
		return index;
	}

	/// <summary>
	/// Sets the inventory this itemstack is within.
	/// **WARNING** DO NOT USE if you do not know exactly what you are doing.
	/// </summary>
	/// <param name="_inventory">The inventory the itemstack is within.</param>
	public void setInventory(Inventory _inventory) {
		if (inventory == null) {
			inventory = _inventory;
		}
	}

	/// <summary>
	/// Gets the inventory the itemstack is within.
	/// </summary>
	/// <returns>The inventory the itemstack is in, <c>null</c> if not in an inventory.</returns>
	public Inventory getInventory() {
		return inventory;
	}
	#endregion

	#region Private Technical functions
	/// <summary>
	/// Generates the correct images and text to display if the itemstack is physical.
	/// </summary>
	void generateImages() {
		if (physical) {
			if (itemImage == null) {
				itemImage = (Image)GameObject.Instantiate(InventoryManager.getManager().defaultSlot);
			}
			if (countText == null) {
				countText = (Text)GameObject.Instantiate(InventoryManager.getManager().defaultItemCount);
			}

			itemImage.overrideSprite = InventoryUI.getUI().getImageForItem(itemName);
			countText.text = !itemName.Equals("Air") ? amount.ToString() : "";
		}
	}
	#endregion
}

public enum ItemSlotType {
	Inventory, Hotbar, Crafting, Output, Fuel, Armor
}
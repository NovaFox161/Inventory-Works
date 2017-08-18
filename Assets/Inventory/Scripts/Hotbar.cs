using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Hotbar. This is the hotbar class that stores information relating to the specific hotbar instance referanced.
/// </summary>
public class Hotbar {
	Inventory inventory;

	bool hotbarDisplayed;
	Image hotbarBg;
	Text hbItemInfoText;

	readonly SortedDictionary<int, ItemStack> itemStacks = new SortedDictionary<int, ItemStack>();

	#region Constructors
	/// <summary>
	/// Initializes a new instance of the <see cref="Hotbar"/> class.
	/// </summary>
	/// <param name="_inv">The parent Inventory.</param>
	public Hotbar(Inventory _inv) {
		inventory = _inv;

		hotbarDisplayed = false;

		for (int i = 0; i < 8; i++) {
			ItemStack newItemStack = new ItemStack(null, 0, true);
			newItemStack.setIndex(i);
			newItemStack.setInventory(inventory);
			newItemStack.setItemSlotType(ItemSlotType.Hotbar);
			newItemStack.setAs(inventory.getItem(i));
			itemStacks.Add(i, newItemStack);
		}
	}
	#endregion

	#region Public Hotbar Operations
	/// <summary>
	/// Displays the hotbar on screen.
	/// </summary>
	public void displayHotbar() {
		UpdateHotbar();
		hotbarDisplayed = true;
			
		hotbarBg.gameObject.SetActive(true);
		for (int i = 0; i < 8; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack stack;
				itemStacks.TryGetValue(i, out stack);
				stack.getItemImage().gameObject.SetActive(true);
			}
		}
	}

	/// <summary>
	/// Hides the hotbar on screen.
	/// </summary>
	public void hideHotbar() {
		hotbarDisplayed = false;
		hotbarBg.gameObject.SetActive(false);
		for (int i = 0; i < 8; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack stack;
				itemStacks.TryGetValue(i, out stack);
				stack.getItemImage().gameObject.SetActive(false);
			}
		}
	}

	/// <summary>
	/// Updates the hotbar to display the correct information.
	/// </summary>
	public void UpdateHotbar() {
		for (int i = 0; i < 8; i++) {
			if (itemStacks.ContainsKey(i)) {
				ItemStack stack;
				itemStacks.TryGetValue(i, out stack);
				stack.setAs(inventory.getItem(i));
				if (!stack.getName().Equals("Air")) {
					stack.setItemSprite(InventoryUI.getUI().getImageForItem(stack.getName()));
					stack.getItemCountText().text = stack.getAmount().ToString();
				} else {
					stack.setItemSprite(InventoryUI.getUI().getImageForItem("Air"));
					stack.getItemCountText().text = "";
				}
			}
		}
	}
	#endregion

	#region Getters
	/// <summary>
	/// Gets the hotbar's parent inventory.
	/// </summary>
	/// <returns>The inventory.</returns>
	public Inventory getInventory() {
		return inventory;
	}

	/// <summary>
	/// Checks if the hotbar is currently being display
	/// </summary>
	/// <returns><c>true</c>, if the hotbar is displayed, <c>false</c> otherwise.</returns>
	public bool isDisplayed() {
		return hotbarDisplayed;
	}

	/// <summary>
	/// Gets the hotbar's background image.
	/// </summary>
	/// <returns>The hotbar background.</returns>
	public Image getHotbarBg() {
		return hotbarBg;
	}

	/// <summary>
	/// Gets the hotbar's item info text.
	/// </summary>
	/// <returns>The hb item info text.</returns>
	public Text getHbItemInfoText() {
		return hbItemInfoText;
	}

	/// <summary>
	/// Gets specified itemstack from the slot.
	/// </summary>
	/// <returns>The itemstack from the hotbar slot</returns>
	/// <param name="i">The index of the slot to get</param>
	public ItemStack getHotbarSlot(int i) {
		if (itemStacks.ContainsKey(i)) {
			ItemStack stack;
			itemStacks.TryGetValue(i, out stack);
			return stack;
		}
		return null;
	}
	#endregion

	#region Setters
	/// <summary>
	/// Sets the hotbar Background
	/// </summary>
	/// <param name="_hotbarBg">The new Hotbar background.</param>
	public void setHotbarBg(Image _hotbarBg) {
		hotbarBg = _hotbarBg;
	}

	/// <summary>
	/// Sets the HB item info text.
	/// </summary>
	/// <param name="_text">The new HB Item Info Text.</param>
	public void setHBItemInfoText(Text _text) {
		hbItemInfoText = _text;
	}
	#endregion
}
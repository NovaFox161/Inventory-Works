using UnityEngine;
using System;
using System.Collections.Generic;

[AddComponentMenu("Inventory/Inventory Events")]
/// <summary>
/// Inventory events.
/// A singleton class which manages all inventory related events.
/// This contains all events that can be subscribed to and all of the calling methods for those events.
/// </summary>
public class InventoryEvents : MonoBehaviour {
	static InventoryEvents instance;
	static Transform thisTransStatic;

	//General Inventory events
	/// <summary>
	/// Occurs when an inventory was created.
	/// </summary>
	public event Action<Inventory> onInventoryCreateEvent;
	/// <summary>
	/// Occurs when an inventory was removed/deleted
	/// </summary>
	public event Action<Inventory> onInventoryRemoveEvent;
	/// <summary>
	/// Occurs when an inventory was opened.
	/// </summary>
	public event Action<Inventory> onInventoryOpenEvent;
	/// <summary>
	/// Occurs when an inventory was closed.
	/// </summary>
	public event Action<Inventory> onInventoryCloseEvent;
	/// <summary>
	/// Occurs when an inventory was updated.
	/// </summary>
	public event Action<Inventory> onInventoryUpdateEvent;

	//Detection events - In inventory
	/// <summary>
	/// Occurs when an itemstack is hovered over by the mouse cursor inside an inventory.
	/// </summary>
	public event Action<ItemStack> onHoverItemEvent;
	/// <summary>
	/// Occurs when an itemstack is left clicked inside an inventory.
	/// </summary>
	public event Action<ItemStack> onLeftClickItemEvent;
	/// <summary>
	/// Occurs when an itemstack is right clicked inside an inventory.
	/// </summary>
	public event Action<ItemStack> onRightClickItemEvent;

	//Detection events - Hotbar
	/// <summary>
	/// Occurs when a hotbar's selection has been changed.
	/// </summary>
	public event Action<Inventory, int> onHotbarSelectionChange;

	//Detections events - Outside inventory
	/// <summary>
	/// Occurs when a viewer left clicks with an item in their hand.
	/// </summary>
	public event Action<InventoryViewer, ItemStack, Item> onLeftClickInteractEvent;
	/// <summary>
	/// Occurs when a viewer right clicks with an item in their hand.
	/// </summary>
	public event Action<InventoryViewer, ItemStack, Item> onRightClickInteractEvent;

	//Inventory & item related events
	/// <summary>
	/// Occurs when an item is picked up and added to an inventory.
	/// </summary>
	public event Action<Inventory, Item, string> onItemPickUpEvent;
	/// <summary>
	/// Occurs when an item is dropped and removed from an inventory.
	/// </summary>
	public event Action<Inventory, string> onItemDropEvent;

	//Saver Events
	/// <summary>
	/// Occurs when an inventory is saved to file.
	/// </summary>
	public event Action<Inventory> onInventorySaveEvent;
	/// <summary>
	/// Occurs when an inventory is loaded from file.
	/// </summary>
	public event Action<Inventory> onInventoryLoadEvent;
	/// <summary>
	/// Occurs when all items within a scene are saved to file.
	/// </summary>
	public event Action<List<Guid>, List<Guid>, List<Item>> onItemSaveEvent;
	/// <summary>
	/// Occurs when all items within a scene are loaded from file.
	/// </summary>
	public event Action<ItemCacheSaveData> onItemLoadEvent;

	#region Unity methods
	void Awake() {
		thisTransStatic = gameObject.transform;
	}
	#endregion

	#region Instance handling
	InventoryEvents() {} //Prevent initialization

	/// <summary>
	/// Gets the instance of the InventoryEvents
	/// </summary>
	/// <returns>The instance of the InventoryEvents.</returns>
	public static InventoryEvents getEvents() {
		if (instance == null) {
			instance = InventoryEvents.thisTransStatic.gameObject.GetComponent<InventoryEvents>();
		}
		return instance;
	}
	#endregion

	#region General inventory event callers
	/// <summary>
	/// Calls the inventory create event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory that was created</param>
	public void callInventoryCreateEvent(Inventory inv) {
		inv.onInventoryOpenEvent += callInventoryOpenEvent;
		inv.onInventoryCloseEvent += callInventoryCloseEvent;
		inv.onInventoryUpdateEvent += callInventoryUpdateEvent;

		if (onInventoryCreateEvent != null) {
			onInventoryCreateEvent(inv);
		}
	}

	/// <summary>
	/// Calls the inventory remove event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory that was removed.</param>
	public void callInventoryRemoveEvent(Inventory inv) {
		inv.onInventoryOpenEvent -= callInventoryOpenEvent;
		inv.onInventoryCloseEvent -= callInventoryCloseEvent;
		inv.onInventoryUpdateEvent -= callInventoryUpdateEvent;

		if (onInventoryRemoveEvent != null) {
			onInventoryRemoveEvent(inv);
		}
	}

	/// <summary>
	/// Calls the inventory open event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory that was opened</param>
	public void callInventoryOpenEvent(Inventory inv) {
		if (onInventoryOpenEvent != null) {
			onInventoryOpenEvent(inv);
			if (InventoryManager.getManager().debug) {
				print("Opened inventory: " + inv.getName());
			}
		}
	}

	/// <summary>
	/// Calls the inventory close event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory that was closed.</param>
	public void callInventoryCloseEvent(Inventory inv) {
		if (onInventoryCloseEvent != null) {
			onInventoryCloseEvent(inv);
			if (InventoryManager.getManager().debug) {
				print("Closed inventory: " + inv.getName()); 
			}
		}
	}

	/// <summary>
	/// Calls the inventory update event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory that was updated.</param>
	public void callInventoryUpdateEvent(Inventory inv) {
		if (onInventoryUpdateEvent != null) {
			onInventoryUpdateEvent(inv);
		}
	}
	#endregion

	#region Detection Event - In Inventory; Callers
	/// <summary>
	/// Calls the hover item event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="_itemstack">The itemstack that was hovered on.</param>
	public void callHoverItemEvent(ItemStack _itemstack) {
		if (onHoverItemEvent != null) {
			onHoverItemEvent(_itemstack);
		}
	}

	/// <summary>
	/// Calls the left click item event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="_itemstack">The itemstack that was left clicked.</param>
	public void callLeftClickItemEvent(ItemStack _itemstack) {
		if (onLeftClickItemEvent != null) {
			onLeftClickItemEvent(_itemstack);
		}
	}

	/// <summary>
	/// Calls the right click item event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="_itemstack">The itemstack that wsa right clicked.</param>
	public void callRightClickItemEvent(ItemStack _itemstack) {
		if (onRightClickItemEvent != null) {
			onRightClickItemEvent(_itemstack);
		}
	}
	#endregion

	#region Detection Event - Hotbar; Callers
	/// <summary>
	/// Calls the hotbar selection change event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The parent inventory of the hotbar involved.</param>
	/// <param name="slotIndex">The new index of the selection</param>
	public void callHotbarSelectionChange(Inventory inv, int slotIndex) {
		if (onHotbarSelectionChange != null) {
			onHotbarSelectionChange(inv, slotIndex);
		}
	}
	#endregion

	#region Detection Event - Outside inventory; Callers
	/// <summary>
	/// Calls the Left click interact event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="viewer">The viewer that interacted.</param>
	/// <param name="itemstack">The itemstack in the inventory.</param>
	/// <param name="item">The item in the viewer's hand</param>
	public void callLeftClickInteractEvent(InventoryViewer viewer, ItemStack itemstack, Item item) {
		if (onLeftClickInteractEvent != null) {
			onLeftClickInteractEvent(viewer, itemstack, item);
		}
	}

	/// <summary>
	/// Calls the right click interact event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="viewer">The viewer that interacted.</param>
	/// <param name="itemstack">The itemstack in the inventory.</param>
	/// <param name="item">The item in the viewer's hand.</param>
	public void callRightClickInteractEvent(InventoryViewer viewer, ItemStack itemstack, Item item) {
		if (onRightClickInteractEvent != null) {
			onRightClickInteractEvent(viewer, itemstack, item);
		}
	}
	#endregion

	#region Inventory & Item related Event Callers
	/// <summary>
	/// Calls the item pickup event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="item">The item that was picked up.</param>
	/// <param name="itemName">The name of the item that was picked up.</param>
	public void callItemPickupEvent(Inventory inv, Item item, String itemName) {
		if (onItemPickUpEvent != null) {
			onItemPickUpEvent(inv, item, itemName);
			if (InventoryManager.getManager().debug) {
				print("Inv: " + inv.getName() + " Picked up item: " + itemName);
			}
		}
	}

	/// <summary>
	/// Calls the item drop event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="itemName">The name of the item that was dropped</param>
	public void callItemDropEvent(Inventory inv, string itemName) {
		if (onItemDropEvent != null) {
			onItemDropEvent(inv, itemName);
			if (InventoryManager.getManager().debug) {
				print("Inv: " + inv.getName() + " Dropped item: " + itemName);
			}
		}
	}
	#endregion

	#region Saver Event Callers
	/// <summary>
	/// Calls the inventory save event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory that was saved to file.</param>
	public void callInventorySaveEvent(Inventory inv) {
		if (onInventorySaveEvent != null) {
			onInventorySaveEvent(inv);
		}
	}

	/// <summary>
	/// Calls the inventory load event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="inv">The inventory that was loaded from file.</param>
	public void callInventoryLoadEvent(Inventory inv) {
		if (onInventoryLoadEvent != null) {
			onInventoryLoadEvent(inv);
		}
	}

	/// <summary>
	/// Calls the item save event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="allItems">A list of all item IDs that were saved to file</param>
	/// <param name="usedItems">A list of all used item item IDs that were saved to file</param>
	/// <param name="items">A list of all items that were saved to file.</param>
	public void callItemSaveEvent(List<Guid> allItems, List<Guid> usedItems, List<Item> items) {
		if (onItemSaveEvent != null) {
			onItemSaveEvent(allItems, usedItems, items);
		}
	}

	/// <summary>
	/// Cals the L item load event.
	/// **WARNING** Only use if you know exactly what you are doing!
	/// </summary>
	/// <param name="data">An ItemCacheSaveData class containining all relevant data.</param>
	public void calLItemLoadEvent(ItemCacheSaveData data) {
		if (onItemLoadEvent != null) {
			onItemLoadEvent(data);
		}
	}
	#endregion
}
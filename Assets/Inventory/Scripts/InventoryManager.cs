using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[AddComponentMenu("Inventory/Inventory Manager")]
// Analysis disable once ConvertToStaticType
/// <summary>
/// Inventory manager.
/// A singleton class that handles and stores data about every inventory.
/// This class manages inventories loaded ingame, relevant data, settings, and more.
/// </summary>
public class InventoryManager : MonoBehaviour {
	static InventoryManager instance;

	public bool debug;

	public Image defaultInvBG;
	public Image defaultHotbarBg;
	public Image hbSelection;
	public Image defaultSlot;
	public Text defaultItemCount;
	public Text defaultDisplayText;
	public Text defaultHotbarItemInfo;
	public Text defaultInvNameText;
	public GameObject holderPrefab;
	public Canvas invCanvas;
	public GameObject invHolder;

	public bool manualPickup;
	public bool autoPickup = true;
	public bool highlightItemToPickup;

	public InventorySaverSettings saverSettings;
	public InventoryControlSettings controls;

	public bool useSounds = true;
	public bool createNewItemsFromPrefab;


	static readonly SortedDictionary<string, Inventory> inventories = new SortedDictionary<string, Inventory>();
	static Transform thisTransStatic;

	#region Instnance handling
	InventoryManager() {} //Prevent initialization.

	/// <summary>
	/// Gets the instance of the InventoryManager.
	/// </summary>
	/// <returns>The instance of the InventoryManager.</returns>
	public static InventoryManager getManager() {
		if (instance == null) {
			instance = InventoryManager.thisTransStatic.gameObject.GetComponent<InventoryManager>();
		}
		return instance;
	}
	#endregion

	#region Unity methods
	void Awake() {
		thisTransStatic = gameObject.transform;
	}

	void Start() {
		InventoryEvents.getEvents().onInventoryCloseEvent += SaveOnInvClose;
		InventoryEvents.getEvents().onItemPickUpEvent += SaveOnItemPickup;
		InventoryEvents.getEvents().onItemDropEvent += SaveOnItemDrop;
	}
	#endregion

	#region Bools & checkers
	/// <summary>
	/// Checks if the specified inventorty exists.
	/// </summary>
	/// <returns><c>true</c>, if the inventory exists, <c>false</c> otherwise.</returns>
	/// <param name="name">Name.</param>
	public bool inventoryExists(string name) {
		return inventories.ContainsKey(name);
	}
	#endregion

	#region Functionals
	/// <summary>
	/// Creates a new inventory and will load saved data if any exists for it.
	/// </summary>
	/// <returns>The inventory that was created.</returns>
	/// <param name="name">The name of the inventory to create.</param>
	/// <param name="size">The size of the inventory (must be a multiple of 8).</param>
	public Inventory createInventory(string name, int size) {
		if (!inventories.ContainsKey(name)) {
			Inventory newInv = new Inventory(name, size);
			inventories.Add(name, newInv);
			if (saverSettings.useSaver) {
				InventorySaver.getSaver().LoadInventory(newInv);
			}
			if (debug) {
				print("Inventory Created. Name: " + name + ". Size: " + size + " item slots.");
			}
			InventoryEvents.getEvents().callInventoryCreateEvent(newInv);
			return newInv;
		} else {
			if (debug) {
				print("Failed to create inventory, one already exists with that name!");
			}
			return null;
		}
	}

	/// <summary>
	/// Creates a new inventory and will load saved data if any exists for it.
	/// </summary>
	/// <returns>The inventory that was created.</returns>
	/// <param name="name">The name of the inventory to create.</param>
	/// <param name="size">The size of the inventory (must be a mutliple of 8).</param>
	/// <param name="owner">The owner of the inventory.</param>
	public Inventory createInventory(string name, int size, InventoryViewer owner) {
		if (!inventories.ContainsKey(name)) {
			Inventory newInv = new Inventory(name, size, owner);
			inventories.Add(name, newInv);
			if (saverSettings.useSaver) {
				InventorySaver.getSaver().LoadInventory(newInv);
			}
			if (debug) {
				print("Inventory Created. Name: " + name + ". Size: " + size + " item slots. Owner: " + owner.name);
			}
			InventoryEvents.getEvents().callInventoryCreateEvent(newInv);
			return newInv;
		} else {
			if (debug) {
				print("Failed to create inventory, one already exists with that name!");
			}
			return null;
		}
	}

	/// <summary>
	/// Removes and deletes the inventory from memory.
	/// **WARNING** this DOES NOT save the inventories data!!
	/// </summary>
	/// <param name="name">The name of the inventory to remove.</param>
	public void removeInventory(string name) {
		if (inventoryExists(name)) {
			Inventory inv = getInventory(name);
			InventoryEvents.getEvents().callInventoryRemoveEvent(inv);

			for (int i = 0; i < inv.getSize(); i++) {
				ItemStack item = inv.getItem(i);
				if (item.getItemImage() != null) {
					Destroy(item.getItemImage());
				}
				if (item.getItemCountText() != null) {
					Destroy(item.getItemCountText());
				}
			}
			if (inv.getInventoryBackground() != null) {
				Destroy(inv.getInventoryBackground());
			}

			if (inv.hasHotbar() && inv.getHotbar() != null) {
				for (int i = 0; i < 7; i++) {
					if (inv.getHotbar().getHotbarSlot(i) != null) {
						if (inv.getHotbar().getHotbarSlot(i).getItemImage() != null) {
							Destroy(inv.getHotbar().getHotbarSlot(i).getItemImage());
						}
						if (inv.getHotbar().getHotbarSlot(i).getItemCountText() != null) {
							Destroy(inv.getHotbar().getHotbarSlot(i).getItemCountText());
						}
					}
				}
				if (inv.getHotbar().getHotbarBg() != null) {
					Destroy(inv.getHotbar().getHotbarBg());
				}
			}
			inventories.Remove(name);
		}
	}

	/// <summary>
	/// Gets the specified inventory.
	/// **TIP** Check if the inventory exits or this will return null.
	/// </summary>
	/// <returns>The inventory requested.</returns>
	/// <param name="name">The name of the inventory to get.</param>
	public Inventory getInventory(string name) {
		if (inventoryExists(name)) {
			Inventory inv;
			inventories.TryGetValue(name, out inv);
			return inv;
		}
		return null;
	}

	/// <summary>
	/// Gets a list of all inventories that exist.
	/// **WARNING** Use with care, this gets ALL loaded inventories.
	/// </summary>
	/// <returns>All loaded inventories.</returns>
	public List<Inventory> getAllInventories() {
		List<Inventory> invs = new List<Inventory>();

		foreach (Inventory inv in inventories.Values) {
			invs.Add(inv);	
		}
		return invs;
	}
	#endregion

	#region Debugging
	/// <summary>
	/// Draws the inventories Unity world space regions if debugging is on.
	/// </summary>
	void OnDrawGizmos() {
		if (debug) {
			//Gizmos.DrawWireCube(transform.position, new Vector3(inventorySize.x, inventorySize.y, 0));
		}
	}
	#endregion

	#region Event listeners
	/// <summary>
	/// Saves the inventory on close if enabled.
	/// Automatically called on OnInventoryCloseEvent event.
	/// </summary>
	/// <param name="inv">The inventory that was closed.</param>
	void SaveOnInvClose(Inventory inv) {
		if (saverSettings.useSaver) {
			if (saverSettings.saveOnInvClose) {
				InventorySaver.getSaver().SaveInventory(inv);
			}
		}
	}

	/// <summary>
	/// Saves the inventory when picking up an item if enabled.
	/// Automatically called on OnItemPickupEvent event.
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="item">The item that was picked up.</param>
	/// <param name="itemName">The name of the item that was picked up</param>
	void SaveOnItemPickup(Inventory inv, Item item, string itemName) {
		if (saverSettings.useSaver && saverSettings.saveOnItemPickup) {
			InventorySaver.getSaver().SaveInventory(inv);
		}
	}

	/// <summary>
	/// Saves the inventory when dropping an item if enabled.
	/// Automatically called OnItemDropEvent event.
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="itemName">The name of the item that was dropped.</param>
	void SaveOnItemDrop(Inventory inv, string itemName) {
		if (saverSettings.useSaver && saverSettings.saveOnItemDrop) {
			InventorySaver.getSaver().SaveInventory(inv);
		}
	}
	#endregion
}

[Serializable]
/// <summary>
/// Inventory saver settings.
/// A struct that holds the settings for the Inventory Saver to save room in the Unity Editor.
/// </summary>
public struct InventorySaverSettings {
	public bool useSaver;
	public bool saveOnInvClose;
	public bool saveOnItemPickup;
	public bool saveOnItemDrop;
	public bool saveOnItemMove;
}

[Serializable]
/// <summary>
/// Inventory control settings.
/// </summary>
/// A struct that holds the settings for controlling the inventory to save room in the Unity Editory.
public struct InventoryControlSettings {
	public KeyCode inventoryOpenClose;
	public KeyCode itemPickup;
	public KeyCode itemDrop;
	public KeyCode modifierOne;
	public bool invertHotbarScroll;
}
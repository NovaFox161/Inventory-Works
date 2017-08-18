using UnityEngine;
using System.Collections.Generic;
using System;
using System.Configuration;

[AddComponentMenu("Inventory/Item Manager")]
/// <summary>
/// Item manager.
/// A singleton class that handles item related functions and data.
/// </summary>
public class ItemManager : MonoBehaviour {
	static ItemManager instance;
	static Transform thisTransStatic;

	public List<ItemPrefab> itemPrefabs;

	readonly SortedDictionary<Guid, Item> allItems = new SortedDictionary<Guid, Item>();
	readonly SortedDictionary<Guid, Item> usedItems = new SortedDictionary<Guid, Item>();

	#region Instance handling
	ItemManager() {} //Prevent initialization

	/// <summary>
	/// Gets the ItemManager instance.
	/// </summary>
	/// <returns>The ItemManager instance.</returns>
	public static ItemManager getItemManager() {
		if (instance == null) {
			instance = ItemManager.thisTransStatic.gameObject.GetComponent<ItemManager>();
		}
		return instance;
	}
	#endregion

	#region Unity methods
	void Awake() {
		thisTransStatic = gameObject.transform;
	}

	void Start() {
		foreach (Item item in GameObject.FindObjectsOfType<Item>()) {
			allItems.Add(item.getUniqueId(), item);
		}
		if (InventoryManager.getManager().saverSettings.useSaver) {
			ItemCacheSaveData itemCache = InventorySaver.getSaver().loadItemCache();
			if (itemCache != null) {
				foreach (Guid id in itemCache.usedItems) {
					if (!usedItems.ContainsKey(id)) {
						Item item;
						allItems.TryGetValue(id, out item);
						if (item != null) {
							usedItems.Add(id, item);
							item.gameObject.SetActive(false);
						} else {
							if (InventoryManager.getManager().createNewItemsFromPrefab) {
								//Item saved but does not exist, create clone here if enabled.
								Item itemF = cloneItem((string)itemCache.itemNames[id], id);
								if (itemF != null) {
									usedItems.Add(id, itemF);
									itemF.gameObject.SetActive(false);
								}
							}
						}
					}
				}
				foreach (Item item in allItems.Values) {
					try {
						float xP = (float)itemCache.xPositions[item.getUniqueId()];
						float yP = (float)itemCache.yPositions[item.getUniqueId()];
						float zP = (float)itemCache.zPositions[item.getUniqueId()];

						float xR = (float)itemCache.xRotations[item.getUniqueId()];
						float yR = (float)itemCache.yRotations[item.getUniqueId()];
						float zR = (float)itemCache.zRotations[item.getUniqueId()];

						Vector3 pos = new Vector3(xP, yP, zP);

						item.transform.position = pos;
						item.transform.rotation.eulerAngles.Set(xR, yR, zR);

						if (itemCache.itemMetas.ContainsKey(item.getUniqueId())) {
							item.setItemMeta((ItemMeta)itemCache.itemMetas[item.getUniqueId()]);
						} else {
							item.clearItemMeta();
						}
					} catch (NullReferenceException e) {
						e.ToString();
						continue;
					}
				}
			}
		}
	}
	#endregion

	#region Public functionals
	/// <summary>
	/// Picks up the specified item if possible.
	/// </summary>
	/// <returns><c>true</c>, the item was picked up, <c>false</c> otherwise.</returns>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="item">The item to be picked up.</param>
	public bool pickUpItem(Inventory inv, Item item) {
		bool res;
		res = inv.addItem(new ItemStack(item));
		if (res) {
			String itemName = item.getName();
			usedItems.Add(item.getUniqueId(), item);
			item.gameObject.SetActive(false);
			InventoryEvents.getEvents().callItemPickupEvent(inv, item, itemName);
			if (InventoryManager.getManager().debug) {
				print("Item: " + item.getName() + " picked up!");
			}
			if (InventoryManager.getManager().saverSettings.saveOnItemPickup && InventoryManager.getManager().saverSettings.useSaver) {
				saveItemsToFile();
			}
		}
		return res;
	}

	/// <summary>
	/// Drops the specified item if possible
	/// </summary>
	/// <returns><c>true</c>, if the item was dropped <c>false</c> otherwise.</returns>
	/// <param name="itemName">The name of the item to be dropped</param>
	/// <param name="dropPosition">The Unity World Space position to drop the item at</param>
	public bool dropItem(String itemName, Vector3 dropPosition) {
		Item item = getItem(itemName);
		if (item != null) {
			item.transform.position = dropPosition;
			item.transform.rotation = Quaternion.identity;
			item.gameObject.SetActive(true);
			item.itemMeta.addMeta = false;
			usedItems.Remove(item.getUniqueId());

			if (InventoryManager.getManager().debug) {
				print("Dropped Item: " + item.getName());
			}
			if (InventoryManager.getManager().saverSettings.saveOnItemDrop) {
				saveItemsToFile();
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Drops the specified item if possible and adds the correct itemMeta.
	/// </summary>
	/// <returns><c>true</c>, if the item was dropped, <c>false</c> otherwise.</returns>
	/// <param name="itemName">The name of the item to be dropped</param>
	/// <param name="dropPosition">The Unity World space position to drop the item at</param>
	/// <param name="meta">The ItemMeta to add to the dropped item</param>
	public bool dropItem(String itemName, Vector3 dropPosition, ItemMeta meta) {
		Item item = getItem(itemName);
		if (item != null) {
			item.transform.position = dropPosition;
			item.transform.rotation = Quaternion.identity;
			item.gameObject.SetActive(true);
			item.setItemMeta(meta);
			usedItems.Remove(item.getUniqueId());

			if (InventoryManager.getManager().debug) {
				print("Dropped Item: " + item.getName());
			}
			if (InventoryManager.getManager().saverSettings.saveOnItemDrop) {
				saveItemsToFile();
			}
			return true;
		}
		return false;
	}

	/// <summary>
	/// Drops all items within the specified item stack.
	/// **TIP** This handles ItemMeta internally.
	/// **WARNING** This will drop ALL items in the stack.
	/// </summary>
	/// <returns><c>true</c>, if the enire stack was dropped, <c>false</c> if only some or none of the items were dropped.</returns>
	/// <param name="stack">The itemstack to drop.</param>
	/// <param name="dropPosition">The Unity World space position to drop the items at.</param>
	public bool dropEntireStack(ItemStack stack, Vector3 dropPosition) {
		String itemName = stack.getName();
		while (stack.getAmount() > 0) {
			if (stack.hasItemMeta()) {
				if (dropItem(itemName, dropPosition, stack.getItemMeta())) {
					stack.setAmount(stack.getAmount() - 1);
				} else {
					break;
				}
			} else {
				if (dropItem(itemName, dropPosition)) {
					stack.setAmount(stack.getAmount() - 1);
				} else {
					break;
				}
			}
		}

		if (stack.getAmount() <= 0) {
			stack.clear();
			return true;
		}
		return false;
	}

	/// <summary>
	/// Clones the specified item and sets it to the defualt Unity transform values.
	/// </summary>
	/// <returns>The clone of the item</returns>
	/// <param name="itemstack">The itemstack containing the item to clone.</param>
	public Item cloneItem(ItemStack itemstack) {
		if (!itemstack.getName().Equals("Air")) {
			Item obj = GameObject.Instantiate(getUnusedItem(itemstack.getName()));
			obj.resetUniqueId();
			obj.gameObject.SetActive(false);
			obj.transform.position = Vector3.zero;
			obj.transform.rotation = Quaternion.identity;
			obj.name = itemstack.getName() + "; Clone";
			return obj;
		}
		return null;
	}

	/// <summary>
	/// Clones the specified item and sets it to the default Unity transform values.
	/// </summary>
	/// <returns>A clone of the item.</returns>
	/// <param name="itemName">The name of the item to clone.</param>
	/// <param name="itemId">The id to assign to the item.</param>
	public Item cloneItem(String itemName, Guid itemId) {
		if (!itemName.Equals("Air")) {
			Item obj = GameObject.Instantiate(getUnusedItem(itemName));
			obj.uniqueId = itemId.ToString();
			obj.gameObject.SetActive(false);
			obj.transform.position = Vector3.zero;
			obj.transform.rotation = Quaternion.identity;
			obj.name = itemName + "; Clone";
			return obj;
		}
		return null;
	}

	/// <summary>
	/// Generates the needed data and has the InventorySaver save the item data.
	/// </summary>
	public void saveItemsToFile() {
		if (InventoryManager.getManager().saverSettings.useSaver) {
			List<Guid> allItemsList = new List<Guid>();
			List<Guid> usedItemsList = new List<Guid>();
			List<Item> items = new List<Item>();

			foreach (Guid id in allItems.Keys) {
				allItemsList.Add(id);
				if (usedItems.ContainsKey(id)) {
					usedItemsList.Add(id);
				}
			}
			foreach (Item item in allItems.Values) {
				items.Add(item);
			}

			InventorySaver.getSaver().SaveItemCache(allItemsList, usedItemsList, items);
		}
	}
	#endregion

	#region Public Getters
	/// <summary>
	/// Gets the specified item from its name.
	/// </summary>
	/// <returns>The specified item.</returns>
	/// <param name="itemName">The name of the item to get</param>
	public Item getItem(string itemName) {
		foreach (Item item in usedItems.Values) {
			if (item.getName().Equals(itemName)) {
				return item;
			}
		}
		return null;
	}

	/// <summary>
	/// Gets the specified item from its Unique Id.
	/// </summary>
	/// <returns>The specified item.</returns>
	/// <param name="id">The Unique Id of the item to get, <c>null</c> if it does not exist</param>
	public Item getItem(Guid id) {
		if (allItems.ContainsKey(id)) {
			Item item;
			allItems.TryGetValue(id, out item);
			return item;
		}
		return null;
	}

	/// <summary>
	/// Gets the specified item if it is not disabled or in use.
	/// If enabled and provided, this will create a new item from a provided prefab.
	/// </summary>
	/// <returns>The specified item, <c>null</c> if it does not exist or is in use.</returns>
	/// <param name="itemName">The name of the item to get.</param>
	public Item getUnusedItem(string itemName) {
		foreach (Item item in allItems.Values) {
			if (item.getName().Equals(itemName)) {
				return item;
			}
		}
		if (InventoryManager.getManager().createNewItemsFromPrefab) {
			foreach (ItemPrefab ip in itemPrefabs) {
				if (ip.itemName.Equals(itemName)) {
					Item item = GameObject.Instantiate(ip.item);
					item.resetUniqueId();
					item.itemMeta.addMeta = false;
					allItems.Add(item.getUniqueId(), item);
					return item;
				}
			}
		}
		return null;
	}
	#endregion
}

/// <summary>
/// Item Prefab.
/// This struct is used to contain data for an item in the case one wants to generate new instances of the item.
/// </summary>
[Serializable]
public struct ItemPrefab {
	public string itemName;
	public Item item;
}
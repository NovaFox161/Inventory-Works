using UnityEngine;
using System;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("Inventory/Inventory Saver")]
// Analysis disable once ConvertToStaticType
/// <summary>
/// Inventory saver.
/// A specialized singleton class that will save inventory and item data to file.
/// </summary>
public class InventorySaver : MonoBehaviour {
	static InventorySaver instance;
	static Transform thisTransStatic;

	#region Unity methods
	void Awake() {
		thisTransStatic = gameObject.transform;

		createInventorySaveFolder();
		createItemCacheFolder();
	}
	#endregion

	#region Instance handling
	InventorySaver() {} //Prevent initialization.

	/// <summary>
	/// Gets the instance of the InventorySaver.
	/// </summary>
	/// <returns>The instance of the InventorySaver.</returns>
	public static InventorySaver getSaver() {
		if (instance == null) {
			instance = InventorySaver.thisTransStatic.gameObject.GetComponent<InventorySaver>();
		}
		return instance;
	}
	#endregion
		
	#region Inventory
	/// <summary>
	/// Saves the specified inventory to file.
	/// **WARNING** Use with care, as this will overwrite any previously saved data.
	/// </summary>
	/// <param name="inv">The inventory to save.</param>
	public void SaveInventory(Inventory inv) {
		if (InventoryManager.getManager().debug) {
			print("Attepting to save inventory '" + inv.getName() + "' to file.");
		}
		deleteInventoryFile(inv);

		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/inventories/" + inv.getName() + ".inv");

		InventorySaveData invData = new InventorySaveData();

		invData.name = inv.getName();
		invData.type = inv.getType();
		invData.size = inv.getSize();
		invData.maxStackSize = inv.getMaxStackSize();

		for (int i = 0; i < inv.getSize(); i ++) {
			ItemStack stack = inv.getItem(i);
			ItemStackSaveData stackData = new ItemStackSaveData();

			stackData.index = stack.getIndex();
			stackData.amount = stack.getAmount();

			stackData.itemName = stack.getName();
			stackData.itemMeta = stack.getItemMeta();

			invData.itemStackSaveDatas.Add(i, stackData);
		}

		bf.Serialize(file, invData);
		file.Close();

		if (InventoryManager.getManager().debug) {
			print("Saved inventory: " + inv.getName() + " to file.");
		}
		InventoryEvents.getEvents().callInventorySaveEvent(inv);
	}

	/// <summary>
	/// Loas the specifed inventory from file if data was previously saved.
	/// **WARNING** Use with care, this will overwrite the inventory's current contents.
	/// **TIP** This WILL NOT delete the save data of the inventory.
	/// </summary>
	/// <returns><c>true</c>, if the inventory was successfully loaded, <c>false</c> otherwise.</returns>
	/// <param name="inv">The inventory to load from file.</param>
	public bool LoadInventory(Inventory inv) {
		if (InventoryManager.getManager().debug) {
			print("Attepting to load inventory: " + inv.getName() + " from file.");
		}
		if (File.Exists(Application.persistentDataPath + "/inventories/" + inv.getName() + ".inv")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/inventories/" + inv.getName() + ".inv", FileMode.Open);

			if (file.Length > 0) {
				InventorySaveData inventoryData = (InventorySaveData) bf.Deserialize(file);
				file.Close();

				//Clear inventory so that duplication does not occur.
				inv.clear();

				//Set data
				inv.setMaxStackSize(inventoryData.maxStackSize);
				inv.setType(inventoryData.type);

				for (int i = 0; i < inv.getSize(); i++) {
					if (inventoryData.itemStackSaveDatas.ContainsKey(i)) {
						ItemStackSaveData stackData = (ItemStackSaveData) inventoryData.itemStackSaveDatas[i];
						ItemStack stack = inv.getItem(i);


						stack.setItem(stackData.itemName, stackData.amount);
						stack.setItemMeta(stackData.itemMeta);
						stack.setItemSprite(InventoryUI.getUI().getImageForItem(stackData.itemName));
						inv.setItem(i, stack);
					} else {
						if (InventoryManager.getManager().debug) {
							print("Did not load data for slot '" + i + "' because data was never saved for it!");
						}
					}
				}

				if (InventoryManager.getManager().debug) {
					print("Successfully loaded inventory '" + inv.getName() + "' from file!");
				}
				inv.UpdateInventory();
				InventoryEvents.getEvents().callInventoryLoadEvent(inv);
				return true;
			}
		}
		if (InventoryManager.getManager().debug) {
			print("Failed to load inventory as it's file may not exist or is corrupted!");
		}
		return false;
	}
	#endregion

	#region Items
	/// <summary>
	/// Saves the specified item data to file.
	/// **WARNING** This will overwrite any previously saved data.
	/// **TIP** This saves each scene seperately.
	/// **TIP** This will save item positions and rotations.
	/// </summary>
	/// <param name="allItems">A list of all item IDs in the scene</param>
	/// <param name="usedItems">A list of all used items' IDs in the scene.</param>
	/// <param name="items">A list of all items in the scene.</param>
	public void SaveItemCache(List<Guid> allItems, List<Guid> usedItems, List<Item> items) {
		if (InventoryManager.getManager().debug) {
			print("Attempting to save item cache...");
		}

		deleteItemCacheFile();

		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/cache/items/" + SceneManager.GetActiveScene().name + ".iCache");

		ItemCacheSaveData itemCache = new ItemCacheSaveData();

		foreach (Item item in items) {
			itemCache.xPositions.Add(item.getUniqueId(), item.transform.position.x);
			itemCache.yPositions.Add(item.getUniqueId(), item.transform.position.y);
			itemCache.zPositions.Add(item.getUniqueId(), item.transform.position.z);

			itemCache.xRotations.Add(item.getUniqueId(), item.transform.rotation.eulerAngles.x);
			itemCache.yRotations.Add(item.getUniqueId(), item.transform.rotation.eulerAngles.y);
			itemCache.zRotations.Add(item.getUniqueId(), item.transform.rotation.eulerAngles.z);

			if (item.hasItemMeta()) {
				itemCache.itemMetas.Add(item.getUniqueId(), item.getItemMeta());
			}
			itemCache.itemNames.Add(item.getUniqueId(), item.getName());
		}

		foreach (Guid id in allItems) {
			itemCache.allItems.Add(id);
		}
		foreach (Guid id in usedItems) {
			itemCache.usedItems.Add(id);
		}

		bf.Serialize(file, itemCache);
		file.Close();

		if (InventoryManager.getManager().debug) {
			print("Saved item cache to file!");
		}
		InventoryEvents.getEvents().callItemSaveEvent(allItems, usedItems, items);
	}

	/// <summary>
	/// Loads all item data from file if data was previously saved.
	/// **TIP** This will not delete the save data if any exists.
	/// **TIP** This will not affect items in the scene, that must be done within another method.
	/// </summary>
	/// <returns>An ItemCacheSaveData object containing all relevant data.</returns>
	public ItemCacheSaveData loadItemCache() {
		if (InventoryManager.getManager().debug) {
			print("Attempting to load item cache...");
		}
		if (File.Exists(Application.persistentDataPath + "/cache/items/" + SceneManager.GetActiveScene().name + ".iCache")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/cache/items/" + SceneManager.GetActiveScene().name + ".iCache", FileMode.Open);

			if (file.Length > 0) {
				ItemCacheSaveData cache = (ItemCacheSaveData) bf.Deserialize(file);
				file.Close();
				InventoryEvents.getEvents().calLItemLoadEvent(cache);
				return cache;
			}
		}
		if (InventoryManager.getManager().debug) {
			print("Failed to load item cache! One may not exist or is corrupted!");
		}
		return null;
	}
	#endregion

	#region Files
	/// <summary>
	/// Creates the inventory save folder.
	/// </summary>
	void createInventorySaveFolder() {
		if (!Directory.Exists(Application.persistentDataPath + "/inventories/")) {
			Directory.CreateDirectory(Application.persistentDataPath + "/inventories/");
		}
	}

	/// <summary>
	/// Creates the item save folder.
	/// </summary>
	void createItemCacheFolder() {
		if (!Directory.Exists(Application.persistentDataPath + "/cache/items/")) {
			Directory.CreateDirectory(Application.persistentDataPath + "/cache/items/");
		}
	}

	/// <summary>
	/// Deletes the specified inventory's save file as to prevent duplication issues or delete a specific inventory.
	/// </summary>
	/// <param name="inv">The inventory whose file is to be deleted.</param>
	void deleteInventoryFile(Inventory inv) {
		if (File.Exists(Application.persistentDataPath + "/inventories/" + inv.getName() + ".inv")) {
			File.Delete(Application.persistentDataPath + "/inventories/" + inv.getName() + ".inv");
		}
	}

	/// <summary>
	/// Deletes the item cache file for the current scene as to prevent duplication issues or delete the data completely.
	/// </summary>
	void deleteItemCacheFile() {
		if (File.Exists(Application.persistentDataPath + "/cache/items/" + SceneManager.GetActiveScene().name + ".iCache")) {
			File.Delete(Application.persistentDataPath + "/cache/items/" + SceneManager.GetActiveScene().name + ".iCache");
		}
	}

	#if UNITY_EDITOR
	/// <summary>
	/// Deletes all saved files and folders.
	/// </summary>
	[ContextMenu("Delete ALL Saved Data")]
	void deleteAllSaves() {
		if (Directory.Exists(Application.persistentDataPath + "/inventories/")) {
			FileUtil.DeleteFileOrDirectory(Application.persistentDataPath + "/inventories/");
			if (InventoryManager.getManager().debug) {
				print("Deleted all saved inventories!");
			}
		} else {
			if (InventoryManager.getManager().debug) {
				print("No inventories saved, nothing to delete!");
			}
		}
		if (Directory.Exists(Application.persistentDataPath + "/cache/items/")) {
			FileUtil.DeleteFileOrDirectory(Application.persistentDataPath + "/cache/items/");
			if (InventoryManager.getManager().debug) {
				print("Deleted all saved items!");
			}
		} else {
			if (InventoryManager.getManager().debug) {
				print("No items saved, nothing to delete!");
			}
		}
	}
	#endif
	#endregion
}

/// <summary>
/// InventorySaveData.
/// This is a class used to save data to file.
/// This contains all relevant and serializable inventory data.
/// </summary>
[Serializable]
class InventorySaveData {
	public String name;

	public InventoryType type;
	public int size;
	public int maxStackSize;

	public Hashtable itemStackSaveDatas = new Hashtable();
}

/// <summary>
/// ItemStackSaveData.
/// This is a class used to save data to file.
/// This contains all relevant and serializable itemstack data.
/// </summary>
[Serializable]
class ItemStackSaveData {
	public int index;

	public int amount;
	public string itemName;
	public ItemMeta itemMeta;
}

/// <summary>
/// ItemCacheSaveData.
/// This is a class used to save data to file.
/// This contains all relevant and serializable item data.
/// </summary>
[Serializable]
public class ItemCacheSaveData {
	public List<Guid> allItems = new List<Guid>();
	public List<Guid> usedItems = new List<Guid>();

	public Hashtable xPositions = new Hashtable();
	public Hashtable yPositions = new Hashtable();
	public Hashtable zPositions = new Hashtable();

	public Hashtable xRotations = new Hashtable();
	public Hashtable yRotations = new Hashtable();
	public Hashtable zRotations = new Hashtable();

	public Hashtable itemMetas = new Hashtable();
	public Hashtable itemNames = new Hashtable();
}
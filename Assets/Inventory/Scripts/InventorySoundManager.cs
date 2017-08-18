using UnityEngine;
using System;

[AddComponentMenu("Inventory/Inventory Sound Manager")]
/// <summary>
/// Inventory sound manager.
/// A singleton class that handles and issues all sound from inventories.
/// </summary>
public class InventorySoundManager : MonoBehaviour {
	static InventorySoundManager instance;
	static Transform thisTransStatic;

	[Range(0, 100)]
	public float volume = 100;

	public AudioClip inventoryOpen;
	public AudioClip inventoryClose;
	public AudioClip itemPickup;
	public AudioClip itemDrop;
	public AudioClip hotbarSelectionChange;

	#region Instance handling
	InventorySoundManager() {} //Prevent initialization.

	/// <summary>
	/// Gets the InventorySoundManager instance.
	/// </summary>
	/// <returns>The InventorySoundManager instance.</returns>
	public static InventorySoundManager getSoundManager() {
		if (instance == null) {
			instance = InventorySoundManager.thisTransStatic.gameObject.GetComponent<InventorySoundManager>();
		}
		return instance;
	}
	#endregion

	#region Unity methods
	void Awake() {
		thisTransStatic = gameObject.transform;
	}

	void Start() {
		InventoryEvents.getEvents().onInventoryOpenEvent += playSoundOnInventoryOpen;
		InventoryEvents.getEvents().onInventoryCloseEvent += playSoundOnInventoryClose;

		InventoryEvents.getEvents().onHotbarSelectionChange += playOnHotbarSelectionChange;

		InventoryEvents.getEvents().onItemPickUpEvent += playOnItemPickup;
		InventoryEvents.getEvents().onItemDropEvent += playOnItemDrop;
	}
	#endregion

	#region Sound players
	/// <summary>
	/// Plays the Inventory Open sound if one exists and is enabled.
	/// **WARNING** This is automatically called on the related event and should not be used elsewhere.
	/// </summary>
	/// <param name="inv">The inventory that was opened.</param>
	void playSoundOnInventoryOpen(Inventory inv) {
		if (InventoryManager.getManager().useSounds && inventoryOpen != null) {
			if (inv.hasOwner()) {
				InventoryViewer owner = inv.getOwner();
				owner.GetComponent<AudioSource>().PlayOneShot(inventoryOpen, volume / 100);
			}
		}
	}

	/// <summary>
	/// Plays the Inventory Close sound if one exists and is enabled.
	/// **WARNING** This is automatically called on the related event and should not be used elsewhere.
	/// </summary>
	/// <param name="inv">The inventory that was closed.</param>
	void playSoundOnInventoryClose(Inventory inv) {
		if (InventoryManager.getManager().useSounds && inventoryClose != null) {
			if (inv.hasOwner()) {
				InventoryViewer owner = inv.getOwner();
				owner.GetComponent<AudioSource>().PlayOneShot(inventoryClose, volume / 100);
			}
		}
	}

	/// <summary>
	/// Plays the Hotbar Selection Change sound if one exists and is enabled.
	/// **WARNING** This is automatically called on the related event and should not be used elsewhere.
	/// </summary>
	/// <param name="inv">The hotbar's parent inventory.</param>
	/// <param name="hotbarIndex">The index of the new selection.</param>
	void playOnHotbarSelectionChange(Inventory inv, int hotbarIndex) {
		if (InventoryManager.getManager().useSounds && hotbarSelectionChange != null) {
			if (inv.hasOwner()) {
				InventoryViewer owner = inv.getOwner();
				owner.GetComponent<AudioSource>().PlayOneShot(hotbarSelectionChange, volume / 100);
			}
		}
	}

	/// <summary>
	/// Plays the Item Pickup sound if one exists and is enabled.
	/// **WARNING** This is automatically called on the related event and should not be used elsewhere.
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="item">The item that was picked up.</param>
	/// <param name="itemName">The name of the item that was picked up.</param>
	void playOnItemPickup(Inventory inv, Item item, String itemName) {
		if (InventoryManager.getManager().useSounds && itemPickup != null) {
			if (inv.hasOwner()) {
				InventoryViewer owner = inv.getOwner();
				owner.GetComponent<AudioSource>().PlayOneShot(itemPickup, volume / 100);
			}
		}
	}

	/// <summary>
	/// Plays the Item Drop sound if one exists and is enabled.
	/// **WARNING** This is automatically called on the related event and should not be used eslewhere.
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="itemName">The name of the item that was dropped.</param>
	void playOnItemDrop(Inventory inv, string itemName) {
		if (InventoryManager.getManager().useSounds && itemPickup != null) {
			if (inv.hasOwner()) {
				InventoryViewer owner = inv.getOwner();
				owner.GetComponent<AudioSource>().PlayOneShot(itemDrop, volume / 100);
			}
		}
	}
	#endregion
}
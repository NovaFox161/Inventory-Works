using UnityEngine;
using UnityEngine.UI;

public class WinTextOnCollect : MonoBehaviour {
	public string inventoryToRead;
	public string itemNeeded;
	public int amountNeeded;
	public Text winText;
	public string winDisplay = "You win!!!!";

	void Start () {
		InventoryEvents.getEvents().onItemPickUpEvent += onItemPickupEvent;
		InventoryEvents.getEvents().onItemDropEvent += onItemDropEvent;
	}

	//Event listeners
	public void onItemPickupEvent(Inventory inv, Item item, string itemName) {
		if (inv.getName().Equals(inventoryToRead)) {
			if (inv.contains(itemNeeded, amountNeeded)) {
				winText.text = winDisplay;
			}
		}
	}

	public void onItemDropEvent(Inventory inv, string itemName) {
		if (inv.getName().Equals(inventoryToRead)) {
			if (!inv.contains(itemNeeded, amountNeeded)) {
				winText.text = "";
			}
		}
	}
}
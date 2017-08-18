using UnityEngine;

[AddComponentMenu("Inventory/Fun Options/Open Close Object")]
public class OpenCloseObj : MonoBehaviour {
	ObjState state;
	bool canOpen;

	public bool onKeyPress;
	public KeyCode key = KeyCode.Return;
	public string inventoryToRead;
	public string itemNeeded;
	public int amountNeeded;
	public GameObject targetObj;

	// Use this for initialization
	void Start () {
		canOpen = false;

		InventoryEvents.getEvents().onItemPickUpEvent += onItemPickupEvent;
		InventoryEvents.getEvents().onItemDropEvent += onItemDropEvent;
	}
	
	void Update() {
		if (canOpen && onKeyPress && Input.GetKeyDown(key)) {
			if (state.Equals(ObjState.Open)) {
				close();
			} else if (state.Equals(ObjState.Closed)) {
				open();
			}
		}
	}

	//Event listeners
	public void onItemPickupEvent(Inventory inv, Item item, string itemName) {
		if (inv.getName().Equals(inventoryToRead)) {
			if (inv.contains(itemNeeded, amountNeeded)) {
				canOpen = true;
				if (!(state.Equals(ObjState.Open) || state.Equals(ObjState.Opening))) {
					if (!onKeyPress || (onKeyPress && Input.GetKeyDown(key))) {
						open();
					}
				}
			}
		}
	}

	public void onItemDropEvent(Inventory inv, string itemName) {
		if (inv.getName().Equals(inventoryToRead)) {
			if (!inv.contains(itemNeeded, amountNeeded)) {
				canOpen = false;
				if (!(state.Equals(ObjState.Closed) || state.Equals(ObjState.Closing))) {
					if (!onKeyPress || (onKeyPress && Input.GetKeyDown(key))) {
						close();
					}
				}
			}
		}
	}

	//Animation controllers
	public void open() {
		targetObj.SetActive(false);
		state = ObjState.Open;
	}

	public void close() {
		targetObj.SetActive(true);
		state = ObjState.Closed;
	}
}

using UnityEngine;
using System.Collections;

[AddComponentMenu("Inventory/Fun Options/Open Close Object With Animation")]
public class OpenCloseObjAnimate : MonoBehaviour {

	ObjState state;
	bool canOpen;

	public bool onKeyPress;
	public KeyCode key = KeyCode.Return;
	public string openAnimName;
	public string closeAnimName;
	public float openAnimLength;
	public float closeAnimLength;
	public string inventoryToRead;
	public string itemNeeded;
	public int amountNeeded;

	Animator animator;

	//Unity methods
	void Start () {
		animator = GetComponent<Animator>();
		canOpen = false;

		InventoryEvents.getEvents().onItemPickUpEvent += onItemPickupEvent;
		InventoryEvents.getEvents().onItemDropEvent += onItemDropEvent;
	}

	void Update() {
		if (canOpen && onKeyPress && Input.GetKeyDown(key)) {
			if (state.Equals(ObjState.Open)) {
				StartCoroutine(close());
			} else if (state.Equals(ObjState.Closed)) {
				StartCoroutine(open());
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
						StartCoroutine(open());
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
						StartCoroutine(close());
					}
				}
			}
		}
	}

	//Animation controllers
	public IEnumerator open() {
		state = ObjState.Opening;
		animator.Play(openAnimName);
		yield return new WaitForSeconds(openAnimLength);
		state = ObjState.Open;
	}

	public IEnumerator close() {
		state = ObjState.Closing;
		animator.Play(closeAnimName);
		yield return new WaitForSeconds(closeAnimLength);
		state = ObjState.Closed;
	}
}

public enum ObjState {
	Closed, Closing, Open, Opening
}
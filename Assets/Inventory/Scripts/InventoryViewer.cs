using UnityEngine;

[AddComponentMenu("Inventory/Inventory Viewer")]
[RequireComponent(typeof(AudioSource))]
// Analysis disable once ConvertToStaticType
/// <summary>
/// Inventory viewer. A component to add to GameObject's and/or players that can view inventories.
/// This can also be extended to a player to let them own and use inventories.
/// </summary>
public class InventoryViewer : MonoBehaviour {
	public bool acceptInput = true;
	public bool canPickUpItems = true;
	public bool usePersonalInv;
	public string personalInvName;
	public int personalInvSize = 32;
	public bool showItemInHand;
	public bool rightHanded = true;
	public bool highlightPickup;
	public bool useCamForHolding = true;

	//This is their personal inventory. No other users can see this (unless made to).
	Inventory personalInv;

	//This is the inventory that they currently have open.
	Inventory openInv;

	//Other vars and stuffs
	Item itemHoldingClone;
	Camera viewerCam;

	#region Unity Methods
	void Start () {
		if (usePersonalInv) {
			personalInv = InventoryManager.getManager().createInventory(personalInvName, personalInvSize, this);
		}
		if (useCamForHolding) {
			Camera cO = GetComponent<Camera>();
			Camera c = GetComponentInChildren<Camera>();
			if (cO != null) {
				viewerCam = cO;
			} else if (c != null) {
				viewerCam = c;
			}
		}
	}

	void Update () {
		if (acceptInput) {
			if (Input.GetKeyDown(InventoryManager.getManager().controls.inventoryOpenClose)) {
				if (hasOpenInventory()) {
					CloseInventory();
				} else {
					if (InventoryDetector.getDetector().isLookingAtOpenableInv(this)) {
						//Get inv they are looking at.
					} else {
						OpenPersonalInventory();
					}
				}
			}
			if (Input.GetKeyDown(InventoryManager.getManager().controls.itemPickup)) {
				if (canPickUpItems && InventoryManager.getManager().manualPickup) {
					if (!hasOpenInventory()) {
						PickUpData pickUpData;

						if (hasCamera() && useCamForHolding) {
							pickUpData = InventoryDetector.getDetector().isLookingAtItem(this, getCamera().gameObject, highlightPickup);
						} else {
							pickUpData = InventoryDetector.getDetector().isLookingAtItem(this, highlightPickup);
						}

						if (pickUpData.canBePickedUp) {
							if (ItemManager.getItemManager().pickUpItem(personalInv, pickUpData.item)) {
								if (InventoryManager.getManager().debug) {
									print("Successful manaual pickup!");
								}
								//Display text that item was picked up.
							}
						}
					}
				}
			}
			if (Input.GetKeyDown(InventoryManager.getManager().controls.itemDrop) && !Input.GetKey(InventoryManager.getManager().controls.modifierOne)) {
				//Drop single item if in hotbar.
				if (usePersonalInv && personalInv.hasHotbar() && personalInv.getHotbar().isDisplayed()) {
					int hotbarIndex = InventoryDetector.getDetector().getData(personalInv).hotbarSelectionNumber;
					if (!personalInv.getItem(hotbarIndex).getName().Equals("Air")) {
						InventoryDetector.getDetector().dropItem(personalInv, hotbarIndex);
					}
					
				}
			}
			if (Input.GetKeyDown(InventoryManager.getManager().controls.itemDrop) && Input.GetKey(InventoryManager.getManager().controls.modifierOne)) {
				//Drop entire stack if in hotbar.
				if (usePersonalInv && personalInv.hasHotbar() && personalInv.getHotbar().isDisplayed()) {
					int hotbarIndex = InventoryDetector.getDetector().getData(personalInv).hotbarSelectionNumber;
					if (!personalInv.getItem(hotbarIndex).getName().Equals("Air")) {
						InventoryDetector.getDetector().dropEntireStack(personalInv, hotbarIndex);
					}
				}
			}
			if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0) {
				if (hasPersonalInventory() && personalInv.hasHotbar() && personalInv.getHotbar().isDisplayed()) {
					InventoryDetector.getDetector().changeHotbarSelection(this);
				}
			}
			if (Input.GetKeyDown(KeyCode.Mouse0)) {
				if (usePersonalInv && !personalInv.isOpen() && isHoldingItem()) {
					int hbSelection = InventoryDetector.getDetector().getData(personalInv).hotbarSelectionNumber;
					ItemStack itemStack = personalInv.getItem(hbSelection);
					InventoryEvents.getEvents().callLeftClickInteractEvent(this, itemStack, getItemHolding());
				}
			}
			if (Input.GetKeyDown(KeyCode.Mouse1)) {
				if (usePersonalInv && !personalInv.isOpen() && isHoldingItem()) {
					int hbSelection = InventoryDetector.getDetector().getData(personalInv).hotbarSelectionNumber;
					ItemStack itemstack = personalInv.getItem(hbSelection);
					InventoryEvents.getEvents().callRightClickInteractEvent(this, itemstack, getItemHolding());
				}
			}
		}
		if (canPickUpItems && InventoryManager.getManager().autoPickup) {
			PickUpData pickUpData = InventoryDetector.getDetector().isInRangeOfItem(this);
			if (pickUpData.canBePickedUp) {
				if (ItemManager.getItemManager().pickUpItem(personalInv, pickUpData.item)) {
					if (InventoryManager.getManager().debug) {
						print("Successful automatic pickup!");
					}
					//Display text that item was picked up.
				}
			}
		}

		if (hasOpenInventory()) {
			InventoryDetector.getDetector().checkStackThing(openInv);
		}
	}

	void FixedUpdate() {
		if (acceptInput) {
			if (!hasOpenInventory()) {
				freezeCursor();
			}

			if (InventoryManager.getManager().manualPickup && highlightPickup) {
				if (hasCamera() && useCamForHolding) {
					InventoryDetector.getDetector().isLookingAtItem(this, getCamera().gameObject, true);
				} else {
					InventoryDetector.getDetector().isLookingAtItem(this, true);
				}
			}
		}
	}
	#endregion

	#region Private functionals
	/// <summary>
	/// Freezes the mouse cursor when called.
	/// </summary>
	///
	void freezeCursor() {
		if (acceptInput) {
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	/// <summary>
	/// Unfreezes the mouse cursor when called.
	/// </summary>
	void unfreezeCursor() {
		if (acceptInput) {
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
	}
	#endregion

	#region Public functionals
	/// <summary>
	/// Opens the personal inventory of this viewer and displays it to them.
	/// </summary>
	public void OpenPersonalInventory() {
		if (!hasOpenInventory() && hasPersonalInventory()) {
			personalInv.Open(this);
			openInv = personalInv;
			unfreezeCursor();
		}
	}

	/// <summary>
	/// Opens the specified inventory and displays it to the viewer.
	/// </summary>
	/// <param name="inv">The inventory to open and display.</param>
	public void OpenInventory(Inventory inv) {
		if (!hasOpenInventory()) {
			inv.Open(this);
			openInv = personalInv;
			unfreezeCursor();
		}
	}

	/// <summary>
	/// Closes the currently opened inventory and hides it from the viewer.
	/// </summary>
	public void CloseInventory() {
		if (hasOpenInventory()) {
			openInv.Close();
			openInv = null;
			freezeCursor();
		}
	}

	/// <summary>
	/// Picks up an item if possible.
	/// </summary>
	/// <returns><c>true</c>, if the item was picked up, <c>false</c> otherwise.</returns>
	/// <param name="pickUpdata">The pickup data containing the item and other data.</param>
	public bool pickupItem(PickUpData pickUpdata) {
		return hasPersonalInventory() && ItemManager.getItemManager().pickUpItem(personalInv, pickUpdata.item);
	}
	#endregion

	#region Bools & Checkers
	/// <summary>
	/// Checks if the viewer has a personal inventory.
	/// </summary>
	/// <returns><c>true</c>, if the viewer has a personal inventory, <c>false</c> otherwise.</returns>
	public bool hasPersonalInventory() {
		return personalInv != null;
	}

	/// <summary>
	/// Checks if the viewer has an open inventory displayed.
	/// </summary>
	/// <returns><c>true</c>, if the viewer has an open inventory displayed, <c>false</c> otherwise.</returns>
	public bool hasOpenInventory() {
		return openInv != null;
	}

	/// <summary>
	/// Checks if the viewer is currently holding an item in their hand.
	/// </summary>
	/// <returns><c>true</c>, if currently holding an item in their hand, <c>false</c> otherwise.</returns>
	public bool isHoldingItem() {
		return itemHoldingClone != null;
	}

	/// <summary>
	/// Checks if the viewer has a camera attached.
	/// </summary>
	/// <returns><c>true</c>, if the viewer has a camera, <c>false</c> otherwise.</returns>
	public bool hasCamera() {
		return viewerCam != null;
	}
	#endregion

	#region Getters
	/// <summary>
	/// Gets the viewer's personal inventory.
	/// </summary>
	/// <returns>The viewer's personal inventory.</returns>
	public Inventory getPersonalInventory() {
		return personalInv;
	}

	/// <summary>
	/// Gets the viewer's open inventory.
	/// </summary>
	/// <returns>The viewer's open inventory, <code>null</code> if one does not exist.</returns>
	public Inventory getOpenInventory() {
		return openInv;
	}

	/// <summary>
	/// Gets the item the viewer is holding.
	/// </summary>
	/// <returns>The item the viewer is holding, <c>null</c> if one does not exist.</returns>
	public Item getItemHolding() {
		return itemHoldingClone;
	}

	/// <summary>
	/// Gets the viewer's camera.
	/// </summary>
	/// <returns>The viewer's camera, <c>null</c> if one does not exist.</returns>
	public Camera getCamera() {
		return viewerCam;
	}
	#endregion

	#region Setters
	/// <summary>
	/// Sets the item the viewer is holding.
	/// This also destroys the currently held item if one exists.
	/// </summary>
	/// <param name="_item">The item to hold.</param>
	public void setItemHolding(Item _item) {
		if (itemHoldingClone != null) {
			GameObject.Destroy(itemHoldingClone.gameObject);
		}
		itemHoldingClone = _item;
	}
	#endregion
}
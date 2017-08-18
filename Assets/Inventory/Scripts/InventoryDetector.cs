using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Configuration;

[AddComponentMenu("Inventory/Inventory Detector")]
// Analysis disable once ConvertToStaticType
/// <summary>
/// Inventory detector. This is a singleton object which handles all of the methods for detecting inventory things.
/// This will function both automatically (and call events) or manually.
/// Use methods with care as some can be CPU intensive.
/// </summary>
public class InventoryDetector : MonoBehaviour {
	static InventoryDetector instance;
	static Transform thisTransStatic;

	//Public vars
	public bool checkResOnUpdate;

	public float autoPickUpRange = 2;
	public float manualPickUpRange = 4;
	public Vector3 itemDropOffset;
	public Canvas invCanvas;
	public GameObject objHolder;

	//not static vars
	int oldResWidth;
	int oldResHeight;

	//Array list for storing inventory detection data. (This may change to another storage method soon).
	static List<InventoryDetectionData> detections = new List<InventoryDetectionData>();

	#region Instnance handling
	InventoryDetector() {} //Prevent initialization.

	/// <summary>
	/// Gets the current instance of the InventoryDetector.
	/// </summary>
	/// <returns>The current instance of the detector.</returns>
	public static InventoryDetector getDetector() {
		if (instance == null) {
			instance = InventoryDetector.thisTransStatic.gameObject.GetComponent<InventoryDetector>();
		}
		return instance;
	}
	#endregion

	#region Unity Methods
	void Awake() {
		thisTransStatic = gameObject.transform;

		oldResWidth = Screen.width;
		oldResHeight = Screen.height;
	}

	void Start() {
		InventoryEvents.getEvents().onInventoryCloseEvent += checkHoldingOnInvClose;
		InventoryEvents.getEvents().onLeftClickItemEvent += checkHoldingOnLeftClick;
		InventoryEvents.getEvents().onInventoryCreateEvent += OnInventoryCreateRegister;
		InventoryEvents.getEvents().onInventoryRemoveEvent += OnInventoryRemoveUnregister;
	}

	void Update() {
		//Item setting and checking things for every inventory.
		checkForHover();
		if (Input.GetMouseButtonDown(0)) {
			checkLeftClick();
		} else {
			//Make left clicks null
			clearLeftClick();
		}
		if (Input.GetMouseButtonDown(1)) {
			checkRightClick();
		} else {
			//Make right clicks null
			clearRightClick();
		}
		if (Input.GetKeyDown(InventoryManager.getManager().controls.itemDrop) && !Input.GetKey(InventoryManager.getManager().controls.modifierOne)) {
			//Drop one item from stack.
			dropItem();
		}
		if (Input.GetKeyDown(InventoryManager.getManager().controls.itemDrop) && Input.GetKey(InventoryManager.getManager().controls.modifierOne)) {
			//Drop all items from stack.
			dropEntireStack();
		}

		if (checkResOnUpdate) {
			if (oldResWidth != Screen.width || oldResHeight != Screen.height) {
				oldResWidth = Screen.width;
				oldResHeight = Screen.height;
				InventoryUI.getUI().resetAllInventoryDimensions();
			}
		}

		//Function calling
		moveHoldingItem();
	}

	void FixedUpdate() {
		if (!checkResOnUpdate) {
			if (oldResWidth != Screen.width || oldResHeight != Screen.height) {
				oldResWidth = Screen.width;
				oldResHeight = Screen.height;
				InventoryUI.getUI().resetAllInventoryDimensions();
			}
		}
	}
	#endregion

	#region Inventory detection methods
	/// <summary>
	/// Checks for the mouse hovering over an item slot in an inventory.
	/// This will loop through ALL inventories.
	/// </summary>
	void checkForHover() {
		// Analysis disable once ForCanBeConvertedToForeach
		for (int detIndex = 0; detIndex < detections.Count; detIndex++) {
			InventoryDetectionData data = detections[detIndex];
			Inventory inv = InventoryManager.getManager().getInventory(data.invName);
			if (inv.isOpen()) {
				Vector2 mousePos = Input.mousePosition;

				bool hasSetHover = false;
				for (int index = 0; index < inv.getSize(); index++) {
					ItemStack stack = inv.getItem(index);
					if (stack != null) {
						Image stackIm = stack.getItemImage();
						Vector3 stackPos = stackIm.transform.position;
						float width = (stackIm.rectTransform.rect.width * invCanvas.scaleFactor) / 2;
						float height = (stackIm.rectTransform.rect.height * invCanvas.scaleFactor) / 2;
						if (mousePos.x - stackPos.x < width && mousePos.x - stackPos.x > -width && mousePos.y - stackPos.y < height && mousePos.y - stackPos.y > -height) {
							if (InventoryManager.getManager().debug) {
								print("Mouse hovering over: " + stackIm.gameObject.name);
							}
							data.hover = stack;
							InventoryEvents.getEvents().callHoverItemEvent(stack);
							hasSetHover = true;
							break;
						}
					}
				}
				if (!hasSetHover) {
					data.hover = null;
				}
			} else {
				data.hover = null;
				continue;
			}
		}
	}

	/// <summary>
	/// Checks for the mouse left clicking on an item slot.
	/// This will loop through ALL inventories.
	/// </summary>
	void checkLeftClick() {
		// Analysis disable once ForCanBeConvertedToForeach
		for (int detIndex = 0; detIndex < detections.Count; detIndex++) {
			InventoryDetectionData data = detections[detIndex];
			Inventory inv = InventoryManager.getManager().getInventory(data.invName);
			if (inv.isOpen()) {
				Vector2 mousePos = Input.mousePosition;
				bool hasSetClick = false;
				for (int index = 0; index < inv.getSize(); index++) {
					ItemStack stack = inv.getItem(index);
					if (stack != null) {
						Image stackIm = stack.getItemImage();
						Vector3 stackPos = stackIm.transform.position;
						float width = (stackIm.rectTransform.rect.width * invCanvas.scaleFactor) / 2;
						float height = (stackIm.rectTransform.rect.height * invCanvas.scaleFactor) / 2;
						if (mousePos.x - stackPos.x < width && mousePos.x - stackPos.x > -width && mousePos.y - stackPos.y < height && mousePos.y - stackPos.y > -height) {
							if (InventoryManager.getManager().debug) {
								print("Mouse left clicked: " + stackIm.gameObject.name);
							}
							data.leftClick = stack;
							InventoryEvents.getEvents().callLeftClickItemEvent(stack);
							hasSetClick = true;
							break;
						}
					}
				}
				if (!hasSetClick) {
					data.leftClick = null;
				}
			} else {
				data.leftClick = null;
				continue;
			}
		}
	}

	/// <summary>
	/// Checks for the mouse right clicking on an item slot.
	/// This will loop through ALL inventories.
	/// </summary>
	void checkRightClick() {
		// Analysis disable once ForCanBeConvertedToForeach
		for (int detIndex = 0; detIndex < detections.Count; detIndex++) {
			InventoryDetectionData data = detections[detIndex];
			Inventory inv = InventoryManager.getManager().getInventory(data.invName);
			if (inv.isOpen()) {
				Vector2 mousePos = Input.mousePosition;
				bool hasSetClick = false;
				for (int index = 0; index < inv.getSize(); index++) {
					ItemStack stack = inv.getItem(index);
					if (stack != null) {
						Image stackIm = stack.getItemImage();
						Vector3 stackPos = stackIm.transform.position;
						float width = (stackIm.rectTransform.rect.width * invCanvas.scaleFactor) / 2;
						float height = (stackIm.rectTransform.rect.height * invCanvas.scaleFactor) / 2;
						if (mousePos.x - stackPos.x < width && mousePos.x - stackPos.x > -width && mousePos.y - stackPos.y < height && mousePos.y - stackPos.y > -height) {
							if (InventoryManager.getManager().debug) {
								print("Mouse right clicked: " + stackIm.gameObject.name);
							}
							data.rightClick = stack;
							InventoryEvents.getEvents().callRightClickItemEvent(stack);
							hasSetClick = true;
						}
					}
				}
				if (!hasSetClick) {
					data.rightClick = null;
				}
			} else {
				data.rightClick = null;
				continue;
			}
		}
	}
	#endregion

	#region Hotbar detection methods
	/// <summary>
	/// This will change the currently selected Hotbar Selection if need be.
	/// This will be automatically called by the InventoryViewer if it has a need to.
	/// </summary>
	/// <param name="viewer">Viewer.</param>
	public void changeHotbarSelection(InventoryViewer viewer) {
		if (viewer.usePersonalInv) {
			if (viewer.getPersonalInventory().hasHotbar()) {
				InventoryDetectionData data = getData(viewer.getPersonalInventory());
				int newHotbarIndex = data.hotbarSelectionNumber;
				if (!InventoryManager.getManager().controls.invertHotbarScroll) {
					if (Input.GetAxis("Mouse ScrollWheel") < 0) {
						if (data.hotbarSelectionNumber <= 0) {
							newHotbarIndex = 7;
						} else {
							newHotbarIndex = data.hotbarSelectionNumber - 1;
						}
					} else if (Input.GetAxis("Mouse ScrollWheel") > 0) {
						if (data.hotbarSelectionNumber >= 7) {
							newHotbarIndex = 0;
						} else {
							newHotbarIndex = data.hotbarSelectionNumber + 1;
						}
					}
				} else {
					//Scrolling inverted
					if (Input.GetAxis("Mouse ScrollWheel") < 0) {
						if (data.hotbarSelectionNumber >= 7) {
							newHotbarIndex = 0;
						} else {
							newHotbarIndex = data.hotbarSelectionNumber + 1;
						}
					} else if (Input.GetAxis("Mouse ScrollWheel") > 0) {
						if (data.hotbarSelectionNumber <= 0) {
							newHotbarIndex = 7;
						} else {
							newHotbarIndex = data.hotbarSelectionNumber - 1;
						}
					}
				}
				if (data.hotbarSelectionNumber != newHotbarIndex) {
					InventoryEvents.getEvents().callHotbarSelectionChange(viewer.getPersonalInventory(), newHotbarIndex);
				}
			}
		}
	}
	#endregion

	#region Special detection methods
	/// <summary>
	/// This will check if the viewer is looking at an inventory within a GameObject.
	/// Such inventories would be container inventories.
	/// </summary>
	/// <returns><c>true</c>, if looking at an openable inventory, <c>false</c> otherwise.</returns>
	/// <param name="viewer">Viewer.</param>
	public bool isLookingAtOpenableInv(InventoryViewer viewer) {
		//This does nothing right now because well, there are no other supported inventories yet.
		return false;
	}

	/// <summary>
	/// Checks if the viewer is looking at an item that can be picked up.
	/// Normally will automatically be called by the viewer itself.
	/// **TIP** This issues the raycast from the center of the viewer, this may not be what you want.
	/// **WARNING** Use with care, this uses Raycasts and can be hard on the CPU.
	/// </summary>
	/// <returns>A PickupData object which holds all relevant data</returns>
	/// <param name="viewer">The viewer which may be looking at an item.</param>
	public PickUpData isLookingAtItem(InventoryViewer viewer) {
		if (InventoryManager.getManager().manualPickup && viewer.canPickUpItems) {
			Ray ray = new Ray(viewer.transform.position, viewer.transform.forward);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, manualPickUpRange)) {
				if (hit.collider.gameObject.GetComponent<Item>() != null) {
					if (InventoryManager.getManager().debug) {
						print("Item pickup detected! Item: " + hit.collider.GetComponent<Item>().getName());
					}
					return new PickUpData(true, hit.collider.GetComponent<Item>());
				}
			}
		}
		return new PickUpData(false, null);
	}

	/// <summary>
	/// Checks if the viewer is looking at an item that can be picked up.
	/// Normally will automatically be called by the viewer itself.
	/// **TIP** This issues the raycast from an override point, this may not be what you want.
	/// **WARNING** Use with care, this uses Raycasts and can be hard on the CPU.
	/// </summary>
	/// <returns>A PickupData object which holds all relevant data.</returns>
	/// <param name="viewer">The viewer which may be looking at an item</param>
	/// <param name="overrideViewPoint">Override view point to issue the raycast from (like an FPS camera)</param>
	public PickUpData isLookingAtItem(InventoryViewer viewer, GameObject overrideViewPoint) {
		if (InventoryManager.getManager().manualPickup && viewer.canPickUpItems) {
			Ray ray = new Ray(overrideViewPoint.transform.position, overrideViewPoint.transform.forward);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, manualPickUpRange)) {
				if (hit.collider.gameObject.GetComponent<Item>() != null) {
					if (InventoryManager.getManager().debug) {
						print("Item pickup detected! Item: " + hit.collider.GetComponent<Item>().getName());
					}
					return new PickUpData(true, hit.collider.GetComponent<Item>());
				}
			}
		}
		return new PickUpData(false, null);
	}

	/// <summary>
	/// Checks if the viewer is looking at an item that can be picked up.
	/// **TIP** If desired, this will highlight the item in an outline.
	/// **TIP** This issues the raycast from the center of the viewer, this may not be what you want.
	/// **WARNING** Use with care, this uses Raycasts and can be hard on the CPU.
	/// </summary>
	/// <returns>A PickupData object which holds all relevant data.</returns>
	/// <param name="viewer">The viewer which may be looking at an item.</param>
	/// <param name="highlight">If set to <c>true</c> will highlight the item in an outline (if enabled).</param>
	public PickUpData isLookingAtItem(InventoryViewer viewer, bool highlight) {
		if (InventoryManager.getManager().manualPickup && viewer.canPickUpItems) {
			Ray ray = new Ray(viewer.transform.position, viewer.transform.forward);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, manualPickUpRange)) {
				if (hit.collider.gameObject.GetComponent<Item>() != null) {
					if (InventoryManager.getManager().debug) {
						print("Item pickup detected! Item: " + hit.collider.GetComponent<Item>().getName());
					}
					if (InventoryManager.getManager().highlightItemToPickup && highlight) {
						Item item = hit.collider.GetComponent<Item>();
						Color color = InventoryUI.getUI().manualPickupHighlight;
						ItemOutline.getManager().outlineItem(item, color, viewer);
					}
					return new PickUpData(true, hit.collider.GetComponent<Item>());
				}
			}
		}
		if (InventoryManager.getManager().highlightItemToPickup && highlight) {
			ItemOutline.getManager().stopOutlining(viewer);
		}
		return new PickUpData(false, null);
	}

	/// <summary>
	/// Checks if the viewer is looking at an item that can be picked up.
	/// **TIP** If desired, this will highlight the item in an outline.
	/// **TIP** This issues the raycast from an override point, this may not be what you want.
	/// **WARNING** Use with care, this uses Raycasts and can be hard on the CPU.
	/// </summary>
	/// <returns>A PickupData object which holds all relevant data.</returns>
	/// <param name="viewer">The viewer that may be looking at an item..</param>
	/// <param name="overrideViewPoint">Override view point.</param>
	/// <param name="highlight">If set to <c>true</c> will highlight the item in an outline (if enabled).</param>
	public PickUpData isLookingAtItem(InventoryViewer viewer, GameObject overrideViewPoint, bool highlight) {
		if (InventoryManager.getManager().manualPickup && viewer.canPickUpItems) {
			Ray ray = new Ray(overrideViewPoint.transform.position, overrideViewPoint.transform.forward);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, manualPickUpRange)) {
				if (hit.collider.gameObject.GetComponent<Item>() != null) {
					if (InventoryManager.getManager().debug) {
						print("Item pickup detected! Item: " + hit.collider.GetComponent<Item>().getName());
					}
					if (InventoryManager.getManager().highlightItemToPickup && highlight) {
						Item item = hit.collider.GetComponent<Item>();
						Color color = InventoryUI.getUI().manualPickupHighlight;
						ItemOutline.getManager().outlineItem(item, color, viewer);
					}
					return new PickUpData(true, hit.collider.GetComponent<Item>());
				}
			}
		}
		if (InventoryManager.getManager().highlightItemToPickup && highlight) {
			ItemOutline.getManager().stopOutlining(viewer);
		}
		return new PickUpData(false, null);
	}

	/// <summary>
	/// Checks if the viewer is in range to pickup an item (from autopickup).
	/// </summary>
	/// <returns>A PickupData object that holds all relevant data.</returns>
	/// <param name="viewer">The viewer that may be in range of an item.</param>
	public PickUpData isInRangeOfItem(InventoryViewer viewer) {
		foreach (Collider col in Physics.OverlapSphere(viewer.transform.position, autoPickUpRange)) {
			if (col.GetComponent<Item>() != null) {
				if (InventoryManager.getManager().debug) {
					print("Auto pickup detected! Item: " + col.GetComponent<Item>().getName());
				}
				return new PickUpData(true, col.GetComponent<Item>());
			}
		}
		return new PickUpData(false, null);
	}
	#endregion

	#region Private functionals
	/// <summary>
	/// Sets the item holding clone for the invnetory and itemstack.
	/// </summary>
	/// <param name="toHold">To hold.</param>
	void setHoldingItem(ItemStack toHold) {
		if (toHold.getAmount() != 0) {
			InventoryDetectionData data = getData(toHold.getInventory());
			data.holdingClone.setAs(toHold, data.holdingClone.getItemImage());
			data.holdingClone.setPosition(Input.mousePosition);

			data.holdingClone.getItemImage().gameObject.SetActive(true);

			getData(toHold.getInventory()).holdingItemRef = toHold;
			if (InventoryManager.getManager().debug) {
				print("[1] Now holding item from slot: " + toHold.getItemImage().name);
			}
		} else if (InventoryManager.getManager().debug) {
			InventoryDetectionData data = getData(toHold.getInventory());
			data.holdingClone.setAs(toHold, data.holdingClone.getItemImage());
			data.holdingClone.setPosition(Input.mousePosition);

			data.holdingClone.getItemImage().gameObject.SetActive(true);

			getData(toHold.getInventory()).holdingItemRef = toHold;
			if (InventoryManager.getManager().debug) {
				print("[2] Now holding item from slot: " + toHold.getItemImage().name);
			}
		}
	}

	/// <summary>
	/// Clears the holding item and the item ref.
	/// </summary>
	/// <param name="inv">The inventory to use.</param>
	void clearHoldingItem(Inventory inv) {
		InventoryDetectionData data = getData(inv);
		data.holdingClone.getItemImage().gameObject.SetActive(false);

		inv.clearSlot(data.holdingItemRef.getIndex());

		data.holdingClone.setPosition(Vector3.zero);
		data.holdingClone.clear();
		data.holdingClone.setAmount(0);
		data.holdingClone.setItemMeta(null);
		data.holdingClone.setItemSprite(InventoryUI.getUI().getImageForItem("Air"));
	    
		data.holdingItemRef = null;

		if (InventoryManager.getManager().debug) {
			print("No longer holding item!");
		}
	}

	/// <summary>
	/// Clears the holding item but not the item ref.
	/// </summary>
	/// <param name="inv">The inventory to use.</param>
	/// <param name="i">The index.</param>
	void clearHoldingItem(Inventory inv, int i) {
		InventoryDetectionData data = getData(inv);
		data.holdingClone.getItemImage().gameObject.SetActive(false);

		data.holdingClone.setPosition(Vector3.zero);
		data.holdingClone.clear();
		data.holdingClone.setAmount(0);
		data.holdingClone.setItemMeta(null);
		data.holdingClone.setItemSprite(InventoryUI.getUI().getImageForItem("Air"));

		data.holdingItemRef = null;

		if (InventoryManager.getManager().debug) {
			print("No longer holding item!");
		}
	}

	/// <summary>
	/// Sets the click item from hover.
	/// </summary>
	/// <param name="stackTo">Stack to.</param>
	void setClickItemFromHover(ItemStack stackTo) {
		if (InventoryManager.getManager().debug) {
			print("Setting item in slot: " + stackTo.getItemImage().name + ", from: " + getData(stackTo.getInventory()).holdingItemRef.getItemImage().name);
		}
		stackTo.setAs(getData(stackTo.getInventory()).holdingItemRef);
		if (!stackTo.Equals(getData(stackTo.getInventory()).holdingItemRef)) {
			clearHoldingItem(stackTo.getInventory());
		} else {
			clearHoldingItem(stackTo.getInventory(), 0);
		}
		if (InventoryManager.getManager().saverSettings.saveOnItemMove) {
			InventorySaver.getSaver().SaveInventory(stackTo.getInventory());
		}
	}

	/// <summary>
	/// Sets the click item from hover. But only adds 1 item to the itemstack.
	/// **WARNING** This DOES NOT check if the stack matches at all!!!!!!!
	/// </summary>
	/// <param name="stackTo">Stack to.</param>
	void setOneClickItemFromHover(ItemStack stackTo) {
		if (InventoryManager.getManager().debug) {
			print("Adding one item in slot: " + stackTo.getItemImage().name + ", from: " + getData(stackTo.getInventory()).holdingItemRef.getItemImage().name);
		}
		ItemStack itemRef = getData(stackTo.getInventory()).holdingItemRef;
		stackTo.setAmount(stackTo.getAmount() + 1);
		itemRef.setAmount(itemRef.getAmount() - 1);
		if (!stackTo.Equals(itemRef)) {
			if (itemRef.getAmount() <= 0) {
				//Clear holding ref
				itemRef.clear();
			}
			clearHoldingItem(stackTo.getInventory());
		} else {
			if (itemRef.getAmount() <= 0) {
				//Clear holding ref
				itemRef.clear();
			}
			clearHoldingItem(stackTo.getInventory(), 0);
		}

	}

	void setHoveringAsHoldingOne(ItemStack stackFrom, ItemStack stackTo) {
		if (InventoryManager.getManager().debug) {
			print("Adding item in slot: " + stackTo.getItemImage().name + ", from: " + getData(stackTo.getInventory()).holdingItemRef.getItemImage().name);
		}
		if (stackTo.isSimilar(stackFrom)) {
			Inventory inv = stackTo.getInventory();
			//Working here

		}
	}

	/// <summary>
	/// Moves the holding item to the mouse location.
	/// </summary>
	void moveHoldingItem() {
		foreach (InventoryDetectionData data in detections) {
			if (data.holdingItemRef != null) {
				data.holdingClone.getItemImage().transform.position = Input.mousePosition;
			}
		}
	}

	/// <summary>
	/// Clears the left click within the inventory data.
	/// </summary>
	void clearLeftClick() {
		// Analysis disable once ForCanBeConvertedToForeach
		for (int detIndex = 0; detIndex < detections.Count; detIndex++) {
			InventoryDetectionData data = detections[detIndex];
			data.leftClick = null;
		}
	}

	/// <summary>
	/// Clears the right click within the inventory data.
	/// </summary>
	void clearRightClick() {
		// Analysis disable once ForCanBeConvertedToForeach
		for (int detIndex = 0; detIndex < detections.Count; detIndex++) {
			InventoryDetectionData data = detections[detIndex];
			data.rightClick = null;
		}
	}

	/// <summary>
	/// Creates the holding clone for the specified inventory
	/// </summary>
	/// <param name="inv">The inventory to create the clone for.</param>
	void createHoldingClone(Inventory inv) {
		InventoryDetectionData data = getData(inv);

		data.holdingClone = new ItemStack(null, 1, true);
		data.holdingClone.setPosition(Vector3.zero);
		resetHoldingCloneSize(data.holdingClone);
		data.holdingClone.getItemImage().name = inv.getName() + "; HoldingItemClone";
		data.holdingClone.getItemImage().rectTransform.SetParent(objHolder.transform);
		data.holdingClone.getItemImage().transform.localScale = Vector3.one;

		Text countText = data.holdingClone.getItemCountText();
		countText.rectTransform.sizeDelta = new Vector2(100, 100);

		countText.rectTransform.position = Vector3.zero;
		countText.rectTransform.localScale = Vector3.one;

		countText.gameObject.name = inv.getName() + "; HoldingClone; countText";
		countText.rectTransform.SetParent(data.holdingClone.getItemImage().rectTransform);
		countText.rectTransform.localScale = Vector3.one;
		countText.rectTransform.position = data.holdingClone.getItemImage().rectTransform.position;
		countText.color = InventoryUI.getUI().invItemCountColor;
		countText.font = InventoryUI.getUI().inventoryFont;
		data.holdingClone.setItemCountText(countText);

		data.holdingClone.getItemImage().gameObject.SetActive(false);
	}

	/// <summary>
	/// Creates the display text object for the specified inventory.
	/// </summary>
	/// <param name="inv">The inventory to create the display text for.</param>
	void createDisplayText(Inventory inv) {
		InventoryDetectionData data = getData(inv);

		Text displayText = GameObject.Instantiate(InventoryManager.getManager().defaultDisplayText);
		displayText.text = "";
		displayText.color = InventoryUI.getUI().invItemHoverColor;
		displayText.font = InventoryUI.getUI().inventoryFont;
		displayText.name = inv.getName() + "; DisplayText";
		displayText.rectTransform.SetParent(objHolder.transform);
		displayText.rectTransform.localScale = Vector3.one;
		displayText.transform.position = Input.mousePosition;
		data.hoverDisplayText = displayText;
	}

	/// <summary>
	/// Creates the hotbar selection visualizer image.
	/// </summary>
	/// <param name="inv">The inventory to create the visual for.</param>
	void createHotbarSelectionVisual(Inventory inv) {
		InventoryDetectionData data = getData(inv);

		Image vis = GameObject.Instantiate(InventoryManager.getManager().hbSelection);
		vis.name = inv.getName() + "; HB-Selection";
		vis.rectTransform.SetParent(objHolder.transform);
		vis.rectTransform.localScale = Vector3.one;
		vis.transform.position = InventoryUI.getUI().getNextSlotPosition(0, vis);
		vis.gameObject.SetActive(inv.getHotbar().isDisplayed());
		data.hotbarSelectionVisual = vis;
	}

	/// <summary>
	/// Drops the item the mouse is hovering over.
	/// </summary>
	void dropItem() {
		foreach (InventoryDetectionData data in detections) {
			Inventory inv = InventoryManager.getManager().getInventory(data.invName);
			if (inv.isOpen()) {
				if (data.hover != null) {
					ItemStack itemstack = inv.getItem(data.hover.getIndex());
					int index = itemstack.getIndex();
					String itemName = itemstack.getName();
					InventoryViewer viewer = inv.getViewer();
					Vector3 dropPosition = viewer.gameObject.transform.position + (viewer.transform.forward * itemDropOffset.x) + (Vector3.up * itemDropOffset.y);

					if (!itemstack.hasItemMeta()) {
						if (ItemManager.getItemManager().dropItem(itemName, dropPosition)) {
							if (itemstack.getAmount() - 1 <= 0) {
								inv.clearSlot(index);
							} else {
								itemstack.setAmount(itemstack.getAmount() - 1);
								inv.UpdateInventory();
							}
							//Call item drop event
							InventoryEvents.getEvents().callItemDropEvent(inv, itemName);
						}
					} else {
						if (ItemManager.getItemManager().dropItem(itemName, dropPosition, itemstack.getItemMeta())) {
							if (itemstack.getAmount() - 1 <= 0) {
								inv.clearSlot(index);
							} else {
								itemstack.setAmount(itemstack.getAmount() - 1);
								inv.UpdateInventory();
							}
							//Call item drop event 
							InventoryEvents.getEvents().callItemDropEvent(inv, itemName);
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// Drops all items in the itemstack the mouse is hovering over.
	/// </summary>
	void dropEntireStack() {
		foreach (InventoryDetectionData data in detections) {
			Inventory inv = InventoryManager.getManager().getInventory(data.invName);
			if (inv.isOpen()) {
				if (data.hover != null) {
					ItemStack itemstack = inv.getItem(data.hover.getIndex());
					int initialAmount = itemstack.getAmount();
					int index = itemstack.getIndex();
					String itemName = itemstack.getName();
					InventoryViewer viewer = inv.getViewer();
					Vector3 dropPosition = viewer.gameObject.transform.position + (viewer.transform.forward * itemDropOffset.x) + (Vector3.up * itemDropOffset.y);

					if (ItemManager.getItemManager().dropEntireStack(itemstack, dropPosition)) {
						if (itemstack.getAmount() <= 0) {
							inv.clearSlot(index);
							inv.UpdateInventory();
							//Call item drop event
							InventoryEvents.getEvents().callItemDropEvent(inv, itemName);
						}
					} else if (itemstack.getAmount() < initialAmount) {
						//Did not drop all items but did drop some.
						//Call item drop event.
						inv.UpdateInventory();
						InventoryEvents.getEvents().callItemDropEvent(inv, itemName);
					}
				}
			}
		}
	}

	/// <summary>
	/// Drops the specified item from the inventory.
	/// </summary>
	/// <returns><c>true</c>, if the item was dropped, <c>false</c> otherwise.</returns>
	/// <param name="inv">The inventory to drop the item from.</param>
	/// <param name="itemIndex">The index of the item to drop.</param>
	public bool dropItem(Inventory inv, int itemIndex) {
		if (inv.hasOwner()) {
			ItemStack itemstack = inv.getItem(itemIndex);
			String itemName = itemstack.getName();
			InventoryViewer owner = inv.getOwner();
			Vector3 dropPosition = owner.gameObject.transform.position + (owner.transform.forward * itemDropOffset.x) + (Vector3.up * itemDropOffset.y);

			if (!itemstack.hasItemMeta()) {
				if (ItemManager.getItemManager().dropItem(itemName, dropPosition)) {
					if (itemstack.getAmount() - 1 <= 0) {
						inv.clearSlot(itemIndex);
					} else {
						itemstack.setAmount(itemstack.getAmount() - 1);
						inv.UpdateInventory();
					}
					//Call item drop event
					InventoryEvents.getEvents().callItemDropEvent(inv, itemName);
					return true;
				}
			} else {
				if (ItemManager.getItemManager().dropItem(itemName, dropPosition, itemstack.getItemMeta())) {
					if (itemstack.getAmount() - 1 <= 0) {
						inv.clearSlot(itemIndex);
					} else {
						itemstack.setAmount(itemstack.getAmount() - 1);
						inv.UpdateInventory();
					}
					//Call item drop event
					InventoryEvents.getEvents().callItemDropEvent(inv, itemName);
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>
	/// Drops all items in the itemstack in the inventory.
	/// </summary>
	/// <returns><c>true</c>, if at least one (1) item was dropped, <c>false</c> otherwise.</returns>
	/// <param name="inv">Inv.</param>
	/// <param name="itemIndex">Item index.</param>
	public bool dropEntireStack(Inventory inv, int itemIndex) {
		if (inv.hasOwner()) {
			ItemStack itemstack = inv.getItem(itemIndex);
			int initialAmount = itemstack.getAmount();
			String itemName = itemstack.getName();
			InventoryViewer owner = inv.getOwner();
			Vector3 dropPosition = owner.gameObject.transform.position + (owner.transform.forward * itemDropOffset.x) + (Vector3.up * itemDropOffset.y);

			if (ItemManager.getItemManager().dropEntireStack(itemstack, dropPosition)) {
				if (itemstack.getAmount() <= 0) {
					inv.clearSlot(itemIndex);
					inv.UpdateInventory();
					//Call item drop event
					InventoryEvents.getEvents().callItemDropEvent(inv, itemName);
					return true;
				}
			} else if (itemstack.getAmount() < initialAmount) {
				//Did not drop all items but did drop some.
				//Call item drop event.
				inv.UpdateInventory();
				InventoryEvents.getEvents().callItemDropEvent(inv, itemName);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Resets the size of the holding clone.
	/// </summary>
	/// <param name="holdingClone">Holding clone.</param>
	void resetHoldingCloneSize(ItemStack holdingClone){
		holdingClone.getItemImage().rectTransform.sizeDelta = new Vector2(100, 100);
		holdingClone.getItemCountText().rectTransform.sizeDelta = new Vector2(100, 100);
	}
	#endregion

	#region Event listeners
	/// <summary>
	/// Registers the inventory when it is created.
	/// This is called on the onInventoryCreateEvent event.
	/// </summary>
	/// <param name="inv">The inventory created.</param>
	void OnInventoryCreateRegister(Inventory inv) {
		if (!hasData(inv)) {
			InventoryDetectionData data = new InventoryDetectionData(inv.getName());
			detections.Add(data);
			createHoldingClone(inv);
			createDisplayText(inv);

			if (inv.hasHotbar()) {
				createHotbarSelectionVisual(inv);
			}
			if (InventoryManager.getManager().debug) {
				print("Registered inventory detection data for inv: " + inv.getName());
			}
		}
	}

	/// <summary>
	/// Deregisters the inventory when it is removed/deleted.
	/// This is called on the onInventoryRemoveEvent event.
	/// </summary>
	/// <param name="inv">The inventory removed.</param>
	void OnInventoryRemoveUnregister(Inventory inv) {
		if (hasData(inv)) {
			InventoryDetectionData data = getData(inv);

			if (data.holdingClone != null) {
				if (data.holdingClone.getItemImage() != null) {
					Destroy(data.holdingClone.getItemImage());
				}
				if (data.holdingClone.getItemCountText() != null) {
					Destroy(data.holdingClone.getItemCountText());
				}
			}
			if (data.hotbarSelectionVisual != null) {
				Destroy(data.hotbarSelectionVisual);
			}
			if (data.hoverDisplayText != null) {
				Destroy(data.hoverDisplayText);
			}

			detections.Remove(data);
			if (InventoryManager.getManager().debug) {
				print("Unregistered detection data for inv: " + inv.getName());
			}
		}
	}

	/// <summary>
	/// Checks if the holdingClone is not null on inventory close and does the appropriate action.
	/// This is called on the onInventoryCloseEvent
	/// </summary>
	/// <param name="inv">The inventory that has been closed</param>
	void checkHoldingOnInvClose(Inventory inv) {
		if (getData(inv).holdingItemRef != null) {
			clearHoldingItem(inv);
		}
	}

	/// <summary>
	/// Checks if the holdingClone is not null on left click and does the appropriate action.
	/// </summary>
	/// <param name="clickedItem">The clicked itemstack.</param>
	void checkHoldingOnLeftClick(ItemStack clickedItem) {
		if (getData(clickedItem.getInventory()).holdingItemRef == null) {
			if (InventoryManager.getManager().debug) {
				print("Setting holding item!");
			}
			setHoldingItem(clickedItem);
		} else {
			if (InventoryManager.getManager().debug) {
				print("Setting new item stuffs from holding item!");
			}
			setClickItemFromHover(clickedItem);
		}
	}

	/// <summary>
	/// Checks if the holding clone is not null on right click and does the appropriate action.
	/// </summary>
	/// <param name="clickedItem">The clicked itemstack.</param>
	void checkHoldingOnRightClickAndSetStuff(ItemStack clickedItem) {
		if (getData(clickedItem.getInventory()).holdingItemRef != null) {
			if (clickedItem.isSimilar(getData(clickedItem.getInventory()).holdingItemRef)) {
				if (InventoryManager.getManager().debug) {
					print("Adding one item to clicked stack from holding item!");
				}
				setOneClickItemFromHover(clickedItem);
			}
		}
	}
	#endregion

	#region Bools & Checkers
	/// <summary>
	/// Checks if the specified inventory has detection data.
	/// </summary>
	/// <returns><c>true</c>, if the inventory has detection data, <c>false</c> otherwise.</returns>
	/// <param name="inv">Inv.</param>
	public bool hasData(Inventory inv) {
		foreach (InventoryDetectionData data in detections) {
			if (data.invName == inv.getName()) {
				return true;
			} else {
				continue;
			}
		}
		return false;
	}
	#endregion

	#region Getters
	/// <summary>
	/// Gets the InventoryDetectionData object for the specified inventory.
	/// **TIP** Check if the inventory has data or this will be null.
	/// </summary>
	/// <returns>The InventoryDetectionData object for the specified inventory.</returns>
	/// <param name="inv">The inventory to get data for.</param>
	public InventoryDetectionData getData(Inventory inv) {
		foreach (InventoryDetectionData data in detections) {
			if (data.invName == inv.getName()) {
				return data;
			}
		}
		return new InventoryDetectionData("");
	}

	/// <summary>
	/// Gets the hover itemstack from the specified inventory's detection data.
	/// </summary>
	/// <returns>The hover itemstack.</returns>
	/// <param name="inv">The inventory to get the data for.</param>
	public ItemStack getHoverItemStack(Inventory inv) {
		return getData(inv).hover;
	}

	/// <summary>
	/// Gets the left clicked itemstack from the specified inventory's detection data.
	/// </summary>
	/// <returns>The left clicked itemstack.</returns>
	/// <param name="inv">The inventory to get the data for.</param>
	public ItemStack getLeftClickItemStack(Inventory inv) {
		return getData(inv).leftClick;
	}

	/// <summary>
	/// Gets the right clicked itemstack from the specified inventory's detection data.
	/// </summary>
	/// <returns>The right clicked itemstack.</returns>
	/// <param name="inv">The inventory to get the data for.</param>
	public ItemStack getRightClickItemStack(Inventory inv) {
		return getData(inv).rightClick;
	}

	/// <summary>
	/// Gets the holding Item reference itemstack from the specified inventory's detection data.
	/// </summary>
	/// <returns>The holding itemstack</returns>
	/// <param name="inv">The inventory to get the data for</param>
	public ItemStack getHoldingItem(Inventory inv) {
		return getData(inv).holdingItemRef;
	}

	/// <summary>
	/// Gets the itemstack from the specified inventory in (or near) the specified Unity World Space position.
	/// </summary>
	/// <returns>The itemstack from the specified position.</returns>
	/// <param name="inv">The inventory to get the data for.</param>
	/// <param name="pos">The Unity World Space position of (or near) the itemstack.</param>
	public ItemStack getItemStackFromPosition(Inventory inv, Vector2 pos) {
		for (int i = 0; i < inv.getSize(); i++) {
			ItemStack stack = inv.getItem(i);
			if (stack != null) {
				Vector3 stackPos = stack.getPosition();
				Image stackIm = stack.getItemImage();
				float width = (stackIm.rectTransform.rect.width * invCanvas.scaleFactor) / 2;
				float height = (stackIm.rectTransform.rect.height * invCanvas.scaleFactor) / 2;
				if (pos.x - stackPos.x < width && pos.x - stackPos.x > -width && pos.y - stackPos.y < height && pos.y - stackPos.y > -height) {
					return stack;
				} else {
					continue;
				}
			} else {
				continue;
			}
		}
		return null;
	}
	#endregion

	#region Debugging
	/// <summary>
	/// Used for debugging, this method will highlight itemstacks as a representative color to test detections.
	/// Black: hovered over
	/// Clear: right or left clicked
	/// White: undetected/not applicable
	/// </summary>
	/// <param name="inv">The inventory to test.</param>
	public void checkStackThing(Inventory inv) {
		if (InventoryManager.getManager().debug) {
			if (inv.isOpen()) {
				for (int i = 0; i < inv.getSize(); i++) {
					ItemStack stack = inv.getItem(i);
					if (stack == getHoverItemStack(inv) && stack != getRightClickItemStack(inv) && stack != getLeftClickItemStack(inv)) {
						Image im = stack.getItemImage();
						im.color = Color.black;
					} else if (stack == getRightClickItemStack(inv) || stack == getLeftClickItemStack(inv)) {
						Image im = stack.getItemImage();
						im.color = Color.clear;
					} else {
						Image im = stack.getItemImage();
						im.color = Color.white;
					}
				}
			}
		}
	}
	#endregion
}

/// <summary>
/// Inventory detection data.
/// A specialized class that is used to contain detection data for a specific inventory.
/// This houses all relevant data for inventory detections.
/// </summary>
public class InventoryDetectionData {
	public readonly string invName;

	public ItemStack hover;
	public ItemStack leftClick;
	public ItemStack rightClick;
	public ItemStack holdingItemRef;
	public ItemStack holdingClone;
	public Text hoverDisplayText;

	public Image hotbarSelectionVisual;
	public int hotbarSelectionNumber;
	public int hbItemInfoCallback;

	/// <summary>
	/// Initializes a new instance of the <see cref="InventoryDetectionData"/> class.
	/// </summary>
	/// <param name="_invName">The name of the inventory this data is for.</param>
	public InventoryDetectionData(string _invName) {
		invName = _invName;
		hover = null;
		leftClick = null;
		rightClick = null;
		holdingItemRef = null;
		holdingClone = null;
		hotbarSelectionVisual = null;
		hotbarSelectionNumber = 0;
		hbItemInfoCallback = -1;
	}
}

/// <summary>
/// Pick up data.
/// A specialized class that is used to contain item pick up data for a specific call.
/// This houses all relevant data for item pick ups.
/// </summary>
public class PickUpData {
	public bool canBePickedUp;
	public Item item;

	/// <summary>
	/// Initializes a new instance of the <see cref="PickUpData"/> class.
	/// </summary>
	/// <param name="_canBePickedUp">If set to <c>true</c> the item involved can be picked up</param>
	/// <param name="_item">The item involved..</param>
	public PickUpData(bool _canBePickedUp, Item _item) {
		canBePickedUp = _canBePickedUp;
		item = _item;
	}
}
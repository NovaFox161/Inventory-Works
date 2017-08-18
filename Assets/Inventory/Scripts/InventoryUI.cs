using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering;

using System.Collections;


[AddComponentMenu("Inventory/Inventory UI")]
/// <summary>
/// Inventory UI.
/// This is a singleton class that handles all operations and methods relating to the inventory UI or related UI.
/// </summary>
public class InventoryUI : MonoBehaviour {
	static InventoryUI instance;
	static Transform thisTransStatic;

	public Font inventoryFont;
	public Color invNameColor = Color.cyan;
	public Color invItemHoverColor = Color.red;
	public Color invItemCountColor = Color.black;
	public Color hotbarItemCountColor = Color.red;
	public Color hotbarItemInfoColor = Color.green;
	public Color manualPickupHighlight = Color.cyan;
	public float itemSlotOffset = 145;
	public Vector3 holdOffsetWithoutCam = new Vector3(0.5f, 0.5f, 0.25f);
	public Vector3 hoverTextOffset = new Vector3(0, -50, 0);
	public Vector2 itemInfoPosition = new Vector2(0, -325);
	public Vector2 invNameTextPosition = new Vector2(-800, 450);
	public Camera cam;
	public float hbInfoTextTime = 2;

	#region Instance handling
	InventoryUI() {} //Prevent initialization.

	/// <summary>
	/// Gets the InventoryUI instance.
	/// </summary>
	/// <returns>The InventoryUI instance.</returns>
	public static InventoryUI getUI() {
		if (instance == null) {
			instance = InventoryUI.thisTransStatic.gameObject.GetComponent<InventoryUI>();
		}
		return instance;
	}
	#endregion

	#region Unity methods
	void Awake() {
		thisTransStatic = gameObject.transform;
	}

	public void Start() {
		moveDisplayTextToMouse();
		InventoryEvents.getEvents().onHoverItemEvent += displayText;
		InventoryEvents.getEvents().onInventoryOpenEvent += hideHotbarSelectionOnOpen;
		InventoryEvents.getEvents().onInventoryCloseEvent += showHotbarSelectionOnClose;
		InventoryEvents.getEvents().onInventoryUpdateEvent += changeHoldingOnInventoryUpdate;
		InventoryEvents.getEvents().onItemDropEvent += changeHoldingOnItemDrop;
		InventoryEvents.getEvents().onHotbarSelectionChange += changeHotbarSelection;
		InventoryEvents.getEvents().onHotbarSelectionChange += showItemInfoText;

		InventoryEvents.getEvents().onInventoryOpenEvent += hideHotbarInfoTextOnInventoryOpen;
	}

	public void Update() {
		moveDisplayTextToMouse();
		hideDisplayText();
		updateItemInHandPositions();
	}
	#endregion

	#region Inventory specific funcionals
	/// <summary>
	/// Sets the specified inventory's dimensions on the screen.
	/// This will calculate where everything should be.
	/// **WARNING** This is automatically called upon inventory create and should not be used elsewhere.
	/// </summary>
	/// <param name="inv">The inventory that dimenstions need to be set for.</param>
	public void setInventoryDimensions(Inventory inv) {
		GameObject _invHolder = Instantiate(InventoryManager.getManager().holderPrefab);
		_invHolder.name = "Inv: " + inv.getName();
		_invHolder.transform.SetParent(InventoryManager.getManager().invHolder.transform);
		_invHolder.transform.localPosition = Vector3.zero;
		_invHolder.transform.localScale = Vector3.one;


		Image invBg = Instantiate(InventoryManager.getManager().defaultInvBG);
		invBg.name = inv.getName() + "; InvBG";
		invBg.rectTransform.position = transform.position;
		invBg.rectTransform.SetParent(InventoryManager.getManager().invHolder.transform);
		invBg.rectTransform.sizeDelta = new Vector2(1920, 1080);

		invBg.rectTransform.localScale = Vector3.one;

		inv.setInventoryBackground(invBg);

		Text invNameText = Instantiate(InventoryManager.getManager().defaultInvNameText);
		invNameText.name = inv.getName() + "; InvName";
		invNameText.rectTransform.position = transform.position;
		invNameText.rectTransform.SetParent(InventoryManager.getManager().invHolder.transform);
		invNameText.rectTransform.localScale = Vector3.one;
		invNameText.rectTransform.localPosition = new Vector3(invNameTextPosition.x, invNameTextPosition.y, 0);
		invNameText.font = inventoryFont;
		invNameText.color = invNameColor;

		invNameText.text = inv.isOpen() ? inv.getName() : "";

		inv.setInventoryNameText(invNameText);

		for (int i = 0; i < inv.getSize(); i++) {
			//Get stuff
			ItemStack itemStack = inv.getItem(i);
			Image im = itemStack.getItemImage();
			im.rectTransform.sizeDelta = new Vector2(100, 100);

			Vector2 itemPos = getNextSlotPosition(i, im);

			//Set item image stuff
			im.rectTransform.SetParent(invBg.transform);
			itemStack.setPosition(itemPos);

			im.transform.localScale = Vector3.one;
			im.name = inv.getName() + "; slot " + i;
			itemStack.setItemImage(im);

			//Set itemCountText stuff
			Text countText = itemStack.getItemCountText();
			countText.rectTransform.sizeDelta = new Vector2(100, 100);

			countText.rectTransform.position = Vector3.zero;
			countText.rectTransform.localScale = Vector3.one;

			countText.gameObject.name = inv.getName() + "; countText " + i;
			countText.rectTransform.SetParent(im.rectTransform);
			countText.rectTransform.localScale = Vector3.one;
			countText.rectTransform.position = itemPos;
			countText.color = invItemCountColor;
			countText.font = inventoryFont;
			countText.text = "";

			itemStack.setItemCountText(countText);
		}
			
		if (inv.hasHotbar()) {
			setHotbarDimensions(inv);
		}
	}

	/// <summary>
	/// Sets the specified inventory's hotbar dimensions on the screen.
	/// This will calculate where everything should be.
	/// **WARNING** This is automatically called upon inventory create and should not be used elsewhere.
	/// </summary>
	/// <param name="inv">The hotbar's parent invnetory.</param>
	public void setHotbarDimensions(Inventory inv) {
		Hotbar hotbar = inv.getHotbar();
		Image hotbarBg = Instantiate(InventoryManager.getManager().defaultHotbarBg);
		hotbarBg.name = inv.getName() + "; HotbarBg";
		hotbarBg.rectTransform.position = transform.position;
		hotbarBg.rectTransform.SetParent(InventoryManager.getManager().invHolder.transform);
		hotbarBg.rectTransform.sizeDelta = new Vector2(1920, 1080);

		hotbarBg.rectTransform.localScale = Vector3.one;
			
		hotbar.setHotbarBg(hotbarBg);

		Text hbItemInfo = Instantiate(InventoryManager.getManager().defaultHotbarItemInfo);
		hbItemInfo.name = inv.getName() + "; HotbarItemInfo";
		hbItemInfo.rectTransform.position = Vector3.zero;
		hbItemInfo.rectTransform.SetParent(InventoryManager.getManager().invCanvas.transform);
		hbItemInfo.rectTransform.localScale = Vector3.one;
		hbItemInfo.rectTransform.localPosition = new Vector3(itemInfoPosition.x, itemInfoPosition.y, 0);
		hbItemInfo.color = hotbarItemInfoColor;
		hbItemInfo.font = inventoryFont;
		hbItemInfo.text = "";

		hotbar.setHBItemInfoText(hbItemInfo);

		for (int i = 0; i < 8; i++) {
			//Get stuff
			ItemStack itemStack = hotbar.getHotbarSlot(i);
			Image im = itemStack.getItemImage();
			im.rectTransform.sizeDelta = new Vector2(100, 100);

			Vector2 itemPos = getNextSlotPosition(i, im);

			//Set item image stuff
			im.rectTransform.SetParent(hotbarBg.transform);
			itemStack.setPosition(itemPos);

			im.transform.localScale = Vector3.one;
			im.name = inv.getName() + "; HB-slot " + i;
			itemStack.setItemImage(im);

			//Set itemCountText stuff
			Text countText = itemStack.getItemCountText();
			countText.rectTransform.sizeDelta = new Vector2(100, 100);

			countText.rectTransform.position = Vector3.zero;
			countText.rectTransform.localScale = Vector3.one;

			countText.gameObject.name = inv.getName() + "; countText HB-" + i;
			countText.rectTransform.SetParent(im.rectTransform);
			countText.rectTransform.localScale = Vector3.one;
			countText.rectTransform.position = itemPos;
			countText.color = hotbarItemCountColor;
			countText.font = inventoryFont;
			countText.text = "";

			itemStack.setItemCountText(countText);
		}
	}

	/// <summary>
	/// Resets the specified inventory's dimensions based on the screen size.
	/// This will recalculate where everything should be if the screen size has changed.
	/// **WARNING** This is automatically called upon screen size change and should not be used elsewhere.
	/// </summary>
	/// <param name="inv">The inventory to have recalculated.</param>
	public void resetInventoryDimensions(Inventory inv) {
		if (InventoryManager.getManager().debug) {
			print("Resetting inventory Dimensions for inv:" + inv.getName());
		}
		//Inventory
		GameObject _invHolder = GameObject.Find("Inv: " + inv.getName());
		_invHolder.transform.localPosition = Vector3.zero;
		_invHolder.transform.localScale = Vector3.one;

		Image invBg = inv.getInventoryBackground();
		invBg.rectTransform.position = transform.position;
		invBg.rectTransform.sizeDelta = new Vector2(1920, 1080);

		invBg.rectTransform.localScale = Vector3.one;
		inv.setInventoryBackground(invBg);

		for (int i = 0; i < inv.getSize(); i++) {
			ItemStack itemStack = inv.getItem(i);
			Image im = itemStack.getItemImage();
			Text ct = itemStack.getItemCountText();

			//Set item image stuff
			im.rectTransform.sizeDelta = new Vector2(100, 100);

			itemStack.setItemImage(im);

			//Set item count text stuff
			ct.rectTransform.sizeDelta = new Vector2(100, 100);
		}

		if (inv.hasHotbar()) {
			resetHotbarDimensions(inv);
		}
	}

	/// <summary>
	/// Resets the specified inventory's hotbar dimensions based on the screen size.
	/// This will recalculate where everything should be if the screen size has changed.
	/// **WARNING** This is automatically called upon screen size change and should not be used elsewhere.
	/// </summary>
	/// <param name="inv">The hotbar's parent inventory to have recalculated.</param>
	public void resetHotbarDimensions(Inventory inv) { 
		Image hotbarBg = inv.getHotbar().getHotbarBg();
		hotbarBg.rectTransform.position = transform.position;
		hotbarBg.rectTransform.sizeDelta = new Vector2(1920, 1080);

		hotbarBg.rectTransform.localScale = Vector3.one;
		inv.getHotbar().setHotbarBg(hotbarBg);

		for (int i = 0; i < 8; i++) {
			ItemStack itemStack = inv.getHotbar().getHotbarSlot(i);
			Image im = itemStack.getItemImage();
			Text ct = itemStack.getItemCountText();

			//Set item image stuff
			im.rectTransform.sizeDelta = new Vector2(100, 100);

			itemStack.setItemImage(im);

			//Set item count text stuff
			ct.rectTransform.sizeDelta = new Vector2(100, 100);
		}
	}

	/// <summary>
	/// Resets all existing inventorys' dimensions.
	/// **WARNING** This is automatically called upon sceen size change and should not be used elsewhere.
	/// </summary>
	public void resetAllInventoryDimensions() {
		if (InventoryManager.getManager().debug) {
			print("Attempting to reset all inventory dimensions!"); 
		}
		foreach (Inventory inv in InventoryManager.getManager().getAllInventories()) {
			resetInventoryDimensions(inv);
		}
	}

	/// <summary>
	/// Gets the position of the specified item slot in Unity World Space.
	/// **WARNING** This is not accurate after initial creation. Refer to the itemstack's position.
	/// </summary>
	/// <returns>The position the itemslot should be at in Unity World Space</returns>
	/// <param name="index">The index of the item slot.</param>
	/// <param name="im">The item's physical image.</param>
	public Vector2 getNextSlotPosition(int index, Image im) {
		float width = im.rectTransform.rect.width * InventoryManager.getManager().invCanvas.scaleFactor;
		float height = im.rectTransform.rect.height * InventoryManager.getManager().invCanvas.scaleFactor;

		Vector3 offset = new Vector3(itemSlotOffset * InventoryManager.getManager().invCanvas.scaleFactor, itemSlotOffset * InventoryManager.getManager().invCanvas.scaleFactor, 0);
		Vector3 bottomLeft = cam.ScreenToWorldPoint(new Vector3(Screen.width - (Screen.width / 2), Screen.height - (Screen.height / 2), 0));
		if (index == 0) {
			if (InventoryManager.getManager().debug) {
				print("Offset" + offset);
			}
			return new Vector3(bottomLeft.x + (width * 1) + (offset.x * 0), bottomLeft.y + (height * 1) + (offset.y * 0), 0);
		} else if (index == 1) {
			return new Vector3(bottomLeft.x + (width * 2) + (offset.x * 1), bottomLeft.y + (height * 1) + (offset.y * 0), 0);
		} else if (index == 2) {
			return new Vector3(bottomLeft.x + (width * 3) + (offset.x * 2), bottomLeft.y + (height * 1) + (offset.y * 0), 0);
		} else if (index == 3) {
			return new Vector3(bottomLeft.x + (width * 4) + (offset.x * 3), bottomLeft.y + (height * 1) + (offset.y * 0), 0);
		} else if (index == 4) {
			return new Vector3(bottomLeft.x + (width * 5) + (offset.x * 4), bottomLeft.y + (height * 1) + (offset.y * 0), 0);
		} else if (index == 5) {
			return new Vector3(bottomLeft.x + (width * 6) + (offset.x * 5), bottomLeft.y + (height * 1) + (offset.y * 0), 0);
		} else if (index == 6) {
			return new Vector3(bottomLeft.x + (width * 7) + (offset.x * 6), bottomLeft.y + (height * 1) + (offset.y * 0), 0);
		} else if (index == 7) {
			return new Vector3(bottomLeft.x + (width * 8) + (offset.x * 7), bottomLeft.y + (height * 1) + (offset.y * 0), 0);
		} else if (index == 8) {
			return new Vector3(bottomLeft.x + (width * 1) + (offset.x * 0), bottomLeft.y + (height * 2) + (offset.y * 1), 0);
		} else if (index == 9) {
			return new Vector3(bottomLeft.x + (width * 2) + (offset.x * 1), bottomLeft.y + (height * 2) + (offset.y * 1), 0);
		} else if (index == 10) {
			return new Vector3(bottomLeft.x + (width * 3) + (offset.x * 2), bottomLeft.y + (height * 2) + (offset.y * 1), 0);
		} else if (index == 11) {
			return new Vector3(bottomLeft.x + (width * 4) + (offset.x * 3), bottomLeft.y + (height * 2) + (offset.y * 1), 0);
		} else if (index == 12) {
			return new Vector3(bottomLeft.x + (width * 5) + (offset.x * 4), bottomLeft.y + (height * 2) + (offset.y * 1), 0);
		} else if (index == 13) {
			return new Vector3(bottomLeft.x + (width * 6) + (offset.x * 5), bottomLeft.y + (height * 2) + (offset.y * 1), 0);
		} else if (index == 14) {
			return new Vector3(bottomLeft.x + (width * 7) + (offset.x * 6), bottomLeft.y + (height * 2) + (offset.y * 1), 0);
		} else if (index == 15) {
			return new Vector3(bottomLeft.x + (width * 8) + (offset.x * 7), bottomLeft.y + (height * 2) + (offset.y * 1), 0);
		} else if (index == 16) {
			return new Vector3(bottomLeft.x + (width * 1) + (offset.x * 0), bottomLeft.y + (height * 3) + (offset.y * 2), 0);
		} else if (index == 17) {
			return new Vector3(bottomLeft.x + (width * 2) + (offset.x * 1), bottomLeft.y + (height * 3) + (offset.y * 2), 0);
		} else if (index == 18) {
			return new Vector3(bottomLeft.x + (width * 3) + (offset.x * 2), bottomLeft.y + (height * 3) + (offset.y * 2), 0);
		} else if (index == 19) {
			return new Vector3(bottomLeft.x + (width * 4) + (offset.x * 3), bottomLeft.y + (height * 3) + (offset.y * 2), 0);
		} else if (index == 20) {
			return new Vector3(bottomLeft.x + (width * 5) + (offset.x * 4), bottomLeft.y + (height * 3) + (offset.y * 2), 0);
		} else if (index == 21) {
			return new Vector3(bottomLeft.x + (width * 6) + (offset.x * 5), bottomLeft.y + (height * 3) + (offset.y * 2), 0);
		} else if (index == 22) {
			return new Vector3(bottomLeft.x + (width * 7) + (offset.x * 6), bottomLeft.y + (height * 3) + (offset.y * 2), 0);
		} else if (index == 23) {
			return new Vector3(bottomLeft.x + (width * 8) + (offset.x * 7), bottomLeft.y + (height * 3) + (offset.y * 2), 0);
		} else if (index == 24) {
			return new Vector3(bottomLeft.x + (width * 1) + (offset.x * 0), bottomLeft.y + (height * 4) + (offset.y * 3), 0);
		} else if (index == 25) {
			return new Vector3(bottomLeft.x + (width * 2) + (offset.x * 1), bottomLeft.y + (height * 4) + (offset.y * 3), 0);
		} else if (index == 26) {
			return new Vector3(bottomLeft.x + (width * 3) + (offset.x * 2), bottomLeft.y + (height * 4) + (offset.y * 3), 0);
		} else if (index == 27) {
			return new Vector3(bottomLeft.x + (width * 4) + (offset.x * 3), bottomLeft.y + (height * 4) + (offset.y * 3), 0);
		} else if (index == 28) {
			return new Vector3(bottomLeft.x + (width * 5) + (offset.x * 4), bottomLeft.y + (height * 4) + (offset.y * 3), 0);
		} else if (index == 29) {
			return new Vector3(bottomLeft.x + (width * 6) + (offset.x * 5), bottomLeft.y + (height * 4) + (offset.y * 3), 0);
		} else if (index == 30) {
			return new Vector3(bottomLeft.x + (width * 7) + (offset.x * 6), bottomLeft.y + (height * 4) + (offset.y * 3), 0);
		} else if (index == 31) {
			return new Vector3(bottomLeft.x + (width * 8) + (offset.x * 7), bottomLeft.y + (height * 4) + (offset.y * 3), 0);
		} else {
			return Vector3.zero;
		}
	}
	#endregion

	#region Hotbar & related functionals
	/// <summary>
	/// Displays the item selected in the hotbar as if the person were holding the item.
	/// </summary>
	/// <param name="viewer">The inventory's owner or other viewer.</param>
	public void displayHotbarItem(InventoryViewer viewer) {
		if (viewer.showItemInHand && viewer.usePersonalInv && viewer.hasPersonalInventory() && viewer.getPersonalInventory().hasHotbar()) {
			Hotbar hotbar = viewer.getPersonalInventory().getHotbar();
			int hotbarIndex = InventoryDetector.getDetector().getData(viewer.getPersonalInventory()).hotbarSelectionNumber;
			ItemStack stack = viewer.getPersonalInventory().getItem(hotbarIndex);
			if (hotbar.isDisplayed()) {
				if (!stack.getName().Equals("Air")) {
					Item itemClone = ItemManager.getItemManager().cloneItem(stack);

					itemClone.transform.parent = viewer.transform;
					itemClone.transform.localScale = itemClone.transform.localScale / 2;

					if (viewer.hasCamera()) {
						//Set position relative to the corner of the camera.
						itemClone.transform.position = viewer.transform.position;
						itemClone.transform.localRotation = Quaternion.identity;
					} else {
						//Set potion relative to viewer's location.
						itemClone.transform.position = viewer.transform.position;
						itemClone.transform.localRotation = Quaternion.identity;

						Vector3 itemPos;
						itemPos = viewer.rightHanded ? holdOffsetWithoutCam : new Vector3(holdOffsetWithoutCam.x, 0, -holdOffsetWithoutCam.z);
						itemClone.transform.localPosition = itemPos;
					}

					if (itemClone.GetComponent<Rigidbody>() != null) {
						itemClone.GetComponent<Rigidbody>().useGravity = false;
					}
					if (itemClone.GetComponent<Collider>() != null) {
						itemClone.GetComponent<Collider>().enabled = false;
					}
					if (itemClone.GetComponent<MeshRenderer>() != null) {
						itemClone.GetComponent<MeshRenderer>().receiveShadows = false;
						itemClone.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
					}

					viewer.setItemHolding(itemClone);
					updateItemInHandPositions(viewer.getPersonalInventory());
					itemClone.gameObject.SetActive(true);
				} else {
					viewer.setItemHolding(null);
				}
			} else {
				viewer.setItemHolding(null);
			}
		}
	}

	/// <summary>
	/// Updates the position of the item in the viewer's hand relative to their position.
	/// **WARNING** This updates ALL inventories and should not be used unless needed.
	/// **WARNING** This is automatically called and can be ignored.
	/// </summary>
	public void updateItemInHandPositions() {
		foreach (Inventory inv in InventoryManager.getManager().getAllInventories()) {
			if (inv.hasOwner() && inv.hasHotbar() && inv.getHotbar().isDisplayed()) {
				InventoryViewer owner = inv.getOwner();
				if (owner.isHoldingItem()) {
					if (owner.hasCamera()) {
						Vector3 newPos;
						newPos = !owner.rightHanded ? owner.getCamera().ViewportToWorldPoint(new Vector3(0.2f, 0, 1)) : owner.getCamera().ViewportToWorldPoint(new Vector3(1, 0, 1));
						owner.getItemHolding().transform.position = newPos;
					}
				}
			}
		}
	}

	/// <summary>
	/// Updates the position of the item in the viewer's hand relative to their poision.
	/// **WARNING** this is automatically called and can be ignored.
	/// </summary>
	/// <param name="inv">The inventory to use</param>
	public void updateItemInHandPositions(Inventory inv) {
		if (inv.hasOwner() && inv.hasHotbar() && inv.getHotbar().isDisplayed()) {
			InventoryViewer owner = inv.getOwner();
			if (owner.isHoldingItem()) {
				if (owner.hasCamera()) {
					Vector3 newPos;
					newPos = !owner.rightHanded ? owner.getCamera().ViewportToWorldPoint(new Vector3(0.2f, 0, 1)) : owner.getCamera().ViewportToWorldPoint(new Vector3(1, 0, 1));
					owner.getItemHolding().transform.position = newPos;
				}
			}
		}
	} 

	/// <summary>
	/// Shows the item info text for the hotbar.
	/// **WARNING** this is automatically called and can be ignored.
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="index">The index of the hotbar slot.</param>
	public void showItemInfoText(Inventory inv, int index) {
		if (inv.hasHotbar()) {
			InventoryDetectionData data = InventoryDetector.getDetector().getData(inv);
			ItemStack stack = inv.getHotbar().getHotbarSlot(index);

			if (!stack.getName().Equals("Air")) {
				String nameToDisplay = stack.getName();
				if (stack.hasItemMeta()) {
					if (stack.getItemMeta().hasDisplayName()) {
						nameToDisplay = stack.getItemMeta().getDisplayName();
					}
				}
				data.hbItemInfoCallback = UnityEngine.Random.Range(0, 9999999);
				inv.getHotbar().getHbItemInfoText().text = nameToDisplay;

				StartCoroutine(hideHotbarItemInfo(inv.getName(), data.hbItemInfoCallback, hbInfoTextTime));
			} else {
				data.hbItemInfoCallback = -1;
				inv.getHotbar().getHbItemInfoText().text = "";
			}
		}
	}
	#endregion

	#region Public functionals
	/// <summary>
	/// Displays the hover item text.
	/// If in debug mode prints to be "No item here"
	/// </summary>
	/// <param name="item">The itemstack to display.</param>
	public void displayText(ItemStack item) {
		if (item.getInventory() != null) {
			if (!InventoryManager.getManager().debug) {
				if (!item.getName().ToLower().Equals("air")) {
					String nameToDisplay = item.getName();
					if (item.hasItemMeta()) {
						ItemMeta meta = item.getItemMeta();
						if (meta.hasDisplayName()) {
							nameToDisplay = item.getItemMeta().getDisplayName();
						}
						if (meta.hasLore()) {
							foreach (string loreText in meta.getLore()) {
								nameToDisplay = nameToDisplay + Environment.NewLine + loreText;
							}
						}
					}
				    
					InventoryDetectionData data = InventoryDetector.getDetector().getData(item.getInventory());
					data.hoverDisplayText.color = invItemHoverColor;
					data.hoverDisplayText.font = inventoryFont;
					data.hoverDisplayText.text = nameToDisplay;
				}
			} else {
				String nameToDisplay;
				nameToDisplay = !item.getName().Equals("Air") ? item.getName() : "No item here";
				if (item.hasItemMeta()) {
					ItemMeta meta = item.getItemMeta();
					if (meta.hasDisplayName()) {
						nameToDisplay = item.getItemMeta().getDisplayName();
					}
					if (meta.hasLore()) {
						foreach (string loreText in meta.getLore()) {
							nameToDisplay = nameToDisplay + Environment.NewLine + loreText;
						}
					}
				}

				InventoryDetectionData data = InventoryDetector.getDetector().getData(item.getInventory());
				data.hoverDisplayText.color = invItemHoverColor;
				data.hoverDisplayText.font = inventoryFont;
				data.hoverDisplayText.text = nameToDisplay;
			}
		}
	}

	/// <summary>
	/// Moves the display text to the current mouse position
	/// **TIP** This is called automatically and can be ignored.
	/// </summary>
	public void moveDisplayTextToMouse() {
		foreach (Inventory inv in InventoryManager.getManager().getAllInventories()) {
			if (inv.isOpen()) {
				if (InventoryDetector.getDetector().hasData(inv)) {
					InventoryDetectionData data = InventoryDetector.getDetector().getData(inv);
					Vector3 offset = hoverTextOffset * InventoryManager.getManager().invCanvas.scaleFactor;
					Vector3 newPos = new Vector3(Input.mousePosition.x + offset.x, Input.mousePosition.y + offset.y, 0);

					data.hoverDisplayText.transform.position = newPos;
				}
			}
		}
	}

	/// <summary>
	/// Hides the display text.
	/// </summary>
	public void hideDisplayText() {
		foreach (Inventory inv in InventoryManager.getManager().getAllInventories()) {
			if (InventoryDetector.getDetector().hasData(inv)) {
				InventoryDetectionData data = InventoryDetector.getDetector().getData(inv);
				if (data.hover == null) {
					data.hoverDisplayText.text = "";
				}
			}
		}
	}
	#endregion

	#region Private functionals
	static IEnumerator hideHotbarItemInfo(String invName, int callBack, float time) {
		yield return new WaitForSeconds(time);
		if (InventoryManager.getManager().inventoryExists(invName)) {
			Inventory inv = InventoryManager.getManager().getInventory(invName);
			if (inv.hasHotbar()) {
				InventoryDetectionData data = InventoryDetector.getDetector().getData(inv);
				if (data.hbItemInfoCallback == callBack) {
					//Hide the display
					data.hbItemInfoCallback = -1;
					inv.getHotbar().getHbItemInfoText().text = "";
				}
			}
		}
	}
	#endregion

	#region Getters
	/// <summary>
	/// Gets the sprite for the specified item.
	/// If the item does not have an image, it will return the missing texture image to avoid errors.
	/// </summary>
	/// <returns>The image for the specified item.</returns>
	/// <param name="itemName">The name of the item.</param>
	public Sprite getImageForItem(string itemName) {
		if (!InventoryManager.getManager().debug) {
			Sprite image = Resources.Load<Sprite>("Items/" + itemName);
			return image ?? Resources.Load<Sprite>("Items/MissingTexture");
		} else {
			return Resources.Load<Sprite>("Items/AirDebug");
		}
	}
	#endregion

	#region Event Listeners
	/// <summary>
	/// Hides the hotbar selection on inventory open.
	/// </summary>
	/// <param name="inv">The inventory that was opened.</param>
	static void hideHotbarSelectionOnOpen(Inventory inv) {
		if (inv.hasHotbar()) {
			InventoryDetectionData data = InventoryDetector.getDetector().getData(inv);
			data.hotbarSelectionVisual.gameObject.SetActive(false);
			if (inv.hasOwner()) {
				inv.getOwner().setItemHolding(null);
			}
		}
	}

	/// <summary>
	/// Shows the hotbar selection on inventory close.
	/// </summary>
	/// <param name="inv">The inventory that was closed.</param>
	void showHotbarSelectionOnClose(Inventory inv) {
		if (inv.hasHotbar()) {
			InventoryDetectionData data = InventoryDetector.getDetector().getData(inv);
			data.hotbarSelectionVisual.gameObject.SetActive(true);
			if (inv.hasOwner()) {
				displayHotbarItem(inv.getOwner());
			}
		}
	}

	/// <summary>
	/// Changes the hotbar selection visual to the correct slot location.
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="slotIndex">The new slot's index.</param>
	void changeHotbarSelection(Inventory inv, int slotIndex) {
		if (inv.getHotbar().isDisplayed()) {
			InventoryDetectionData data = InventoryDetector.getDetector().getData(inv);
			if (slotIndex != data.hotbarSelectionNumber) {
				data.hotbarSelectionNumber = slotIndex;
				Vector3 newPos = inv.getItem(slotIndex).getItemImage().transform.position;
				data.hotbarSelectionVisual.transform.position = newPos;
			}
		}
		displayHotbarItem(inv.getOwner());
	}

	/// <summary>
	/// Changes the holding item on inventory update.
	/// </summary>
	/// <param name="inv">The inventory that was updated.</param>
	void changeHoldingOnInventoryUpdate(Inventory inv) {
		if (inv.hasOwner() && inv.hasHotbar() && inv.getHotbar().isDisplayed()) {
			displayHotbarItem(inv.getOwner());
		}
	}

	/// <summary>
	/// Changes the holding item on item drop.
	/// </summary>
	/// <param name="inv">The inventory involved.</param>
	/// <param name="itemName">The name of the item that was dropped.</param>
	void changeHoldingOnItemDrop(Inventory inv, String itemName) {
		if (inv.hasOwner() && inv.hasHotbar() && inv.getHotbar().isDisplayed()) {
			displayHotbarItem(inv.getOwner());
		}
	}

	/// <summary>
	/// Hides the hotbar info text on inventory open.
	/// </summary>
	/// <param name="inv">The inventory that was opened.</param>
	static void hideHotbarInfoTextOnInventoryOpen(Inventory inv) {
		if (inv.hasHotbar()) {
			InventoryDetectionData data = InventoryDetector.getDetector().getData(inv);
			inv.getHotbar().getHbItemInfoText().text = "";
			data.hbItemInfoCallback = -1;
		}
	}
	#endregion
}
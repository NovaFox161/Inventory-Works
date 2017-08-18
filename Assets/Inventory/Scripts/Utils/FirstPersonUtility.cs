using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections.Generic;

[AddComponentMenu("Inventory/Utils/First Person Unity Asset")]
// Analysis disable once ConvertToStaticType
/// <summary>
/// First person controller utility class for handling the FirstPersonController in Unity.
/// </summary>
public class FirstPersonUtility : MonoBehaviour {
	static readonly SortedDictionary<string, InventoryViewer> freezedViewers = new SortedDictionary<string, InventoryViewer>();

	#region Unity methods
	void Start() {
		InventoryEvents.getEvents().onInventoryOpenEvent += freezeCharacter;
		InventoryEvents.getEvents().onInventoryCloseEvent += unfreezeCharacter;
	}
	#endregion

	#region Functionals
	/// <summary>
	/// Freezes the character.
	/// </summary>
	/// <param name="inv">The inventory to have the viewer frozen</param>
	void freezeCharacter(Inventory inv) {
		if (inv.getViewer() != null) {
			if (inv.getViewer().acceptInput) {
				if (inv.getViewer().gameObject.GetComponent<FirstPersonController>() != null) {
					inv.getViewer().gameObject.GetComponent<FirstPersonController>().enabled = false;
					if (freezedViewers.ContainsKey(inv.getName())) {
						freezedViewers.Remove(inv.getName());
					}
					freezedViewers.Add(inv.getName(), inv.getViewer());
				}
			}
		}
	}

	/// <summary>
	/// Unfreezes the character.
	/// </summary>
	/// <param name="inv">The inventory to have the viewer unfrozen</param>
	void unfreezeCharacter(Inventory inv) {
		InventoryViewer viewer;
		if (freezedViewers.ContainsKey(inv.getName())) {
			freezedViewers.TryGetValue(inv.getName(), out viewer);
			if (viewer.acceptInput) {
				viewer.gameObject.GetComponent<FirstPersonController>().enabled = true;
			}
		}
	}
	#endregion
}

# Editor-Essentials

Just a bunch of neat editor niceties:
* AssetCreatorWindow: A window to easily search for and create custom ScriptableObject based assets.
* NonNullableAttribute: An attribute for serialized fields (that are object references). Comes with a test to ensure that your scenes, ScriptableObject assets, and prefabs aren't missing unassigned objects! Automatically crawls collections.
* LockWindowUtility: Context menu items for creating new locked instances of the Inspector and ProjectBrowser windows based on your current selection. Useful for multi-object editing workflows.

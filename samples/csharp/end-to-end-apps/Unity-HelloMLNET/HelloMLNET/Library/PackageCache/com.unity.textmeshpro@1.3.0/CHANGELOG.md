# Changelog
These are the release notes for the TextMesh Pro UPM package which was first introduced with Unity 2018.1. Please see the following link for the Release Notes for prior versions of TextMesh Pro. http://digitalnativestudios.com/forum/index.php?topic=1363.0

## [1.3.0] - 2018-08-09
### Changes
- Revamped UI to conform to Unity Human Interface Guidelines.
- Updated the title text on the Font Asset Creator window tab to "Font Asset Creator".
- Using TMP_Text.SetCharArray() with an empty char[] array will now clear the text.
- Made a small improvement to the TMP Input Field when using nested 2d RectMasks.
- Renamed symbol defines used by TMP to append TMP_ in front of the define to avoid potential conflicts with user defines.
- Improved the Project Files GUID Remapping tool to allow specifying a target folder to scan.
- Added the ability to cancel the scanning process used by the Project Files GUID Remapping tool.
- Moved TMP Settings to universal settings window in 2018.3 and above.
- Changing style sheet in the TMP Settings will now be reflected automatically on existing text objects in the editor.
- Added new function TMP_StyleSheet.UpdateStyleSheet() to update the internal reference to which style sheet text objects should be using in conjunction with the style tag.

## [1.2.4] - 2018-06-10
### Changes
- Fixed a minor issue when using Justified and Flush alignment in conjunction with \u00A0.
- The Font Asset creationSettings field is no longer an Editor only serialized field.

## [1.2.3] - 2018-05-29
### Changes
- Added new bitmap shader with support for Custom Font Atlas texture. This shader also includes a new property "Padding" to provide control over the geometry padding to closely fit a modified / custom font atlas texture.
- Fixed an issue with ForceMeshUpdate(bool ignoreActiveState) not being handled correctly.
- Cleaned up memory allocations from repeated use of the Font Asset Creator.
- Sprites are now scaled based on the current font instead of the primary font asset assigned to the text object.
- It is now possible to recall the most recent settings used when creating a font asset in the Font Asset Creator.
- Newly created font assets now contain the settings used when they were last created. This will make the process of updating / regenerating font assets much easier.
- New context menu "Update Font Asset" was added to the Font Asset inspector which will open the Font Asset Creator with the most recently used settings for that font asset.
- New Context Menu "Create Font Asset" was added to the Font inspector panel which will open the Font Asset Creator with this source font file already selected.
- Fixed 3 compiler warnings that would appear when using .Net 4.x.
- Modified the TMP Settings to place the Missing Glyph options in their own section.
- Renamed a symbol used for internal debugging to avoid potential conflicts with other user project defines.
- TMP Sprite Importer "Create Sprite Asset" and "Save Sprite Asset" options are disabled unless a Sprite Data Source, Import Format and Sprite Texture Atlas are provided.
- Improved the performance of the Project Files GUID Remapping tool.
- Users will now be prompted to import the TMP Essential Resources when using the Font Asset Creator if such resources have not already been imported.

## [1.2.2] - 2018-03-28
### Changes
- Calling SetAllDirty() on a TMP text component will now force a regeneration of the text object including re-parsing of the text.
- Fixed potential Null Reference Exception that could occur when assigning a new fallback font asset.
- Removed public from test classes.
- Fixed an issue where using nested links (which doesn't make sense conceptually) would result in an error. Should accidental use of nested links occurs, the last / most nested ends up being used.
- Fixed a potential text alignment issue where an hyphen at the end of a line followed by a new line containing a single word too long to fit the text container would result in miss alignment of the hyphen.
- Updated package license.
- Non-Breaking Space character (0xA0) will now be excluded from word spacing adjustments when using Justified or Flush text alignment.
- Improved handling of Underline, Strikethrough and Mark tag with regards to vertex color and Color tag alpha.
- Improved TMP_FontAsset.HasCharacter(char character, bool searchFallbacks) to include a recursive search of fallbacks as well as TMP Settings fallback list and default font asset.
- The &ltgradient&gt tag will now also apply to sprites provided the sprite tint attribute is set to a value of 1. Ex. &ltsprite="Sprite Asset" index=0 tint=1&gt.
- Updated Font Asset Creator Plugin to allow for cancellation of the font asset generation process.
- Added callback to support the Scriptable Render Pipeline (SRP) with the normal TextMeshPro component.
- Improved handling of some non-breaking space characters which should not be ignored at the end of a line.
- Sprite Asset fallbacks will now be searched when using the &ltsprite&gt tag and referencing a sprite by Unicode or by Name.
- Updated EmojiOne samples from https://www.emojione.com/ and added attribution.
- Removed the 32bit versions of the TMP Plugins used by the Font Asset Creator since the Unity Editor is now only available as 64bit.
- The isTextTruncated property is now serialized.
- Added new event handler to the TMP_TextEventHandler.cs script included in Example 12a to allow tracking of interactions with Sprites.

## [1.2.1] - 2018-02-14
### Changes
- Package is now backwards compatible with Unity 2018.1.
- Renamed Assembly Definitions (.asmdef) to new UPM package conventions.
- Added DisplayName for TMP UPM package.
- Revised Editor and Playmode tests to ignore / skip over the tests if the required resources are not present in the project.
- Revised implementation of Font Asset Creator progress bar to use Unity's EditorGUI.ProgressBar instead of custom texture.
- Fixed an issue where using the material tag in conjunction with fallback font assets was not handled correctly.
- Fixed an issue where changing the fontStyle property in conjunction with using alternative typefaces / font weights would not correctly trigger a regeneration of the text object.

## [1.2.0] - 2018-01-23
### Changes
- Package version # increased to 1.2.0 which is the first release for Unity 2018.2.

## [1.1.0] - 2018-01-23
### Changes
- Package version # increased to 1.1.0 which is the first release for Unity 2018.1. 

## [1.0.27] - 2018-01-16
### Changes
- Fixed an issue where setting the TMP_InputField.text property to null would result in an error.
- Fixed issue with Raycast Target state not getting serialized properly when saving / reloading a scene.
- Changed reference to PrefabUtility.GetPrefabParent() to PrefabUtility.GetCorrespondingObjectFromSource() to reflect public API change in 2018.2
- Option to import package essential resources will only be presented to users when accessing a TMP component or the TMP Settings file via the project menu.

## [1.0.26] - 2018-01-10
### Added
- Removed Tizen player references in the TMP_InputField as the Tizen player is no longer supported as of Unity 2018.1.

## [1.0.25] - 2018-01-05
### Added
- Fixed a minor issue with PreferredValues calculation in conjunction with using text auto-sizing.
- Improved Kerning handling where it is now possible to define positional adjustments for the first and second glyph in the pair.
- Renamed Kerning Info Table to Glyph Adjustment Table to better reflect the added functionality of this table.
- Added Search toolbar to the Glyph Adjustment Table.
- Fixed incorrect detection / handling of Asset Serialization mode in the Project Conversion Utility.
- Removed SelectionBase attribute from TMP components.
- Revised TMP Shaders to support the new UNITY_UI_CLIP_RECT shader keyword which can provide a performance improvement of up to 30% on some devices.
- Added TMP_PRESENT define as per the request of several third party asset publishers.

## [1.0.23] - 2017-11-14
### Added
- New menu option added to Import Examples and additional content like Font Assets, Materials Presets, etc for TextMesh Pro. This new menu option is located in "Window -> TextMeshPro -> Import Examples and Extra Content".
- New menu option added to Convert existing project files and assets created with either the Source Code or DLL only version of TextMesh Pro. Please be sure to backup your project before using this option. The new menu option is located in "Window -> TextMeshPro -> Project Files GUID Remapping Tool".
- Added Assembly Definitions for the TMP Runtime and Editor scripts.
- Added support for the UI DirtyLayoutCallback, DirtyVerticesCallback and DirtyMaterialCallback.
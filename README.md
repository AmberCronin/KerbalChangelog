# KerbalChangeLog
This project is meant to be a simple way for mod creators to add an ingame changelog for their users when they release a new version.  
**THIS WILL DO NOTHING ON ITS OWN**

## Adding a changelog
To add a changelog, simply create a config file (.cfg) with the following nodes and fields (as an example):
```
KERBALCHANGELOG
{
	showChangelog = True //optional but strongly recommended (will be created if non-existant)
	modName = Kerbal Changelog //required
	license = MIT //optional
	author = Benjamin Cronin //optional
  
	VERSION //Node(s) required for anything to show up
	{
		verison = 1.8 //optional but strongly recommended (will display as version D.N.E otherwise)
		versionName = Audacious Apple //optional
		CHANGE //required in order to utilize subchanges
		{
			change = Add features suggested by zer0Kerbal //required in a CHANGE node
			subchange = Add new change nodes //optional, as many as you want
			subchange = Add license field
			subchange = Add author node
			subchange = Add version naming field
		}
		change = Internal code rewrite to improve maintainability //change fields can be outside of CHANGE nodes, but order is not maintained
	}
	VERSION
	{
		version = 1.1.7
		CHANGE
		{
			change = Update for KSP 1.9
		}
		CHANGE
		{
			change = Downgrade to .NET 4.5 for compatibility
		}
	}
}
```
This will then be outputted in a changelog window that appears in the space center view the first time the user loads a game with a changelog that has the `showChangelog` set to  `True`. After this initial load, the user will no longer see the changelog for that mod until the mod creator releases a new version with the changelog cfg file's `showChangelog` field set to `True`.  

KCL will handle as many mods as have changelogs the user has installed, but please do not create multiple changelog files for a single mod. This will lead to multiple changelog pages showing up in the window, and confusion for everyone. 

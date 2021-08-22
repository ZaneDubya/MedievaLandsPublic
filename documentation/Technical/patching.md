# CommunityKit Update/Patching Process

* Note: The 330 series of builds added a brand new patcher that gives more information about the patching process, and adds a 'uninstall' option. *

Current running client application checks for updates on launch.
* If an update is available, prompt the user to download it, noting that this will require a restart.
* If user permits, download the patching app and updated files into a temporary location. The update will include a separate signed patching application (patcher).
* Launch the patcher and shut down.
  * The patcher waits for the current client application to terminate.
  * The patcher deletes the current client application from the app folder.
  * The patcher replaces copies the files in the update to the current directory.
    * If a file exists, then first it deletes the existing file, then copies the replaced file.
  * The patcher runs the newly updated client application and shuts down.
* The update is done and the newly updated client application runs!

Command line parameters to patcher: 
* First param is the process ID of the old app (which is running).
* Second is the path to the current client application.
* Third is the path to the new version.

## How does the current application / patcher application know what to download?
* The server has a manifest file that includes the required files to run.
* There are separate manifest files each platform (windows and mac).
* The manifest file is a csv and has these parameters for each file: 
  * Filename (string)
  * CRC32
  * CRC32 backwards (int)
* The client application checks the current files against the CRC32 values and platform values.
  * If it detects any new or changed files, it downloads the patcher to a temporary folder and runs it.
  * The patcher runs the same check, except it is now downloading the files.
  
## Except, easier:
* Noting that this is a relatively small program (20mb?) Might it not be smarter to just download the whole thing?
  * This might be required for a mac app bundle.

## A Revised version:

```csharp
namespace XPT.Core.Updating {
    delegate void UpdateMessageHandler(string message, int? progress);

    static class UpdateLogic {
		/*	=== Game Execution =======================================================================================
		 *	==========================================================================================================
		 *	Variables:
		 *		this-build-id: the build-id of the launched program
		 *		local-build-id: the build-id of the program in local storage.
		 *		update-build-id: the build-id of the most recent patch on the server.
		 *		minimum-build-id: the build-id of the minimum version that can invoke the patching process, fetched from the server.
		 *		lock: whether the updater/patcher is currently running. Only one copy of the updater/patcher can run at once.
		 *	Check lock:
		 *		If locked, show error message.
		 *	LOCK.
		 *	If CheckMinimumVersion:
		 *		If (RunInLocal && this-build-id < min-build-id && local-build-id < min-build-id) || (this-build-id < min-build-id):
		 *			Inform user they need to re-download, suggest delete.
		 *			EXIT.
		 *	If RunInLocal && !RunningInLocal
		 *		If local-build-id does not exist:
		 *			If (CheckForUdates && update-build-id > this-build-id)
		 *				Download updater.
		 *				Invoke updater in update package.
		 *				EXIT (UNLOCK is automatic).
		 *			Else // either do not check for updates, or this-build-id is equal to update-build-id
		 *				Copy app to local storage.
		 *		UNLOCK.
		 *		Run app in local storage.
		 *		EXIT.
		 *	If CheckForUpdates:
		 *		If update-build-id > this-build-id:
		 *			Download updater.
		 *			Invoke updater.
		 *			EXIT (UNLOCK is automatic).
		 *	Check ResourcesBehavior
		 *		DevEnvOnlyBuildAndDeploy:
		 *			Invoke LpkBuild, deploying apps to local storage.
		 *		VerifyDownloadAndDeploy:
		 *			Download manifest.
		 *			Verify against manifest.
		 *			For any out of date files:
		 *			 *	Download from server to local storage.
		 *		AlwaysDownloadAndDeploy:
		 *			Clear resources in local storage.
		 *			Download manifest.
		 *			For all files:
		 *				Download from server to local storage.
		 *	UNLOCK.
		 *	Game is ready and running!
		*/

		internal static EUpdateValue AsGame(UpdateMessageHandler onMsg, string pathLocalStorage, string pathServer, 
			bool checkMinimumVersion, bool runInLocal, bool checkForUpdates, EResourceBehavior resourceBehavior) {
			return EUpdateValue.Ready;
        }

		/*	=== Patcher Execution ====================================================================================
		 *	==========================================================================================================
		 *	Wait until unlocked.
		 *	LOCK.
		 *	Download update.
		 *	Unpack update in local storage.
		 *	Copy update to app folder in local storage.
		 *	UNLOCK.
		 *	Run app in local storage.
		 *	EXIT.
		*/

		internal static void AsUpdater() {

        }
	}
}

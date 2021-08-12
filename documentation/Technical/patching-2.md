# New patch logic

## Game Execution
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

## Updater execution
	Wait until unlocked.
	LOCK.
	Download update.
	Unpack update in local storage.
	Copy update to app folder in local storage.
	UNLOCK.
	Run app in local storage.
	EXIT.

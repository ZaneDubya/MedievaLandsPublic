# New patch logic

## Game Execution
	Check lock:
		If locked, exit.
	LOCK.
	If CheckMinimumVersion:
		Download min-build-id (on server).
		Check min-build-id against this-build-id.
		If this-build-id < min-build-id:
			Inform user they need to delete and re-download.
			EXIT (UNLOCK is automatic).
	If RunInLocal:
		Check local-build-id (build id of app in local storage) against this-build-id.
		If local-build-id does not exist, or this-build-id > local-build-id:
			Copy app to local storage.
			UNLOCK.
			Run app in local storage.
			EXIT.
	If CheckForUpdates:
		Download update-build-id (on server) and check against this-build-id.
		If update-build-id > this-build-id:
			Download updater.
			Invoke updater in update package.
			EXIT (UNLOCK is automatic).
	Check ResourcesBehavior
		DevEnvOnlyBuildAndDeply:
			Invoke LpkBuild, deploying apps to local storage.
		VerifyDownloadAndDeploy:
			Download manifest.
			Verify against manifest.
			For any out of date files:
				Download from server to local storage.
		AlwaysDownloadAndDeploy:
			Clear resources in local storage.
			Download manifest.
			For all files:
				Download from server to local storage.
	UNLOCK.
	Game is ready and running!

## Updater execution
	Wait until unlocked.
	LOCK.
	Download update.
	Unpack update in local storage.
	Copy update to app folder in local storage.
	UNLOCK.
	Run app in local storage.
	EXIT.

# CommunityKit Update/Patching Process

Current running client application checks for updates on launch.
* If an update is available, prompt the user to download it, noting that this will require a restart.
* If user permits, download and unpack the update into a temporary location. The update will include a separate signed patching application (patcher).
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
* The server has a manifest file that includes the required files to run and a CRC32 (backwards and forwards) for each file.
* The client application checks the current files against the CRC32.
  * If it detects any changes, it downloads the patcher and runs it.
  * The patcher runs the same check, except it is now downloading the files.
  
## Except, easier:
* Noting that this is a relatively small program (20mb?) Might it not be smarter to just download the whole thing?
  * This might be required for a mac app bundle.

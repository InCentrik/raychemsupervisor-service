Raychem Supervisor Transfer Service
For IMTT
Developed by InCentrik 3.8.2024


This solution is a Window service + GUI for configuring and running the deserialization of RCS trend group data for consumption by
Ignition systems.


Getting started:


A. Install the RCS Transfer Service
	1. Open command prompt as an administrator
	2. Enter command: cd PathToRCSTransferServiceFolder
	3. Enter command: IC.RCS.RCSService --install
	4. Enter command: sc start RCSTransferService


B. Open the RCS Transfer GUI
	1. Open "IC.RCS.RCSForm" exe file in the RCSTransferService folder by right clicking and running as administrator
	2. Only one GUI window is available
	3. You can only change the configuration of the service and trend groups if the service is running


C. Configure the service manually
	1. All configuration parameters in the GUI are also able to be changed manually in the service configuration file
	2. Open the "IC.RCS.RCSService" configuration file in the RCSTransferService folder
	3. Change the values as needed and save
	4. Restart the service


D. Uninstall the RCS Transfer Service
	1. Open command prompt as an administrator
	2. Enter command: sc stop RCSTransferService
	3. Enter command: cd PathToRCSTransferServiceFolder
	4. Enter command: IC.RCS.RCSService --uninstall	
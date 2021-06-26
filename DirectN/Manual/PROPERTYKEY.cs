using System;
using System.Runtime.InteropServices;

namespace DirectN
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY : IEquatable<PROPERTYKEY>
    {
        public Guid FormatId;
        public int Id;

        public PROPERTYKEY(Guid formatId, int id)
        {
            FormatId = formatId;
            Id = id;
        }

        public string Name
        {
            get
            {
                var pk = this;
                if (PSGetNameFromPropertyKey(ref pk, out var ptr) != 0 || ptr == IntPtr.Zero)
                    return FormatId.ToString("B") + " " + Id;

                var name = Marshal.PtrToStringUni(ptr);
                Marshal.FreeCoTaskMem(ptr);
                return name;
            }
        }

#if DEBUG
        public override string ToString() => Name;
#else
        public override string ToString() => FormatId.ToString("B") + " " + Id;
#endif
        public override bool Equals(object obj) => obj is PROPERTYKEY pk && Equals(pk);
        public bool Equals(PROPERTYKEY other) => other.Id == Id && other.FormatId == FormatId;
        public override int GetHashCode() => FormatId.GetHashCode() ^ Id.GetHashCode();
        public static bool operator ==(PROPERTYKEY left, PROPERTYKEY right) => left.Equals(right);
        public static bool operator !=(PROPERTYKEY left, PROPERTYKEY right) => !left.Equals(right);

        public static PROPERTYKEY Parse(string value)
        {
            if (!TryParse(value, out var pk))
                throw new ArgumentException(null, nameof(value));

            return pk;
        }

        public static bool TryParse(string value, out PROPERTYKEY pk)
        {
            pk = new PROPERTYKEY();
            if (value == null)
                return false;

            var hr = PSPropertyKeyFromString(value, ref pk);
            if (hr != 0)
                return false;

            return true;
        }

        [DllImport("propsys")]
        private static extern int PSGetNameFromPropertyKey(ref PROPERTYKEY propkey, out IntPtr ppszCanonicalName);

        [DllImport("propsys", CharSet = CharSet.Unicode)]
        private extern static int PSPropertyKeyFromString(string pszString, ref PROPERTYKEY pkey);

        // devpkey.h
        public static readonly PROPERTYKEY DEVPKEY_Device_AdditionalSoftwareRequested = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 19);
        public static readonly PROPERTYKEY DEVPKEY_Device_Address = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 30);
        public static readonly PROPERTYKEY DEVPKEY_Device_AssignedToGuest = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 24);
        public static readonly PROPERTYKEY DEVPKEY_Device_BaseContainerId = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 38);
        public static readonly PROPERTYKEY DEVPKEY_Device_BiosDeviceName = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 10);
        public static readonly PROPERTYKEY DEVPKEY_Device_BusNumber = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 23);
        public static readonly PROPERTYKEY DEVPKEY_Device_BusRelations = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 7);
        public static readonly PROPERTYKEY DEVPKEY_Device_BusReportedDeviceDesc = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 4);
        public static readonly PROPERTYKEY DEVPKEY_Device_BusTypeGuid = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 21);
        public static readonly PROPERTYKEY DEVPKEY_Device_Capabilities = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 17);
        public static readonly PROPERTYKEY DEVPKEY_Device_Characteristics = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 29);
        public static readonly PROPERTYKEY DEVPKEY_Device_Children = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 9);
        public static readonly PROPERTYKEY DEVPKEY_Device_Class = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 9);
        public static readonly PROPERTYKEY DEVPKEY_Device_ClassGuid = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 10);
        public static readonly PROPERTYKEY DEVPKEY_Device_CompatibleIds = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 4);
        public static readonly PROPERTYKEY DEVPKEY_Device_ConfigFlags = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 12);
        public static readonly PROPERTYKEY DEVPKEY_Device_ConfigurationId = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 7);
        public static readonly PROPERTYKEY DEVPKEY_Device_ContainerId = new PROPERTYKEY(new Guid("8c7ed206-3f8a-4827-b3ab-ae9e1faefc6c"), 2);
        public static readonly PROPERTYKEY DEVPKEY_Device_DebuggerSafe = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 12);
        public static readonly PROPERTYKEY DEVPKEY_Device_DependencyDependents = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 21);
        public static readonly PROPERTYKEY DEVPKEY_Device_DependencyProviders = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 20);
        public static readonly PROPERTYKEY DEVPKEY_Device_DeviceDesc = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 2);
        public static readonly PROPERTYKEY DEVPKEY_Device_DevNodeStatus = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 2);
        public static readonly PROPERTYKEY DEVPKEY_Device_DevType = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 27);
        public static readonly PROPERTYKEY DEVPKEY_Device_DHP_Rebalance_Policy = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 2);
        public static readonly PROPERTYKEY DEVPKEY_Device_Driver = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 11);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverCoInstallers = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 11);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverDate = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 2);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverDesc = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 4);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverInfPath = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 5);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverInfSection = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 6);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverInfSectionExt = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 7);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverLogoLevel = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 15);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverProblemDesc = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 11);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverPropPageProvider = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 10);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverProvider = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 9);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverRank = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 14);
        public static readonly PROPERTYKEY DEVPKEY_Device_DriverVersion = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 3);
        public static readonly PROPERTYKEY DEVPKEY_Device_EjectionRelations = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 4);
        public static readonly PROPERTYKEY DEVPKEY_Device_EnumeratorName = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 24);
        public static readonly PROPERTYKEY DEVPKEY_Device_Exclusive = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 28);
        public static readonly PROPERTYKEY DEVPKEY_Device_ExtendedAddress = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 23);
        public static readonly PROPERTYKEY DEVPKEY_Device_ExtendedConfigurationIds = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 15);
        public static readonly PROPERTYKEY DEVPKEY_Device_FirmwareDate = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 17);
        public static readonly PROPERTYKEY DEVPKEY_Device_FirmwareRevision = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 19);
        public static readonly PROPERTYKEY DEVPKEY_Device_FirmwareVersion = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 18);
        public static readonly PROPERTYKEY DEVPKEY_Device_FirstInstallDate = new PROPERTYKEY(new Guid("83da6326-97a6-4088-9453-a1923f573b29"), 101);
        public static readonly PROPERTYKEY DEVPKEY_Device_FriendlyName = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 14);
        public static readonly PROPERTYKEY DEVPKEY_Device_FriendlyNameAttributes = new PROPERTYKEY(new Guid("80d81ea6-7473-4b0c-8216-efc11a2c4c8b"), 3);
        public static readonly PROPERTYKEY DEVPKEY_Device_GenericDriverInstalled = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 18);
        public static readonly PROPERTYKEY DEVPKEY_Device_HardwareIds = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 3);
        public static readonly PROPERTYKEY DEVPKEY_Device_HasProblem = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 6);
        public static readonly PROPERTYKEY DEVPKEY_Device_InLocalMachineContainer = new PROPERTYKEY(new Guid("8c7ed206-3f8a-4827-b3ab-ae9e1faefc6c"), 4);
        public static readonly PROPERTYKEY DEVPKEY_Device_InstallDate = new PROPERTYKEY(new Guid("83da6326-97a6-4088-9453-a1923f573b29"), 100);
        public static readonly PROPERTYKEY DEVPKEY_Device_InstallState = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 36);
        public static readonly PROPERTYKEY DEVPKEY_Device_InstanceId = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 256);
        public static readonly PROPERTYKEY DEVPKEY_Device_IsAssociateableByUserAction = new PROPERTYKEY(new Guid("80d81ea6-7473-4b0c-8216-efc11a2c4c8b"), 7);
        public static readonly PROPERTYKEY DEVPKEY_Device_IsPresent = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 5);
        public static readonly PROPERTYKEY DEVPKEY_Device_IsRebootRequired = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 16);
        public static readonly PROPERTYKEY DEVPKEY_Device_LastArrivalDate = new PROPERTYKEY(new Guid("83da6326-97a6-4088-9453-a1923f573b29"), 102);
        public static readonly PROPERTYKEY DEVPKEY_Device_LastRemovalDate = new PROPERTYKEY(new Guid("83da6326-97a6-4088-9453-a1923f573b29"), 103);
        public static readonly PROPERTYKEY DEVPKEY_Device_Legacy = new PROPERTYKEY(new Guid("80497100-8c73-48b9-aad9-ce387e19c56e"), 3);
        public static readonly PROPERTYKEY DEVPKEY_Device_LegacyBusType = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 22);
        public static readonly PROPERTYKEY DEVPKEY_Device_LocationInfo = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 15);
        public static readonly PROPERTYKEY DEVPKEY_Device_LocationPaths = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 37);
        public static readonly PROPERTYKEY DEVPKEY_Device_LowerFilters = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 20);
        public static readonly PROPERTYKEY DEVPKEY_Device_Manufacturer = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 13);
        public static readonly PROPERTYKEY DEVPKEY_Device_ManufacturerAttributes = new PROPERTYKEY(new Guid("80d81ea6-7473-4b0c-8216-efc11a2c4c8b"), 4);
        public static readonly PROPERTYKEY DEVPKEY_Device_MatchingDeviceId = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 8);
        public static readonly PROPERTYKEY DEVPKEY_Device_Model = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 39);
        public static readonly PROPERTYKEY DEVPKEY_Device_ModelId = new PROPERTYKEY(new Guid("80d81ea6-7473-4b0c-8216-efc11a2c4c8b"), 2);
        public static readonly PROPERTYKEY DEVPKEY_Device_NoConnectSound = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 17);
        public static readonly PROPERTYKEY DEVPKEY_Device_Numa_Node = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 3);
        public static readonly PROPERTYKEY DEVPKEY_Device_Numa_Proximity_Domain = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 1);
        public static readonly PROPERTYKEY DEVPKEY_Device_Parent = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 8);
        public static readonly PROPERTYKEY DEVPKEY_Device_PDOName = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 16);
        public static readonly PROPERTYKEY DEVPKEY_Device_PhysicalDeviceLocation = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 9);
        public static readonly PROPERTYKEY DEVPKEY_Device_PostInstallInProgress = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 13);
        public static readonly PROPERTYKEY DEVPKEY_Device_PowerData = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 32);
        public static readonly PROPERTYKEY DEVPKEY_Device_PowerRelations = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 6);
        public static readonly PROPERTYKEY DEVPKEY_Device_PresenceNotForDevice = new PROPERTYKEY(new Guid("80d81ea6-7473-4b0c-8216-efc11a2c4c8b"), 5);
        public static readonly PROPERTYKEY DEVPKEY_Device_ProblemCode = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 3);
        public static readonly PROPERTYKEY DEVPKEY_Device_ProblemStatus = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 12);
        public static readonly PROPERTYKEY DEVPKEY_Device_RemovalPolicy = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 33);
        public static readonly PROPERTYKEY DEVPKEY_Device_RemovalPolicyDefault = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 34);
        public static readonly PROPERTYKEY DEVPKEY_Device_RemovalPolicyOverride = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 35);
        public static readonly PROPERTYKEY DEVPKEY_Device_RemovalRelations = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 5);
        public static readonly PROPERTYKEY DEVPKEY_Device_Reported = new PROPERTYKEY(new Guid("80497100-8c73-48b9-aad9-ce387e19c56e"), 2);
        public static readonly PROPERTYKEY DEVPKEY_Device_ReportedDeviceIdsHash = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 8);
        public static readonly PROPERTYKEY DEVPKEY_Device_ResourcePickerExceptions = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 13);
        public static readonly PROPERTYKEY DEVPKEY_Device_ResourcePickerTags = new PROPERTYKEY(new Guid("a8b865dd-2e3d-4094-ad97-e593a70c75d6"), 12);
        public static readonly PROPERTYKEY DEVPKEY_Device_SafeRemovalRequired = new PROPERTYKEY(new Guid("afd97640-86a3-4210-b67c-289c41aabe55"), 2);
        public static readonly PROPERTYKEY DEVPKEY_Device_SafeRemovalRequiredOverride = new PROPERTYKEY(new Guid("afd97640-86a3-4210-b67c-289c41aabe55"), 3);
        public static readonly PROPERTYKEY DEVPKEY_Device_Security = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 25);
        public static readonly PROPERTYKEY DEVPKEY_Device_SecuritySDS = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 26);
        public static readonly PROPERTYKEY DEVPKEY_Device_Service = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 6);
        public static readonly PROPERTYKEY DEVPKEY_Device_SessionId = new PROPERTYKEY(new Guid("83da6326-97a6-4088-9453-a1923f573b29"), 6);
        public static readonly PROPERTYKEY DEVPKEY_Device_ShowInUninstallUI = new PROPERTYKEY(new Guid("80d81ea6-7473-4b0c-8216-efc11a2c4c8b"), 8);
        public static readonly PROPERTYKEY DEVPKEY_Device_Siblings = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 10);
        public static readonly PROPERTYKEY DEVPKEY_Device_SignalStrength = new PROPERTYKEY(new Guid("80d81ea6-7473-4b0c-8216-efc11a2c4c8b"), 6);
        public static readonly PROPERTYKEY DEVPKEY_Device_SoftRestartSupported = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 22);
        public static readonly PROPERTYKEY DEVPKEY_Device_Stack = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 14);
        public static readonly PROPERTYKEY DEVPKEY_Device_TransportRelations = new PROPERTYKEY(new Guid("4340a6c5-93fa-4706-972c-7b648008a5a7"), 11);
        public static readonly PROPERTYKEY DEVPKEY_Device_UINumber = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 18);
        public static readonly PROPERTYKEY DEVPKEY_Device_UINumberDescFormat = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 31);
        public static readonly PROPERTYKEY DEVPKEY_Device_UpperFilters = new PROPERTYKEY(new Guid("a45c254e-df1c-4efd-8020-67d146a850e0"), 19);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_Characteristics = new PROPERTYKEY(new Guid("4321918b-f69e-470d-a5de-4d88c75ad24b"), 29);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_ClassCoInstallers = new PROPERTYKEY(new Guid("713d1703-a2e2-49f5-9214-56472ef3da5c"), 2);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_ClassInstaller = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 5);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_ClassName = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 3);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_DefaultService = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 11);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_DevType = new PROPERTYKEY(new Guid("4321918b-f69e-470d-a5de-4d88c75ad24b"), 27);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_DHPRebalanceOptOut = new PROPERTYKEY(new Guid("d14d3ef3-66cf-4ba2-9d38-0ddb37ab4701"), 2);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_Exclusive = new PROPERTYKEY(new Guid("4321918b-f69e-470d-a5de-4d88c75ad24b"), 28);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_Icon = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 4);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_IconPath = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 12);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_LowerFilters = new PROPERTYKEY(new Guid("4321918b-f69e-470d-a5de-4d88c75ad24b"), 20);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_Name = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 2);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_NoDisplayClass = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 8);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_NoInstallClass = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 7);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_NoUseClass = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 10);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_PropPageProvider = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 6);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_Security = new PROPERTYKEY(new Guid("4321918b-f69e-470d-a5de-4d88c75ad24b"), 25);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_SecuritySDS = new PROPERTYKEY(new Guid("4321918b-f69e-470d-a5de-4d88c75ad24b"), 26);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_SilentInstall = new PROPERTYKEY(new Guid("259abffc-50a7-47ce-af08-68c9a7d73366"), 9);
        public static readonly PROPERTYKEY DEVPKEY_DeviceClass_UpperFilters = new PROPERTYKEY(new Guid("4321918b-f69e-470d-a5de-4d88c75ad24b"), 19);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Address = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 51);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_AlwaysShowDeviceAsConnected = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 101);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_AssociationArray = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 80);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_BaselineExperienceId = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 78);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Category = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 90);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Category_Desc_Plural = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 92);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Category_Desc_Singular = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 91);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Category_Icon = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 93);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_CategoryGroup_Desc = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 94);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_CategoryGroup_Icon = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 95);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_ConfigFlags = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 105);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_CustomPrivilegedPackageFamilyNames = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 107);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_DeviceDescription1 = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 81);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_DeviceDescription2 = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 82);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_DeviceFunctionSubRank = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 100);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_DiscoveryMethod = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 52);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_ExperienceId = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 89);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_FriendlyName = new PROPERTYKEY(new Guid("656a3bb3-ecc0-43fd-8477-4ae0404a96cd"), 12288);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_HasProblem = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 83);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Icon = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 57);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_InstallInProgress = new PROPERTYKEY(new Guid("83da6326-97a6-4088-9453-a1923f573b29"), 9);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsAuthenticated = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 54);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsConnected = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 55);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsDefaultDevice = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 86);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsDeviceUniquelyIdentifiable = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 79);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsEncrypted = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 53);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsLocalMachine = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 70);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsMetadataSearchInProgress = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 72);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsNetworkDevice = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 85);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsNotInterestingForDisplay = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 74);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsPaired = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 56);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsRebootRequired = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 108);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsSharedDevice = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 84);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_IsShowInDisconnectedState = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 68);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Last_Connected = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 67);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Last_Seen = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 66);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_LaunchDeviceStageFromExplorer = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 77);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_LaunchDeviceStageOnDeviceConnect = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 76);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Manufacturer = new PROPERTYKEY(new Guid("656a3bb3-ecc0-43fd-8477-4ae0404a96cd"), 8192);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_MetadataCabinet = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 87);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_MetadataChecksum = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 73);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_MetadataPath = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 71);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_ModelName = new PROPERTYKEY(new Guid("656a3bb3-ecc0-43fd-8477-4ae0404a96cd"), 8194);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_ModelNumber = new PROPERTYKEY(new Guid("656a3bb3-ecc0-43fd-8477-4ae0404a96cd"), 8195);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_PrimaryCategory = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 97);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_PrivilegedPackageFamilyNames = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 106);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_RequiresPairingElevation = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 88);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_RequiresUninstallElevation = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 99);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_UnpairUninstall = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 98);
        public static readonly PROPERTYKEY DEVPKEY_DeviceContainer_Version = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 65);
        public static readonly PROPERTYKEY DEVPKEY_DeviceDisplay_AlwaysShowDeviceAsConnected = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 101);
        public static readonly PROPERTYKEY DEVPKEY_DeviceDisplay_Category = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 90);
        public static readonly PROPERTYKEY DEVPKEY_DeviceDisplay_DiscoveryMethod = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 52);
        public static readonly PROPERTYKEY DEVPKEY_DeviceDisplay_IsNetworkDevice = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 85);
        public static readonly PROPERTYKEY DEVPKEY_DeviceDisplay_IsNotInterestingForDisplay = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 74);
        public static readonly PROPERTYKEY DEVPKEY_DeviceDisplay_IsShowInDisconnectedState = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 68);
        public static readonly PROPERTYKEY DEVPKEY_DeviceDisplay_RequiresUninstallElevation = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 99);
        public static readonly PROPERTYKEY DEVPKEY_DeviceDisplay_UnpairUninstall = new PROPERTYKEY(new Guid("78c34fc8-104a-4aca-9ea4-524d52996e57"), 98);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterface_ClassGuid = new PROPERTYKEY(new Guid("026e516e-b814-414b-83cd-856d6fef4822"), 4);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterface_Enabled = new PROPERTYKEY(new Guid("026e516e-b814-414b-83cd-856d6fef4822"), 3);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterface_FriendlyName = new PROPERTYKEY(new Guid("026e516e-b814-414b-83cd-856d6fef4822"), 2);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterface_ReferenceString = new PROPERTYKEY(new Guid("026e516e-b814-414b-83cd-856d6fef4822"), 5);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterface_Restricted = new PROPERTYKEY(new Guid("026e516e-b814-414b-83cd-856d6fef4822"), 6);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterface_SchematicName = new PROPERTYKEY(new Guid("026e516e-b814-414b-83cd-856d6fef4822"), 9);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterface_UnrestrictedAppCapabilities = new PROPERTYKEY(new Guid("026e516e-b814-414b-83cd-856d6fef4822"), 8);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterfaceClass_DefaultInterface = new PROPERTYKEY(new Guid("14c83a99-0b3f-44b7-be4c-a178d3990564"), 2);
        public static readonly PROPERTYKEY DEVPKEY_DeviceInterfaceClass_Name = new PROPERTYKEY(new Guid("14c83a99-0b3f-44b7-be4c-a178d3990564"), 3);
        public static readonly PROPERTYKEY DEVPKEY_DevQuery_ObjectType = new PROPERTYKEY(new Guid("13673f42-a3d6-49f6-b4da-ae46e0c5237c"), 2);
        public static readonly PROPERTYKEY DEVPKEY_DrvPkg_BrandingIcon = new PROPERTYKEY(new Guid("cf73bb51-3abf-44a2-85e0-9a3dc7a12132"), 7);
        public static readonly PROPERTYKEY DEVPKEY_DrvPkg_DetailedDescription = new PROPERTYKEY(new Guid("cf73bb51-3abf-44a2-85e0-9a3dc7a12132"), 4);
        public static readonly PROPERTYKEY DEVPKEY_DrvPkg_DocumentationLink = new PROPERTYKEY(new Guid("cf73bb51-3abf-44a2-85e0-9a3dc7a12132"), 5);
        public static readonly PROPERTYKEY DEVPKEY_DrvPkg_Icon = new PROPERTYKEY(new Guid("cf73bb51-3abf-44a2-85e0-9a3dc7a12132"), 6);
        public static readonly PROPERTYKEY DEVPKEY_DrvPkg_Model = new PROPERTYKEY(new Guid("cf73bb51-3abf-44a2-85e0-9a3dc7a12132"), 2);
        public static readonly PROPERTYKEY DEVPKEY_DrvPkg_VendorWebSite = new PROPERTYKEY(new Guid("cf73bb51-3abf-44a2-85e0-9a3dc7a12132"), 3);
        public static readonly PROPERTYKEY DEVPKEY_NAME = new PROPERTYKEY(new Guid("b725f130-47ef-101a-a5f1-02608c9eebac"), 10);
        public static readonly PROPERTYKEY DEVPKEY_Numa_Proximity_Domain = new PROPERTYKEY(new Guid("540b947e-8b40-45bc-a8a2-6a0b894cbda2"), 1);
    }
}

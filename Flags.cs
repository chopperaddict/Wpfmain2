//#define SHOWFLAGS

//using System;

//using GenericSqlLib. ViewModels;

using System;
using System . Collections . Generic;
using System . Windows;
using System . Windows . Controls;
using System . Windows . Media;

namespace Wpfmain
{
    public static class Flags
    {

        // GenericSqlLib version 
        public static Dictionary<string , string> ConnectionStringsDict = new Dictionary<string , string>();

        public static bool USESDAPPERSTDPROCEDURES = false;
        public static bool USEADOWITHSTOREDPROCEDURES = true;
        public static bool USEDAPPERWITHSTOREDPROCEDURE = false;
        public static bool GETMULTIACCOUNTS = false;
        public static bool USECOPYDATA = false;
        public static string COPYBANKDATANAME = "NewBank";
        public static string COPYCUSTDATANAME = "NewCust";
        public static string COPYDETDATANAME = "NewDet";
        // Controls whether we use the comon Collection View or not in data viewers of all types
        public static bool UseSharedView
        {
            get; set;
        }

        public static bool SqlGridSwitchingActive = false;
        public static bool SqlBankActive = false;
        public static bool SqlCustActive = false;
        public static bool SqlDetActive = false;
        public static string DbDomain { get; set; } = "IAN1";
        public static DataGrid SqlBankGrid;// = null;
        public static DataGrid CurrentEditDbViewerBankGrid;// = null;
        public static Window NwSelectionWindow;

        //public static Datagrids dataGrids;
        //// Pointers to our data collections
        //public static DetCollection DetCollection = null;
        //public static AllCustomers CustCollection = null;
        public static object DbData = null;
        public static string DbSaveJsonPath = "";
        public static string CurrentConnectionString
        {
            get; set;
        }

        public static bool UseMagnify
        {
            get; set;
        }

        //-------------------------------------------------------------------
        // FlowDoc flags
        public static bool UseFlowdoc
        {
            get; set;
        }
        public static bool UseScrollView
        {
            get; set;
        }
        public static bool UseFlowScrollbar
        {
            get; set;
        }
        public static double FlowdocCrMultplier
        {
            get; set;
        }
        public static bool PinToBorder
        {
            get; set;
        }
        public static bool IsFlowDocActive
        {
            get; set;
        }
        //-------------------------------------------------------------------
        // Datagrid Options (Generic  types)
        public static bool ReplaceFldNames = true;
        //-------------------------------------------------------------------
        public static bool SqlDataChanged = false;
        public static bool EditDbDataChanged = false;
        // system wide flags to avoid selection change processing when we are loading/Reloading FULL DATA in SqlDbViewer
        public static bool DataLoadIngInProgress = false;
        public static bool UpdateInProgress = false;


        //public static RunSearchPaths ExecuteViewer
        //{
        //    get; set;
        //}
        public static string SingleSearchPath
        {
            get; set;
        }

        public static bool EditDbChangeHandled = false;

        public static bool IsFiltered = false;
        public static string FilterCommand = "";

        //Control CW output of event handlers
        public static bool EventHandlerDebug = false;
        public static bool IsMultiMode = false;

        public static bool LinkviewerRecords = false;

        public static bool UseBeeps = true;


        // Set default sort to Custno + Bankno
        public static int SortOrderRequested = 0;
        public enum SortOrderEnum
        {
            DEFAULT = 0,
            ID,
            BANKNO,
            CUSTNO,
            ACTYPE,
            DOB,
            ODATE,
            CDATE
        }
        public static bool AddDictionaryEntry<TKey, TValue> (this IDictionary<TKey , TValue> dictionary , TKey key , TValue value)
        {

            if ( dictionary == null )
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            if ( !dictionary . ContainsKey(key) )
            {
                dictionary . Add(key , value);
                return true;
            }

            return false;
        }

        public static T GetChildOfType<T> (this DependencyObject depObj) where T : DependencyObject
        {
            if ( depObj == null )
                return null;

            for ( int i = 0 ; i < VisualTreeHelper . GetChildrenCount(depObj) ; i++ )
            {
                var child = VisualTreeHelper . GetChild(depObj , i);

                var result = ( child as T ) ?? GetChildOfType<T>(child);
                if ( result != null )
                    return result;
            }
            return null;
        }
    }
}

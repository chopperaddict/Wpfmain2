//using NewWpfDev. Models;
using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Threading . Tasks;
using System . Windows;

using Dapper;

using Microsoft . Data . SqlClient;

using Views;

using Wpfmain;
using Wpfmain . Dapper;

namespace ViewModels
{
	//===========================
	//CUSTOMER VIEW MODEL CLASS
	//===========================
	[Serializable]
    public partial class CustomerViewModel//: INotifyPropertyChanged
    {
        #region CONSTRUCTORS

        //==================
        // BASIC CONSTRUCTOR
        //==================
        public CustomerViewModel ()
        {
        }

        #endregion CONSTRUCTORS
        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged (string propertyName)
        {
            //PropertyChanged?.Invoke ( this, new PropertyChangedEventArgs ( propertyName ) );
            if ( Flags . SqlCustActive == false )
                //				this . VerifyPropertyName ( propertyName );

                if ( this . PropertyChanged != null )
                {
                    var e = new PropertyChangedEventArgs(propertyName);
                    this . PropertyChanged(this , e);
                }
        }
        /// <summary>
        /// Warns the developer if this object does not have
        /// a public property with the specified name. This
        /// method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public virtual void VerifyPropertyName (string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if ( TypeDescriptor . GetProperties(this) [ propertyName ] == null )
            {
                string msg = "Invalid property name: " + propertyName;

                if ( this . ThrowOnInvalidPropertyName )
                    throw new Exception(msg);
                else
                    Debug . Fail(msg);
            }
        }

        /// <summary>
        /// Returns whether an exception is thrown, or if a Debug.Fail() is used
        /// when an invalid property name is passed to the VerifyPropertyName method.
        /// The default value is false, but subclasses used by unit tests might
        /// override this property's getter to return true.
        /// </summary>
        protected virtual bool ThrowOnInvalidPropertyName
        {
            get; private set;
        }

        #endregion PropertyChanged


        #region PRIVATE Variables declarations

        private int id;
        private string custno;
        private string bankno;
        private int actype;
        private string fname;
        private string lname;
        private string addr1;
        private string addr2;
        private string town;
        private string county;
        private string pcode;
        private string phone;
        private string mobile;
        private DateTime dob;
        private DateTime odate;
        private DateTime cdate;

        //		private static bool loaded = false;

        //public bool FilterResult = false;
        //public bool isMultiMode = false;

        #endregion PRIVATE Variables declarations

        #region PROPERTY SETTERS

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value; OnPropertyChanged(Id . ToString());
            }
        }

        public string CustNo
        {
            get
            {
                return custno;
            }
            set
            {
                custno = value; OnPropertyChanged(CustNo . ToString());
            }
        }

        public string BankNo
        {
            get
            {
                return bankno;
            }
            set
            {
                bankno = value; OnPropertyChanged(BankNo . ToString());
            }
        }

        public int AcType
        {
            get
            {
                return actype;
            }
            set
            {
                actype = value; OnPropertyChanged(AcType . ToString());
            }
        }

        public string FName
        {
            get
            {
                return fname;
            }
            set
            {
                fname = value; OnPropertyChanged(FName . ToString());
            }
        }

        public string LName
        {
            get
            {
                return lname;
            }
            set
            {
                lname = value; OnPropertyChanged(LName . ToString());
            }
        }

        public string Addr1
        {
            get
            {
                return addr1;
            }
            set
            {
                addr1 = value; OnPropertyChanged(Addr1 . ToString());
            }
        }

        public string Addr2
        {
            get
            {
                return addr2;
            }
            set
            {
                addr2 = value; OnPropertyChanged(Addr2 . ToString());
            }
        }

        public string Town
        {
            get
            {
                return town;
            }
            set
            {
                town = value; OnPropertyChanged(Town . ToString());
            }
        }

        public string County
        {
            get
            {
                return county;
            }
            set
            {
                county = value; OnPropertyChanged(County . ToString());
            }
        }

        public string PCode
        {
            get
            {
                return pcode;
            }
            set
            {
                pcode = value; OnPropertyChanged(PCode . ToString());
            }
        }

        public string Phone
        {
            get
            {
                return phone;
            }
            set
            {
                phone = value; OnPropertyChanged(Phone . ToString());
            }
        }

        public string Mobile
        {
            get
            {
                return mobile;
            }
            set
            {
                mobile = value; OnPropertyChanged(Mobile . ToString());
            }
        }

        public DateTime Dob
        {
            get
            {
                return dob;
            }
            set
            {
                dob = value; OnPropertyChanged(Dob . ToString());
            }
        }

        public DateTime ODate
        {
            get
            {
                return odate;
            }
            set
            {
                odate = value; OnPropertyChanged(ODate . ToString());
            }
        }

        public DateTime CDate
        {
            get
            {
                return cdate;
            }
            set
            {
                cdate = value; OnPropertyChanged(CDate . ToString());
            }
        }

        #endregion PROPERTY SETTERS

        #region PUBLIC & STATIC DECLARATIONS

        public static BankAccountViewModel bvm = MainWindow . bvm;
        public static CustomerViewModel cvm = MainWindow . cvm;
        public static DetailsViewModel dvm = MainWindow . dvm;

        #endregion PUBLIC & STATIC DECLARATIONS

        #region  DAPPER data methods for BankAccount
        public static List<CustomerViewModel> GetCustDataWithDictAsList (string SqlCommand = "" , bool Notify = false)
        {
            List<CustomerViewModel> cvmlist = new List<CustomerViewModel>();
            string ConString = Flags . CurrentConnectionString;
            if ( ConString == "" )
            {
                DapperSupport. CheckDbDomain("IAN1");
                ConString = Flags . CurrentConnectionString;
            }
            using ( IDbConnection db = new SqlConnection(ConString) )
            {
                try
                {
                    if ( SqlCommand == "" )
                        cvmlist = db . Query<CustomerViewModel>("Select * From Customer") . ToList();
                    else
                        cvmlist = db . Query<CustomerViewModel>(SqlCommand) . ToList();
                }
                catch ( Exception Ex )
                {
                    Debug. WriteLine($"GETCUSTDATAASLIST: DAPPER data load error - {Ex . Message}, {Ex . Data}");
                }
                if ( Notify )
                {
                    //					collection = bvmcollection;
                    //EventControl . TriggerCustListDataLoaded ( null ,
                    //	new LoadedEventArgs
                    //	{
                    //		CallerType = "SQLSERVER" ,
                    //		CallerDb = "" ,
                    //		DataSource = cvmlist ,
                    //		RowCount = cvmlist . Count
                    //	} );
                }
                return cvmlist;
            }
        }
        public ObservableCollection<CustomerViewModel> GetCustObsCollection (ObservableCollection<CustomerViewModel> collection , string SqlCommand = "" , bool Notify = false , string Caller = "")
        {
            ObservableCollection<CustomerViewModel> cvmcollection = new ObservableCollection<CustomerViewModel>();
            cvmcollection = collection;
            List<CustomerViewModel> cvmlist = new List<CustomerViewModel>();
            string ConString = Flags . CurrentConnectionString;
            if ( ConString == "" )
            {
                DapperSupport . CheckDbDomain("IAN1");
                ConString = Flags . CurrentConnectionString;
            }
            using ( IDbConnection db = new SqlConnection(ConString) )
            {
                try
                {
                    if ( SqlCommand == "" )
                        cvmlist = db . Query<CustomerViewModel>("Select * From Customer") . ToList();
                    else
                        cvmlist = db . Query<CustomerViewModel>(SqlCommand) . ToList();

                    if ( cvmlist . Count > 0 )
                    {
                        foreach ( var item in cvmlist )
                        {
                            cvmcollection . Add(item);
                        }
                    }
                }
                catch ( Exception ex )
                {
                    Debug. WriteLine($"SQL DAPPER error : {ex . Message}, {ex . Data}");
                }
            }
            if ( Notify )
            {
                collection = cvmcollection;
                Application . Current . Dispatcher . Invoke(() =>
                    EventControl . TriggerCustDataLoaded(null ,
                    new LoadedEventArgs
                    {
                        CallerType = "SQLSERVER" ,
                        CallerDb = Caller ,
                        DataSource = collection ,
                        RowCount = collection . Count
                    })
                    );
            }
            return cvmcollection;
        }
        public static ObservableCollection<CustomerViewModel> GetCustObsCollectionWithDict (ObservableCollection<CustomerViewModel> collection , string SqlCommand = "" , bool Notify = false , string Caller = "")
        {
            //object  Bankcollection = new object();
            ObservableCollection<CustomerViewModel> cvmcollection = new ObservableCollection<CustomerViewModel>();
            cvmcollection = collection;
            IDictionary<int , string> CustDict = new Dictionary<int , string>();
            List<CustomerViewModel> cvmlist = new List<CustomerViewModel>();
            string ConString = Flags . CurrentConnectionString;
            if ( ConString == "" )
            {
                DapperSupport . CheckDbDomain("IAN1");
                ConString = Flags . CurrentConnectionString;
            }
            using ( IDbConnection db = new SqlConnection(ConString) )
            {
                try
                {
                    if ( SqlCommand == "" )
                        cvmlist = db . Query<CustomerViewModel>("Select * From Customer order by CustNo") . ToList();
                    else
                        cvmlist = db . Query<CustomerViewModel>(SqlCommand) . ToList();// as ObservableCollection<BankAccountViewModel>;

                    if ( cvmlist . Count > 0 )
                    {
                        foreach ( var item in cvmlist )
                        {
                            cvmcollection . Add(item);
                            //Debug. WriteLine ( $"SQL DAPPER Dictionary : Adding {item . LName} + {item.FName} " );
                            //if ( CustDict . ContainsKey ( item . LName ) == false )
                            //	CustDict . Add ( int . Parse ( item . BankNo ) , item . LName. ToString ( ) );
                        }
                    }
                    //					Debug. WriteLine ( $"SQL DAPPER has loaded : {cvmcollection . Count} Customer  Records" );
                }
                catch ( Exception ex )
                {
                    Debug. WriteLine($"SQL DAPPER error : {ex . Message}, {ex . Data}");
                }
            }
            if ( Notify )
            {
                collection = cvmcollection;
                Application . Current . Dispatcher . Invoke(() =>
            EventControl . TriggerCustDataLoaded(null ,
                    new LoadedEventArgs
                    {
                        CallerType = "SQLSERVER" ,
                        CallerDb = Caller ,
                        DataSource = cvmcollection ,
                        RowCount = cvmcollection . Count
                    })
            );
            }
            return cvmcollection;
        }

        public static async Task<ObservableCollection<CustomerViewModel>> GetCustDataAsObsCollectionAsync (ObservableCollection<CustomerViewModel> collection , string SqlCommand = "" , bool Notify = false , string Caller = "")
        {
            ObservableCollection<CustomerViewModel> cvmcollection = collection;
            List<BankAccountViewModel> bvmlist = new List<BankAccountViewModel>();
            string ConString = ( string )Wpfmain . Properties . Settings . Default [ "BankSysConnectionString" ];

            using ( IDbConnection db = new SqlConnection(ConString) )
            {
                try
                {
                    if ( SqlCommand == "" )
                        cvmcollection = await db . QueryAsync<CustomerViewModel>("Select * From Customer") . ConfigureAwait(false) as ObservableCollection<CustomerViewModel>;
                    else
                        cvmcollection = await db . QueryAsync<CustomerViewModel>(SqlCommand) . ConfigureAwait(false) as ObservableCollection<CustomerViewModel>;
                }
                catch ( Exception ex )
                {
                    Debug. WriteLine($"SQL DAPPER error : {ex . Message}, {ex . Data}");
                }
            }
            if ( Notify )
            {
                collection = cvmcollection;
                EventControl . TriggerBankDataLoaded(null ,
                    new LoadedEventArgs
                    {
                        CallerType = "SQLSERVER" ,
                        CallerDb = Caller ,
                        DataSource = cvmcollection ,
                        RowCount = cvmcollection . Count
                    });
            }
            return cvmcollection;

            #endregion  DAPPER data methods for BankAccount

            //}
        }
    }
}
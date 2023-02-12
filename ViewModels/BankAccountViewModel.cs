
using System;
using System . Collections . Generic;
using System . Collections . ObjectModel;
using System . ComponentModel;
using System . Data;
using System . Diagnostics;
using System . Linq;
using System . Threading . Tasks;
using System . Windows . Controls;
using System . Windows . Data;

using Dapper;

using Microsoft . Data . SqlClient;

using Views;

using Wpfmain;

namespace ViewModels
{
	[Serializable]
    public partial class BankAccountViewModel
    {
        private  ObservableCollection<BankAccountViewModel> BankCollection { get; set; } = new ObservableCollection<BankAccountViewModel> ( );
        private CollectionView BankCollectionView { get; set; }//; = new CollectionView ( CollectionViewSource . GetDefaultView ( BankCollection ) );

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged ( string propertyName )
        {
            if ( Flags . SqlBankActive == false )
                //				this . VerifyPropertyName ( propertyName );

                if ( this . PropertyChanged != null )
                {
                    var e = new PropertyChangedEventArgs ( propertyName );
                    this . PropertyChanged ( this , e );
                }
        }
        #endregion PropertyChanged

        #region CONSTRUCTOR
        public BankAccountViewModel ( )
        {

        }
        //bvm = this;
        //			BankCollectionView = CollectionViewSource . GetDefaultView ( BankViewObservableCollection );
        //BindingOperations . EnableCollectionSynchronization ( BankCollectionView , _lock );

        #endregion CONSTRUCTOR
        public  void loaddb ( )
        {
            BankAccountViewModel bv = new BankAccountViewModel ( );
            if ( BankCollection . Count == 0 )
            {
                BankCollection = GetBankAccounts ( BankCollection , "" , false , "" );
                BankCollectionView = new CollectionView ( CollectionViewSource . GetDefaultView ( BankCollection ) );
                BankAccountViewModel bm = new BankAccountViewModel ( );

                // How to add a filter to a CollectionView
                Predicate<BankAccountViewModel> addfilter1 = delegate ( BankAccountViewModel s ) { return s . AcType == 1; };
                Predicate<BankAccountViewModel> addfilter2 = delegate ( BankAccountViewModel s ) { return s . AcType == 2; };
                BankCollectionView . Filter += new Predicate<object> ( o => addfilter1 ( o as BankAccountViewModel ) );
//                BankCollectionView . Filter += new Predicate<object> ( o => addfilter2 ( o as BankAccountViewModel ) );
            }
        }
        //public  bool addfilter1 <int>(BankAccountViewModel bvm )
        //{

        //    return bvm.AcType != 4;
        //}



        //        public static DataGrid ActiveEditDbViewer = null;

        //BankAccountViewModel bvm;
        #region STANDARD CLASS PROPERTIES SETUP

        private int id;
        private string bankno;
        private string custno;
        private int actype;
        private decimal balance;
        private decimal intrate;
        private DateTime odate;
        private DateTime cdate;
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                OnPropertyChanged ( Id . ToString ( ) );
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
                bankno = value;
                OnPropertyChanged ( BankNo );
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
                custno = value;
                OnPropertyChanged ( CustNo );
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
                actype = value;
                OnPropertyChanged ( AcType . ToString ( ) );
            }
        }

        public decimal Balance
        {
            get
            {
                return balance;
            }

            set
            {
                balance = value;
                OnPropertyChanged ( Balance . ToString ( ) );
            }
        }

        public decimal IntRate
        {
            get
            {
                return intrate;
            }
            set
            {
                intrate = value;
                OnPropertyChanged ( IntRate . ToString ( ) );
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
                odate = value;
                OnPropertyChanged ( ODate . ToString ( ) );
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
                cdate = value;
                OnPropertyChanged ( CDate . ToString ( ) );
            }
        }

        //        public static CollectionView BankCollectionView { get => bankCollectionView; set => bankCollectionView =  value ; }

        //public string ToString ( bool full = false )
        //{
        //    return base . ToString ( );
        //}
        //public override string ToString ( )
        //{
        ////    return base . ToString ( );
        //}
        #endregion STANDARD CLASS PROPERTIES SETUP

        public ObservableCollection<BankAccountViewModel> GetBankAccounts ( ObservableCollection<BankAccountViewModel> collection ,
           string SqlCommand = "" , bool Notify = false , string Caller = "" )
        {
            object Bankcollection = new object ( );
            ObservableCollection<BankAccountViewModel> bvmcollection = new ObservableCollection<BankAccountViewModel> ( );
            //			bvmcollection = collection;
            //			IDictionary <int, string> BankDict = new Dictionary<int, string>();
            List<BankAccountViewModel> bvmlist = new List<BankAccountViewModel> ( );

            string ConString = ( string ) Wpfmain . Properties . Settings . Default [ "BankSysConnectionString" ];
            using ( IDbConnection db = new SqlConnection(ConString) )
            {
                try
                {
                    if ( SqlCommand == "" )
                        bvmlist = db . Query<BankAccountViewModel>("Select * From BankAccount") . ToList();
                    else
                        bvmlist = db . Query<BankAccountViewModel>(SqlCommand) . ToList();

                    if ( bvmlist . Count > 0 )
                    {
                        foreach ( var item in bvmlist )
                        {
                            bvmcollection . Add(item);
                        }
                        collection = bvmcollection;
                    }
                    if ( Notify )
                    {
                        EventControl . TriggerBankDataLoaded(null ,
                            new LoadedEventArgs
                            {
                                CallerType = "BANKACCOUNTVIEWMODEL" ,
                                CallerDb = Caller ,
                                DataSource = collection ,
                                RowCount = collection . Count
                            });
                    }
                }
                catch ( Exception ex )
                {
                    Debug. WriteLine($"SQL DAPPER error : {ex . Message}, {ex . Data}");
                }
            }
            return bvmcollection;
        }
        // NOT USED
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public static async Task<ObservableCollection<BankAccountViewModel>> GetBankDataAsObsCollectionAsync ( ObservableCollection<BankAccountViewModel> collection , string SqlCommand = "" , bool Notify = true , string Caller = "" )
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            //			//			object  Bankcollection = new object();
            ObservableCollection<BankAccountViewModel> bvmcollection = collection;
//TODO
//			List<BankAccountViewModel> bvmlist = new List<BankAccountViewModel>();
            //string ConString = ( string ) Properties . Settings . Default [ "BankSysConnectionString" ];

            //using ( IDbConnection db = new SqlConnection ( ConString ) )
            //{
            //    try
            //    {
            //        if ( SqlCommand == "" )
            //            bvmcollection = await db . QueryAsync<BankAccountViewModel> ( "Select * From BankAccount" ) . ConfigureAwait ( false ) as ObservableCollection<BankAccountViewModel>;
            //        else
            //            bvmcollection = await db . QueryAsync<BankAccountViewModel> ( SqlCommand ) . ConfigureAwait ( false ) as ObservableCollection<BankAccountViewModel>;
            //    }
            //    catch ( Exception ex )
            //    {
            //        Debug. WriteLine ( $"SQL DAPPER error : {ex . Message}, {ex . Data}" );
            //    }
            //}
            //if ( Notify )
            //{
            //    collection = bvmcollection;
            //    EventControl . TriggerBankDataLoaded ( null ,
            //        new LoadedEventArgs
            //        {
            //            CallerType = "SQLSERVER" ,
            //            CallerDb = Caller ,
            //            DataSource = bvmcollection ,
            //            RowCount = bvmcollection . Count
            //        } );
            //}
            return bvmcollection;

        }

        public static int FindMatchingIndex ( BankAccountViewModel bvm , DataGrid dgrid )
        {
            int reslt = -1, indx = 0;
            BankAccountViewModel bv2 = new BankAccountViewModel ( );
            foreach ( BankAccountViewModel item in dgrid . Items )
            {
                bv2 = item as BankAccountViewModel;
                if ( item . CustNo == bvm . CustNo && item . BankNo == bvm . BankNo )
                { reslt = indx; break; }
            }
            return reslt;
        }
    }
}

/*
 *
 #if USETASK
			{
				int? taskid = Task.CurrentId;
				DateTime start = DateTime.Now;
				Task<bool> DataLoader = FillBankAccountDataGrid ();
				DataLoader.ContinueWith
				(
					task =>
					{
						LoadBankAccountIntoList (dtBank);
					},
					TaskScheduler.FromCurrentSynchronizationContext ()
				);
				Console.WriteLine ($"Completed AWAITED task to load BankAccount  Data via Sql\n" +
					$"task =Id is [ {taskid}], Completed status  [{DataLoader.IsCompleted}] in {(DateTime.Now - start)} Ticks\n");
			}
#else
			{
* */

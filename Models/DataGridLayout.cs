using System;
using System . Collections . Generic;
using System . ComponentModel;
using System . Linq;
using System . Text;
using System . Threading . Tasks;

namespace Wpfmain . Models
{
	[Serializable]
	public class DataGridLayout
    {
        #region OnPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged ( string PropertyName )
        {
            if ( this . PropertyChanged != null )
            {
                var e = new PropertyChangedEventArgs(PropertyName);
                this . PropertyChanged ( this , e );
            }
        }

        #endregion OnPropertyChanged
        public DataGridLayout ( )
        {
        }

        public string fieldname;
        public string fieldtype;
        public int fieldlength;
        public int fielddec;
        public int fieldpart;
		public object datavalue;
        public string Fieldname
        {
            get
            {
                return fieldname;
            }
            set
            {
                fieldname = value; OnPropertyChanged ( nameof ( Fieldname ) );
            }
        }
        public string Fieldtype
        {
            get
            {
                return ( string ) fieldtype;
            }
            set
            {
                fieldtype = value; OnPropertyChanged ( nameof ( Fieldtype ) );
            }
        }
        public int Fieldlength
        {
            get
            {
                return fieldlength;
            }
            set
            {
                fieldlength = value; OnPropertyChanged ( nameof ( Fieldlength ) );
            }
        }
        public int Fielddec
        {
            get
            {
                return fielddec;
            }
            set
            {
                fielddec = value; OnPropertyChanged ( nameof ( Fielddec ) );
            }
        }
        public int Fieldpart
        {
            get
            {
                return fieldpart;
            }
            set
            {
                fieldpart = value; OnPropertyChanged ( nameof ( fieldpart ) );
            }
        }
        public object DataValue
        {
            get
            {
                return ( object ) datavalue;
            }
            set
            {
                datavalue = value; OnPropertyChanged ( nameof ( DataValue ) );
            }
        }
    }

}

using System;
using System.Collections.ObjectModel;
using System.Linq;
using FileBotPP.Helpers;

namespace FileBotPP.Tree
{
    public class DirectoryItem : Item, IDirectoryItem
    {
        private IFsPoller _fsPoller;

        public DirectoryItem() : base( true )
        {
            this.ItemItems = new ObservableCollection< IItem >();
            this.ItemSuggestedName = "";
            this.Parent?.Update();
        }

        public override ObservableCollection< IItem > Items
        {
            get { return this.ItemItems; }
            set
            {
                this.ItemItems = value;
                this.Parent?.Update();
                this.OnPropertyChanged( "Items" );
            }
        }

        public override bool Duplicate { get; set; }
        public override string ShortName { get; set; }
        public override string Extension { get; set; }
        public override string NewPath { get; set; }
        public override bool NewPathExists { get; set; }

        public override bool Empty
        {
            get { return this.Items.Count == 0 || this.Items.Any( item => item.Empty ); }
            set { }
        }

        public override bool AllowedType
        {
            get { return this.Items.All( item => item.AllowedType ); }
            set { }
        }

        public override bool Missing
        {
            get { return this.Items.Any( item => item.Missing ); }
            set
            {
                this.ItemMissing = value;
                this.Parent?.Update();
                this.OnPropertyChanged( "Missing" );
            }
        }

        public override bool Extra
        {
            get { return this.Items.Any( item => item.Extra ); }
            set
            {
                this.ItemExtra = value;
                this.Parent?.Update();
                this.OnPropertyChanged( "Extra" );
            }
        }

        public override bool BadLocation
        {
            get { return this.Items.Any( item => item.BadLocation ); }
            set
            {
                this.ItemBadLocation = value;
                this.Parent?.Update();
                this.OnPropertyChanged( "BadLocation" );
            }
        }

        public override bool BadName
        {
            get { return this.ItemBadName || this.Items.Any( item => item.BadName ); }
            set
            {
                this.ItemBadName = value;
                this.Parent?.Update();
                this.OnPropertyChanged( "BadName" );
            }
        }

        public override bool BadQuality
        {
            get { return this.Items.Any( item => item.BadQuality ); }
            set { }
        }

        public override bool Corrupt
        {
            get { return this.Items.Any( item => item.Corrupt ); }
            set { }
        }

        public override bool Torrent
        {
            get { return this.Items.Any( item => item.Torrent ); }
            set { }
        }

        public override string SuggestedName
        {
            get
            {
                foreach ( var item in this.Items.Where( item => String.Compare( item.SuggestedName, "", StringComparison.Ordinal ) != 0 ) )
                {
                    return item.SuggestedName;
                }
                return "";
            }
            set
            {
                this.ItemSuggestedName = value;
                this.Parent?.Update();
                this.OnPropertyChanged( "SuggestedName" );
            }
        }

        public override int Count
        {
            get { return this.Items.Sum( item => item.Count ); }
        }

        public override bool Dirty
        {
            get { return this.Empty || this.Corrupt || this.AllowedType == false; }
            set { }
        }

        public bool Polling
        {
            get { return this._fsPoller != null; }
            set
            {
                if ( value )
                {
                    this._fsPoller = new FsPoller( this );
                }
                else
                {
                    this._fsPoller.remove_poller();
                    this._fsPoller.stop_poller();
                    this._fsPoller = null;
                }
            }
        }

        public override bool Rename( string newName, IDirectoryItem sender = null )
        {
            var currentPath = System.IO.Directory.GetParent( this.Path );
            var newpath = currentPath.FullName + "\\" + newName;

            if ( sender != null )
            {
                this.Path = sender.Path + "\\" + this.FullName;

                foreach ( var child in this.Items )
                {
                    child.Rename( "", this );
                }

                return true;
            }


            if ( System.IO.Directory.Exists( newpath ) )
            {
                Factory.Instance.LogLines.Enqueue( "Unable to rename, new directory name exists" );
                return false;
            }

            try
            {
                System.IO.Directory.Move( this.Path, newpath );
                this.Path = newpath;
                this.FullName = newName;

                Factory.Instance.ItemProvider.move_item( this );
                this.Polling = false;
                this.Polling = true;

                foreach ( var child in this.Items )
                {
                    child.Rename( "", this );
                }

                return true;
            }
            catch ( Exception ex )
            {
                Factory.Instance.LogLines.Enqueue( ex.Message );
                Factory.Instance.LogLines.Enqueue( ex.StackTrace );
                return false;
            }
        }

        public IFileItem ContainsFile( string name )
        {
            foreach ( var item in this.ItemItems.OfType< IFileItem >() )
            {
                if ( String.Compare( item.FullName, name, StringComparison.Ordinal ) == 0 )
                {
                    return item;
                }
            }

            return null;
        }

        public IDirectoryItem ContainsDirectory( string name )
        {
            foreach ( var item in this.ItemItems.OfType< IDirectoryItem >() )
            {
                if ( String.Compare( item.FullName, name, StringComparison.Ordinal ) == 0 )
                {
                    return item;
                }
            }

            return null;
        }

        ~DirectoryItem()
        {
            if ( this._fsPoller != null )
            {
                this._fsPoller.stop_poller();
                this._fsPoller.stop_poller();
                this._fsPoller = null;
            }
        }
    }
}
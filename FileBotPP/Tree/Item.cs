using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using FileBotPP.Helpers;
using FileBotPP.Metadata;

namespace FileBotPP.Tree
{
    public abstract class Item : IItem, INotifyPropertyChanged
    {
        protected static readonly List< string > AllowedTypes;
        protected readonly bool Directory;
        protected bool ItemAllowedType;
        protected bool ItemBadLocation;
        protected bool ItemBadName;
        protected bool ItemBadQuality;
        protected bool ItemCorrupt;
        protected bool ItemExtra;
        protected string ItemFullName;
        protected ObservableCollection< IItem > ItemItems;
        protected IMediaInfo ItemMediaInfo;
        protected bool ItemMissing;
        protected string ItemPath;
        protected string ItemSuggestedName;
        protected bool ItemTorrent;

        static Item()
        {
            AllowedTypes = Factory.Instance.Settings.AllowedTypes;
        }

        protected Item( bool directory )
        {
            this.Directory = directory;
            this.ItemAllowedType = true;
            /*
            ItemStatusMask mask = ItemStatusMask.Empty;
            mask |= ItemStatusMask.BadName;

            Console.WriteLine("{0,3} - {1:G}", (int)mask, (ItemStatusMask)mask);

            for (int val = 0; val <= 256; val++)
                Console.WriteLine("{0,3} - {1:G}", (int)val, (ItemStatusMask)val);
            */
        }

        public virtual ObservableCollection< IItem > Items { get; set; }
        public virtual string ShortName { get; set; }
        public virtual string Extension { get; set; }
        public virtual string SuggestedName { get; set; }
        public virtual string NewPath { get; set; }
        public virtual bool Torrent { get; set; }
        public virtual string TorrentLink { get; set; }
        public virtual bool NewPathExists { get; set; }
        public virtual bool Empty { get; set; }
        public virtual bool AllowedType { get; set; }
        public virtual bool Missing { get; set; }
        public virtual bool BadLocation { get; set; }
        public virtual bool BadName { get; set; }
        public virtual bool BadQuality { get; set; }
        public virtual bool Corrupt { get; set; }
        public virtual bool Duplicate { get; set; }
        public virtual bool Dirty { get; set; }
        public virtual bool Extra { get; set; }

        public virtual int Count
        {
            get { return 1; }
            set { }
        }

        public virtual IMediaInfo Mediainfo { get; set; }
        public IItem Parent { get; set; }

        public string FullName
        {
            get { return this.ItemFullName; }
            set
            {
                this.ItemFullName = value;
                this.OnPropertyChanged( "FullName" );
            }
        }

        public string Path
        {
            get { return this.ItemPath; }
            set
            {
                this.ItemPath = value;
                this.OnPropertyChanged( "Path" );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void Update()
        {
            this.OnPropertyChanged( "Items" );
            this.OnPropertyChanged( "Dirty" );
            this.OnPropertyChanged( "AllowedType" );
            this.OnPropertyChanged( "Missing" );
            this.OnPropertyChanged( "BadLocation" );
            this.OnPropertyChanged( "BadName" );
            this.OnPropertyChanged( "BadQuality" );
            this.OnPropertyChanged( "Corrupt" );
            this.OnPropertyChanged( "Empty" );
            this.OnPropertyChanged( "SuggestedName" );
            this.OnPropertyChanged( "FullName" );
            this.OnPropertyChanged( "Path" );
            this.OnPropertyChanged( "Mediainfo" );
            this.OnPropertyChanged( "Torrent" );
            this.OnPropertyChanged( "TorrentLink" );
            this.OnPropertyChanged( "Extra" );

            this.Parent?.Update();
        }

        public virtual bool Rename( string newName, IDirectoryItem sender = null )
        {
            return false;
        }

        protected void OnPropertyChanged( string propertyName )
        {
            this.PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }
    }
}
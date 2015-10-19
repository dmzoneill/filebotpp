using System;
using System.Collections.ObjectModel;
using System.Linq;
using FileBotPP.Interfaces;

namespace FileBotPP.Tree
{
    public class DirectoryItem : Item, IDirectoryItem
    {
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
    }
}
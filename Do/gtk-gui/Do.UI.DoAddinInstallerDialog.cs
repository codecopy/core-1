// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 2.0.50727.42
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Do.UI {
    
    
    public partial class DoAddinInstallerDialog {
        
        private Gtk.VBox vbox2;
        
        private Gtk.Label addinList;
        
        private Gtk.Alignment alignment1;
        
        private Gtk.VBox vbox3;
        
        private Gtk.ScrolledWindow plugin_scroll;
        
        private Gtk.ProgressBar progress_bar;
        
        private Gtk.Button button_cancel;
        
        private Gtk.Button button_ok;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget Do.UI.DoAddinInstallerDialog
            this.Name = "Do.UI.DoAddinInstallerDialog";
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            this.HasSeparator = false;
            // Internal child Do.UI.DoAddinInstallerDialog.VBox
            Gtk.VBox w1 = this.VBox;
            w1.Name = "dialog1_VBox";
            w1.BorderWidth = ((uint)(2));
            // Container child dialog1_VBox.Gtk.Box+BoxChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            // Container child vbox2.Gtk.Box+BoxChild
            this.addinList = new Gtk.Label();
            this.addinList.Name = "addinList";
            this.addinList.Xpad = 5;
            this.addinList.Ypad = 3;
            this.addinList.Xalign = 0F;
            this.addinList.Yalign = 0F;
            this.addinList.LabelProp = Mono.Unix.Catalog.GetString("<span size=\"large\"><b>Updated plugins are available!</b></span>");
            this.addinList.UseMarkup = true;
            this.vbox2.Add(this.addinList);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.vbox2[this.addinList]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.alignment1 = new Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
            this.alignment1.Name = "alignment1";
            this.alignment1.LeftPadding = ((uint)(12));
            this.alignment1.TopPadding = ((uint)(12));
            this.alignment1.RightPadding = ((uint)(12));
            this.alignment1.BottomPadding = ((uint)(24));
            // Container child alignment1.Gtk.Container+ContainerChild
            this.vbox3 = new Gtk.VBox();
            this.vbox3.Name = "vbox3";
            this.vbox3.Spacing = 6;
            // Container child vbox3.Gtk.Box+BoxChild
            this.plugin_scroll = new Gtk.ScrolledWindow();
            this.plugin_scroll.CanFocus = true;
            this.plugin_scroll.Name = "plugin_scroll";
            this.plugin_scroll.ShadowType = ((Gtk.ShadowType)(1));
            this.vbox3.Add(this.plugin_scroll);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.vbox3[this.plugin_scroll]));
            w3.Position = 0;
            // Container child vbox3.Gtk.Box+BoxChild
            this.progress_bar = new Gtk.ProgressBar();
            this.progress_bar.Name = "progress_bar";
            this.progress_bar.Ellipsize = ((Pango.EllipsizeMode)(2));
            this.vbox3.Add(this.progress_bar);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.vbox3[this.progress_bar]));
            w4.Position = 1;
            w4.Expand = false;
            w4.Fill = false;
            this.alignment1.Add(this.vbox3);
            this.vbox2.Add(this.alignment1);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(this.vbox2[this.alignment1]));
            w6.Position = 1;
            w1.Add(this.vbox2);
            Gtk.Box.BoxChild w7 = ((Gtk.Box.BoxChild)(w1[this.vbox2]));
            w7.Position = 0;
            // Internal child Do.UI.DoAddinInstallerDialog.ActionArea
            Gtk.HButtonBox w8 = this.ActionArea;
            w8.Name = "dialog1_ActionArea";
            w8.Spacing = 6;
            w8.BorderWidth = ((uint)(5));
            w8.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.button_cancel = new Gtk.Button();
            this.button_cancel.CanFocus = true;
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.UseStock = true;
            this.button_cancel.UseUnderline = true;
            this.button_cancel.Label = "gtk-cancel";
            this.AddActionWidget(this.button_cancel, -6);
            Gtk.ButtonBox.ButtonBoxChild w9 = ((Gtk.ButtonBox.ButtonBoxChild)(w8[this.button_cancel]));
            w9.Expand = false;
            w9.Fill = false;
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.button_ok = new Gtk.Button();
            this.button_ok.CanDefault = true;
            this.button_ok.CanFocus = true;
            this.button_ok.Name = "button_ok";
            this.button_ok.UseUnderline = true;
            // Container child button_ok.Gtk.Container+ContainerChild
            Gtk.Alignment w10 = new Gtk.Alignment(0.5F, 0.5F, 0F, 0F);
            // Container child GtkAlignment1.Gtk.Container+ContainerChild
            Gtk.HBox w11 = new Gtk.HBox();
            w11.Spacing = 2;
            // Container child GtkHBox1.Gtk.Container+ContainerChild
            Gtk.Image w12 = new Gtk.Image();
            w12.Pixbuf = Stetic.IconLoader.LoadIcon(this, "gtk-ok", Gtk.IconSize.Button, 16);
            w11.Add(w12);
            // Container child GtkHBox1.Gtk.Container+ContainerChild
            Gtk.Label w14 = new Gtk.Label();
            w14.LabelProp = Mono.Unix.Catalog.GetString("_Install Updates");
            w14.UseUnderline = true;
            w11.Add(w14);
            w10.Add(w11);
            this.button_ok.Add(w10);
            this.AddActionWidget(this.button_ok, -5);
            Gtk.ButtonBox.ButtonBoxChild w18 = ((Gtk.ButtonBox.ButtonBoxChild)(w8[this.button_ok]));
            w18.Position = 1;
            w18.Expand = false;
            w18.Fill = false;
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 443;
            this.DefaultHeight = 463;
            this.progress_bar.Hide();
            this.Show();
            this.button_ok.Clicked += new System.EventHandler(this.OnButtonOKClick);
        }
    }
}

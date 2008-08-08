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
    
    
    public partial class GeneralPreferencesWidget {
        
        private Gtk.Alignment alignment4;
        
        private Gtk.VBox vbox1;
        
        private Gtk.Label label1;
        
        private Gtk.Alignment alignment2;
        
        private Gtk.VBox vbox4;
        
        private Gtk.CheckButton login_check;
        
        private Gtk.CheckButton hide_check;
        
        private Gtk.CheckButton notification_check;
        
        private Gtk.Label label2;
        
        private Gtk.Alignment alignment3;
        
        private Gtk.VBox vbox3;
        
        private Gtk.HBox hbox1;
        
        private Gtk.Label label3;
        
        private Gtk.ComboBox theme_combo;
        
        private Gtk.VBox vbox2;
        
        private Gtk.CheckButton pin_check;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget Do.UI.GeneralPreferencesWidget
            Stetic.BinContainer.Attach(this);
            this.Name = "Do.UI.GeneralPreferencesWidget";
            // Container child Do.UI.GeneralPreferencesWidget.Gtk.Container+ContainerChild
            this.alignment4 = new Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
            this.alignment4.Name = "alignment4";
            this.alignment4.BorderWidth = ((uint)(10));
            // Container child alignment4.Gtk.Container+ContainerChild
            this.vbox1 = new Gtk.VBox();
            this.vbox1.Name = "vbox1";
            this.vbox1.Spacing = 6;
            // Container child vbox1.Gtk.Box+BoxChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.Xalign = 0F;
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("<b>First-launch Behavior</b>");
            this.label1.UseMarkup = true;
            this.vbox1.Add(this.label1);
            Gtk.Box.BoxChild w1 = ((Gtk.Box.BoxChild)(this.vbox1[this.label1]));
            w1.Position = 0;
            w1.Expand = false;
            w1.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.alignment2 = new Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
            this.alignment2.Name = "alignment2";
            this.alignment2.LeftPadding = ((uint)(20));
            this.alignment2.BottomPadding = ((uint)(10));
            // Container child alignment2.Gtk.Container+ContainerChild
            this.vbox4 = new Gtk.VBox();
            this.vbox4.Name = "vbox4";
            this.vbox4.Spacing = 6;
            // Container child vbox4.Gtk.Box+BoxChild
            this.login_check = new Gtk.CheckButton();
            this.login_check.CanFocus = true;
            this.login_check.Name = "login_check";
            this.login_check.Label = Mono.Unix.Catalog.GetString("Start GNOME Do at login.");
            this.login_check.DrawIndicator = true;
            this.login_check.UseUnderline = true;
            this.vbox4.Add(this.login_check);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.vbox4[this.login_check]));
            w2.Position = 0;
            w2.Expand = false;
            w2.Fill = false;
            // Container child vbox4.Gtk.Box+BoxChild
            this.hide_check = new Gtk.CheckButton();
            this.hide_check.CanFocus = true;
            this.hide_check.Name = "hide_check";
            this.hide_check.Label = Mono.Unix.Catalog.GetString("Hide window on first launch (quiet mode).");
            this.hide_check.DrawIndicator = true;
            this.hide_check.UseUnderline = true;
            this.vbox4.Add(this.hide_check);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.vbox4[this.hide_check]));
            w3.Position = 1;
            w3.Expand = false;
            w3.Fill = false;
            // Container child vbox4.Gtk.Box+BoxChild
            this.notification_check = new Gtk.CheckButton();
            this.notification_check.CanFocus = true;
            this.notification_check.Name = "notification_check";
            this.notification_check.Label = Mono.Unix.Catalog.GetString("Show notification icon");
            this.notification_check.DrawIndicator = true;
            this.notification_check.UseUnderline = true;
            this.vbox4.Add(this.notification_check);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.vbox4[this.notification_check]));
            w4.Position = 2;
            w4.Expand = false;
            w4.Fill = false;
            this.alignment2.Add(this.vbox4);
            this.vbox1.Add(this.alignment2);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(this.vbox1[this.alignment2]));
            w6.Position = 1;
            w6.Expand = false;
            w6.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.label2 = new Gtk.Label();
            this.label2.Name = "label2";
            this.label2.Xalign = 0F;
            this.label2.LabelProp = Mono.Unix.Catalog.GetString("<b>Appearance</b>");
            this.label2.UseMarkup = true;
            this.vbox1.Add(this.label2);
            Gtk.Box.BoxChild w7 = ((Gtk.Box.BoxChild)(this.vbox1[this.label2]));
            w7.Position = 2;
            w7.Expand = false;
            w7.Fill = false;
            // Container child vbox1.Gtk.Box+BoxChild
            this.alignment3 = new Gtk.Alignment(0.5F, 0.5F, 1F, 1F);
            this.alignment3.Name = "alignment3";
            this.alignment3.LeftPadding = ((uint)(20));
            // Container child alignment3.Gtk.Container+ContainerChild
            this.vbox3 = new Gtk.VBox();
            this.vbox3.Name = "vbox3";
            this.vbox3.Spacing = 6;
            // Container child vbox3.Gtk.Box+BoxChild
            this.hbox1 = new Gtk.HBox();
            this.hbox1.Name = "hbox1";
            this.hbox1.Spacing = 6;
            // Container child hbox1.Gtk.Box+BoxChild
            this.label3 = new Gtk.Label();
            this.label3.Name = "label3";
            this.label3.LabelProp = Mono.Unix.Catalog.GetString("Theme:");
            this.hbox1.Add(this.label3);
            Gtk.Box.BoxChild w8 = ((Gtk.Box.BoxChild)(this.hbox1[this.label3]));
            w8.Position = 0;
            w8.Expand = false;
            w8.Fill = false;
            // Container child hbox1.Gtk.Box+BoxChild
            this.theme_combo = Gtk.ComboBox.NewText();
            this.theme_combo.AppendText(Mono.Unix.Catalog.GetString("Classic"));
            this.theme_combo.AppendText(Mono.Unix.Catalog.GetString("Glass Frame"));
            this.theme_combo.AppendText(Mono.Unix.Catalog.GetString("Mini"));
            this.theme_combo.WidthRequest = 150;
            this.theme_combo.Name = "theme_combo";
            this.theme_combo.Active = 0;
            this.hbox1.Add(this.theme_combo);
            Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(this.hbox1[this.theme_combo]));
            w9.Position = 1;
            w9.Expand = false;
            w9.Fill = false;
            this.vbox3.Add(this.hbox1);
            Gtk.Box.BoxChild w10 = ((Gtk.Box.BoxChild)(this.vbox3[this.hbox1]));
            w10.Position = 0;
            w10.Expand = false;
            w10.Fill = false;
            // Container child vbox3.Gtk.Box+BoxChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            // Container child vbox2.Gtk.Box+BoxChild
            this.pin_check = new Gtk.CheckButton();
            this.pin_check.CanFocus = true;
            this.pin_check.Name = "pin_check";
            this.pin_check.Label = Mono.Unix.Catalog.GetString("Always show results window");
            this.pin_check.DrawIndicator = true;
            this.pin_check.UseUnderline = true;
            this.vbox2.Add(this.pin_check);
            Gtk.Box.BoxChild w11 = ((Gtk.Box.BoxChild)(this.vbox2[this.pin_check]));
            w11.Position = 0;
            w11.Expand = false;
            w11.Fill = false;
            this.vbox3.Add(this.vbox2);
            Gtk.Box.BoxChild w12 = ((Gtk.Box.BoxChild)(this.vbox3[this.vbox2]));
            w12.Position = 1;
            this.alignment3.Add(this.vbox3);
            this.vbox1.Add(this.alignment3);
            Gtk.Box.BoxChild w14 = ((Gtk.Box.BoxChild)(this.vbox1[this.alignment3]));
            w14.Position = 3;
            this.alignment4.Add(this.vbox1);
            this.Add(this.alignment4);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Show();
            this.login_check.Clicked += new System.EventHandler(this.OnLoginCheckClicked);
            this.hide_check.Clicked += new System.EventHandler(this.OnHideCheckClicked);
            this.notification_check.Clicked += new System.EventHandler(this.OnNotificationCheckClicked);
            this.theme_combo.Changed += new System.EventHandler(this.OnThemeComboChanged);
            this.pin_check.Clicked += new System.EventHandler(this.OnPinChecklicked);
        }
    }
}

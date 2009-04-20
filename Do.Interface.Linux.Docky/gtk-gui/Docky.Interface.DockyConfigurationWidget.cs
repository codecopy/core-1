// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Docky.Interface {
    
    
    public partial class DockyConfigurationWidget {
        
        private Gtk.VBox vbox2;
        
        private Gtk.HBox hbox5;
        
        private Gtk.Label orientation_label;
        
        private Gtk.ComboBox orientation_combobox;
        
        private Gtk.CheckButton autohide_checkbutton;
        
        private Gtk.CheckButton window_overlap_checkbutton;
        
        private Gtk.CheckButton zoom_checkbutton;
        
        private Gtk.CheckButton advanced_indicators_checkbutton;
        
        private Gtk.Label zoom_size_label;
        
        private Gtk.HScale zoom_scale;
        
        private Gtk.HButtonBox hbuttonbox2;
        
        private Gtk.Button configure_docklets_button;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget Docky.Interface.DockyConfigurationWidget
            Stetic.BinContainer.Attach(this);
            this.Name = "Docky.Interface.DockyConfigurationWidget";
            // Container child Docky.Interface.DockyConfigurationWidget.Gtk.Container+ContainerChild
            this.vbox2 = new Gtk.VBox();
            this.vbox2.Name = "vbox2";
            this.vbox2.Spacing = 6;
            // Container child vbox2.Gtk.Box+BoxChild
            this.hbox5 = new Gtk.HBox();
            this.hbox5.Name = "hbox5";
            this.hbox5.Spacing = 6;
            // Container child hbox5.Gtk.Box+BoxChild
            this.orientation_label = new Gtk.Label();
            this.orientation_label.Name = "orientation_label";
            this.orientation_label.LabelProp = Mono.Unix.Catalog.GetString("Orientation:");
            this.hbox5.Add(this.orientation_label);
            Gtk.Box.BoxChild w1 = ((Gtk.Box.BoxChild)(this.hbox5[this.orientation_label]));
            w1.Position = 0;
            w1.Expand = false;
            w1.Fill = false;
            // Container child hbox5.Gtk.Box+BoxChild
            this.orientation_combobox = Gtk.ComboBox.NewText();
            this.orientation_combobox.Name = "orientation_combobox";
            this.hbox5.Add(this.orientation_combobox);
            Gtk.Box.BoxChild w2 = ((Gtk.Box.BoxChild)(this.hbox5[this.orientation_combobox]));
            w2.Position = 1;
            w2.Expand = false;
            w2.Fill = false;
            this.vbox2.Add(this.hbox5);
            Gtk.Box.BoxChild w3 = ((Gtk.Box.BoxChild)(this.vbox2[this.hbox5]));
            w3.Position = 0;
            w3.Expand = false;
            w3.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.autohide_checkbutton = new Gtk.CheckButton();
            this.autohide_checkbutton.CanFocus = true;
            this.autohide_checkbutton.Name = "autohide_checkbutton";
            this.autohide_checkbutton.Label = Mono.Unix.Catalog.GetString("AutoHide");
            this.autohide_checkbutton.DrawIndicator = true;
            this.autohide_checkbutton.UseUnderline = true;
            this.vbox2.Add(this.autohide_checkbutton);
            Gtk.Box.BoxChild w4 = ((Gtk.Box.BoxChild)(this.vbox2[this.autohide_checkbutton]));
            w4.Position = 1;
            w4.Expand = false;
            w4.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.window_overlap_checkbutton = new Gtk.CheckButton();
            this.window_overlap_checkbutton.CanFocus = true;
            this.window_overlap_checkbutton.Name = "window_overlap_checkbutton";
            this.window_overlap_checkbutton.Label = Mono.Unix.Catalog.GetString("Allow Window Overlap");
            this.window_overlap_checkbutton.DrawIndicator = true;
            this.window_overlap_checkbutton.UseUnderline = true;
            this.vbox2.Add(this.window_overlap_checkbutton);
            Gtk.Box.BoxChild w5 = ((Gtk.Box.BoxChild)(this.vbox2[this.window_overlap_checkbutton]));
            w5.Position = 2;
            w5.Expand = false;
            w5.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.zoom_checkbutton = new Gtk.CheckButton();
            this.zoom_checkbutton.CanFocus = true;
            this.zoom_checkbutton.Name = "zoom_checkbutton";
            this.zoom_checkbutton.Label = Mono.Unix.Catalog.GetString("Zoom Enabled");
            this.zoom_checkbutton.DrawIndicator = true;
            this.zoom_checkbutton.UseUnderline = true;
            this.vbox2.Add(this.zoom_checkbutton);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(this.vbox2[this.zoom_checkbutton]));
            w6.Position = 3;
            w6.Expand = false;
            w6.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.advanced_indicators_checkbutton = new Gtk.CheckButton();
            this.advanced_indicators_checkbutton.CanFocus = true;
            this.advanced_indicators_checkbutton.Name = "advanced_indicators_checkbutton";
            this.advanced_indicators_checkbutton.Label = Mono.Unix.Catalog.GetString("Advanced Indicators");
            this.advanced_indicators_checkbutton.DrawIndicator = true;
            this.advanced_indicators_checkbutton.UseUnderline = true;
            this.vbox2.Add(this.advanced_indicators_checkbutton);
            Gtk.Box.BoxChild w7 = ((Gtk.Box.BoxChild)(this.vbox2[this.advanced_indicators_checkbutton]));
            w7.Position = 4;
            w7.Expand = false;
            w7.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.zoom_size_label = new Gtk.Label();
            this.zoom_size_label.Name = "zoom_size_label";
            this.zoom_size_label.LabelProp = Mono.Unix.Catalog.GetString("Zoom Size");
            this.vbox2.Add(this.zoom_size_label);
            Gtk.Box.BoxChild w8 = ((Gtk.Box.BoxChild)(this.vbox2[this.zoom_size_label]));
            w8.Position = 5;
            w8.Expand = false;
            w8.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.zoom_scale = new Gtk.HScale(null);
            this.zoom_scale.CanFocus = true;
            this.zoom_scale.Name = "zoom_scale";
            this.zoom_scale.Adjustment.Upper = 100;
            this.zoom_scale.Adjustment.PageIncrement = 10;
            this.zoom_scale.Adjustment.StepIncrement = 1;
            this.zoom_scale.DrawValue = true;
            this.zoom_scale.Digits = 0;
            this.zoom_scale.ValuePos = ((Gtk.PositionType)(2));
            this.vbox2.Add(this.zoom_scale);
            Gtk.Box.BoxChild w9 = ((Gtk.Box.BoxChild)(this.vbox2[this.zoom_scale]));
            w9.Position = 6;
            w9.Expand = false;
            w9.Fill = false;
            // Container child vbox2.Gtk.Box+BoxChild
            this.hbuttonbox2 = new Gtk.HButtonBox();
            this.hbuttonbox2.Name = "hbuttonbox2";
            this.hbuttonbox2.BorderWidth = ((uint)(3));
            // Container child hbuttonbox2.Gtk.ButtonBox+ButtonBoxChild
            this.configure_docklets_button = new Gtk.Button();
            this.configure_docklets_button.CanFocus = true;
            this.configure_docklets_button.Name = "configure_docklets_button";
            this.configure_docklets_button.UseUnderline = true;
            this.configure_docklets_button.Label = Mono.Unix.Catalog.GetString("Configure Docklets");
            this.hbuttonbox2.Add(this.configure_docklets_button);
            Gtk.ButtonBox.ButtonBoxChild w10 = ((Gtk.ButtonBox.ButtonBoxChild)(this.hbuttonbox2[this.configure_docklets_button]));
            w10.Expand = false;
            w10.Fill = false;
            this.vbox2.Add(this.hbuttonbox2);
            Gtk.Box.BoxChild w11 = ((Gtk.Box.BoxChild)(this.vbox2[this.hbuttonbox2]));
            w11.Position = 7;
            w11.Expand = false;
            w11.Fill = false;
            this.Add(this.vbox2);
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.Hide();
            this.orientation_combobox.Changed += new System.EventHandler(this.OnOrientationComboboxChanged);
            this.autohide_checkbutton.Toggled += new System.EventHandler(this.OnAutohideCheckbuttonToggled);
            this.window_overlap_checkbutton.Toggled += new System.EventHandler(this.OnWindowOverlapCheckbuttonToggled);
            this.zoom_checkbutton.Toggled += new System.EventHandler(this.OnZoomCheckbuttonToggled);
            this.advanced_indicators_checkbutton.Toggled += new System.EventHandler(this.OnAdvancedIndicatorsCheckbuttonToggled);
            this.zoom_scale.FormatValue += new Gtk.FormatValueHandler(this.OnZoomScaleFormatValue);
            this.zoom_scale.ValueChanged += new System.EventHandler(this.OnZoomScaleValueChanged);
        }
    }
}

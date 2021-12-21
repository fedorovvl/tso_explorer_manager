using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ExplorerManager
{
    /// <summary>
    /// Логика взаимодействия для Result.xaml
    /// </summary>
    public partial class Result : Window
    {
        public List<Item> Items = new List<Item>();
        public List<Item> GItems = new List<Item>();
        public List<ResultRow> Results = new List<ResultRow>();
        public int playerLevel { get; set; }
        public List<SavedItem> SaveResults = new List<SavedItem>();
        private bool selectionfix = false;

        public Result()
        {
            InitializeComponent();
            (tabs.Items[0] as TabItem).Header = Loca.locaData[Main.lang]["Explorer"];
            (tabs.Items[1] as TabItem).Header = Loca.locaData[Main.lang]["Geologist"];
            geospec.Header = Loca.locaData[Main.lang]["UnitSkillIsSpecialist"];
            explspec.Header = Loca.locaData[Main.lang]["UnitSkillIsSpecialist"];
            bcancel.Content = Loca.locaData[Main.lang]["Cancel"];
            bsend.Content = Loca.locaData[Main.lang]["Send"];
        }


        public List<ComboBoxItem> genExplCombo(Explorer expl)
        {
            List<ComboBoxItem> result = new List<ComboBoxItem>()
            {
                new ComboBoxItem() { Content = Loca.locaData[Main.lang]["Cancel"], Tag = "0" },
                new ComboBoxItem() { Content = string.Format("{0}{1}", Main.lang == "ru-ru" ? Loca.locaData[Main.lang]["FindTreasure"] + " " : "", Loca.locaData[Main.lang]["FindTreasureShort"]), Tag = "1,0" },
                new ComboBoxItem() { Content = string.Format("{0}{1}", Main.lang == "ru-ru" ? Loca.locaData[Main.lang]["FindTreasure"] + " " : "", Loca.locaData[Main.lang]["FindTreasureMedium"]), Tag = "1,1" },
                new ComboBoxItem() { Content = string.Format("{0}{1}", Main.lang == "ru-ru" ? Loca.locaData[Main.lang]["FindTreasure"] + " " : "", Loca.locaData[Main.lang]["FindTreasureLong"]), Tag = "1,2" },
                new ComboBoxItem() { Content = string.Format("{0}{1}", Main.lang == "ru-ru" ? Loca.locaData[Main.lang]["FindTreasure"] + " " : "", Loca.locaData[Main.lang]["FindTreasureEvenLonger"]), Tag = "1,3" },
                new ComboBoxItem() { Content = string.Format("{0}{1}", Main.lang == "ru-ru" ? Loca.locaData[Main.lang]["FindTreasure"] + " " : "", Loca.locaData[Main.lang]["FindTreasureLongest"]), Tag = "1,6" },
                new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindAdventureZoneShort"], Tag = "2,0" },
                new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindAdventureZoneMedium"], Tag = "2,1" },
                new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindAdventureZoneLong"], Tag = "2,2" },
                new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindAdventureZoneVeryLong"], Tag = "2,3" }
            };
            if (expl.artefact)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindTreasureTravellingErudite"], Tag = "1,4" });
            if (expl.beans)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindTreasureBeanACollada"], Tag = "1,5" });
            return result;
        }

        private void addExpl(Explorer expl, List<SavedItem> saved)
        {
            Item item = new Item()
            {
                Id = expl.id,
                Id2 = expl.id2,
                Name = expl.name,
                Combo = genExplCombo(expl)
            };
            item.Sk = string.Join(" / ", new string[2] { expl.artefact ? "a" : "-", expl.beans ? "b" : "-" });
            item.selected = saved.Any(x => x.Key == expl.id + "_" + expl.id2) ? saved.Single(x => x.Key == expl.id + "_" + expl.id2).Value : 0;
            Items.Add(item);
        }

        public List<ComboBoxItem> genGeoCombo()
        {
            List<ComboBoxItem> result = new List<ComboBoxItem>() {
                 new ComboBoxItem() { Content = Loca.locaData[Main.lang]["Cancel"], Tag = "0" },
                 new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositStone"], Tag = "0,0" }
            };
            if (playerLevel >= 9)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositBronzeOre"], Tag = "0,1" });
            if (playerLevel >= 17)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositMarble"], Tag = "0,2" });
            if (playerLevel >= 18)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositIronOre"], Tag = "0,3" });
            if (playerLevel >= 23)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositGoldOre"], Tag = "0,4" });
            if (playerLevel >= 24)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositCoal"], Tag = "0,5" });
            if (playerLevel >= 60)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositGranite"], Tag = "0,6" });
            if (playerLevel >= 61)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositAlloyOre"], Tag = "0,7" });
            if (playerLevel >= 62)
                result.Add(new ComboBoxItem() { Content = Loca.locaData[Main.lang]["FindDepositSalpeter"], Tag = "0,8" });
            return result;
        }
        private void addGeo(Explorer expl, List<SavedItem> saved)
        {
            Item item = new Item()
            {
                Id = expl.id,
                Id2 = expl.id2,
                Name = expl.name,
                Combo = genGeoCombo()
            };
            item.selected = saved.Any(x => x.Key == expl.id + "_" + expl.id2) ? saved.Single(x => x.Key == expl.id + "_" + expl.id2).Value : 0;
            GItems.Add(item);
        }

        public void setData(List<Explorer> Data, List<SavedItem> saved)
        {
            foreach(Explorer expl in Data)
            {
                if (expl.specType == 0)
                    addExpl(expl, saved);
                if(expl.specType == 1)
                    addGeo(expl, saved);
                
            }
            
            Dispatcher.BeginInvoke(new ThreadStart(delegate
            {
                SortDescription sd = new SortDescription("Name", ListSortDirection.Ascending);
                lvTable.ItemsSource = Items;
                glvTable.ItemsSource = GItems;
                ICollectionView view = CollectionViewSource.GetDefaultView(lvTable.Items);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(sd);
                view.Refresh();
                lvTable.Items.Refresh();
                view = CollectionViewSource.GetDefaultView(glvTable.Items);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(sd);
                view.Refresh();
                glvTable.Items.Refresh();
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            this.DialogResult = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            foreach(Item item in lvTable.Items)
            {
                ResultRow rItem = new ResultRow()
                {
                    id = item.Id,
                    id2 = item.Id2,
                    name = item.Name,
                    specType = 0
                };
                try
                {
                    ComboBoxItem currentItem;
                    try
                    {
                        currentItem = item.Combo.Single(x => x.IsSelected == true);
                    }
                    catch (System.Exception exx)
                    {
                        currentItem = item.Combo.ElementAt(item.selected);
                    }
                    int index = item.Combo.IndexOf(currentItem);
                    SaveResults.Add(new SavedItem() { Key = item.Id + "_" + item.Id2, Value = index, Name = item.Name });
                    string selectedTag = currentItem.Tag.ToString();
                    if (selectedTag == "0")
                        continue;
                    string[] selectedTags = selectedTag.Split(new[] { ',' });
                    rItem.type = int.Parse(selectedTags[0]);
                    rItem.task = int.Parse(selectedTags[1]);
                    rItem.taskName = currentItem.Content.ToString();
                    Results.Add(rItem);
                } catch (System.Exception ex)
                {
                    throw ex;
                }
                
            }
            foreach (Item item in glvTable.Items)
            {
                ResultRow rItem = new ResultRow()
                {
                    id = item.Id,
                    id2 = item.Id2,
                    name = item.Name,
                    specType = 1
                };
                try
                {
                    ComboBoxItem currentItem;
                    try
                    {
                        currentItem = item.Combo.Single(x => x.IsSelected == true);
                    }
                    catch (System.Exception exx)
                    {
                        currentItem = item.Combo.ElementAt(item.selected);
                    }
                    int index = item.Combo.IndexOf(currentItem);
                    SaveResults.Add(new SavedItem() { Key = item.Id + "_" + item.Id2, Value = index, Name = item.Name });
                    string selectedTag = currentItem.Tag.ToString();
                    if (selectedTag == "0")
                        continue;
                    string[] selectedTags = selectedTag.Split(new[] { ',' });
                    rItem.type = int.Parse(selectedTags[0]);
                    rItem.task = int.Parse(selectedTags[1]);
                    rItem.taskName = currentItem.Content.ToString();
                    Results.Add(rItem);
                }
                catch (System.Exception ex)
                {
                    throw ex;
                }
            }
            if (Results.Count == 0)
            {
                error.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.DialogResult = true;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = (sender as ComboBox).SelectedIndex;
            foreach (Item item in lvTable.Items)
            {
                foreach (ComboBoxItem cItem in item.Combo)
                    cItem.IsSelected = false;
                item.selected = (item.Combo.Count > index) ? index : 0;
            }
            lvTable.Items.Refresh();
        }
        private void GComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (!selectionfix)
            {
                selectionfix = true;
                return;
            }
            int index = (sender as ComboBox).SelectedIndex;
            foreach (Item item in glvTable.Items)
            {
                foreach (ComboBoxItem cItem in item.Combo)
                    cItem.IsSelected = false;
                item.selected = index;
            }
            glvTable.Items.Refresh();
        }

        private void GCombo_Initialized(object sender, System.EventArgs e)
        {
            List<ComboBoxItem> list = genGeoCombo();
            if (playerLevel < 62)
                (sender as ComboBox).Items.RemoveAt(9);
            if (playerLevel < 61)
                (sender as ComboBox).Items.RemoveAt(8);
            if (playerLevel < 60)
                (sender as ComboBox).Items.RemoveAt(7);
            if (playerLevel < 24)
                (sender as ComboBox).Items.RemoveAt(6);
            if (playerLevel < 23)
                (sender as ComboBox).Items.RemoveAt(5);
            if (playerLevel < 18)
                (sender as ComboBox).Items.RemoveAt(4);
            if (playerLevel < 17)
                (sender as ComboBox).Items.RemoveAt(3);
            if (playerLevel < 9)
                (sender as ComboBox).Items.RemoveAt(2);
            foreach(ComboBoxItem item in (sender as ComboBox).Items)
            {
                item.Content = list[(sender as ComboBox).Items.IndexOf(item)].Content;
            }
        }

        private void ComboBox_Initialized(object sender, System.EventArgs e)
        {
            List<ComboBoxItem> list = genExplCombo(new Explorer() { beans = true, artefact = true });
            foreach (ComboBoxItem item in (sender as ComboBox).Items)
            {
                item.Content = list[(sender as ComboBox).Items.IndexOf(item)].Content;
            }
        }
    }

    public class Item
    {
        public int Id { get; set; }
        public int Id2 { get; set; }
        public string Name { get; set; }
        public string Sk { get; set; }
        public List<ComboBoxItem> Combo { get; set; }
        public int selected { get; set; }
    }

    public class FixedWidthColumn : GridViewColumn
    {
        static FixedWidthColumn()
        {
            WidthProperty.OverrideMetadata(typeof(FixedWidthColumn), new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceWidth)));
        }

        public double FixedWidth
        {
            get { return (double)GetValue(FixedWidthProperty); }
            set { SetValue(FixedWidthProperty, value); }
        }

        public static readonly DependencyProperty FixedWidthProperty =

            DependencyProperty.Register(
                "FixedWidth",
                typeof(double),
                typeof(FixedWidthColumn),
                new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnFixedWidthChanged)));

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            FixedWidthColumn fwc = o as FixedWidthColumn;
            if (fwc != null)
                fwc.CoerceValue(WidthProperty);
        }

        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {
            FixedWidthColumn fwc = o as FixedWidthColumn;
            if (fwc != null)
                return fwc.FixedWidth;
            return baseValue;
        }
    }

    public class OrdinalConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {

            ListViewItem lvi = value as ListViewItem;
            int ordinal = 0;

            if (lvi != null)
            {
                ListView lv = ItemsControl.ItemsControlFromItemContainer(lvi) as ListView;
                ordinal = lv.ItemContainerGenerator.IndexFromContainer(lvi) + 1;
            }

            return ordinal;

        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.InvalidOperationException();
        }
    }
    public class ResultRow
    {
        public int id { get; set; }
        public int id2 { get; set; }
        public int type { get; set; }
        public int task { get; set; }
        public string name { get; set; }
        public string taskName { get; set; }
        public int specType { get; set; }
    }
}

using System;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace blocktemplate.BlockTemplates
{
    class IfFrame : UserControl
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Grid MainGrid = new Grid();

            Grid Grid1 = new Grid();

            ColumnDefinition colDef1 = new ColumnDefinition();
            colDef1.Width = new GridLength(39.0);
            ColumnDefinition colDef2 = new ColumnDefinition();
            ColumnDefinition colDef3 = new ColumnDefinition();
            colDef3.Width = new GridLength(7.0);
            Grid1.ColumnDefinitions.Add(colDef1);
            Grid1.ColumnDefinitions.Add(colDef2);
            Grid1.ColumnDefinitions.Add(colDef3);

            RowDefinition rowDef1 = new RowDefinition();
            rowDef1.Height = new GridLength(31.0);
            RowDefinition rowDef2 = new RowDefinition();
            RowDefinition rowDef3 = new RowDefinition();
            rowDef3.Height = new GridLength(9.0);
            Grid1.RowDefinitions.Add(rowDef1);
            Grid1.RowDefinitions.Add(rowDef2);
            Grid1.RowDefinitions.Add(rowDef3);


            Image img1 = new Image();
            RenderOptions.SetBitmapScalingMode(img1, BitmapScalingMode.NearestNeighbor);
            img1.Source = new BitmapImage(new Uri("../Resources/if_left_up.png", UriKind.Relative));
            Grid.SetRow(img1, 0);
            Grid.SetColumn(img1, 0);

            Border border1 = new Border()
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xA7, 0x10)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xB8, 0x10)),
                Margin = new Thickness(0, 0, 0, 0),
                BorderThickness = new Thickness(0, 1, 0, 0)
            };
            Grid.SetRow(border1, 0);
            Grid.SetColumn(border1, 1);

            Image img2 = new Image();
            RenderOptions.SetBitmapScalingMode(img2, BitmapScalingMode.NearestNeighbor);
            img2.Source = new BitmapImage(new Uri("../Resources/if_right_up.png", UriKind.Relative));
            Grid.SetRow(img2, 0);
            Grid.SetColumn(img2, 2);

            Border border2 = new Border()
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xA7, 0x10)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xB8, 0x10)),
                Margin = new Thickness(0, 0, 0, 0),
                BorderThickness = new Thickness(1, 0, 0, 0)
            };
            Grid.SetRow(border2, 1);
            Grid.SetColumn(border2, 0);

            Border border3 = new Border()
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xA7, 0x10)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xB8, 0x10)),
                Margin = new Thickness(0, 0, 0, 0),
                BorderThickness = new Thickness(0, 0, 0, 0)
            };
            Grid.SetRow(border3, 1);
            Grid.SetColumn(border3, 1);

            Border border4 = new Border()
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xA7, 0x10)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xB8, 0x10)),
                Margin = new Thickness(0, 0, 0, 0),
                BorderThickness = new Thickness(0, 0, 0, 0)
            };
            Grid.SetRow(border4, 1);
            Grid.SetColumn(border4, 2);

            Image img3 = new Image();
            RenderOptions.SetBitmapScalingMode(img3, BitmapScalingMode.NearestNeighbor);
            img3.Source = new BitmapImage(new Uri("../Resources/if_left_center.png", UriKind.Relative));
            Grid.SetRow(img3, 2);
            Grid.SetColumn(img3, 0);

            Border border5 = new Border()
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xA7, 0x10)),
                BorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xFF, 0xFF, 0xB8, 0x10)),
                Margin = new Thickness(0, 0, 0, 5),
                BorderThickness = new Thickness(0, 0, 0, 1)
            };
            Grid.SetRow(border5, 2);
            Grid.SetColumn(border5, 1);

            Image img4 = new Image();
            RenderOptions.SetBitmapScalingMode(img4, BitmapScalingMode.NearestNeighbor);
            img4.Source = new BitmapImage(new Uri("../Resources/if_right_center.png", UriKind.Relative));
            Grid.SetRow(img4, 2);
            Grid.SetColumn(img4, 2);


            Grid1.Children.Add(img1);
            Grid1.Children.Add(border1);
            Grid1.Children.Add(img2);
            Grid1.Children.Add(border2);
            Grid1.Children.Add(border3);
            Grid1.Children.Add(border4);
            Grid1.Children.Add(img3);
            Grid1.Children.Add(border5);
            Grid1.Children.Add(img4);

            MainGrid.Children.Add(Grid1);

            ContentPresenter contentP = new ContentPresenter
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(30, 5, 10, 10),
                Content = Content
            };
            MainGrid.Children.Add(contentP);

            Content = MainGrid;


        }
    }
}

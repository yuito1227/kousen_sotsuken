using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Threading;

using System.ComponentModel;
using System.Reflection;

using Microsoft.Win32;

namespace blocktemplate
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = new blocktemplate.MainViewModel();
            var thumb = new Thumb();
            pieces.Add(thumb);
            Set_Pieces();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Arrange_Pieces();
                Set_Combobox();
            }),
            DispatcherPriority.Loaded);
        }

        const int VERTUCALLY_DENT = 6;
        const int SIDE_DENT = 6;
        const int SIDE_LINE_DENT = 10;
        List<string> pieces_name_list = new List<string>()
        {
            //"AnalogOutPut",
            //"AnalogOutPutAsVariable",
            //"DigitalOutPut",
            "Mortor",
            "Delay",
            "SerialPrint",
            "BlueToothSerialPrint",
            //"GetWhatTimeIs",
            //"GetWhatDayIs",
            "SetVariable",
            "ToSensorOutput",
            "ToNumber",
            "ToVariable",
            "LogicVariableAndConst",
            "LogicVariableAndVariable",
            //"IfDigitalPinr",
            "IfBanper",
            "IfElseBanper",
            "IfConst",
            "IfVariable",
            "IfElseConst",
            "IfElseVariable",
            "DoOnce",
            "WhileTrue",
            "WhileConst",
            "WhileVariable",
            "ForConst",
            "ForVariable"
        };
        int error_code = 0;
        int num = 0;
        int block_count = 0;
        public List<Thumb> pieces = new List<Thumb>();
        private List<int> control_statement_IDs = new List<int>();
        private List<int> raw_piece_IDs = new List<int>();
        private List<int> copied_piece_IDs = new List<int>();
        private Dictionary<string, string> cnv_code = new Dictionary<string, string>();
        private Dictionary<string, Dictionary<string, string>> add_code = new Dictionary<string, Dictionary<string, string>>();

        private void Pieces_DragStarted(object sender, DragStartedEventArgs e)
        {
            
            if(show_work_space.IsChecked)
            {
                
                var trash_area = (Border)FindName("TrashArea");
                Border trash_area_frame = new Border()
                {
                    Name = "trash_area_frame",
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x90, 0xFF, 0xFF, 0xFF)),
                    Height = trash_area.ActualHeight - 4,
                    Width = trash_area.ActualWidth - 4
                };
                Label trash_area_label = new Label()
                {
                    Name = "trash_area_label",
                    FontSize = 30,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xCC, 0x00, 0x00, 0x00)),
                    Margin = new Thickness(0, 50, 0, 0),
                    Content = "こ\nこ\nで\nブ\nロ\nッ\nク\nを\n削\n除\nで\nき\nま\nす"
                };
                area.Children.Add(trash_area_frame);
                Canvas.SetLeft(trash_area_frame, Canvas.GetLeft(trash_area) + 2);
                Canvas.SetTop(trash_area_frame, Canvas.GetTop(trash_area) + 2);
                trash_area_frame.Child = trash_area_label;

                var copy_area = (Border)FindName("CopyArea");
                Border copy_area_frame = new Border()
                {
                    Name = "copy_area_frame",
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x90, 0xFF, 0xFF, 0xFF)),
                    Height = copy_area.ActualHeight - 4,
                    Width = copy_area.ActualWidth - 4
                };
                Label copy_area_label = new Label()
                {
                    Name = "copy_area_label",
                    FontSize = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xCC, 0x00, 0x00, 0x00)),
                    Content = "複\n製\nエ\nリ\nア"
                };
                area.Children.Add(copy_area_frame);
                Canvas.SetRight(copy_area_frame, Canvas.GetRight(copy_area) + 2);
                Canvas.SetTop(copy_area_frame, Canvas.GetTop(copy_area) + 2);
                copy_area_frame.Child = copy_area_label;

                var build_area = (Border)FindName("BuildArea");
                Border build_area_frame = new Border()
                {
                    Name = "build_area_frame",
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x90, 0xFF, 0xFF, 0xFF)),
                    Height = build_area.ActualHeight - 4,
                    Width = build_area.ActualWidth - 4
                };
                Label build_area_label = new Label()
                {
                    Name = "build_area_label",
                    FontSize = 30,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0xCC, 0x00, 0x00, 0x00)),
                    Content = "変\n換\nエ\nリ\nア"
                };
                Canvas.SetRight(build_area_frame, Canvas.GetRight(build_area)+ 2);
                Canvas.SetTop(build_area_frame, Canvas.GetTop(build_area) + 2);
                area.Children.Add(build_area_frame);
                build_area_frame.Child = build_area_label;
            }

            var selected_piece = sender as Thumb;
            double delta_y = 0;
            int selected_ID = AttachedProperty.GetID(selected_piece);
            List<int> if_temp_list = new List<int>();
            List<int> else_temp_list = new List<int>();
            List<int> temp_list = new List<int>();
            if ((string)((Thumb)selected_piece).Tag == "Copied")
            {
                string return_string = Line_Up_Piece_IDs(selected_ID);
                string[] splited_IDs = return_string.Split(',');
                var selected_pieces_list = new List<string>();
                selected_pieces_list.AddRange(splited_IDs);
                selected_pieces_list.RemoveAt(0);

                foreach (string string_ID in selected_pieces_list)
                {
                    int ID = Convert.ToInt32(string_ID);
                    copied_piece_IDs.RemoveAt(raw_piece_IDs.IndexOf(ID));
                    raw_piece_IDs.Remove(ID);
                    pieces[ID].Tag = "Normal";
                }
                
            }
                foreach (int control_statement_ID in control_statement_IDs)
            {
                if ((AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) == selected_ID || AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) == selected_ID) && AttachedProperty.GetChainPieceIDProperty(selected_piece) == 0)
                {
                    delta_y = selected_piece.ActualHeight - 41;
                    break;
                }
                else
                {
                    delta_y = selected_piece.ActualHeight - VERTUCALLY_DENT;
                }
            }
            int targeted_piece_ID = selected_ID;
            while (true)
            {
                bool flag = false;

                if (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                {
                    while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                    }
                }
                foreach (int control_statement_ID in control_statement_IDs)
                {
                    if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                    {
                        var side_line = pieces[control_statement_ID].Template.FindName("MainSideLine", pieces[control_statement_ID]) as Border;
                        side_line.Height = side_line.ActualHeight - delta_y;
                        if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) != 0)
                        {
                            int chain_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]);
                            int chained_piece_ID = 0;
                            Control_Piece("Move", chain_piece_ID, 0, -delta_y);
                            while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                            {
                                chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                chained_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                AttachedProperty.SetChainPieceIDProperty(pieces[chain_piece_ID], 0);
                                Control_Piece("Move", chain_piece_ID, 0, -delta_y);

                                AttachedProperty.SetChainPieceIDProperty(pieces[chain_piece_ID], chained_piece_ID);
                            }
                        }

                        targeted_piece_ID = control_statement_ID;
                        flag = true;
                        break;
                    }
                    else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                    {
                        var side_line = pieces[control_statement_ID].Template.FindName("SubSideLine", pieces[control_statement_ID]) as Border;
                        side_line.Height = side_line.ActualHeight - delta_y;
                        targeted_piece_ID = control_statement_ID;
                        flag = true;
                        break;
                    }
                }
                if (flag == true)
                {
                    if (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        int chain_piece_ID = targeted_piece_ID;
                        while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                        {
                            chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                            Control_Piece("Move", chain_piece_ID, 0, -delta_y);
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            if (AttachedProperty.GetOwnedPieceIDProperty(selected_piece) != 0)
            {
                int Owner_ID = AttachedProperty.GetOwnedPieceIDProperty(pieces[selected_ID]);
                AttachedProperty.SetOwnPieceIDProperty(pieces[Owner_ID], 0);
                AttachedProperty.SetOwnedPieceIDProperty(selected_piece, 0);
            }
            foreach (int control_statement_ID in control_statement_IDs)
            {
                if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) == selected_ID)
                {
                    AttachedProperty.SetMainOwnPieceIDProperty(pieces[control_statement_ID], AttachedProperty.GetChainPieceIDProperty(selected_piece));
                    int chain_piece_ID = selected_ID;
                    int chained_piece_ID = 0;
                    while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                    {
                        chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                        chained_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                        AttachedProperty.SetChainPieceIDProperty(pieces[chain_piece_ID], 0);
                        Control_Piece("Move", chain_piece_ID, 0, VERTUCALLY_DENT - selected_piece.ActualHeight);
                        AttachedProperty.SetChainPieceIDProperty(pieces[chain_piece_ID], chained_piece_ID);
                    }
                }
                if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) == selected_ID)
                {
                    AttachedProperty.SetSubOwnPieceIDProperty(pieces[control_statement_ID], AttachedProperty.GetChainPieceIDProperty(selected_piece));
                    int chain_piece_ID = selected_ID;
                    int chained_piece_ID = 0;
                    while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                    {
                        chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                        chained_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                        AttachedProperty.SetChainPieceIDProperty(pieces[chain_piece_ID], 0);
                        Control_Piece("Move", chain_piece_ID, 0, VERTUCALLY_DENT - selected_piece.ActualHeight);

                        AttachedProperty.SetChainPieceIDProperty(pieces[chain_piece_ID], chained_piece_ID);
                    }
                }
            }
            if (AttachedProperty.GetChainedPieceIDProperty(selected_piece) != 0 && AttachedProperty.GetChainPieceIDProperty(selected_piece) != 0)
            {
                int Chained_ID = AttachedProperty.GetChainedPieceIDProperty(selected_piece);
                int Chainer_ID = AttachedProperty.GetChainPieceIDProperty(selected_piece);
                int chain_piece_ID = selected_ID;
                while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                {
                    chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                    Control_Piece("Move", chain_piece_ID, 0, VERTUCALLY_DENT - selected_piece.ActualHeight);
                }
                AttachedProperty.SetChainPieceIDProperty(selected_piece, 0);
                AttachedProperty.SetChainedPieceIDProperty(selected_piece, 0);
                AttachedProperty.SetChainPieceIDProperty(pieces[Chained_ID], Chainer_ID);
                AttachedProperty.SetChainedPieceIDProperty(pieces[Chainer_ID], Chained_ID);
            }
            else if (AttachedProperty.GetChainedPieceIDProperty(selected_piece) != 0)
            {
                int Chained_ID = AttachedProperty.GetChainedPieceIDProperty(selected_piece);
                AttachedProperty.SetChainPieceIDProperty(pieces[Chained_ID], 0);
                AttachedProperty.SetChainedPieceIDProperty(selected_piece, 0);
            }
            else if (AttachedProperty.GetChainPieceIDProperty(selected_piece) != 0)
            {
                int Chainer_ID = AttachedProperty.GetChainPieceIDProperty(selected_piece);
                AttachedProperty.SetChainedPieceIDProperty(pieces[Chainer_ID], 0);
                AttachedProperty.SetChainPieceIDProperty(selected_piece, 0);
            }
            Control_Piece("Stand_Out", selected_ID, 0, 0);
        }

        private void Pieces_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (show_work_space.IsChecked)
            {
                Border trash_area_frame = (Border)LogicalTreeHelper.FindLogicalNode(area, "trash_area_frame");
                area.Children.Remove(trash_area_frame);

                Border copy_area_frame = (Border)LogicalTreeHelper.FindLogicalNode(area, "copy_area_frame");
                area.Children.Remove(copy_area_frame);

                Border build_area_frame = (Border)LogicalTreeHelper.FindLogicalNode(area, "build_area_frame");
                area.Children.Remove(build_area_frame);
            }

            List<int> temp_list = new List<int>();
            var selected_piece = sender as Thumb;
            int selected_piece_ID = AttachedProperty.GetID(selected_piece);
            double x = Canvas.GetLeft(selected_piece);
            double y = Canvas.GetTop(selected_piece);
            var canvas = selected_piece.Parent as Canvas;
            object obj = FindName("TrashArea");
            if (obj != null)
            {
                var trash_area = (Border)obj;
                double trash_width = trash_area.ActualWidth;
                double trash_x = Canvas.GetLeft(trash_area);
                if (x < (trash_x + trash_width))
                {
                    Control_Piece("Remove", selected_piece_ID, 0, 0);
                    return;
                }
            }
            obj = FindName("BuildArea");
            if (obj != null)
            {
                var build_area = (Border)obj;
                double build_x = canvas.ActualWidth - Canvas.GetRight(build_area) - build_area.ActualWidth;
                double build_y = Canvas.GetTop(build_area);
                ;
                if (x > build_x && y > build_y)
                {
                    Create_Source_Code(selected_piece_ID);
                    return;
                }
            }

            var tuple = Check_Side(selected_piece_ID);
            int another_piece_ID = tuple.Item2;
            double piece_vertical_delta = tuple.Item3;
            double piece_horizontal_delta = tuple.Item4;
            var another_piece = pieces[another_piece_ID] as Thumb;
            double delta_y = 0;
            int targeted_piece_ID = 0;
            var side_line = new Border();
            switch (tuple.Item1)
            {
                case "None":
                    obj = FindName("CopyArea");
                    if (obj != null)
                    {
                        var copy_area = (Border)obj;
                        double copy_x = canvas.ActualWidth - Canvas.GetRight(copy_area) - copy_area.ActualWidth;
                        double copy_y = Canvas.GetTop(copy_area) + copy_area.ActualHeight;

                        if (x > copy_x && y < copy_y)
                        {
                            foreach (Thumb piece in pieces)
                            {
                                if ((string)((Thumb)piece).Tag == "Copied") Control_Piece("Remove", AttachedProperty.GetID(piece), 0, 0);
                            }
                            Control_Piece("Copy", selected_piece_ID, canvas.ActualWidth - Canvas.GetRight(copy_area) - copy_area.ActualWidth + 20 - x, Canvas.GetTop(copy_area) + 20 - y);
                            return;
                        }
                    }
                    break;

                case "MainInside":
                    if ((string)((Thumb)another_piece).Tag == "Copied")
                    {
                        string return_string = Line_Up_Piece_IDs(selected_piece_ID);
                        string[] splited_IDs = return_string.Split(',');
                        var selected_pieces_list = new List<string>();
                        selected_pieces_list.AddRange(splited_IDs);
                        selected_pieces_list.RemoveAt(0);

                        foreach (string string_ID in selected_pieces_list)
                        {
                            int ID = Convert.ToInt32(string_ID);
                            var piece = pieces[ID] as Thumb;
                            var copied_piece = new Thumb();


                            piece.Tag = "Copied";

                            block_count++;
                            raw_piece_IDs.Add(ID);
                            copied_piece_IDs.Add(block_count);

                            pieces.Add(copied_piece);

                            Canvas.SetLeft(copied_piece, 2000);
                            Canvas.SetTop(copied_piece, 1000);
                            copied_piece.Template = piece.Template;
                            copied_piece.Tag = "Copied";
                            copied_piece.Name = piece.Name;
                            AttachedProperty.SetID(copied_piece, block_count);
                            if (AttachedProperty.GetType(piece) == "Control_Statement")
                            {
                                control_statement_IDs.Add(block_count);
                            }
                            area.Children.Add(copied_piece);
                        }
                    }
                    Control_Piece("Move", selected_piece_ID, -piece_horizontal_delta, -piece_vertical_delta);
                    side_line = another_piece.Template.FindName("MainSideLine", another_piece) as Border;
                    if (AttachedProperty.GetMainOwnPieceIDProperty(another_piece) != 0)
                    {

                        AttachedProperty.SetChainPieceIDProperty(selected_piece, AttachedProperty.GetMainOwnPieceIDProperty(another_piece));
                        AttachedProperty.SetChainedPieceIDProperty(pieces[AttachedProperty.GetMainOwnPieceIDProperty(another_piece)], selected_piece_ID);
                        AttachedProperty.SetMainOwnPieceIDProperty(another_piece, selected_piece_ID);
                        delta_y = selected_piece.ActualHeight - VERTUCALLY_DENT;
                        side_line.Height = side_line.ActualHeight + delta_y;
                        if (AttachedProperty.GetChainPieceIDProperty(pieces[AttachedProperty.GetMainOwnPieceIDProperty(another_piece)]) != 0)
                        {
                            targeted_piece_ID = selected_piece_ID;
                            while (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                                Control_Piece("Move", targeted_piece_ID, 0, delta_y);
                            }
                        }
                    }
                    else
                    {
                        AttachedProperty.SetMainOwnPieceIDProperty(another_piece, selected_piece_ID);
                        delta_y = selected_piece.ActualHeight - 10 - side_line.ActualHeight;
                        side_line.Height = selected_piece.ActualHeight - 10;
                        if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[another_piece_ID]) != 0)
                        {
                            int chain_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[another_piece_ID]);
                            Control_Piece("Move", chain_piece_ID, 0, delta_y);
                            while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                            {
                                chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                Control_Piece("Move", chain_piece_ID, 0, delta_y);
                            }
                        }
                    }
                    targeted_piece_ID = another_piece_ID;
                    while (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                        Control_Piece("Move", targeted_piece_ID, 0, delta_y);
                    }
                    targeted_piece_ID = another_piece_ID;
                    while (true)
                    {
                        bool flag = false;
                        while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                        }
                        foreach (int control_statement_ID in control_statement_IDs)
                        {
                            if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                            {
                                side_line = pieces[control_statement_ID].Template.FindName("MainSideLine", pieces[control_statement_ID]) as Border;
                                side_line.Height = side_line.ActualHeight + delta_y;
                                if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) != 0)
                                {
                                    int chain_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]);
                                    Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                    while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                                    {
                                        chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                        Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                    }
                                }
                                targeted_piece_ID = control_statement_ID;
                                flag = true;
                                break;
                            }
                            else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                            {
                                side_line = pieces[control_statement_ID].Template.FindName("SubSideLine", pieces[control_statement_ID]) as Border;
                                side_line.Height = side_line.ActualHeight + delta_y;
                                targeted_piece_ID = control_statement_ID;
                                flag = true;
                                break;
                            }
                        }
                        if (flag == true)
                        {
                            if (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                int chain_piece_ID = targeted_piece_ID;
                                while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                                {
                                    chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                    Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;

                case "SubInside":
                    if ((string)((Thumb)another_piece).Tag == "Copied")
                    {
                        string return_string = Line_Up_Piece_IDs(selected_piece_ID);
                        string[] splited_IDs = return_string.Split(',');
                        var selected_pieces_list = new List<string>();
                        selected_pieces_list.AddRange(splited_IDs);
                        selected_pieces_list.RemoveAt(0);

                        foreach (string string_ID in selected_pieces_list)
                        {
                            int ID = Convert.ToInt32(string_ID);
                            var piece = pieces[ID] as Thumb;
                            var copied_piece = new Thumb();


                            piece.Tag = "Copied";

                            block_count++;
                            raw_piece_IDs.Add(ID);
                            copied_piece_IDs.Add(block_count);

                            pieces.Add(copied_piece);

                            Canvas.SetLeft(copied_piece, 10000);
                            Canvas.SetTop(copied_piece, 10000);
                            copied_piece.Template = piece.Template;
                            copied_piece.Tag = "Copied";
                            copied_piece.Name = piece.Name;
                            AttachedProperty.SetID(copied_piece, block_count);
                            if (AttachedProperty.GetType(piece) == "Control_Statement")
                            {
                                control_statement_IDs.Add(block_count);
                            }
                            area.Children.Add(copied_piece);
                        }
                    }
                    Control_Piece("Move", selected_piece_ID, -piece_horizontal_delta, -piece_vertical_delta);
                    side_line = another_piece.Template.FindName("SubSideLine", another_piece) as Border;
                    if (AttachedProperty.GetSubOwnPieceIDProperty(another_piece) != 0)
                    {
                        AttachedProperty.SetChainPieceIDProperty(selected_piece, AttachedProperty.GetSubOwnPieceIDProperty(another_piece));
                        AttachedProperty.SetChainedPieceIDProperty(pieces[AttachedProperty.GetSubOwnPieceIDProperty(another_piece)], selected_piece_ID);
                        AttachedProperty.SetSubOwnPieceIDProperty(another_piece, selected_piece_ID);
                        delta_y = selected_piece.ActualHeight - VERTUCALLY_DENT;
                        side_line.Height = side_line.ActualHeight + delta_y;
                        if (AttachedProperty.GetChainPieceIDProperty(pieces[AttachedProperty.GetSubOwnPieceIDProperty(another_piece)]) != 0)
                        {
                            targeted_piece_ID = selected_piece_ID;
                            while (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                                Control_Piece("Move", targeted_piece_ID, 0, delta_y);
                            }
                        }
                    }
                    else
                    {
                        AttachedProperty.SetSubOwnPieceIDProperty(another_piece, selected_piece_ID);
                        delta_y = selected_piece.ActualHeight - 10 - side_line.ActualHeight;
                        side_line.Height = selected_piece.ActualHeight - 10;
                    }
                    targeted_piece_ID = another_piece_ID;
                    while (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                        Control_Piece("Move", targeted_piece_ID, 0, delta_y);
                    }
                    targeted_piece_ID = another_piece_ID;
                    while (true)
                    {
                        bool flag = false;
                        if (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                            }
                        }
                        foreach (int control_statement_ID in control_statement_IDs)
                        {
                            if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                            {
                                side_line = pieces[control_statement_ID].Template.FindName("MainSideLine", pieces[control_statement_ID]) as Border;
                                side_line.Height = side_line.ActualHeight + delta_y;
                                if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) != 0)
                                {
                                    int chain_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]);
                                    Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                    while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                                    {
                                        chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                        Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                    }
                                }
                                targeted_piece_ID = control_statement_ID;
                                flag = true;
                                break;
                            }
                            else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                            {
                                side_line = pieces[control_statement_ID].Template.FindName("SubSideLine", pieces[control_statement_ID]) as Border;
                                side_line.Height = side_line.ActualHeight + delta_y;
                                targeted_piece_ID = control_statement_ID;
                                flag = true;
                                break;
                            }
                        }
                        if (flag == true)
                        {
                            if (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                int chain_piece_ID = targeted_piece_ID;
                                while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                                {
                                    chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                    Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;

                case "Right":
                    if ((string)((Thumb)another_piece).Tag == "Copied")
                    {
                        string return_string = Line_Up_Piece_IDs(selected_piece_ID);
                        string[] splited_IDs = return_string.Split(',');
                        var selected_pieces_list = new List<string>();
                        selected_pieces_list.AddRange(splited_IDs);
                        selected_pieces_list.RemoveAt(0);

                        foreach (string string_ID in selected_pieces_list)
                        {
                            int ID = Convert.ToInt32(string_ID);
                            var piece = pieces[ID] as Thumb;
                            var copied_piece = new Thumb();


                            piece.Tag = "Copied";

                            block_count++;
                            raw_piece_IDs.Add(ID);
                            copied_piece_IDs.Add(block_count);

                            pieces.Add(copied_piece);

                            Canvas.SetLeft(copied_piece, 10000);
                            Canvas.SetTop(copied_piece, 10000);
                            copied_piece.Template = piece.Template;
                            copied_piece.Tag = "Copied";
                            copied_piece.Name = piece.Name;
                            AttachedProperty.SetID(copied_piece, block_count);
                            if (AttachedProperty.GetType(piece) == "Control_Statement")
                            {
                                control_statement_IDs.Add(block_count);
                            }
                            area.Children.Add(copied_piece);
                        }
                    }
                    Control_Piece("Move", selected_piece_ID, -piece_horizontal_delta, piece_vertical_delta);
                    AttachedProperty.SetOwnPieceIDProperty(another_piece, selected_piece_ID);
                    AttachedProperty.SetOwnedPieceIDProperty(selected_piece, another_piece_ID);
                    break;

                case "Left":
                    if ((string)((Thumb)another_piece).Tag == "Copied")
                    {
                        string return_string = Line_Up_Piece_IDs(selected_piece_ID);
                        string[] splited_IDs = return_string.Split(',');
                        var selected_pieces_list = new List<string>();
                        selected_pieces_list.AddRange(splited_IDs);
                        selected_pieces_list.RemoveAt(0);

                        foreach (string string_ID in selected_pieces_list)
                        {
                            int ID = Convert.ToInt32(string_ID);
                            var piece = pieces[ID] as Thumb;
                            var copied_piece = new Thumb();


                            piece.Tag = "Copied";

                            block_count++;
                            raw_piece_IDs.Add(ID);
                            copied_piece_IDs.Add(block_count);

                            pieces.Add(copied_piece);

                            Canvas.SetLeft(copied_piece, 10000);
                            Canvas.SetTop(copied_piece, 10000);
                            copied_piece.Template = piece.Template;
                            copied_piece.Tag = "Copied";
                            copied_piece.Name = piece.Name;
                            AttachedProperty.SetID(copied_piece, block_count);
                            if (AttachedProperty.GetType(piece) == "Control_Statement")
                            {
                                control_statement_IDs.Add(block_count);
                            }
                            area.Children.Add(copied_piece);
                        }
                    }
                    Control_Piece("Move", selected_piece_ID, piece_horizontal_delta, piece_vertical_delta);
                    AttachedProperty.SetOwnPieceIDProperty(selected_piece, another_piece_ID);
                    AttachedProperty.SetOwnedPieceIDProperty(another_piece, selected_piece_ID);
                    break;

                case "Bottom":
                    targeted_piece_ID = another_piece_ID;
                    bool Is_Inside = false;
                    if (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                        }
                    }
                    if ((string)((Thumb)another_piece).Tag == "Copied")
                    {
                        foreach (int control_statement_ID in control_statement_IDs)
                        {                        
                            if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID || AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                            {
                                Is_Inside = true;
                            }
                        }
                        if (Is_Inside == true)
                        {
                            string return_string = Line_Up_Piece_IDs(selected_piece_ID);
                            string[] splited_IDs = return_string.Split(',');
                            var selected_pieces_list = new List<string>();
                            selected_pieces_list.AddRange(splited_IDs);
                            selected_pieces_list.RemoveAt(0);

                            foreach (string string_ID in selected_pieces_list)
                            {
                                int ID = Convert.ToInt32(string_ID);
                                var piece = pieces[ID] as Thumb;
                                var copied_piece = new Thumb();


                                piece.Tag = "Copied";

                                block_count++;
                                raw_piece_IDs.Add(ID);
                                copied_piece_IDs.Add(block_count);

                                pieces.Add(copied_piece);

                                Canvas.SetLeft(copied_piece, 10000);
                                Canvas.SetTop(copied_piece, 10000);
                                copied_piece.Template = piece.Template;
                                copied_piece.Tag = "Copied";
                                copied_piece.Name = piece.Name;
                                AttachedProperty.SetID(copied_piece, block_count);
                                if (AttachedProperty.GetType(piece) == "Control_Statement")
                                {
                                    control_statement_IDs.Add(block_count);
                                }
                                area.Children.Add(copied_piece);
                            }
                        }
                        else
                        {
                            obj = FindName("CopyArea");
                            if (obj != null)
                            {
                                var copy_area = (Border)obj;
                                double copy_x = canvas.ActualWidth - Canvas.GetRight(copy_area) - copy_area.ActualWidth;
                                double copy_y = Canvas.GetTop(copy_area) + copy_area.ActualHeight;

                                if (x > copy_x && y < copy_y)
                                {
                                    foreach (Thumb piece in pieces)
                                    {
                                        if ((string)((Thumb)piece).Tag == "Copied") Control_Piece("Remove", AttachedProperty.GetID(piece), 0, 0);
                                    }
                                    Control_Piece("Copy", selected_piece_ID, canvas.ActualWidth - Canvas.GetRight(copy_area) - copy_area.ActualWidth + 20 - x, Canvas.GetTop(copy_area) + 20 - y);
                                    return;
                                }
                            }
                        }
                        
                        
                    }
                        
                    targeted_piece_ID = another_piece_ID;
                    Control_Piece("Move", selected_piece_ID, -piece_horizontal_delta, -piece_vertical_delta);
                    delta_y = selected_piece.ActualHeight - VERTUCALLY_DENT;
                    if (AttachedProperty.GetChainPieceIDProperty(pieces[another_piece_ID]) != 0)
                    {
                        while (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                            Control_Piece("Move", targeted_piece_ID, 0, delta_y);
                        }
                        AttachedProperty.SetChainPieceIDProperty(selected_piece, AttachedProperty.GetChainPieceIDProperty(another_piece));
                        AttachedProperty.SetChainedPieceIDProperty(pieces[AttachedProperty.GetChainPieceIDProperty(another_piece)], selected_piece_ID);
                    }
                    AttachedProperty.SetChainPieceIDProperty(another_piece, selected_piece_ID);
                    AttachedProperty.SetChainedPieceIDProperty(selected_piece, another_piece_ID);
                    targeted_piece_ID = another_piece_ID;
                    while (true)
                    {
                        bool flag = false;
                        if (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                            }
                        }
                        foreach (int control_statement_ID in control_statement_IDs)
                        {
                            if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                            {
                                side_line = pieces[control_statement_ID].Template.FindName("MainSideLine", pieces[control_statement_ID]) as Border;
                                side_line.Height = side_line.ActualHeight + delta_y;
                                if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) != 0)
                                {
                                    int chain_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]);
                                    Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                    while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                                    {
                                        chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                        Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                    }
                                }
                                targeted_piece_ID = control_statement_ID;
                                flag = true;
                                break;
                            }
                            else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) == targeted_piece_ID)
                            {
                                side_line = pieces[control_statement_ID].Template.FindName("SubSideLine", pieces[control_statement_ID]) as Border;
                                side_line.Height = side_line.ActualHeight + delta_y;
                                targeted_piece_ID = control_statement_ID;
                                flag = true;
                                break;
                            }
                        }
                        if (flag == true)
                        {
                            if (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                int chain_piece_ID = targeted_piece_ID;
                                while (AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]) != 0)
                                {
                                    chain_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[chain_piece_ID]);
                                    Control_Piece("Move", chain_piece_ID, 0, delta_y);
                                }
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    break;

                case "Top":
                    
                    Control_Piece("Move", selected_piece_ID, -piece_horizontal_delta, piece_vertical_delta);
                    AttachedProperty.SetChainPieceIDProperty(selected_piece, another_piece_ID);
                    AttachedProperty.SetChainedPieceIDProperty(another_piece, selected_piece_ID);
                    break;
            }
            if (tuple.Item1 == "None")
            {
                
            }

            
        }

        private string Line_Up_Piece_IDs(int ID)
        {
            string return_IDs = "0";
            List<int> selected_main_control_statement_list = new List<int>();
            List<int> selected_sub_control_statement_list = new List<int>();
            var piece = pieces[ID] as Thumb;
            int targeted_piece_ID = ID;
            int owned_piece_ID = 0;
            bool flag = true;
            if (AttachedProperty.GetType(pieces[ID]) =="Control_Statement")
            {
                while (flag)
                {
                    piece = pieces[targeted_piece_ID] as Thumb;
                    return_IDs = return_IDs + "," + targeted_piece_ID;
                    if (AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        owned_piece_ID = AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID]);
                        return_IDs = return_IDs + "," + owned_piece_ID;
                    }
                    if (AttachedProperty.GetType(pieces[targeted_piece_ID]) == "Control_Statement")
                    {
                        if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0 && AttachedProperty.GetSubOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            selected_sub_control_statement_list.Insert(0, targeted_piece_ID);
                            selected_main_control_statement_list.Insert(0, targeted_piece_ID);
                            targeted_piece_ID = AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]);
                        }
                        else if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0 && AttachedProperty.GetSubOwnPieceIDProperty(pieces[targeted_piece_ID]) == 0)
                        {
                            selected_main_control_statement_list.Insert(0, targeted_piece_ID);
                            targeted_piece_ID = AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]);
                        }
                        else if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]) == 0 && AttachedProperty.GetSubOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            selected_sub_control_statement_list.Insert(0, targeted_piece_ID);
                            targeted_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[targeted_piece_ID]);
                        }
                        else if (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            if (selected_main_control_statement_list.Count == 0 && selected_sub_control_statement_list.Count == 0)
                            {
                                flag = false;
                            }
                            else
                            {
                                targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                            }
                        }
                        else
                        {
                            if (selected_main_control_statement_list.Count == 0 && selected_sub_control_statement_list.Count == 0)
                            {
                                flag = false;
                            }
                            else
                            {
                                bool control_statement_loop = true;
                                while (control_statement_loop == true)
                                {
                                    while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                                    {
                                        targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                                    }
                                    if (selected_main_control_statement_list.Count == 0)
                                    {
                                        if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) != 0)
                                        {
                                            targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                            control_statement_loop = false;
                                        }
                                        else
                                        {
                                            targeted_piece_ID = selected_sub_control_statement_list[0];
                                        }
                                        selected_sub_control_statement_list.RemoveAt(0);
                                    }
                                    else if (selected_sub_control_statement_list.Count == 0)
                                    {
                                        if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]) != 0)
                                        {
                                            targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]);
                                            control_statement_loop = false;
                                        }
                                        else
                                        {
                                            targeted_piece_ID = selected_main_control_statement_list[0];
                                        }
                                        selected_main_control_statement_list.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[selected_main_control_statement_list[0]]) == targeted_piece_ID)
                                        {
                                            if (selected_main_control_statement_list[0] == selected_sub_control_statement_list[0])
                                            {
                                                targeted_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                                control_statement_loop = false;
                                            }
                                            else if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]) != 0)
                                            {
                                                targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]);
                                                control_statement_loop = false;
                                            }
                                            else
                                            {
                                                targeted_piece_ID = selected_main_control_statement_list[0];
                                            }
                                            selected_main_control_statement_list.RemoveAt(0);
                                        }
                                        else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) == targeted_piece_ID)
                                        {
                                            if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) != 0)
                                            {
                                                targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                                control_statement_loop = false;
                                            }
                                            else
                                            {
                                                targeted_piece_ID = selected_sub_control_statement_list[0];
                                            }
                                            selected_sub_control_statement_list.RemoveAt(0);
                                        }
                                    }
                                    if (selected_main_control_statement_list.Count == 0 && selected_sub_control_statement_list.Count == 0)
                                    {
                                        control_statement_loop = false;
                                        flag = false;
                                    }
                                }
                            }
                        }
                        continue;
                    }
                    if (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                    }
                    else
                    {
                        bool control_statement_loop = true;
                        while (control_statement_loop == true)
                        {
                            while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                            }
                            if (selected_main_control_statement_list.Count == 0)
                            {
                                if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) != 0)
                                {
                                    targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                    control_statement_loop = false;
                                }
                                else
                                {
                                    targeted_piece_ID = selected_sub_control_statement_list[0];
                                }
                                selected_sub_control_statement_list.RemoveAt(0);
                            }
                            else if (selected_sub_control_statement_list.Count == 0)
                            {
                                if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]) != 0)
                                {
                                    targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]);
                                    control_statement_loop = false;
                                }
                                else
                                {
                                    targeted_piece_ID = selected_main_control_statement_list[0];
                                }
                                selected_main_control_statement_list.RemoveAt(0);
                            }
                            else
                            {
                                if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[selected_main_control_statement_list[0]]) == targeted_piece_ID)
                                {
                                    if (selected_main_control_statement_list[0] == selected_sub_control_statement_list[0])
                                    {
                                        targeted_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                        control_statement_loop = false;
                                    }
                                    else if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]) != 0)
                                    {
                                        targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]);
                                        control_statement_loop = false;
                                    }
                                    else
                                    {
                                        targeted_piece_ID = selected_main_control_statement_list[0];
                                    }
                                    selected_main_control_statement_list.RemoveAt(0);
                                }
                                else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) == targeted_piece_ID)
                                {
                                    if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) != 0)
                                    {
                                        targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                        control_statement_loop = false;
                                    }
                                    else
                                    {
                                        targeted_piece_ID = selected_sub_control_statement_list[0];
                                    }
                                    selected_sub_control_statement_list.RemoveAt(0);
                                }
                            }
                            if (selected_main_control_statement_list.Count == 0 && selected_sub_control_statement_list.Count == 0)
                            {
                                control_statement_loop = false;
                                flag = false;
                            }
                        }
                    }
                }
            }
            else
            {
                return_IDs = return_IDs + "," + ID;
                if (AttachedProperty.GetOwnPieceIDProperty(pieces[ID]) != 0)
                {
                    owned_piece_ID = AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID]);
                    return_IDs = return_IDs + "," + owned_piece_ID;
                }
            }
            return return_IDs;
        }

        public void Control_Piece (string operation, int selected_piece_ID, double delta_x, double delta_y)
        {
            string return_string = Line_Up_Piece_IDs(selected_piece_ID);
            string[] splited_IDs = return_string.Split(',');
            var selected_pieces_list = new List<string>();
            selected_pieces_list.AddRange(splited_IDs);
            selected_pieces_list.RemoveAt(0);
            if(selected_pieces_list.Count != 0)
            {
                switch (operation)
                {
                case "Move":
                        foreach(string targeted_piece_ID in selected_pieces_list)
                        {
                            int ID = Convert.ToInt32(targeted_piece_ID);
                            var piece = pieces[ID] as Thumb;
                            var canvas = piece.Parent as Canvas;
                            if (piece == null) return;
                            if (canvas == null) return;
                            double x = Canvas.GetLeft(piece);
                            if (double.IsNaN(x)) x = 0;
                            double y = Canvas.GetTop(piece);
                            if (double.IsNaN(y)) y = 0;
                            x += delta_x;
                            y += delta_y;
                            Canvas.SetLeft(piece, x);
                            Canvas.SetTop(piece, y);
                        }
                        break;

                    case "Stand_Out":
                        foreach (string targeted_piece_ID in selected_pieces_list)
                        {
                            int ID = Convert.ToInt32(targeted_piece_ID);
                            var piece = pieces[ID] as Thumb;
                            num++;
                            Canvas.SetZIndex(piece, num);
                        }
                            
                        break;

                    case "Remove":
                        foreach (string targeted_piece_ID in selected_pieces_list)
                        {
                            int ID = Convert.ToInt32(targeted_piece_ID);
                            var piece = pieces[ID] as Thumb;
                            piece.Tag = "Removed";
                            area.Children.Remove(piece);
                        }
                        break;

                    case "Copy":
                        copied_piece_IDs.Clear();
                        raw_piece_IDs.Clear();
                        pieces[Convert.ToInt32(selected_pieces_list[0])].DragStarted -= Pieces_DragStarted;
                        pieces[Convert.ToInt32(selected_pieces_list[0])].DragStarted += Copy_Piece;
                        foreach (string targeted_piece_ID in selected_pieces_list)
                        {
                            int ID = Convert.ToInt32(targeted_piece_ID);
                            var piece = pieces[ID] as Thumb;
                            var copied_piece = new Thumb();

                            
                            piece.Tag = "Copied";
                            Canvas.SetLeft(piece, delta_x + Canvas.GetLeft(piece));
                            Canvas.SetTop(piece, delta_y + Canvas.GetTop(piece));

                            block_count++;
                            raw_piece_IDs.Add(ID);
                            copied_piece_IDs.Add(block_count);

                            pieces.Add(copied_piece);

                            Canvas.SetLeft(copied_piece, 10000);
                            Canvas.SetTop(copied_piece, 10000);
                            copied_piece.Template = piece.Template;
                            copied_piece.Tag = "Copied";
                            copied_piece.Name = piece.Name;
                            AttachedProperty.SetID(copied_piece, block_count);
                            if (AttachedProperty.GetType(piece) == "Control_Statement")
                            {
                                control_statement_IDs.Add(block_count);
                            }
                            area.Children.Add(copied_piece);
                        }
                        break;
                }
            }
        }
        private void Height_Change(Thumb piece, Thumb copied_piece)
        {
            var main_side_line = new Border();
            var copied_main_side_line = new Border();
            var sub_side_line = new Border();
            var copied_sub_side_line = new Border();
            if (piece.Template.FindName("MainSideLine", piece) != null)
            {
                main_side_line = piece.Template.FindName("MainSideLine", piece) as Border;
            }
            if (copied_piece.Template.FindName("MainSideLine", copied_piece) != null)
            {
                copied_main_side_line = copied_piece.Template.FindName("MainSideLine", copied_piece) as Border;
            }
            
            copied_main_side_line.Height = main_side_line.Height;

            if (piece.Template.FindName("SubSideLine", piece) != null)
            {
                sub_side_line = piece.Template.FindName("SubSideLine", piece) as Border;
            }
            if (copied_piece.Template.FindName("SubSideLine", copied_piece) != null)
            {
                copied_sub_side_line = copied_piece.Template.FindName("SubSideLine", copied_piece) as Border;
            }
            copied_sub_side_line.Height = sub_side_line.Height;
        }


        private void Copy_Piece(object sender, DragStartedEventArgs e)
        {
            pieces[Convert.ToInt32(raw_piece_IDs[0])].DragStarted -= Copy_Piece;
            pieces[Convert.ToInt32(raw_piece_IDs[0])].DragStarted += Pieces_DragStarted;
            foreach (int ID in raw_piece_IDs)
            {
                var piece = pieces[ID] as Thumb;
                var copied_piece = pieces[copied_piece_IDs[raw_piece_IDs.IndexOf(ID)]];


                Control_Piece("Stand_Out",ID, 0, 0);
                piece.Tag = "Normal";

                AttachedProperty.SetType(copied_piece, AttachedProperty.GetType(piece));
                AttachedProperty.SetMainInsideProperty(copied_piece, AttachedProperty.GetMainInsideProperty(piece));
                AttachedProperty.SetSubInsideProperty(copied_piece, AttachedProperty.GetSubInsideProperty(piece));
                AttachedProperty.SetTopSideProperty(copied_piece, AttachedProperty.GetTopSideProperty(piece));
                AttachedProperty.SetLeftSideProperty(copied_piece, AttachedProperty.GetLeftSideProperty(piece));
                AttachedProperty.SetRightSideProperty(copied_piece, AttachedProperty.GetRightSideProperty(piece));
                AttachedProperty.SetBottomSideProperty(copied_piece, AttachedProperty.GetBottomSideProperty(piece));
                if (raw_piece_IDs.Contains(AttachedProperty.GetMainOwnPieceIDProperty(piece))) AttachedProperty.SetMainOwnPieceIDProperty(copied_piece, copied_piece_IDs[raw_piece_IDs.IndexOf(AttachedProperty.GetMainOwnPieceIDProperty(piece))]);
                if (raw_piece_IDs.Contains(AttachedProperty.GetSubOwnPieceIDProperty(piece))) AttachedProperty.SetSubOwnPieceIDProperty(copied_piece, copied_piece_IDs[raw_piece_IDs.IndexOf(AttachedProperty.GetSubOwnPieceIDProperty(piece))]);
                if (raw_piece_IDs.Contains(AttachedProperty.GetChainedPieceIDProperty(piece))) AttachedProperty.SetChainedPieceIDProperty(copied_piece, copied_piece_IDs[raw_piece_IDs.IndexOf(AttachedProperty.GetChainedPieceIDProperty(piece))]);
                if (raw_piece_IDs.Contains(AttachedProperty.GetChainPieceIDProperty(piece))) AttachedProperty.SetChainPieceIDProperty(copied_piece, copied_piece_IDs[raw_piece_IDs.IndexOf(AttachedProperty.GetChainPieceIDProperty(piece))]);
                if (raw_piece_IDs.Contains(AttachedProperty.GetOwnPieceIDProperty(piece))) AttachedProperty.SetOwnPieceIDProperty(copied_piece, copied_piece_IDs[raw_piece_IDs.IndexOf(AttachedProperty.GetOwnPieceIDProperty(piece))]);
                if (raw_piece_IDs.Contains(AttachedProperty.GetOwnedPieceIDProperty(piece))) AttachedProperty.SetOwnedPieceIDProperty(copied_piece, copied_piece_IDs[raw_piece_IDs.IndexOf(AttachedProperty.GetOwnedPieceIDProperty(piece))]);
              
                
                if (AttachedProperty.GetType(piece) == "Control_Statement")
                {
                    Height_Change(piece, copied_piece);
                }

                try
                {
                    ComboBox combobox = piece.Template.FindName("ComboBox1", piece) as ComboBox;
                    int combo_box_index = combobox.SelectedIndex;
                    combobox = copied_piece.Template.FindName("ComboBox1", copied_piece) as ComboBox;
                    combobox.SelectedIndex = combo_box_index;
                    try
                    {
                        combobox = piece.Template.FindName("ComboBox2", piece) as ComboBox;
                        combo_box_index = combobox.SelectedIndex;
                        combobox = copied_piece.Template.FindName("ComboBox2", copied_piece) as ComboBox;
                        combobox.SelectedIndex = combo_box_index;
                        try
                        {
                            combobox = piece.Template.FindName("ComboBox3", piece) as ComboBox;
                            combo_box_index = combobox.SelectedIndex;
                            combobox = copied_piece.Template.FindName("ComboBox3", copied_piece) as ComboBox;
                            combobox.SelectedIndex = combo_box_index;
                        }
                        catch
                        {

                        }
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }

                try
                {
                    TextBox textbox = piece.Template.FindName("TextBox1", piece) as TextBox;
                    string textbox_text = textbox.Text;
                    textbox = copied_piece.Template.FindName("TextBox1", copied_piece) as TextBox;
                    textbox.Text =textbox_text;
                    try
                    {
                        textbox = piece.Template.FindName("TextBox2", piece) as TextBox;
                        textbox_text = textbox.Text;
                        textbox = copied_piece.Template.FindName("TextBox2", copied_piece) as TextBox;
                        textbox.Text = textbox_text;
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }

                Canvas.SetLeft(copied_piece, Canvas.GetLeft(piece));
                Canvas.SetTop(copied_piece, Canvas.GetTop(piece));


                copied_piece.DragStarted += Pieces_DragStarted;
                copied_piece.DragDelta += Pieces_DragDelta;
                copied_piece.DragCompleted += Pieces_DragCompleted;


            }
            pieces[Convert.ToInt32(copied_piece_IDs[0])].DragStarted -= Pieces_DragStarted;
            pieces[Convert.ToInt32(copied_piece_IDs[0])].DragStarted += Copy_Piece;
            raw_piece_IDs.Clear();
            raw_piece_IDs.AddRange(copied_piece_IDs);
            copied_piece_IDs.Clear();
            foreach(int ID in raw_piece_IDs)
            {
                var piece = pieces[ID] as Thumb;
                var copied_piece = new Thumb();

                block_count++;
                copied_piece_IDs.Add(block_count);

                pieces.Add(copied_piece);

                Canvas.SetLeft(copied_piece, 10000);
                Canvas.SetTop(copied_piece, 10000);
                copied_piece.Template = piece.Template;
                copied_piece.Tag = "Copied";
                copied_piece.Name = piece.Name;
                AttachedProperty.SetID(copied_piece, block_count);
                if (AttachedProperty.GetType(piece) == "Control_Statement")
                {
                    control_statement_IDs.Add(block_count);
                }
                area.Children.Add(copied_piece);
            }
            
        }

        private void Create_Source_Code(int ID)
        {

            StreamReader sr = new StreamReader("convert_sourcecode.txt", Encoding.GetEncoding("Shift_JIS"));
            string convert_str = sr.ReadToEnd();
            sr.Close();
            cnv_code.Clear();
            convert_str = convert_str.Replace("\r", "").Replace("\n", "");
            string[] arr = convert_str.Split('$');
            for (int i = 1; i < arr.Length; i += 2)
            {
                cnv_code.Add(arr[i], arr[i + 1]);
            }

            sr = new StreamReader("add_sorcecode.txt", Encoding.GetEncoding("Shift_JIS"));
            convert_str = sr.ReadToEnd();
            sr.Close();
            add_code.Clear();
            convert_str = convert_str.Replace("\r", "").Replace("\n", "");
            string[] add_arr = convert_str.Split('$');
            string[] add_arr_key = new string[5];
            for (int i = 1; i < 6; i += 1)
            { 
                add_arr_key[i-1] = add_arr[i];
            }
            for (int i = 6; i < add_arr.Length; i +=5)
            {
                Dictionary<string, string> tmp_dict = new Dictionary<string, string>();
                for (int j = 1; j < 5; j += 1)
                {
                    tmp_dict.Add(add_arr_key[j], add_arr[i + j]);
                }
                add_code.Add(add_arr[i], tmp_dict);
            }

            string source_code = "";
            List<int> selected_main_control_statement_list = new List<int>();
            List<int> selected_sub_control_statement_list = new List<int>();
            var piece = pieces[ID] as Thumb;
            int targeted_piece_ID = ID;
            int owned_piece_ID = 0;
            bool loop_source_code_flag = false;
            bool flag = true;
            if (pieces[targeted_piece_ID].Name == "WhileTrue")
            {
                loop_source_code_flag = true;

            }
            if (AttachedProperty.GetType(pieces[targeted_piece_ID]) == "Control_Statement")
            {
                while (flag)
                {
                    piece = pieces[targeted_piece_ID] as Thumb;
                    source_code += Edit_Source_Code(targeted_piece_ID);
                    if (AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        owned_piece_ID = AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID]);
                        source_code += Edit_Source_Code(owned_piece_ID);
                    }
                    else if (AttachedProperty.GetRightSideProperty(pieces[targeted_piece_ID]) == true)
                    {
                        source_code += "0\r\n";
                    }

                    if (AttachedProperty.GetType(pieces[targeted_piece_ID]) == "Control_Statement")
                    {

                        source_code += "{\r\n";

                        if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0 && AttachedProperty.GetSubOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            selected_sub_control_statement_list.Insert(0, targeted_piece_ID);
                            selected_main_control_statement_list.Insert(0, targeted_piece_ID);
                            targeted_piece_ID = AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]);
                        }
                        else if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0 && AttachedProperty.GetSubOwnPieceIDProperty(pieces[targeted_piece_ID]) == 0)
                        {
                            selected_main_control_statement_list.Insert(0, targeted_piece_ID);
                            targeted_piece_ID = AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]);
                        }
                        else if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[targeted_piece_ID]) == 0 && AttachedProperty.GetSubOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            source_code += "}\r\nelse\r\n{\r\n";
                            selected_sub_control_statement_list.Insert(0, targeted_piece_ID);
                            targeted_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[targeted_piece_ID]);
                        }
                        else if (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                        {
                            source_code += "\r\n}\r\n";
                            if (selected_main_control_statement_list.Count == 0 && selected_sub_control_statement_list.Count == 0)
                            {
                                flag = false;
                            }
                            else
                            {
                                targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                            }
                        }
                        else
                        {
                            source_code += "\r\n}\r\n";
                            if (selected_main_control_statement_list.Count == 0 && selected_sub_control_statement_list.Count == 0)
                            {
                                if (AttachedProperty.GetSubInsideProperty(pieces[targeted_piece_ID]) == true) source_code += "\r\nelse\r\n{\r\n}\r\n";
                                flag = false;
                            }
                            else
                            {

                                bool control_statement_loop = true;
                                while (control_statement_loop == true)
                                {
                                    while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                                    {
                                        targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                                    }
                                    if (selected_main_control_statement_list.Count == 0)
                                    {
                                        if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) != 0)
                                        {
                                            targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                            control_statement_loop = false;
                                        }
                                        else
                                        {
                                            targeted_piece_ID = selected_sub_control_statement_list[0];
                                        }
                                        selected_sub_control_statement_list.RemoveAt(0);
                                        source_code += "}\r\n";
                                    }
                                    else if (selected_sub_control_statement_list.Count == 0)
                                    {
                                        if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]) != 0)
                                        {
                                            targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]);
                                            control_statement_loop = false;

                                        }
                                        else
                                        {
                                            targeted_piece_ID = selected_main_control_statement_list[0];
                                        }
                                        source_code += "}\r\n";
                                        if (AttachedProperty.GetSubInsideProperty(pieces[selected_main_control_statement_list[0]]) == true) source_code += "\r\nelse\r\n{\r\n}\r\n";
                                        selected_main_control_statement_list.RemoveAt(0);
                                    }
                                    else
                                    {
                                        if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[selected_main_control_statement_list[0]]) == targeted_piece_ID)
                                        {
                                            if (selected_main_control_statement_list[0] == selected_sub_control_statement_list[0])
                                            {
                                                targeted_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                                control_statement_loop = false;
                                                source_code += "}\r\nelse\r\n{\r\n";
                                            }
                                            else if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]) != 0)
                                            {
                                                targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]);
                                                control_statement_loop = false;
                                                source_code += "}\r\n";
                                            }
                                            else
                                            {
                                                targeted_piece_ID = selected_main_control_statement_list[0];
                                                source_code += "}\r\n";
                                            }
                                            selected_main_control_statement_list.RemoveAt(0);

                                        }
                                        else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) == targeted_piece_ID)
                                        {
                                            if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) != 0)
                                            {
                                                targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                                control_statement_loop = false;
                                            }
                                            else
                                            {
                                                targeted_piece_ID = selected_sub_control_statement_list[0];
                                            }
                                            source_code += "}\r\n";
                                            selected_sub_control_statement_list.RemoveAt(0);
                                        }
                                    }
                                    if (selected_main_control_statement_list.Count == 0 && selected_sub_control_statement_list.Count == 0)
                                    {
                                        control_statement_loop = false;
                                        flag = false;
                                    }
                                }
                            }
                        }
                        continue;
                    }

                    if (AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                    {
                        targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[targeted_piece_ID]);
                    }
                    else
                    {
                        bool control_statement_loop = true;
                        while (control_statement_loop == true)
                        {
                            while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                            {
                                targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                            }
                            if (selected_main_control_statement_list.Count == 0)
                            {
                                if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) != 0)
                                {
                                    targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                    control_statement_loop = false;
                                }
                                else
                                {
                                    targeted_piece_ID = selected_sub_control_statement_list[0];
                                }
                                source_code += "}\r\n";
                                selected_sub_control_statement_list.RemoveAt(0);
                            }
                            else if (selected_sub_control_statement_list.Count == 0)
                            {
                                if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]) != 0)
                                {
                                    targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]);
                                    source_code += "}\r\n";
                                    control_statement_loop = false;
                                }
                                else
                                {
                                    targeted_piece_ID = selected_main_control_statement_list[0];
                                    source_code += "}\r\n";
                                    if (AttachedProperty.GetSubInsideProperty(pieces[targeted_piece_ID]) == true) source_code += "else\r\n{\r\n}\r\n";
                                }
                                selected_main_control_statement_list.RemoveAt(0);
                            }
                            else
                            {
                                if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[selected_main_control_statement_list[0]]) == targeted_piece_ID)
                                {
                                    if (selected_main_control_statement_list[0] == selected_sub_control_statement_list[0])
                                    {
                                        targeted_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                        control_statement_loop = false;
                                        source_code += "}\r\nelse\r\n{\r\n";
                                    }
                                    else if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]) != 0)
                                    {
                                        targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_main_control_statement_list[0]]);
                                        control_statement_loop = false;
                                        source_code += "}\r\n";
                                    }
                                    else
                                    {
                                        targeted_piece_ID = selected_main_control_statement_list[0];
                                        source_code += "}\r\n";
                                    }
                                    selected_main_control_statement_list.RemoveAt(0);

                                }
                                else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) == targeted_piece_ID)
                                {
                                    if (AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]) != 0)
                                    {
                                        targeted_piece_ID = AttachedProperty.GetChainPieceIDProperty(pieces[selected_sub_control_statement_list[0]]);
                                        control_statement_loop = false;
                                    }
                                    else
                                    {
                                        targeted_piece_ID = selected_sub_control_statement_list[0];
                                    }
                                    source_code += "}\r\n";
                                    selected_sub_control_statement_list.RemoveAt(0);
                                }
                            }
                            if (selected_main_control_statement_list.Count == 0 && selected_sub_control_statement_list.Count == 0)
                            {
                                control_statement_loop = false;
                                flag = false;
                            }
                        }
                    }
                }
            }
            else
            {
                source_code += Edit_Source_Code(targeted_piece_ID);
                if (AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                {
                    owned_piece_ID = AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID]);
                    source_code += Edit_Source_Code(owned_piece_ID);
                }
                else if (AttachedProperty.GetRightSideProperty(pieces[targeted_piece_ID]) == true)
                {
                    source_code += "0\r\n";
                }
            }
            if (pieces[ID].Name == "WhileTrue" || pieces[ID].Name == "DoOnce")
            {
                source_code = source_code.Substring(3, source_code.Length - 6);
                source_code = source_code + "\r\n";
            }

            source_code += "end";
            source_code = Format_Source_Code(source_code, loop_source_code_flag);
            if (error_code == 0)
            {
                Clipboard.SetText(source_code);
                if (show_source_code.IsChecked)
                {
                    show_source_code window = new show_source_code();
                    window.source_code = source_code;
                    window.Show();
                }
                else
                {
                    MessageBox.Show("コードをクリップボードへコピーしました。", "ESP32 制御補助ツール", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                
            }
            else
            {
                switch (error_code)
                {
                    case 1:
                        MessageBox.Show("&&の後ろには変数を入力する必要があります。", "ESP32 制御補助ツール", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                }
            }

        }

        private string Edit_Source_Code(int piece_ID)
        {
            string source_code = "";
            ComboBox combobox = new ComboBox();
            TextBox textbox = new TextBox();
            MainViewModel main_view_model = new MainViewModel();
            switch (pieces[piece_ID].Name)
            {
                case "SerialPrint":
                    textbox = pieces[piece_ID].Template.FindName("TextBox1", pieces[piece_ID]) as TextBox;
                    string text = "Serial.print( ";
                    text = text + "\"" + textbox.Text + "\"";
                    string search = "[";
                    int pos = text.IndexOf(search);
                    int final_pos = 12;
                    if (text.Contains(search) && text.Substring(pos + 2, 1) =="]")
                    {
                        bool variable_error_flag = false;
                        while (0 <= pos)
                        {
                            final_pos = pos + 18;
                            int nextIndex = pos + search.Length;
                            string variable = text.Substring(nextIndex, 1);
                            if (variable_error_flag == false)
                            {
                                for (char moji = 'a'; moji <= 'z'; ++moji)
                                {
                                    if (moji.ToString() == variable)
                                    {
                                        variable_error_flag = false;
                                        break;
                                    }
                                    variable_error_flag = true;
                                }
                            }
                            text = text.Substring(0, pos) + "\" );\r\nSerial.print( " + variable + " );\r\nSerial.print( \"" + text.Substring(nextIndex + 2);
                            if (nextIndex < text.Length)
                            {
                                pos = text.IndexOf(search, nextIndex);
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (variable_error_flag)
                        {
                            error_code = 1;
                        }
                    }
                    text += " );\r\n";
                    text = text.Replace("Serial.print( \"\" );\r\n", "");
                    search = "Serial.print";
                    pos = text.IndexOf(search);
                    while (0 <= pos)
                    {
                        final_pos = pos + 12;
                        int nextIndex = pos + search.Length;
                        if (nextIndex < text.Length)
                        {
                            pos = text.IndexOf(search, nextIndex);
                        }
                        else
                        {
                            break;
                        }
                    }
                    text = text.Substring(0, final_pos) + "ln" + text.Substring(final_pos);
                    source_code += text;
                    break;

                case "BlueToothSerialPrint":
                    textbox = pieces[piece_ID].Template.FindName("TextBox1", pieces[piece_ID]) as TextBox;
                    text = "SerialBT.print( ";
                    text = text + "\"" + textbox.Text + "\"";
                    search = "&&";
                    pos = text.IndexOf(search);
                    final_pos = 12;
                    if (text.Contains(search))
                    {
                        bool variable_error_flag = false;
                        while (0 <= pos)
                        {
                            final_pos = pos + 18;
                            int nextIndex = pos + search.Length;
                            string variable = text.Substring(nextIndex, 1);
                            if (variable_error_flag == false)
                            {
                                for (char moji = 'a'; moji <= 'z'; ++moji)
                                {
                                    if (moji.ToString() == variable)
                                    {
                                        variable_error_flag = false;
                                        break;
                                    }
                                    variable_error_flag = true;
                                }
                            }
                            text = text.Substring(0, pos) + "\" );\r\nSerialBT.print( " + variable + " );\r\nSerialBT.print( \"" + text.Substring(nextIndex + 1);
                            if (nextIndex < text.Length)
                            {
                                pos = text.IndexOf(search, nextIndex);
                            }
                            else
                            {
                                break;
                            }
                        }
                        if (variable_error_flag)
                        {
                            error_code = 1;
                        }
                    }
                    text += " );\r\n";
                    text = text.Replace("SerialBT.print( \"\" );\r\n", "");
                    search = "SerialBT.print";
                    pos = text.IndexOf(search);
                    while (0 <= pos)
                    {
                        final_pos = pos + 14;
                        int nextIndex = pos + search.Length;
                        if (nextIndex < text.Length)
                        {
                            pos = text.IndexOf(search, nextIndex);
                        }
                        else
                        {
                            break;
                        }
                    }
                    text = text.Substring(0, final_pos) + "ln" + text.Substring(final_pos);
                    source_code += text;
                    break;

                case "DoOnce":
                    break;

                case "WhileTrue":
                    break;

                case "IfVariable":
                    source_code += "if ( ";
                    combobox = pieces[piece_ID].Template.FindName("ComboBox1", pieces[piece_ID]) as ComboBox;
                    source_code += ComboBoxItems_To_Script(combobox.Text);
                    source_code += " ";
                    combobox = pieces[piece_ID].Template.FindName("ComboBox1", pieces[piece_ID]) as ComboBox;
                    source_code += ComboBoxItems_To_Script(combobox.Text);
                    source_code += " ";
                    combobox = pieces[piece_ID].Template.FindName("TextBox1", pieces[piece_ID]) as ComboBox;
                    source_code += ComboBoxItems_To_Script(combobox.Text);
                    source_code += " ) ";
                    break;

                default:
                    StringBuilder sb = new StringBuilder();

                    string code = cnv_code[pieces[piece_ID].Name];
                    bool control_box_flag = false;
                    string control_box_name = "";
                    for (int i = 0; i < code.Length; i++)
                    {
                        if (code[i] == '#')
                        {
                            control_box_flag = !control_box_flag;
                            continue;
                        }
                        if (control_box_flag)
                        {
                            sb.Append(code[i]);
                        }
                        else
                        {
                            if (sb.Length != 0)
                            {
                                control_box_name = sb.ToString();
                                if (control_box_name.Contains("ComboBox"))
                                {
                                    combobox = pieces[piece_ID].Template.FindName(control_box_name, pieces[piece_ID]) as ComboBox;
                                    source_code += ComboBoxItems_To_Script(combobox.Text);
                                }
                                if (control_box_name.Contains("TextBox"))
                                {
                                    textbox = pieces[piece_ID].Template.FindName(control_box_name, pieces[piece_ID]) as TextBox;
                                    source_code += textbox.Text.ToString();
                                }
                            }
                            sb.Clear();
                            source_code += code[i];
                        }
                    }
                    source_code = source_code.Replace(" %%", "\r\n");
                    break;
            }
            return source_code;
        }

        private string ComboBoxItems_To_Script(string comboboxitem)
        {
            if (comboboxitem.Contains("変数"))
            {
                return (comboboxitem.Replace("変数", ""));
            }
            switch (comboboxitem)
            {
                case "オン":
                    return ("HIGH");

                case "オフ":
                    return ("LOW");

                case "ピン16":
                    return ("16");

                case "ピン17":
                    return ("17");

                case "ピン27":
                    return ("27");

                case "ピン32":
                    return ("32");

                case "ピン33":
                    return ("33");

                case "ピン2":
                    return ("A12");

                case "ピン4":
                    return ("A10");

                case "ピン12":
                    return ("A15");

                case "ピン13":
                    return ("A14");

                case "ピン14":
                    return ("A16");

                case "ピン15":
                    return ("A13");

                case "晴れ":
                    return ("Clear");

                case "曇り":
                    return ("Clouds");

                case "雨":
                    return ("Rain");

                case "＝":
                    return ("==");

                case "＞":
                    return (">");

                case "＜":
                    return ("<");

                case "≧":
                    return (">=");

                case "≦":
                    return ("<=");

                case "＋":
                    return ("+");

                case "－":
                    return ("-");

                case "÷":
                    return ("/");

                case "×":
                    return ("*");

                case "％":
                    return ("%");

                case "左バンパー":
                    return ("LeftBanper");

                case "右バンパー":
                    return ("RightBanper");

                case "押されて":
                    return ("LOW");

                case "離れて":
                    return ("HIGH");
            }
            return ("error");
        }
        private string Format_Source_Code(string raw_source_code, bool loop_source_code_flag)
        {
            string formatted_source_code = "";
            string setup_source_code = "";
            string func_source_code = "";
            string main_source_code = raw_source_code;
            bool wifi_flag = false;
            while (main_source_code.Contains("}}"))
            {
                main_source_code = main_source_code.Replace("}}", "}\r\n}");
            }
            main_source_code = main_source_code.Replace("}end", "}\r\nend");
            main_source_code = "  " + main_source_code;
            string pad_space = "";
            int layer = 1;
            int pos = -1;
            int initial_pos = 0;
            while (true)
            {
                pad_space = "";
                pos = main_source_code.IndexOf("\r\n", pos + 1);
                initial_pos = pos + 2;
                string initial_string = main_source_code.Substring(initial_pos, 3);
                if (initial_string.StartsWith("}"))
                {
                    layer--;
                }
                for (int i = 0; i < layer; i++)
                {
                    pad_space += "  ";
                }
                main_source_code = main_source_code.Substring(0, initial_pos) + pad_space + main_source_code.Substring(initial_pos);
                if (initial_string.StartsWith("{"))
                {
                    layer++;
                }
                if (initial_string.StartsWith("end"))
                {
                    main_source_code = main_source_code.Remove(initial_pos);
                    string[] indent = { "\r\n" };
                    string[] splited_formatted_source_code = main_source_code.Split(indent, StringSplitOptions.None);
                    List<string> list_splited_formatted_source_code = new List<string>();
                    list_splited_formatted_source_code.AddRange(splited_formatted_source_code);
                    int row = 0;
                    int delete_count = 0;
                    foreach (string single_line in splited_formatted_source_code)
                    {
                        if (single_line.Replace(" ", "").Length == 0)
                        {
                            list_splited_formatted_source_code.RemoveAt(row - delete_count);
                            delete_count++;
                        }
                        row++;
                    }
                    main_source_code = String.Join("\r\n", list_splited_formatted_source_code);
                    break;
                }
            }
            string variable_declaration = "";
            for (char moji = 'a'; moji <= 'z'; ++moji)
            {
                string variable = " " + moji + " ";
                if (main_source_code.Contains(variable))
                {
                    variable_declaration += "int" + variable + "= 0;\r\n";
                }
            }

            foreach(string func_name in add_code.Keys)
            {
                if (main_source_code.Contains(func_name) || func_source_code.Contains(func_name))
                {
                    if (add_code[func_name].ContainsKey("variable")) variable_declaration += add_code[func_name]["variable"].Replace(" %%", "\r\n");
                    if (add_code[func_name].ContainsKey("setup")) setup_source_code += add_code[func_name]["setup"].Replace(" %%", "\r\n");
                    if (add_code[func_name].ContainsKey("main")) main_source_code += add_code[func_name]["main"].Replace(" %%", "\r\n");
                    if (add_code[func_name].ContainsKey("func")) func_source_code += add_code[func_name]["func"].Replace(" %%", "\r\n");
                }
            }

            string search = "SerialBT";
            if (main_source_code.Contains(search))
            {
                StreamReader device_name_file = new StreamReader("device_name.txt", Encoding.GetEncoding("Shift_JIS"));
                string device_name = device_name_file.ReadToEnd();
                device_name_file.Close();
                setup_source_code += "  SerialBT.begin( \"";
                setup_source_code += device_name.Split(',')[0];
                setup_source_code += "\" );\r\n";
            }


            search = "AnalogOutput( ";
            if (main_source_code.Contains(search))
            {
                string[] used_pin = { "A12", "A10", "A15", "A14", "A16", "A13" };
                pos = main_source_code.IndexOf(search);
                while (0 <= pos)
                {
                    int nextIndex = pos + search.Length;
                    string pin = main_source_code.Substring(nextIndex, 3);
                    for (int i = 0; i < used_pin.Length; i++)
                    {
                        if (pin == used_pin[i] && !setup_source_code.Contains(used_pin[i]))
                        {
                            main_source_code = main_source_code.Replace(used_pin[i], i.ToString());
                            setup_source_code += "  ledcSetup(" + i + ", 12800, 10);\r\n  ledcAttachPin(" + used_pin[i] + ", " + i + ");\r\n";
                        }
                    }
                    if (nextIndex < main_source_code.Length)
                    {
                        pos = main_source_code.IndexOf(search, nextIndex);
                    }
                    else
                    {
                        break;
                    }
                }

            }
            search = "digitalWrite( ";
            if (main_source_code.Contains(search))
            {
                string[] used_pin = { "16", "17", "27", "32", "33" }; ;
                pos = main_source_code.IndexOf(search);
                while (0 <= pos)
                {
                    int nextIndex = pos + search.Length;
                    string pin = main_source_code.Substring(nextIndex, 2);
                    for (int i = 0; i < used_pin.Length; i++)
                    {
                        if (pin == used_pin[i] && !setup_source_code.Contains(used_pin[i]))
                        {
                            setup_source_code += "  pinMode( " + used_pin[i] + ", OUTPUT );\r\n";
                        }
                    }
                    if (nextIndex < main_source_code.Length)
                    {
                        pos = main_source_code.IndexOf(search, nextIndex);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            search = "CompareWeather";
            if (main_source_code.Contains(search))
            {
                wifi_flag = true;
                StreamReader API_key_file = new StreamReader("API_key.txt", Encoding.GetEncoding("Shift_JIS"));
                string API_key = API_key_file.ReadToEnd();
                API_key_file.Close();
                
            }
            search = "getLocalTime(&timeInfo)";
            if (main_source_code.Contains(search))
            {
            }
            if (wifi_flag)
            {
                formatted_source_code += "#include <WiFi.h>\r\n";
                StreamReader setting_file = new StreamReader("wifi_setting.txt", Encoding.GetEncoding("Shift_JIS"));
                string setting = setting_file.ReadToEnd();
                setting_file.Close();
                setup_source_code = setup_source_code
                    + "  WiFi.begin( \""
                    + setting.Split(',')[0]
                    + "\", \""
                    + setting.Split(',')[1]
                    + "\" );\r\n"
                    + "  while ( WiFi.status() != WL_CONNECTED )\r\n"
                    + "  {\r\n"
                    + "    delay( 1000 );\r\n"
                    + "    Serial.println( \"WiFi検索中...\" );\r\n"
                    + "  }\r\n"
                    + "  Serial.println( \"WiFiに接続しました\" );\r\n";
            }
            if (loop_source_code_flag == true)
            {
                setup_source_code = "void setup()\r\n{\r\n" + setup_source_code + "\r\n}\r\n";
                main_source_code = "void loop()\r\n{\r\n" + main_source_code + "\r\n}";
            }
            else
            {
                setup_source_code = "void setup()\r\n{\r\n" + setup_source_code + main_source_code + "\r\n}\r\n";
                main_source_code = "void loop()\r\n{\r\n\r\n}";
            }

            formatted_source_code += variable_declaration + "\r\n" + setup_source_code + "\r\n\r\n" + main_source_code + "\r\n" + func_source_code;
            if (formatted_source_code.Contains("http.begin("))
            {
                formatted_source_code = "#include <HTTPClient.h>\r\n" + formatted_source_code;
            }
            if (formatted_source_code.Contains("JsonObject&"))
            {
                formatted_source_code = "#include <ArduinoJson.h>\r\n" + formatted_source_code;
            }
            if (formatted_source_code.Contains("SerialBT"))
            {
                formatted_source_code = "#include \"BluetoothSerial.h\"\r\n\r\n"
                                      + "#if !defined(CONFIG_BT_ENABLED) || !defined(CONFIG_BLUEDROID_ENABLED)\r\n"
                                      + "#error Bluetooth is not enabled! Please run `make menuconfig` to and enable it\r\n"
                                      + "#endif+ formatted_source_code;\r\n\r\n"
                                      + "BluetoothSerial SerialBT;\r\n" + formatted_source_code;
            }
            return formatted_source_code;
        }

        private Tuple<string, int, double, double> Check_Side(int selected_piece_ID)
        {
            string side = "None";
            double piece_vertical_delta = 0;
            double piece_horizontal_delta = 0;
            double piece_min_vertical_delta = 100;
            double piece_min_horizontal_delta = 0;
            int another_piece_ID = -1;
            int return_piece_ID = 0;
            foreach (Thumb another_piece in pieces)
            {
                another_piece_ID++;
                if (selected_piece_ID == another_piece_ID) continue;
                if (AttachedProperty.GetTopSideProperty(pieces[selected_piece_ID]) && AttachedProperty.GetMainInsideProperty(another_piece) == true)
                {
                    var frame = another_piece.Template.FindName("MainFrame", another_piece) as UserControl;
                    piece_vertical_delta = Canvas.GetTop(pieces[selected_piece_ID]) - Canvas.GetTop(another_piece) - frame.ActualHeight + VERTUCALLY_DENT;
                    piece_horizontal_delta = Canvas.GetLeft(pieces[selected_piece_ID]) - Canvas.GetLeft(another_piece) - SIDE_LINE_DENT;
                    if (piece_vertical_delta > -5 && piece_vertical_delta < 30 && piece_horizontal_delta > -30 && piece_horizontal_delta < 30)
                    {
                        if (System.Math.Abs(piece_vertical_delta) < piece_min_vertical_delta)
                        {
                            piece_min_vertical_delta = piece_vertical_delta;
                            piece_min_horizontal_delta = piece_horizontal_delta;
                            return_piece_ID = another_piece_ID;
                            side = "MainInside";
                        }
                    }
                }
            }
            another_piece_ID = -1;
            foreach (Thumb another_piece in pieces)
            {
                another_piece_ID++;
                if (selected_piece_ID == another_piece_ID) continue;
                if (AttachedProperty.GetTopSideProperty(pieces[selected_piece_ID]) && AttachedProperty.GetSubInsideProperty(another_piece) == true)
                {
                    var if_frame = another_piece.Template.FindName("MainFrame", another_piece) as UserControl;
                    var else_frame = another_piece.Template.FindName("SubFrame", another_piece) as UserControl;
                    var if_side_line = another_piece.Template.FindName("MainSideLine", another_piece) as Border;
                    piece_vertical_delta = Canvas.GetTop(pieces[selected_piece_ID]) - Canvas.GetTop(another_piece) - if_frame.ActualHeight - if_side_line.ActualHeight - else_frame.ActualHeight + VERTUCALLY_DENT;
                    piece_horizontal_delta = Canvas.GetLeft(pieces[selected_piece_ID]) - Canvas.GetLeft(another_piece) - SIDE_LINE_DENT;
                    if (piece_vertical_delta > -5 && piece_vertical_delta < 30 && piece_horizontal_delta > -30 && piece_horizontal_delta < 30)
                    {
                        if (System.Math.Abs(piece_vertical_delta) < piece_min_vertical_delta)
                        {
                            piece_min_vertical_delta = piece_vertical_delta;
                            piece_min_horizontal_delta = piece_horizontal_delta;
                            return_piece_ID = another_piece_ID;
                            side = "SubInside";
                        }
                    }
                }
            }
            another_piece_ID = -1;
            foreach (Thumb another_piece in pieces)
            {
                another_piece_ID++;
                if (selected_piece_ID == another_piece_ID) continue;
                if (AttachedProperty.GetLeftSideProperty(pieces[selected_piece_ID]) && AttachedProperty.GetRightSideProperty(another_piece) == true)
                {
                    piece_vertical_delta = Canvas.GetTop(another_piece) - Canvas.GetTop(pieces[selected_piece_ID]);
                    piece_horizontal_delta = Canvas.GetLeft(pieces[selected_piece_ID]) - Canvas.GetLeft(another_piece) - another_piece.ActualWidth + SIDE_DENT;
                    if (piece_vertical_delta > -30 && piece_vertical_delta < 30 && piece_horizontal_delta > -5 && piece_horizontal_delta < 30)
                    {
                        if (System.Math.Abs(piece_vertical_delta) < piece_min_vertical_delta)
                        {
                            piece_min_vertical_delta = piece_vertical_delta;
                            piece_min_horizontal_delta = piece_horizontal_delta;
                            return_piece_ID = another_piece_ID;
                            side = "Right";
                        }
                    }
                }
            }
            another_piece_ID = -1;
            foreach (Thumb another_piece in pieces)
            {
                if(AttachedProperty.GetOwnPieceIDProperty(pieces[selected_piece_ID]) != 0)
                {
                    break;
                }
                another_piece_ID++;
                if (selected_piece_ID == another_piece_ID) continue;
                if (AttachedProperty.GetRightSideProperty(pieces[selected_piece_ID]) && AttachedProperty.GetLeftSideProperty(another_piece) == true)
                {
                    piece_vertical_delta = Canvas.GetTop(another_piece) - Canvas.GetTop(pieces[selected_piece_ID]);
                    piece_horizontal_delta = Canvas.GetLeft(another_piece) - Canvas.GetLeft(pieces[selected_piece_ID]) - pieces[selected_piece_ID].ActualWidth + SIDE_DENT;
                    if (piece_vertical_delta > -30 && piece_vertical_delta < 30 && piece_horizontal_delta > -5 && piece_horizontal_delta < 30)
                    {
                        if (System.Math.Abs(piece_vertical_delta) < piece_min_vertical_delta)
                        {
                            piece_min_vertical_delta = piece_vertical_delta;
                            piece_min_horizontal_delta = piece_horizontal_delta;
                            return_piece_ID = another_piece_ID;
                            side = "Left";
                        }
                    }
                }
            }
            another_piece_ID = -1;
            foreach (Thumb another_piece in pieces)
            {
                another_piece_ID++;
                if (selected_piece_ID == another_piece_ID) continue;
                if (AttachedProperty.GetTopSideProperty(pieces[selected_piece_ID]) && AttachedProperty.GetBottomSideProperty(another_piece) == true)
                {
                    piece_vertical_delta = Canvas.GetTop(pieces[selected_piece_ID]) - Canvas.GetTop(another_piece) - another_piece.ActualHeight + VERTUCALLY_DENT;
                    piece_horizontal_delta = Canvas.GetLeft(pieces[selected_piece_ID]) - Canvas.GetLeft(another_piece);
                    if (piece_vertical_delta > -5 && piece_vertical_delta < 30 && piece_horizontal_delta > -30 && piece_horizontal_delta < 30)
                    {
                        if (System.Math.Abs(piece_vertical_delta) < piece_min_vertical_delta)
                        {
                            piece_min_vertical_delta = piece_vertical_delta;
                            piece_min_horizontal_delta = piece_horizontal_delta;
                            return_piece_ID = another_piece_ID;
                            side = "Bottom";
                        }
                    }
                }
            }

            another_piece_ID = -1;
            foreach (Thumb another_piece in pieces)
            {
                another_piece_ID++;
                if (selected_piece_ID == another_piece_ID) continue;
                if (AttachedProperty.GetBottomSideProperty(pieces[selected_piece_ID]) && AttachedProperty.GetTopSideProperty(another_piece) == true)
                {
                    piece_vertical_delta = Canvas.GetTop(another_piece) - (Canvas.GetTop(pieces[selected_piece_ID]) + pieces[selected_piece_ID].ActualHeight) + VERTUCALLY_DENT;
                    piece_horizontal_delta = Canvas.GetLeft(pieces[selected_piece_ID]) - Canvas.GetLeft(another_piece);

                    if (piece_vertical_delta > -5 && piece_vertical_delta < 30 && piece_horizontal_delta > -30 && piece_horizontal_delta < 30)
                    {
                        if (System.Math.Abs(piece_vertical_delta) < piece_min_vertical_delta)
                        {
                            bool own_flag = false;
                            if (AttachedProperty.GetChainedPieceIDProperty(another_piece) != 0) own_flag = true;
                            foreach(int control_statement_ID in control_statement_IDs)
                            {
                                if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) == another_piece_ID || AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) == another_piece_ID) {
                                    own_flag = true;
                                    break;
                                }
                            }

                            if(own_flag == true)
                            {
                                if (side == "None")
                                {
                                    int targeted_piece_ID = another_piece_ID;
                                    double delta_x = 0;
                                    bool flag = false;
                                    while (flag)
                                    {
                                        flag = false;
                                        while (AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                                        {
                                            targeted_piece_ID = AttachedProperty.GetChainedPieceIDProperty(pieces[targeted_piece_ID]);
                                        }
                                        foreach (int control_statement_ID in control_statement_IDs)
                                        {
                                            if (AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]) != 0)
                                            {
                                                targeted_piece_ID = AttachedProperty.GetMainOwnPieceIDProperty(pieces[control_statement_ID]);
                                                flag = true;
                                            }
                                            else if (AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]) != 0)
                                            {
                                                targeted_piece_ID = AttachedProperty.GetSubOwnPieceIDProperty(pieces[control_statement_ID]);
                                                flag = true;
                                            }
                                        }
                                    }
                                    delta_x = Canvas.GetLeft(pieces[targeted_piece_ID]) - Canvas.GetLeft(pieces[selected_piece_ID]) - pieces[selected_piece_ID].ActualWidth - 30;
                                    if (AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID]) != 0)
                                    {
                                        delta_x += pieces[AttachedProperty.GetOwnPieceIDProperty(pieces[targeted_piece_ID])].ActualHeight;
                                    }
                                    Control_Piece("Move", selected_piece_ID, delta_x , 0);
                                }
                            }
                            else
                            {
                                piece_min_vertical_delta = piece_vertical_delta;
                                piece_min_horizontal_delta = piece_horizontal_delta;
                                return_piece_ID = another_piece_ID;
                                side = "Top";
                            }                         }
                    }
                }
            }
            return Tuple.Create(side, return_piece_ID, piece_min_vertical_delta, piece_min_horizontal_delta);
        }

        private void Pieces_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var piece = sender as Thumb;
            int ID = AttachedProperty.GetID(piece);
            Control_Piece("Move", ID, e.HorizontalChange, e.VerticalChange);
        }

        public void lost_focus(object sender, EventArgs e)
        {
            Thumb thumb = sender as Thumb;
            var obj = thumb.Template.FindName("TextBox1", thumb);
            TextBox textbox = obj as TextBox;
            if (textbox.Text == "") textbox.Text = "0";
            if (int.Parse(textbox.Text) > 100) textbox.Text = "100";
            if (int.Parse(textbox.Text) <= 0) textbox.Text = "0";
            else
            {
                while (textbox.Text.StartsWith("0"))
                {
                    textbox.Text = textbox.Text.Substring(1);
                }
            }
        }

        private void Set_Pieces()
        {
            double area_height = 0;
            foreach (string piece_name in pieces_name_list)
            {
                block_count++;
                var thumb = new Thumb();
                switch (piece_name)
                {
                    case "SetVariable":
                        AttachedProperty.SetType(thumb, "Function");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, true);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "Delay":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "Mortor":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "SerialPrint":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "BlueToothSerialPrint":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "AnalogOutPut":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "AnalogOutPutAsVariable":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "DigitalOutPut":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "GetWhatTimeIs":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "GetWhatDayIs":
                        AttachedProperty.SetType(thumb, "Command");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        break;

                    case "LogicVariableAndConst":
                        AttachedProperty.SetType(thumb, "Text");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, false);
                        AttachedProperty.SetLeftSideProperty(thumb, true);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, false);
                        break;

                    case "LogicVariableAndVariable":
                        AttachedProperty.SetType(thumb, "Text");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, false);
                        AttachedProperty.SetLeftSideProperty(thumb, true);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, false);
                        break;

                    case "IfElseVariable":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, true);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "IfBanper":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "IfElseBanper":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, true);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "IfElseConst":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, true);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "IfVariable":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "IfConst":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "DoOnce":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, false);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, false);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "WhileTrue":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, false);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, false);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "WhileVariable":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "WhileConst":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "ForVariable":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "ForConst":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;

                    case "ToSensorOutput":
                        AttachedProperty.SetType(thumb, "Text");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, false);
                        AttachedProperty.SetLeftSideProperty(thumb, true);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, false);
                        break;

                    case "ToNumber":
                        thumb.MouseLeave += lost_focus;
                        AttachedProperty.SetType(thumb, "Text");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, false);
                        AttachedProperty.SetLeftSideProperty(thumb, true);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, false);
                        break;

                    case "ToVariable":
                        AttachedProperty.SetType(thumb, "Text");
                        AttachedProperty.SetMainInsideProperty(thumb, false);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, false);
                        AttachedProperty.SetLeftSideProperty(thumb, true);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, false);
                        break;
                    case "IfWeatherIs":
                        AttachedProperty.SetType(thumb, "Control_Statement");
                        AttachedProperty.SetMainInsideProperty(thumb, true);
                        AttachedProperty.SetSubInsideProperty(thumb, false);
                        AttachedProperty.SetTopSideProperty(thumb, true);
                        AttachedProperty.SetLeftSideProperty(thumb, false);
                        AttachedProperty.SetRightSideProperty(thumb, false);
                        AttachedProperty.SetBottomSideProperty(thumb, true);
                        control_statement_IDs.Add(block_count);
                        break;
                }
                thumb.Tag = "Arranged";
                thumb.Template = WINDOW.Resources[piece_name] as ControlTemplate;
                thumb.Name = piece_name;
                thumb.DragStarted += Add_Pieces;
                AttachedProperty.SetID(thumb, block_count);
                area_height = 10 + thumb.ActualHeight;
                ArrangeArea.Children.Add(thumb);
                Canvas.SetLeft(thumb, 10);
                Canvas.SetTop(thumb, 10);
                pieces.Add(thumb);
            }
            ArrangeArea.Height += area_height;
            ArrangeArea.Height += 2000;
        }

        private void Arrange_Pieces()
        {
            double set_y = 10;
            for(int i=2; i<=block_count; i++)
            {
                set_y += 10;
                set_y += pieces[i-1].ActualHeight;
                Canvas.SetTop(pieces[i], set_y);
            }
        }
        private void Set_Combobox()
        {
            object obj;
            for (int i=1; i<=block_count; i++)
            {
                try
                {
                    obj = pieces[i].Template.FindName("ComboBox1", pieces[i]);
                    ComboBox combobox1 = obj as ComboBox;
                    combobox1.SelectedIndex = 0;
                    try
                    {
                        obj = pieces[i].Template.FindName("ComboBox2", pieces[i]);
                        ComboBox combobox2 = obj as ComboBox;
                        if (combobox2 == null) continue;
                        combobox2.SelectedIndex = 0;
                        try
                        {
                            obj = pieces[i].Template.FindName("ComboBox3", pieces[i]);
                            ComboBox combobox3 = obj as ComboBox;
                            if (combobox3 == null) continue;
                            combobox3.SelectedIndex = 0;
                        }
                        catch 
                        { 

                        }
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }
            }
        }
        private void Add_Pieces(object sender, DragStartedEventArgs e)
        {
            Set_new_piece(sender);
        }

        private void Set_new_piece(object sender)
        {
            var replace_piece = sender as Thumb;
            var reset_piece = new Thumb();
            var new_replace_piece = new Thumb();
            object obj = FindName("ScrollViewer");
            List<int> combo_box_list = new List<int>();
            double set_x = Canvas.GetLeft(replace_piece);
            double set_y = Canvas.GetTop(replace_piece);
            block_count++;
            reset_piece.Template = replace_piece.Template;
            reset_piece.Tag = "Arranged";
            reset_piece.Name = replace_piece.Name;
            AttachedProperty.SetType(reset_piece, AttachedProperty.GetType(replace_piece));
            AttachedProperty.SetID(reset_piece, block_count);
            AttachedProperty.SetMainInsideProperty(reset_piece, AttachedProperty.GetMainInsideProperty(replace_piece));
            AttachedProperty.SetSubInsideProperty(reset_piece, AttachedProperty.GetSubInsideProperty(replace_piece));
            AttachedProperty.SetTopSideProperty(reset_piece, AttachedProperty.GetTopSideProperty(replace_piece));
            AttachedProperty.SetLeftSideProperty(reset_piece, AttachedProperty.GetLeftSideProperty(replace_piece));
            AttachedProperty.SetRightSideProperty(reset_piece, AttachedProperty.GetRightSideProperty(replace_piece));
            AttachedProperty.SetBottomSideProperty(reset_piece, AttachedProperty.GetBottomSideProperty(replace_piece));
            if (AttachedProperty.GetType(replace_piece) == "Control_Statement")
            {
                control_statement_IDs.Add(block_count);
            }
            pieces.Add(reset_piece);
            reset_piece.DragStarted += Add_Pieces;
            Canvas.SetLeft(reset_piece, set_x);
            Canvas.SetTop(reset_piece, set_y);
            ArrangeArea.Children.Add(reset_piece);
            replace_piece.Tag = "ResetConboBox";
            replace_piece.DragStarted -= Add_Pieces;
            replace_piece.DragStarted += Pieces_DragStarted;
            replace_piece.DragCompleted += Pieces_DragCompleted;
            replace_piece.DragDelta += Reset_New_ComboBox;
            if (replace_piece.Name == "AnalogOutPut") replace_piece.MouseLeave += lost_focus;
            if (obj != null)
            {
                var scroll_viewer = (ScrollViewer)obj;
                set_y -= scroll_viewer.VerticalOffset;
            }
            Canvas.SetLeft(replace_piece, set_x + 10);
            Canvas.SetTop(replace_piece, set_y + 10);
            try
            {
                ComboBox combobox = replace_piece.Template.FindName("ComboBox1", replace_piece) as ComboBox;
                combo_box_list.Add(combobox.SelectedIndex);
                try
                {
                    combobox = replace_piece.Template.FindName("ComboBox2", replace_piece) as ComboBox;
                    combo_box_list.Add(combobox.SelectedIndex);
                    try
                    {
                        combobox = replace_piece.Template.FindName("ComboBox3", replace_piece) as ComboBox;
                        combo_box_list.Add(combobox.SelectedIndex);
                       
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }
            }
            catch
            {

            }
            ArrangeArea.Children.Remove(replace_piece);
            area.Children.Add(replace_piece);
            Control_Piece("Stand_Out", AttachedProperty.GetID(replace_piece), 0, 0);
            try
            {
                ComboBox combobox = replace_piece.Template.FindName("ComboBox1", replace_piece) as ComboBox;
                combobox.SelectedIndex = combo_box_list[0];
                try
                {
                    combobox = replace_piece.Template.FindName("ComboBox2", replace_piece) as ComboBox;
                    combobox.SelectedIndex = combo_box_list[1];
                    try
                    {
                        combobox = replace_piece.Template.FindName("ComboBox3", replace_piece) as ComboBox;
                        combobox.SelectedIndex = combo_box_list[2];
                        
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }
            }
            catch
            {

            }
        }

        

        private void Reset_New_ComboBox(object sender, DragDeltaEventArgs e)
        {
            Thumb thumb = sender as Thumb;
            thumb.DragDelta -= Reset_New_ComboBox;
            thumb.DragDelta += Pieces_DragDelta;
            thumb.Tag = "Normal";
            try
            {
                ComboBox combobox = pieces[block_count].Template.FindName("ComboBox1", pieces[block_count]) as ComboBox;
                combobox.SelectedIndex = 0;
                try
                {
                    combobox = pieces[block_count].Template.FindName("ComboBox2", pieces[block_count]) as ComboBox;
                    combobox.SelectedIndex = 0;
                    try
                    {
                        combobox = pieces[block_count].Template.FindName("ComboBox3", pieces[block_count]) as ComboBox;
                        combobox.SelectedIndex = 0;
                        
                    }
                    catch
                    {

                    }
                   
                }
                catch
                {

                }
            }
            catch
            {

            }
        }
        private void Save_Blocks(object sender, RoutedEventArgs e)
        {
            string block_data = "";
            var piece = new Thumb();

            block_data += num;
            block_data += ", ";
            block_data += block_count;
            block_data += ", [";
            block_data += String.Join(",", control_statement_IDs);
            block_data += "], [";
            block_data += String.Join(",", raw_piece_IDs);
            block_data += "], [";
            block_data += String.Join(",", copied_piece_IDs);
            block_data += "]\r\n";
            for (int i = 0; i <= pieces.Count; i++)
            {
                try
                {

                    piece = pieces[i];
                }
                catch
                {
                    continue;
                }

                if ((string)((Thumb)piece).Tag == "ResetComboBox") continue;
                if ((string)((Thumb)piece).Tag == "Removed") continue;

                block_data += "$";
                block_data += AttachedProperty.GetID(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetType(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetOwnPieceIDProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetOwnedPieceIDProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetChainPieceIDProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetChainedPieceIDProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetMainOwnPieceIDProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetSubOwnPieceIDProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetConnectedIDProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetConnectIDProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetMainInsideProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetSubInsideProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetTopSideProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetLeftSideProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetRightSideProperty(piece);
                block_data += ", ";
                block_data += AttachedProperty.GetBottomSideProperty(piece);
                block_data += ", ";
                block_data += piece.Tag;
                block_data += ", ";
                block_data += piece.Name;
                block_data += ", ";
                block_data += (int)Canvas.GetLeft(piece);
                block_data += ", ";
                block_data += (int)Canvas.GetTop(piece);
                block_data += ", ";
                block_data += Canvas.GetZIndex(piece);


                block_data += ", ";
                block_data += "[";
                if (AttachedProperty.GetType(piece) == "Control_Statement")
                {
                    var main_side_line = new Border();
                    var sub_side_line = new Border();
                    if (piece.Template.FindName("MainSideLine", piece) != null)
                    {
                        main_side_line = piece.Template.FindName("MainSideLine", piece) as Border;
                        block_data += main_side_line.Height;
                    }

                    if (piece.Template.FindName("SubSideLine", piece) != null)
                    {
                        sub_side_line = piece.Template.FindName("SubSideLine", piece) as Border;
                        block_data += ",";
                        block_data += sub_side_line.Height;
                    }
                }
                block_data += "]";
                block_data += ", ";
                block_data += "[";
                try
                {
                    ComboBox combobox = piece.Template.FindName("ComboBox1", piece) as ComboBox;
                    int combobox_item = combobox.SelectedIndex;
                    block_data += combobox_item;
                    try
                    {
                        combobox = piece.Template.FindName("ComboBox2", piece) as ComboBox;
                        combobox_item = combobox.SelectedIndex;
                        block_data += ",";
                        block_data += combobox_item;
                        try
                        {
                            combobox = piece.Template.FindName("ComboBox3", piece) as ComboBox;
                            combobox_item = combobox.SelectedIndex;
                            block_data += ",";
                            block_data += combobox_item;

                        }
                        catch
                        {

                        }
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }
                block_data += "]";
                block_data += ", ";
                block_data += "[";
                try
                {
                    TextBox textbox = piece.Template.FindName("TextBox1", piece) as TextBox;
                    string textbox_text = textbox.Text;
                    block_data += textbox_text;
                    try
                    {
                        textbox = piece.Template.FindName("TextBox2", piece) as TextBox;
                        textbox_text = textbox.Text;
                        block_data += ",";
                        block_data += textbox_text;
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }
                block_data += "]";

                block_data += "\r\n";

            }

            //StreamWriter block_data_file = new StreamWriter("block_datas.txt", false, Encoding.GetEncoding("Shift_JIS"));
            //block_data_file.WriteLine(block_data);
            //block_data_file.Close();

            string filePath = System.IO.Path.GetFullPath(@".\SaveFiles");

            SaveFileDialog dlg = new SaveFileDialog();
            dlg.FileName = "*.blockdata"; //はじめに「ファイル名」で表示される文字列を指定する
            dlg.InitialDirectory = filePath;  //はじめに表示されるフォルダを指定する
            dlg.AddExtension = true; // ユーザーが拡張子を省略したときに、自動的に拡張子を付けるか。規定値はtrue。
            dlg.CheckFileExists = false; // ユーザーが存在しないファイルを指定したときに、警告するか。規定値はfalse。
            dlg.CheckPathExists = true; // ユーザーが存在しないパスを指定したときに、警告するか。規定値はtrue。
            dlg.CreatePrompt = false; // ユーザーが存在しないファイルを指定したときに、作成の許可を求めるか。規定値はfalse。
            dlg.CustomPlaces = null; // ダイアログ左側のショートカットのリスト。
            dlg.DefaultExt = string.Empty; // ダイアログに表示するファイルの拡張子。規定値はEmpty。
            dlg.DereferenceLinks = false; // ショートカットが参照先を返す場合はtrue。リンクファイルを返す場合はfalse。規定値はfalse。
            dlg.Filter = string.Empty; // ダイアログで表示するファイルの種類のフィルタを指定する文字列。
            dlg.FilterIndex = 1; // 選択されたFilterのインデックス。規定値は1。
            dlg.OverwritePrompt = true; // 存在するファイルを指定したときに、警告するか。規定値はtrue。
            dlg.Title = "保存するファイル名を入力してください。"; // ダイアログのタイトル。
            dlg.ValidateNames = true; // ファイル名がWin32に適合するか検査するかどうか。規定値はfalse。

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;

                Encoding enc = Encoding.GetEncoding("shift_jis");
                try
                {
                    File.WriteAllText(filename, block_data, enc);
                }
                catch
                {
                    return;
                }
            }
            else
            {
                return;
            }
            MessageBox.Show("編集データを保存が完了しました。", "保存データの読み込み", MessageBoxButton.OK, MessageBoxImage.Information);
        
        }

        private List<int> StringToList(string data)
        {
            data = data.Replace("[", "").Replace("]", "");
            var list_datas = data.Split(',');
            var output = new List<int>();
            foreach (string list_data in list_datas)
            {
                try
                {
                    output.Add(int.Parse(list_data));
                }
                catch
                {
                    continue;
                }
            }
            return output;
        }

        private void Load_Blocks(object sender, RoutedEventArgs e)
        {
            //OpenFileDialogクラスのインスタンスを作成
            OpenFileDialog ofd = new OpenFileDialog();

            string raw_block_datas = "";

            //[ファイルの種類]ではじめに選択されるものを指定する
            //2番目の「すべてのファイル」が選択されているようにする
            ofd.FilterIndex = 2;
            //タイトルを設定する
            ofd.Title = "ファイルを選択";
            //ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            ofd.RestoreDirectory = true;

            // 開くボタン以外が押下された場合
            if (ofd.ShowDialog() == true)
            {
                string filename = ofd.FileName;

                Encoding enc = Encoding.GetEncoding("shift_jis");
                try
                {
                    StreamReader block_datas_file = new StreamReader(ofd.FileName, Encoding.GetEncoding("Shift_JIS"));
                    raw_block_datas = block_datas_file.ReadToEnd();
                    block_datas_file.Close();
                }
                catch 
                {
                    return;
                }
            }
            else
            {
                return;
            }


            

            var temp_pieces = new List<Thumb>(pieces);
            for (int i= 1; i < temp_pieces.Count; i++)
            {
                if((string)((Thumb)temp_pieces[i]).Tag  == "Normal")
                {
                    area.Children.Remove(temp_pieces[i]);

                    pieces.Remove(temp_pieces[i]);
                }
                if ((string)((Thumb)temp_pieces[i]).Tag == "Arranged")
                {
                    ArrangeArea.Children.Remove(temp_pieces[i]);

                    pieces.Remove(temp_pieces[i]);
                }
                if ((string)((Thumb)temp_pieces[i]).Tag == "Copied")
                {
                    area.Children.Remove(temp_pieces[i]);

                    pieces.Remove(temp_pieces[i]);
                }
                if ((string)((Thumb)temp_pieces[i]).Tag == "ResetComboBox")
                {
                    area.Children.Remove(temp_pieces[i]);

                    pieces.Remove(temp_pieces[i]);
                }
                if ((string)((Thumb)temp_pieces[i]).Tag == "Removed")
                {
                    area.Children.Remove(temp_pieces[i]);

                    pieces.Remove(temp_pieces[i]);
                }
            }

            

            raw_block_datas = raw_block_datas.Replace("\r", "").Replace("\n", "");


            var block_datas = raw_block_datas.Split('$');

            var header = Regex.Split(block_datas[0], ", ");
            num = int.Parse(header[0]);
            control_statement_IDs = StringToList(header[2]);
            raw_piece_IDs = StringToList(header[3]);
            copied_piece_IDs = StringToList(header[4]);


            Set_Loaded_Blocks(block_datas);

            var Progress = new progress_bar();
            Progress.Show();

            block_count = pieces.Count-1;
            Adjust_Blocks(block_datas);
            Progress.Close();
            MessageBoxResult result = MessageBox.Show("編集データを読み込みが完了しました。", "保存データの読み込み", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void Set_Loaded_Blocks(string[] block_datas)
        {
            for (int i = 2; i <= block_datas.Length - 1; i++)
            {
                var piece = new Thumb();
                var block_data = Regex.Split(block_datas[i], ", ");




                piece.Tag = block_data[16];

                switch (piece.Tag)
                {
                    case "Normal":
                        piece.DragStarted += Pieces_DragStarted;
                        piece.DragCompleted += Pieces_DragCompleted;
                        piece.DragDelta += Pieces_DragDelta;

                        area.Children.Add(piece);

                        break;

                    case "Arranged":

                        piece.DragStarted += Add_Pieces;

                        ArrangeArea.Children.Add(piece);

                        break;

                    case "Copied":
                        piece.DragStarted += Copy_Piece;
                        piece.DragCompleted += Pieces_DragCompleted;
                        piece.DragDelta += Pieces_DragDelta;

                        area.Children.Add(piece);

                        break;
                }

                AttachedProperty.SetID(piece, int.Parse(block_data[0]));
                AttachedProperty.SetType(piece, block_data[1]);

                AttachedProperty.SetOwnPieceIDProperty(piece, int.Parse(block_data[2]));
                AttachedProperty.SetOwnedPieceIDProperty(piece, int.Parse(block_data[3]));
                AttachedProperty.SetChainPieceIDProperty(piece, int.Parse(block_data[4]));
                AttachedProperty.SetChainedPieceIDProperty(piece, int.Parse(block_data[5]));
                AttachedProperty.SetMainOwnPieceIDProperty(piece, int.Parse(block_data[6]));
                AttachedProperty.SetSubOwnPieceIDProperty(piece, int.Parse(block_data[7]));
                AttachedProperty.SetConnectedIDProperty(piece, int.Parse(block_data[8]));
                AttachedProperty.SetConnectIDProperty(piece, int.Parse(block_data[9]));

                AttachedProperty.SetMainInsideProperty(piece, bool.Parse(block_data[10]));
                AttachedProperty.SetSubInsideProperty(piece, bool.Parse(block_data[11]));
                AttachedProperty.SetTopSideProperty(piece, bool.Parse(block_data[12]));
                AttachedProperty.SetLeftSideProperty(piece, bool.Parse(block_data[13]));
                AttachedProperty.SetRightSideProperty(piece, bool.Parse(block_data[14]));
                AttachedProperty.SetBottomSideProperty(piece, bool.Parse(block_data[15]));

                piece.Name = block_data[17];
                piece.Template = WINDOW.Resources[piece.Name] as ControlTemplate;


                Canvas.SetLeft(piece, int.Parse(block_data[18]));
                Canvas.SetTop(piece, int.Parse(block_data[19]));
                Canvas.SetZIndex(piece, int.Parse(block_data[20]));

                pieces.Add(piece);
            }
        }

        private void Adjust_Blocks(string[] block_datas)
        {
            for (int i = 2; i <= block_datas.Length - 1; i++)
            {


                var piece = pieces[i-1];
                var block_data = Regex.Split(block_datas[i], ", ");

                if (AttachedProperty.GetType(piece) == "Control_Statement")
                {


                    var line_heights = StringToList(block_data[21]);
                    var main_side_line = new Border();
                    var sub_side_line = new Border();

                    if (piece.Template.FindName("MainSideLine", piece) != null)
                    {
                        main_side_line = piece.Template.FindName("MainSideLine", piece) as Border;
                        main_side_line.Height = line_heights[0];
                    }

                    if (piece.Template.FindName("SubSideLine", piece) != null)
                    {
                        sub_side_line = piece.Template.FindName("SubSideLine", piece) as Border;
                        sub_side_line.Height = line_heights[1];
                    }
                }

                var combo_box_list = StringToList(block_data[22]);
                try
                {
                    ComboBox combobox = piece.Template.FindName("ComboBox1", piece) as ComboBox;
                    combobox.SelectedIndex = combo_box_list[0];
                    try
                    {
                        combobox = piece.Template.FindName("ComboBox2", piece) as ComboBox;
                        combobox.SelectedIndex = combo_box_list[1];
                        try
                        {
                            combobox = piece.Template.FindName("ComboBox3", piece) as ComboBox;
                            combobox.SelectedIndex = combo_box_list[2];

                        }
                        catch
                        {

                        }
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }

                var text_box_list = block_data[22].Replace("]","").Replace("[","").Split(',');
                try
                {
                    TextBox textbox = piece.Template.FindName("TextBox1", piece) as TextBox;
                    textbox.Text = text_box_list[0];
                    try
                    {
                        textbox = piece.Template.FindName("TextBox2", piece) as TextBox;
                        textbox.Text = text_box_list[1];
                    }
                    catch
                    {

                    }
                }
                catch
                {

                }

                

            }
        }

        private void Setting_WiFi_Point(object sender, RoutedEventArgs e)
        {
            wifi_setting WiFiWindow = new wifi_setting();
            WiFiWindow.Show();
        }
        private void Setting_Blue_Tooth_Device(object sender, RoutedEventArgs e)
        {
            bluetooth_setting BlueToothWindow = new bluetooth_setting();
            BlueToothWindow.Show();
        }
        private void Setting_API_key(object sender, RoutedEventArgs e)
        {
            key_setting APIkeyWindow = new key_setting();
            APIkeyWindow.Show();
        }

        public class AttachedProperty
        {
            public static readonly DependencyProperty ID =
                DependencyProperty.RegisterAttached("ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(1));
            public static readonly DependencyProperty Type =
               DependencyProperty.RegisterAttached("is_control", typeof(string), typeof(AttachedProperty), new PropertyMetadata(""));

            public static readonly DependencyProperty OwnPieceIDProperty =
                DependencyProperty.RegisterAttached("own_ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(0));
            public static readonly DependencyProperty OwnedPieceIDProperty =
               DependencyProperty.RegisterAttached("owned_ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(0));

            public static readonly DependencyProperty ChainPieceIDProperty =
               DependencyProperty.RegisterAttached("chain_ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(0));
            public static readonly DependencyProperty ChainedPieceIDProperty =
               DependencyProperty.RegisterAttached("chained_ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(0));

            public static readonly DependencyProperty SubOwnPieceIDProperty =
                DependencyProperty.RegisterAttached("sub_own_ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(0));
            public static readonly DependencyProperty MainOwnPieceIDProperty =
                DependencyProperty.RegisterAttached("main_own_ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(0));


            public static readonly DependencyProperty ConnectedIDProperty =
                DependencyProperty.RegisterAttached("connected_ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(0));
            public static readonly DependencyProperty ConnectIDProperty =
                DependencyProperty.RegisterAttached("connect_ID", typeof(int), typeof(AttachedProperty), new PropertyMetadata(0));

            public static readonly DependencyProperty SubInsideProperty =
               DependencyProperty.RegisterAttached("sub_in_side", typeof(bool), typeof(AttachedProperty), new PropertyMetadata(false));
            public static readonly DependencyProperty MainInsideProperty =
                DependencyProperty.RegisterAttached("main_in_side", typeof(bool), typeof(AttachedProperty), new PropertyMetadata(false));
            public static readonly DependencyProperty TopSideProperty =
                DependencyProperty.RegisterAttached("top_side", typeof(bool), typeof(AttachedProperty), new PropertyMetadata(false));
            public static readonly DependencyProperty LeftSideProperty =
                DependencyProperty.RegisterAttached("left_side", typeof(bool), typeof(AttachedProperty), new PropertyMetadata(false));
            public static readonly DependencyProperty RightSideProperty =
                DependencyProperty.RegisterAttached("right_side", typeof(bool), typeof(AttachedProperty), new PropertyMetadata(false));
            public static readonly DependencyProperty BottomSideProperty =
                DependencyProperty.RegisterAttached("bottom_side", typeof(bool), typeof(AttachedProperty), new PropertyMetadata(false));




            public static int GetID(DependencyObject obj)
            {
                return (int)obj.GetValue(ID);
            }
            public static string GetType(DependencyObject obj)
            {
                return (string)obj.GetValue(Type);
            }
            public static int GetOwnPieceIDProperty(DependencyObject obj)
            {
                return (int)obj.GetValue(OwnPieceIDProperty);
            }
            public static int GetOwnedPieceIDProperty(DependencyObject obj)
            {
                return (int)obj.GetValue(OwnedPieceIDProperty);
            }
            public static int GetChainPieceIDProperty(DependencyObject obj)
            {
                return (int)obj.GetValue(ChainPieceIDProperty);
            }
            public static int GetChainedPieceIDProperty(DependencyObject obj)
            {
                return (int)obj.GetValue(ChainedPieceIDProperty);
            }

            public static int GetMainOwnPieceIDProperty(DependencyObject obj)
            {
                return (int)obj.GetValue(MainOwnPieceIDProperty);
            }
            public static int GetSubOwnPieceIDProperty(DependencyObject obj)
            {
                return (int)obj.GetValue(SubOwnPieceIDProperty);
            }
           
            public static int GetConnectedIDProperty(DependencyObject obj)
            {
                return (int)obj.GetValue(ConnectedIDProperty);
            }
            public static int GetConnectIDProperty(DependencyObject obj)
            {
                return (int)obj.GetValue(ConnectIDProperty);
            }

            public static bool GetMainInsideProperty(DependencyObject obj)
            {
                return (bool)obj.GetValue(MainInsideProperty);
            }
            public static bool GetSubInsideProperty(DependencyObject obj)
            {
                return (bool)obj.GetValue(SubInsideProperty);
            }
            public static bool GetTopSideProperty(DependencyObject obj)
            {
                return (bool)obj.GetValue(TopSideProperty);
            }
            public static bool GetLeftSideProperty(DependencyObject obj)
            {
                return (bool)obj.GetValue(LeftSideProperty);
            }
            public static bool GetRightSideProperty(DependencyObject obj)
            {
                return (bool)obj.GetValue(RightSideProperty);
            }
            public static bool GetBottomSideProperty(DependencyObject obj)
            {
                return (bool)obj.GetValue(BottomSideProperty);
            }


            public static void SetID(DependencyObject obj, int value)
            {
                obj.SetValue(ID, value);
            }
            public static void SetType(DependencyObject obj, string value)
            {
                obj.SetValue(Type, value);
            }
            public static void SetOwnPieceIDProperty(DependencyObject obj, int value)
            {
                obj.SetValue(OwnPieceIDProperty, value);
            }
            public static void SetOwnedPieceIDProperty(DependencyObject obj, int value)
            {
                obj.SetValue(OwnedPieceIDProperty, value);
            }

            public static void SetChainPieceIDProperty(DependencyObject obj, int value)
            {
                obj.SetValue(ChainPieceIDProperty, value);
            }
            public static void SetChainedPieceIDProperty(DependencyObject obj, int value)
            {
                obj.SetValue(ChainedPieceIDProperty, value);
            }

            public static void SetMainOwnPieceIDProperty(DependencyObject obj, int value)
            {
                obj.SetValue(MainOwnPieceIDProperty, value);
            }
            public static void SetSubOwnPieceIDProperty(DependencyObject obj, int value)
            {
                obj.SetValue(SubOwnPieceIDProperty, value);
            }
           
            public static void SetConnectedIDProperty(DependencyObject obj, int value)
            {
                obj.SetValue(ConnectedIDProperty, value);
            }
            public static void SetConnectIDProperty(DependencyObject obj, int value)
            {
                obj.SetValue(ConnectIDProperty, value);
            }
            public static void SetMainInsideProperty(DependencyObject obj, bool value)
            {
                obj.SetValue(MainInsideProperty, value);
            }
            public static void SetSubInsideProperty(DependencyObject obj, bool value)
            {
                obj.SetValue(SubInsideProperty, value);
            }
            public static void SetTopSideProperty(DependencyObject obj, bool value)
            {
                obj.SetValue(TopSideProperty, value);
            }
            public static void SetLeftSideProperty(DependencyObject obj, bool value)
            {
                obj.SetValue(LeftSideProperty, value);
            }
            public static void SetRightSideProperty(DependencyObject obj, bool value)
            {
                obj.SetValue(RightSideProperty, value);
            }
            public static void SetBottomSideProperty(DependencyObject obj, bool value)
            {
                obj.SetValue(BottomSideProperty, value);
            }
        }

        private void Adjust_Blocks_Event(object sender, RoutedEventArgs e)
        {
            StreamReader block_datas_file = new StreamReader("block_datas.txt", Encoding.GetEncoding("Shift_JIS"));
            string raw_block_datas = block_datas_file.ReadToEnd();
            block_datas_file.Close();

            raw_block_datas = raw_block_datas.Replace("\r", "").Replace("\n", "");


            var block_datas = raw_block_datas.Split('$');
            Adjust_Blocks(block_datas);
        }
    }

    public class EventDatum
    {
        public static Delegate GetEventHandler(object obj, string eventName)
        {
            EventHandlerList ehl = GetEvents(obj);
            object key = GetEventKey(obj, eventName);
            return ehl[key];
        }



        private delegate MethodInfo delGetEventsMethod(Type objType, delGetEventsMethod callback);
        private static EventHandlerList GetEvents(object obj)
        {
            delGetEventsMethod GetEventsMethod = delegate (Type objtype, delGetEventsMethod callback)
            {
                MethodInfo mi = objtype.GetMethod("get_Events", All);
                if ((mi == null) & (objtype.BaseType != null))
                    mi = callback(objtype.BaseType, callback);
                return mi;
            };

            MethodInfo methodInfo = GetEventsMethod(obj.GetType(), GetEventsMethod);
            if (methodInfo == null) return null;
            return (EventHandlerList)methodInfo.Invoke(obj, new object[] { });
        }



        private delegate FieldInfo delGetKeyField(Type objType, string eventName, delGetKeyField callback);
        private static object GetEventKey(object obj, string eventName)
        {
            delGetKeyField GetKeyField = delegate (Type objtype, string eventname, delGetKeyField callback)
            {
                FieldInfo fi = objtype.GetField("Event" + eventName, All);
                if ((fi == null) & (objtype.BaseType != null))
                    fi = callback(objtype.BaseType, eventName, callback);
                return fi;
            };

            FieldInfo fieldInfo = GetKeyField(obj.GetType(), eventName, GetKeyField);
            if (fieldInfo == null) return null;
            return fieldInfo.GetValue(obj);
        }



        private static BindingFlags All
        {
            get
            {
                return
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.IgnoreCase |
                    BindingFlags.Static;
            }
        }
    }

    public partial class MainViewModel : ResourceDictionary
    {
        private string[] compare_items = new string[]
         { "＝", "＞", "＜", "≧", "≦" };
        public string[] CompareItems
        {
            get { return compare_items; }
        }

        private string[] logic_items = new string[]
        { "＋", "－", "÷", "×", "％" };
        public string[] LogicItems
        {
            get { return logic_items; }
        }

        private string[] time_items = new string[]
        { "年", "月", "日", "時間", "分数", "秒数" };
        public string[] TimeItems
        {
            get { return time_items; }
        }

        public string[] variable_items = new string[]
        { "変数a", "変数b", "変数c", "変数d", "変数e", "変数f", "変数g", "変数h", "変数i", "変数j",
          "変数k", "変数n", "変数m", "変数n", "変数o", "変数p", "変数q", "変数r", "変数s", "変数t",
          "変数u", "変数v", "変数w", "変数x", "変数y", "変数z" };
        public string[] VariableItems
        {
            get { return variable_items; }
        }

        public string[] banper_items = new string[]
       { "右バンパー", "左バンパー" };
        public string[] BanperItems
        {
            get { return banper_items; }
        }

        public string[] analog_pin_items = new string[]
        { "ピン2", "ピン4", "ピン12", "ピン13", "ピン15" };
        public string[] AnalogPinItems
        {
            get { return analog_pin_items; }
        }
        public string[] digital_pin_items = new string[]
        { "ピン16", "ピン17", "ピン27", "ピン32", "ピン33" };

        public string[] DigitalPinItems
        {
            get { return digital_pin_items; }
        }
        public string[] bool_items = new string[]
       { "オン", "オフ" };

        public string[] BoolItems
        {
            get { return bool_items; }
        }
        public string[] banper_bool_items = new string[]
        { "押されて", "離れて" };

        public string[] BanperBoolItems
        {
            get { return banper_bool_items; }
        }

        public string[] weather_items = new string[]
        { "晴れ", "曇り", "雨" };
        public string[] WeatherItems
        {
            get { return weather_items; }
        }

        public void TextBox_Allow_Numbers(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        public void lost_focus(object sender, EventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox.Text == "") textbox.Text = "0";
            if (int.Parse(textbox.Text) > 100) textbox.Text = "100";
            if(int.Parse(textbox.Text) <= 0) textbox.Text = "0";
            else
            {
                while (textbox.Text.StartsWith("0"))
                {
                    textbox.Text = textbox.Text.Substring(1);
                }
            }            
        }
    }
}

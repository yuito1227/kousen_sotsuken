   M S B A M L   `   `   `  ����H  Dblocktemplate, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:blocktemplateblocktemplate  5   	       blocktemplate.MainWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:blocktemplate     5   	   -   5        $��WINDOW��5   ,     � Title$   ESP32制御支援ツール��5   	   $
��1050��6   $
��1920��6,    � 	Resources 5	       �� Source�� 5
   
   $  BlockTemplates/Blocks.xaml?�6   %    .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      .+�5   
   o� .@�5      m� $��オプション��6   .@�5      m� -   5        $��Wi-FI設定��5      6-   m� 5      -   5        $��Bluetooth設定��5      61   m� 5      -   5        $��APIキー設定��5      6-   m� 5      -   5        $��show_source_code��5      $B��<スクリプト変換時にソースコードを表示する��64    m� IsCheckable .  m� 	IsChecked . 6g   m� 5      -   5        $��show_work_space��5      $�� 枠を表示する��63    .  . 6Y   m� 5      -   5        $��ファイルに保存��5      6-   m� 5      -   5        $$��ファイルから読み込む��5      60   5      5      �� 5   
   -	   5        $
��area��5   )   
�������$��1��65   .+�5      �� -
   5         ��Name$ 	TrashArea��5      
������� �� Left$ 10��6=   ���	#FF878686
 �� Top$ 10��6f   $	��300��6v   $��2q�6�   $	��980��6�   �� 5      -   5        $ CopyArea��5      ���	#FFA5E6F9���	#FF0080FF$ 10��6W   $	��300��6g   $��2q�6s   $	��300��6�   	 �� Right$	 10��6�   5      �� 5       -   5        $ 	BuildArea��5       ���	#FFBCF9A5���	#FF117E00$	 350��6X   $	��300��6i   $��2q�6u   $	��630��6�   $	 10��6�   �� 5!      -   5        
 ��Name$
 ScrollViewer��5!      $	��980��61   $ 10��6>   $ 10��6O   .��5"      �� -   5         ��Name$ ArrangeArea��5"      $ 10��6.   $	��300��6A   $	��980��6M   5#      5$      5&      5'      
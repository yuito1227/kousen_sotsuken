   M S B A M L   `   `   `  ����H  Dblocktemplate, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:blocktemplateblocktemplate  5   	        blocktemplate.wifi_setting   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:blocktemplate     5   	     � Title$  Wi-Fiポイント設定��5   	   $	��170��6   $	��390��6*   .��5	      �  � RowDefinitions 5
   
   �� 5      $��30 �6   �� 5      $��19 �6   �� 5      $��8 �6   �� 5      $��19 �6   �� 5      $��* �6   �� 5      $��23 �6   �� 5      $��12 �6    � ColumnDefinitions 5   
   �� 5      $��12 �6   �� 5      $	��60* �6   �� 5      $��8 �6   �� 5      $
��120* �6   �� 5      $��8 �6   �� 5      $��75 �6   �� 5      $��12 �6   .+�5   
   �� -   5        $��
TextBlock1��5   2   $��1��5      $��1��6$   $
��Auto��61   $��Stretch=�5      $!��アクセスポイント名��5      $��Center=�6%    �� TextAlignment$ Right=�6@   �� 5   
   -   5        $��
TextBlock2��5    2   $��1��5      $��3��6$   $
��Auto��61   $��Stretch=�5       $��パスワード��5!      $��Center=�6!   $ Right=�6<   �� 5"   
   -   5        $
��SSID��5$      $��3��5"      $��1��6"   $��3��6/   $
��Auto��5#      $��Stretch=�6    $��Center=�5$      G� 5%   
   -   5        $��Password��5'      $��3��5%      $��3��6&   $��3��63   $
��Auto��5&      $��Stretch=�6    $��Center=�5'   "   �� 5(   
   -   5        $��btnLogin��5(   d   $��設定完了��6   $��3��6    $��5��60   $��75��6=   $��Right=�6H   6t   �� 5)   
   -   5        $��Cancel��5*   D   $��キャンセル��5)      $��5��6!   $��5��61   $��23��5*      $��75��6   $��Right=�6(   $��Bottom=�5+      6-   5,      5-      
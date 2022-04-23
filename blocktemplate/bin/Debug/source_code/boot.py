# -*- coding: utf-8 -*-
"""
Created on Sat Nov 16 13:40:25 2019

@author: koreto
"""

def connect():
    import network
 
    ssid = "<Buffalo-A-70DD"
    password =  "nkuf6bh8k477c"
 
    station = network.WLAN(network.STA_IF)
 
    if station.isconnected() == True:
        print("接続済みです")
        return
 
    station.active(True)
    station.connect(ssid, password)
 
    while station.isconnected() == False:
        pass
 
    print("接続しました")
    print(station.ifconfig())
connect()
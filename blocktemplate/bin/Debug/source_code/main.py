import network
import time

 station = network.WLAN(network.AP_IF)
station.active(True)
time.sleep(20)
station.active(False)

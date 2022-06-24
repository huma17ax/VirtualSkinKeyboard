import cv2
from cv2 import aruco
import time
import numpy as np

dict_aruco = aruco.Dictionary_get(aruco.DICT_4X4_50)
parameters = aruco.DetectorParameters_create()

def detect_marker(frame):
    gray = cv2.cvtColor(frame, cv2.COLOR_RGB2GRAY)

    corners, ids, rejectedImgPoints = aruco.detectMarkers(gray, dict_aruco, parameters=parameters)
    borad_corners = calc_board_corners(corners)
    frame_markers = aruco.drawDetectedMarkers(frame.copy(), corners, ids)
    if borad_corners is not None:
        frame_markers = cv2.polylines(frame_markers, [borad_corners], True, (0, 0, 255), 2)
    
    return frame_markers

def calc_board_corners(corners):
    if len(corners) == 0:
        return None

    c1 = (corners[0][0][0]+corners[0][0][3])/2
    c2 = (corners[0][0][1]+corners[0][0][2])/2
    d = 6*(c1 - c2)

    p2 = corners[0][0][0]*2 - c1
    p3 = corners[0][0][3]*2 - c1
    p1 = p2 + d
    p4 = p3 + d

    e = np.array([p1, p2, p3, p4]).astype('int32')

    return e


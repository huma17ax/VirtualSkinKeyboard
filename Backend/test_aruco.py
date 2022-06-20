import cv2
from cv2 import aruco
import time
import numpy as np

def generate_marker():
    dir_mark = '.\markers\\'
    num_mark = 20
    size_mark = 500

    dict_aruco = aruco.Dictionary_get(aruco.DICT_4X4_50)

    for i in range(num_mark):
        img = aruco.drawMarker(dict_aruco, i, size_mark)
        cv2.imwrite(dir_mark+'marker_'+str(i)+'.png', img)


def detect_marker():
    dict_aruco = aruco.Dictionary_get(aruco.DICT_4X4_50)
    parameters = aruco.DetectorParameters_create()

    capture = cv2.VideoCapture(2)

    while True:
        ret, frame = capture.read()
        frame = cv2.flip(frame, -1)
        gray = cv2.cvtColor(frame, cv2.COLOR_RGB2GRAY)

        corners, ids, rejectedImgPoints = aruco.detectMarkers(gray, dict_aruco, parameters=parameters)
        frame_markers = aruco.drawDetectedMarkers(frame.copy(), corners, ids)
        cv2.imshow('frame', frame_markers)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
    
    cv2.destroyAllWindows()
    capture.release()

detect_marker()
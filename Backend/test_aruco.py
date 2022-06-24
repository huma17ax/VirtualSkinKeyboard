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

def homography(corners):
    if len(corners) == 0:
        return None

    print('----------')
    print(corners)
    dst = np.array([[0,0], [1,0], [1,1], [0,1]])

    matrix, _ = cv2.findHomography(corners[0][0], dst, cv2.RANSAC,10.0)

    print(matrix)
    matrix = np.matrix(matrix)

    a = np.array(np.dot(matrix.I, np.array([[-6,0,1]]).T).T)
    b = np.array(np.dot(matrix.I, np.array([[0,0,1]]).T).T)
    c = np.array(np.dot(matrix.I, np.array([[0,1,1]]).T).T)
    d = np.array(np.dot(matrix.I, np.array([[-6,1,1]]).T).T)
    e = np.array([a[0][:2]/a[0][2], b[0][:2]/b[0][2], c[0][:2]/c[0][2], d[0][:2]/d[0][2]]).astype('int32')
    print(e[3][0])

    return e

def keys_base_width(corners):
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
    
def detect_marker():
    dict_aruco = aruco.Dictionary_get(aruco.DICT_4X4_50)
    parameters = aruco.DetectorParameters_create()

    capture = cv2.VideoCapture(2)

    while True:
        ret, frame = capture.read()
        frame = cv2.flip(frame, -1)
        gray = cv2.cvtColor(frame, cv2.COLOR_RGB2GRAY)

        corners, ids, rejectedImgPoints = aruco.detectMarkers(gray, dict_aruco, parameters=parameters)
        borad_corners = keys_base_width(corners)
        frame_markers = aruco.drawDetectedMarkers(frame.copy(), corners, ids)
        if borad_corners is not None:
            frame_markers = cv2.polylines(frame_markers, [borad_corners], True, (0, 0, 255), 2)
        cv2.imshow('frame', frame_markers)
        if cv2.waitKey(1) & 0xFF == ord('q'):
            break
    
    cv2.destroyAllWindows()
    capture.release()

detect_marker()